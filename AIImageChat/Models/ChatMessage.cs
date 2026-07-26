using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIImageChat.Models
{
    /// <summary>
    /// Tipo de mensagem na conversa
    /// </summary>
    public enum MessageType
    {
        User,
        Assistant,
        System
    }

    /// <summary>
    /// Modelo de mensagem de chat
    /// </summary>
    public class ChatMessage : INotifyPropertyChanged
    {
        private MessageType _type;
        private string _text = string.Empty;
        private string? _imagePath;
        private DateTime _timestamp;
        private bool _isProcessing;
        private bool _wasOptimized;

        /// <summary>
        /// Tipo da mensagem
        /// </summary>
        public MessageType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Texto da mensagem
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Caminho da imagem (se aplicável)
        /// </summary>
        public string? ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Timestamp da mensagem
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (_timestamp != value)
                {
                    _timestamp = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indica se está processando
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indica se o resultado foi otimizado automaticamente
        /// </summary>
        public bool WasOptimized
        {
            get => _wasOptimized;
            set
            {
                if (_wasOptimized != value)
                {
                    _wasOptimized = value;
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
