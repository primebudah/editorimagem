namespace AIImageChat.Services
{
    /// <summary>
    /// Interface do serviço de exportação de imagens
    /// </summary>
    public interface IImageExportService
    {
        /// <summary>
        /// Exportar imagem para formato específico
        /// </summary>
        /// <param name="sourcePath">Caminho da imagem de origem</param>
        /// <param name="destinationPath">Caminho de destino</param>
        /// <param name="format">Formato de exportação</param>
        /// <param name="quality">Qualidade (0-100)</param>
        void ExportImage(string sourcePath, string destinationPath, string format, int quality);

        /// <summary>
        /// Exportar imagem com DPI específico
        /// </summary>
        /// <param name="sourcePath">Caminho da imagem de origem</param>
        /// <param name="destinationPath">Caminho de destino</param>
        /// <param name="format">Formato de exportação</param>
        /// <param name="quality">Qualidade (0-100)</param>
        /// <param name="dpi">DPI desejado</param>
        void ExportImageWithDPI(string sourcePath, string destinationPath, string format, int quality, int dpi);
    }
}
