// CryptoSoft v3.0 — Outil de chiffrement XOR mono-instance
// ProSoft — Projet EasySave
//
// Usage : CryptoSoft.exe "<chemin_fichier>"
//
// Algorithme : XOR octet par octet avec une clé dérivée du nom de fichier.
//   XOR étant son propre inverse, appliquer l'opération deux fois rétablit
//   le fichier original (chiffrer = déchiffrer).
//
// Mono-instance (v3.0) : un Mutex système nommé empêche l'exécution simultanée
//   de deux instances, même depuis des processus/threads différents.
//   Comportement en cas de conflit : attente jusqu'à 30 secondes, puis abandon (code -99).
//
// Codes de retour :
//   0   : succès
//   1   : argument manquant
//   2   : fichier introuvable
//   3   : fichier vide (rien à chiffrer)
//  -1   : erreur E/S ou exception imprévue
//  -99  : instance déjà en cours (timeout Mutex)

using System.Security.Cryptography;
using System.Text;

namespace CryptoSoft
{
    internal static class Program
    {
        // ─── Mutex système — garantit le mono-instance (v3.0) ───────────
        private const string MutexName = @"Global\CryptoSoft_Instance";

        // ─── Clé XOR ────────────────────────────────────────────────────
        // Dérivée du nom de fichier (SHA-256 → 32 octets de clé roulante).
        // Ce schéma garantit que deux fichiers différents produisent des
        // chiffrements distincts, tout en restant déterministe et réversible.
        private const int KeyLength = 32;

        // ────────────────────────────────────────────────────────────────

        static int Main(string[] args)
        {
            // ── 1. Vérification des arguments ────────────────────────────
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.Error.WriteLine("[CryptoSoft] ERROR: No file path provided.");
                Console.Error.WriteLine("Usage: CryptoSoft.exe \"<file_path>\"");
                return 1;
            }

            string filePath = args[0].Trim('"');

            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"[CryptoSoft] ERROR: File not found: {filePath}");
                return 2;
            }

            // ── 2. Mutex mono-instance (v3.0) ────────────────────────────
            using var mutex = new Mutex(initiallyOwned: false, MutexName);
            bool acquired = false;

            try
            {
                // Attendre jusqu'à 30 secondes qu'une éventuelle instance précédente termine
                acquired = mutex.WaitOne(TimeSpan.FromSeconds(30));
            }
            catch (AbandonedMutexException)
            {
                // Le processus précédent a planté en tenant le mutex — on le récupère
                acquired = true;
            }

            if (!acquired)
            {
                Console.Error.WriteLine("[CryptoSoft] ERROR: Another instance is already running (timeout after 30s).");
                return -99;
            }

            try
            {
                return EncryptFile(filePath);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        // ────────────────────────────────────────────────────────────────
        // Chiffrement XOR avec clé dérivée du nom de fichier
        // ────────────────────────────────────────────────────────────────

        private static int EncryptFile(string filePath)
        {
            try
            {
                byte[] data = File.ReadAllBytes(filePath);

                if (data.Length == 0)
                {
                    Console.Error.WriteLine($"[CryptoSoft] WARNING: File is empty, nothing to encrypt: {filePath}");
                    return 3;
                }

                // Dériver la clé à partir du nom de fichier (SHA-256)
                byte[] key = DeriveKey(Path.GetFileName(filePath));

                // Chiffrement XOR roulant
                for (int i = 0; i < data.Length; i++)
                    data[i] ^= key[i % KeyLength];

                // Écriture atomique : écriture dans un fichier temporaire puis remplacement
                string tempPath = filePath + ".cryptotmp";
                File.WriteAllBytes(tempPath, data);
                File.Move(tempPath, filePath, overwrite: true);

                Console.WriteLine($"[CryptoSoft] OK: {filePath} ({data.Length} bytes)");
                return 0;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"[CryptoSoft] ERROR: Access denied — {ex.Message}");
                return -1;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"[CryptoSoft] ERROR: I/O error — {ex.Message}");
                return -1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CryptoSoft] ERROR: Unexpected — {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Dérive une clé XOR de <see cref="KeyLength"/> octets à partir du nom de fichier.
        /// Utilise SHA-256 sur le nom en UTF-8. Le résultat est déterministe et stable.
        /// </summary>
        private static byte[] DeriveKey(string fileName)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(fileName.ToLowerInvariant());
            return SHA256.HashData(nameBytes); // 32 octets = KeyLength
        }
    }
}
