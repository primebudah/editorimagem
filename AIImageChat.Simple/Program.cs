var builder = WebApplication.CreateBuilder(args);

// Configurar porta 5052
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5052);
});

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

app.UseHttpsRedirection();
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
    var sw = System.Diagnostics.Stopwatch.StartNew();
    long t1 = sw.ElapsedMilliseconds;
    Console.WriteLine($"[BACKEND] T1 - Requisição recebida: {t1} ms");
    
    try
    {
        var form = await request.ReadFormAsync();
        var file = form.Files["image"];
        var prompt = form["prompt"];

        Console.WriteLine($"[API] ===== AUDITORIA DE ARQUIVO =====");
        Console.WriteLine($"[API] Nome do arquivo: {file?.FileName}");
        var extension = Path.GetExtension(file?.FileName);
        Console.WriteLine($"[API] Extensão original: {extension}");
        Console.WriteLine($"[API] Content-Type do arquivo: {file?.ContentType}");
        Console.WriteLine($"[API] Tamanho: {file?.Length} bytes");
        Console.WriteLine($"[API] Prompt: {prompt}");
        Console.WriteLine($"[API] Campos no multipart: {string.Join(", ", form.Keys)}");
        Console.WriteLine($"[API] ===== FIM AUDITORIA =====");

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
            mimeType = GetMimeType(extension);
            Console.WriteLine($"[API] MIME detectado: {mimeType}");
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"[API] ERRO: {ex.Message}");
            return Results.BadRequest(new { error = ex.Message });
        }

        // Ler API Key do arquivo
        var apiKeyFilePath = @"c:\Users\uni_t\OneDrive\Desktop\image editor\api_key.txt";
        var apiKey = File.ReadAllLines(apiKeyFilePath)
            .FirstOrDefault(line => line.StartsWith("OPENAI_API_KEY="))
            ?.Substring("OPENAI_API_KEY=".Length).Trim();

        if (string.IsNullOrEmpty(apiKey))
        {
            return Results.BadRequest(new { error = "API Key não configurada" });
        }

        // Ler bytes da imagem
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        Console.WriteLine($"[API] Bytes lidos: {imageBytes.Length}");
        Console.WriteLine($"[API] ===== EDITANDO IMAGEM =====");
        Console.WriteLine($"[API] Endpoint: /images/edits");
        Console.WriteLine($"[API] Modelo: gpt-image-1.5");
        Console.WriteLine($"[API] Content-Type enviado: {mimeType}");
        Console.WriteLine($"[API] Content-Type: multipart/form-data");

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
        
        Console.WriteLine($"[API] Prompt do usuário: {prompt}");
        Console.WriteLine($"[API] Prompt completo (interno + usuário): {fullPrompt.Length} caracteres");

        // Criar multipart/form-data
        var multipartContent = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        multipartContent.Add(imageContent, "image", $"image{extension}");
        multipartContent.Add(new StringContent(fullPrompt), "prompt");
        multipartContent.Add(new StringContent("gpt-image-1.5"), "model");
        multipartContent.Add(new StringContent("1024x1024"), "size");
        multipartContent.Add(new StringContent("high"), "quality");

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        long t2 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[BACKEND] T2 - Início da chamada para OpenAI: {t2} ms");

        var response = await httpClient.PostAsync("https://api.openai.com/v1/images/edits", multipartContent);

        long t3 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[BACKEND] T3 - Primeiro byte recebido da OpenAI: {t3} ms");

        Console.WriteLine($"[API] Status HTTP: {(int)response.StatusCode} {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[API] Erro: {errorContent}");
            return Results.BadRequest(new { error = errorContent });
        }

        var json = await response.Content.ReadAsStringAsync();
        
        long t4 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[BACKEND] T4 - Resposta completa da OpenAI: {t4} ms");
        
        Console.WriteLine($"[API] Response: {json}");

        // Auditoria da conversão Base64
        long t4_1 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[AUDITORIA] T4.1 - Início desserialização JSON: {t4_1} ms");
        
        var jsonResponse = System.Text.Json.JsonDocument.Parse(json);
        
        long t4_2 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[AUDITORIA] T4.2 - Desserialização JSON concluída: {t4_2} ms (Delta: {t4_2 - t4_1} ms)");
        
        long t4_3 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[AUDITORIA] T4.3 - Início extração campo Base64: {t4_3} ms");
        
        var dataArray = jsonResponse.RootElement.GetProperty("data");
        var b64Json = dataArray[0].GetProperty("b64_json").GetString();

        long t4_4 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[AUDITORIA] T4.4 - Extração Base64 concluída: {t4_4} ms (Delta: {t4_4 - t4_3} ms)");
        Console.WriteLine($"[AUDITORIA] Tamanho string Base64: {b64Json?.Length ?? 0} caracteres");

        if (string.IsNullOrEmpty(b64Json))
        {
            return Results.BadRequest(new { error = "A API não retornou dados de imagem válidos" });
        }

        long t4_5 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[AUDITORIA] T4.5 - Início Convert.FromBase64String: {t4_5} ms");

        var imageBytesResult = Convert.FromBase64String(b64Json);
        
        long t4_6 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[AUDITORIA] T4.6 - Convert.FromBase64String concluído: {t4_6} ms (Delta: {t4_6 - t4_5} ms)");
        Console.WriteLine($"[AUDITORIA] Tamanho bytes resultantes: {imageBytesResult.Length} bytes");
        
        long t5 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[BACKEND] T5 - Conversão Base64: {t5} ms (Total: {t5 - t4} ms)");

        var result = Results.File(imageBytesResult, "image/png", "edited_image.png");
        
        long t6 = sw.ElapsedMilliseconds;
        Console.WriteLine($"[BACKEND] T6 - Resposta enviada ao navegador: {t6} ms");
        
        sw.Stop();
        
        // Calcular deltas
        var tempoPreparacao = t2 - t1;
        var tempoOpenAI = t4 - t2;
        var tempoConversao = t5 - t4;
        var tempoResposta = t6 - t5;
        var tempoTotal = t6 - t1;
        
        Console.WriteLine($"[BACKEND] ===== RESUMO DE TEMPOS =====");
        Console.WriteLine($"[BACKEND] Tempo preparação (T2-T1): {tempoPreparacao} ms");
        Console.WriteLine($"[BACKEND] Tempo OpenAI (T4-T2): {tempoOpenAI} ms");
        Console.WriteLine($"[BACKEND] Tempo conversão (T5-T4): {tempoConversao} ms");
        Console.WriteLine($"[BACKEND] Tempo resposta (T6-T5): {tempoResposta} ms");
        Console.WriteLine($"[BACKEND] Tempo total: {tempoTotal} ms");
        Console.WriteLine($"[BACKEND] ===== FIM RESUMO =====");

        return result;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[API] Erro: {ex.Message}");
        Console.WriteLine($"[API] StackTrace: {ex.StackTrace}");
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("EditImage")
.WithOpenApi();

app.Run();
