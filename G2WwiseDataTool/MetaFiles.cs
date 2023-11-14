using System.Text.Json;

namespace G2WwiseDataTool
{
    public class MetaFiles
    {
        public class MetaData
        {
            public string hashValue = "";
            public uint hashOffset = 0;
            public uint hashSize = 0;
            public string hashResourceType = "";
            public uint hashReferenceTableSize = 0;
            public uint hashReferenceTableDummy = 0;
            public uint hashSizeFinal = 0;
            public uint hashSizeInMemory = 0;
            public uint hashSizeInVideoMemory = 0;
            public List<object> hashReferenceData = new List<object>();
        }

        public static class JsonSerializerOptionsProvider
        {
            public static JsonSerializerOptions Options { get; }

            static JsonSerializerOptionsProvider()
            {
                Options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
            }
        }

        public static void GenerateMeta(ref MetaData meta, string outputPath)
        {
            string wbnkMetaData = JsonSerializer.Serialize(new
            {
                hash_value = meta.hashValue,
                hash_offset = meta.hashOffset,
                hash_size = meta.hashSize,
                hash_resource_type = meta.hashResourceType,
                hash_reference_table_size = meta.hashReferenceTableSize,
                hash_reference_table_dummy = meta.hashReferenceTableDummy,
                hash_size_final = meta.hashSizeFinal,
                hash_size_in_memory = meta.hashSizeInMemory,
                hash_size_in_video_memory = meta.hashSizeInVideoMemory,
                hash_reference_data = meta.hashReferenceData
            }, JsonSerializerOptionsProvider.Options);

            System.IO.File.WriteAllText(outputPath, wbnkMetaData);
        }
    }
}
