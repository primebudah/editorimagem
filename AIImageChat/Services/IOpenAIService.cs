using System;
using System.Threading;
using System.Threading.Tasks;
using AIImageChat.Models;

namespace AIImageChat.Services
{
    /// <summary>
    /// Interface do serviço de integração com OpenAI
    /// </summary>
    public interface IOpenAIService
    {
        /// <summary>
        /// Processar imagem com OpenAI
        /// </summary>
        /// <param name="imageInfo">Informações da imagem</param>
        /// <param name="userInstruction">Instrução do usuário</param>
        /// <param name="settings">Configurações do aplicativo</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <param name="progressCallback">Callback de progresso</param>
        /// <returns>Resultado do processamento</returns>
        Task<ImageProcessingResult> ProcessImageAsync(
            ImageInfo imageInfo,
            string userInstruction,
            Settings settings,
            CancellationToken cancellationToken,
            Action<int> progressCallback);

        /// <summary>
        /// Buscar modelos disponíveis da OpenAI e retornar o mais recente
        /// </summary>
        /// <param name="apiKey">API Key da OpenAI</param>
        /// <returns>Nome do modelo mais recente</returns>
        Task<string> GetLatestModelAsync(string apiKey);
    }

    /// <summary>
    /// Resultado do processamento de imagem
    /// </summary>
    public class ImageProcessingResult
    {
        /// <summary>
        /// Caminho da imagem resultante
        /// </summary>
        public string ResultImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o resultado foi otimizado automaticamente
        /// </summary>
        public bool WasOptimized { get; set; }

        /// <summary>
        /// Número de tentativas realizadas
        /// </summary>
        public int AttemptsCount { get; set; }
    }
}
