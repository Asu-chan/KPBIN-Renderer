using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

/******************************************************************************************************/
/* This program allows to render NewerSMBW's KPBIN files as pictures using the resources they require */
/******************************* Programmed by RedStoneMatt / Asu-chan ********************************/
/******************************************************************************************************/

// ~~Stolen~~ borrowed code pieces belong to their respective owners :)

namespace KPBIN_Renderer
{
    class Program
    {
        // Hardcoded tileset texture size
        static readonly int gwidth = 768;
        static readonly int gheight = 384;

        // Bitmap dictionaries; avoids rendering the same thing twice
        static Dictionary<uint, Bitmap> gxPicsDict = new Dictionary<uint, Bitmap>();
        static Dictionary<uint, Bitmap> actDoodadsPicsDict = new Dictionary<uint, Bitmap>();
        static Dictionary<int, Bitmap> layerPicsDict = new Dictionary<int, Bitmap>();
        static Dictionary<int, Point> layerPosDict = new Dictionary<int, Point>();

        // World boundaries (updated each time an element goes beyond them)
        static int minWorldX = 0x7FFFFFFF;
        static int maxWorldX = -1;
        static int minWorldY = 0x7FFFFFFF;
        static int maxWorldY = -1;
        
        // KPBIN instance; structures from NewerSMBW headers
        static dKPMapFile_s kpbin = new dKPMapFile_s();

        // Unlock data as an array of int arrays. Works as follows:
        // Each int array represents an entry, with the structure:
        // For levels:
        //     { world, level, 0, 1 } -> level only has normal exit and is completed
        //     { world, level, 1, 1 } -> level only has secret exit and is completed
        //     { world, level, 2, 1, 1 } -> level only has both exits and both are completed
        // For comparisons:
        //     { 255, 0, 100 } -> total star coins = 100
        //     { 255, 1, 100 } -> unspent star coins = 100
        static int[][] udatas = new int[0][];

        // Layer visibilities as an array of int arrays. Works as follows:
        // Each int array represents a layer's visibility and its index is the corresponding layer ID, with the structure:
        //     { layer.type, layer.visible }
        // Note: the last entry corresponds to the visibility of path lines & misc nodes
        static int[][] layerVisibilities = new int[0][];
        static bool pathLinesVisible = false;

        // Export paths for the different elements of the KPBIN. Leaving one blank makes corresponding element not be saved
        static string tilesetsExportPath = "";
        static string doodadsExportPath = "";
        static string layersExportPath = "";
        static string worldmapExportPath = "";

        // Worldmap filename, whether it's LH or not is autodetected during parsing
        static string worldmapFilename = "";

        // Base image format
        static ImageFormat imageFormat = ImageFormat.Png;

        // Maps folder, duh
        static string mapsFolder = "";


        [STAThread] // Necessary for folder selecting lib, sorry :c
        public static void Main(string[] args)
        {
            // Fancy title
            Console.Title = "KPBIN Renderer - " + Application.ExecutablePath;

            // Make the forms not look like Windows 95
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Actual stuff
            Console.WriteLine("Please select your Maps folder.");

            mapsFolder = askForFolder("Select your Maps folder");
            if(mapsFolder == "")
            {
                Console.WriteLine("Maps folder selection cancelled. Aborting.");
                return;
            }

            // If there's no KPBIN, or no Texture folder, or no List.txt, then it's not an actual Maps folder
            if(Directory.GetFiles(mapsFolder, "*.kpbin.*").Length == 0 || !Directory.Exists(mapsFolder + "Texture\\") || !File.Exists(mapsFolder + "List.txt"))
            {
                Console.WriteLine("This is not a valid Maps folder.");
                return;
            }

            Console.WriteLine("Selected Maps folder: \"" + mapsFolder + "\"");

            // Get KPBIN name & what to export
            if (!askForOptions()) return;

            // Returns true if parsing went correctly
            if (kpbin.load(mapsFolder + worldmapFilename))
            {
                Console.WriteLine("Please fill the unlock criterias.");
                askForUData();
                Console.WriteLine("Decoding graphics...");
                if(!decodeGXTextures()) return;
                Console.WriteLine("Removing invisible node & path layers...");
                setPathLayersVisibility();
                Console.WriteLine("Please fill the layer visibilities.");
                askForLayersVisibility();
                Console.WriteLine("Rendering layers...");
                renderLayers();
                Console.WriteLine("Rendering worldmap...");
                blendLayers();

                Console.WriteLine("All done!");
            }

            Console.WriteLine("Press any key to continue... ");
            Console.ReadLine();
        }


        public static string askForFolder(string title)
        {

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = title;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName + "\\";
            }
            else
            {
                return "";
            }
        }


        public static string askForFile(string title)
        {

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Portable Network Graphics|*.png|Joint Photographic Experts Group|*.jpg|Graphics Interchange Format|*.gif|Bitmap Image file|*.bmp|Tagged Image File Format|*.tiff";
            dialog.Title = title;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.FileName;
            }
            else
            {
                return "";
            }
        }


        public static bool askForOptions()
        {
            using (MapAndOptionsForm mapAndOptionsForm = new MapAndOptionsForm())
            {
                mapAndOptionsForm.mapsFolder = mapsFolder;

                if (mapAndOptionsForm.ShowDialog() == DialogResult.OK)
                {
                    tilesetsExportPath = mapAndOptionsForm.tilesetsExportPath;
                    doodadsExportPath = mapAndOptionsForm.doodadsExportPath;
                    layersExportPath = mapAndOptionsForm.layersExportPath;
                    worldmapExportPath = mapAndOptionsForm.worldmapExportPath;

                    imageFormat = mapAndOptionsForm.imageFormat;

                    worldmapFilename = mapAndOptionsForm.worldmapFilename;
                }
                else
                {
                    Console.WriteLine("Cancelled.");
                    return false;
                }
            }
            return true;
        }


        public static void askForUData()
        {
            udatas = new int[0][];
            using (UnlockDataForm unlockDataForm = new UnlockDataForm())
            {
                for (int layerID = 0; layerID < kpbin.layers.Count; layerID++)
                {
                    dKPLayer_s layer = kpbin.layers[layerID];
                    if (layer.type == dKPLayer_s.LayerTypes.PATHS)
                    {
                        foreach(dKPNode_s node in layer.nodes)
                        {
                            if(node.type == dKPNode_s.NodeTypes.LEVEL && node.levelNumber != 80)
                            {
                                if (node.hasSecret)
                                    udatas = udatas.Append(new int[] { node.worldNumber - 1, node.levelNumber - 1, 2, 1, 1 }).ToArray();
                                else
                                    udatas = udatas.Append(new int[] { node.worldNumber - 1, node.levelNumber - 1, 0, 1 }).ToArray();
                            }
                        }

                        break;
                    }
                }
                
                
                unlockDataForm.udatas = kpbin.unlockData.GetAsUdatas(ref udatas);

                if (unlockDataForm.ShowDialog() == DialogResult.OK)
                {
                    udatas = unlockDataForm.udatas;
                }
                else
                {
                    Console.WriteLine("Cancelled.");
                    Console.ReadLine();
                    return;
                }
            }
        }


        public static Dictionary<ImageFormat, string> extensions = new Dictionary<ImageFormat, string> { [ImageFormat.Png] = ".png", [ImageFormat.Jpeg] = ".jpg", [ImageFormat.Gif] = ".gif", [ImageFormat.Bmp] = ".bmp", [ImageFormat.Tiff] = ".tiff" };

        public static void decodeGXTexture(GXTexObj texObj)
        {
            uint texKey = texObj.offsetIdentifier;
            if (!gxPicsDict.ContainsKey(texObj.offsetIdentifier))
            {
                if (texObj.tilesetPath != "")
                {
                    uint[] tilesetProps = texObj.getInfo();

                    int width = (int)tilesetProps[1];
                    int height = (int)tilesetProps[2];

                    Bitmap output = new Bitmap(width, height);

                    bool isLZ = false;
                    bool isLH = false;

                    string tilesetiPath = texObj.tilesetPath.Substring(6);
                    string tilesetPath = mapsFolder + tilesetiPath.Replace('/', '\\');

                    if (File.Exists(tilesetPath + ".LZ")) isLZ = true; // LZ has priority over LH
                    else if (File.Exists(tilesetPath + ".LH")) isLH = true;

                    else if (!File.Exists(tilesetPath)) MessageBox.Show("Cannot find tileset \"" + tilesetiPath + "\": No such file or directory");

                    string realTilesetPath = tilesetPath + ((isLZ) ? ".LZ" : ((isLH) ? ".LH" : ""));
                    byte[] tilesetRaw = File.ReadAllBytes(realTilesetPath);
                    if(isLZ || isLH)
                    {
                        byte[] compressedTileset = tilesetRaw.ToArray();
                        int error = 0;
                        if (isLZ)
                        {
                            tilesetRaw = new byte[LZDecompressor.getDecompSize(compressedTileset)];
                            error = LZDecompressor.decomp(ref tilesetRaw, compressedTileset);
                        }
                        else // isLH
                        {
                            tilesetRaw = new byte[LHDecompressor.getDecompSize(compressedTileset)];
                            error = LHDecompressor.decomp(ref tilesetRaw, compressedTileset);
                        }

                        if(error != 0)
                        {
                            Console.Write("Failed to decompress " + Path.GetFileName(realTilesetPath) + ": Error " + error);
                            if (File.Exists(tilesetPath)) {
                                Console.WriteLine(", falling back to " + tilesetiPath);
                                tilesetRaw = File.ReadAllBytes(tilesetPath);
                            }
                            else
                            {
                                Console.WriteLine(", aborting.");
                                return;
                            }
                        }
                    }

                    if (tilesetProps[0] == 6) output = BitmapUtils.RGBA8Decode(tilesetRaw, width, height);
                    else output = BitmapUtils.RGB5A3Decode(tilesetRaw, width, height);


                    Bitmap actOutput = new Bitmap(gwidth, gheight);

                    for (int x = 0; x < actOutput.Width; x++)
                        for (int y = 0; y < actOutput.Height; y++)
                            actOutput.SetPixel(x, y, Color.Transparent);


                    int gx = 0, gy = 0;
                    for (int x = 0; x < output.Width; x++)
                    {
                        if (x % 28 < 2 || x % 28 > 25) continue;
                        gy = 0;
                        for (int y = 0; y < output.Height; y++)
                        {
                            if (y % 28 < 2 || y % 28 > 25) continue;

                            actOutput.SetPixel(gx, gy, output.GetPixel(x, y));
                            gy++;
                        }
                        gx++;
                    }

                    gxPicsDict.Add(texKey, actOutput);
                }
                else if (texObj.rawPicture.Length > 0)
                {
                    if (!gxPicsDict.ContainsKey(texKey))
                    {
                        uint[] doodadProps = texObj.getInfo();

                        int width = (int)doodadProps[1];
                        int height = (int)doodadProps[2];

                        Bitmap output = new Bitmap(width, height);

                        if (doodadProps[0] == 6) output = BitmapUtils.RGBA8Decode(texObj.rawPicture.ToArray(), width, height);
                        else output = BitmapUtils.RGB5A3Decode(texObj.rawPicture.ToArray(), width, height);

                        gxPicsDict.Add(texKey, output);
                    }
                }
                else
                {
                    Console.WriteLine("Critical error with GX " + texKey.ToString("X") + ": Not doodad not tileset");
                }
            }
        }


        public static bool decodeGXTextures()
        {
            int di = 0;
            int ti = 0;
            foreach (KeyValuePair<uint, GXTexObj> GXEntry in Globals.gxInstancesDict)
            {
                GXTexObj texObj = GXEntry.Value;
                uint texKey = GXEntry.Key;
                decodeGXTexture(texObj);

                if (gxPicsDict.ContainsKey(texKey))
                {
                    if (texObj.tilesetPath != "")
                    {
                        if (tilesetsExportPath != "")
                            gxPicsDict[texKey].Save(tilesetsExportPath + @"t_" + texKey.ToString("X") + "_" + texObj.tilesetPath.Substring(14, texObj.tilesetPath.Length - 18) + extensions[imageFormat], imageFormat);

                        ti++;

                        Console.WriteLine("Decoded Tileset " + ti);
                    }
                    else if (texObj.rawPicture.Length > 0)
                    {
                        if (doodadsExportPath != "")
                            gxPicsDict[texKey].Save(doodadsExportPath + @"d_" + texKey.ToString("X") + extensions[imageFormat], imageFormat);

                        di++;

                        Console.WriteLine("Decoded Doodad " + di);
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }


        public static void setPathLayersVisibility()
        {
            for(int layerID = 0; layerID < kpbin.layers.Count; layerID++)
            {
                dKPLayer_s layer = kpbin.layers[layerID];
                if (layer.type == dKPLayer_s.LayerTypes.PATHS)
                {
                    layer.alpha = 255;
                    layer.visible = true;
                    foreach (dKPNode_s node in layer.nodes)
                    {
                        bool valueForNode = false;

                        bool[] bools = new bool[] { node.hasLeftPath, node.hasRightPath, node.hasUpPath, node.hasDownPath };
                        dKPPath_s[] paths = new dKPPath_s[] { node.leftExit, node.rightExit, node.upExit, node.downExit };
                        for(int i = 0; i < 4; i++)
                        {
                            if(bools[i])
                            {
                                ushort pathID = (ushort)layer.paths.IndexOf(paths[i]);
                                bool exitValue = kpbin.unlockData.IsPathAvailable(pathID, udatas);
                                valueForNode |= exitValue;
                                Console.WriteLine("Checking conditions for pathID " + pathID + " -> " + exitValue);

                                if (!exitValue) { paths[i].tileLayer.visible = false; paths[i].doodadLayer.visible = false; }
                            }
                        }

                        if (!valueForNode) { node.tileLayer.visible = false; node.doodadLayer.visible = false; }

                        if (node.levelNumber == 80) node.visible = false; // World 1 Cutscene node
                        if (node.type == dKPNode_s.NodeTypes.LEVEL)
                        {
                            if (valueForNode)
                            {
                                UnlockCriteria levelCriteria = new UnlockCriteria();
                                byte world = (byte)(node.worldNumber - 1);
                                byte level = (byte)(node.levelNumber - 1);

                                levelCriteria.criteria = new byte[] { (byte)((1 << 6) | world), level }; // Normal
                                bool normalClear = levelCriteria.IsCriteriaTrue(udatas);
                                bool secretClear = false;
                                if (node.hasSecret)
                                {
                                    levelCriteria.criteria = new byte[] { (byte)((1 << 6) | 0x10 | world), level }; // Secret
                                    secretClear = levelCriteria.IsCriteriaTrue(udatas);
                                }

                                //Console.WriteLine("worldNumber " + (node.worldNumber) + "-" + (node.levelNumber) + " " + node.hasSecret + " -> " + normalClear + " " + secretClear);
                                
                                // One-time levels
                                if (node.levelNumber >= 30 && node.levelNumber <= 37)
                                {
                                    if(normalClear)
                                        node.nodeColor = dKPNode_s.NodeColor.BLACK;
                                    else
                                        node.nodeColor = dKPNode_s.NodeColor.RED;
                                }
                                // Shops
                                else if (node.levelNumber == 99) node.nodeColor = dKPNode_s.NodeColor.SHOP;
                                // Regular levels
                                else if (node.hasSecret)
                                {
                                    if (normalClear && secretClear)
                                        node.nodeColor = dKPNode_s.NodeColor.BLUE;
                                    else if (normalClear || secretClear)
                                        node.nodeColor = dKPNode_s.NodeColor.PURPLE;
                                    else
                                        node.nodeColor = dKPNode_s.NodeColor.RED;
                                }
                                else
                                {
                                    if (normalClear)
                                        node.nodeColor = dKPNode_s.NodeColor.BLUE;
                                    else
                                        node.nodeColor = dKPNode_s.NodeColor.RED;
                                }
                            }
                            else node.nodeColor = dKPNode_s.NodeColor.BLACK;
                        }
                    }


                    break;
                }
            }
        }


        public static void askForLayersVisibility()
        {
            layerVisibilities = new int[kpbin.layers.Count + 1][];
            for(int layerID = 0; layerID < kpbin.layers.Count; layerID++)
            {
                dKPLayer_s layer = kpbin.layers[layerID];
                layerVisibilities[layerID] = new int[] { (int)layer.type, (layer.visible) ? 1 : 0 };
            }
            layerVisibilities[kpbin.layers.Count] = new int[] { -1, pathLinesVisible ? 1 : 0 };

            using (LayerVisibilityForm layerVisibilityForm = new LayerVisibilityForm())
            {
                layerVisibilityForm.layerVisibilities = layerVisibilities;

                if (layerVisibilityForm.ShowDialog() == DialogResult.OK)
                {
                    layerVisibilities = layerVisibilityForm.layerVisibilities;
                }
                else
                {
                    Console.WriteLine("Cancelled.");
                    Console.ReadLine();
                    return;
                }
            }

            for (int layerID = 0; layerID < kpbin.layers.Count; layerID++)
            {
                dKPLayer_s layer = kpbin.layers[layerID];
                layer.visible = layerVisibilities[layerID][1] > 0;
            }
            pathLinesVisible = layerVisibilities[kpbin.layers.Count][1] > 0;
        }


        public static void renderLayer(int layerID)
        {
            dKPLayer_s layer = kpbin.layers[layerID];
            if (!layer.visible)
            {
                Console.WriteLine("Skipped layer " + layerID + " (invisible)");
                return;
            }

            switch (layer.type)
            {
                case dKPLayer_s.LayerTypes.OBJECTS:

                    Bitmap tileset = gxPicsDict[layer.tileset.offsetIdentifier];

                    int toRenderMinX = layer.boundLeft;
                    int toRenderMinY = layer.boundTop;

                    int toRenderMaxX = layer.boundRight;
                    int toRenderMaxY = layer.boundBottom;

                    int sectorBaseX = layer.sectorLeft;
                    int sectorBaseY = layer.sectorTop;
                    int sectorIndexStride = (layer.sectorRight - layer.sectorLeft + 1);

                    int sectorMinX = toRenderMinX / 16;
                    int sectorMinY = toRenderMinY / 16;
                    int sectorMaxX = toRenderMaxX / 16;
                    int sectorMaxY = toRenderMaxY / 16;

                    int worldLeft = (sectorMinX << 4) * 24;
                    int worldRight = (sectorMaxX << 4) * 24 + 384;
                    int worldTop = (sectorMinY << 4) * 24;
                    int worldBottom = (sectorMaxY << 4) * 24 + 384;
                    //Console.WriteLine("Rendering " + sectorMinX + ";" + sectorMinY + " (" + worldLeft + ";" + worldTop + ") to " + sectorMaxX + ";" + sectorMaxY + " (" + worldRight + ";" + worldBottom + ")");

                    Bitmap tilemap = new Bitmap(worldRight - worldLeft, worldBottom - worldTop);

                    if (worldLeft < minWorldX) minWorldX = worldLeft;
                    if (worldRight > maxWorldX) maxWorldX = worldRight;
                    if (worldTop < minWorldY) minWorldY = worldTop;
                    if (worldBottom > maxWorldY) maxWorldY = worldBottom;

                    for (int sectorY = sectorMinY; sectorY <= sectorMaxY; sectorY++)
                    {

                        int baseIndex = (sectorY - sectorBaseY) * sectorIndexStride;

                        int iMinY = (sectorY == sectorMinY) ? (toRenderMinY & 0xF) : 0;
                        int iMaxY = (sectorY == sectorMaxY) ? (toRenderMaxY & 0xF) : 15;

                        int worldSectorY = sectorY << 4;

                        for (int sectorX = sectorMinX; sectorX <= sectorMaxX; sectorX++)
                        {
                            ushort index = layer.indices[baseIndex + sectorX - sectorBaseX];
                            if (index == 0xFFFF)
                                continue;

                            ushort[,] sector = kpbin.sectors[index];

                            int iMinX = (sectorX == sectorMinX) ? (toRenderMinX & 0xF) : 0;
                            int iMaxX = (sectorX == sectorMaxX) ? (toRenderMaxX & 0xF) : 15;
                            int worldSectorX = sectorX << 4;



                            for (int inY = iMinY; inY <= iMaxY; inY++)
                            {
                                for (int inX = iMinX; inX <= iMaxX; inX++)
                                {
                                    ushort tileID = sector[inY, inX];
                                    if (tileID == 0xFFFF)
                                        continue;

                                    short worldX = (short)((worldSectorX | inX) * 24);
                                    short worldY = (short)(-((worldSectorY | inY) * 24));

                                    //Console.WriteLine("Layer " + layerID + ", Sector " + sectorX + ";" + sectorY + " rendering tile " + inX + ";" + inY + " of ID " + tileID + " at " + worldX + ";" + worldY);

                                    tilemap.PlaceTile(tileset, tileID, worldX - worldLeft, -worldY - worldTop);
                                }
                            }
                        }
                    }

                    tilemap = BitmapExtensions.SetImageOpacity(tilemap, layer.alpha);

                    layerPicsDict.Add(layerID, tilemap);
                    layerPosDict.Add(layerID, new Point(worldLeft, worldTop));

                    Console.WriteLine("Rendered Layer " + layerID + " (OBJECTS)");
                    break;

                case dKPLayer_s.LayerTypes.DOODADS:
                    int minLayerX = 0x7FFFFFFF;
                    int maxLayerX = -1;
                    int minLayerY = 0x7FFFFFFF;
                    int maxLayerY = -1;

                    for (int doodadID = 0; doodadID < layer.doodads.Count; doodadID++)
                    {
                        dKPDoodad_s doodad = layer.doodads[doodadID];

                        int doodadX = (int)Math.Round(doodad.x);
                        int doodadY = (int)Math.Round(doodad.y);
                        int doodadWidth = (int)Math.Round(doodad.width);
                        int doodadHeight = (int)Math.Round(doodad.height);

                        Bitmap processedDoodad = BitmapExtensions.RotateImage(BitmapExtensions.ResizeBitmap(gxPicsDict[doodad.texObj.offsetIdentifier], doodadWidth, doodadHeight), doodad.angle);
                        actDoodadsPicsDict.Add((uint)(layerID << 16 | doodadID), processedDoodad);

                        doodadX = (doodadX + (doodadWidth / 2)) - (processedDoodad.Width / 2);
                        doodadY = (doodadY + (doodadHeight / 2)) - (processedDoodad.Height / 2);

                        doodadWidth = processedDoodad.Width;
                        doodadHeight = processedDoodad.Height;

                        if (doodadX < minWorldX) minWorldX = doodadX;
                        if (doodadX + doodadWidth > maxWorldX) maxWorldX = doodadX + doodadWidth;
                        if (doodadY < minWorldY) minWorldY = doodadY;
                        if (doodadY + doodadHeight > maxWorldY) maxWorldY = doodadY + doodadHeight;

                        if (doodadX < minLayerX) minLayerX = doodadX;
                        if (doodadX + doodadWidth > maxLayerX) maxLayerX = doodadX + doodadWidth;
                        if (doodadY < minLayerY) minLayerY = doodadY;
                        if (doodadY + doodadHeight > maxLayerY) maxLayerY = doodadY + doodadHeight;
                    }

                    Bitmap doodmap = new Bitmap(maxLayerX - minLayerX, maxLayerY - minLayerY);

                    for (int doodadID = 0; doodadID < layer.doodads.Count; doodadID++)
                    {
                        dKPDoodad_s doodad = layer.doodads[doodadID];

                        Bitmap processedDoodad = actDoodadsPicsDict[(uint)(layerID << 16 | doodadID)];

                        int doodadX = (int)Math.Round(doodad.x);
                        int doodadY = (int)Math.Round(doodad.y);
                        int doodadWidth = (int)Math.Round(doodad.width);
                        int doodadHeight = (int)Math.Round(doodad.height);

                        doodadX = (doodadX + (doodadWidth / 2)) - (processedDoodad.Width / 2);
                        doodadY = (doodadY + (doodadHeight / 2)) - (processedDoodad.Height / 2);

                        doodmap.BlendBitmap(processedDoodad, doodadX - minLayerX, doodadY - minLayerY);
                    }

                    doodmap = BitmapExtensions.SetImageOpacity(doodmap, layer.alpha);

                    layerPicsDict.Add(layerID, doodmap);
                    layerPosDict.Add(layerID, new Point(minLayerX, minLayerY));

                    Console.WriteLine("Rendered Layer " + layerID + " (DOODADS)");
                    break;

                case dKPLayer_s.LayerTypes.PATHS:
                    int minPLayerX = 0x7FFFFFFF;
                    int maxPLayerX = -1;
                    int minPLayerY = 0x7FFFFFFF;
                    int maxPLayerY = -1;

                    for (int nodeID = 0; nodeID < layer.nodes.Count; nodeID++)
                    {
                        dKPNode_s node = layer.nodes[nodeID];

                        int nodeX = node.x - 25;
                        int nodeY = node.y - 19;
                        int nodeWidth = 51;
                        int nodeHeight = 39;

                        if (nodeX < minWorldX) minWorldX = nodeX;
                        if (nodeX + nodeWidth > maxWorldX) maxWorldX = nodeX + nodeWidth;
                        if (nodeY < minWorldY) minWorldY = nodeY;
                        if (nodeY + nodeHeight > maxWorldY) maxWorldY = nodeY + nodeHeight;

                        if (nodeX < minPLayerX) minPLayerX = nodeX;
                        if (nodeX + nodeWidth > maxPLayerX) maxPLayerX = nodeX + nodeWidth;
                        if (nodeY < minPLayerY) minPLayerY = nodeY;
                        if (nodeY + nodeHeight > maxPLayerY) maxPLayerY = nodeY + nodeHeight;
                    }

                    Bitmap pathmap = new Bitmap(maxPLayerX - minPLayerX, maxPLayerY - minPLayerY);

                    if (pathLinesVisible)
                    {
                        for (int pathID = 0; pathID < layer.paths.Count; pathID++)
                        {
                            dKPPath_s path = layer.paths[pathID];

                            pathmap.DrawLineInt(Color.Black, new Point(path.start.x - minPLayerX, path.start.y - minPLayerY), new Point(path.end.x - minPLayerX, path.end.y - minPLayerY), 3);
                        }
                    }

                    Random rnd = new Random();
                    for (int nodeID = 0; nodeID < layer.nodes.Count; nodeID++)
                    {
                        dKPNode_s node = layer.nodes[nodeID];
                        int tx = node.x - minPLayerX;
                        int ty = node.y - minPLayerY;

                        switch (node.type)
                        {
                            case dKPNode_s.NodeTypes.PASS_THROUGH:
                                if (pathLinesVisible)
                                    pathmap.DrawPTNode(Color.FromArgb(255, rnd.Next(255), rnd.Next(255), rnd.Next(255)), new Point(tx, ty));
                                break;
                            case dKPNode_s.NodeTypes.STOP:
                                if (pathLinesVisible)
                                    pathmap.DrawSNode(Color.FromArgb(255, rnd.Next(255), rnd.Next(255), rnd.Next(255)), new Point(tx, ty));
                                break;
                            case dKPNode_s.NodeTypes.LEVEL:
                                if (node.visible)
                                {
                                    Bitmap levelcolor = new Bitmap(1, 1);
                                    switch (node.nodeColor)
                                    {
                                        case dKPNode_s.NodeColor.BLUE:
                                            levelcolor = Properties.Resources.level_blue;
                                            break;
                                        case dKPNode_s.NodeColor.RED:
                                            levelcolor = Properties.Resources.level_red;
                                            break;
                                        case dKPNode_s.NodeColor.PURPLE:
                                            levelcolor = Properties.Resources.level_purple;
                                            break;
                                        case dKPNode_s.NodeColor.BLACK:
                                            levelcolor = Properties.Resources.level_black;
                                            break;
                                        case dKPNode_s.NodeColor.SHOP:
                                            levelcolor = Properties.Resources.level_shop;
                                            break;
                                    }

                                    pathmap.BlendBitmap(levelcolor, tx - 25, ty - 21);
                                }

                                if (pathLinesVisible)
                                    pathmap.DrawSNode(Color.FromArgb(255, rnd.Next(255), rnd.Next(255), rnd.Next(255)), new Point(tx, ty));

                                break;
                            case dKPNode_s.NodeTypes.CHANGE:
                                if (pathLinesVisible)
                                {
                                    Bitmap mapchnid = new Bitmap(Properties.Resources.changemap);
                                    mapchnid.WriteTextAt(new Rectangle(0, 0, 31, 20), "Calibri", Color.FromArgb(255, 140, 140, 255), node.thisID.ToString());
                                    pathmap.BlendBitmap(mapchnid, tx - 15, ty - 10);
                                }
                                break;
                            case dKPNode_s.NodeTypes.WORLD_CHANGE:
                                if (pathLinesVisible)
                                {
                                    Bitmap worldchnid = new Bitmap(Properties.Resources.changeworld);
                                    worldchnid.WriteTextAt(new Rectangle(0, 3, 29, 22), "Calibri", Color.FromArgb(255, 0, 140, 255), node.worldID.ToString());
                                    pathmap.BlendBitmap(worldchnid, tx - 12, ty - 15);
                                }
                                break;
                                
                        }
                    }

                    layerPicsDict.Add(layerID, pathmap);
                    layerPosDict.Add(layerID, new Point(minPLayerX, minPLayerY));

                    Console.WriteLine("Rendered Layer " + layerID + " (PATHS)");

                    break;
            }
        }


        public static readonly string[] layerType2String = new string[3] { "t", "d", "p" };

        public static void renderLayers()
        {
            for (int layerID = 0; layerID < kpbin.layers.Count; layerID++)
            {
                renderLayer(layerID);
                if(layersExportPath != "")
                {
                    if(layerPicsDict.ContainsKey(layerID)) {
                        layerPicsDict[layerID].Save(layersExportPath + "l" + layerType2String[(int)kpbin.layers[layerID].type] + "_" + layerID + extensions[imageFormat], imageFormat);
                    }
                }
            }
        }


        public static void blendLayers()
        {
            int sizeX = maxWorldX - minWorldX;
            int sizeY = maxWorldY - minWorldY;

            Console.WriteLine("Done with layers. Creating Map render of size " + sizeX + "*" + sizeY + "...");

            Bitmap mapRender = new Bitmap(sizeX, sizeY);

            for (int lID = 0; lID < kpbin.layers.Count; lID++)
            {
                int layerID = (kpbin.layers.Count - 1) - lID;
                if (layerPicsDict.ContainsKey(layerID))
                {
                    dKPLayer_s layer = kpbin.layers[layerID];
                    int posX = layerPosDict[layerID].X - minWorldX;
                    int posY = layerPosDict[layerID].Y - minWorldY;
                    mapRender.BlendBitmap(layerPicsDict[layerID], posX, posY);
                    Console.WriteLine("Blended in Layer " + layerID + " at position " + posX + ";" + posY);
                }
            }

            if (worldmapExportPath != "")
                mapRender.Save(worldmapExportPath);
        }
    }
}
