using BiblioRate.Application.Interfaces;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Repositories;
using BiblioRate.Infrastructure.Services;
using BiblioRate.API.Middleware;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── 0. .env Yükleme ────────────────────────────────────────────────────────
// Proje kökündeki .env dosyasından ortam değişkenlerini yükler.
// Dosya yoksa sessizce devam eder (prod ortamında gerçek env var kullanılır).
Env.TraversePath().Load();

// ─── 1. Veritabanı ──────────────────────────────────────────────────────────
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Veritabanı bağlantı dizesi bulunamadı.");

// .env'den DB_PASSWORD değerini al; yoksa appsettings.json'daki değeri koru
var dbPassword       = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;
var connectionString = string.IsNullOrWhiteSpace(dbPassword)
    ? rawConnectionString
    : rawConnectionString.Replace("{DB_PASSWORD}", dbPassword);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ─── 2. Repository Kayıtları ────────────────────────────────────────────────
builder.Services.AddScoped<IBookRepository,      BookRepository>();
builder.Services.AddScoped<IRatingRepository,    RatingRepository>();
builder.Services.AddScoped<IReviewRepository,    ReviewRepository>();
builder.Services.AddScoped<IFavoriteRepository,  FavoriteRepository>();
builder.Services.AddScoped<IBookViewRepository,  BookViewRepository>();
builder.Services.AddScoped<IUserRepository,      UserRepository>();
builder.Services.AddScoped<ISearchLogRepository, SearchLogRepository>();

// ─── 3. Servis Kayıtları ────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

// ─── 4. HTTP İstemcileri ────────────────────────────────────────────────────
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();
builder.Services.AddScoped<DataSeederService>();

builder.Services.AddHttpClient("FlaskApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["FlaskApi:BaseUrl"] ?? "http://localhost:5000/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ─── 4. JWT Authentication ──────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key yapılandırılmamış.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ─── 5. CORS (React ve diğer SPA origin'leri) ───────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                      ?? Array.Empty<string>();

        if (origins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// ─── 6. API & Swagger (JWT destekli) ────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Null property'leri JSON'a yazma (temiz çıktı)
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        // camelCase — frontend uyumu
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        // Circular reference (EF navigation) guard
        opts.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BiblioRate API", Version = "v1" });

    // Swagger UI'dan JWT ile test edebilmek için Bearer token desteği
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "JWT token'ınızı girin. Örnek: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Uygulama Pipeline ──────────────────────────────────────────────────────
var app = builder.Build();

// Global exception handler — HER ŞEYDEN önce
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// DOĞRU MIDDLEWARE SIRASI:
// 1. HTTPS yönlendirme
// 2. CORS
// 3. Authentication (kim olduğunu belirle)
// 4. Authorization (ne yapabileceğini belirle)
// 5. Controller mapping
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    await dataSeeder.SeedAsync();

    // NOT: Veritabanı temizleme (deduplication + kategori normalizasyonu) artık
    // startup'ta otomatik çalışmaz. Tek seferlik çalıştırmak için:
    // POST /api/admin/cleanup  (AdminController)
}

app.MapControllers();

app.Run();
