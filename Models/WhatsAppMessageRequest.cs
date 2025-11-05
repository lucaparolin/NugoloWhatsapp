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
    /// Contenuto del messaggio da inviare o caption per media
    /// </summary>
    [StringLength(1600, ErrorMessage = "Il messaggio non può superare i 1600 caratteri")]
    public string? Message { get; set; }

    /// <summary>
    /// URL del media da inviare (immagine, video, audio, documento)
    /// </summary>
    [Url(ErrorMessage = "URL media non valido")]
    public string? MediaUrl { get; set; }

    /// <summary>
    /// Tipo di media da inviare (default: Text)
    /// </summary>
    public MediaType MediaType { get; set; } = MediaType.Text;

    /// <summary>
    /// Nome del file per i documenti (opzionale)
    /// </summary>
    [StringLength(255, ErrorMessage = "Il nome del file non può superare i 255 caratteri")]
    public string? FileName { get; set; }
}
