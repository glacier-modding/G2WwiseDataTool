using System.Text;

namespace G2WwiseDataTool
{
    public class AudioSwitchWriter
    {
        public class SwitchGroup
        {
            public string name = "";
            public string outputPath = "";
            public List<string> switches = new List<string>();
        }

        public static void WriteAudioSwitch(ref SwitchGroup switchGroup)
        {
            if (!Directory.Exists(switchGroup.outputPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(switchGroup.outputPath));
            }

            using (var stream = File.Open(switchGroup.outputPath, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                {
                    List<UInt32> offsets = new List<UInt32>();

                    writer.Write(Encoding.UTF8.GetBytes("BIN1"));
                    writer.Write((UInt32)0x00010800); // 8 byte alignment, 1 segment
                    writer.Write((UInt32)0); // to hold big endian size, offset 0x8
                    writer.Write((UInt32)0); // pad
                    UInt32 sizeStart = (UInt32)writer.BaseStream.Position;
                    writer.Write((UInt64)switchGroup.name.Length | 0x40000000);
                    offsets.Add((UInt32)writer.BaseStream.Position - 0x10);
                    writer.Write((UInt64)0x2C); // switch group name offset
                    UInt32 tableStart = 0x2C + (UInt32)switchGroup.name.Length + 1;
                    while ((tableStart - 0x4) % 0x8 != 0) { tableStart++; }
                    tableStart += 0x4;
                    offsets.Add((UInt32)writer.BaseStream.Position - 0x10);
                    writer.Write((UInt64)tableStart); // switches table start offset
                    UInt32 tableEnd = tableStart + (UInt32)switchGroup.switches.Count * 0x10;
                    offsets.Add((UInt32)writer.BaseStream.Position - 0x10);
                    writer.Write((UInt64)tableEnd); // switches table end offset
                    offsets.Add((UInt32)writer.BaseStream.Position - 0x10);
                    writer.Write((UInt64)tableEnd); // switches table end offset
                    writer.Write((UInt32)switchGroup.name.Length + 1);
                    writer.Write(System.Text.Encoding.UTF8.GetBytes(switchGroup.name));
                    writer.Write((byte)0); // string pad
                    UInt32 lol = (UInt32)(writer.BaseStream.Position - 0x10);
                    UInt32 lol2 = (tableStart - 0x4);
                    while ((UInt32)(writer.BaseStream.Position - 0x10) != (tableStart - 0x4)) { writer.Write((byte)0); }
                    writer.Write((UInt32)switchGroup.switches.Count);
                    UInt32 switchStringOffset = tableEnd + 0x4;
                    foreach (string switchString in switchGroup.switches)
                    {
                        while (switchStringOffset % 0x4 != 0) { switchStringOffset++; }
                        writer.Write((UInt64)switchString.Length | 0x40000000);
                        offsets.Add((UInt32)writer.BaseStream.Position - 0x10);
                        writer.Write((UInt64)switchStringOffset);
                        switchStringOffset += (UInt32)switchString.Length + 1 + 4;
                    }
                    for (int i = 0; i < switchGroup.switches.Count; i++)
                    {
                        writer.Write((UInt32)switchGroup.switches[i].Length + 1);
                        writer.Write(System.Text.Encoding.UTF8.GetBytes(switchGroup.switches[i]));
                        writer.Write((byte)0); // string pad
                        if (i != switchGroup.switches.Count - 1)
                        {
                            while ((UInt32)writer.BaseStream.Position % 0x4 != 0) { writer.Write((byte)0); }
                        }
                    }
                    UInt32 sizeEnd = (UInt32)writer.BaseStream.Position;
                    writer.Write((UInt32)0x12EBA5ED);
                    writer.Write((UInt32)offsets.Count * 0x4 + 0x4);
                    writer.Write((UInt32)offsets.Count);
                    foreach (UInt32 offset in offsets)
                    {
                        writer.Write((UInt32)offset);
                    }
                    writer.Seek(0x8, SeekOrigin.Begin);
                    byte[] size = BitConverter.GetBytes(sizeEnd - sizeStart);
                    Array.Reverse(size);
                    writer.Write(size);
                }
            }
        }

    }
}