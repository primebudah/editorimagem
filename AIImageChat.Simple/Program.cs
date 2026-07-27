var builder = WebApplication.CreateBuilder(args);

// Configurar porta dinâmica para Railway
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(int.Parse(port));
    });
}

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionar HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Comentado para Railway proxy
app.UseDefaultFiles(); // Serve index.html na rota raiz
app.UseStaticFiles(); // Para servir arquivos estáticos

// Função para detectar MIME type corretamente
string GetMimeType(string extension)
{
    return extension.ToLower() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => throw new NotSupportedException($"Formato não suportado: {extension}. Use .jpg, .jpeg, .png ou .webp")
    };
}

// Endpoint para editar imagem
app.MapPost("/api/edit-image", async (HttpRequest request, HttpClient httpClient) =>
{
    try
    {
        var form = await request.ReadFormAsync();
        var file = form.Files["image"];
        var prompt = form["prompt"];

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "Nenhuma imagem enviada" });
        }

        if (string.IsNullOrEmpty(prompt))
        {
            return Results.BadRequest(new { error = "Prompt não fornecido" });
        }

        // Detectar MIME type correto
        string mimeType;
        try
        {
            mimeType = GetMimeType(Path.GetExtension(file?.FileName));
        }
        catch (NotSupportedException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        // Ler API Key da variável de ambiente
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            return Results.BadRequest(new { error = "API Key não configurada (OPENAI_API_KEY)" });
        }

        // Ler bytes da imagem
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        // Prompt interno obrigatório para preservação de imagem
        var internalPrompt = @"Você é um especialista em restauração profissional de logos, desenhos, marcas, ilustrações e artes gráficas.

Sua prioridade absoluta é preservar exatamente a imagem enviada pelo usuário.

Considere a imagem original como a referência definitiva.

Regras obrigatórias:

• Não redesenhe a imagem.
• Não recrie a arte.
• Não substitua elementos.
• Não altere o estilo.
• Não altere a composição.
• Não altere o layout.
• Não altere a posição dos objetos.
• Não altere as proporções.
• Não altere a espessura das linhas.
• Não altere a tipografia.
• Não altere as fontes.
• Não altere logotipos.
• Não altere símbolos.
• Não altere ícones.
• Não altere detalhes gráficos.
• Não altere cores, exceto quando solicitado.
• Não adicione elementos.
• Não remova elementos, exceto quando solicitado.
• Não invente detalhes inexistentes.
• Não modernize o design.
• Não interprete artisticamente.
• Não transforme em outro estilo.
• Não faça releitura.
• Não faça redesign.
• Preserve integralmente todas as características visuais da imagem original.

Sua função é agir como um restaurador profissional e não como um artista.

A única alteração permitida é exatamente aquela solicitada pelo usuário.

Caso exista qualquer dúvida entre modificar ou preservar, SEMPRE preserve.

Caso a solicitação seja impossível sem alterar a identidade visual, realize a menor alteração possível.

Melhore apenas a qualidade técnica da imagem, aumentando definição, nitidez, limpeza, resolução e acabamento, sem modificar sua identidade visual.

Solicitação do usuário:

";

        // Combinar prompt interno com prompt do usuário
        var fullPrompt = internalPrompt + (prompt.ToString() ?? string.Empty);

        // Criar multipart/form-data
        var multipartContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        multipartContent.Add(imageContent, "image", $"image{Path.GetExtension(file?.FileName)}");
        multipartContent.Add(new StringContent(fullPrompt), "prompt");
        multipartContent.Add(new StringContent("gpt-image-1.5"), "model");
        multipartContent.Add(new StringContent("1024x1024"), "size");
        multipartContent.Add(new StringContent("high"), "quality");

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await httpClient.PostAsync("https://api.openai.com/v1/images/edits", multipartContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return Results.BadRequest(new { error = errorContent });
        }

        var json = await response.Content.ReadAsStringAsync();
        var jsonResponse = System.Text.Json.JsonDocument.Parse(json);
        var dataArray = jsonResponse.RootElement.GetProperty("data");
        var b64Json = dataArray[0].GetProperty("b64_json").GetString();

        if (string.IsNullOrEmpty(b64Json))
        {
            return Results.BadRequest(new { error = "A API não retornou dados de imagem válidos" });
        }

        var imageBytesResult = Convert.FromBase64String(b64Json);
        return Results.File(imageBytesResult, "image/png", "edited_image.png");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("EditImage")
.WithOpenApi();

app.Run();
