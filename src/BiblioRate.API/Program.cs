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
Env.TraversePath().Load();

// ─── 1. Veritabanı ──────────────────────────────────────────────────────────
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Veritabanı bağlantı dizesi bulunamadı.");

var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;
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

// ─── 3. Servis Kayıtları (Analiz & Kalite Katmanı) ───────────────────────────
builder.Services.AddScoped<IBookSimilarityScorer, BookSimilarityScorer>();
builder.Services.AddScoped<IBookQualityEvaluator, BookQualityEvaluator>(); // Rütbe Sistemi Eklendi
builder.Services.AddScoped<IAuthService,           AuthService>();

// ─── 4. HTTP İstemcileri ────────────────────────────────────────────────────
builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();
builder.Services.AddScoped<DataSeederService>();

builder.Services.AddHttpClient("FlaskApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["FlaskApi:BaseUrl"] ?? "http://localhost:5000/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ─── 5. JWT Authentication ──────────────────────────────────────────────────
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

// ─── 6. CORS ───────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// ─── 7. API & Swagger ───────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BiblioRate API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "JWT token'ınızı girin."
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

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ─── Veritabanı Otomasyonu & Seeding ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    // await dataSeeder.SeedAsync(); // Manual Trigger: Otomatik çalışma devre dışı bırakıldı. Sadece Admin Cleanup veya manuel tetikleme kullanılacak.
}

app.MapControllers();
app.Run();