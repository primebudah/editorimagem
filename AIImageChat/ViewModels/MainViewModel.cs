using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AIImageChat.Models;
using AIImageChat.Services;
using Microsoft.Win32;

namespace AIImageChat.ViewModels
{
    /// <summary>
    /// ViewModel principal da aplicação
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IOpenAIService _openAIService;
        private readonly ISettingsService _settingsService;
        private string _userInput = string.Empty;
        private bool _isProcessing;
        private string _processingStatus = string.Empty;
        private int _processingProgress;
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Lista de mensagens da conversa
        /// </summary>
        public ObservableCollection<ChatMessage> Messages { get; }

        /// <summary>
        /// Lista de imagens selecionadas
        /// </summary>
        public ObservableCollection<ImageInfo> SelectedImages { get; }

        /// <summary>
        /// Texto digitado pelo usuário
        /// </summary>
        public string UserInput
        {
            get => _userInput;
            set => SetProperty(ref _userInput, value);
        }

        /// <summary>
        /// Indica se está processando
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                {
                    ((RelayCommand)SendCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CancelCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Status do processamento
        /// </summary>
        public string ProcessingStatus
        {
            get => _processingStatus;
            set => SetProperty(ref _processingStatus, value);
        }

        /// <summary>
        /// Progresso do processamento (0-100)
        /// </summary>
        public int ProcessingProgress
        {
            get => _processingProgress;
            set => SetProperty(ref _processingProgress, value);
        }

        /// <summary>
        /// Comando para enviar mensagem
        /// </summary>
        public ICommand SendCommand { get; }

        /// <summary>
        /// Comando para escolher imagem
        /// </summary>
        public ICommand SelectImageCommand { get; }

        /// <summary>
        /// Comando para cancelar processamento
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Comando para abrir configurações
        /// </summary>
        public ICommand OpenSettingsCommand { get; }

        /// <summary>
        /// Comando para remover imagem
        /// </summary>
        public ICommand RemoveImageCommand { get; }

        /// <summary>
        /// Comando para visualizar imagem
        /// </summary>
        public ICommand ViewImageCommand { get; }

        /// <summary>
        /// Construtor
        /// </summary>
        public MainViewModel(IOpenAIService openAIService, ISettingsService settingsService)
        {
            _openAIService = openAIService ?? throw new ArgumentNullException(nameof(openAIService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            Messages = new ObservableCollection<ChatMessage>();
            SelectedImages = new ObservableCollection<ImageInfo>();

            SendCommand = new RelayCommand(SendMessage, CanSendMessage);
            SelectImageCommand = new RelayCommand(SelectImages);
            CancelCommand = new RelayCommand(CancelProcessing, CanCancelProcessing);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            RemoveImageCommand = new RelayCommand<ImageInfo>(RemoveImage);
            ViewImageCommand = new RelayCommand<ChatMessage>(ViewImage);
        }

        /// <summary>
        /// Verificar se pode enviar mensagem
        /// </summary>
        private bool CanSendMessage()
        {
            return !IsProcessing && !string.IsNullOrWhiteSpace(UserInput) && SelectedImages.Count > 0;
        }

        /// <summary>
        /// Verificar se pode cancelar processamento
        /// </summary>
        private bool CanCancelProcessing()
        {
            return IsProcessing;
        }

        /// <summary>
        /// Enviar mensagem para processamento
        /// </summary>
        private async void SendMessage()
        {
            if (SelectedImages.Count == 0)
            {
                MessageBox.Show("Por favor, selecione pelo menos uma imagem.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var settings = _settingsService.LoadSettings();
            if (string.IsNullOrWhiteSpace(settings.OpenAIApiKey))
            {
                MessageBox.Show("Por favor, configure sua API Key da OpenAI nas configurações.", "Configuração Necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
                OpenSettings();
                return;
            }

            IsProcessing = true;
            ProcessingStatus = "A IA está processando...";
            ProcessingProgress = 0;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Adicionar mensagem do usuário
                var userMessage = new ChatMessage
                {
                    Type = MessageType.User,
                    Text = UserInput,
                    Timestamp = DateTime.Now
                };
                Messages.Add(userMessage);

                // Adicionar mensagem de processamento
                var processingMessage = new ChatMessage
                {
                    Type = MessageType.Assistant,
                    Text = "Processando imagem...",
                    IsProcessing = true,
                    Timestamp = DateTime.Now
                };
                Messages.Add(processingMessage);

                ProcessingProgress = 20;

                // Processar cada imagem selecionada
                foreach (var imageInfo in SelectedImages.ToList())
                {
                    ProcessingProgress = 30;
                    ProcessingStatus = $"Processando {imageInfo.FileName}...";

                    // Processar imagem com OpenAI
                    var result = await _openAIService.ProcessImageAsync(
                        imageInfo,
                        UserInput,
                        settings,
                        _cancellationTokenSource.Token,
                        progress => ProcessingProgress = progress
                    );

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    ProcessingProgress = 80;

                    // Remover mensagem de processamento
                    Messages.Remove(processingMessage);

                    // Adicionar resultado
                    var resultMessage = new ChatMessage
                    {
                        Type = MessageType.Assistant,
                        Text = result.WasOptimized ? "Resultado otimizado automaticamente." : "Imagem processada com sucesso.",
                        ImagePath = result.ResultImagePath,
                        Timestamp = DateTime.Now,
                        WasOptimized = result.WasOptimized
                    };
                    Messages.Add(resultMessage);

                    ProcessingProgress = 100;
                }

                // Limpar seleção
                SelectedImages.Clear();
                UserInput = string.Empty;
            }
            catch (OperationCanceledException)
            {
                // Cancelado pelo usuário
                var cancelMessage = Messages.FirstOrDefault(m => m.IsProcessing);
                if (cancelMessage != null)
                {
                    cancelMessage.IsProcessing = false;
                    cancelMessage.Text = "Processamento cancelado pelo usuário.";
                }
            }
            catch (Exception ex)
            {
                // Erro no processamento
                var errorMessage = Messages.FirstOrDefault(m => m.IsProcessing);
                if (errorMessage != null)
                {
                    errorMessage.IsProcessing = false;
                    errorMessage.Text = $"Erro: {ex.Message}";
                }
                else
                {
                    Messages.Add(new ChatMessage
                    {
                        Type = MessageType.System,
                        Text = $"Erro: {ex.Message}",
                        Timestamp = DateTime.Now
                    });
                }
            }
            finally
            {
                IsProcessing = false;
                ProcessingStatus = string.Empty;
                ProcessingProgress = 0;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Selecionar imagens
        /// </summary>
        private void SelectImages()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Selecione imagens",
                Filter = "Imagens|*.png;*.jpg;*.jpeg;*.webp;*.bmp;*.gif;*.tiff;*.heic|Todos os arquivos|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    try
                    {
                        var imageInfo = LoadImageInfo(fileName);
                        SelectedImages.Add(imageInfo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao carregar imagem {fileName}: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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

            // Obter dimensões da imagem (simplificado - em produção usar biblioteca de imagem)
            var imageInfo = new ImageInfo
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                Format = fileInfo.Extension.ToUpper().TrimStart('.'),
                FileSizeBytes = fileInfo.Length,
                ImageData = imageData
            };

            // Tentar obter dimensões (usando SkiaSharp se disponível)
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
                // Se falhar, usar valores padrão
                imageInfo.Width = 0;
                imageInfo.Height = 0;
            }

            return imageInfo;
        }

        /// <summary>
        /// Cancelar processamento
        /// </summary>
        private void CancelProcessing()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Abrir configurações
        /// </summary>
        private void OpenSettings()
        {
            var settingsWindow = new Views.SettingsWindow();
            settingsWindow.ShowDialog();
        }

        /// <summary>
        /// Remover imagem da seleção
        /// </summary>
        private void RemoveImage(ImageInfo? imageInfo)
        {
            if (imageInfo != null)
            {
                SelectedImages.Remove(imageInfo);
            }
        }

        /// <summary>
        /// Visualizar imagem
        /// </summary>
        private void ViewImage(ChatMessage? message)
        {
            if (message != null && !string.IsNullOrEmpty(message.ImagePath) && File.Exists(message.ImagePath))
            {
                var viewerWindow = new Views.ImageViewerWindow(message.ImagePath, message.Text);
                viewerWindow.Show();
            }
        }
    }
}
