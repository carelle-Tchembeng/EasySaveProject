using System;
using System.Diagnostics;
using System.IO;

namespace CryptoSoft
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: CryptoSoft <filepath>");
                return -1;
            }

            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File not found: {filePath}");
                return -2;
            }

            try
            {
                var sw = Stopwatch.StartNew();

                byte[] data = File.ReadAllBytes(filePath);
                byte[] key = System.Text.Encoding.UTF8.GetBytes("EasySaveKey2024!");

                for (int i = 0; i < data.Length; i++)
                    data[i] ^= key[i % key.Length];

                File.WriteAllBytes(filePath, data);

                sw.Stop();

                // Écrit le temps dans la sortie standard
                Console.WriteLine(sw.ElapsedMilliseconds);

                // Retourne TOUJOURS 0 pour succès
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return -3;
            }
        }
    }
}