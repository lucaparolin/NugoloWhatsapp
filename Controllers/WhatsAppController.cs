using Microsoft.AspNetCore.Mvc;
using WhatsAppAPI.Models;
using WhatsAppAPI.Services;

namespace WhatsAppAPI.Controllers;

/// <summary>
/// Controller per la gestione dell'invio di messaggi WhatsApp
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(IWhatsAppService whatsAppService, ILogger<WhatsAppController> logger)
    {
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    /// <summary>
    /// Invia un messaggio WhatsApp a un destinatario
    /// </summary>
    /// <param name="request">Dati del messaggio da inviare</param>
    /// <returns>Risposta con l'esito dell'invio</returns>
    /// <response code="200">Messaggio inviato con successo</response>
    /// <response code="400">Dati della richiesta non validi</response>
    /// <response code="500">Errore interno del server</response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(WhatsAppMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WhatsAppMessageResponse>> SendMessage([FromBody] WhatsAppMessageRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Richiesta non valida: {Errors}", ModelState.Values);
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _whatsAppService.SendMessageAsync(request);

            if (response.Success)
            {
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'elaborazione della richiesta di invio messaggio");
            return StatusCode(StatusCodes.Status500InternalServerError, new WhatsAppMessageResponse
            {
                Success = false,
                ErrorMessage = "Si è verificato un errore durante l'invio del messaggio",
                SentAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Invia un messaggio WhatsApp semplice (solo testo)
    /// </summary>
    /// <param name="to">Numero di telefono del destinatario</param>
    /// <param name="message">Testo del messaggio</param>
    /// <returns>Risposta con l'esito dell'invio</returns>
    [HttpGet("send-simple")]
    [ProducesResponseType(typeof(WhatsAppMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WhatsAppMessageResponse>> SendSimpleMessage(
        [FromQuery] string to,
        [FromQuery] string message)
    {
        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(message))
        {
            return BadRequest("I parametri 'to' e 'message' sono obbligatori");
        }

        var request = new WhatsAppMessageRequest
        {
            To = to,
            Message = message
        };

        var response = await _whatsAppService.SendMessageAsync(request);

        if (response.Success)
        {
            return Ok(response);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    /// <summary>
    /// Invia un'immagine via WhatsApp
    /// </summary>
    /// <param name="to">Numero di telefono del destinatario</param>
    /// <param name="imageUrl">URL dell'immagine</param>
    /// <param name="caption">Caption dell'immagine (opzionale)</param>
    /// <returns>Risposta con l'esito dell'invio</returns>
    [HttpPost("send-image")]
    [ProducesResponseType(typeof(WhatsAppMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WhatsAppMessageResponse>> SendImage(
        [FromQuery] string to,
        [FromQuery] string imageUrl,
        [FromQuery] string? caption = null)
    {
        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(imageUrl))
        {
            return BadRequest("I parametri 'to' e 'imageUrl' sono obbligatori");
        }

        var request = new WhatsAppMessageRequest
        {
            To = to,
            MediaUrl = imageUrl,
            Message = caption,
            MediaType = MediaType.Image
        };

        var response = await _whatsAppService.SendMessageAsync(request);
        return response.Success ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    /// <summary>
    /// Invia un video via WhatsApp
    /// </summary>
    /// <param name="to">Numero di telefono del destinatario</param>
    /// <param name="videoUrl">URL del video</param>
    /// <param name="caption">Caption del video (opzionale)</param>
    /// <returns>Risposta con l'esito dell'invio</returns>
    [HttpPost("send-video")]
    [ProducesResponseType(typeof(WhatsAppMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WhatsAppMessageResponse>> SendVideo(
        [FromQuery] string to,
        [FromQuery] string videoUrl,
        [FromQuery] string? caption = null)
    {
        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(videoUrl))
        {
            return BadRequest("I parametri 'to' e 'videoUrl' sono obbligatori");
        }

        var request = new WhatsAppMessageRequest
        {
            To = to,
            MediaUrl = videoUrl,
            Message = caption,
            MediaType = MediaType.Video
        };

        var response = await _whatsAppService.SendMessageAsync(request);
        return response.Success ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    /// <summary>
    /// Invia un audio via WhatsApp
    /// </summary>
    /// <param name="to">Numero di telefono del destinatario</param>
    /// <param name="audioUrl">URL del file audio</param>
    /// <returns>Risposta con l'esito dell'invio</returns>
    [HttpPost("send-audio")]
    [ProducesResponseType(typeof(WhatsAppMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WhatsAppMessageResponse>> SendAudio(
        [FromQuery] string to,
        [FromQuery] string audioUrl)
    {
        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(audioUrl))
        {
            return BadRequest("I parametri 'to' e 'audioUrl' sono obbligatori");
        }

        var request = new WhatsAppMessageRequest
        {
            To = to,
            MediaUrl = audioUrl,
            MediaType = MediaType.Audio
        };

        var response = await _whatsAppService.SendMessageAsync(request);
        return response.Success ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    /// <summary>
    /// Invia un documento via WhatsApp
    /// </summary>
    /// <param name="to">Numero di telefono del destinatario</param>
    /// <param name="documentUrl">URL del documento</param>
    /// <param name="fileName">Nome del file (opzionale)</param>
    /// <param name="caption">Caption del documento (opzionale)</param>
    /// <returns>Risposta con l'esito dell'invio</returns>
    [HttpPost("send-document")]
    [ProducesResponseType(typeof(WhatsAppMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WhatsAppMessageResponse>> SendDocument(
        [FromQuery] string to,
        [FromQuery] string documentUrl,
        [FromQuery] string? fileName = null,
        [FromQuery] string? caption = null)
    {
        if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(documentUrl))
        {
            return BadRequest("I parametri 'to' e 'documentUrl' sono obbligatori");
        }

        var request = new WhatsAppMessageRequest
        {
            To = to,
            MediaUrl = documentUrl,
            Message = caption,
            FileName = fileName,
            MediaType = MediaType.Document
        };

        var response = await _whatsAppService.SendMessageAsync(request);
        return response.Success ? Ok(response) : StatusCode(StatusCodes.Status500InternalServerError, response);
    }

    /// <summary>
    /// Verifica lo stato della connessione con Meta WhatsApp Business API
    /// </summary>
    /// <returns>Stato della connessione</returns>
    /// <response code="200">Connessione verificata con successo</response>
    /// <response code="503">Servizio non disponibile</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<object>> HealthCheck()
    {
        try
        {
            var isConnected = await _whatsAppService.VerifyConnectionAsync();

            if (isConnected)
            {
                return Ok(new
                {
                    status = "healthy",
                    service = "WhatsApp API",
                    timestamp = DateTime.UtcNow,
                    metaApiConnection = "active"
                });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "unhealthy",
                service = "WhatsApp API",
                timestamp = DateTime.UtcNow,
                metaApiConnection = "inactive",
                message = "Impossibile connettersi a Meta WhatsApp Business API"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il controllo dello stato del servizio");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "unhealthy",
                service = "WhatsApp API",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Ottiene informazioni sull'API
    /// </summary>
    /// <returns>Informazioni sull'API</returns>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetInfo()
    {
        return Ok(new
        {
            name = "WhatsApp API",
            version = "2.0.0",
            description = "API per l'invio di messaggi WhatsApp tramite Meta WhatsApp Business API",
            supportedMediaTypes = new[] { "Text", "Image", "Video", "Audio", "Document" },
            endpoints = new[]
            {
                new { method = "POST", path = "/api/whatsapp/send", description = "Invia un messaggio WhatsApp (JSON, supporta tutti i tipi di media)" },
                new { method = "GET", path = "/api/whatsapp/send-simple", description = "Invia un messaggio di testo semplice (query params)" },
                new { method = "POST", path = "/api/whatsapp/send-image", description = "Invia un'immagine con caption opzionale" },
                new { method = "POST", path = "/api/whatsapp/send-video", description = "Invia un video con caption opzionale" },
                new { method = "POST", path = "/api/whatsapp/send-audio", description = "Invia un file audio" },
                new { method = "POST", path = "/api/whatsapp/send-document", description = "Invia un documento con nome e caption opzionali" },
                new { method = "GET", path = "/api/whatsapp/health", description = "Verifica lo stato del servizio" },
                new { method = "GET", path = "/api/whatsapp/info", description = "Informazioni sull'API" }
            },
            documentation = "/swagger"
        });
    }
}
