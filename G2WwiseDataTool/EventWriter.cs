﻿using System.Text;
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
                public uint prefetchSize = 0;
                public byte[]? prefetchBuffer;
                public uint wemLength = 0;
                public byte[]? wemData;
                public bool isStreamed = false;
                public bool isPrefetched = false;
            }

            public string eventName = "";
            public string eventObjectPath = "";
            public string eventAssemblyPath = "";
            public string eventNameHash = "";
            public float eventMaxAttenuation = -1; // doesn't seem to affect anything in-game, probably used for debugging purposes.
            public string outputPath = "";
            public bool isStreamed = false;
            public bool isPrefetched = false;
            public bool isMemory = false;
            public List<Entry> entries = new List<Entry>();
        }

        public static void WriteWWEV(ref Event wwev)
        {
            int eventNameLength = wwev.eventName.Length;
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
                    writer.Write((sbyte)0x0); // Padding
                    writer.Write(wwev.eventMaxAttenuation);

                    if (wwev.isStreamed)
                    {
                        writer.Write((uint)0x0); // Count 0 / Padding / isStreamed and/or isPrefetched
                    }

                    if (wwev.entries.Count == 0)
                    {
                        writer.Write((uint)0x0);
                    }

                    writer.Write((uint)wwev.entries.Count);

                    uint wemIndex = 1;

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
                            writer.Write((uint)0x0); // prefetchSize
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
                        writer.Write((uint)0x0);
                    }
                }
            }
        }
    }
}
