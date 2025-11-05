using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WhatsAppAPI.Models;

namespace WhatsAppAPI.Services;

/// <summary>
/// Implementazione del servizio per l'invio di messaggi WhatsApp tramite Twilio
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly TwilioSettings _twilioSettings;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(IOptions<TwilioSettings> twilioSettings, ILogger<WhatsAppService> logger)
    {
        _twilioSettings = twilioSettings.Value;
        _logger = logger;

        // Inizializza il client Twilio
        TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
    }

    /// <summary>
    /// Invia un messaggio WhatsApp tramite Twilio API
    /// </summary>
    public async Task<WhatsAppMessageResponse> SendMessageAsync(WhatsAppMessageRequest request)
    {
        try
        {
            _logger.LogInformation("Invio messaggio WhatsApp a {To}", request.To);

            // Formatta il numero destinatario nel formato WhatsApp
            var toNumber = FormatWhatsAppNumber(request.To);
            var fromNumber = new PhoneNumber(_twilioSettings.WhatsAppNumber);

            MessageResource? message;

            // Invia messaggio con o senza media
            if (!string.IsNullOrEmpty(request.MediaUrl))
            {
                message = await MessageResource.CreateAsync(
                    body: request.Message,
                    from: fromNumber,
                    to: toNumber,
                    mediaUrl: new List<Uri> { new Uri(request.MediaUrl) }
                );
            }
            else
            {
                message = await MessageResource.CreateAsync(
                    body: request.Message,
                    from: fromNumber,
                    to: toNumber
                );
            }

            _logger.LogInformation("Messaggio inviato con successo. ID: {MessageId}, Stato: {Status}",
                message.Sid, message.Status);

            return new WhatsAppMessageResponse
            {
                Success = true,
                MessageId = message.Sid,
                Status = message.Status.ToString(),
                To = request.To,
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'invio del messaggio WhatsApp a {To}", request.To);

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
    /// Verifica la connessione con Twilio controllando l'account
    /// </summary>
    public async Task<bool> VerifyConnectionAsync()
    {
        try
        {
            var account = await Twilio.Rest.Api.V2010.AccountResource.FetchAsync(
                pathSid: _twilioSettings.AccountSid
            );

            _logger.LogInformation("Connessione Twilio verificata. Account: {AccountName}", account.FriendlyName);
            return account != null && account.Status == Twilio.Rest.Api.V2010.AccountResource.StatusEnum.Active;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la verifica della connessione Twilio");
            return false;
        }
    }

    /// <summary>
    /// Formatta il numero di telefono nel formato WhatsApp richiesto da Twilio
    /// </summary>
    private PhoneNumber FormatWhatsAppNumber(string phoneNumber)
    {
        // Rimuove spazi e caratteri non numerici (eccetto il +)
        var cleaned = new string(phoneNumber.Where(c => char.IsDigit(c) || c == '+').ToArray());

        // Assicura che il numero inizi con +
        if (!cleaned.StartsWith("+"))
        {
            cleaned = "+" + cleaned;
        }

        // Aggiunge il prefisso whatsapp:
        return new PhoneNumber($"whatsapp:{cleaned}");
    }
}
