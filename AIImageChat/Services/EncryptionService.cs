using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AIImageChat.Services
{
    /// <summary>
    /// Serviço de criptografia usando AES
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _encryptionKey;
        private readonly byte[] _iv;

        /// <summary>
        /// Construtor
        /// Usa chave fixa baseada em informações da máquina para simplicidade
        /// Em produção, usar DPAPI ou Windows Credential Manager
        /// </summary>
        public EncryptionService()
        {
            // Gerar chave e IV fixos baseados em informações da máquina
            var machineInfo = Environment.MachineName + Environment.UserName;
            using var sha = SHA256.Create();
            _encryptionKey = sha.ComputeHash(Encoding.UTF8.GetBytes(machineInfo + "AIImageChat_Key"));
            _iv = sha.ComputeHash(Encoding.UTF8.GetBytes(machineInfo + "AIImageChat_IV"))[..16];
        }

        /// <summary>
        /// Criptografar texto usando AES
        /// </summary>
        public string Encrypt(string plainText)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText))
                    return string.Empty;

                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using var swEncrypt = new StreamWriter(csEncrypt);
                    swEncrypt.Write(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criptografar dados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Descriptografar texto usando AES
        /// </summary>
        public string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText))
                    return string.Empty;

                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                
                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao descriptografar dados: {ex.Message}", ex);
            }
        }
    }
}
