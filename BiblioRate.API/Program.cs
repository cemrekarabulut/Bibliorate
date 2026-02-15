using BiblioRate.Application.Interfaces;
using BiblioRate.Infrastructure.Context;
using BiblioRate.Infrastructure.Repositories;
using BiblioRate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı ve Repository Ayarları
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Repository Kayıtları (Dependency Injection)
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// Yeni eklediğimiz Favorite Repository kaydı
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();

builder.Services.AddHttpClient<IGoogleBooksService, GoogleBooksService>();

// 2. API ve Swagger Ayarları
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Geliştirme ortamında Swagger'ı aç
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();