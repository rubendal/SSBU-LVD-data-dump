using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using OpenTK;

namespace Ultimate_LVD_data
{
    public static class StageData
    {
        public static Dictionary<string, string> Names = new Dictionary<string, string>();
        public static Dictionary<string, string> Names3ds = new Dictionary<string, string>();
        public static Dictionary<string, float> CameraY = new Dictionary<string, float>();
        public enum materialTypes : byte
        {
            Basic = 0x00,            //
            Rock = 0x01,             //
            Grass = 0x02,            //Increased traction (1.5)
            Soil = 0x03,             //
            Wood = 0x04,             //
            LightMetal = 0x05,       //"Iron" internally.
            HeavyMetal = 0x06,       //"NibuIron" (Iron2) internally.
            Carpet = 0x07,           //Used for Delfino Plaza roof things
            Alien = 0x08,            //"NumeNume" (squelch sound) internally. Used on Brinstar
            MasterFortress = 0x09,   //"Creature" internally.
            Water = 0x0a,     //"Asase" (shallows) internally. Used for Delfino Plaza shallow water
            Soft = 0x0b,             //Used on Woolly World
            TuruTuru = 0x0c,         //Reduced traction (0.1). Unknown meaning and use
            Snow = 0x0d,             //
            Ice = 0x0e,              //Reduced traction (0.2). Used on P. Stadium 2 ice form
            GameWatch = 0x0f,        //Used on Flat Zone
            Oil = 0x10,              //Reduced traction (0.1). Used for Flat Zone oil spill (presumably; not found in any collisions)
            Cardboard = 0x11,        //"Danbouru" (corrugated cardboard) internally. Used on Paper Mario
            Damage1 = 0x12, //Unknown. From Brawl, and appears to still be hazard-related but is probably not functional
            Damage2 = 0x13,   //See above
            Damage3 = 0x14,   //See above
            Electroplankton = 0x15,  //"ElectroP" internally. Not known to be used anywhere in this game
            Cloud = 0x16,            //Used on Skyworld, Magicant
            Subspace = 0x17,         //"Akuukan" (subspace) internally. Not known to be used anywhere in this game
            Brick = 0x18,            //Used on Skyworld, Gerudo V., Smash Run
            NoEffects = 0x19,         //Unknown. From Brawl
            NES8Bit = 0x1a,          //"Famicom" internally. Not known to be used anywhere in this game
            Grate = 0x1b,            //Used on Delfino and P. Stadium 2
            Sand = 0x1c,             //
            Homerun = 0x1d,          //From Brawl, may not be functional
            Asase_Earth = 0x1e,    //From Brawl, may not be functional
            Hurt = 0x1f,             //Takes hitbox data from stdat. Used for Cave and M. Fortress Danger Zones
            RingMat = 0x20,          //Unknown. Uses bomb SFX?
            Glass = 0x21,
            SlipMelee = 0x22,
            SpiritsPoison = 0x23,
            SpiritsFlame = 0x24,
            SpiritsShock = 0x25,
            SpiritsSleep = 0x26,
            SpiritsFreeze = 0x27,
            SpiritsAdhesion = 0x28,
            Ice_No_Slip = 0x29,
            Cloud_No_Through = 0x2a,
            Mementos = 0x2b
        }

        public static void Initialize()
        {
            string read = File.ReadAllText("list.csv", Encoding.UTF8);
            foreach (string s in read.Split('\n'))
            {
                if (!s.Contains(","))
                {
                    continue;
                }
                string[] line = Util.SplitCSV(s.Trim('\r'));
                string codename = line[0], name = line[1].Replace("\"","").Trim();
                Names.Add(codename, name);

                if(line.Length > 2)
                {
                    CameraY.Add(codename, float.Parse(line[2].Replace("\"", "").Trim()));
                }
            }
        }
    }

    public class Vertex
    {
        public float x, y;

        public Vertex(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vertex(Vector2 v)
        {
            x = (float)v.X;
            y = (float)v.Y;
        }

        public List<float> ToList()
        {
            return new List<float>() { x, y };
        }
    }

    public class ItemSpawn
    {
        public string name { get; set; }
        public string subname { get; set; }
        public List<ItemSection> sections { get; set; }

        public ItemSpawn()
        {

        }
    }

    public class ItemSection
    {
        public List<List<float>> points { get; set; }

        public ItemSection()
        {

        }
    }


    public class Stage : IComparable<Stage>
    {
        public string stage { get; set; }
        public string name { get; set; }
        public string lvd { get; set; }
        [JsonIgnore]
        public int legal = 0;
        public List<Collision> collisions { get; set; }
        public List<Collision> platforms { get; set; }
        public List<float> blast_zones { get; set; }
        public List<float> camera { get; set; }
        public List<float> center { get; set; }
        public List<List<float>> spawns { get; set; }

        public List<List<float>> respawns { get; set; }
        public List<ItemSpawn> items { get; set; }

        [JsonIgnore]
        public bool valid = true;

        [JsonIgnore]
        public int Type = 0;

        public Stage(string name, string filename, Smash_Forge.LVD lvd, int type = 0)
        {
            Type = type;
            string t = name.Substring(0, name.Length - 2);
            string id = name.Substring(name.Length - 2,2);
            int idInt = 0;
            bool hasId = int.TryParse(id, out idInt);
            if (!hasId)
            {
                t = name;
                id = "00";
            }
            if (StageData.Names.Keys.Contains(t))
            {
                this.name = StageData.Names[t];
            }
            else
            {
                this.name = name;
                valid = false;
            }
            stage = this.name;
            if (id != "00" && hasId)
            {
                stage += $" ({idInt})";
            }
            this.lvd = Path.GetFileNameWithoutExtension(filename);

            filename = filename.Replace(".lvd", "");

            collisions = new List<Collision>();
            platforms = new List<Collision>();
            blast_zones = new List<float>();
            camera = new List<float>();
            spawns = new List<List<float>>();
            respawns = new List<List<float>>();
            items = new List<ItemSpawn>();

            if (lvd.blastzones.Count == 0)
            {
                valid = false;
                return;
            }

            float camY = 0;

            if (StageData.CameraY.ContainsKey(t))
            {
                camY = StageData.CameraY[t];
            }

            blast_zones.Add(lvd.blastzones[lvd.blastzones.Count - 1].left);
            blast_zones.Add(lvd.blastzones[lvd.blastzones.Count - 1].right);
            blast_zones.Add(lvd.blastzones[lvd.blastzones.Count - 1].top + camY);
            blast_zones.Add(lvd.blastzones[lvd.blastzones.Count - 1].bottom + camY);

            camera.Add(lvd.cameraBounds[lvd.cameraBounds.Count - 1].left);
            camera.Add(lvd.cameraBounds[lvd.cameraBounds.Count - 1].right);
            camera.Add(lvd.cameraBounds[lvd.cameraBounds.Count - 1].top + camY);
            camera.Add(lvd.cameraBounds[lvd.cameraBounds.Count - 1].bottom + camY);

            for (int i = 0; i < lvd.spawns.Count; i++)
            {
                spawns.Add(new Vertex(lvd.spawns[i].x, lvd.spawns[i].y).ToList());
            }

            for (int i = 0; i < lvd.respawns.Count; i++)
            {
                respawns.Add(new Vertex(lvd.respawns[i].x, lvd.respawns[i].y).ToList());
            }

            for (int i = 0; i < lvd.itemSpawns.Count; i++)
            {
                ItemSpawn s = new ItemSpawn()
                {
                    name = lvd.itemSpawns[i].name,
                    subname = lvd.itemSpawns[i].subname,
                    sections = new List<ItemSection>()
                };
                for (int j = 0; j < lvd.itemSpawns[i].sections.Count; j++)
                {
                    ItemSection sec = new ItemSection()
                    {
                        points = new List<List<float>>()
                    };
                    for (int k = 0; k < lvd.itemSpawns[i].sections[j].points.Count; k++)
                    {
                        sec.points.Add(new Vertex(lvd.itemSpawns[i].sections[j].points[k].X, lvd.itemSpawns[i].sections[j].points[k].Y).ToList());
                    }
                    s.sections.Add(sec);
                }
                items.Add(s);
            }

            for (int i = 0; i < lvd.collisions.Count; i++)
            {
                Collision c;
                List<List<float>> normals = new List<List<float>>();
                List<StageCollisionMat> materials = new List<StageCollisionMat>();
                if (Math.Abs(lvd.collisions[i].startPos[2]) > 5)
                {
                    //Console.WriteLine(this.stage);
                }
                if (lvd.collisions[i].flag4)
                {
                    //Platform
                    c = new Collision(true, lvd.collisions[i].name, lvd.collisions[i].useStartPos, lvd.collisions[i].startPos);

                    float minY = 0, maxY = 0;
                    float minX = 0, maxX = 0;
                    for (int j = 0; j < lvd.collisions[i].verts.Count; j++)
                    {
                        c.addVertex(new Vertex(lvd.collisions[i].verts[j]));
                        if (j == 0)
                        {
                            minY = lvd.collisions[i].verts[j].Y;
                            maxY = lvd.collisions[i].verts[j].Y;
                            minX = lvd.collisions[i].verts[j].X;
                            maxX = lvd.collisions[i].verts[j].X;
                        }
                        else
                        {
                            if (lvd.collisions[i].verts[j].Y < minY)
                            {
                                minY = lvd.collisions[i].verts[j].Y;
                            }
                            if (lvd.collisions[i].verts[j].Y > maxY)
                            {
                                maxY = lvd.collisions[i].verts[j].Y;
                            }
                            if (lvd.collisions[i].verts[j].X < minX)
                            {
                                minX = lvd.collisions[i].verts[j].X;
                            }
                            if (lvd.collisions[i].verts[j].X > maxX)
                            {
                                maxX = lvd.collisions[i].verts[j].X;
                            }
                        }
                    }

                    c.boundingBox.Add(new Vertex(minX, minY).ToList());

                    c.boundingBox.Add(new Vertex(maxX, maxY).ToList());

                    //if (c.boundingBox.Count < 5)
                    //{
                    //    Console.WriteLine(name);
                    //}
                    for (int j = 0; j < lvd.collisions[i].normals.Count; j++)
                    {
                        normals.Add(new Vertex(lvd.collisions[i].normals[j]).ToList());
                    }
                    for (int j = 0; j < lvd.collisions[i].materials.Count; j++)
                    {
                        StageCollisionMat mat = new StageCollisionMat(lvd.collisions[i].materials[j].getPhysics(), lvd.collisions[i].materials[j].getFlag(6), lvd.collisions[i].materials[j].getFlag(7), lvd.collisions[i].materials[j].getFlag(4));
                        mat.passthroughAngle = (int)(Math.Atan2(normals[j][1], normals[j][0]) * 180.0 / Math.PI);
                        if (mat.passthroughAngle < 0)
                        {
                            mat.passthroughAngle += 360;
                        }
                        materials.Add(mat);
                    }
                    c.normals = normals;
                    c.materials = materials;

                    platforms.Add(c);
                }
                else
                {
                    //Collision
                    c = new Collision(false, lvd.collisions[i].name, lvd.collisions[i].useStartPos, lvd.collisions[i].startPos);
                    float minY = 0, maxY = 0;
                    float minX = 0, maxX = 0;
                    for (int j = 0; j < lvd.collisions[i].verts.Count; j++)
                    {
                        c.addVertex(new Vertex(lvd.collisions[i].verts[j]));
                        if (j == 0)
                        {
                            minY = lvd.collisions[i].verts[j].Y;
                            maxY = lvd.collisions[i].verts[j].Y;
                            minX = lvd.collisions[i].verts[j].X;
                            maxX = lvd.collisions[i].verts[j].X;
                        }
                        else
                        {
                            if (lvd.collisions[i].verts[j].Y < minY)
                            {
                                minY = lvd.collisions[i].verts[j].Y;
                            }
                            if (lvd.collisions[i].verts[j].Y > maxY)
                            {
                                maxY = lvd.collisions[i].verts[j].Y;
                            }
                            if (lvd.collisions[i].verts[j].X < minX)
                            {
                                minX = lvd.collisions[i].verts[j].X;
                            }
                            if (lvd.collisions[i].verts[j].X > maxX)
                            {
                                maxX = lvd.collisions[i].verts[j].X;
                            }
                        }
                    }

                    c.boundingBox.Add(new Vertex(minX, minY).ToList());
                    c.boundingBox.Add(new Vertex(maxX, maxY).ToList());

                    for (int j = 0; j < lvd.collisions[i].normals.Count; j++)
                    {
                        normals.Add(new Vertex(lvd.collisions[i].normals[j]).ToList());
                    }
                    for (int j = 0; j < lvd.collisions[i].materials.Count; j++)
                    {
                        StageCollisionMat mat = new StageCollisionMat(lvd.collisions[i].materials[j].getPhysics(), lvd.collisions[i].materials[j].getFlag(6), lvd.collisions[i].materials[j].getFlag(7), lvd.collisions[i].materials[j].getFlag(4));
                        mat.calculateLength(new Vertex(c.vertex[j][0], c.vertex[j][1]), new Vertex(c.vertex[j + 1][0], c.vertex[j + 1][1]), normals[j][0], normals[j][1]);
                        mat.passthroughAngle = (int)(Math.Atan2(normals[j][1], normals[j][0]) * 180.0 / Math.PI);
                        if (mat.passthroughAngle < 0)
                        {
                            mat.passthroughAngle += 360;
                        }
                        materials.Add(mat);
                    }
                    c.normals = normals;
                    c.materials = materials;
                    collisions.Add(c);
                }
            }
            center = new Vertex(0, 0).ToList();
            center[0] = 0;
            float my = 0;
            if (collisions.Count > 0)
            {
                for (int i = 0; i < collisions.Count; i++)
                {
                    if (collisions[i].boundingBox[0][0] <= center[0] && center[0] <= collisions[i].boundingBox[1][0])
                    {
                        my = Math.Max(my, collisions[i].boundingBox[1][1]);
                    }
                }
            }
            else
            {
                my = 0;
            }
            center[1] = my;

            if (collisions.Count == 0 && platforms.Count == 0)
            {
                valid = false;
            }
        }

        public int CompareTo(Stage s)
        {
            return name.CompareTo(s.stage);
        }
    }


    public class StageCollisionMat
    {
        public bool leftLedge;
        public bool rightLedge;
        public bool noWallJump;
        public string material { get; set; }
        public int passthroughAngle = 0;
        public float length = 0;
        public bool ceiling = false;
        public bool wall = false;

        public StageCollisionMat(byte mat, bool leftLedge, bool rightLedge, bool noWallJump)
        {
            material = Enum.GetName(typeof(StageData.materialTypes), mat);
            if(material == null)
            {
                material = "0x" + mat.ToString("X2");
            }
            this.leftLedge = leftLedge;
            this.rightLedge = rightLedge;
            this.noWallJump = noWallJump;
        }

        public void calculateLength(Vertex v1, Vertex v2, float normalx, float normaly)
        {
            float angle = (float)(Math.Atan2(normaly, normalx) * 180 / Math.PI);
            if ((angle <= 20 && angle >= -70) || (angle <= -110 && angle >= -180) || angle == 180)
            {
                wall = true;
            }
            else if (normaly < 0)
            {
                ceiling = true;
            }
            length = (float)Math.Sqrt(Math.Pow(v2.x - v1.x, 2) + Math.Pow(v2.y - v1.y, 2));
        }
    }

    public class Collision
    {
        public string name { get; set; }
        public List<List<float>> vertex { get; set; }
        [JsonIgnore]
        public List<List<float>> normals { get; set; }
        public List<StageCollisionMat> materials { get; set; }
        public List<List<float>> boundingBox { get; set; }

        [JsonIgnore]
        public float[] startPos = new float[3] { 0, 0, 0 };
        [JsonIgnore]
        public bool useStartPos = false;

        public Collision(bool platform, string name, bool useStartPos, Vector3 startPos)
        {
            this.name = name;
            this.useStartPos = useStartPos;
            this.startPos = new float[] { startPos.X, startPos.Y, startPos.Z };
            vertex = new List<List<float>>();
            normals = new List<List<float>>();
            materials = new List<StageCollisionMat>();

            boundingBox = new List<List<float>>();
        }

        public void addVertex(Vertex v)
        {
            if (useStartPos)
            {
                v.x += startPos[0];
                v.y += startPos[1];
            }
            vertex.Add(v.ToList());
        }
    }


}
