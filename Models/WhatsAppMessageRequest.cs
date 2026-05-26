using System.ComponentModel.DataAnnotations;

namespace WhatsAppAPI.Models;

/// <summary>
/// Rappresenta una richiesta per inviare un messaggio WhatsApp
/// </summary>
public class WhatsAppMessageRequest : IValidatableObject
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

    /// <summary>
    /// Validazione custom condizionale in base al MediaType
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Valida che MediaType sia un valore valido dell'enum
        if (!Enum.IsDefined(typeof(MediaType), MediaType))
        {
            yield return new ValidationResult(
                $"MediaType non valido: {(int)MediaType}. Valori permessi: 0-4 (Text, Image, Video, Audio, Document)",
                new[] { nameof(MediaType) });
        }

        // Per messaggi di testo, Message è obbligatorio
        if (MediaType == MediaType.Text && string.IsNullOrWhiteSpace(Message))
        {
            yield return new ValidationResult(
                "Il campo Message è obbligatorio per messaggi di tipo Text",
                new[] { nameof(Message) });
        }

        // Per media (non Text), MediaUrl è obbligatorio
        if (MediaType != MediaType.Text && string.IsNullOrWhiteSpace(MediaUrl))
        {
            yield return new ValidationResult(
                $"Il campo MediaUrl è obbligatorio per messaggi di tipo {MediaType}",
                new[] { nameof(MediaUrl) });
        }
    }
}
