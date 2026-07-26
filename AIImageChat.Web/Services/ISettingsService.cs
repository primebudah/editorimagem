using AIImageChat.Models;

namespace AIImageChat.Services
{
    /// <summary>
    /// Interface do serviço de configurações
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Carregar configurações
        /// </summary>
        /// <returns>Configurações atuais</returns>
        Settings LoadSettings();

        /// <summary>
        /// Salvar configurações
        /// </summary>
        /// <param name="settings">Configurações a salvar</param>
        void SaveSettings(Settings settings);
    }
}
