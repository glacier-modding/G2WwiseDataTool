using CommandLine;

namespace G2WwiseDataTool
{
    public class Options
    {
        [Option('i', "input", SetName = "export", Required = false, HelpText = "Path to SoundBanksInfoPath.xml file (Located in Wwise_Project_Root\\GeneratedSoundBanks\\Windows\\).")]
        public string inputPath { get; set; }

        [Option('o', "output", SetName = "export", Required = false, HelpText = "Path to output files (Defaults to current working directory).")]
        public string outputPath { get; set; } = Directory.GetCurrentDirectory();

        [Option('r', "rpkg", SetName = "export", Required = false, HelpText = "Path to rpkg-cli for automatic .meta.json to .meta conversion.")]
        public string rpkgPath { get; set; }

        [Option('f', "output-folder-structure", SetName = "export", Required = false, HelpText = "Output to a folder structure instead of hashes.")]
        public bool outputToFolderStructure { get; set; }

        [Option('s', "save-paths", SetName = "export", Required = false, HelpText = "Save Event and SoundBank paths to a events.txt and soundbanks.txt file in the output path.")]
        public bool saveEventAndSoundBankPaths { get; set; }

        [Option('v', "verbose", SetName = "export", Required = false, HelpText = "Set output to verbose messages.")]
        public bool verbose { get; set; }

        [Option('l', "licenses", SetName = "licenses", Required = false, HelpText = "Prints license information for G2WwiseDataTool and third party libraries that are used.")]
        public bool licenses { get; set; }
    }
}