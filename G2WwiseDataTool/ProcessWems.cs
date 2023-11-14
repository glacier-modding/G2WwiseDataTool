namespace G2WwiseDataTool
{
    public class ProcessWems
    {
        public static byte[] GetWemBuffer(string inputPath, int prefetchSize)
        {
            inputPath += ".wem";

            using (BinaryReader reader = new BinaryReader(File.Open(inputPath, FileMode.Open)))
            {
                byte[] data = reader.ReadBytes(prefetchSize);
                return data;
            }
        }

        public static uint GetWemLength(string inputPath)
        {
            inputPath += ".wem";

            using (BinaryReader reader = new BinaryReader(File.Open(inputPath, FileMode.Open)))
            {
                uint data = (uint)reader.BaseStream.Length;
                return data;
            }
        }

        public static byte[] GetWemData(string inputPath)
        {
            inputPath += ".wem";

            using (BinaryReader reader = new BinaryReader(File.Open(inputPath, FileMode.Open)))
            {
                byte[] data = reader.ReadBytes((int)reader.BaseStream.Length);
                return data;
            }
        }
    }
}
