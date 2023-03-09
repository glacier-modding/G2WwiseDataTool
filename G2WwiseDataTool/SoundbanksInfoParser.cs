using System.Diagnostics;
using System.Xml;

namespace G2WwiseDataTool
{
    public class SoundbanksInfoParser
    {
        public static void ReadSoundbankInfo(string inputPath, string outputPath, bool outputToFolderStructure, bool saveEventAndSoundBankPaths, bool verbose)
        {

            string directoryPath = Path.GetDirectoryName(inputPath);

            var metaFiles = new MetaFiles();

            HashSet<string> logEventPaths = new HashSet<string>();
            HashSet<string> logSoundBankPaths = new HashSet<string>();
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

                        XmlNodeList events = soundBankNode.SelectNodes("IncludedEvents/Event");
                        foreach (XmlNode eventNode in events)
                        {
                            if (soundBankName == "Init") // Ignore Init soundbank because the game already has one
                            {
                                continue;
                            }

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
                                    entry.wemPath = entry.wemPath.Substring(0, entry.wemPath.Length - 4);
                                    entry.isStreamed = true;
                                    entry.wemAssemblyPath = "[assembly:/sound/wwise/originals/" + entry.wemPath + ".wav].pc_wem";
                                    entry.wemNameHash = MD5.ConvertStringtoMD5(entry.wemAssemblyPath);
                                    entry.wemLength = ProcessWems.GetWemLength(directoryPath + "\\" + entry.wemID);
                                    entry.wemData = ProcessWems.GetWemData(directoryPath + "\\" + entry.wemID);
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
                                                entry.prefetchBuffer = ProcessWems.GetWemBuffer(directoryPath + "\\" + entry.wemID, Convert.ToInt32(prefetchSize));
                                                wwev.isPrefetched = true;
                                            }
                                        }
                                    }

                                    if (!found)
                                    {
                                        EventWriter.Event.Entry entry = new EventWriter.Event.Entry();
                                        entry.wemID = excludedMemFile.Attributes["Id"].Value;
                                        entry.wemLength = ProcessWems.GetWemLength(directoryPath + "\\" + entry.wemID);
                                        entry.wemData = ProcessWems.GetWemData(directoryPath + "\\" + entry.wemID);
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
                            logSoundBankPaths.Add(soundBankAssemblyPath.Replace("\\", "/"));

                            if (wwev.isStreamed && wwev.isPrefetched && wwev.isMemory)
                            {
                                logUnsupportedEvents.Add(wwev.eventName + " is unsupported! Please remove the non-streamed audio object from the event or change it to streamed or prefetched instead.");
                            }

                            if (outputToFolderStructure)
                            {
                                string finalOutputPath = Path.Combine(outputPath, wwev.eventObjectPath.TrimStart('\\'));
                                wwev.outputPath = finalOutputPath + ".wwiseevent";

                                EventWriter.WriteWWEV(ref wwev);

                                MetaFiles.GenerateWWEVMetaFile(wwev.eventNameHash, soundBankAssemblyPath, ref depends, finalOutputPath + ".wwiseevent.meta.json");

                                foreach (EventWriter.Event.Entry entry in wwev.entries)
                                {
                                    string finalOutputPathWem = Path.Combine(outputPath, "Originals/", entry.wemPath + ".wav");

                                    if (!Directory.Exists(finalOutputPathWem))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(finalOutputPathWem));
                                    }

                                    if (entry.isStreamed)
                                    {
                                        File.Copy(directoryPath + "\\" + entry.wemID + ".wem", finalOutputPathWem, true);
                                        MetaFiles.GenerateWWEMMetaFile(entry.wemNameHash, finalOutputPathWem + ".meta.json");
                                    }
                                }
                            }
                            else
                            {
                                wwev.outputPath = outputPath + wwev.eventNameHash + ".WWEV";

                                EventWriter.WriteWWEV(ref wwev);

                                MetaFiles.GenerateWWEVMetaFile(wwev.eventNameHash, soundBankAssemblyPath, ref depends, outputPath + wwev.eventNameHash + ".WWEV.meta.json");

                                foreach (EventWriter.Event.Entry entry in wwev.entries)
                                {
                                    if (entry.isStreamed)
                                    {
                                        File.Copy(Path.Combine(directoryPath, entry.wemID + ".wem"), outputPath + entry.wemNameHash + ".WWEM", true);
                                        MetaFiles.GenerateWWEMMetaFile(entry.wemNameHash, outputPath + entry.wemNameHash + ".WWEM.meta.json");
                                    }
                                }
                            }
                        }

                        if (soundBankName == "Init") // Ignore Init soundbank because the game already has one
                        {
                            continue;
                        }

                        if (outputToFolderStructure == true)
                        {
                            string finalOutputPath = Path.Combine(outputPath, soundBankObjectPath.TrimStart('\\'));
                            if (!Directory.Exists(outputPath))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                            }
                            ProcessSoundbanks.ProcessSoundbank(Path.Combine(directoryPath, soundBankPath), finalOutputPath + ".wwisesoundbank");
                            MetaFiles.GenerateWBNKMetaFile(soundBankHash, finalOutputPath + ".wwisesoundbank.meta.json");
                        }
                        else
                        {
                            ProcessSoundbanks.ProcessSoundbank(Path.Combine(directoryPath, soundBankPath), outputPath + soundBankHash + ".WBNK");
                            MetaFiles.GenerateWBNKMetaFile(soundBankHash, outputPath + soundBankHash + ".WBNK.meta.json");
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
