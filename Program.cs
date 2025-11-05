using WhatsAppAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register WhatsApp Service
builder.Services.AddSingleton<IWhatsAppService, WhatsAppService>();

// Configure Twilio settings from appsettings.json
builder.Services.Configure<TwilioSettings>(
    builder.Configuration.GetSection("Twilio"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
