using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using BiblioRate.API.Middleware;
using System.Net;
using FluentAssertions;

namespace BiblioRate.Tests.Middleware;

public class MiddlewareTests
{
    [Fact]
    public async Task ExceptionMiddleware_ShouldHandleException_AndReturnJson()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var mockNext = new Mock<RequestDelegate>();
        mockNext.Setup(n => n(It.IsAny<HttpContext>())).Throws(new Exception("Test error"));
        
        var mockLogger = new Mock<ILogger<ExceptionMiddleware>>();
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        
        var middleware = new ExceptionMiddleware(mockNext.Object, mockLogger.Object, mockEnv.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        
        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);
        json.GetProperty("Message").GetString().Should().Be("Sunucu tarafında bir hata oluştu.");
        json.GetProperty("Detail").GetString().Should().Be("Test error");
    }
}
