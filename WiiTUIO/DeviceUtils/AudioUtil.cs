using System;
using System.Collections.Generic;
using System.IO;

namespace WiiTUIO.DeviceUtils
{
    internal class AudioUtil
    {
        private static readonly string[] audioExtensions = { ".wav", ".mp3", ".aac", ".ogg", ".flac" };

        public static bool IsValid(string fileName)
        {
            string baseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);

            foreach (string extension in audioExtensions)
            {
                string filePath = baseFilePath + extension;

                if (File.Exists(filePath))
                {
                    if (extension == ".wav" && IsValidFormat(filePath))
                    {
                        return true;
                    }

                    return ConvertToYamahaADPCM(baseFilePath, extension);
                }
            }

            return false;
        }

        private static bool IsValidFormat(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) using (BinaryReader reader = new BinaryReader(fs))
            { 
                byte[] riff = reader.ReadBytes(4);
                if (System.Text.Encoding.ASCII.GetString(riff) != "RIFF")
                    return false;
                reader.ReadInt32();
                
                byte[] wave = reader.ReadBytes(4);
                if (System.Text.Encoding.ASCII.GetString(wave) != "WAVE")
                    return false;
                
                byte[] fmt = reader.ReadBytes(4);
                if (System.Text.Encoding.ASCII.GetString(fmt) != "fmt ")
                    return false;
                reader.ReadInt32();
                
                short formatCode = reader.ReadInt16();
                if (formatCode != 0x0020)
                    return false;

                short channels = reader.ReadInt16();
                if (channels != 1)
                    return false;

                int sampleRate = reader.ReadInt32();
                if (sampleRate != 3000)
                    return false;
                reader.ReadBytes(6);

                short bitsPerSample = reader.ReadInt16();

                return bitsPerSample == 4;
            }
        }

        private static bool ConvertToYamahaADPCM(string baseFilePath, string extension) // Use ffmpeg to convert to valid audio file
        {
            string filePath = baseFilePath + extension;
            string outputPath = baseFilePath + ".wav";
            if (extension == ".wav")
            {
                File.Move(filePath, filePath + ".bak");
                filePath += ".bak";
            }

            if (!Launcher.Launch(null, "ffmpeg", $"-i \"{filePath}\" -ar 3000 -ac 1 -c:a adpcm_yamaha \"{outputPath}\" -hide_banner", null))
            {
                if (extension == ".wav") File.Move(filePath, baseFilePath + extension);
                return false;
            }

            return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
        }
    }
}
