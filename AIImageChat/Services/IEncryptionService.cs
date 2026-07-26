namespace AIImageChat.Services
{
    /// <summary>
    /// Interface do serviço de criptografia
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Criptografar texto
        /// </summary>
        /// <param name="plainText">Texto plano</param>
        /// <returns>Texto criptografado</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Descriptografar texto
        /// </summary>
        /// <param name="cipherText">Texto criptografado</param>
        /// <returns>Texto plano</returns>
        string Decrypt(string cipherText);
    }
}
