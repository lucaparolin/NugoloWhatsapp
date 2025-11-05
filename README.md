# WhatsApp API - Servizio di Invio Messaggi

API REST in C# (.NET 8) per l'invio di messaggi WhatsApp tramite **Meta WhatsApp Business API** (API native di Facebook/Meta).

## Caratteristiche

- ✉️ Invio messaggi di testo via WhatsApp
- 🖼️ Supporto per invio di immagini con caption
- 📊 Documentazione Swagger integrata
- 🔍 Health check endpoint
- 📝 Logging completo
- ⚡ Async/Await per prestazioni ottimali
- 🚀 Integrazione diretta con le API native di Meta (no intermediari)

## Prerequisiti

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Account [Meta for Developers](https://developers.facebook.com/)
- WhatsApp Business Account configurato
- Visual Studio 2022, VS Code o Rider (opzionale)

## Configurazione Meta WhatsApp Business API

### 1. Crea un Account Meta for Developers

1. Vai su [Meta for Developers](https://developers.facebook.com/)
2. Accedi con il tuo account Facebook
3. Crea una nuova app (tipo "Business")

### 2. Configura WhatsApp Business

1. Nel dashboard della tua app, vai su "Add Products"
2. Seleziona "WhatsApp" e clicca su "Set up"
3. Segui la procedura guidata per:
   - Collegare o creare un Business Account
   - Aggiungere un numero di telefono
   - Verificare il numero di telefono

### 3. Ottieni le Credenziali

#### Access Token

1. Nel dashboard WhatsApp, vai su "API Setup"
2. Copia il **Temporary Access Token** (valido 24 ore)
3. Per produzione, genera un **Permanent Access Token**:
   - Vai su Settings > Business settings > System users
   - Crea un nuovo system user
   - Genera un token con i permessi `whatsapp_business_messaging`

#### Phone Number ID

1. Nel dashboard WhatsApp, vai su "API Setup"
2. Troverai il **Phone Number ID** sotto il tuo numero di telefono
3. Copia questo ID (formato: `123456789012345`)

### 4. Aggiungi Numeri di Test (Sviluppo)

Durante la fase di test, puoi inviare messaggi solo a numeri verificati:

1. Nel dashboard WhatsApp, vai su "API Setup"
2. Nella sezione "To", clicca su "Manage phone number list"
3. Aggiungi i numeri di telefono per il testing
4. Ogni numero riceverà un codice di verifica via SMS

## Installazione e Configurazione

### 1. Clone del Repository

```bash
git clone https://github.com/lucaparolin/NugoloWhatsapp.git
cd NugoloWhatsapp
```

### 2. Configurazione

Modifica il file `appsettings.json` inserendo le tue credenziali Meta:

```json
{
  "WhatsAppBusiness": {
    "AccessToken": "EAAxxxxxxxxxxxx",
    "PhoneNumberId": "123456789012345",
    "ApiVersion": "v18.0",
    "BaseUrl": "https://graph.facebook.com"
  }
}
```

**Parametri:**
- **AccessToken**: Token di accesso ottenuto dal dashboard Meta
- **PhoneNumberId**: ID del numero di telefono WhatsApp Business
- **ApiVersion**: Versione dell'API Graph (consigliato: v18.0 o superiore)
- **BaseUrl**: URL base dell'API Graph (normalmente non va modificato)

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

**Parametri:**
- `to` (obbligatorio): Numero destinatario in formato internazionale (con +)
- `message` (obbligatorio): Testo del messaggio (max 1600 caratteri)
- `mediaUrl` (opzionale): URL di un'immagine da inviare

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
  "messageId": "wamid.HBgNMzkzMzEyMzQ1NjcVAgARGBI5QUFENDE1...",
  "status": "sent",
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

Verifica lo stato della connessione con Meta WhatsApp Business API.

**Risposta:**
```json
{
  "status": "healthy",
  "service": "WhatsApp API",
  "timestamp": "2024-01-15T10:30:00Z",
  "metaApiConnection": "active"
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

### Invio Immagine con Caption

```bash
curl -X POST https://localhost:5001/api/whatsapp/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "+393331234567",
    "message": "Guarda questa immagine!",
    "mediaUrl": "https://picsum.photos/800/600"
  }'
```

## Struttura del Progetto

```
NugoloWhatsapp/
├── Controllers/
│   └── WhatsAppController.cs      # Controller API REST
├── Models/
│   ├── WhatsAppMessageRequest.cs  # Modello richiesta
│   ├── WhatsAppMessageResponse.cs # Modello risposta
│   ├── WhatsAppBusinessSettings.cs # Configurazione Meta
│   └── MetaApiModels.cs           # Modelli API Meta
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

## Limitazioni e Requisiti

### Account di Sviluppo vs Produzione

**Account di Sviluppo (Sandbox):**
- Puoi inviare messaggi solo ai numeri verificati nella lista test
- Limite: circa 250 conversazioni ogni 24 ore
- Gratuito per il testing

**Account di Produzione:**
- Richiede la verifica del Business Account
- Devi completare il Business Verification process di Meta
- Richiede l'approvazione dell'app da parte di Meta
- Necessita di un token di accesso permanente
- Costi: consultare la [pricing page di Meta](https://developers.facebook.com/docs/whatsapp/pricing)

### Formato Numeri di Telefono

I numeri devono essere in formato internazionale:
- ✅ Corretto: `+393331234567`
- ✅ Corretto: `+14155238886`
- ❌ Errato: `3331234567` (senza prefisso internazionale)
- ❌ Errato: `00393331234567` (usa + invece di 00)

### Template Messages

Per inviare messaggi a utenti che non hanno iniziato una conversazione nelle ultime 24 ore, devi usare i **Template Messages**:
- Vai nel dashboard Meta WhatsApp
- Crea un Message Template
- Aspetta l'approvazione da Meta
- Usa l'API con il template approvato

**Nota:** Questa implementazione attuale supporta messaggi liberi (session messages). Per i template messages è necessaria un'estensione dell'API.

## Sicurezza

### Protezione delle Credenziali

1. **Non committare mai** `appsettings.json` con credenziali reali
2. Usa variabili d'ambiente in produzione:

```bash
export WhatsAppBusiness__AccessToken="EAAxxxxxxxxxxxx"
export WhatsAppBusiness__PhoneNumberId="123456789012345"
```

3. Usa Azure Key Vault, AWS Secrets Manager o simili in cloud

### Autenticazione API

Prima di esporre l'API in produzione, implementa l'autenticazione:
- JWT Bearer Token
- API Key authentication
- OAuth 2.0

### Rate Limiting

Implementa rate limiting per evitare abusi:
- Meta ha limiti di throughput (messaggi al secondo)
- Implementa una coda per gestire picchi di traffico

### HTTPS

In produzione, usa sempre HTTPS per proteggere i dati in transito.

## Deployment

### Variabili d'Ambiente (Produzione)

```bash
export WhatsAppBusiness__AccessToken="EAAxxxxxxxxxxxx"
export WhatsAppBusiness__PhoneNumberId="123456789012345"
export WhatsAppBusiness__ApiVersion="v18.0"
```

### Docker

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
  -e WhatsAppBusiness__AccessToken="EAAxxxx" \
  -e WhatsAppBusiness__PhoneNumberId="123456789012345" \
  whatsapp-api
```

### Azure App Service

1. Crea un'App Service su Azure
2. Configura le variabili d'ambiente nelle Application Settings
3. Deploy tramite Visual Studio, CLI o GitHub Actions

## Troubleshooting

### Errore 401 Unauthorized
- Verifica che l'Access Token sia valido e non scaduto
- Per token permanenti, controlla i permessi del system user

### Errore 403 Forbidden
- Il numero destinatario non è verificato (in modalità sviluppo)
- Aggiungi il numero alla lista dei numeri di test nel dashboard Meta

### Errore 131031 - Message Recipient Not Whitelisted
- In modalità sviluppo, puoi inviare solo ai numeri nella whitelist
- Aggiungi il numero nel dashboard Meta > WhatsApp > API Setup > Recipients

### Errore 131047 - Re-engagement message
- Stai cercando di inviare un messaggio dopo 24 ore dall'ultima interazione
- Usa un Template Message invece di un messaggio libero

### Errore 100 - Invalid Parameter
- Controlla il formato del numero di telefono
- Verifica che il PhoneNumberId sia corretto
- Controlla che l'URL media sia pubblicamente accessibile (per immagini)

### Messaggio non ricevuto
- Verifica che il numero WhatsApp Business sia attivo
- Controlla i log dell'applicazione per errori
- Verifica lo stato del messaggio nel dashboard Meta

## Monitoring

### Webhook per Status Updates

Per ricevere aggiornamenti sullo stato dei messaggi (consegnato, letto, fallito), configura un webhook:

1. Nel dashboard Meta, vai su WhatsApp > Configuration
2. Aggiungi un Webhook URL (es: `https://tuodominio.com/webhook`)
3. Sottoscrivi agli eventi: `messages`, `message_status`
4. Implementa un endpoint POST nel tuo controller per ricevere gli aggiornamenti

### Analytics

Meta fornisce analytics nel dashboard:
- Numero di messaggi inviati
- Tasso di consegna
- Conversazioni attive
- Costs overview

## Costi

Meta WhatsApp Business API ha un modello di pricing basato su conversazioni:
- **Conversazioni Service:** iniziate dal business (es: notifiche)
- **Conversazioni User:** iniziate dall'utente

Consulta la [Meta WhatsApp Pricing](https://developers.facebook.com/docs/whatsapp/pricing) per dettagli aggiornati.

**Nota:** I primi 1000 messaggi/mese potrebbero essere gratuiti (verifica le condizioni attuali).

## Risorse Utili

- [Meta WhatsApp Business API Docs](https://developers.facebook.com/docs/whatsapp/cloud-api)
- [Getting Started Guide](https://developers.facebook.com/docs/whatsapp/cloud-api/get-started)
- [API Reference](https://developers.facebook.com/docs/whatsapp/cloud-api/reference)
- [Message Templates](https://developers.facebook.com/docs/whatsapp/cloud-api/guides/send-message-templates)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [.NET 8 Release Notes](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8)

## Estensioni Future

Possibili miglioramenti da implementare:
- [ ] Supporto per Template Messages
- [ ] Webhook per ricevere messaggi in entrata
- [ ] Supporto per altri tipi di media (video, audio, documenti)
- [ ] Supporto per messaggi interattivi (bottoni, liste)
- [ ] Rate limiting e code management
- [ ] Database per storico messaggi
- [ ] Dashboard web per monitoring

## Licenza

MIT License - Sentiti libero di usare questo progetto per scopi personali o commerciali.

## Supporto

Per problemi o domande:
- Apri una issue su GitHub
- Consulta la documentazione Meta WhatsApp Business API
- Controlla i log dell'applicazione per dettagli sugli errori

## Autore

Sviluppato per semplificare l'integrazione con Meta WhatsApp Business API utilizzando .NET 8 e C#.
