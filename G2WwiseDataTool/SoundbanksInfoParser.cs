using System.Diagnostics;
using System.Xml;

namespace G2WwiseDataTool
{
    public class SoundbanksInfoParser
    {
        public static void ReadSoundbankInfo(string inputPath, string outputPath, bool outputToFolderStructure, bool saveEventAndSoundBankPaths, bool verbose, IEnumerable<string> filterSoundBanks)
        {

            string directoryPath = Path.GetDirectoryName(inputPath);

            var metaFiles = new MetaFiles();

            HashSet<string> logEventPaths = new HashSet<string>();
            HashSet<string> logSoundBankPaths = new HashSet<string>();
            HashSet<string> logSwitchPaths = new HashSet<string>();
            HashSet<string> logUnsupportedEvents = new HashSet<string>();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(inputPath);

                XmlNodeList soundBanksList = xmlDoc.GetElementsByTagName("SoundBanks");

                foreach (XmlNode soundBanksNode in soundBanksList)
                {
                    foreach (XmlNode soundBankNode in soundBanksNode.ChildNodes)
                    {
                        string soundBankObjectPath = soundBankNode.SelectSingleNode("ObjectPath").InnerText;
                        string soundBankName = soundBankNode.SelectSingleNode("ShortName").InnerText;
                        string soundBankPath = soundBankNode.SelectSingleNode("Path").InnerText;

                        string soundBankAssemblyPath = "[assembly:/sound/wwise/exportedwwisedata" + soundBankObjectPath + ".wwisesoundbank].pc_wwisebank";
                        string soundBankHash = MD5.ConvertStringtoMD5(soundBankAssemblyPath);

                        if (soundBankName == "Init") // Ignore Init soundbank because the game already has one
                        {
                            continue;
                        }

                        if (filterSoundBanks.Any())
                        {
                            if (!filterSoundBanks.Contains(soundBankName))
                            {
                                continue;
                            }
                        }

                        logSoundBankPaths.Add(soundBankAssemblyPath.Replace("\\", "/"));

                        XmlNodeList events = soundBankNode.SelectNodes("IncludedEvents/Event");
                        foreach (XmlNode eventNode in events)
                        {

                            List<string> depends = new List<string>();

                            EventWriter.Event wwev = new EventWriter.Event();
                            wwev.eventName = eventNode.Attributes["Name"].Value;
                            wwev.eventObjectPath = eventNode.Attributes["ObjectPath"].Value;
                            wwev.eventAssemblyPath = "[assembly:/sound/wwise/exportedwwisedata" + wwev.eventObjectPath + ".wwiseevent].pc_wwisebank";
                            wwev.eventNameHash = MD5.ConvertStringtoMD5(wwev.eventAssemblyPath);

                            if (eventNode.SelectSingleNode("ReferencedStreamedFiles") != null)
                            {
                                XmlNodeList refStreamedFiles = eventNode.SelectNodes("ReferencedStreamedFiles/File");
                                foreach (XmlNode refStreamedFile in refStreamedFiles)
                                {
                                    EventWriter.Event.Entry entry = new EventWriter.Event.Entry();
                                    entry.wemID = refStreamedFile.Attributes["Id"].Value;
                                    entry.wemShortName = refStreamedFile.SelectSingleNode("ShortName")?.InnerText;
                                    entry.wemPath = refStreamedFile.SelectSingleNode("Path")?.InnerText;
                                    entry.wemPath = entry.wemPath.Substring(0, entry.wemPath.Length - 4).Replace("\\", "/");
                                    entry.isStreamed = true;
                                    entry.wemAssemblyPath = "[assembly:/sound/wwise/originals/" + entry.wemPath + ".wav].pc_wem";
                                    entry.wemNameHash = MD5.ConvertStringtoMD5(entry.wemAssemblyPath);
                                    entry.wemLength = ProcessWems.GetWemLength(directoryPath + "/" + entry.wemID);
                                    entry.wemData = ProcessWems.GetWemData(directoryPath + "/" + entry.wemID);
                                    wwev.entries.Add(entry);
                                    wwev.isStreamed = true;

                                    depends.Add(entry.wemAssemblyPath);
                                }
                            }

                            if (eventNode.SelectSingleNode("ExcludedMemoryFiles") != null)
                            {
                                XmlNodeList excludedMemFiles = eventNode.SelectNodes("ExcludedMemoryFiles/File");
                                foreach (XmlNode excludedMemFile in excludedMemFiles)
                                {
                                    bool found = false;
                                    string wemID = excludedMemFile.Attributes["Id"].Value;

                                    foreach (EventWriter.Event.Entry entry in wwev.entries)
                                    {
                                        if (entry.wemID == wemID)
                                        {
                                            found = true;

                                            if (excludedMemFile.SelectSingleNode("PrefetchSize") != null)
                                            {
                                                entry.isPrefetched = true;

                                                string prefetchSize = excludedMemFile.SelectSingleNode("PrefetchSize")?.InnerText;
                                                UInt32 prefetchSizeUInt32 = Convert.ToUInt32(prefetchSize);
                                                entry.prefetchSize = Convert.ToUInt32(prefetchSize);
                                                entry.prefetchBuffer = ProcessWems.GetWemBuffer(directoryPath + "/" + entry.wemID, Convert.ToInt32(prefetchSize));
                                                wwev.isPrefetched = true;
                                            }
                                        }
                                    }

                                    if (!found)
                                    {
                                        EventWriter.Event.Entry entry = new EventWriter.Event.Entry();
                                        entry.wemID = excludedMemFile.Attributes["Id"].Value;
                                        entry.wemLength = ProcessWems.GetWemLength(directoryPath + "/" + entry.wemID);
                                        entry.wemData = ProcessWems.GetWemData(directoryPath + "/" + entry.wemID);
                                        wwev.entries.Add(entry);
                                        wwev.isMemory = true;
                                    }
                                }
                            }

                            if (verbose)
                                foreach (EventWriter.Event.Entry entry in wwev.entries)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Event Name: " + wwev.eventName);
                                    Console.WriteLine("Event Object Path: " + wwev.eventObjectPath);
                                    Console.WriteLine("Wem ID: " + entry.wemID);
                                    Console.WriteLine("Wem Assembly Path: " + entry.wemAssemblyPath);
                                    Console.WriteLine("Wem Name Hash: " + entry.wemNameHash);
                                    Console.WriteLine("Prefetch Size: " + entry.prefetchSize.ToString());
                                    Console.WriteLine("Wem Length: " + entry.wemLength.ToString());
                                    Console.WriteLine("isStreamed: " + entry.isStreamed.ToString());
                                    Console.WriteLine("isPrefetched: " + entry.isPrefetched.ToString());
                                    Console.WriteLine();
                                }

                            logEventPaths.Add(wwev.eventAssemblyPath.Replace("\\", "/"));

                            if (wwev.isStreamed && wwev.isPrefetched && wwev.isMemory)
                            {
                                logUnsupportedEvents.Add(wwev.eventName + " is unsupported! Please remove the non-streamed audio object from the event or change it to streamed or prefetched instead.");
                            }

                            MetaFiles.MetaData wwevMetaData = new MetaFiles.MetaData();

                            wwevMetaData.hashValue = wwev.eventNameHash;
                            wwevMetaData.hashOffset = 315858615;
                            wwevMetaData.hashSize = 2147483648;
                            wwevMetaData.hashResourceType = "WWEV";
                            wwevMetaData.hashReferenceTableSize = 13;
                            wwevMetaData.hashReferenceTableDummy = 0;
                            wwevMetaData.hashSizeFinal = 33;
                            wwevMetaData.hashSizeInMemory = 4294967295;
                            wwevMetaData.hashSizeInVideoMemory = 4294967295;
                            wwevMetaData.hashReferenceData.Add(new
                            {
                                hash = soundBankAssemblyPath.ToLower().Replace("\\", "/"),
                                flag = "1F"
                            });
                            foreach (string depend in depends)
                            {
                                wwevMetaData.hashReferenceData.Add(new
                                {
                                    hash = depend.ToLower().Replace("\\", "/"),
                                    flag = "9F"
                                });
                            }

                            MetaFiles.MetaData wwemMetaData = new MetaFiles.MetaData();

                            wwemMetaData.hashValue = "";
                            wwemMetaData.hashOffset = 1127443684;
                            wwemMetaData.hashSize = 2147483648;
                            wwemMetaData.hashResourceType = "WWEM";
                            wwemMetaData.hashReferenceTableSize = 0;
                            wwemMetaData.hashReferenceTableDummy = 0;
                            wwemMetaData.hashSizeFinal = 361339;
                            wwemMetaData.hashSizeInMemory = 4294967295;
                            wwemMetaData.hashSizeInVideoMemory = 4294967295;

                            if (outputToFolderStructure)
                            {
                                string finalOutputPath = Path.Combine(outputPath, wwev.eventObjectPath.TrimStart('\\').Replace("\\", "/"));
                                wwev.outputPath = finalOutputPath + ".wwiseevent";

                                EventWriter.WriteWWEV(ref wwev);

                                MetaFiles.GenerateMeta(ref wwevMetaData, finalOutputPath + ".wwiseevent.meta.json");

                                foreach (EventWriter.Event.Entry entry in wwev.entries)
                                {
                                    string finalOutputPathWem = Path.Combine(outputPath, "Originals/", entry.wemPath + ".wav");

                                    if (!Directory.Exists(finalOutputPathWem))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(finalOutputPathWem));
                                    }

                                    if (entry.isStreamed)
                                    {
                                        File.Copy(directoryPath + "/" + entry.wemID + ".wem", finalOutputPathWem, true);
                                        wwemMetaData.hashValue = entry.wemNameHash;
                                        MetaFiles.GenerateMeta(ref wwemMetaData, finalOutputPath + ".meta.json");
                                    }
                                }
                            }
                            else
                            {
                                wwev.outputPath = outputPath + wwev.eventNameHash + ".WWEV";

                                EventWriter.WriteWWEV(ref wwev);

                                MetaFiles.GenerateMeta(ref wwevMetaData, outputPath + wwev.eventNameHash + ".WWEV.meta.json");

                                foreach (EventWriter.Event.Entry entry in wwev.entries)
                                {
                                    if (entry.isStreamed)
                                    {
                                        File.Copy(Path.Combine(directoryPath, entry.wemID + ".wem"), outputPath + entry.wemNameHash + ".WWEM", true);
                                        wwemMetaData.hashValue = entry.wemNameHash;
                                        MetaFiles.GenerateMeta(ref wwemMetaData, outputPath + entry.wemNameHash + ".WWEM.meta.json");
                                    }
                                }
                            }
                        }

                        XmlNodeList switchGroups = soundBankNode.SelectNodes("SwitchGroups/SwitchGroup");
                        foreach (XmlNode switchGroupNode in switchGroups)
                        {
                            string switchGroupName = switchGroupNode.Attributes["Name"].Value;
                            string switchGroupObjectPath = switchGroupNode.Attributes["ObjectPath"].Value;
                            string switchGroupAssemblyPath = "[assembly:/sound/wwise/exportedwwisedata" + switchGroupObjectPath + ".wwiseswitchgroup].pc_entityblueprint";
                            string switchGroupAssemblyHash = MD5.ConvertStringtoMD5(switchGroupAssemblyPath);

                            string switchGroupTypeAssemblyPath = "[assembly:/sound/wwise/exportedwwisedata" + switchGroupObjectPath + ".wwiseswitchgroup].pc_entitytype";
                            string switchGroupTypeAssemblyHash = MD5.ConvertStringtoMD5(switchGroupTypeAssemblyPath);

                            logSwitchPaths.Add(switchGroupTypeAssemblyPath.Replace("\\", "/"));
                            logSwitchPaths.Add(switchGroupAssemblyPath.Replace("\\", "/"));

                            AudioSwitchWriter.SwitchGroup switchData = new AudioSwitchWriter.SwitchGroup();

                            switchData.name = switchGroupName;

                            foreach (XmlNode switches in switchGroupNode.SelectNodes("Switches/Switch"))
                            {
                                string switchName = switches.Attributes["Name"].Value;

                                switchData.switches.Add(switchName);
                            }

                            MetaFiles.MetaData switchTypeMetaData = new MetaFiles.MetaData();

                            switchTypeMetaData.hashValue = switchGroupTypeAssemblyHash;
                            switchTypeMetaData.hashOffset = 73362489;
                            switchTypeMetaData.hashSize = 2147483648;
                            switchTypeMetaData.hashResourceType = "WSWT";
                            switchTypeMetaData.hashReferenceTableSize = 22;
                            switchTypeMetaData.hashReferenceTableDummy = 0;
                            switchTypeMetaData.hashSizeFinal = 0;
                            switchTypeMetaData.hashSizeInMemory = 4294967295;
                            switchTypeMetaData.hashSizeInVideoMemory = 4294967295;
                            switchTypeMetaData.hashReferenceData.Add(new
                            {
                                hash = "[modules:/zaudioswitchentity.class].pc_entitytype",
                                flag = "1F"
                            });
                            switchTypeMetaData.hashReferenceData.Add(new
                            {
                                hash = switchGroupAssemblyPath.ToLower().Replace("\\", "/"),
                                flag = "1F"
                            });

                            MetaFiles.MetaData switchBlueprintMetaData = new MetaFiles.MetaData();

                            switchBlueprintMetaData.hashValue = switchGroupAssemblyHash;
                            switchBlueprintMetaData.hashOffset = 44115007;
                            switchBlueprintMetaData.hashSize = 2147483843;
                            switchBlueprintMetaData.hashResourceType = "WSWB";
                            switchBlueprintMetaData.hashReferenceTableSize = 13;
                            switchBlueprintMetaData.hashReferenceTableDummy = 0;
                            switchBlueprintMetaData.hashSizeFinal = 265;
                            switchBlueprintMetaData.hashSizeInMemory = 201;
                            switchBlueprintMetaData.hashSizeInVideoMemory = 4294967295;
                            switchBlueprintMetaData.hashReferenceData.Add(new
                            {
                                hash = "[modules:/zaudioswitchentity.class].pc_entityblueprint",
                                flag = "1F"
                            });

                            if (outputToFolderStructure == true)
                            {
                                string finalOutputPath = Path.Combine(outputPath, switchGroupObjectPath.TrimStart('\\').Replace("\\", "/"));
                                if (!Directory.Exists(outputPath))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                                }

                                switchData.outputPath = finalOutputPath + ".wwiseswitchgroup";
                                AudioSwitchWriter.WriteAudioSwitch(ref switchData);

                                MetaFiles.GenerateMeta(ref switchBlueprintMetaData, finalOutputPath + ".wwiseswitchgroup.meta.json");
                            }
                            else
                            {
                                switchData.outputPath = outputPath + switchGroupAssemblyHash + ".WSWB";
                                AudioSwitchWriter.WriteAudioSwitch(ref switchData);

                                MetaFiles.GenerateMeta(ref switchTypeMetaData, outputPath + switchGroupTypeAssemblyHash + ".WSWT.meta.json");
                                MetaFiles.GenerateMeta(ref switchBlueprintMetaData, outputPath + switchGroupAssemblyHash + ".WSWB.meta.json");

                                // Create blank WSWT file
                                File.Create(outputPath + switchGroupTypeAssemblyHash + ".WSWT");
                            }

                        }

                        MetaFiles.MetaData wbnkMetaData = new MetaFiles.MetaData();

                        wbnkMetaData.hashValue = soundBankHash;
                        wbnkMetaData.hashOffset = 2147494565;
                        wbnkMetaData.hashSize = 2147494565;
                        wbnkMetaData.hashResourceType = "WBNK";
                        wbnkMetaData.hashReferenceTableSize = 13;
                        wbnkMetaData.hashReferenceTableDummy = 0;
                        wbnkMetaData.hashSizeFinal = 27726;
                        wbnkMetaData.hashSizeInMemory = 4294967295;
                        wbnkMetaData.hashSizeInVideoMemory = 4294967295;
                        wbnkMetaData.hashReferenceData.Add(new
                        {
                            hash = "[assembly:/sound/wwise/exportedwwisedata/soundbanks/globaldata/global.wwisesoundbank].pc_wwisebank",
                            flag = "1F"
                        });

                        if (outputToFolderStructure == true)
                        {
                            string finalOutputPath = Path.Combine(outputPath, soundBankObjectPath.TrimStart('\\').Replace("\\", "/"));
                            if (!Directory.Exists(outputPath))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                            }

                            ProcessSoundbanks.ProcessSoundbank(Path.Combine(directoryPath, soundBankPath), finalOutputPath + ".wwisesoundbank");
                            MetaFiles.GenerateMeta(ref wbnkMetaData, finalOutputPath + ".wwisesoundbank.meta.json");
                        }
                        else
                        {
                            ProcessSoundbanks.ProcessSoundbank(Path.Combine(directoryPath, soundBankPath), outputPath + soundBankHash + ".WBNK");
                            MetaFiles.GenerateMeta(ref wbnkMetaData, outputPath + soundBankHash + ".WBNK.meta.json");
                        }
                    }
                }

                if (logEventPaths.Count > 0)
                {
                    Console.WriteLine("Event Paths:");
                    foreach (string logEventPath in logEventPaths)
                    {
                        Console.WriteLine(logEventPath);
                    }
                }

                if (logSwitchPaths.Count > 0)
                {
                    Console.WriteLine("Switch Paths:");
                    foreach (string logSwitchPath in logSwitchPaths)
                    {
                        Console.WriteLine(logSwitchPath);
                    }
                }

                if (logSoundBankPaths.Count > 0)
                {
                    Console.WriteLine("SoundBank Paths:");
                    foreach (string logSoundBankPath in logSoundBankPaths)
                    {
                        Console.WriteLine(logSoundBankPath);
                    }
                }

                if (saveEventAndSoundBankPaths)
                {
                    File.WriteAllLines(Path.Combine(outputPath + "events.txt"), logEventPaths);
                    File.WriteAllLines(Path.Combine(outputPath + "switches.txt"), logSwitchPaths);
                    File.WriteAllLines(Path.Combine(outputPath + "soundbanks.txt"), logSoundBankPaths);
                }

                if (logUnsupportedEvents.Count > 0)
                {
                    Console.WriteLine("Unsupported Events:");
                    foreach (string logUnsupportedEvent in logUnsupportedEvents)
                    {
                        Console.WriteLine(logUnsupportedEvent);
                    }
                }
            }

            catch (XmlException ex)
            {
                Trace.TraceError("Error parsing XML document: {0} The file may be corrupted, please regenerate SoundBanks in your Wwise project.", ex.ToString);
            }
        }
    }
}
