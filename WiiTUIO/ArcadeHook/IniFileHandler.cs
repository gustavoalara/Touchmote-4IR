using System;
using System.IO;

namespace WiiTUIO.ArcadeHook
{
    internal class IniFileHandler
    {
        private static string DirectoryPath => ".\\ini";
        private static string GetIniFileName(string name) => $"{name}.ini";

        private static string GetFilePath(string name) => Path.Combine(DirectoryPath, GetIniFileName(name));

        static IniFileHandler()
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        private static void CreateIniFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                string[] lines = {
                    "[General]",
                    "MameStart=",
                    "MameStop=",
                    "StateChange=",
                    "OnRotate=",
                    "OnPause=",
                    "[Outputs]"
                };
                File.WriteAllLines(filePath, lines);
            }
        }

        public static void AddKeyToIniFile(string Name, string Key)
        {
            string filePath = GetFilePath(Name);

            if (File.Exists(filePath))
            {
                File.AppendAllText(filePath, $"{Key}=\n");
                Console.WriteLine($"Key '{Key}' added to INI file.");
            }
            else
            {
                throw new FileNotFoundException($"File '{filePath}' does not exist.");
            }
        }

        public static string ReadFromIniFile(string Name, string Key)
        {
            string filePath = GetFilePath(Name);
            Console.WriteLine(filePath);

            if (!File.Exists(filePath))
            {
                CreateIniFile(filePath);
                Console.WriteLine($"New config file for {Name} created");
            }

            if (File.Exists(filePath))
            {
                var iniLines = File.ReadAllLines(filePath);
                foreach (var line in iniLines)
                {
                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2 && keyValue[0].Trim() == Key)
                        return keyValue[1];
                }
                AddKeyToIniFile(Name, Key);
            }
            Console.WriteLine($"Key '{Key}' not found in INI file.");
            return null;
        }

    }
}
