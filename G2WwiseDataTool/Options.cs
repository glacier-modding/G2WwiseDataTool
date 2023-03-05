using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G2WwiseDataTool
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Path to SoundBanksInfoPath.xml file (Located in Wwise_Project_Root\\GeneratedSoundBanks\\Windows\\).")]
        public string inputPath { get; set; }

        [Option('o', "output", Required = true, HelpText = "Path to output files.")]
        public string outputPath { get; set; }

        [Option('r', "rpkg", Required = false, HelpText = "Path to rpkg-cli for automatic .meta.json to .meta conversion.")]
        public string rpkgPath { get; set; }

        [Option('f', "output-folder-structure", Required = false, HelpText = "Output to a folder structure instead of hashes.")]
        public bool outputToFolderStructure { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool verbose { get; set; }
    }
}
