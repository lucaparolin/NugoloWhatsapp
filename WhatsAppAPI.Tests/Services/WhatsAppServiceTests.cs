using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WhatsAppAPI.Models;
using WhatsAppAPI.Services;
using Xunit;

namespace WhatsAppAPI.Tests.Services;

public class WhatsAppServiceTests
{
    private readonly Mock<ILogger<WhatsAppService>> _loggerMock;
    private readonly WhatsAppBusinessSettings _settings;

    public WhatsAppServiceTests()
    {
        _loggerMock = new Mock<ILogger<WhatsAppService>>();
        _settings = new WhatsAppBusinessSettings
        {
            AccessToken = "test_token_12345",
            PhoneNumberId = "123456789",
            ApiVersion = "v18.0",
            BaseUrl = "https://graph.facebook.com"
        };
    }

    [Fact]
    public void Constructor_WithEmptyAccessToken_ShouldThrowException()
    {
        // Arrange
        var invalidSettings = new WhatsAppBusinessSettings
        {
            AccessToken = "",
            PhoneNumberId = "123456789"
        };
        var options = Options.Create(invalidSettings);
        var httpClient = new HttpClient();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new WhatsAppService(options, _loggerMock.Object, httpClient));

        exception.Message.Should().Contain("AccessToken non configurato");
    }

    [Fact]
    public void Constructor_WithEmptyPhoneNumberId_ShouldThrowException()
    {
        // Arrange
        var invalidSettings = new WhatsAppBusinessSettings
        {
            AccessToken = "test_token",
            PhoneNumberId = ""
        };
        var options = Options.Create(invalidSettings);
        var httpClient = new HttpClient();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new WhatsAppService(options, _loggerMock.Object, httpClient));

        exception.Message.Should().Contain("PhoneNumberId non configurato");
    }

    [Theory]
    [InlineData("+393331234567", "393331234567")] // Standard international format
    [InlineData("00393331234567", "393331234567")] // 00 prefix format
    [InlineData("393331234567", "393331234567")]   // Already normalized
    [InlineData("+1 (555) 123-4567", "15551234567")] // US format with spaces/parentheses
    public async Task SendMessageAsync_PhoneNumberNormalization_ShouldNormalizeCorrectly(
        string inputPhone, string expectedNormalized)
    {
        // Arrange
        var options = Options.Create(_settings);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        // Capture the actual request
        HttpRequestMessage? capturedRequest = null;

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                capturedRequest = req;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    messaging_product = "whatsapp",
                    messages = new[] { new { id = "test_msg_id" } }
                }))
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var service = new WhatsAppService(options, _loggerMock.Object, httpClient);

        var request = new WhatsAppMessageRequest
        {
            To = inputPhone,
            Message = "Test message",
            MediaType = MediaType.Text
        };

        // Act
        await service.SendMessageAsync(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        var requestBody = await capturedRequest!.Content!.ReadAsStringAsync();
        requestBody.Should().Contain($"\"to\":\"{expectedNormalized}\"");
    }

    [Fact]
    public async Task SendMessageAsync_AudioMessage_ShouldNotIncludeCaption()
    {
        // Arrange
        var options = Options.Create(_settings);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        HttpRequestMessage? capturedRequest = null;

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                capturedRequest = req;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    messaging_product = "whatsapp",
                    messages = new[] { new { id = "test_msg_id" } }
                }))
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var service = new WhatsAppService(options, _loggerMock.Object, httpClient);

        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            Message = "This should be ignored",
            MediaUrl = "https://example.com/audio.mp3",
            MediaType = MediaType.Audio
        };

        // Act
        await service.SendMessageAsync(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        var requestBody = await capturedRequest!.Content!.ReadAsStringAsync();

        // Should not contain caption field for audio
        requestBody.Should().Contain("\"type\":\"audio\"");
        requestBody.Should().NotContain("\"caption\":");
    }

    [Fact]
    public async Task SendMessageAsync_ConcurrentRequests_ShouldNotCorruptHeaders()
    {
        // Arrange
        var options = Options.Create(_settings);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var capturedHeaders = new System.Collections.Concurrent.ConcurrentBag<string>();

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
            {
                // Capture authorization header
                var authHeader = req.Headers.Authorization?.Parameter ?? "missing";
                capturedHeaders.Add(authHeader);
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    messaging_product = "whatsapp",
                    messages = new[] { new { id = "test_msg_id" } }
                }))
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var service = new WhatsAppService(options, _loggerMock.Object, httpClient);

        var requests = Enumerable.Range(0, 10).Select(i => new WhatsAppMessageRequest
        {
            To = $"+3933312345{i:00}",
            Message = $"Test message {i}",
            MediaType = MediaType.Text
        }).ToList();

        // Act - Send requests concurrently
        var tasks = requests.Select(req => service.SendMessageAsync(req));
        await Task.WhenAll(tasks);

        // Assert - All requests should have the same correct token
        capturedHeaders.Should().HaveCount(10);
        capturedHeaders.Should().OnlyContain(h => h == "test_token_12345");
    }

    [Fact]
    public async Task SendMessageAsync_SuccessResponse_ShouldReturnSuccessResult()
    {
        // Arrange
        var options = Options.Create(_settings);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    messaging_product = "whatsapp",
                    contacts = new[] { new { input = "393331234567", wa_id = "393331234567" } },
                    messages = new[] { new { id = "wamid.test123", message_status = "sent" } }
                }))
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var service = new WhatsAppService(options, _loggerMock.Object, httpClient);

        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            Message = "Test message",
            MediaType = MediaType.Text
        };

        // Act
        var result = await service.SendMessageAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.MessageId.Should().Be("wamid.test123");
        result.Status.Should().Be("sent");
    }

    [Fact]
    public async Task SendMessageAsync_ErrorResponse_ShouldReturnFailureResult()
    {
        // Arrange
        var options = Options.Create(_settings);
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    error = new
                    {
                        message = "Invalid phone number",
                        type = "OAuthException",
                        code = 100
                    }
                }))
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var service = new WhatsAppService(options, _loggerMock.Object, httpClient);

        var request = new WhatsAppMessageRequest
        {
            To = "invalid",
            Message = "Test message",
            MediaType = MediaType.Text
        };

        // Act
        var result = await service.SendMessageAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid phone number");
    }

    [Theory]
    [InlineData("123")]        // Too short
    [InlineData("abc")]        // Non-numeric
    [InlineData("")]           // Empty
    public void SendMessageAsync_InvalidPhoneNumber_ShouldThrowException(string invalidPhone)
    {
        // Arrange
        var options = Options.Create(_settings);
        var httpClient = new HttpClient();
        var service = new WhatsAppService(options, _loggerMock.Object, httpClient);

        var request = new WhatsAppMessageRequest
        {
            To = invalidPhone,
            Message = "Test",
            MediaType = MediaType.Text
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.SendMessageAsync(request));
    }
}
