using System;
using System.Windows;
using AIImageChat.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AIImageChat.Views
{
    /// <summary>
    /// Lógica de interação para SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        /// <summary>
        /// Construtor
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();

            // Obter ViewModel através de injeção de dependência
            _viewModel = App.ServiceProvider?.GetRequiredService<SettingsViewModel>()
                ?? throw new InvalidOperationException("Não foi possível obter o ViewModel");

            DataContext = _viewModel;

            // Carregar configurações existentes
            LoadSettings();

            // Inscrever no evento de fechamento
            _viewModel.RequestClose += (s, e) => Close();
        }

        /// <summary>
        /// Carregar configurações nos controles
        /// </summary>
        private void LoadSettings()
        {
            var settings = _viewModel.Settings;

            // API Key
            ApiKeyPasswordBox.Password = settings.OpenAIApiKey;

            // Modelo
            foreach (var item in ModelComboBox.Items)
            {
                if (item is System.Windows.Controls.ComboBoxItem comboBoxItem && 
                    comboBoxItem.Content?.ToString() == settings.OpenAIModel)
                {
                    ModelComboBox.SelectedItem = item;
                    break;
                }
            }

            // Tentativas
            switch (settings.MaxRetryAttempts)
            {
                case 1:
                    Retry1Radio.IsChecked = true;
                    break;
                case 2:
                    Retry2Radio.IsChecked = true;
                    break;
                case 3:
                    Retry3Radio.IsChecked = true;
                    break;
            }

            // Autoavaliação
            AutoEvaluateCheckBox.IsChecked = settings.AutoEvaluateResults;

            // Salvar criptografado
            SaveEncryptedCheckBox.IsChecked = settings.SaveApiKeyEncrypted;

            // Pasta de exportação
            ExportFolderTextBox.Text = settings.DefaultExportFolder;
        }

        /// <summary>
        /// Salvar configurações dos controles no ViewModel
        /// </summary>
        private void SaveSettingsToViewModel()
        {
            _viewModel.OpenAIApiKey = ApiKeyPasswordBox.Password;

            if (ModelComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                _viewModel.OpenAIModel = selectedItem.Content?.ToString() ?? "gpt-4-vision-preview";
            }

            if (Retry1Radio.IsChecked == true)
                _viewModel.MaxRetryAttempts = 1;
            else if (Retry2Radio.IsChecked == true)
                _viewModel.MaxRetryAttempts = 2;
            else if (Retry3Radio.IsChecked == true)
                _viewModel.MaxRetryAttempts = 3;

            _viewModel.AutoEvaluateResults = AutoEvaluateCheckBox.IsChecked == true;
            _viewModel.SaveApiKeyEncrypted = SaveEncryptedCheckBox.IsChecked == true;
            _viewModel.DefaultExportFolder = ExportFolderTextBox.Text;
        }

        /// <summary>
        /// Clique no botão Salvar
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsToViewModel();
            _viewModel.SaveCommand.Execute(null);
        }

        /// <summary>
        /// Clique no botão Cancelar
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CancelCommand.Execute(null);
        }

        /// <summary>
        /// Clique no botão de pasta
        /// </summary>
        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseFolderCommand.Execute(null);
            ExportFolderTextBox.Text = _viewModel.DefaultExportFolder;
        }
    }
}
