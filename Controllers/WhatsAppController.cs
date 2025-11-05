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
            version = "1.0.0",
            description = "API per l'invio di messaggi WhatsApp tramite Meta WhatsApp Business API",
            endpoints = new[]
            {
                new { method = "POST", path = "/api/whatsapp/send", description = "Invia un messaggio WhatsApp (JSON)" },
                new { method = "GET", path = "/api/whatsapp/send-simple", description = "Invia un messaggio semplice (query params)" },
                new { method = "GET", path = "/api/whatsapp/health", description = "Verifica lo stato del servizio" },
                new { method = "GET", path = "/api/whatsapp/info", description = "Informazioni sull'API" }
            },
            documentation = "/swagger"
        });
    }
}
