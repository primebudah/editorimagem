using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIImageChat.Models;
using Newtonsoft.Json;

namespace AIImageChat.Services
{
    /// <summary>
    /// Serviço de integração com OpenAI
    /// Implementa processamento inteligente com autoavaliação
    /// </summary>
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.openai.com/v1";

        /// <summary>
        /// Construtor
        /// </summary>
        public OpenAIService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        /// <summary>
        /// Buscar modelos disponíveis da OpenAI e retornar o mais recente
        /// </summary>
        public async Task<string> GetLatestModelAsync(string apiKey)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.GetAsync($"{BaseUrl}/models");
                
                if (!response.IsSuccessStatusCode)
                {
                    // Se falhar, retornar o modelo padrão atual
                    return "gpt-5.5";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonConvert.DeserializeObject<ModelsResponse>(responseContent);

                if (modelsResponse?.Data == null)
                {
                    return "gpt-5.5";
                }

                // Filtrar apenas modelos GPT e ordenar por versão
                var gptModels = modelsResponse.Data
                    .Where(m => m.Id.StartsWith("gpt-"))
                    .OrderByDescending(m => m.Id)
                    .ToList();

                if (gptModels.Any())
                {
                    return gptModels.First().Id;
                }

                return "gpt-5.5";
            }
            catch
            {
                // Em caso de erro, retornar o modelo padrão
                return "gpt-5.5";
            }
        }

        /// <summary>
        /// Processar imagem com OpenAI usando fluxo inteligente
        /// </summary>
        public async Task<ImageProcessingResult> ProcessImageAsync(
            ImageInfo imageInfo,
            string userInstruction,
            Settings settings,
            CancellationToken cancellationToken,
            Action<int> progressCallback)
        {
            var result = new ImageProcessingResult();
            string? bestImagePath = null;
            int bestScore = 0;

            // Prompt interno do sistema
            var systemPrompt = @"Você é um especialista mundial em restauração, edição, reconstrução e aprimoramento profissional de imagens.
Seu objetivo é executar exatamente o que o usuário solicitar.
Priorize máxima qualidade.
Priorize aparência profissional.
Priorize fidelidade ao pedido do usuário.
Nunca altere elementos importantes sem necessidade.
Nunca invente detalhes importantes quando não houver evidências suficientes.
Quando houver mais de uma solução possível, escolha a que produzir o melhor resultado visual.
Evite aparência artificial.";

            try
            {
                // Converter imagem para base64
                var base64Image = Convert.ToBase64String(imageInfo.ImageData ?? Array.Empty<byte>());
                var dataUrl = $"data:image/{imageInfo.Format.ToLower()};base64,{base64Image}";

                // Tentar processar com autoavaliação
                for (int attempt = 1; attempt <= settings.MaxRetryAttempts; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    progressCallback(30 + (attempt * 20));

                    // Preparar prompt com contexto de tentativas anteriores
                    var userPrompt = attempt == 1 
                        ? userInstruction 
                        : $"{userInstruction}\n\nEsta é a tentativa {attempt} de {settings.MaxRetryAttempts}.";

                    // Fazer chamada à API
                    var response = await CallOpenAIAsync(
                        settings.OpenAIApiKey,
                        settings.OpenAIModel,
                        systemPrompt,
                        userPrompt,
                        dataUrl,
                        cancellationToken
                    );

                    if (response == null || string.IsNullOrEmpty(response.ImageData))
                    {
                        throw new Exception("Não foi possível gerar a imagem. A API não retornou dados válidos.");
                    }

                    // Salvar imagem temporária
                    var tempPath = Path.Combine(Path.GetTempPath(), $"ai_image_chat_{Guid.NewGuid()}.png");
                    await File.WriteAllBytesAsync(tempPath, Convert.FromBase64String(response.ImageData), cancellationToken);

                    // Se for a primeira tentativa ou não houver autoavaliação, usar este resultado
                    if (attempt == 1 || !settings.AutoEvaluateResults)
                    {
                        bestImagePath = tempPath;
                        result.WasOptimized = false;
                        result.AttemptsCount = attempt;
                        break;
                    }

                    // Autoavaliar resultado
                    progressCallback(50 + (attempt * 10));

                    var evaluation = await EvaluateResultAsync(
                        settings.OpenAIApiKey,
                        settings.OpenAIModel,
                        userInstruction,
                        dataUrl,
                        response.ImageData,
                        cancellationToken
                    );

                    // Calcular pontuação baseada na avaliação
                    var score = CalculateScore(evaluation);

                    if (score > bestScore)
                    {
                        // Deletar imagem anterior se existir
                        if (bestImagePath != null && File.Exists(bestImagePath))
                        {
                            File.Delete(bestImagePath);
                        }

                        bestImagePath = tempPath;
                        bestScore = score;
                        result.WasOptimized = true;
                        result.AttemptsCount = attempt;

                        // Se a avaliação for muito boa, parar
                        if (score >= 8)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // Deletar imagem atual pois não é melhor
                        File.Delete(tempPath);
                    }

                    // Preparar prompt para próxima tentativa com feedback da avaliação
                    userInstruction = $"{userInstruction}\n\nFeedback da avaliação anterior: {evaluation}";
                }

                // Mover melhor resultado para local permanente
                if (bestImagePath != null && File.Exists(bestImagePath))
                {
                    var finalPath = Path.Combine(Path.GetTempPath(), $"ai_image_chat_final_{Guid.NewGuid()}.png");
                    File.Move(bestImagePath, finalPath);
                    result.ResultImagePath = finalPath;
                }
                else
                {
                    throw new Exception("Não foi possível gerar uma imagem válida após todas as tentativas.");
                }

                return result;
            }
            catch (Exception ex)
            {
                // Limpar arquivos temporários em caso de erro
                if (bestImagePath != null && File.Exists(bestImagePath))
                {
                    try { File.Delete(bestImagePath); } catch { }
                }
                throw;
            }
        }

        /// <summary>
        /// Chamar API da OpenAI para gerar/editar imagem
        /// </summary>
        private async Task<OpenAIResponse?> CallOpenAIAsync(
            string apiKey,
            string model,
            string systemPrompt,
            string userPrompt,
            string imageDataUrl,
            CancellationToken cancellationToken)
        {
            try
            {
                // Verificar conexão com internet
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    throw new Exception("Sem conexão com internet. Verifique sua conexão e tente novamente.");
                }

                var requestBody = new
                {
                    model = model,
                    messages = new object[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = new object[] 
                        {
                            new { type = "text", text = userPrompt },
                            new { type = "image_url", image_url = new { url = imageDataUrl } }
                        }}
                    },
                    max_tokens = 1000
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync($"{BaseUrl}/chat/completions", content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    
                    // Tratar erros específicos
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("API Key inválida. Verifique suas configurações.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        throw new Exception("Limite de uso da API atingido. Tente novamente mais tarde.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new Exception($"Requisição inválida: {errorContent}");
                    }
                    else
                    {
                        throw new Exception($"Erro na API da OpenAI: {response.StatusCode} - {errorContent}");
                    }
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);

                // Verificar se a resposta contém dados válidos
                if (openAIResponse == null)
                {
                    throw new Exception("A API retornou uma resposta vazia.");
                }

                return openAIResponse;
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                throw new Exception($"Erro de conexão: {ex.Message}. Verifique sua conexão com internet.", ex);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new Exception("Tempo limite excedido. A API demorou muito para responder.", ex);
            }
        }

        /// <summary>
        /// Avaliar resultado automaticamente
        /// </summary>
        private async Task<string> EvaluateResultAsync(
            string apiKey,
            string model,
            string originalInstruction,
            string originalImageDataUrl,
            string generatedImageDataUrl,
            CancellationToken cancellationToken)
        {
            try
            {
                var evaluationPrompt = $@"Avalie a imagem gerada com base na instrução original do usuário: ""{originalInstruction}""

Analise:
- O pedido do usuário foi realmente atendido?
- Existem artefatos visuais?
- Existem distorções?
- Existem bordas defeituosas?
- Existem áreas estranhas?
- Existem erros perceptíveis?
- A qualidade parece profissional?
- Existe algo que possa ser melhorado?

Forneça uma avaliação detalhada e específica sobre o que precisa ser melhorado.";

                var requestBody = new
                {
                    model = model,
                    messages = new object[]
                    {
                        new { role = "system", content = "Você é um especialista em avaliação de qualidade de imagens processadas por IA." },
                        new { role = "user", content = new object[] 
                        {
                            new { type = "text", text = evaluationPrompt },
                            new { type = "image_url", image_url = new { url = originalImageDataUrl } },
                            new { type = "image_url", image_url = new { url = generatedImageDataUrl } }
                        }}
                    },
                    max_tokens = 500
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync($"{BaseUrl}/chat/completions", content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Se a avaliação falhar, retornar uma mensagem neutra
                    return "Avaliação automática não disponível. Usando resultado atual.";
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);

                return openAIResponse?.Choices?[0]?.Message?.Content ?? "Avaliação não disponível";
            }
            catch
            {
                // Se a avaliação falhar por qualquer motivo, retornar uma mensagem neutra
                return "Avaliação automática não disponível. Usando resultado atual.";
            }
        }

        /// <summary>
        /// Calcular pontuação baseada na avaliação
        /// </summary>
        private int CalculateScore(string evaluation)
        {
            // Análise simples de sentimento para determinar qualidade
            var lowerEvaluation = evaluation.ToLower();
            
            int score = 5; // Pontuação base

            // Pontos positivos
            if (lowerEvaluation.Contains("excelente") || lowerEvaluation.Contains("perfeito") || lowerEvaluation.Contains("profissional"))
                score += 3;
            else if (lowerEvaluation.Contains("bom") || lowerEvaluation.Contains("satisfatório"))
                score += 2;
            else if (lowerEvaluation.Contains("adequado"))
                score += 1;

            // Pontos negativos
            if (lowerEvaluation.Contains("artefato") || lowerEvaluation.Contains("distorção") || lowerEvaluation.Contains("erro"))
                score -= 2;
            if (lowerEvaluation.Contains("ruim") || lowerEvaluation.Contains("precisa melhorar") || lowerEvaluation.Contains("não atendeu"))
                score -= 3;
            if (lowerEvaluation.Contains("grave") || lowerEvaluation.Contains("inaceitável"))
                score -= 4;

            return Math.Max(0, Math.Min(10, score));
        }
    }

    /// <summary>
    /// Resposta da API da OpenAI
    /// </summary>
    internal class OpenAIResponse
    {
        [JsonProperty("choices")]
        public Choice[]? Choices { get; set; }

        [JsonProperty("data")]
        public Data[]? Data { get; set; }

        public string? ImageData => Data?.Length > 0 ? Data[0].B64Json : null;
    }

    internal class Choice
    {
        [JsonProperty("message")]
        public Message? Message { get; set; }
    }

    internal class Message
    {
        [JsonProperty("content")]
        public string? Content { get; set; }
    }

    internal class Data
    {
        [JsonProperty("b64_json")]
        public string? B64Json { get; set; }
    }

    /// <summary>
    /// Resposta da API de modelos
    /// </summary>
    internal class ModelsResponse
    {
        [JsonProperty("data")]
        public Model[]? Data { get; set; }
    }

    /// <summary>
    /// Modelo OpenAI
    /// </summary>
    internal class Model
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
    }
}
