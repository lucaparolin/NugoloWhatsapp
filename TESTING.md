# Testing Guide - WhatsApp API

Guida completa per eseguire e comprendere i test automatici del progetto.

## 📋 Panoramica

Il progetto include **30+ test automatici** che validano:
- ✅ Tutti i 7 bug fixes della code review
- ✅ Validazione modelli e input
- ✅ Logica business del servizio
- ✅ Integrazione controller
- ✅ Error handling e edge cases

## 🏗️ Struttura Test

```
WhatsAppAPI.Tests/
├── Models/
│   └── WhatsAppMessageRequestTests.cs      # 9 tests - Validazione modelli
├── Services/
│   └── WhatsAppServiceTests.cs             # 12 tests - Logica servizio
├── Controllers/
│   └── WhatsAppControllerTests.cs          # 10 tests - Endpoint API
└── GlobalUsings.cs                          # Using globali
```

## 🚀 Esecuzione Rapida

### Tutti i test
```bash
dotnet test
```

### Con output dettagliato
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Solo test falliti
```bash
dotnet test --logger "console;verbosity=quiet" || dotnet test --filter "FullyQualifiedName~Failed"
```

## 📊 Test Coverage per Bug Fix

### 1. HttpClient Thread Safety ✅
**Bug:** Race condition su headers condivisi in richieste concorrenti  
**Test:** `SendMessageAsync_ConcurrentRequests_ShouldNotCorruptHeaders`
```csharp
// Invia 10 richieste concorrenti e verifica che ogni richiesta
// usi il token corretto (test_token_12345) senza corruzione
```

### 2. AccessToken Validation ✅
**Bug:** Token vuoto causa fail silenzioso a runtime  
**Test:** `Constructor_WithEmptyAccessToken_ShouldThrowException`
```csharp
// Verifica che InvalidOperationException sia lanciata allo startup
// se AccessToken è vuoto, prevenendo errori 401 silenziosi
```

### 3. Phone Number Normalization ✅
**Bug:** Prefissi 00/+ non normalizzati, Meta API reject  
**Test:** `SendMessageAsync_PhoneNumberNormalization_ShouldNormalizeCorrectly`
```csharp
// Testa conversione di:
// +393331234567 → 393331234567
// 00393331234567 → 393331234567
// +1 (555) 123-4567 → 15551234567
```

### 4. Audio Caption Not Supported ✅
**Bug:** Caption inviato per audio ma ignorato da Meta  
**Test:** `SendMessageAsync_AudioMessage_ShouldNotIncludeCaption`
```csharp
// Verifica che request JSON non contenga campo "caption"
// per messaggi audio
```

### 5. Message Validation ✅
**Bug:** Message nullable senza validazione per MediaType.Text  
**Test:** `Validate_TextMessageWithoutMessage_ShouldFail`
```csharp
// Verifica che messaggio di testo senza Message
// fallisca validazione con errore chiaro
```

### 6. MediaType Enum Validation ✅
**Bug:** Valori enum invalidi (99) accettati  
**Test:** `Validate_InvalidMediaTypeEnum_ShouldFail`
```csharp
// Verifica che MediaType=99 (non definito) fallisca
// validazione con messaggio "MediaType non valido"
```

### 7. Parameter Trimming ✅
**Bug:** Whitespace non rimosso da query parameters  
**Test:** `SendSimpleMessage_WithWhitespace_ShouldTrimParameters`
```csharp
// Verifica che "  +393331234567  " diventi "+393331234567"
// e "  Test  " diventi "Test"
```

## 🧪 Test Categories

### Model Validation (9 tests)
- Validazione condizionale Message/MediaUrl
- Enum range validation
- String length limits
- Required fields
- Phone format validation

### Service Logic (12 tests)
- Constructor validation (fail-fast)
- Phone normalization (5 varianti)
- Audio caption handling
- Thread safety (concurrent requests)
- Success/Error response parsing
- Invalid input rejection

### Controller Integration (10 tests)
- Parameter trimming (5 endpoint)
- Empty parameter validation
- MediaType routing
- Health check states
- API info endpoint
- Error status codes

## 📈 Coverage Report

```bash
# Genera report coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Output HTML interattivo (richiede reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport
```

**Target Coverage:**
- Controllers: >90%
- Services: >85%
- Models: >80%

## 🔍 Test Specifici

### Esegui solo test per bug critici
```bash
dotnet test --filter "FullyQualifiedName~Constructor_WithEmpty"
dotnet test --filter "FullyQualifiedName~ConcurrentRequests"
dotnet test --filter "FullyQualifiedName~PhoneNumberNormalization"
```

### Esegui test per categoria
```bash
# Solo validation
dotnet test --filter "FullyQualifiedName~WhatsAppMessageRequestTests"

# Solo service
dotnet test --filter "FullyQualifiedName~WhatsAppServiceTests"

# Solo controller
dotnet test --filter "FullyQualifiedName~WhatsAppControllerTests"
```

## 🔄 CI/CD Integration

### GitHub Actions
Il workflow `.github/workflows/dotnet-tests.yml` esegue automaticamente:
- ✅ Build del progetto
- ✅ Esecuzione test
- ✅ Report test failures
- ✅ Code coverage
- ✅ Upload risultati

Trigger:
- Push su `main`, `master`, `claude/**`
- Pull Request verso `main`/`master`

### Azure DevOps
```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: '--configuration Release --logger trx --collect:"XPlat Code Coverage"'
```

## 🐛 Debugging Test Failures

### Test fallisce localmente
```bash
# Verbose output
dotnet test --logger "console;verbosity=detailed"

# Solo il test specifico
dotnet test --filter "FullyQualifiedName~TestMethodName"
```

### Test passa localmente ma fallisce in CI
1. Verifica dipendenze (versioni .NET)
2. Controlla timezone/culture (DateTime, formato numeri)
3. Verifica file system case-sensitivity (Linux vs Windows)

### Mock non funziona come previsto
```csharp
// Verifica mock setup con Verify
_mockService.Verify(s => s.Method(It.IsAny<T>()), Times.Once);

// Debug con callback
_mockService
    .Setup(s => s.Method(It.IsAny<T>()))
    .Callback<T>(param => Console.WriteLine($"Called with: {param}"))
    .Returns(result);
```

## 📚 Best Practices

### ✅ DO
- Test isolati (no dipendenze esterne)
- Arrange-Act-Assert pattern
- Test names descrittivi (`Method_Scenario_ExpectedBehavior`)
- Mock dependencies, non HttpClient reale
- Assert multipli con FluentAssertions

### ❌ DON'T
- Test che dipendono da ordine esecuzione
- Test che modificano stato globale
- Hard-coded credentials/tokens reali
- Sleep/delays per timing
- Test che chiamano API esterne reali

## 🔧 Tools Utilizzati

- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library (readable)
- **Coverlet** - Code coverage
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

## 📖 Risorse

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/)
- [.NET Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## 🎯 Prossimi Passi

- [ ] Aggiungere integration tests con TestServer
- [ ] Test performance (benchmark)
- [ ] Test load (concurrent users)
- [ ] Test contract (API schema validation)
- [ ] Mutation testing (Stryker.NET)
