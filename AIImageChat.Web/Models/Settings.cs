using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIImageChat.Models
{
    /// <summary>
    /// Modelo de configurações do aplicativo
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        private string _openAIApiKey = string.Empty;
        private string _openAIModel = "gpt-5.5";
        private int _maxRetryAttempts = 2;
        private bool _autoEvaluateResults = true;
        private bool _saveApiKeyEncrypted = true;
        private string _defaultExportFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        /// <summary>
        /// API Key da OpenAI
        /// </summary>
        public string OpenAIApiKey
        {
            get => _openAIApiKey;
            set
            {
                if (_openAIApiKey != value)
                {
                    _openAIApiKey = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Modelo da OpenAI a ser utilizado
        /// </summary>
        public string OpenAIModel
        {
            get => _openAIModel;
            set
            {
                if (_openAIModel != value)
                {
                    _openAIModel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Número máximo de tentativas automáticas
        /// </summary>
        public int MaxRetryAttempts
        {
            get => _maxRetryAttempts;
            set
            {
                if (_maxRetryAttempts != value)
                {
                    _maxRetryAttempts = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Autoavaliar resultados automaticamente
        /// </summary>
        public bool AutoEvaluateResults
        {
            get => _autoEvaluateResults;
            set
            {
                if (_autoEvaluateResults != value)
                {
                    _autoEvaluateResults = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Salvar API Key localmente de forma criptografada
        /// </summary>
        public bool SaveApiKeyEncrypted
        {
            get => _saveApiKeyEncrypted;
            set
            {
                if (_saveApiKeyEncrypted != value)
                {
                    _saveApiKeyEncrypted = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Pasta padrão de exportação
        /// </summary>
        public string DefaultExportFolder
        {
            get => _defaultExportFolder;
            set
            {
                if (_defaultExportFolder != value)
                {
                    _defaultExportFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Evento de propriedade alterada
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notificar mudança de propriedade
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
