# WhatsApp API - Test Suite

Test automatici completi per validare funzionalità e bug fixes.

## Test Coverage

### 1. Model Validation Tests (`Models/WhatsAppMessageRequestTests.cs`)
- ✅ Validazione Message obbligatorio per Text
- ✅ Validazione MediaUrl obbligatorio per media types
- ✅ Validazione enum MediaType range (fix bug #6)
- ✅ Validazione lunghezza messaggio (max 1600 caratteri)
- ✅ Validazione numero telefono richiesto
- ✅ Test per tutti i media types (Image, Video, Audio, Document)

### 2. Service Tests (`Services/WhatsAppServiceTests.cs`)
- ✅ Fail-fast validation: AccessToken vuoto (fix bug #2)
- ✅ Fail-fast validation: PhoneNumberId vuoto (fix bug #2)
- ✅ Phone normalization: +39, 0039, formati vari (fix bug #3)
- ✅ Audio caption non incluso in request (fix bug #4)
- ✅ Thread safety: concurrent requests headers (fix bug #1)
- ✅ Success response parsing
- ✅ Error response handling
- ✅ Invalid phone number rejection

### 3. Controller Tests (`Controllers/WhatsAppControllerTests.cs`)
- ✅ Parameter trimming su tutti endpoint (fix bug #7)
- ✅ Validazione parametri vuoti/whitespace
- ✅ MediaType corretto per ogni endpoint
- ✅ Health check success/failure
- ✅ API info endpoint
- ✅ Error handling 500

## Esecuzione Test

### Tutti i test
```bash
cd /home/user/NugoloWhatsapp
dotnet test
```

### Con coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test specifici
```bash
# Solo validation tests
dotnet test --filter "FullyQualifiedName~WhatsAppMessageRequestTests"

# Solo service tests
dotnet test --filter "FullyQualifiedName~WhatsAppServiceTests"

# Solo controller tests
dotnet test --filter "FullyQualifiedName~WhatsAppControllerTests"
```

### Con verbose output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Statistiche

- **Totale test:** 30+
- **Model validation:** 9 tests
- **Service logic:** 12 tests
- **Controller integration:** 10 tests
- **Coverage target:** >80%

## Bug Fixes Validati

Tutti i 7 bug critici identificati nella code review sono coperti da test:

1. ✅ **HttpClient thread safety** - Test concurrent requests
2. ✅ **AccessToken validation** - Test empty token constructor
3. ✅ **Phone normalization** - Test 00/+ prefix handling
4. ✅ **Audio caption** - Test caption not included
5. ✅ **Message validation** - Test conditional validation
6. ✅ **MediaType enum** - Test invalid enum values
7. ✅ **Parameter trimming** - Test whitespace removal

## CI/CD Integration

### GitHub Actions
```yaml
- name: Run tests
  run: dotnet test --no-build --verbosity normal
  
- name: Generate coverage
  run: dotnet test /p:CollectCoverage=true
```

### Azure DevOps
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
```
