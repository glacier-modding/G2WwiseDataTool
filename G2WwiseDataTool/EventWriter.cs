using System.Text;
using static G2WwiseDataTool.EventWriter.Event;

namespace G2WwiseDataTool
{
    public class EventWriter
    {
        public class Event
        {
            public class Entry
            {
                public string wemID = "";
                public string wemShortName = "";
                public string wemPath = "";
                public string wemAssemblyPath = "";
                public string wemNameHash = "";
                public UInt32 prefetchSize = 0;
                public byte[]? prefetchBuffer;
                public UInt32 wemLength = 0;
                public byte[]? wemData;
                public Boolean isStreamed = false;
                public Boolean isPrefetched = false;
            }

            public string eventName = "";
            public string eventObjectPath = "";
            public string eventAssemblyPath = "";
            public string eventNameHash = "";
            public Single unknownFloat = -1; // Max Attenuation Radius? (the game uses the value in the soundbank instead so changing this does absolutely nothing)
            public string outputPath = "";
            public bool isStreamed = false;
            public bool isPrefetched = false;
            public bool isMemory = false;
            public List<Entry> entries = new List<Entry>();
        }

        public static void WriteWWEV(ref Event wwev)
        {
            Int32 eventNameLength = wwev.eventName.Length;
            byte[] eventNameBytes = Encoding.UTF8.GetBytes(wwev.eventName);

            // Streamed = Do not include wem data in WWEV file
            // Prefetched = Include buffer wem data in WWEV file
            // If Streamed is false, then the full wem data is included in the WWEV file

            if (!Directory.Exists(wwev.outputPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(wwev.outputPath));
            }

            using (var stream = File.Open(wwev.outputPath, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    writer.Write(eventNameLength + 0x1);
                    writer.Write(eventNameBytes);
                    writer.Write((SByte)0x0); // Padding
                    writer.Write(wwev.unknownFloat);

                    if (wwev.isStreamed)
                    {
                        writer.Write((UInt32)0x0); // Count 0 / Padding / isStreamed and/or isPrefetched
                    }

                    if (wwev.entries.Count == 0)
                    {
                        writer.Write((UInt32)0x0);
                    }

                    writer.Write((UInt32)wwev.entries.Count);

                    UInt32 wemIndex = 1;

                    foreach (Entry entry in wwev.entries)
                    {
                        if (entry.isStreamed & entry.isPrefetched)
                        {
                            writer.Write(wemIndex);
                            writer.Write(Convert.ToUInt32(entry.wemID));
                            writer.Write(entry.prefetchSize);
                            writer.Write(entry.prefetchBuffer);
                        }
                        else if (entry.isStreamed)
                        {
                            writer.Write(wemIndex);
                            writer.Write(Convert.ToUInt32(entry.wemID));
                            writer.Write((UInt32)0x0); // prefetchSize
                        }
                        else
                        {
                            writer.Write(Convert.ToUInt32(entry.wemID));
                            writer.Write(entry.wemLength);
                            writer.Write(entry.wemData);
                        }

                        wemIndex++;
                    }

                    if (wwev.isMemory)
                    {
                        writer.Write((UInt32)0x0);
                    }
                }
            }
        }
    }
}
