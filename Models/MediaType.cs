namespace WhatsAppAPI.Models;

/// <summary>
/// Tipi di media supportati dall'API WhatsApp Business
/// </summary>
public enum MediaType
{
    /// <summary>
    /// Messaggio di solo testo (default)
    /// </summary>
    Text,

    /// <summary>
    /// Immagine (PNG, JPEG)
    /// </summary>
    Image,

    /// <summary>
    /// Video (MP4, 3GP)
    /// </summary>
    Video,

    /// <summary>
    /// Audio (AAC, MP3, AMR, OGG)
    /// </summary>
    Audio,

    /// <summary>
    /// Documento (PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX)
    /// </summary>
    Document
}
