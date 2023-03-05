using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CommandLine;
using CommandLine.Text;

namespace G2WwiseDataTool
{
    public class Program
    {
        public static List<string> errorLogs = new List<string>();

        static void Main(string[] args)
        {
            var bufferedListener = new BufferedTraceListener();
            Trace.Listeners.Add(bufferedListener);

            var result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed<Options>(o =>
            {
                if (Path.GetFileName(o.inputPath) != "SoundbanksInfo.xml")
                {
                    Console.WriteLine("Input file specified must be SoundBanksInfo.xml");
                    return;
                }
                else
                {
                    SoundbanksInfoParser.ReadSoundbankInfo(o.inputPath, Path.TrimEndingDirectorySeparator(o.outputPath) + "\\", o.outputToFolderStructure, o.rpkgPath, o.verbose);
                }
            });

            bufferedListener.WriteBufferedMessages();
        }
    }
}