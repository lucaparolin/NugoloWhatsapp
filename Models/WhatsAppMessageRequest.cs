using System.ComponentModel.DataAnnotations;

namespace WhatsAppAPI.Models;

/// <summary>
/// Rappresenta una richiesta per inviare un messaggio WhatsApp
/// </summary>
public class WhatsAppMessageRequest
{
    /// <summary>
    /// Numero di telefono del destinatario in formato internazionale (es: +393331234567)
    /// </summary>
    [Required(ErrorMessage = "Il numero del destinatario è obbligatorio")]
    [Phone(ErrorMessage = "Formato numero di telefono non valido")]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Contenuto del messaggio da inviare
    /// </summary>
    [Required(ErrorMessage = "Il messaggio è obbligatorio")]
    [StringLength(1600, ErrorMessage = "Il messaggio non può superare i 1600 caratteri")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// URL dell'immagine da inviare (opzionale)
    /// </summary>
    [Url(ErrorMessage = "URL immagine non valido")]
    public string? MediaUrl { get; set; }
}
