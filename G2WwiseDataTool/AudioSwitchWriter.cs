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
                    List<uint> offsets = new List<uint>();

                    writer.Write(Encoding.UTF8.GetBytes("BIN1"));
                    writer.Write((uint)0x00010800); // 8 byte alignment, 1 segment
                    writer.Write((uint)0); // to hold big endian size, offset 0x8
                    writer.Write((uint)0); // pad
                    uint sizeStart = (uint)writer.BaseStream.Position;
                    writer.Write((ulong)switchGroup.name.Length | 0x40000000);
                    offsets.Add((uint)writer.BaseStream.Position - 0x10);
                    writer.Write((ulong)0x2C); // switch group name offset
                    uint tableStart = 0x2C + (uint)switchGroup.name.Length + 1;
                    while ((tableStart - 0x4) % 0x8 != 0) { tableStart++; }
                    tableStart += 0x4;
                    offsets.Add((uint)writer.BaseStream.Position - 0x10);
                    writer.Write((ulong)tableStart); // switches table start offset
                    uint tableEnd = tableStart + (uint)switchGroup.switches.Count * 0x10;
                    offsets.Add((uint)writer.BaseStream.Position - 0x10);
                    writer.Write((ulong)tableEnd); // switches table end offset
                    offsets.Add((uint)writer.BaseStream.Position - 0x10);
                    writer.Write((ulong)tableEnd); // switches table end offset
                    writer.Write((uint)switchGroup.name.Length + 1);
                    writer.Write(System.Text.Encoding.UTF8.GetBytes(switchGroup.name));
                    writer.Write((byte)0); // string pad
                    uint lol = (uint)(writer.BaseStream.Position - 0x10);
                    uint lol2 = (tableStart - 0x4);
                    while ((uint)(writer.BaseStream.Position - 0x10) != (tableStart - 0x4)) { writer.Write((byte)0); }
                    writer.Write((uint)switchGroup.switches.Count);
                    uint switchStringOffset = tableEnd + 0x4;
                    foreach (string switchString in switchGroup.switches)
                    {
                        while (switchStringOffset % 0x4 != 0) { switchStringOffset++; }
                        writer.Write((ulong)switchString.Length | 0x40000000);
                        offsets.Add((uint)writer.BaseStream.Position - 0x10);
                        writer.Write((ulong)switchStringOffset);
                        switchStringOffset += (uint)switchString.Length + 1 + 4;
                    }
                    for (int i = 0; i < switchGroup.switches.Count; i++)
                    {
                        writer.Write((uint)switchGroup.switches[i].Length + 1);
                        writer.Write(System.Text.Encoding.UTF8.GetBytes(switchGroup.switches[i]));
                        writer.Write((byte)0); // string pad
                        if (i != switchGroup.switches.Count - 1)
                        {
                            while ((uint)writer.BaseStream.Position % 0x4 != 0) { writer.Write((byte)0); }
                        }
                    }
                    uint sizeEnd = (uint)writer.BaseStream.Position;
                    writer.Write((uint)0x12EBA5ED);
                    writer.Write((uint)offsets.Count * 0x4 + 0x4);
                    writer.Write((uint)offsets.Count);
                    foreach (uint offset in offsets)
                    {
                        writer.Write((uint)offset);
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