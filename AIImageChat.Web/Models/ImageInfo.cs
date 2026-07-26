using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIImageChat.Models
{
    /// <summary>
    /// Informações sobre uma imagem
    /// </summary>
    public class ImageInfo : INotifyPropertyChanged
    {
        private string _fileName = string.Empty;
        private string _filePath = string.Empty;
        private int _width;
        private int _height;
        private string _format = string.Empty;
        private long _fileSizeBytes;
        private byte[]? _imageData;

        /// <summary>
        /// Nome do arquivo
        /// </summary>
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Caminho completo do arquivo
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Largura da imagem em pixels
        /// </summary>
        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Altura da imagem em pixels
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Formato da imagem (PNG, JPG, etc.)
        /// </summary>
        public string Format
        {
            get => _format;
            set
            {
                if (_format != value)
                {
                    _format = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Tamanho do arquivo em bytes
        /// </summary>
        public long FileSizeBytes
        {
            get => _fileSizeBytes;
            set
            {
                if (_fileSizeBytes != value)
                {
                    _fileSizeBytes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Tamanho do arquivo formatado para exibição
        /// </summary>
        public string FormattedFileSize
        {
            get
            {
                if (_fileSizeBytes < 1024)
                    return $"{_fileSizeBytes} B";
                if (_fileSizeBytes < 1024 * 1024)
                    return $"{_fileSizeBytes / 1024.0:F2} KB";
                return $"{_fileSizeBytes / (1024.0 * 1024.0):F2} MB";
            }
        }

        /// <summary>
        /// Resolução formatada para exibição
        /// </summary>
        public string FormattedResolution => $"{_width} x {_height}";

        /// <summary>
        /// Dados binários da imagem
        /// </summary>
        public byte[]? ImageData
        {
            get => _imageData;
            set
            {
                if (_imageData != value)
                {
                    _imageData = value;
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
