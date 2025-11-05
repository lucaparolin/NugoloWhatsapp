namespace WhatsAppAPI.Models;

/// <summary>
/// Configurazione per l'integrazione con Meta WhatsApp Business API
/// </summary>
public class WhatsAppBusinessSettings
{
    /// <summary>
    /// Access Token dell'app Meta/Facebook
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Phone Number ID dal WhatsApp Business Account
    /// </summary>
    public string PhoneNumberId { get; set; } = string.Empty;

    /// <summary>
    /// Versione dell'API Graph di Meta (default: v18.0)
    /// </summary>
    public string ApiVersion { get; set; } = "v18.0";

    /// <summary>
    /// URL base dell'API Graph di Meta
    /// </summary>
    public string BaseUrl { get; set; } = "https://graph.facebook.com";
}
