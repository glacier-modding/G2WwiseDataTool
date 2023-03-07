namespace G2WwiseDataTool
{
    public class ProcessSoundbanks
    {
        public static void ProcessSoundbank(string inputPath, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            }

            File.Copy(inputPath, outputPath, true);

            byte[] fileData = File.ReadAllBytes(outputPath);
            long fileSize = new FileInfo(outputPath).Length;

            using (FileStream fs = File.Open(outputPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }

            using (BinaryWriter writer = new BinaryWriter(File.Open(outputPath, FileMode.Open)))
            {
                byte[] newData = new byte[fileData.Length + 4];

                byte[] fileSizeBytes = BitConverter.GetBytes((UInt32)fileSize);
                Array.Copy(fileSizeBytes, newData, fileSizeBytes.Length);

                Array.Copy(fileData, 0, newData, 4, fileData.Length);

                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((SByte)0x0); // Soundbank count? Hardcoded to 0 for now to match other soundbanks in the game
                writer.Write(newData);
            }

        }
    }
}
