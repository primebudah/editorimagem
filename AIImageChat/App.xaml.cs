using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AIImageChat.Services;
using AIImageChat.ViewModels;

namespace AIImageChat
{
    /// <summary>
    /// Lógica de interação para App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Provedor de serviços para injeção de dependência
        /// </summary>
        public static ServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        /// Evento de inicialização do aplicativo
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // Configurar injeção de dependência
                var serviceCollection = new ServiceCollection();

                // Registrar serviços (ordem importa para dependências)
                serviceCollection.AddSingleton<IEncryptionService, EncryptionService>();
                serviceCollection.AddSingleton<IOpenAIService, OpenAIService>();
                serviceCollection.AddSingleton<IImageExportService, ImageExportService>();
                serviceCollection.AddSingleton<ISettingsService, SettingsService>();

                // Registrar ViewModels
                serviceCollection.AddTransient<MainViewModel>();
                serviceCollection.AddTransient<SettingsViewModel>();
                serviceCollection.AddTransient<ImageViewerViewModel>();

                ServiceProvider = serviceCollection.BuildServiceProvider();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Erro ao inicializar o aplicativo: {ex.Message}\n\nDetalhes: {ex.StackTrace}";

                // Salvar erro em arquivo PRIMEIRO
                try
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
                    File.WriteAllText(logPath, errorMessage);
                }
                catch { }

                // Tentar mostrar MessageBox
                try
                {
                    MessageBox.Show(errorMessage, "Erro de Inicialização", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch { }

                Shutdown();
            }
        }

        /// <summary>
        /// Evento de encerramento do aplicativo
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            ServiceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
