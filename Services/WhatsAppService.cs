using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WhatsAppAPI.Models;

namespace WhatsAppAPI.Services;

/// <summary>
/// Implementazione del servizio per l'invio di messaggi WhatsApp tramite Meta WhatsApp Business API
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly WhatsAppBusinessSettings _settings;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;

    public WhatsAppService(
        IOptions<WhatsAppBusinessSettings> settings,
        ILogger<WhatsAppService> logger,
        HttpClient httpClient)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;

        // Costruisce l'URL dell'API: https://graph.facebook.com/v18.0/{phone-number-id}/messages
        _apiUrl = $"{_settings.BaseUrl}/{_settings.ApiVersion}/{_settings.PhoneNumberId}/messages";

        // Configura l'HttpClient
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Invia un messaggio WhatsApp tramite Meta WhatsApp Business API
    /// </summary>
    public async Task<WhatsAppMessageResponse> SendMessageAsync(WhatsAppMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Invio messaggio WhatsApp a {To} tramite Meta API", request.To);

            // Prepara la richiesta per Meta API
            var metaRequest = BuildMetaRequest(request);

            // Serializza la richiesta
            var jsonContent = JsonSerializer.Serialize(metaRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            _logger.LogDebug("Richiesta Meta API: {JsonContent}", jsonContent);

            // Invia la richiesta HTTP POST
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_apiUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Risposta Meta API: {ResponseContent}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                var metaResponse = JsonSerializer.Deserialize<MetaMessageResponse>(responseContent);

                var messageId = metaResponse?.Messages?.FirstOrDefault()?.Id ?? "unknown";
                var status = metaResponse?.Messages?.FirstOrDefault()?.MessageStatus ?? "sent";

                _logger.LogInformation(
                    "Messaggio inviato con successo. ID: {MessageId}, Stato: {Status}",
                    messageId, status);

                return new WhatsAppMessageResponse
                {
                    Success = true,
                    MessageId = messageId,
                    Status = status,
                    To = request.To,
                    SentAt = DateTime.UtcNow
                };
            }
            else
            {
                // Gestisce gli errori dalle API Meta
                var errorResponse = JsonSerializer.Deserialize<MetaErrorResponse>(responseContent);
                var errorMessage = errorResponse?.Error?.Message ?? response.ReasonPhrase ?? "Errore sconosciuto";
                var errorCode = errorResponse?.Error?.Code ?? (int)response.StatusCode;

                _logger.LogError(
                    "Errore durante l'invio del messaggio. Status: {StatusCode}, Error: {ErrorMessage}, Code: {ErrorCode}",
                    response.StatusCode, errorMessage, errorCode);

                return new WhatsAppMessageResponse
                {
                    Success = false,
                    ErrorMessage = $"[{errorCode}] {errorMessage}",
                    To = request.To,
                    SentAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eccezione durante l'invio del messaggio WhatsApp a {To}", request.To);

            return new WhatsAppMessageResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                To = request.To,
                SentAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Verifica la connessione con Meta WhatsApp Business API
    /// </summary>
    public async Task<bool> VerifyConnectionAsync()
    {
        try
        {
            // Verifica chiamando l'endpoint del phone number
            var verifyUrl = $"{_settings.BaseUrl}/{_settings.ApiVersion}/{_settings.PhoneNumberId}";

            var response = await _httpClient.GetAsync(verifyUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("Verifica connessione Meta API: {StatusCode}, {Response}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Connessione Meta WhatsApp Business API verificata con successo");
                return true;
            }
            else
            {
                _logger.LogWarning("Verifica connessione fallita. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la verifica della connessione Meta API");
            return false;
        }
    }

    /// <summary>
    /// Costruisce la richiesta nel formato richiesto dalle API Meta
    /// </summary>
    private MetaMessageRequest BuildMetaRequest(WhatsAppMessageRequest request)
    {
        // Pulisce il numero di telefono (rimuove il + se presente)
        var cleanedNumber = request.To.TrimStart('+');

        var metaRequest = new MetaMessageRequest
        {
            To = cleanedNumber
        };

        // Determina il tipo di messaggio in base al MediaType
        switch (request.MediaType)
        {
            case MediaType.Image:
                if (string.IsNullOrEmpty(request.MediaUrl))
                    throw new ArgumentException("MediaUrl è obbligatorio per i messaggi di tipo Image");

                metaRequest.Type = "image";
                metaRequest.Image = new MetaMediaMessage
                {
                    Link = request.MediaUrl,
                    Caption = request.Message
                };
                break;

            case MediaType.Video:
                if (string.IsNullOrEmpty(request.MediaUrl))
                    throw new ArgumentException("MediaUrl è obbligatorio per i messaggi di tipo Video");

                metaRequest.Type = "video";
                metaRequest.Video = new MetaMediaMessage
                {
                    Link = request.MediaUrl,
                    Caption = request.Message
                };
                break;

            case MediaType.Audio:
                if (string.IsNullOrEmpty(request.MediaUrl))
                    throw new ArgumentException("MediaUrl è obbligatorio per i messaggi di tipo Audio");

                metaRequest.Type = "audio";
                metaRequest.Audio = new MetaMediaMessage
                {
                    Link = request.MediaUrl,
                    Caption = request.Message
                };
                break;

            case MediaType.Document:
                if (string.IsNullOrEmpty(request.MediaUrl))
                    throw new ArgumentException("MediaUrl è obbligatorio per i messaggi di tipo Document");

                metaRequest.Type = "document";
                metaRequest.Document = new MetaDocumentMessage
                {
                    Link = request.MediaUrl,
                    Caption = request.Message,
                    Filename = request.FileName
                };
                break;

            case MediaType.Text:
            default:
                // Messaggio di solo testo
                if (string.IsNullOrEmpty(request.Message))
                    throw new ArgumentException("Message è obbligatorio per i messaggi di tipo Text");

                metaRequest.Type = "text";
                metaRequest.Text = new MetaTextMessage
                {
                    Body = request.Message,
                    PreviewUrl = false
                };
                break;
        }

        return metaRequest;
    }
}
