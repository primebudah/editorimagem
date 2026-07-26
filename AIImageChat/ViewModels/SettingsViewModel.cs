using System;
using System.Windows;
using System.Windows.Input;
using AIImageChat.Models;
using AIImageChat.Services;

namespace AIImageChat.ViewModels
{
    /// <summary>
    /// ViewModel da janela de configurações
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IEncryptionService _encryptionService;
        private Settings _settings;

        /// <summary>
        /// Configurações atuais
        /// </summary>
        public Settings Settings
        {
            get => _settings;
            set
            {
                if (SetProperty(ref _settings, value))
                {
                    OnPropertyChanged(nameof(OpenAIApiKey));
                    OnPropertyChanged(nameof(OpenAIModel));
                    OnPropertyChanged(nameof(MaxRetryAttempts));
                    OnPropertyChanged(nameof(AutoEvaluateResults));
                    OnPropertyChanged(nameof(SaveApiKeyEncrypted));
                    OnPropertyChanged(nameof(DefaultExportFolder));
                }
            }
        }

        /// <summary>
        /// API Key da OpenAI
        /// </summary>
        public string OpenAIApiKey
        {
            get => _settings.OpenAIApiKey;
            set => _settings.OpenAIApiKey = value;
        }

        /// <summary>
        /// Modelo da OpenAI
        /// </summary>
        public string OpenAIModel
        {
            get => _settings.OpenAIModel;
            set => _settings.OpenAIModel = value;
        }

        /// <summary>
        /// Número máximo de tentativas
        /// </summary>
        public int MaxRetryAttempts
        {
            get => _settings.MaxRetryAttempts;
            set => _settings.MaxRetryAttempts = value;
        }

        /// <summary>
        /// Autoavaliar resultados
        /// </summary>
        public bool AutoEvaluateResults
        {
            get => _settings.AutoEvaluateResults;
            set => _settings.AutoEvaluateResults = value;
        }

        /// <summary>
        /// Salvar API Key criptografada
        /// </summary>
        public bool SaveApiKeyEncrypted
        {
            get => _settings.SaveApiKeyEncrypted;
            set => _settings.SaveApiKeyEncrypted = value;
        }

        /// <summary>
        /// Pasta de exportação padrão
        /// </summary>
        public string DefaultExportFolder
        {
            get => _settings.DefaultExportFolder;
            set => _settings.DefaultExportFolder = value;
        }

        /// <summary>
        /// Comando para salvar configurações
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// Comando para selecionar pasta de exportação
        /// </summary>
        public ICommand BrowseFolderCommand { get; }

        /// <summary>
        /// Comando para cancelar
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Evento de solicitação de fechamento
        /// </summary>
        public event EventHandler? RequestClose;

        /// <summary>
        /// Construtor
        /// </summary>
        public SettingsViewModel(ISettingsService settingsService, IEncryptionService encryptionService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));

            // Carregar configurações existentes
            _settings = _settingsService.LoadSettings();

            SaveCommand = new RelayCommand(SaveSettings);
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
            CancelCommand = new RelayCommand(Cancel);
        }

        /// <summary>
        /// Salvar configurações
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // Criar uma cópia das configurações para salvar
                var settingsToSave = new Settings
                {
                    OpenAIApiKey = _settings.OpenAIApiKey,
                    OpenAIModel = _settings.OpenAIModel,
                    MaxRetryAttempts = _settings.MaxRetryAttempts,
                    AutoEvaluateResults = _settings.AutoEvaluateResults,
                    SaveApiKeyEncrypted = _settings.SaveApiKeyEncrypted,
                    DefaultExportFolder = _settings.DefaultExportFolder
                };

                // Criptografar API Key se necessário
                if (settingsToSave.SaveApiKeyEncrypted && !string.IsNullOrWhiteSpace(settingsToSave.OpenAIApiKey))
                {
                    settingsToSave.OpenAIApiKey = _encryptionService.Encrypt(settingsToSave.OpenAIApiKey);
                }

                _settingsService.SaveSettings(settingsToSave);

                MessageBox.Show("Configurações salvas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar configurações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Selecionar pasta de exportação
        /// </summary>
        private void BrowseFolder()
        {
            // Usar OpenFileDialog com estilo de pasta como workaround para WPF
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Selecione a pasta padrão de exportação",
                Filter = "Folders|*.none",
                FileName = "Select Folder",
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = _settings.DefaultExportFolder
            };

            if (dialog.ShowDialog() == true)
            {
                var folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    _settings.DefaultExportFolder = folderPath;
                    OnPropertyChanged(nameof(DefaultExportFolder));
                }
            }
        }

        /// <summary>
        /// Cancelar e fechar
        /// </summary>
        private void Cancel()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
