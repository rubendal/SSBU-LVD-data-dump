using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK;

namespace Smash_Forge
{
    public abstract class LVDEntry
    {
        public abstract string magic { get; }
        private string _name = new string(new char[0x38]);
        private string _subname = new string(new char[0x40]);
        public Vector3 startPos = new Vector3();
        public bool useStartPos = false;
        public int unk1 = 0;
        public float[] unk2 = new float[3];
        public int unk3 = -1;
        private string _boneName = new string(new char[0x40]);

        private static string getString(string baseStr, int maxLength)
        {
            int length = baseStr.IndexOf((char)0);
            if (length == -1)
                length = Math.Min(baseStr.Length, maxLength);
            return baseStr.Substring(0, length);
        }
        private static string setString(string baseStr, int maxLength)
        {
            return baseStr.PadRight(maxLength, (char)0).Substring(0, maxLength);
        }

        public string name
        {
            get { return getString(_name, 0x38); }
            set { _name = setString(value, 0x38); }
        }
        public string subname
        {
            get { return getString(_subname, 0x40); }
            set { _subname = setString(value, 0x40); }
        }
        public string boneName
        {
            get { return getString(_boneName, 0x40); }
            set { _boneName = setString(value, 0x40); }
        }

        public void read(FileData f)
        {
            f.skip(0xC);

            f.skip(1);
            _name = f.readString(f.pos(), 0x38);
            f.skip(0x38);

            f.skip(1);
            _subname = f.readString(f.pos(), 0x40);
            f.skip(0x40);

            f.skip(1);
            for (int i = 0; i < 3; i++)
                startPos[i] = f.readFloat();
            useStartPos = Convert.ToBoolean(f.readByte());

            //Some kind of count? Only seen it as 0 so I don't know what it's for
            f.skip(1);
            unk1 = f.readInt();

            //Not sure what this is for, but it seems like it could be a vector followed by an index
            f.skip(1);
            for (int i = 0; i < 3; i++)
                unk2[i] = f.readFloat();
            unk3 = f.readInt();

            f.skip(1);
            _boneName = f.readString(f.pos(), 0x40);
            f.skip(0x40);
        }
        public void save(FileOutput f)
        {
            f.writeHex(magic);

            f.writeByte(1);
            f.writeString(_name);

            f.writeByte(1);
            f.writeString(_subname);

            f.writeByte(1);
            for (int i = 0; i < 3; i++)
                f.writeFloat(startPos[i]);
            f.writeFlag(useStartPos);

            f.writeByte(1);
            f.writeInt(unk1);

            f.writeByte(1);
            foreach (float i in unk2)
                f.writeFloat(i);
            f.writeInt(unk3);

            f.writeByte(1);
            f.writeString(_boneName);
        }
    }

    public class Point : LVDEntry
    {
        public override string magic { get { return ""; } }
        public float x;
        public float y;
    }

    public enum CollisionMatType : byte
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

    public class CollisionMat
    {
        public byte[] material = new byte[0xC];

        public byte physics
        {
            get { return material[3]; }
            set { material[3] = value; }
        }

        public byte getPhysics()
        {
            return physics;
        }

        public bool leftLedge
        {
            get { return getFlag(6); }
            set { setFlag(6, value); }
        }
        public bool rightLedge
        {
            get { return getFlag(7); }
            set { setFlag(7, value); }
        }
        public bool noWallJump
        {
            get { return getFlag(4); }
            set { setFlag(4, value); }
        }

        public bool getFlag(int n)
        {
            return ((material[10] & (1 << n)) != 0);
        }
        public void setFlag(int flag, bool value)
        {
            //Console.WriteLine("B - " + getFlag(flag));
            byte mask = (byte)(1 << flag);
            bool isSet = (material[10] & mask) != 0;
            if (value)
                material[10] |= mask;
            else
                material[10] &= (byte)~mask;
            //Console.WriteLine("A - " + getFlag(flag));
        }
    }

    public class CollisionCliff : LVDEntry
    {
        public override string magic { get { return "030401017735BB7500000002"; } }

        public Vector2 pos;
        public float angle; //I don't know what this does exactly, but it's -1 for left and 1 for right
        public int lineIndex;

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(1);
            pos = new Vector2();
            pos.X = f.readFloat();
            pos.Y = f.readFloat();
            angle = f.readFloat();
            lineIndex = f.readInt();
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeByte(1);
            f.writeFloat(pos.X);
            f.writeFloat(pos.Y);
            f.writeFloat(angle);
            f.writeInt(lineIndex);
        }
    }

    public class Collision : LVDEntry
    {
        public override string magic { get { return "030401017735BB7500000002"; } }

        public List<Vector2> verts = new List<Vector2>();
        public List<Vector2> normals = new List<Vector2>();
        public List<CollisionCliff> cliffs = new List<CollisionCliff>();
        public List<CollisionMat> materials = new List<CollisionMat>();
        //Flags: ???, rig collision, ???, drop-through
        public bool flag1 = false, flag2 = false, flag3 = false, flag4 = false;

        public bool IsPolygon
        {
            get
            {
                if (verts.Count < 2) return false;
                return verts[0].Equals(verts[verts.Count - 1]);
            }
        }

        public Collision() { }

        public new void read(FileData f)
        {
            base.read(f);

            flag1 = Convert.ToBoolean(f.readByte());
            flag2 = Convert.ToBoolean(f.readByte());
            flag3 = Convert.ToBoolean(f.readByte());
            flag4 = Convert.ToBoolean(f.readByte());

            f.skip(1);
            int vertCount = f.readInt();
            for (int i = 0; i < vertCount; i++)
            {
                f.skip(1);
                Vector2 temp = new Vector2();
                temp.X = f.readFloat();
                temp.Y = f.readFloat();
                verts.Add(temp);
            }

            f.skip(1);
            int normalCount = f.readInt();
            for (int i = 0; i < normalCount; i++)
            {
                f.skip(1);
                Vector2 temp = new Vector2();
                temp.X = f.readFloat();
                temp.Y = f.readFloat();
                normals.Add(temp);
            }

            f.skip(1);
            int cliffCount = f.readInt();
            for (int i = 0; i < cliffCount; i++)
            {
                CollisionCliff temp = new CollisionCliff();
                temp.read(f);
                cliffs.Add(temp);
            }

            f.skip(1);
            int materialCount = f.readInt();
            for (int i = 0; i < materialCount; i++)
            {
                f.skip(1);
                CollisionMat temp = new CollisionMat();
                temp.material = f.read(0xC); //Temporary, will work on fleshing out material more later
                materials.Add(temp);
            }
            f.skip(1);
            int num5 = f.readInt();
            f.skip(328 * num5);

        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeFlag(flag1);
            f.writeFlag(flag2);
            f.writeFlag(flag3);
            f.writeFlag(flag4);

            f.writeByte(1);
            f.writeInt(verts.Count);
            foreach (Vector2 v in verts)
            {
                f.writeByte(1);
                f.writeFloat(v.X);
                f.writeFloat(v.Y);
            }

            f.writeByte(1);
            f.writeInt(normals.Count);
            foreach (Vector2 n in normals)
            {
                f.writeByte(1);
                f.writeFloat(n.X);
                f.writeFloat(n.Y);
            }

            f.writeByte(1);
            f.writeInt(cliffs.Count);
            foreach (CollisionCliff c in cliffs)
            {
                c.save(f);
            }

            f.writeByte(1);
            f.writeInt(materials.Count);
            foreach (CollisionMat m in materials)
            {
                f.writeByte(1);
                f.writeBytes(m.material);
            }
        }
    }

    public class Spawn : LVDEntry
    {
        public override string magic { get { return "020401017735BB7500000002"; } }

        public float x;
        public float y;

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(1);
            x = f.readFloat();
            y = f.readFloat();
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeByte(1);
            f.writeFloat(x);
            f.writeFloat(y);
        }
    }

    public class Bounds : LVDEntry //For Camera Bounds and Blast Zones
    {
        public override string magic { get { return "020401017735BB7500000002"; } }

        public float left;
        public float right;
        public float top;
        public float bottom;

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(1);
            left = f.readFloat();
            right = f.readFloat();
            top = f.readFloat();
            bottom = f.readFloat();
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeByte(1);
            f.writeFloat(left);
            f.writeFloat(right);
            f.writeFloat(top);
            f.writeFloat(bottom);
        }
    }

    public enum LVDShapeType : int
    {
        Point = 1,
        Circle = 2,
        Rectangle = 3,
        Path = 4
    }

    //Basic 2D shape structure used for a variety of purposes within LVD:
    // - ItemSpawner sections
    // - EnemyGenerator enemy spawns and trigger zones
    // - GeneralShape object type
    // - More (unimplemented) things
    public class LVDShape
    {
        public int type;
        //The object always contains four floats, but how they are used differs depending on shape type
        public float shapeValue1, shapeValue2, shapeValue3, shapeValue4;
        public List<Vector2> points = new List<Vector2>();

        //Point and Circle properties
        public float x
        {
            get { return shapeValue1; }
            set { shapeValue1 = value; }
        }
        public float y
        {
            get { return shapeValue2; }
            set { shapeValue2 = value; }
        }
        public float radius
        {
            get { return shapeValue3; }
            set { shapeValue3 = value; }
        }

        //Rectangle properties
        public float left
        {
            get { return shapeValue1; }
            set { shapeValue1 = value; }
        }
        public float right
        {
            get { return shapeValue2; }
            set { shapeValue2 = value; }
        }
        public float bottom
        {
            get { return shapeValue3; }
            set { shapeValue3 = value; }
        }
        public float top
        {
            get { return shapeValue4; }
            set { shapeValue4 = value; }
        }

        public LVDShape()
        {
            type = 1;
        }
        public LVDShape(int type)
        {
            this.type = type;
        }
        public LVDShape(FileData f)
        {
            read(f);
        }

        public void read(FileData f)
        {
            f.readByte();
            type = f.readInt();
            if (!Enum.IsDefined(typeof(LVDShapeType), type))
                throw new NotImplementedException($"Unknown shape type {type} at offset {f.pos() - 4}");

            shapeValue1 = f.readFloat();
            shapeValue2 = f.readFloat();
            shapeValue3 = f.readFloat();
            shapeValue4 = f.readFloat();

            f.skip(1);
            f.skip(1);
            int pointCount = f.readInt();
            for (int i = 0; i < pointCount; i++)
            {
                f.skip(1);
                points.Add(new Vector2(f.readFloat(), f.readFloat()));
            }
        }
        public void save(FileOutput f)
        {
            f.writeByte(0x3);
            f.writeInt(type);

            f.writeFloat(shapeValue1);
            f.writeFloat(shapeValue2);
            f.writeFloat(shapeValue3);
            f.writeFloat(shapeValue4);

            f.writeByte(1);
            f.writeByte(1);
            f.writeInt(points.Count);
            foreach (Vector2 point in points)
            {
                f.writeByte(1);
                f.writeFloat(point.X);
                f.writeFloat(point.Y);
            }
        }
    }

    public class ItemSpawner : LVDEntry
    {
        public override string magic { get { return "010401017735BB7500000002"; } }

        public int id = 0x09840001;
        public List<LVDShape> sections = new List<LVDShape>();

        public ItemSpawner() { }

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(1);
            id = f.readInt();

            f.skip(1);
            f.skip(1);
            int sectionCount = f.readInt();
            for (int i = 0; i < sectionCount; i++)
            {
                f.skip(1);
                sections.Add(new LVDShape(f));
            }
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeByte(1);
            f.writeInt(id);

            f.writeByte(1);
            f.writeByte(1);
            f.writeInt(sections.Count);
            foreach (LVDShape s in sections)
            {
                f.writeByte(1);
                s.save(f);
            }
        }
    }

    public class EnemyGenerator : LVDEntry
    {
        public override string magic { get { return "030401017735BB7500000002"; } }

        public List<LVDShape> spawns = new List<LVDShape>();
        public List<LVDShape> zones = new List<LVDShape>();
        public int egUnk1 = 0;
        public int id;
        public List<int> spawnIds = new List<int>();
        public int egUnk2 = 0;
        public List<int> zoneIds = new List<int>();

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(0x2); //x01 01
            int spawnCount = f.readInt();
            for (int i = 0; i < spawnCount; i++)
            {
                f.skip(1);
                spawns.Add(new LVDShape(f));
            }

            f.skip(0x2); //x01 01
            int zoneCount = f.readInt();
            for (int i = 0; i < zoneCount; i++)
            {
                f.skip(1);
                zones.Add(new LVDShape(f));
            }

            f.skip(0x2); //x01 01
            egUnk1 = f.readInt(); //Only seen as 0

            f.skip(1); //x01
            id = f.readInt();

            f.skip(1); //x01
            int spawnIdCount = f.readInt();
            for (int i = 0; i < spawnIdCount; i++)
            {
                f.skip(1);
                spawnIds.Add(f.readInt());
            }

            f.skip(1); //x01
            egUnk2 = f.readInt(); //Only seen as 0

            f.skip(1); //x01
            int zoneIdCount = f.readInt();
            for (int i = 0; i < zoneIdCount; i++)
            {
                f.skip(1);
                zoneIds.Add(f.readInt());
            }
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeHex("0101");
            f.writeInt(spawns.Count);
            foreach (LVDShape temp in spawns)
            {
                f.writeByte(1);
                temp.save(f);
            }

            f.writeHex("0101");
            f.writeInt(zones.Count);
            foreach (LVDShape temp in zones)
            {
                f.writeByte(1);
                temp.save(f);
            }

            f.writeHex("0101");
            f.writeInt(egUnk1);

            f.writeByte(1);
            f.writeInt(id);

            f.writeByte(1);
            f.writeInt(spawnIds.Count);
            foreach (int temp in spawnIds)
            {
                f.writeByte(1);
                f.writeInt(temp);
            }

            f.writeByte(1);
            f.writeInt(egUnk2);

            f.writeByte(1);
            f.writeInt(zoneIds.Count);
            foreach (int temp in zoneIds)
            {
                f.writeByte(1);
                f.writeInt(temp);
            }
        }
    }

    //This is basically an LVDShape as a standalone object plus an id int
    public class GeneralShape : LVDEntry
    {
        public override string magic { get { return "010401017735BB7500000002"; } }

        public int id;
        public LVDShape shape;

        public GeneralShape()
        {
            shape = new LVDShape();
        }
        public GeneralShape(int type) : this()
        {
            shape.type = type;
        }

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(1);
            id = f.readInt();

            shape = new LVDShape();
            shape.read(f);
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeByte(1);
            f.writeInt(id);

            shape.save(f);
        }
    }

    public class GeneralPoint : LVDEntry
    {
        public override string magic { get { return "010401017735BB7500000002"; } }

        public int id;
        public int type;
        public float x, y, z;

        public GeneralPoint()
        {
            //Seems to always be 4
            type = 4;
        }

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(1);
            id = f.readInt();

            f.skip(1);
            type = f.readInt();

            x = f.readFloat();
            y = f.readFloat();
            z = f.readFloat();
            f.skip(0x10);
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeByte(1);
            f.writeInt(id);

            f.writeByte(1);
            f.writeInt(type);

            f.writeFloat(x);
            f.writeFloat(y);
            f.writeFloat(z);
            f.writeHex("00000000000000000000000000000000");
        }
    }

    public enum DamageShapeType
    {
        Sphere = 2,
        Capsule = 3
    }

    public class DamageShape : LVDEntry
    {
        public override string magic { get { return "010401017735BB7500000002"; } }

        public int type;

        public float x;
        public float y;
        public float z;
        public float dx;
        public float dy;
        public float dz;
        public float radius;
        public byte dsUnk1;
        public int dsUnk2;

        public new void read(FileData f)
        {
            base.read(f);

            f.skip(1);
            type = f.readInt();
            if (!Enum.IsDefined(typeof(DamageShapeType), type))
                throw new NotImplementedException($"Unknown damage shape type {type} at offset {f.pos() - 4}");

            x = f.readFloat();
            y = f.readFloat();
            z = f.readFloat();
            if (type == 2)
            {
                radius = f.readFloat();
                dx = f.readFloat();
                dy = f.readFloat();
                dz = f.readFloat();
            }
            else if (type == 3)
            {
                dx = f.readFloat();
                dy = f.readFloat();
                dz = f.readFloat();
                radius = f.readFloat();
            }
            dsUnk1 = f.readByte();
            dsUnk2 = f.readInt();
        }
        public new void save(FileOutput f)
        {
            base.save(f);

            f.writeByte(1);
            f.writeInt(type);

            f.writeFloat(x);
            f.writeFloat(y);
            f.writeFloat(z);
            if (type == 2)
            {
                f.writeFloat(radius);
                f.writeFloat(dx);
                f.writeFloat(dy);
                f.writeFloat(dz);
            }
            else if (type == 3)
            {
                f.writeFloat(dx);
                f.writeFloat(dy);
                f.writeFloat(dz);
                f.writeFloat(radius);
            }
            f.writeByte(dsUnk1);
            f.writeInt(dsUnk2);
        }
    }

    public class LVD : FileBase
    {
        public LVD()
        {
            collisions = new List<Collision>();
            spawns = new List<Spawn>();
            respawns = new List<Spawn>();
            cameraBounds = new List<Bounds>();
            blastzones = new List<Bounds>();
            enemyGenerators = new List<EnemyGenerator>();
            damageShapes = new List<DamageShape>();
            itemSpawns = new List<ItemSpawner>();
            generalShapes = new List<GeneralShape>();
            generalPoints = new List<GeneralPoint>();
        }
        public LVD(string filename) : this()
        {
            Read(filename);
        }
        public List<Collision> collisions { get; set; }
        public List<Spawn> spawns { get; set; }
        public List<Spawn> respawns { get; set; }
        public List<Bounds> cameraBounds { get; set; }
        public List<Bounds> blastzones { get; set; }
        public List<EnemyGenerator> enemyGenerators { get; set; }
        public List<DamageShape> damageShapes { get; set; }
        public List<ItemSpawner> itemSpawns { get; set; }
        public List<GeneralShape> generalShapes { get; set; }
        public List<GeneralPoint> generalPoints { get; set; }

        public override Endianness Endian { get; set; }

        /*type 1  - collisions
          type 2  - spawns
          type 3  - respawns
          type 4  - camera boundaries
          type 5  - death boundaries
          type 6  - enemy generators
          type 7  - ITEMPT_transform
          type 8  - ???
          type 9  - ITEMPT
          type 10 - fsAreaCam (and other fsArea's ? )
          type 11 - fsCamLimit
          type 12 - damage shapes (damage sphere and damage capsule are the only ones I've seen, type 2 and 3 respectively)
          type 13 - item spawners
          type 14 - general shapes (general rect, general path, etc.)
          type 15 - general points
          type 16 - area lights?
          type 17 - FsStartPoint
          type 18 - ???
          type 19 - ???*/

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.skip(0xA); //It's magic

            f.skip(1);
            int collisionCount = f.readInt();
            for (int i = 0; i < collisionCount; i++)
            {
                Collision temp = new Collision();
                temp.read(f);
                collisions.Add(temp);
            }

            f.skip(1);
            int spawnCount = f.readInt();
            for (int i = 0; i < spawnCount; i++)
            {
                Spawn temp = new Spawn();
                temp.read(f);
                spawns.Add(temp);
            }

            f.skip(1);
            int respawnCount = f.readInt();
            for (int i = 0; i < respawnCount; i++)
            {
                Spawn temp = new Spawn();
                temp.read(f);
                respawns.Add(temp);
            }

            f.skip(1);
            int cameraCount = f.readInt();
            for (int i = 0; i < cameraCount; i++)
            {
                Bounds temp = new Bounds();
                temp.read(f);
                cameraBounds.Add(temp);
            }

            f.skip(1);
            int blastzoneCount = f.readInt();
            for (int i = 0; i < blastzoneCount; i++)
            {
                Bounds temp = new Bounds();
                temp.read(f);
                blastzones.Add(temp);
            }

            //f.skip(1);
            //int enemyGeneratorCount = f.readInt();
            //for (int i = 0; i < enemyGeneratorCount; i++)
            //{
            //    EnemyGenerator temp = new EnemyGenerator();
            //    temp.read(f);
            //    enemyGenerators.Add(temp);
            //}

            //f.skip(1);
            //if (f.readInt() != 0) //7
            //    return;

            //f.skip(1);
            //if (f.readInt() != 0) //8
            //    return;

            //f.skip(1);
            //if (f.readInt() != 0) //9
            //    return;

            //f.skip(1);
            //int fsAreaCamCount = f.readInt();
            //if (fsAreaCamCount != 0)
            //    return;

            //f.skip(1);
            //int fsCamLimitCount = f.readInt();
            //if (fsCamLimitCount != 0)
            //    return;

            //f.skip(1);
            //int damageShapeCount = f.readInt();
            //for (int i = 0; i < damageShapeCount; i++)
            //{
            //    DamageShape temp = new DamageShape();
            //    temp.read(f);
            //    damageShapes.Add(temp);
            //}

            //f.skip(1);
            //int itemCount = f.readInt();
            //for (int i = 0; i < itemCount; i++)
            //{
            //    ItemSpawner temp = new ItemSpawner();
            //    temp.read(f);
            //    itemSpawns.Add(temp);
            //}

            //f.skip(1);
            //int generalShapeCount = f.readInt();
            //for (int i = 0; i < generalShapeCount; i++)
            //{
            //    GeneralShape temp = new GeneralShape();
            //    temp.read(f);
            //    generalShapes.Add(temp);
            //}

            //f.skip(1);
            //int generalPointCount = f.readInt();
            //for (int i = 0; i < generalPointCount; i++)
            //{
            //    GeneralPoint temp = new GeneralPoint();
            //    temp.read(f);
            //    generalPoints.Add(temp);
            //}

            //f.skip(1);
            //if (f.readInt() != 0) //16
            //    return; //no clue how to be consistent in reading these so...

            //f.skip(1);
            //if (f.readInt() != 0) //17
            //    return; //no clue how to be consistent in reading these so...

            //f.skip(1);
            //if (f.readInt() != 0) //18
            //    return; //no clue how to be consistent in reading these so...

            //f.skip(1);
            //if (f.readInt() != 0) //19
            //    return; //no clue how to be consistent in reading these so...

            ////LVD doesn't end here and neither does my confusion, will update this part later
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Big;

            f.writeHex("000000010A014C564431");

            f.writeByte(1);
            f.writeInt(collisions.Count);
            foreach (Collision c in collisions)
                c.save(f);

            f.writeByte(1);
            f.writeInt(spawns.Count);
            foreach (Spawn s in spawns)
                s.save(f);

            f.writeByte(1);
            f.writeInt(respawns.Count);
            foreach (Spawn s in respawns)
                s.save(f);

            f.writeByte(1);
            f.writeInt(cameraBounds.Count);
            foreach (Bounds b in cameraBounds)
                b.save(f);

            f.writeByte(1);
            f.writeInt(blastzones.Count);
            foreach (Bounds b in blastzones)
                b.save(f);

            f.writeByte(1);
            f.writeInt(enemyGenerators.Count);
            foreach (EnemyGenerator e in enemyGenerators)
                e.save(f);

            for (int i = 0; i < 5; i++)
            {
                f.writeByte(1);
                f.writeInt(0);
            }

            f.writeByte(1);
            f.writeInt(damageShapes.Count);
            foreach (DamageShape shape in damageShapes)
                shape.save(f);

            f.writeByte(1);
            f.writeInt(itemSpawns.Count);
            foreach (ItemSpawner item in itemSpawns)
                item.save(f);

            f.writeByte(1);
            f.writeInt(generalShapes.Count);
            foreach (GeneralShape shape in generalShapes)
                shape.save(f);

            f.writeByte(1);
            f.writeInt(generalPoints.Count);
            foreach (GeneralPoint p in generalPoints)
                p.save(f);

            for (int i = 0; i < 4; i++)
            {
                f.writeByte(1);
                f.writeInt(0);
            }

            return f.getBytes();
        }

        public static void GeneratePassthroughs(Collision c, bool PolyCheck = false)
        {
            // Generate Normals Assuming Clockwise
            for (int i = 0; i < c.verts.Count - 1; i++)
            {
                Vector2 v1 = c.verts[i];
                Vector2 v2 = c.verts[i + 1];
                Vector2 normal = new Vector2(v2.Y - v1.Y, v2.X - v1.X).Normalized();
                normal.X *= -1;
                c.normals[i] = normal;
            }

            // If this forms a polygon we can get assume we want the angles to points outside the polygon
            // Not the fastest but lvd won't typically have a massive number of lines
            if (c.IsPolygon && PolyCheck)
            {
                for (int i = 0; i < c.verts.Count - 1; i++)
                {
                    Vector2 pos = (c.verts[i] + c.verts[i + 1]) / 2;
                    Vector2 N1 = c.normals[i];

                    // Check collision
                    // done by counting the number of intersection using the normal as a ray
                    // odd hits = inside even hits = outside
                    // https://rootllama.wordpress.com/2014/06/20/ray-line-segment-intersection-test-in-2d/
                    int count = 0;
                    for (int j = 0; j < c.verts.Count - 1; j++)
                    {
                        if (j == i) continue;

                        Vector2 v1 = c.verts[j];
                        Vector2 v2 = c.verts[j + 1];

                        Vector2 p1 = pos - v1;
                        Vector2 p2 = v2 - v1;
                        Vector2 p3 = new Vector2(-N1.Y, N1.X);

                        float dot = Vector2.Dot(p2, p3);
                        if (Math.Abs(dot) < 0.00001f)
                            continue;

                        float f1 = (p2.X * p1.Y - p2.Y * p1.X) / dot;
                        float f2 = Vector2.Dot(p1, p3) / dot;

                        //Found intersection
                        if (f1 >= 0.0f && (f2 >= 0.0f && f2 <= 1.0f))
                            count++;
                    }

                    if (count % 2 == 1)
                        //odd so flip
                        c.normals[i] = c.normals[i] * -1;
                }
            }
        }

        public static void FlipPassthroughs(Collision c)
        {
            for (int i = 0; i < c.normals.Count; i++)
            {
                c.normals[i] = c.normals[i] * -1;
            }
        }

        //Function to automatically add a cliff to every grabbable ledge in a given collision
        //Works mostly to vanilla standards, though vanilla standards are inconsistent on handling bone name/start pos
        public static void GenerateCliffs(Collision col)
        {
            int[] counts = new int[2];
            bool[,] lines = new bool[col.materials.Count, 2];
            for (int i = 0; i < col.materials.Count; i++)
            {
                lines[i, 0] = col.materials[i].leftLedge;
                lines[i, 1] = col.materials[i].rightLedge;
                if (lines[i, 0]) counts[0]++;
                if (lines[i, 1]) counts[1]++;
            }

            string nameSub;
            if (col.name.Length > 4 && col.name.StartsWith("COL_"))
                nameSub = col.name.Substring(4, col.name.Length - 4);
            else
                nameSub = "Collision";

            col.cliffs = new List<CollisionCliff>();
            counts[0] = counts[0] > 1 ? 1 : 0;
            counts[1] = counts[1] > 1 ? 1 : 0;
            for (int i = 0; i < col.materials.Count; i++)
            {
                if (lines[i, 0])
                {
                    string cliffName = "CLIFF_" + nameSub + "L" + (counts[0] > 0 ? $"{counts[0]++}" : "");
                    CollisionCliff temp = new CollisionCliff();
                    temp.name = cliffName;
                    temp.subname = cliffName.Substring(6, cliffName.Length - 6);
                    temp.boneName = col.boneName;
                    temp.useStartPos = col.useStartPos;
                    int ind = i;
                    temp.pos = new Vector2(col.verts[ind].X, col.verts[ind].Y);
                    temp.startPos = new Vector3(col.verts[ind].X, col.verts[ind].Y, 0);
                    if (col.useStartPos)
                        temp.startPos = Vector3.Add(temp.startPos, col.startPos);
                    temp.angle = -1.0f;
                    temp.lineIndex = i;
                    col.cliffs.Add(temp);
                }
                if (lines[i, 1])
                {
                    string cliffName = "CLIFF_" + nameSub + "R" + (counts[1] > 0 ? $"{counts[1]++}" : "");
                    CollisionCliff temp = new CollisionCliff();
                    temp.name = cliffName;
                    temp.subname = cliffName.Substring(6, cliffName.Length - 6);
                    temp.boneName = col.boneName;
                    temp.useStartPos = col.useStartPos;
                    int ind = i + 1;
                    temp.pos = new Vector2(col.verts[ind].X, col.verts[ind].Y);
                    temp.startPos = new Vector3(col.verts[ind].X, col.verts[ind].Y, 0);
                    if (col.useStartPos)
                        temp.startPos = Vector3.Add(temp.startPos, col.startPos);
                    temp.angle = 1.0f;
                    temp.lineIndex = i;
                    col.cliffs.Add(temp);
                }
            }
        }
    }

}


