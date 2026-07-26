using System;
using System.IO;
using SkiaSharp;

namespace AIImageChat.Services
{
    /// <summary>
    /// Serviço de exportação de imagens usando SkiaSharp
    /// </summary>
    public class ImageExportService : IImageExportService
    {
        /// <summary>
        /// Exportar imagem para formato específico
        /// </summary>
        public void ExportImage(string sourcePath, string destinationPath, string format, int quality)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Arquivo de origem não encontrado", sourcePath);

            // Carregar imagem
            using var inputStream = File.OpenRead(sourcePath);
            using var originalBitmap = SKBitmap.Decode(inputStream);

            if (originalBitmap == null)
                throw new Exception("Não foi possível carregar a imagem de origem");

            // Determinar formato de codificação
            var encodedFormat = GetEncodedFormat(format);
            
            // Criar diretório de destino se não existir
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Salvar imagem
            using var outputStream = File.OpenWrite(destinationPath);
            using var image = SKImage.FromBitmap(originalBitmap);
            
            if (encodedFormat == SKEncodedImageFormat.Png)
            {
                image.Encode(encodedFormat, 100).SaveTo(outputStream);
            }
            else
            {
                image.Encode(encodedFormat, quality).SaveTo(outputStream);
            }
        }

        /// <summary>
        /// Exportar imagem com DPI específico
        /// </summary>
        public void ExportImageWithDPI(string sourcePath, string destinationPath, string format, int quality, int dpi)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Arquivo de origem não encontrado", sourcePath);

            // Carregar imagem
            using var inputStream = File.OpenRead(sourcePath);
            using var originalBitmap = SKBitmap.Decode(inputStream);

            if (originalBitmap == null)
                throw new Exception("Não foi possível carregar a imagem de origem");

            // Calcular novo tamanho baseado no DPI
            // Assumindo DPI original de 96
            const int originalDPI = 96;
            var scaleFactor = (double)dpi / originalDPI;
            var newWidth = (int)(originalBitmap.Width * scaleFactor);
            var newHeight = (int)(originalBitmap.Height * scaleFactor);

            // Redimensionar imagem
            using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
            
            if (resizedBitmap == null)
                throw new Exception("Não foi possível redimensionar a imagem");

            // Determinar formato de codificação
            var encodedFormat = GetEncodedFormat(format);
            
            // Criar diretório de destino se não existir
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Salvar imagem
            using var outputStream = File.OpenWrite(destinationPath);
            using var image = SKImage.FromBitmap(resizedBitmap);
            
            if (encodedFormat == SKEncodedImageFormat.Png)
            {
                image.Encode(encodedFormat, 100).SaveTo(outputStream);
            }
            else
            {
                image.Encode(encodedFormat, quality).SaveTo(outputStream);
            }
        }

        /// <summary>
        /// Obter formato de codificação do SkiaSharp
        /// </summary>
        private SKEncodedImageFormat GetEncodedFormat(string format)
        {
            return format.ToUpper() switch
            {
                "PNG" => SKEncodedImageFormat.Png,
                "JPG" or "JPEG" => SKEncodedImageFormat.Jpeg,
                "WEBP" => SKEncodedImageFormat.Webp,
                "BMP" => SKEncodedImageFormat.Bmp,
                "GIF" => SKEncodedImageFormat.Gif,
                "TIFF" => SKEncodedImageFormat.Png, // SkiaSharp não suporta TIFF nativamente, usar PNG
                "PDF" => SKEncodedImageFormat.Png, // PDF requer biblioteca adicional
                "SVG" => SKEncodedImageFormat.Png, // SVG requer biblioteca adicional
                _ => SKEncodedImageFormat.Png
            };
        }
    }
}
