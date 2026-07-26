using System;
using System.Windows;
using AIImageChat.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AIImageChat.Views
{
    /// <summary>
    /// Lógica de interação para ImageViewerWindow.xaml
    /// </summary>
    public partial class ImageViewerWindow : Window
    {
        private readonly ImageViewerViewModel _viewModel;

        /// <summary>
        /// Construtor
        /// </summary>
        public ImageViewerWindow(string imagePath, string caption)
        {
            InitializeComponent();

            // Obter ViewModel através de injeção de dependência
            _viewModel = App.ServiceProvider?.GetRequiredService<ImageViewerViewModel>()
                ?? throw new InvalidOperationException("Não foi possível obter o ViewModel");

            DataContext = _viewModel;

            // Inicializar com a imagem
            _viewModel.Initialize(imagePath, caption);

            // Inscrever no evento de fechamento
            _viewModel.RequestClose += (s, e) => Close();
        }

        /// <summary>
        /// Evento de tecla pressionada (ESC para sair do modo tela cheia)
        /// </summary>
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (_viewModel.IsFullscreen)
                {
                    _viewModel.ToggleFullscreenCommand.Execute(null);
                }
                else
                {
                    _viewModel.CloseCommand.Execute(null);
                }
            }
        }
    }
}
