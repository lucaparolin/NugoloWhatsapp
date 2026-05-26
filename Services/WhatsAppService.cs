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

        // Validazione configurazione al startup (fail-fast)
        if (string.IsNullOrWhiteSpace(_settings.AccessToken))
            throw new InvalidOperationException("AccessToken non configurato in WhatsAppBusiness settings");

        if (string.IsNullOrWhiteSpace(_settings.PhoneNumberId))
            throw new InvalidOperationException("PhoneNumberId non configurato in WhatsAppBusiness settings");

        // Costruisce l'URL dell'API: https://graph.facebook.com/v18.0/{phone-number-id}/messages
        _apiUrl = $"{_settings.BaseUrl}/{_settings.ApiVersion}/{_settings.PhoneNumberId}/messages";

        // Configura timeout per HttpClient
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
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

            // Crea HttpRequestMessage con headers per richiesta (thread-safe)
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Invia la richiesta HTTP POST
            var response = await _httpClient.SendAsync(requestMessage);

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

            // Crea HttpRequestMessage con headers per richiesta (thread-safe)
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, verifyUrl);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);

            var response = await _httpClient.SendAsync(requestMessage);
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
        // Normalizza il numero di telefono in formato E.164 (solo cifre, no +)
        var cleanedNumber = NormalizePhoneNumber(request.To);

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

                // Nota: Meta WhatsApp API non supporta caption per audio
                // Se Message è presente, viene ignorato (non viene inviato)
                metaRequest.Type = "audio";
                metaRequest.Audio = new MetaMediaMessage
                {
                    Link = request.MediaUrl,
                    Caption = null // Audio non supporta caption
                };

                if (!string.IsNullOrEmpty(request.Message))
                {
                    _logger.LogWarning("Caption ignorato per audio message. Meta API non supporta caption per audio. MediaUrl: {MediaUrl}", request.MediaUrl);
                }
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

    /// <summary>
    /// Normalizza il numero di telefono in formato E.164 per Meta API (solo cifre, no +)
    /// </summary>
    /// <param name="phoneNumber">Numero in formato internazionale (+39..., 0039..., etc.)</param>
    /// <returns>Numero normalizzato (es: 393331234567)</returns>
    private string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Numero di telefono vuoto");

        // Rimuove spazi, trattini, parentesi, punti
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // Gestisce prefisso internazionale 00
        if (cleaned.StartsWith("00"))
        {
            cleaned = cleaned.Substring(2); // Rimuove 00, rimane solo country code + numero
        }
        // Gestisce prefisso +
        else if (cleaned.StartsWith("+"))
        {
            cleaned = cleaned.Substring(1); // Rimuove +, rimane solo country code + numero
        }

        // Valida che il numero risultante sia valido (almeno 10 cifre)
        if (cleaned.Length < 10)
            throw new ArgumentException($"Numero di telefono troppo corto dopo normalizzazione: {cleaned}");

        return cleaned;
    }
}
