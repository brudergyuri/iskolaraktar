using iskolaraktarBackend.Data;
using iskolaraktarBackend.Repositories;

var builder = WebApplication.CreateBuilder(args);

// külön fájlban, hogy publikálás után is szerkeszthető legyen szövegszerkesztővel
builder.Configuration.AddJsonFile("dbsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IDynamicTableRepository, DynamicTableRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Iskolaraktár API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
