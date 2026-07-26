using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AIImageChat.Models;
using AIImageChat.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AIImageChat.Views
{
    /// <summary>
    /// Lógica de interação para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        /// <summary>
        /// Construtor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Obter ViewModel através de injeção de dependência
            _viewModel = App.ServiceProvider?.GetRequiredService<MainViewModel>()
                ?? throw new InvalidOperationException("Não foi possível obter o ViewModel");

            DataContext = _viewModel;
        }

        /// <summary>
        /// Evento de drag enter
        /// </summary>
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Evento de drag leave
        /// </summary>
        private void MainWindow_DragLeave(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Processar arquivos arrastados
        /// </summary>
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                
                if (files != null && files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            // Verificar se é uma imagem
                            var extension = Path.GetExtension(file).ToLower();
                            var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".gif", ".tiff", ".heic" };
                            
                            if (imageExtensions.Contains(extension))
                            {
                                var imageInfo = LoadImageInfo(file);
                                _viewModel.SelectedImages.Add(imageInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erro ao carregar imagem {file}: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Carregar informações da imagem
        /// </summary>
        private ImageInfo LoadImageInfo(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            
            using var fileStream = File.OpenRead(filePath);
            var imageData = new byte[fileStream.Length];
            fileStream.Read(imageData, 0, imageData.Length);

            var imageInfo = new ImageInfo
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                Format = fileInfo.Extension.ToUpper().TrimStart('.'),
                FileSizeBytes = fileInfo.Length,
                ImageData = imageData
            };

            // Tentar obter dimensões
            try
            {
                using var bitmap = SkiaSharp.SKBitmap.Decode(imageData);
                if (bitmap != null)
                {
                    imageInfo.Width = bitmap.Width;
                    imageInfo.Height = bitmap.Height;
                }
            }
            catch
            {
                imageInfo.Width = 0;
                imageInfo.Height = 0;
            }

            return imageInfo;
        }

        /// <summary>
        /// Clique na imagem para visualizar
        /// </summary>
        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Image image && image.Source != null)
            {
                // Encontrar a mensagem associada
                if (image.DataContext is ChatMessage message)
                {
                    _viewModel.ViewImageCommand.Execute(message);
                }
            }
        }
    }

    /// <summary>
    /// Seletor de template para mensagens
    /// </summary>
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UserTemplate { get; set; }
        public DataTemplate? AssistantTemplate { get; set; }
        public DataTemplate? SystemTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessage message)
            {
                return message.Type switch
                {
                    MessageType.User => UserTemplate,
                    MessageType.Assistant => AssistantTemplate,
                    MessageType.System => SystemTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }

            return base.SelectTemplate(item, container);
        }
    }

    /// <summary>
    /// Base class for DataTemplateSelector
    /// </summary>
    public class DataTemplateSelector
    {
        public virtual DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
    }

}
