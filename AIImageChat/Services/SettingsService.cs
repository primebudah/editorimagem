using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AIImageChat.Models;

namespace AIImageChat.Services
{
    /// <summary>
    /// Serviço de gerenciamento de configurações
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        private readonly IEncryptionService _encryptionService;
        private readonly IOpenAIService _openAIService;

        /// <summary>
        /// Construtor
        /// </summary>
        public SettingsService(IEncryptionService encryptionService, IOpenAIService openAIService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _openAIService = openAIService ?? throw new ArgumentNullException(nameof(openAIService));
            
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AIImageChat");
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        }

        /// <summary>
        /// Carregar configurações do arquivo
        /// </summary>
        public async Task<Settings> LoadSettingsAsync()
        {
            try
            {
                // Tentar ler API Key do arquivo api_key.txt primeiro
                var apiKeyTxtPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "api_key.txt");

                string apiKey = string.Empty;

                if (File.Exists(apiKeyTxtPath))
                {
                    var lines = File.ReadAllLines(apiKeyTxtPath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("OPENAI_API_KEY="))
                        {
                            var extractedKey = line.Substring("OPENAI_API_KEY=".Length).Trim();
                            if (!string.IsNullOrWhiteSpace(extractedKey) && extractedKey != "sua_chave_aqui")
                            {
                                apiKey = extractedKey;
                                break;
                            }
                        }
                    }
                }

                Settings settings;

                if (!File.Exists(_settingsFilePath))
                {
                    // Retornar configurações padrão
                    settings = new Settings();
                }
                else
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();

                    // Descriptografar API Key se estiver criptografada
                    if (!string.IsNullOrWhiteSpace(settings.OpenAIApiKey) && settings.SaveApiKeyEncrypted)
                    {
                        try
                        {
                            settings.OpenAIApiKey = _encryptionService.Decrypt(settings.OpenAIApiKey);
                        }
                        catch
                        {
                            // Se falhar a descriptografia, manter como está (pode já estar descriptografada)
                        }
                    }
                }

                // Se tiver API Key do arquivo txt, usar ela
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    settings.OpenAIApiKey = apiKey;
                }

                // Atualizar automaticamente para o modelo mais recente se tiver API Key
                if (!string.IsNullOrWhiteSpace(settings.OpenAIApiKey))
                {
                    try
                    {
                        var latestModel = await _openAIService.GetLatestModelAsync(settings.OpenAIApiKey);
                        if (!string.IsNullOrWhiteSpace(latestModel) && latestModel != settings.OpenAIModel)
                        {
                            settings.OpenAIModel = latestModel;
                        }
                    }
                    catch
                    {
                        // Se falhar a atualização do modelo, manter o atual
                    }
                }

                return settings;
            }
            catch (Exception ex)
            {
                // Em caso de erro, retornar configurações padrão
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar configurações: {ex.Message}");
                return new Settings();
            }
        }

        /// <summary>
        /// Carregar configurações do arquivo (síncrono para compatibilidade)
        /// </summary>
        public Settings LoadSettings()
        {
            try
            {
                // Para compatibilidade, usar a versão síncrona simples
                var task = LoadSettingsAsync();
                task.Wait();
                return task.Result;
            }
            catch
            {
                return new Settings();
            }
        }

        /// <summary>
        /// Salvar configurações no arquivo
        /// </summary>
        public void SaveSettings(Settings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar configurações: {ex.Message}", ex);
            }
        }
    }
}
