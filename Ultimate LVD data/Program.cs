using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultimate_LVD_data
{
    public class Program
    {
        static void Main(string[] args)
        {
            StageData.Initialize();

            List<StageId> stages = processDirectory(new DirectoryInfo(args[0])); //root/stage path from dumped directory from ArcCross

            File.WriteAllText(Path.Combine("output", "stages.json"), JsonConvert.SerializeObject(stages, Formatting.Indented), Encoding.UTF8);

            Console.WriteLine("Done");
        }

        static List<StageId> processDirectory(DirectoryInfo d, int type = 0)
        {
            List<string> stageList = new List<string>();
            Dictionary<string, List<Stage>> stages = new Dictionary<string, List<Stage>>();
            List<Stage> allStages = new List<Stage>(); //List for calculator file
            List<StageId> stagesId = new List<StageId>();
            string stage = d.Name;

            //FD references to generate +5 characters data file
            List<Collision> fdCollisions = new List<Collision>();
            string fdPath = "";

            foreach (DirectoryInfo stageInfo in d.GetDirectories()) //stage/{stageId}
            {
                foreach (DirectoryInfo typeInfo in stageInfo.GetDirectories()) //stage/{stageId}/{normal/battle}
                {
                    switch (typeInfo.Name)
                    {
                        case "normal":
                            type = 0;
                            break;
                        case "end":
                            type = 1;
                            break;
                        case "battle":
                            type = 2;
                            break;
                        default:
                            type = 0;
                            break;
                    }
                    if (type != 0)
                        continue; //Ignore battle and end lvd due to common stage files

                    try
                    {
                        foreach (FileInfo f in new DirectoryInfo(Path.Combine(typeInfo.FullName, "param")).GetFiles())
                        {
                            if (f.Extension == ".lvd")
                            {
                                Console.Write(f.Name);
                                Smash_Forge.LVD lvd = null;

                                try
                                {
                                    lvd = new Smash_Forge.LVD(f.FullName);
                                }
                                catch (Exception e)
                                {
                                    lvd = null;
                                    Console.Write($" - Error, {e.Message}");
                                }

                                if (lvd != null)
                                {
                                    Stage s = new Stage(Path.GetFileNameWithoutExtension(f.Name), f.Name, lvd, type);
                                    if (s.name == "Final Destination (Large)")
                                    {
                                        Smash_Forge.LVD fdlvd = new Smash_Forge.LVD(fdPath);
                                        Stage fd = new Stage(Path.GetFileNameWithoutExtension(fdPath), fdPath, fdlvd, type);
                                        s.collisions = fd.collisions;
                                        s.valid = true;
                                    }
                                    if (s.name.Contains("Stage Builder"))
                                    {
                                        s.valid = true;

                                        if (s.lvd == "level_00")
                                        {
                                            s.lvd = "Small";
                                        }
                                        else if (s.lvd == "level_01")
                                        {
                                            s.lvd = "Normal";
                                        }
                                        else if (s.lvd == "level_02")
                                        {
                                            s.lvd = "Large";
                                        }
                                    }
                                    if (s.valid)
                                    {

                                        string stageGameName = Path.GetFileNameWithoutExtension(f.Name);
                                        stageGameName = stageGameName.Substring(0, stageGameName.Length - 2);
                                        int idInt = 0;
                                        if (!int.TryParse(Path.GetFileNameWithoutExtension(f.Name).Substring(stageGameName.Length, 2), out idInt))
                                        {
                                            stageGameName = Path.GetFileNameWithoutExtension(f.Name);
                                        }
                                        if (!stages.ContainsKey(s.name))
                                            stages.Add(s.name, new List<Stage>());

                                        stages[s.name].Add(s);
                                        allStages.Add(s);

                                        stageList.Add(Path.GetFileNameWithoutExtension(s.name));

                                        if (s.name == "Final Destination")
                                        {
                                            fdPath = f.FullName;
                                        }

                                        if (!stagesId.Contains(new StageId() { gameName = stageGameName, name = StageData.Names[stageGameName], Type = s.Type }))
                                        {
                                            stagesId.Add(new StageId() { gameName = stageGameName, name = StageData.Names[stageGameName], Type = s.Type });
                                        }
                                    }
                                }

                                Console.WriteLine();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"param directory not found {stageInfo.Name}\\{typeInfo.Name}");
                    }
                }
            }

            if (!Directory.Exists("output"))
                Directory.CreateDirectory("output");

            if (!Directory.Exists(Path.Combine("output", "stages")))
                Directory.CreateDirectory(Path.Combine("output", "stages"));

            Stage training = stages["Training Stage"][2];

            stages["Training Stage"] = new List<Stage>() { training };

            foreach (var pair in stages)
            {
                if (!Directory.Exists(Path.Combine("output", "stages", pair.Key)))
                {
                    Directory.CreateDirectory(Path.Combine("output", "stages", pair.Key));
                }
                if (pair.Key == "Final Destination (Large)")
                {
                    foreach (Stage s in pair.Value)
                    {
                        foreach (Collision c in s.collisions)
                        {
                            foreach (var vertex in c.vertex)
                            {
                                vertex[0] *= 1.5f;
                            }
                        }
                    }
                }
                File.WriteAllText(Path.Combine("output", "stages", pair.Key, "data.json"), JsonConvert.SerializeObject(pair.Value, Formatting.Indented));
            }
            //File.WriteAllText(Path.Combine("output", "stagelist.json"), JsonConvert.SerializeObject(stageList, Formatting.Indented));


            //Stages data file for calculator
            allStages.Sort();
            File.WriteAllText(Path.Combine("output", "stagesCalc.json"), JsonConvert.SerializeObject(allStages.Where(s => !string.IsNullOrWhiteSpace(s.stage)).ToList(), Formatting.Indented));

            return stagesId;
        }
    }
}
