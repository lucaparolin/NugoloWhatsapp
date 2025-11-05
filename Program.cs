using WhatsAppAPI.Models;
using WhatsAppAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Meta WhatsApp Business settings from appsettings.json
builder.Services.Configure<WhatsAppBusinessSettings>(
    builder.Configuration.GetSection("WhatsAppBusiness"));

// Register HttpClient for WhatsApp Service
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();

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
