using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine.Text;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;

namespace G2WwiseDataTool
{
    public class MetaFiles
    {
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

        public static void GenerateWBNKMetaFile(string soundBankHash, string outputPath)
        {
            string hashValue = soundBankHash;
            UInt32 hashOffset = 2147494565;
            UInt32 hashSize = 2147494565;
            string hashResourceType = "WBNK";
            UInt32 hashReferenceTableSize = 13;
            UInt32 hashReferenceTableDummy = 0;
            UInt32 hashSizeFinal = 27726;
            UInt32 hashSizeInMemory = 4294967295;
            UInt32 hashSizeInVideoMemory = 4294967295;
            var hashReferenceData = new List<object>();
            hashReferenceData.Add(new
            {
                hash = "[assembly:/sound/wwise/exportedwwisedata/soundbanks/globaldata/global.wwisesoundbank].pc_wwisebank",
                flag = "1F"
            });

            string wbnkMetaData = JsonSerializer.Serialize(new
            {
                hash_value = hashValue,
                hash_offset = hashOffset,
                hash_size = hashSize,
                hash_resource_type = hashResourceType,
                hash_reference_table_size = hashReferenceTableSize,
                hash_reference_table_dummy = hashReferenceTableDummy,
                hash_size_final = hashSizeFinal,
                hash_size_in_memory = hashSizeInMemory,
                hash_size_in_video_memory = hashSizeInVideoMemory,
                hash_reference_data = hashReferenceData
            }, JsonSerializerOptionsProvider.Options);

            System.IO.File.WriteAllText(outputPath, wbnkMetaData);
        }

        public static void GenerateWWEVMetaFile(string eventNameHash, string soundBankAssemblyPath, ref List<string> depends, string outputPath)
        {
            string hashValue = eventNameHash;
            UInt32 hashOffset = 315858615;
            UInt32 hashSize = 2147483648;
            string hashResourceType = "WWEV";
            UInt32 hashReferenceTableSize = 13;
            UInt32 hashReferenceTableDummy = 0;
            UInt32 hashSizeFinal = 33;
            UInt32 hashSizeInMemory = 4294967295;
            UInt32 hashSizeInVideoMemory = 4294967295;
            var hashReferenceData = new List<object>();
            hashReferenceData.Add(new
            {
                hash = soundBankAssemblyPath.ToLower().Replace("\\", "/"),
                flag = "1F"
            });
            foreach (string depend in depends)
            {
                hashReferenceData.Add(new
                {
                    hash = depend.ToLower().Replace("\\", "/"),
                    flag = "9F"
                });
            }
            string wwevMetaData = JsonSerializer.Serialize(new
            {
                hash_value = hashValue,
                hash_offset = hashOffset,
                hash_size = hashSize,
                hash_resource_type = hashResourceType,
                hash_reference_table_size = hashReferenceTableSize,
                hash_reference_table_dummy = hashReferenceTableDummy,
                hash_size_final = hashSizeFinal,
                hash_size_in_memory = hashSizeInMemory,
                hash_size_in_video_memory = hashSizeInVideoMemory,
                hash_reference_data = hashReferenceData
            }, JsonSerializerOptionsProvider.Options);

            System.IO.File.WriteAllText(outputPath, wwevMetaData);
        }

        public static void GenerateWWEMMetaFile(string wemNameHash, string outputPath)
        {
            string hashValue = wemNameHash;
            UInt32 hashOffset = 1127443684;
            UInt32 hashSize = 2147483648;
            string hashResourceType = "WWEM";
            UInt32 hashReferenceTableSize = 0;
            UInt32 hashReferenceTableDummy = 0;
            UInt32 hashSizeFinal = 361339;
            UInt32 hashSizeInMemory = 4294967295;
            UInt32 hashSizeInVideoMemory = 4294967295;
            var hashReferenceData = new List<object>();

            string wwemMetaData = JsonSerializer.Serialize(new
            {
                hash_value = hashValue,
                hash_offset = hashOffset,
                hash_size = hashSize,
                hash_resource_type = hashResourceType,
                hash_reference_table_size = hashReferenceTableSize,
                hash_reference_table_dummy = hashReferenceTableDummy,
                hash_size_final = hashSizeFinal,
                hash_size_in_memory = hashSizeInMemory,
                hash_size_in_video_memory = hashSizeInVideoMemory,
                hash_reference_data = hashReferenceData
            }, JsonSerializerOptionsProvider.Options);

            System.IO.File.WriteAllText(outputPath, wwemMetaData);
        }

        public static void ConvertToMeta(string rpkgPath, string inputPath)
        {
            var processStartInfo = new ProcessStartInfo(rpkgPath, " -json_to_hash_meta " + "\"" + inputPath + "\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            process.WaitForExit();
        }
    }
}
