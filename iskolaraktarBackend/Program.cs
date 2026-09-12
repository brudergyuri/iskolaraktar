using iskolaraktarBackend.Data;
using iskolaraktarBackend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// külön fájlban, hogy publikálás után is szerkeszthető legyen szövegszerkesztővel
// (nem kell újrafordítás a kapcsolati adatok módosításához)
builder.Configuration.AddJsonFile(
    "dbsettings.json",
    optional: false,
    reloadOnChange: true
);

// önaláírt tanúsítvány elérési útja/jelszava,
// hogy publikálás után is cserélhető legyen külön fájlból
builder.Configuration.AddJsonFile(
    "certsettings.json",
    optional: false,
    reloadOnChange: true
);

// A QR-kódba kerülő publikus frontend cím.
builder.Configuration.AddJsonFile(
    "qrsettings.json",
    optional: false,
    reloadOnChange: true
);

// MVC vezérlők és Swagger/OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7142",
                "http://localhost:5283"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// MySQL kapcsolatgyár
builder.Services.AddSingleton<
    IDbConnectionFactory,
    MySqlConnectionFactory
>();

// Dinamikus tábla repository
builder.Services.AddScoped<
    IDynamicTableRepository,
    DynamicTableRepository
>();

// Auth konfiguráció
builder.Services.AddSingleton<
    IAuthConfigService,
    AuthConfigService
>();

// QR konfiguráció
builder.Services.AddSingleton<
    IQrSettingsService,
    QrSettingsService
>();

// Első indításkori inicializálás:
// adatbázis + első admin létrehozása
builder.Services.AddSingleton<StartupInitializer>();

var app = builder.Build();

// Első induláskor:
// - létrehozza az adatbázist, ha még nincs
// - létrehozza az admin / admin123 felhasználót,
//   ha még nincs auth.json
var startupInitializer =
    app.Services.GetRequiredService<StartupInitializer>();

await startupInitializer.InitializeAsync();

// Swagger fejlesztői környezetben
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Iskolaraktár API v1"
        );

        options.RoutePrefix = string.Empty;
    });
}

// Dev Tunnel / frontend proxy miatt nincs szükség
// automatikus localhost HTTPS átirányításra.
//
// app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

app.Run();