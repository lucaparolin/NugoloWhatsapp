namespace WhatsAppAPI.Models;

/// <summary>
/// Rappresenta la risposta dopo l'invio di un messaggio WhatsApp
/// </summary>
public class WhatsAppMessageResponse
{
    /// <summary>
    /// Indica se il messaggio è stato inviato con successo
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// ID del messaggio assegnato da Twilio
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Stato del messaggio
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Messaggio di errore in caso di fallimento
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Numero del destinatario
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// Data e ora di invio
    /// </summary>
    public DateTime SentAt { get; set; }
}
