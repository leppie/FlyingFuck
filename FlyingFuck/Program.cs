using System;
using System.Collections.Generic;
using System.IO;

namespace FlyingFuck
{
    class Program
    {
        static Dictionary<string, ushort> values = new Dictionary<string, ushort>
        {
            {"FlamingoErythree.spb", 750},
            {"Fl_Cochrane.spb",750},
            {"Fl_ElCalafate.spb",750},
            {"Fl_OHiggins.spb",750},
            {"Gs_LimniVoulkaria.spb",800},
            {"Gs_SkadarLake.spb",750},
            {"SG_Allesund.spb",700},
            {"SG_EnglishBay.spb",800},
            {"SG_LakeIsabella.spb",700},
            {"SG_LakeTahoe.spb",750},
            {"SG_McNeilCove.spb",750},
            {"SG_PuntaArenas.spb",750},
            {"SG_Rijeka.spb",750},
            {"SG_Syros.spb",750},
            {"SG_Tivat.spb",700},
            {"SG_Ushuaia.spb",800},
        };

        static Dictionary<string, long> indices = new Dictionary<string, long>
        {
            {"FlamingoErythree.spb", 1607},
            {"Fl_Cochrane.spb",1602},
            {"Fl_ElCalafate.spb",1620},
            {"Fl_OHiggins.spb",1610},
            {"Gs_LimniVoulkaria.spb",1596},
            {"Gs_SkadarLake.spb",1592},
            {"SG_Allesund.spb",1587},
            {"SG_EnglishBay.spb",1592},
            {"SG_LakeIsabella.spb",1594},
            {"SG_LakeTahoe.spb",1611},
            {"SG_McNeilCove.spb",1592},
            {"SG_PuntaArenas.spb",1593},
            {"SG_Rijeka.spb",1588},
            {"SG_Syros.spb",1587},
            {"SG_Tivat.spb",1587},
            {"SG_Ushuaia.spb",1617},
        };

        static void Main(string[] args)
        {
            var dir = Environment.CurrentDirectory;

            if (!dir.Contains("OneStore"))
            {
                Console.Error.WriteLine("Run from MSFS directory");
                return;
            }

            ushort newCount = args.Length > 0 ? Convert.ToUInt16(args) : (ushort)20;

            foreach (var spb in Directory.EnumerateFiles(dir, "*.spb", SearchOption.AllDirectories))
            {
                var filename = Path.GetFileName(spb);
                if (values.TryGetValue(filename, out var value))
                {
                    var orig = spb + ".orig";
                    if (!File.Exists(orig))
                    {
                        File.Copy(spb, orig, false);
                    }
                    else
                    {
                        File.Copy(orig, spb, true);
                    }

                    // this file has some random logic, so just replace with another...
                    if (filename == "SG_Allesund.spb")
                    {
                        var swap = Path.Combine(Path.GetDirectoryName(spb), "../SG_Tivat/SG_Tivat.spb");
                        var swaporig = swap + ".orig";
                        if (File.Exists(swaporig))
                        {
                            swap = swaporig;
                        }
                        File.Copy(swap, spb, true);
                    }

                    var index = FindIndex(spb, value);
                    if (index == -1)
                    {
                        Console.Error.WriteLine(filename + ": Could not find value to patch, ignoring");
                    }
                    else
                    {
                        if (indices.TryGetValue(filename, out var knownIndex) && knownIndex == index)
                        {
                            PatchSpb(spb, index);
                        }
                        else
                        {
                            Console.Error.WriteLine(filename + $": Found index ({index}) does not match known index ({knownIndex}), ignoring");
                        }
                    }
                }
            }

            Console.WriteLine("Done. Enjoy your FPS!");

            long FindIndex(string spb, ushort current)
            {
                var bb = BitConverter.GetBytes(current);
                using (var file = File.OpenRead(spb))
                {
                    int b = 0;
                    while ((b = file.ReadByte()) != -1)
                    {
                        if (b == bb[0])
                        {
                            b = file.ReadByte();
                            if (b == bb[1])
                            {
                                return file.Position - 2;
                            }
                        }
                    }
                }

                return -1;
            }

            void PatchSpb(string spb, long index)
            {
                Console.WriteLine(Path.GetFileName(spb) + ": Patching to " + newCount + " at index: " + index);

                using (var file = File.OpenWrite(spb))
                {
                    file.Position = index;
                    var bb = BitConverter.GetBytes(newCount);
                    file.WriteByte(bb[0]);
                    file.WriteByte(bb[1]);
                }
            }
        }
    }
}
