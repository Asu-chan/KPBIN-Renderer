using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace KPBIN_Renderer
{

    /*
    [StructLayout(LayoutKind.Explicit)]
    public struct dKPLayer_s
    {
        public enum LayerTypes
        {
            OBJECTS, DOODADS, PATHS
        };

        [FieldOffset(0)] public LayerTypes type;
        [FieldOffset(4)] public byte alpha;
        [FieldOffset(5)] public byte[] _padding = new byte[3];

        [FieldOffset(8)] GXTexObj *tileset;
        [FieldOffset(0xC)] int sectorLeft;
        [FieldOffset(0x10)] int sectorTop;
        [FieldOffset(0x14)] int sectorRight;
        [FieldOffset(0x18)] int sectorBottom;
        [FieldOffset(0x1C)] ushort[] indices;

        [FieldOffset(8)] int doodadCount;
        [FieldOffset(0xC)] List<dKPDoodad_s> *doodads;

        [FieldOffset(8)] int nodeCount;
        [FieldOffset(0xC)] List<dKPNode_s> *nodes;
        [FieldOffset(0x10)] int pathCount;
        [FieldOffset(0x14)] List<dKPPath_s> *paths;

    }
    

    [StructLayout(LayoutKind.Explicit)]
    public struct GXTexObj
    {
        [FieldOffset(0)] public uint _0;
        [FieldOffset(4)] public uint _4;
        [FieldOffset(8)] public uint formatStuff;

        [FieldOffset(0xC)] public List<byte> *rawPicture;
        [FieldOffset(0xC)] public string *tilesetPath;

        [FieldOffset(0x10)] public uint _10;
        [FieldOffset(0x14)] public uint _14;
        [FieldOffset(0x18)] public uint _18;

        [FieldOffset(0x1C)] public ushort resolutionStuff;
        [FieldOffset(0x1E)] public ushort _1E;
    }
    */

    public static class Globals
    {
        public static Dictionary<uint, GXTexObj> gxInstancesDict = new Dictionary<uint, GXTexObj>();
        public static Dictionary<uint, dKPLayer_s> layerInstancesDict = new Dictionary<uint, dKPLayer_s>();
    }

    public class dKPLayer_s
    {
        // Common
        public enum LayerTypes
        {
            OBJECTS, DOODADS, PATHS
        };

        public LayerTypes type;
        public byte alpha;

        // OBJECTS
        public GXTexObj tileset = new GXTexObj();
        public int sectorLeft;
        public int sectorTop;
        public int sectorRight;
        public int sectorBottom;
        public int boundLeft;
        public int boundTop;
        public int boundRight;
        public int boundBottom;
        public List<ushort> indices = new List<ushort>();

        // DOODADS
        public List<dKPDoodad_s> doodads = new List<dKPDoodad_s>();

        // PATHS
        public List<dKPNode_s> nodes = new List<dKPNode_s>();
        public List<dKPPath_s> paths = new List<dKPPath_s>();

        // Not in OG
        public bool visible;

        public void parseFromData(ref byte[] data, int offs)
        {
            Globals.layerInstancesDict.Add((uint)offs, this);

            // Reset
            tileset = new GXTexObj();
            doodads = new List<dKPDoodad_s>();
            nodes = new List<dKPNode_s>();
            paths = new List<dKPPath_s>();

            // Parsing
            type = (LayerTypes)data.GetUInt32(offs + 0);
            alpha = data[offs + 4];
            visible = alpha > 0;

            switch (type)
            {
                case LayerTypes.OBJECTS:
                    uint tilesetGXOffs = data.GetUInt32(offs + 8);
                    if (Globals.gxInstancesDict.ContainsKey(tilesetGXOffs)) tileset = Globals.gxInstancesDict[tilesetGXOffs];
                    else tileset.parseFromData(ref data, (int)tilesetGXOffs);

                    sectorLeft = data.GetInt32(offs + 0xC);
                    sectorTop = data.GetInt32(offs + 0x10);
                    sectorRight = data.GetInt32(offs + 0x14);
                    sectorBottom = data.GetInt32(offs + 0x18);

                    boundLeft = data.GetInt32(offs + 0x1C);
                    boundTop = data.GetInt32(offs + 0x20);
                    boundRight = data.GetInt32(offs + 0x24);
                    boundBottom = data.GetInt32(offs + 0x28);

                    int indicesCount = (sectorRight - (sectorLeft - 1)) * (sectorBottom - (sectorTop - 1));
                    //Console.WriteLine("      " + indicesCount + " (" + sectorLeft + ";" + sectorTop + " to " + sectorRight + ";" + sectorBottom + ")");
                    for (int i = 0; i < indicesCount; i++)
                    {
                        indices.Add(data.GetUInt16(offs + 0x2C + (i * 2)));
                        //Console.WriteLine("Found indice " + data.GetUInt16(offs + 0x2C + (i * 2)));
                    }

                    break;

                case LayerTypes.DOODADS:
                    uint doodadCount = data.GetUInt32(offs + 8);
                    for (int i = 0; i < doodadCount; i++)
                    {
                        int doodadOffs = (int)data.GetUInt32(offs + 0xC + (i * 4));

                        dKPDoodad_s doodad = new dKPDoodad_s();
                        doodad.parseFromData(ref data, doodadOffs);
                        doodads.Add(doodad);
                    }

                    break;

                case LayerTypes.PATHS:
                    uint nodeCount = data.GetUInt32(offs + 8);
                    int nodeOffsOffs = (int)data.GetUInt32(offs + 0xC);
                    uint pathCount = data.GetUInt32(offs + 0x10);
                    int pathOffsOffs = (int)data.GetUInt32(offs + 0x14);

                    for (int i = 0; i < nodeCount; i++)
                    {
                        int nodeOffs = (int)data.GetUInt32(nodeOffsOffs + (i * 4));

                        dKPNode_s node = new dKPNode_s();
                        node.parseFromData(ref data, nodeOffs);
                        nodes.Add(node);
                    }

                    for (int i = 0; i < pathCount; i++)
                    {
                        int pathOffs = (int)data.GetUInt32(pathOffsOffs + (i * 4));

                        dKPPath_s path = new dKPPath_s();
                        path.parseFromData(ref data, pathOffs);
                        paths.Add(path);
                    }

                    foreach(dKPNode_s node in nodes)
                        node.bindToPaths(ref data, (int)node.offsetIdentifier, ref paths);

                    foreach(dKPPath_s path in paths)
                        path.bindToNodes(ref data, (int)path.offsetIdentifier, ref nodes);

                    break;
            }
        }
    }

    public class dKPDoodad_s
    {
        public struct animation_s
        {
            public enum LoopTypes
            {
                CONTIGUOUS, LOOP, REVERSE_LOOP
            };

            public enum CurveTypes
            {
                LINEAR, SIN, COS
            };

            public enum AnimTypes
            {
                X_POS, Y_POS, ANGLE, X_SCALE, Y_SCALE, OPACITY
            };

            public LoopTypes loop;
            public CurveTypes curve;
            public int frameCount;
            public AnimTypes type;
            public int start, end;
            public int delay, delayOffset;

            public uint baseTick;
            public bool isReversed;
        };

        public float x, y;
        public float width, height;
        public float angle;

        public GXTexObj texObj = new GXTexObj();
        public int animationCount;
        List<animation_s> animations = new List<animation_s>();

        public void parseFromData(ref byte[] data, int offs)
        {
            // Reset
            animations = new List<animation_s>();
            texObj = new GXTexObj();

            // Parse
            x = data.GetFloat(offs + 0);
            y = data.GetFloat(offs + 4);
            width = data.GetFloat(offs + 8);
            height = data.GetFloat(offs + 0xC);
            angle = data.GetFloat(offs + 0x10);

            uint texObjGXOffs = data.GetUInt32(offs + 0x14);
            if (Globals.gxInstancesDict.ContainsKey(texObjGXOffs)) texObj = Globals.gxInstancesDict[texObjGXOffs];
            else texObj.parseFromData(ref data, (int)texObjGXOffs);

            uint animationCount = data.GetUInt32(offs + 0x18);

            int idx = offs + 0x1C;
            for (int i = 0; i < animationCount; i++)
            {
                animation_s animation = new animation_s();

                animation.loop = (animation_s.LoopTypes)data.GetUInt32(idx + 0);
                animation.curve = (animation_s.CurveTypes)data.GetUInt32(idx + 4);
                animation.frameCount = data.GetInt32(idx + 8);
                animation.type = (animation_s.AnimTypes)data.GetUInt32(idx + 0xC);
                animation.start = data.GetInt32(idx + 0x10);
                animation.end = data.GetInt32(idx + 0x14);
                animation.delay = data.GetInt32(idx + 0x18);
                animation.delayOffset = data.GetInt32(idx + 0x1C);
                animation.baseTick = data.GetUInt32(idx + 0x20);
                animation.isReversed = (data[idx + 0x24] > 0);

                animations.Add(animation);

                idx += 0x28;
            }
        }
    }

    public unsafe class dKPNode_s
    {
        // Common
        public enum NodeTypes
        {
            PASS_THROUGH, STOP, LEVEL, CHANGE, WORLD_CHANGE
        };

        public short x, y;

        public dKPPath_s leftExit;
        public dKPPath_s rightExit;
        public dKPPath_s upExit;
        public dKPPath_s downExit;

        public dKPLayer_s tileLayer;
        public dKPLayer_s doodadLayer;

        public byte reserved1, reserved2, reserved3;
        public NodeTypes type;

        public bool isNew;

        // LEVEL
        public byte worldNumber;
        public byte levelNumber;
        public bool hasSecret;

        // CHANGE
        public string destMap;
        public byte thisID, foreignID, transition, _;

        // WORLD_CHANGE
        public byte worldID;
        public byte[] __ = new byte[3];

        // Not in OG
        public enum NodeColor
        {
            BLACK,
            RED,
            BLUE,
            PURPLE,
            SHOP
        };
        public NodeColor nodeColor;

        public uint offsetIdentifier;
        public bool hasTileLayer;
        public bool hasDoodadLayer;
        public bool hasLeftPath;
        public bool hasRightPath;
        public bool hasUpPath;
        public bool hasDownPath;

        public bool visible;

        public void parseFromData(ref byte[] data, int offs)
        {
            offsetIdentifier = (uint)offs;
            visible = true;

            // Reset
            tileLayer = new dKPLayer_s();
            doodadLayer = new dKPLayer_s();

            x = data.GetInt16(offs + 0);
            y = data.GetInt16(offs + 2);

            // Path pointers are initialized later

            uint tileLayerOffs = data.GetUInt32(offs + 0x14);
            if(tileLayerOffs > 0)
            {
                if (Globals.layerInstancesDict.ContainsKey(tileLayerOffs)) tileLayer = Globals.layerInstancesDict[tileLayerOffs];
                else tileLayer.parseFromData(ref data, (int)tileLayerOffs);
                hasTileLayer = true;
            }
            else hasTileLayer = false;

            uint doodadLayerOffs = data.GetUInt32(offs + 0x18);
            if(doodadLayerOffs > 0)
            {
                if (Globals.layerInstancesDict.ContainsKey(doodadLayerOffs)) doodadLayer = Globals.layerInstancesDict[doodadLayerOffs];
                else doodadLayer.parseFromData(ref data, (int)doodadLayerOffs);
                hasDoodadLayer = true;
            }
            else hasDoodadLayer = false;

            reserved1 = data[offs + 0x1C];
            reserved2 = data[offs + 0x1D];
            reserved3 = data[offs + 0x1E];
            type = (NodeTypes)data[offs + 0x1F];

            isNew = (data[offs + 0x20] > 0);

            switch(type)
            {
                case NodeTypes.LEVEL:
                    worldNumber = data[offs + 0x28];
                    levelNumber = data[offs + 0x29];
                    hasSecret = (data[offs + 0x2A] > 0);
                    break;

                case NodeTypes.CHANGE:
                    destMap = data.GetString((int)data.GetUInt32(offs + 0x28));
                    thisID = data[offs + 0x2C];
                    foreignID = data[offs + 0x2D];
                    transition = data[offs + 0x2E];
                    _ = data[offs + 0x2F];
                    break;

                case NodeTypes.WORLD_CHANGE:
                    worldID = data[offs + 0x28];
                    break;
            }
        }

        public void bindToPaths(ref byte[] data, int offs, ref List<dKPPath_s> paths)
        {
            uint leftPathOffs = data.GetUInt32(offs + 4);
            uint rightPathOffs = data.GetUInt32(offs + 8);
            uint upPathOffs = data.GetUInt32(offs + 0xC);
            uint downPathOffs = data.GetUInt32(offs + 0x10);

            if (leftPathOffs != 0xFFFFFFFF)
            {
                leftExit = paths.FirstOrDefault(z => z.offsetIdentifier == leftPathOffs);
                hasLeftPath = true;
            }
            else hasLeftPath = false;

            if (rightPathOffs != 0xFFFFFFFF)
            {
                rightExit = paths.FirstOrDefault(z => z.offsetIdentifier == rightPathOffs);
                hasRightPath = true;
            }
            else hasRightPath = false;

            if (upPathOffs != 0xFFFFFFFF)
            {
                upExit = paths.FirstOrDefault(z => z.offsetIdentifier == upPathOffs);
                hasUpPath = true;
            }
            else hasUpPath = false;

            if (downPathOffs != 0xFFFFFFFF)
            {
                downExit = paths.FirstOrDefault(z => z.offsetIdentifier == downPathOffs);
                hasDownPath = true;
            }
            else hasDownPath = false;

        }
    }

    public class dKPPath_s
    {
        public enum Availability
        {
            NOT_AVAILABLE = 0,
            AVAILABLE = 1,
            NEWLY_AVAILABLE = 2,
            ALWAYS_AVAILABLE = 3
        };

        public enum Animation
        {
            WALK = 0, WALK_SAND = 1, WALK_SNOW = 2, WALK_WATER = 3,
            JUMP = 4, JUMP_SAND = 5, JUMP_SNOW = 6, JUMP_WATER = 7,
            LADDER = 8, LADDER_LEFT = 9, LADDER_RIGHT = 10, FALL = 11,
            SWIM = 12, RUN = 13, PIPE = 14, DOOR = 15,
            TJUMPED = 16, ENTER_CAVE_UP = 17, RESERVED_18 = 18, INVISIBLE = 19,
            MAX_ANIM = 20
        };

        public dKPNode_s start;
        public dKPNode_s end;
        public dKPLayer_s tileLayer = new dKPLayer_s();
        public dKPLayer_s doodadLayer = new dKPLayer_s();

        public Availability isAvailable; // computed on-the-fly - default from Koopatlas is NOT or ALWAYS
        public byte isSecret;
        //public byte[] _padding = new byte[2];
        public float speed;
        public Animation animation;

        // Not in OG
        public uint offsetIdentifier;
        public bool hasTileLayer;
        public bool hasDoodadLayer;

        public void parseFromData(ref byte[] data, int offs)
        {
            // Reset
            tileLayer = new dKPLayer_s();
            doodadLayer = new dKPLayer_s();

            // Parsing
            offsetIdentifier = (uint)offs;

            // Node pointers are initialized later

            uint tileLayerOffs = data.GetUInt32(offs + 8);
            if (tileLayerOffs > 0)
            {
                if (Globals.layerInstancesDict.ContainsKey(tileLayerOffs)) tileLayer = Globals.layerInstancesDict[tileLayerOffs];
                else tileLayer.parseFromData(ref data, (int)tileLayerOffs);
                hasTileLayer = true;
            }
            else hasTileLayer = false;

            uint doodadLayerOffs = data.GetUInt32(offs + 0xC);
            if (doodadLayerOffs > 0)
            {
                if (Globals.layerInstancesDict.ContainsKey(doodadLayerOffs)) doodadLayer = Globals.layerInstancesDict[doodadLayerOffs];
                else doodadLayer.parseFromData(ref data, (int)doodadLayerOffs);
                hasDoodadLayer = true;
            }
            else hasDoodadLayer = false;

            isAvailable = (Availability)data[offs + 0x10];
            isSecret = data[offs + 0x11];

            speed = data.GetFloat(offs + 0x14);
            animation = (Animation)data.GetUInt32(offs + 0x18);
        }

        public void bindToNodes(ref byte[] data, int offs, ref List<dKPNode_s> nodes)
        {
            uint startNodeOffs = data.GetUInt32(offs + 0);
            uint endNodeOffs = data.GetUInt32(offs + 4);

            start = nodes.FirstOrDefault(z => z.offsetIdentifier == startNodeOffs);
            end = nodes.FirstOrDefault(z => z.offsetIdentifier == endNodeOffs);
        }
    }

    public class dKPWorldDef_s
    {
        public string name;
        public uint[] fsTextColours = new uint[2];
        public uint[] fsHintColours = new uint[2];
        public uint[] hudTextColours = new uint[2];
        public ushort hudHintH;
        public sbyte hudHintS, hudHintL;
        public byte key, trackID;
        public byte worldID;
        public byte titleScreenWorld;
        public byte titleScreenLevel;
        //public byte[] padding = new byte[3];

        public void parseFromData(ref byte[] data, int offs)
        {
            // Reset
            fsTextColours = new uint[2];
            fsHintColours = new uint[2];
            hudTextColours = new uint[2];

            // Parsing
            name = data.GetString((int)data.GetUInt32(offs + 0));

            fsTextColours[0] = data.GetUInt32(offs + 4);
            fsTextColours[1] = data.GetUInt32(offs + 8);
            fsHintColours[0] = data.GetUInt32(offs + 0xC);
            fsHintColours[1] = data.GetUInt32(offs + 0x10);
            hudTextColours[0] = data.GetUInt32(offs + 0x14);
            hudTextColours[1] = data.GetUInt32(offs + 0x18);

            hudHintH = data.GetUInt16(offs + 0x1C);
            hudHintS = (sbyte)data[offs + 0x1E];
            hudHintL = (sbyte)data[offs + 0x1F];

            key = data[offs + 0x20];
            trackID = data[offs + 0x21];
            worldID = data[offs + 0x22];
            titleScreenWorld = data[offs + 0x23];
            titleScreenLevel = data[offs + 0x24];
        }
    }

    public class UnlockData
    {
        public List<UnlockCriteria> unlockCriterias = new List<UnlockCriteria>();

        public void parseFromData(ref byte[] data, int offs)
        {
            // Reset
            unlockCriterias = new List<UnlockCriteria>();

            // Parsing
            int idx = offs;
            while(data[idx] != 0)
            {
                UnlockCriteria unlockCriteria = new UnlockCriteria();
                unlockCriteria.parseFromData(ref data, ref idx);
                unlockCriterias.Add(unlockCriteria);
            }
        }

        public bool IsPathAvailable(ushort pathID, int[][] udatas)
        {
            UnlockCriteria uc = unlockCriterias.Find(p => p.pathIDs.Contains(pathID));
            if (uc == null) return true;
            return uc.IsCriteriaTrue(udatas);
        }

        public int[][] GetAsUdatas(ref int[][] udataso)
        {
            // Acquire them unsorted and with duplicates
            List<int[]> udatas = udataso.ToList();

            foreach(UnlockCriteria unlockCriteria in unlockCriterias)
            {
                udatas = udatas.Concat(unlockCriteria.GetAsUdatas()).ToList();
            }

            // Sort by level & remove duplicates
            List<int[]> udatas2 = new List<int[]>();

            for(int level = 0; level < 42; level++)
            {
                foreach (int[] udata in udatas)
                {
                    if (udata[0] != 255 && udata[1] == level && !udatas2.ContainsSequence(udata))
                    {
                        // If there's no entry that had two exits
                        int[] doubleVerify = new int[] { udata[0], udata[1], 2, 1, 1 };
                        if (!udatas2.ContainsSequence(doubleVerify))
                        {
                            if (udata[2] == 0) // Normal exit entry
                            {
                                int[] verify = new int[] { udata[0], udata[1], 1, 1 };
                                if (udatas2.ContainsSequence(verify)) // If we've got a secret exit entry, remove it and add a double exit entry
                                {
                                    udatas2.Remove(udatas2.GetSequence(verify));
                                    udatas2.Add(doubleVerify);
                                }
                                else udatas2.Add(udata);
                            }
                            else // Secret exit entry
                            {
                                int[] verify = new int[] { udata[0], udata[1], 0, 1 };
                                if (udatas2.ContainsSequence(verify)) // If we've got a normal exit entry, remove it and add a double exit entry
                                {
                                    udatas2.Remove(udatas2.GetSequence(verify));
                                    udatas2.Add(doubleVerify);
                                }
                                else udatas2.Add(udata);
                            }
                        }
                    }
                }
            }

            // Sort by world
            List<int[]> udatas3 = new List<int[]>();
            for (int world = 0; world < 10; world++)
            {
                foreach (int[] udata in udatas2)
                {
                    if (udata[0] == world && !udatas3.ContainsSequence(udata))
                    {
                        udatas3.Add(udata);
                    }
                }
            }

            // Ad comparison criterias
            foreach (int[] udata in udatas)
            {
                if(udata[0] == 255)
                    udatas3.Add(udata);
            }

            return udatas3.ToArray();
        }
    }

    public class UnlockCriteria
    {
        public byte[] criteria = new byte[0];
        public List<ushort> pathIDs = new List<ushort>();

        public string asString = "";

        public void parseFromData(ref byte[] data, ref int offs)
        {
            // Reset
            pathIDs = new List<ushort>();
            asString = "";

            // Parsing
            criteria = getByteArrayForIdx(ref data, ref offs);

            byte pathCount = data[offs++];
            Console.Write("  Parsed criteria \"" + asString + "\" for Path ID" + ((pathCount > 1) ? "s" : "") + " ");
            for (int i = 0; i < pathCount; i++)
            {
                ushort pathID = data.GetUInt16(offs);
                pathIDs.Add(pathID);
                offs += 2;

                Console.Write(pathID + ((i < pathCount-1) ? ", " : ""));
            }

            Console.WriteLine(".");
        }

        public bool IsCriteriaTrue(int[][] udatas)
        {
            int offs = 0;
            return IsCriteriaTrue(udatas, ref offs);
        }

        public bool IsCriteriaTrue(int[][] udatas, ref int offs)
        {

            byte controlByte = criteria[offs++];
            byte conditionType = (byte)(controlByte >> 6);
            //Console.WriteLine("conditionType: " + conditionType.ToString("X") + " for " + controlByte.ToString("X"));

            if (conditionType == 0)
            {
                byte subConditionType = (byte)(controlByte & 0x3F);
                if (subConditionType < 4)
                {
                    byte one = criteria[offs++];
                    byte two = criteria[offs++];

                    int compareOne = ((one & 0x80) > 0) ? 1 : 0;
                    int compareTwo = ((one & 0x7F) << 8) | two;

                    foreach (int[] udata in udatas)
                        if (udata[0] == 255 && udata[1] == compareOne)
                            compareOne = udata[2];

                    switch (subConditionType)
                    {
                        case 0:
                            return compareOne == compareTwo;
                        case 1:
                            return compareOne != compareTwo;
                        case 2:
                            return compareOne < compareTwo;
                        case 3:
                            return compareOne > compareTwo;
                        default:
                            return false;
                    }
                }
                else if (subConditionType == 15) return true;
                else return false;
            }
            else if (conditionType == 1)
            {
                int isSecret = ((controlByte & 0x10) > 0) ? 1 : 0;
                byte worldNumber = (byte)(controlByte & 0xF);
                byte levelNumber = criteria[offs++];

                foreach (int[] udata in udatas)
                {
                    if (udata[0] == worldNumber && udata[1] == levelNumber)
                    {
                        if (udata[2] == isSecret) return (udata[3] > 0);
                        if (udata[2] == 2) return (udata[(isSecret > 0) ? 4 : 3] > 0);
                    }
                }

                return false;
            }
            else if (conditionType == 2 || conditionType == 3)
            {
                bool isOr = (conditionType == 3);

                bool value = isOr ? false : true;

                byte termCount = (byte)((controlByte & 0x3F) + 1);
                for (int i = 0; i < termCount; i++)
                {
                    bool what = IsCriteriaTrue(udatas, ref offs);

                    if (isOr)
                        value |= what;
                    else
                        value &= what;
                }

                return value;
            }
            else
            {
                return false;
            }
        }

        public byte[] getByteArrayForIdx(ref byte[] data, ref int offs)
        {
            int beginOffs = offs;

            byte controlByte = data[offs++];
            byte conditionType = (byte)(controlByte >> 6);

            if (conditionType == 0)
            {
                byte subConditionType = (byte)(controlByte & 0x3F);
                if (subConditionType < 4)
                {
                    byte one = data[offs++];
                    byte two = data[offs++];

                    string compareOne = ((one & 0x80) > 0) ? "unspent star coins" : "total star coins";
                    int compareTwo = ((one & 0x7F) << 8) | two;
                    asString += compareOne;

                    switch (subConditionType)
                    {
                        case 0:
                            asString += " == ";
                            break;
                        case 1:
                            asString += " != ";
                            break;
                        case 2:
                            asString += " < ";
                            break;
                        case 3:
                            asString += " > ";
                            break;
                    }

                    asString += compareTwo;
                }
            }

            if (conditionType == 1)
            {
                bool isSecret = (controlByte & 0x10) > 0;
                byte worldNumber = (byte)(controlByte & 0xF);
                byte levelNumber = data[offs++];

                asString += (worldNumber + 1).ToString("D2") + "-" + (levelNumber + 1).ToString("D2") + ((isSecret) ? " secret" : "");
            }

            if (conditionType == 2 || conditionType == 3)
            {
                bool isOr = (conditionType == 3);

                string comparator = (isOr ? " or " : " and ");

                byte termCount = (byte)((controlByte & 0x3F) + 1);
                for (int i = 0; i < termCount; i++)
                {
                    asString += "(";
                    getByteArrayForIdx(ref data, ref offs);
                    asString += ")";

                    if (i < termCount - 1)
                        asString += comparator;
                }
            }

            return data.Skip(beginOffs).Take(offs - beginOffs).ToArray();
        }

        public int[][] GetAsUdatas()
        {
            int offs = 0;
            return GetAsUdatas(ref offs);
        }

        public int[][] GetAsUdatas(ref int offs)
        {
            byte controlByte = criteria[offs++];
            byte conditionType = (byte)(controlByte >> 6);

            if (conditionType == 0)
            {
                byte subConditionType = (byte)(controlByte & 0x3F);
                if (subConditionType < 4)
                {
                    byte one = criteria[offs++];
                    byte two = criteria[offs++];

                    int compareTwo = ((one & 0x7F) << 8) | two;

                    return new int[][] { new int[] { 255, ((one & 0x80) > 0) ? 1 : 0, 0 } };
                }
                else return new int[0][];
            }

            if (conditionType == 1)
            {
                bool isSecret = (controlByte & 0x10) > 0;
                byte worldNumber = (byte)(controlByte & 0xF);
                byte levelNumber = criteria[offs++];

               return new int[][] { new int[] { worldNumber, levelNumber, (isSecret ? 1 : 0), 1 } };
            }

            if (conditionType == 2 || conditionType == 3)
            {
                List<int[]> udatas = new List<int[]>();

                byte termCount = (byte)((controlByte & 0x3F) + 1);
                for (int i = 0; i < termCount; i++)
                {
                    udatas = udatas.Concat(GetAsUdatas(ref offs)).ToList();
                }

                return udatas.ToArray();
            }

            return new int[0][];
        }
    }



    public class GXTexObj
    {
        // Common
        public uint _0;
        public uint _4;
        public uint formatStuff;

        // Pre-loaded
        public byte[] rawPicture = new byte[0];
        // Filename
        public string tilesetPath;

        // Common
        public uint _10;
        public uint _14;
        public uint _18;

        public ushort resolutionStuff;
        public ushort _1E;

        // Not in OG
        public uint offsetIdentifier;

        public void parseFromData(ref byte[] data, int offs)
        {
            offsetIdentifier = (uint)offs;

            // Reset
            rawPicture = new byte[0];
            tilesetPath = "";

            // Parsing common stuff
            _0 = data.GetUInt32(offs + 0);
            _4 = data.GetUInt32(offs + 4);
            formatStuff = data.GetUInt32(offs + 8);
            _10 = data.GetUInt32(offs + 0x10);
            _14 = data.GetUInt32(offs + 0x14);
            _18 = data.GetUInt32(offs + 0x18);
            resolutionStuff = data.GetUInt16(offs + 0x1C);
            _1E = data.GetUInt16(offs + 0x1E);

            // Picture stuff
            uint picOffs = data.GetUInt32(offs + 0xC) & ~0x10000000u;

            uint thing = data.GetUInt32((int)picOffs);
            if (thing == 0x2F4D6170) // File path
            {
                tilesetPath = data.GetString((int)picOffs);
            }
            else // Raw data
            {
                uint[] info = this.getInfo();
                //int picLen = (int)(info[1] * info[2] * ((info[0] == 6) ? 4 : 2));
                int picLen = (int)(resolutionStuff * ((info[0] == 5) ? 32 : 64));

                rawPicture = data.Skip((int)picOffs).Take(picLen).ToArray();
            }

            Globals.gxInstancesDict.Add(offsetIdentifier, this);
        }

        public uint[] getInfo()
        {
            uint format = (formatStuff >> 20) & 0b111; // 5 is RGB5A3, 6 is RGBA8
            uint height = ((formatStuff >> 10) & 0b1111111111) + 1;
            uint width = (formatStuff & 0b1111111111) + 1;

            return new uint[] { format, width, height };
        }
    }

    public class dKPMapFile_s
    {
        public uint magic = 0xFFFFFFFF;
        public int version = -1;

        public List<dKPLayer_s> layers = new List<dKPLayer_s>();
        public List<GXTexObj> tilesets = new List<GXTexObj>();

        public UnlockData unlockData = new UnlockData();

        public List<ushort[,]> sectors = new List<ushort[,]>(); // 16*16

        public string backgroundName = "";

        public List<dKPWorldDef_s> worldDefs = new List<dKPWorldDef_s>();


        public bool load(string path)
        {
            // Check for file & read it
            if(!File.Exists(path))
            {
                Console.WriteLine("Can't find \"" + path + "\": No such file or directory.");
                return false;
            }

            byte[] data = File.ReadAllBytes(path);
            bool isLZ = path.EndsWith(".LZ");
            bool isLH = path.EndsWith(".LH");

            if (isLZ || isLH)
            {
                byte[] src = data.ToArray();
                int error = 0;
                if (isLZ)
                {
                    data = new byte[LZDecompressor.getDecompSize(src)];
                    error = LZDecompressor.decomp(ref data, src);
                }
                else // isLH
                {
                    data = new byte[LHDecompressor.getDecompSize(src)];
                    error = LHDecompressor.decomp(ref data, src);
                }

                if (error != 0)
                {
                    Console.Write("Failed to decompress " + Path.GetFileName(path) + ": Error " + error);
                    string nonLHPath = path.Substring(0, path.Length - 3);
                    if (File.Exists(nonLHPath))
                    {
                        Console.WriteLine(", falling back to " + nonLHPath);
                        data = File.ReadAllBytes(nonLHPath);
                    }
                    else
                    {
                        Console.WriteLine(", aborting.");
                        return false;
                    }
                }
            }

            Console.WriteLine("File read.");

            // Reset everything
            magic = 0xFFFFFFFF;
            version = -1;
            layers = new List<dKPLayer_s>();
            tilesets = new List<GXTexObj>();
            unlockData = new UnlockData();
            sectors = new List<ushort[,]>(); // 16*16
            backgroundName = "";
            worldDefs = new List<dKPWorldDef_s>();


            // Parsing
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            magic = data.GetUInt32(0);
            if(magic != 0x4B505F6D) // "KP_m"
            {
                Console.WriteLine("Invalid KPBIN File.");
                return false;
            }

            version = (int)data.GetUInt32(4);
            if(version != 2)
            {
                Console.WriteLine("Unsupported version " + version + ".");
                return false;
            }


            Console.WriteLine("Parsing layers...");

            int layerCount = data.GetInt32(8);
            uint layerTableOffs = data.GetUInt32(0xC);
            Console.WriteLine("  " + layerCount + " layers found:");
            Console.Write("    ");
            int consolePosL = Console.CursorLeft;
            int consolePosT = Console.CursorTop;
            for (int layerNum = 0; layerNum < layerCount; layerNum++)
            {
                uint layerOffs = data.GetUInt32((int)(layerTableOffs + (layerNum * 4)));

                dKPLayer_s layer = new dKPLayer_s();
                if (Globals.layerInstancesDict.ContainsKey(layerOffs)) layer = Globals.layerInstancesDict[layerOffs];
                else layer.parseFromData(ref data, (int)layerOffs);
                layers.Add(layer);

                Console.SetCursorPosition(consolePosL, consolePosT);
                Console.WriteLine((layerNum+1) + "/" + layerCount);
            }


            Console.WriteLine("Parsing tilesets...");

            int tilesetCount = data.GetInt32(0x10);
            uint tilesetTableOffs = data.GetUInt32(0x14);
            Console.WriteLine("  " + tilesetCount + " tilesets found:");
            Console.Write("    ");
            consolePosL = Console.CursorLeft;
            consolePosT = Console.CursorTop;
            for (int tilesetNum = 0; tilesetNum < tilesetCount; tilesetNum++)
            {
                uint tilesetOffs = (uint)(tilesetTableOffs + (tilesetNum * 0x20));

                GXTexObj tileset = new GXTexObj();
                if (Globals.gxInstancesDict.ContainsKey(tilesetOffs)) tileset = Globals.gxInstancesDict[tilesetOffs];
                else
                {
                    tileset.parseFromData(ref data, (int)tilesetOffs);
                }
                tilesets.Add(tileset);

                Console.SetCursorPosition(consolePosL, consolePosT);
                Console.WriteLine((tilesetNum + 1) + "/" + tilesetCount);
            }


            Console.WriteLine("Parsing unlock data...");

            uint unlockDataTableOffs = data.GetUInt32(0x18);
            unlockData.parseFromData(ref data, (int)unlockDataTableOffs);
            Console.WriteLine("  Parsed " + unlockData.unlockCriterias.Count + " unlock criterias.");


            Console.WriteLine("Parsing sectors...");

            uint sectorsTableOffs = data.GetUInt32(0x1C);
            List<ushort> sectorIndexes = new List<ushort>();
            foreach(dKPLayer_s layer in layers)
            {
                if(layer.type == dKPLayer_s.LayerTypes.OBJECTS)
                {
                    foreach(ushort sectorIndex in layer.indices)
                    {
                        if (sectorIndex != 0xFFFF && !sectorIndexes.Contains(sectorIndex)) sectorIndexes.Add(sectorIndex);
                    }
                }
            }
            int maxIdx = sectorIndexes.Max() + 1;
            Console.WriteLine("  " + maxIdx + " sectors found:");
            Console.Write("    ");
            consolePosL = Console.CursorLeft;
            consolePosT = Console.CursorTop;
            for (int i = 0; i < maxIdx; i++) {
                ushort[,] sector = new ushort[16,16];

                for (int y = 0; y < 16; y++) {
                    for (int x = 0; x < 16; x++)
                    {
                        sector[x,y] = data.GetUInt16((int)(sectorsTableOffs + (((x * 16) + y) * 2)));
                    }
                }

                sectorsTableOffs += 0x200;

                sectors.Add(sector);

                Console.SetCursorPosition(consolePosL, consolePosT);
                Console.WriteLine((i + 1) + "/" + maxIdx);
            }


            Console.WriteLine("Parsing background name...");

            uint backgroundNameOffs = data.GetUInt32(0x20);
            backgroundName = data.GetString((int)backgroundNameOffs);
            Console.WriteLine("  \"" + backgroundName + "\"");


            Console.WriteLine("Parsing world definitions...");

            int worldDefCount = data.GetInt32(0x28);
            uint worldDefTableOffs = data.GetUInt32(0x24);
            Console.WriteLine("  " + worldDefCount + " world definitions found:");
            Console.Write("    ");
            consolePosL = Console.CursorLeft;
            consolePosT = Console.CursorTop;
            for (int worldDefNum = 0; worldDefNum < worldDefCount; worldDefNum++)
            {
                int worldDefOffs = (int)(worldDefTableOffs + (worldDefNum * 0x28));

                dKPWorldDef_s worldDef = new dKPWorldDef_s();
                worldDef.parseFromData(ref data, worldDefOffs);
                worldDefs.Add(worldDef);

                Console.SetCursorPosition(consolePosL, consolePosT);
                Console.WriteLine((worldDefNum + 1) + "/" + worldDefCount);
            }

            long time2 = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            TimeSpan t = TimeSpan.FromMilliseconds(time2 - time);
            string timeStr = string.Join(" ", string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds).Split(':').SkipWhile(s => Regex.Match(s, @"00\w").Success).ToArray());
            Console.WriteLine("Parsed! Took " + timeStr);
            return true;
        }
    }

    public static class ByteArrayExtension
    {
        public static uint GetUInt32(this byte[] data, int index)
        {
            //return BitConverter.ToUInt32(data.Skip(index).Take(4).Reverse().ToArray(), 0);
            return (uint)((data[index] << 24) | (data[index + 1] << 16) | (data[index + 2] << 8) | (data[index + 3]));
        }

        public static int GetInt32(this byte[] data, int index)
        {
            //return BitConverter.ToInt32(data.Skip(index).Take(4).Reverse().ToArray(), 0);
            return (int)((data[index] << 24) | (data[index + 1] << 16) | (data[index + 2] << 8) | (data[index + 3]));
        }

        public static ushort GetUInt16(this byte[] data, int index)
        {
            //return BitConverter.ToUInt16(data.Skip(index).Take(2).Reverse().ToArray(), 0);
            return (ushort)((data[index] << 8) | (data[index + 1]));
        }

        public static short GetInt16(this byte[] data, int index)
        {
            //return BitConverter.ToInt16(data.Skip(index).Take(2).Reverse().ToArray(), 0);
            return (short)((data[index] << 8) | (data[index + 1]));
        }

        public static float GetFloat(this byte[] data, int index)
        {
            return BitConverter.ToSingle(data.Skip(index).Take(4).Reverse().ToArray(), 0);
        }

        public static string GetString(this byte[] data, int index)
        {
            string str = "";
            while(data[index] != 0)
                str += (char)data[index++];

            return str;
        }
    }

    public static class IntListExtensions
    {
        public static bool ContainsSequence(this List<int[]> list, int[] item)
        {
            return list.Any(p => p.SequenceEqual(item));
        }

        public static int[] GetSequence(this List<int[]> list, int[] item)
        {
            return list.Find(p => p.SequenceEqual(item));
        }
    }
}
