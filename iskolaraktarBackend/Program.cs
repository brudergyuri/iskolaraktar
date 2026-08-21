using iskolaraktarBackend.Data;
using iskolaraktarBackend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// külön fájlban, hogy publikálás után is szerkeszthető legyen szövegszerkesztővel (nem kell újrafordítás a kapcsolati adatok módosításához)
builder.Configuration.AddJsonFile("dbsettings.json", optional: false, reloadOnChange: true);

// MVC vezérlők (Controllers mappa) és a Swagger/OpenAPI leíró generálásának bekapcsolása
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MySQL kapcsolatgyár: egyetlen példány az egész alkalmazás életciklusára (a kapcsolati string nem változik futás közben)
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
// Dinamikus tábla repository: kérésenként új példány, mert a MySqlConnection-t is kérésenként nyitja/zárja
builder.Services.AddScoped<IDynamicTableRepository, DynamicTableRepository>();
// Auth konfiguráció (auth.json): egyetlen példány, mert a betöltött állapotot memóriában tartja és zárolással szinkronizálja
builder.Services.AddSingleton<IAuthConfigService, AuthConfigService>();

var app = builder.Build();

// Csak fejlesztői környezetben él a Swagger UI, ami a gyökér ('/') útvonalon jelenik meg
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Iskolaraktár API v1");
        options.RoutePrefix = string.Empty;
    });
}

// HTTP kérések automatikus átirányítása HTTPS-re
app.UseHttpsRedirection();

// A [Route]/[Http*] attribútumokkal jelölt vezérlő-végpontok bekötése
app.MapControllers();

app.Run();
