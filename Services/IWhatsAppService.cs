using WhatsAppAPI.Models;

namespace WhatsAppAPI.Services;

/// <summary>
/// Interfaccia per il servizio di invio messaggi WhatsApp
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Invia un messaggio WhatsApp a un destinatario
    /// </summary>
    /// <param name="request">Dati del messaggio da inviare</param>
    /// <returns>Risposta con l'esito dell'invio</returns>
    Task<WhatsAppMessageResponse> SendMessageAsync(WhatsAppMessageRequest request);

    /// <summary>
    /// Verifica la connessione con Twilio
    /// </summary>
    /// <returns>True se la connessione è valida</returns>
    Task<bool> VerifyConnectionAsync();
}
