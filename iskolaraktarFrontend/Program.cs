var builder = WebApplication.CreateBuilder(args);

// önaláírt tanúsítvány elérési útja/jelszava,
// hogy publikálás után is cserélhető legyen külön fájlból
builder.Configuration.AddJsonFile(
    "certsettings.json",
    optional: false,
    reloadOnChange: true
);

// Razor Pages
builder.Services.AddRazorPages();

// A frontend ezen keresztül továbbítja az /api/... kéréseket
// a helyben futó backendnek.
builder.Services.AddHttpClient(
    "BackendProxy",
    client =>
    {
        client.BaseAddress =
            new Uri("http://localhost:5136");
    }
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Dev Tunnel miatt nincs HTTPS átirányítás localhostra.
// app.UseHttpsRedirection();

//
// /api/... reverse proxy
//
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    var httpClientFactory =
        context.RequestServices
            .GetRequiredService<IHttpClientFactory>();

    var client =
        httpClientFactory.CreateClient("BackendProxy");

    var targetUrl =
        $"{context.Request.Path}{context.Request.QueryString}";

    using var requestMessage =
        new HttpRequestMessage(
            new HttpMethod(context.Request.Method),
            targetUrl
        );

    var hasBody =
        context.Request.ContentLength > 0 ||
        context.Request.Headers.ContainsKey(
            "Transfer-Encoding"
        );

    if (hasBody)
    {
        requestMessage.Content =
            new StreamContent(context.Request.Body);
    }

    foreach (var header in context.Request.Headers)
    {
        if (header.Key.Equals(
                "Host",
                StringComparison.OrdinalIgnoreCase
            ))
        {
            continue;
        }

        if (!requestMessage.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value.ToArray()
            ))
        {
            requestMessage.Content?.Headers
                .TryAddWithoutValidation(
                    header.Key,
                    header.Value.ToArray()
                );
        }
    }

    using var responseMessage =
        await client.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted
        );

    context.Response.StatusCode =
        (int)responseMessage.StatusCode;

    foreach (var header in responseMessage.Headers)
    {
        context.Response.Headers[header.Key] =
            header.Value.ToArray();
    }

    foreach (var header in responseMessage.Content.Headers)
    {
        context.Response.Headers[header.Key] =
            header.Value.ToArray();
    }

    context.Response.Headers.Remove(
        "transfer-encoding"
    );

    await responseMessage.Content.CopyToAsync(
        context.Response.Body,
        context.RequestAborted
    );
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();