using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using WhatsAppAPI.Models;
using Xunit;

namespace WhatsAppAPI.Tests.Models;

public class WhatsAppMessageRequestTests
{
    private static List<ValidationResult> ValidateModel(WhatsAppMessageRequest model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }

    [Fact]
    public void Validate_TextMessageWithoutMessage_ShouldFail()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            MediaType = MediaType.Text,
            Message = null
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle(r => r.ErrorMessage!.Contains("Message è obbligatorio"));
    }

    [Fact]
    public void Validate_TextMessageWithMessage_ShouldPass()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            MediaType = MediaType.Text,
            Message = "Hello World"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ImageWithoutMediaUrl_ShouldFail()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            MediaType = MediaType.Image,
            MediaUrl = null,
            Message = "Caption"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle(r => r.ErrorMessage!.Contains("MediaUrl è obbligatorio"));
    }

    [Fact]
    public void Validate_ImageWithMediaUrl_ShouldPass()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            MediaType = MediaType.Image,
            MediaUrl = "https://example.com/image.jpg",
            Message = "Caption"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_InvalidMediaTypeEnum_ShouldFail()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            MediaType = (MediaType)99, // Invalid enum value
            Message = "Test"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle(r => r.ErrorMessage!.Contains("MediaType non valido"));
    }

    [Fact]
    public void Validate_EmptyPhoneNumber_ShouldFail()
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "",
            MediaType = MediaType.Text,
            Message = "Test"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().Contain(r => r.ErrorMessage!.Contains("numero del destinatario è obbligatorio"));
    }

    [Fact]
    public void Validate_MessageTooLong_ShouldFail()
    {
        // Arrange
        var longMessage = new string('a', 1601);
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            MediaType = MediaType.Text,
            Message = longMessage
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle(r => r.ErrorMessage!.Contains("1600 caratteri"));
    }

    [Theory]
    [InlineData(MediaType.Video)]
    [InlineData(MediaType.Audio)]
    [InlineData(MediaType.Document)]
    public void Validate_MediaTypesWithoutMediaUrl_ShouldFail(MediaType mediaType)
    {
        // Arrange
        var request = new WhatsAppMessageRequest
        {
            To = "+393331234567",
            MediaType = mediaType,
            MediaUrl = null
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle(r => r.ErrorMessage!.Contains("MediaUrl è obbligatorio"));
    }
}
