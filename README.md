# WhatsApp API - Servizio di Invio Messaggi

API REST in C# (.NET 8) per l'invio di messaggi WhatsApp tramite Twilio.

## Caratteristiche

- ✉️ Invio messaggi di testo via WhatsApp
- 🖼️ Supporto per invio di immagini
- 📊 Documentazione Swagger integrata
- 🔍 Health check endpoint
- 📝 Logging completo
- ⚡ Async/Await per prestazioni ottimali

## Prerequisiti

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Account [Twilio](https://www.twilio.com/) con WhatsApp abilitato
- Visual Studio 2022, VS Code o Rider (opzionale)

## Configurazione Twilio

### 1. Crea un Account Twilio

1. Registrati su [Twilio](https://www.twilio.com/try-twilio)
2. Verifica il tuo numero di telefono
3. Accedi alla console Twilio

### 2. Ottieni le Credenziali

Dalla dashboard Twilio, copia:
- **Account SID**
- **Auth Token**

### 3. Configura WhatsApp

1. Vai su "Messaging" > "Try it out" > "Send a WhatsApp message"
2. Segui le istruzioni per connettere il tuo numero WhatsApp al sandbox Twilio
3. Invia il codice richiesto al numero Twilio (es: `join <codice>`)
4. Copia il numero WhatsApp del sandbox (formato: `whatsapp:+14155238886`)

## Installazione e Configurazione

### 1. Clone del Repository

```bash
git clone https://github.com/tuoaccount/NugoloWhatsapp.git
cd NugoloWhatsapp
```

### 2. Configurazione

Modifica il file `appsettings.json` inserendo le tue credenziali Twilio:

```json
{
  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "il_tuo_auth_token",
    "WhatsAppNumber": "whatsapp:+14155238886"
  }
}
```

⚠️ **IMPORTANTE**: Non committare mai le credenziali reali su Git! Usa variabili d'ambiente in produzione.

### 3. Restore delle Dipendenze

```bash
dotnet restore
```

### 4. Build del Progetto

```bash
dotnet build
```

### 5. Avvio dell'Applicazione

```bash
dotnet run
```

L'API sarà disponibile su:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## Utilizzo dell'API

### Endpoint Disponibili

#### 1. Invia Messaggio (POST)

**Endpoint:** `POST /api/whatsapp/send`

**Body (JSON):**
```json
{
  "to": "+393331234567",
  "message": "Ciao! Questo è un messaggio di test da WhatsApp API.",
  "mediaUrl": "https://esempio.com/immagine.jpg"
}
```

**Esempio cURL:**
```bash
curl -X POST https://localhost:5001/api/whatsapp/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "+393331234567",
    "message": "Ciao da WhatsApp API!"
  }'
```

**Risposta:**
```json
{
  "success": true,
  "messageId": "SMxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "status": "queued",
  "to": "+393331234567",
  "sentAt": "2024-01-15T10:30:00Z"
}
```

#### 2. Invia Messaggio Semplice (GET)

**Endpoint:** `GET /api/whatsapp/send-simple`

**Parametri Query:**
- `to`: Numero destinatario (es: +393331234567)
- `message`: Testo del messaggio

**Esempio:**
```
https://localhost:5001/api/whatsapp/send-simple?to=+393331234567&message=Ciao!
```

#### 3. Health Check

**Endpoint:** `GET /api/whatsapp/health`

Verifica lo stato della connessione con Twilio.

**Risposta:**
```json
{
  "status": "healthy",
  "service": "WhatsApp API",
  "timestamp": "2024-01-15T10:30:00Z",
  "twilioConnection": "active"
}
```

#### 4. Informazioni API

**Endpoint:** `GET /api/whatsapp/info`

Restituisce informazioni sull'API e gli endpoint disponibili.

## Esempi di Utilizzo

### C# HttpClient

```csharp
using System.Net.Http.Json;

var client = new HttpClient();
var request = new
{
    to = "+393331234567",
    message = "Ciao da C#!"
};

var response = await client.PostAsJsonAsync(
    "https://localhost:5001/api/whatsapp/send",
    request
);

var result = await response.Content.ReadFromJsonAsync<WhatsAppMessageResponse>();
Console.WriteLine($"Messaggio inviato: {result.Success}");
```

### JavaScript/Fetch

```javascript
const response = await fetch('https://localhost:5001/api/whatsapp/send', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    to: '+393331234567',
    message: 'Ciao da JavaScript!'
  })
});

const result = await response.json();
console.log('Messaggio inviato:', result.success);
```

### Python

```python
import requests

url = "https://localhost:5001/api/whatsapp/send"
payload = {
    "to": "+393331234567",
    "message": "Ciao da Python!"
}

response = requests.post(url, json=payload)
result = response.json()
print(f"Messaggio inviato: {result['success']}")
```

## Struttura del Progetto

```
NugoloWhatsapp/
├── Controllers/
│   └── WhatsAppController.cs      # Controller API REST
├── Models/
│   ├── WhatsAppMessageRequest.cs  # Modello richiesta
│   ├── WhatsAppMessageResponse.cs # Modello risposta
│   └── TwilioSettings.cs          # Configurazione Twilio
├── Services/
│   ├── IWhatsAppService.cs        # Interfaccia servizio
│   └── WhatsAppService.cs         # Implementazione servizio
├── Properties/
│   └── launchSettings.json        # Configurazione avvio
├── appsettings.json               # Configurazione applicazione
├── appsettings.Development.json   # Configurazione sviluppo
├── Program.cs                     # Entry point
└── WhatsAppAPI.csproj             # File progetto
```

## Note Importanti

### Limitazioni Sandbox Twilio

Con l'account trial di Twilio:
- Puoi inviare messaggi solo ai numeri verificati nel sandbox
- I messaggi potrebbero avere un prefisso "Sent from your Twilio trial account"
- Per uso in produzione, è necessario un account Twilio a pagamento

### Formato Numeri di Telefono

I numeri devono essere in formato internazionale:
- ✅ Corretto: `+393331234567`
- ✅ Corretto: `+14155238886`
- ❌ Errato: `3331234567`
- ❌ Errato: `00393331234567`

### Sicurezza

- Non committare mai `appsettings.json` con credenziali reali
- Usa variabili d'ambiente in produzione
- Implementa autenticazione (JWT, API Key) prima di esporre in produzione
- Considera rate limiting per evitare abusi

## Deployment

### Variabili d'Ambiente (Produzione)

```bash
export Twilio__AccountSid="ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
export Twilio__AuthToken="il_tuo_auth_token"
export Twilio__WhatsAppNumber="whatsapp:+14155238886"
```

### Docker (Opzionale)

Crea un `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "WhatsAppAPI.dll"]
```

Build e run:
```bash
docker build -t whatsapp-api .
docker run -p 8080:80 \
  -e Twilio__AccountSid="ACxxxx" \
  -e Twilio__AuthToken="token" \
  -e Twilio__WhatsAppNumber="whatsapp:+14155238886" \
  whatsapp-api
```

## Troubleshooting

### Errore 401 Unauthorized
- Verifica che AccountSid e AuthToken siano corretti
- Controlla di aver copiato le credenziali dalla console Twilio

### Errore 21211 Invalid 'To' Phone Number
- Il numero destinatario deve essere in formato internazionale
- Con sandbox Twilio, il destinatario deve aver aderito al sandbox

### Messaggio non ricevuto
- Verifica che il numero abbia aderito al sandbox Twilio
- Controlla i log dell'applicazione
- Verifica lo stato del messaggio nella console Twilio

## Risorse Utili

- [Twilio WhatsApp API Docs](https://www.twilio.com/docs/whatsapp)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [.NET 8 Release Notes](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8)

## Licenza

MIT License - Sentiti libero di usare questo progetto per scopi personali o commerciali.

## Supporto

Per problemi o domande:
- Apri una issue su GitHub
- Consulta la documentazione Twilio
- Controlla i log dell'applicazione per dettagli sugli errori

## Autore

Sviluppato con ❤️ per semplificare l'invio di messaggi WhatsApp via API.
