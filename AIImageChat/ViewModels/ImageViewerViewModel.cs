using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AIImageChat.Services;

namespace AIImageChat.ViewModels
{
    /// <summary>
    /// ViewModel do visualizador de imagens
    /// </summary>
    public class ImageViewerViewModel : ViewModelBase
    {
        private readonly IImageExportService _imageExportService;
        private readonly ISettingsService _settingsService;
        private string _imagePath;
        private string _caption;
        private BitmapSource? _imageSource;
        private double _zoom = 1.0;
        private double _panX = 0;
        private double _panY = 0;
        private bool _isFullscreen;
        private bool _showComparison;
        private double _comparisonSliderPosition = 0.5;

        /// <summary>
        /// Caminho da imagem
        /// </summary>
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        /// <summary>
        /// Legenda da imagem
        /// </summary>
        public string Caption
        {
            get => _caption;
            set => SetProperty(ref _caption, value);
        }

        /// <summary>
        /// Fonte da imagem
        /// </summary>
        public BitmapSource? ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        /// <summary>
        /// Nível de zoom
        /// </summary>
        public double Zoom
        {
            get => _zoom;
            set => SetProperty(ref _zoom, value);
        }

        /// <summary>
        /// Posição X do pan
        /// </summary>
        public double PanX
        {
            get => _panX;
            set => SetProperty(ref _panX, value);
        }

        /// <summary>
        /// Posição Y do pan
        /// </summary>
        public double PanY
        {
            get => _panY;
            set => SetProperty(ref _panY, value);
        }

        /// <summary>
        /// Modo tela cheia
        /// </summary>
        public bool IsFullscreen
        {
            get => _isFullscreen;
            set => SetProperty(ref _isFullscreen, value);
        }

        /// <summary>
        /// Mostrar comparação antes/depois
        /// </summary>
        public bool ShowComparison
        {
            get => _showComparison;
            set => SetProperty(ref _showComparison, value);
        }

        /// <summary>
        /// Posição do slider de comparação (0-1)
        /// </summary>
        public double ComparisonSliderPosition
        {
            get => _comparisonSliderPosition;
            set => SetProperty(ref _comparisonSliderPosition, value);
        }

        /// <summary>
        /// Comando para zoom in
        /// </summary>
        public ICommand ZoomInCommand { get; }

        /// <summary>
        /// Comando para zoom out
        /// </summary>
        public ICommand ZoomOutCommand { get; }

        /// <summary>
        /// Comando para resetar zoom
        /// </summary>
        public ICommand ResetZoomCommand { get; }

        /// <summary>
        /// Comando para alternar tela cheia
        /// </summary>
        public ICommand ToggleFullscreenCommand { get; }

        /// <summary>
        /// Comando para salvar como PNG
        /// </summary>
        public ICommand SavePngCommand { get; }

        /// <summary>
        /// Comando para salvar como JPG
        /// </summary>
        public ICommand SaveJpgCommand { get; }

        /// <summary>
        /// Comando para salvar como WEBP
        /// </summary>
        public ICommand SaveWebpCommand { get; }

        /// <summary>
        /// Comando para salvar como TIFF
        /// </summary>
        public ICommand SaveTiffCommand { get; }

        /// <summary>
        /// Comando para salvar como PDF
        /// </summary>
        public ICommand SavePdfCommand { get; }

        /// <summary>
        /// Comando para copiar imagem
        /// </summary>
        public ICommand CopyImageCommand { get; }

        /// <summary>
        /// Comando para salvar em...
        /// </summary>
        public ICommand SaveAsCommand { get; }

        /// <summary>
        /// Comando para abrir pasta
        /// </summary>
        public ICommand OpenFolderCommand { get; }

        /// <summary>
        /// Comando para fechar
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Evento de solicitação de fechamento
        /// </summary>
        public event EventHandler? RequestClose;

        /// <summary>
        /// Construtor
        /// </summary>
        public ImageViewerViewModel(IImageExportService imageExportService, ISettingsService settingsService)
        {
            _imageExportService = imageExportService ?? throw new ArgumentNullException(nameof(imageExportService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
            ResetZoomCommand = new RelayCommand(ResetZoom);
            ToggleFullscreenCommand = new RelayCommand(ToggleFullscreen);
            SavePngCommand = new RelayCommand(() => SaveImage("PNG"));
            SaveJpgCommand = new RelayCommand(() => SaveImage("JPG"));
            SaveWebpCommand = new RelayCommand(() => SaveImage("WEBP"));
            SaveTiffCommand = new RelayCommand(() => SaveImage("TIFF"));
            SavePdfCommand = new RelayCommand(() => SaveImage("PDF"));
            CopyImageCommand = new RelayCommand(CopyImage);
            SaveAsCommand = new RelayCommand(SaveAs);
            OpenFolderCommand = new RelayCommand(OpenFolder);
            CloseCommand = new RelayCommand(Close);
        }

        /// <summary>
        /// Inicializar com imagem
        /// </summary>
        public void Initialize(string imagePath, string caption)
        {
            _imagePath = imagePath;
            _caption = caption;

            if (File.Exists(imagePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
                bitmap.Freeze();
                ImageSource = bitmap;
            }
        }

        /// <summary>
        /// Zoom in
        /// </summary>
        private void ZoomIn()
        {
            Zoom = Math.Min(Zoom * 1.2, 10.0);
        }

        /// <summary>
        /// Zoom out
        /// </summary>
        private void ZoomOut()
        {
            Zoom = Math.Max(Zoom / 1.2, 0.1);
        }

        /// <summary>
        /// Resetar zoom
        /// </summary>
        private void ResetZoom()
        {
            Zoom = 1.0;
            PanX = 0;
            PanY = 0;
        }

        /// <summary>
        /// Alternar tela cheia
        /// </summary>
        private void ToggleFullscreen()
        {
            IsFullscreen = !IsFullscreen;
        }

        /// <summary>
        /// Salvar imagem em formato específico
        /// </summary>
        private void SaveImage(string format)
        {
            try
            {
                var settings = _settingsService.LoadSettings();
                var defaultPath = Path.Combine(settings.DefaultExportFolder, $"image_{DateTime.Now:yyyyMMdd_HHmmss}.{format.ToLower()}");
                
                _imageExportService.ExportImage(_imagePath, defaultPath, format, 100);
                MessageBox.Show($"Imagem salva como {format} com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Salvar como...
        /// </summary>
        private void SaveAs()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Salvar imagem como",
                Filter = "PNG|*.png|JPG|*.jpg|WEBP|*.webp|TIFF|*.tiff|PDF|*.pdf",
                DefaultExt = ".png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var format = Path.GetExtension(saveFileDialog.FileName).ToUpper().TrimStart('.');
                    _imageExportService.ExportImage(_imagePath, saveFileDialog.FileName, format, 100);
                    MessageBox.Show("Imagem salva com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Copiar imagem para área de transferência
        /// </summary>
        private void CopyImage()
        {
            try
            {
                if (File.Exists(_imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(_imagePath);
                    bitmap.EndInit();
                    bitmap.Freeze();

                    Clipboard.SetImage(bitmap);
                    MessageBox.Show("Imagem copiada para a área de transferência!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao copiar imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Abrir pasta da imagem
        /// </summary>
        private void OpenFolder()
        {
            try
            {
                if (File.Exists(_imagePath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{_imagePath}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir pasta: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Fechar visualizador
        /// </summary>
        private void Close()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
