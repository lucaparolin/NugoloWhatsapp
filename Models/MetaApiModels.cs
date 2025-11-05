using System.Text.Json.Serialization;

namespace WhatsAppAPI.Models;

/// <summary>
/// Modelli per le richieste e risposte delle API Meta WhatsApp Business
/// </summary>

/// <summary>
/// Richiesta per l'invio di un messaggio alle API Meta
/// </summary>
public class MetaMessageRequest
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = "whatsapp";

    [JsonPropertyName("recipient_type")]
    public string RecipientType { get; set; } = "individual";

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "text";

    [JsonPropertyName("text")]
    public MetaTextMessage? Text { get; set; }

    [JsonPropertyName("image")]
    public MetaMediaMessage? Image { get; set; }
}

/// <summary>
/// Messaggio di testo per Meta API
/// </summary>
public class MetaTextMessage
{
    [JsonPropertyName("preview_url")]
    public bool PreviewUrl { get; set; } = false;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Messaggio media (immagine) per Meta API
/// </summary>
public class MetaMediaMessage
{
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
}

/// <summary>
/// Risposta dalle API Meta dopo l'invio di un messaggio
/// </summary>
public class MetaMessageResponse
{
    [JsonPropertyName("messaging_product")]
    public string MessagingProduct { get; set; } = string.Empty;

    [JsonPropertyName("contacts")]
    public List<MetaContact>? Contacts { get; set; }

    [JsonPropertyName("messages")]
    public List<MetaMessage>? Messages { get; set; }
}

/// <summary>
/// Contatto nella risposta Meta
/// </summary>
public class MetaContact
{
    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("wa_id")]
    public string WaId { get; set; } = string.Empty;
}

/// <summary>
/// Messaggio nella risposta Meta
/// </summary>
public class MetaMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("message_status")]
    public string? MessageStatus { get; set; }
}

/// <summary>
/// Errore dalle API Meta
/// </summary>
public class MetaErrorResponse
{
    [JsonPropertyName("error")]
    public MetaError? Error { get; set; }
}

/// <summary>
/// Dettaglio errore Meta
/// </summary>
public class MetaError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("error_data")]
    public object? ErrorData { get; set; }

    [JsonPropertyName("fbtrace_id")]
    public string? FbTraceId { get; set; }
}
