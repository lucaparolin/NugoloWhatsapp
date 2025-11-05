namespace WhatsAppAPI.Models;

/// <summary>
/// Configurazione per l'integrazione con Twilio WhatsApp API
/// </summary>
public class TwilioSettings
{
    /// <summary>
    /// Account SID di Twilio
    /// </summary>
    public string AccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Auth Token di Twilio
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Numero WhatsApp mittente (fornito da Twilio, formato: whatsapp:+14155238886)
    /// </summary>
    public string WhatsAppNumber { get; set; } = string.Empty;
}
