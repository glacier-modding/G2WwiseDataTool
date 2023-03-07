namespace G2WwiseDataTool
{
    public class ProcessWems
    {
        public static byte[] GetWemBuffer(string inputPath, int prefetchSize)
        {
            inputPath = inputPath + ".wem";

            using (BinaryReader reader = new BinaryReader(File.Open(inputPath, FileMode.Open)))
            {
                byte[] data = reader.ReadBytes(prefetchSize);
                return data;
            }
        }

        public static UInt32 GetWemLength(string inputPath)
        {
            inputPath = inputPath + ".wem";

            using (BinaryReader reader = new BinaryReader(File.Open(inputPath, FileMode.Open)))
            {
                UInt32 data = (UInt32)reader.BaseStream.Length;
                return data;
            }
        }

        public static byte[] GetWemData(string inputPath)
        {
            inputPath = inputPath + ".wem";

            using (BinaryReader reader = new BinaryReader(File.Open(inputPath, FileMode.Open)))
            {
                byte[] data = reader.ReadBytes((int)reader.BaseStream.Length);
                return data;
            }
        }
    }
}
