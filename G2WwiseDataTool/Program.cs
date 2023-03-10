using CommandLine;

namespace G2WwiseDataTool
{
    public class Program
    {
        public static List<string> errorLogs = new List<string>();

        static void Main(string[] args)
        {
            var parser = Parser.Default.ParseArguments<Options>(args);

            parser
                .WithParsed(options =>
                {
                    if (options.inputPath != null)
                    {
                        if (Path.GetFileName(options.inputPath) != "SoundbanksInfo.xml")
                        {
                            Console.WriteLine("Input file specified must be SoundBanksInfo.xml");
                            return;
                        }
                        else
                        {
                            SoundbanksInfoParser.ReadSoundbankInfo(options.inputPath, Path.TrimEndingDirectorySeparator(options.outputPath) + "\\", options.outputToFolderStructure, options.saveEventAndSoundBankPaths, options.verbose);

                            /*
                            // Audio Switch Group Tests

                            // 00E0FF34FC69B603.WSGB
                            AudioSwitchWriter.SwitchGroup switchGroup = new AudioSwitchWriter.SwitchGroup();
                            switchGroup.name = "Gameplay_MenuDifficulty";
                            switchGroup.outputPath = Path.TrimEndingDirectorySeparator(options.outputPath) + "\\00E0FF34FC69B603.WSGB";
                            switchGroup.switches.Add("None");
                            switchGroup.switches.Add("Normal");
                            switchGroup.switches.Add("Pro1");
                            AudioSwitchWriter.WriteAudioSwitch(ref switchGroup);

                            // 00BFC6D2506A6FF7.WSGB
                            AudioSwitchWriter.SwitchGroup switchGroup2 = new AudioSwitchWriter.SwitchGroup();
                            switchGroup2.name = "StateGroup_Gecko_DX_Grey_Processed";
                            switchGroup2.outputPath = Path.TrimEndingDirectorySeparator(options.outputPath) + "\\00BFC6D2506A6FF7.WSGB";
                            switchGroup2.switches.Add("None");
                            switchGroup2.switches.Add("State_Gecko_DX_Grey_Processed");
                            switchGroup2.switches.Add("State_Gecko_DX_Grey_Unprocessed");
                            AudioSwitchWriter.WriteAudioSwitch(ref switchGroup2);

                            // 0051BE5D114AC863.WSGB
                            AudioSwitchWriter.SwitchGroup switchGroup3 = new AudioSwitchWriter.SwitchGroup();
                            switchGroup3.name = "Gameplay_Missionspecific_Mix";
                            switchGroup3.outputPath = Path.TrimEndingDirectorySeparator(options.outputPath) + "\\0051BE5D114AC863.WSGB";
                            switchGroup3.switches.Add("Bull");
                            switchGroup3.switches.Add("Bulldog");
                            switchGroup3.switches.Add("Copperhead");
                            switchGroup3.switches.Add("Cornflower");
                            switchGroup3.switches.Add("Falcon");
                            switchGroup3.switches.Add("Flamingo");
                            switchGroup3.switches.Add("Fox");
                            switchGroup3.switches.Add("Fox_Intro");
                            switchGroup3.switches.Add("Gecko");
                            switchGroup3.switches.Add("Hawk");
                            switchGroup3.switches.Add("Hippo");
                            switchGroup3.switches.Add("Llama");
                            switchGroup3.switches.Add("Magpie");
                            switchGroup3.switches.Add("Mamba");
                            switchGroup3.switches.Add("Mongoose");
                            switchGroup3.switches.Add("None");
                            switchGroup3.switches.Add("Octopus");
                            switchGroup3.switches.Add("Peacock");
                            switchGroup3.switches.Add("PolarBear");
                            switchGroup3.switches.Add("PolarBear_VR");
                            switchGroup3.switches.Add("PolerBear_Arrival");
                            switchGroup3.switches.Add("Python");
                            switchGroup3.switches.Add("Raccoon");
                            switchGroup3.switches.Add("Rat");
                            switchGroup3.switches.Add("Rocky_Dugong");
                            switchGroup3.switches.Add("Safehouse");
                            switchGroup3.switches.Add("Sheep");
                            switchGroup3.switches.Add("Skunk");
                            switchGroup3.switches.Add("SmoothSnake");
                            switchGroup3.switches.Add("SnowCrane");
                            switchGroup3.switches.Add("Spider");
                            switchGroup3.switches.Add("Stingray");
                            switchGroup3.switches.Add("Tiger");
                            switchGroup3.switches.Add("Wolverine");
                            AudioSwitchWriter.WriteAudioSwitch(ref switchGroup3);*/
                        }
                    }

                    if (options.licenses)
                    {
                        Licenses.PrintLicenses();
                    }

                });
        }

    }
}