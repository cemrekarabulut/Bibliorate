using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BiblioRate.Infrastructure.Context;
using System.Net;
using BiblioRate.API.Controllers;

namespace BiblioRate.Tests.Integration;

public class BasicIntegrationTests : IClassFixture<WebApplicationFactory<BooksController>>
{
    private readonly WebApplicationFactory<BooksController> _factory;

    public BasicIntegrationTests(WebApplicationFactory<BooksController> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Veritabanını InMemory ile değiştiriyoruz ki gerçek MySQL gereksinimi olmasın
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
    }

    [Fact]
    public async Task App_ShouldStartup_AndRespond()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Swagger endpoint'ini kontrol ederek uygulamanın başarıyla ayağa kalktığını doğruluyoruz.
        var response = await client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
