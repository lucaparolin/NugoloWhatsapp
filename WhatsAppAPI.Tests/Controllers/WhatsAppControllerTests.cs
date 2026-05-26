using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WhatsAppAPI.Controllers;
using WhatsAppAPI.Models;
using WhatsAppAPI.Services;
using Xunit;

namespace WhatsAppAPI.Tests.Controllers;

public class WhatsAppControllerTests
{
    private readonly Mock<IWhatsAppService> _serviceMock;
    private readonly Mock<ILogger<WhatsAppController>> _loggerMock;
    private readonly WhatsAppController _controller;

    public WhatsAppControllerTests()
    {
        _serviceMock = new Mock<IWhatsAppService>();
        _loggerMock = new Mock<ILogger<WhatsAppController>>();
        _controller = new WhatsAppController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendMessage_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            Message = "Test message",
            MediaType = MediaType.Text
        };

        var expectedResponse = new WhatsAppMessageResponse
        {
            Success = true,
            MessageId = "test_id",
            Status = "sent",
            To = request.To,
            SentAt = DateTime.UtcNow
        };

        _serviceMock
            .Setup(s => s.SendMessageAsync(It.IsAny<WhatsAppMessageRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<WhatsAppMessageResponse>().Subject;
        response.Success.Should().BeTrue();
        response.MessageId.Should().Be("test_id");
    }

    [Fact]
    public async Task SendSimpleMessage_WithWhitespace_ShouldTrimParameters()
    {
        // Arrange
        var to = "  +393331234567  ";
        var message = "  Test message  ";

        _serviceMock
            .Setup(s => s.SendMessageAsync(It.Is<WhatsAppMessageRequest>(
                r => r.To == "+393331234567" && r.Message == "Test message")))
            .ReturnsAsync(new WhatsAppMessageResponse { Success = true });

        // Act
        var result = await _controller.SendSimpleMessage(to, message);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.SendMessageAsync(
            It.Is<WhatsAppMessageRequest>(r =>
                r.To == "+393331234567" &&
                r.Message == "Test message")), Times.Once);
    }

    [Theory]
    [InlineData("", "message")]
    [InlineData("   ", "message")]
    [InlineData("+123", "")]
    [InlineData("+123", "   ")]
    public async Task SendSimpleMessage_WithEmptyParameters_ReturnsBadRequest(
        string to, string message)
    {
        // Act
        var result = await _controller.SendSimpleMessage(to, message);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SendImage_WithWhitespace_ShouldTrimParameters()
    {
        // Arrange
        var to = "  +393331234567  ";
        var imageUrl = "  https://example.com/image.jpg  ";
        var caption = "  Test caption  ";

        _serviceMock
            .Setup(s => s.SendMessageAsync(It.Is<WhatsAppMessageRequest>(
                r => r.To == "+393331234567" &&
                     r.MediaUrl == "https://example.com/image.jpg" &&
                     r.Message == "Test caption" &&
                     r.MediaType == MediaType.Image)))
            .ReturnsAsync(new WhatsAppMessageResponse { Success = true });

        // Act
        var result = await _controller.SendImage(to, imageUrl, caption);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SendVideo_ValidParameters_ShouldCallServiceWithCorrectMediaType()
    {
        // Arrange
        var to = "+393331234567";
        var videoUrl = "https://example.com/video.mp4";
        var caption = "Test video";

        _serviceMock
            .Setup(s => s.SendMessageAsync(It.Is<WhatsAppMessageRequest>(
                r => r.MediaType == MediaType.Video)))
            .ReturnsAsync(new WhatsAppMessageResponse { Success = true });

        // Act
        var result = await _controller.SendVideo(to, videoUrl, caption);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.SendMessageAsync(
            It.Is<WhatsAppMessageRequest>(r => r.MediaType == MediaType.Video)), Times.Once);
    }

    [Fact]
    public async Task SendAudio_ValidParameters_ShouldCallServiceWithCorrectMediaType()
    {
        // Arrange
        var to = "+393331234567";
        var audioUrl = "https://example.com/audio.mp3";

        _serviceMock
            .Setup(s => s.SendMessageAsync(It.Is<WhatsAppMessageRequest>(
                r => r.MediaType == MediaType.Audio)))
            .ReturnsAsync(new WhatsAppMessageResponse { Success = true });

        // Act
        var result = await _controller.SendAudio(to, audioUrl);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.SendMessageAsync(
            It.Is<WhatsAppMessageRequest>(r => r.MediaType == MediaType.Audio)), Times.Once);
    }

    [Fact]
    public async Task SendDocument_WithAllParameters_ShouldTrimAndPassCorrectly()
    {
        // Arrange
        var to = "  +393331234567  ";
        var documentUrl = "  https://example.com/doc.pdf  ";
        var fileName = "  Report.pdf  ";
        var caption = "  Monthly report  ";

        _serviceMock
            .Setup(s => s.SendMessageAsync(It.Is<WhatsAppMessageRequest>(
                r => r.To == "+393331234567" &&
                     r.MediaUrl == "https://example.com/doc.pdf" &&
                     r.FileName == "Report.pdf" &&
                     r.Message == "Monthly report" &&
                     r.MediaType == MediaType.Document)))
            .ReturnsAsync(new WhatsAppMessageResponse { Success = true });

        // Act
        var result = await _controller.SendDocument(to, documentUrl, fileName, caption);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task HealthCheck_WhenConnectionSuccessful_ReturnsHealthyStatus()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.VerifyConnectionAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        ((string)value!.status).Should().Be("healthy");
    }

    [Fact]
    public async Task HealthCheck_WhenConnectionFails_ReturnsUnhealthyStatus()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.VerifyConnectionAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(503);
    }

    [Fact]
    public void GetInfo_ShouldReturnApiInformation()
    {
        // Act
        var result = _controller.GetInfo();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value as dynamic;
        ((string)value!.name).Should().Be("WhatsApp API");
        ((string)value.version).Should().Be("2.0.0");
    }

    [Fact]
    public async Task SendMessage_WhenServiceFails_Returns500()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            Message = "Test",
            MediaType = MediaType.Text
        };

        _serviceMock
            .Setup(s => s.SendMessageAsync(It.IsAny<WhatsAppMessageRequest>()))
            .ReturnsAsync(new WhatsAppMessageResponse
            {
                Success = false,
                ErrorMessage = "API Error"
            });

        // Act
        var result = await _controller.SendMessage(request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
