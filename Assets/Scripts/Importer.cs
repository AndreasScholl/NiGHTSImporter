using ModelBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Importer : MonoBehaviour
{
    const int HWRAM = 0x06000000;
    const int LWRAM = 0x00200000;

    MemoryManager _dataHW;
    MemoryManager _dataLW;
    byte[] _colorRam = null;

    public int _subDivide = 2;

    public bool _debugOutput = true;

    public Material _objectMaterial = null;
    public Material _objectTransparentMaterial = null;
    public Material _mapMaterial = null;
    public Material _mapTransparentMaterial = null;

    public List<Texture2D> Textures = new List<Texture2D>();
    private List<Texture2D> _groundTextures = null;
    private List<int> _groundPalettes = null;

    List<int> _paletteReferences;

    static public Importer Instance = null;

    private void Awake()
    {
        Instance = this;    
    }

    void Start()
    {
        _dataHW = new MemoryManager(HWRAM);
        _dataLW = new MemoryManager(LWRAM);

        VersionChecker.CheckImagePath();

        // load main binary
        _dataHW.LoadFile(VersionChecker.GetFilePath("0NIGHTS"), 0x06004000, false);

        List<GameObject> models = new List<GameObject>();

        models.Add(MissionLoader.LoadCommon());
        models.Add(MissionLoader.LoadRd1_SpringValley());
        models.Add(MissionLoader.LoadRd1_GillwingBoss());
        models.Add(MissionLoader.LoadRd2_FrozenBell());
        models.Add(MissionLoader.LoadRd2_PuffyBoss());
        models.Add(MissionLoader.LoadRd3_MysticForest());
        models.Add(MissionLoader.LoadRd3_ClawzBoss());
        models.Add(MissionLoader.LoadRd4_SplashGarden());
        models.Add(MissionLoader.LoadRd4_RealaBoss());
        models.Add(MissionLoader.LoadRd5_SoftMuseum());
        models.Add(MissionLoader.LoadRd5_GulpoBoss());
        models.Add(MissionLoader.LoadRd6_StickCanyon());
        models.Add(MissionLoader.LoadRd6_JackleBoss());
        models.Add(MissionLoader.LoadRd7_TwinSeeds());
        models.Add(MissionLoader.LoadRd7_WizemanBoss());

        const float modelGrid = 512f;
        const int modelsPerRow = 4;
        float gridX = 0f;
        float gridZ = 0f;
        int rowCount = 0;
        foreach (GameObject modelObject in models)
        {
            modelObject.transform.position = new Vector3(gridX, 0f, gridZ);
            gridX += modelGrid;

            rowCount++;
            if (rowCount == modelsPerRow)
            {
                rowCount = 0;
                gridX = 0f;
                gridZ += modelGrid;
            }
        }
    }

    public MemoryManager GetMemoryManager(int address)
    {
        if (_dataLW.IsAddressValid(address))
        {
            return _dataLW;
        }
        else if (_dataHW.IsAddressValid(address))
        {
            return _dataHW;
        }

        return null;
    }

    public void LoadColorFile(string file)
    {
        string colorFile = Application.streamingAssetsPath + "/" + file;
        _colorRam = File.ReadAllBytes(colorFile);
    }

    public List<int> SearchForXPDataNights(int memoryPointer = 0, int startOffset = 0, int size = 0, bool debug = false)
    {
        List<int> pointers = new List<int>();

        MemoryManager memory = GetMemoryManager(memoryPointer);

        int maxVertices = 1024;
        int maxPolygons = 1024;

        int pointerStart = memoryPointer + startOffset;
        int pointerEnd = memoryPointer + startOffset + size;

        int count = 0;

        for (int searchPointer = pointerStart;
                 searchPointer < pointerEnd;
                 searchPointer += 4)
        {
            // try to match xpdata pattern
            int value1 = memory.GetInt32(searchPointer);
            int value2 = memory.GetInt32(searchPointer + 4);
            int value3 = memory.GetInt32(searchPointer + 8);
            int value4 = memory.GetInt32(searchPointer + 12);
            int value5 = memory.GetInt32(searchPointer + 16);

            if (value1 == value3)
            {
                continue;
            }

            if (IsPointer(value1, pointerStart, pointerEnd) && IsBetween(value2, 1, maxVertices) &&
                IsPointer(value3, pointerStart, pointerEnd) && IsBetween(value4, 1, maxPolygons) &&
                IsPointer(value5, pointerStart, pointerEnd))
            {

                pointers.Add(searchPointer);

                if (debug)
                {
                    Debug.Log("XPDATA " + count + " : " + searchPointer.ToString("X8"));
                }
                count++;
            }

            if (value1 == 0 && IsBetween(value2, 1, maxVertices) &&
                IsPointer(value3, pointerStart, pointerEnd) && IsBetween(value4, 1, maxPolygons) &&
                IsPointer(value5, pointerStart, pointerEnd))
            {
                if (debug)
                {
                    Debug.Log("ZERO POINTS PTR AT: " + searchPointer.ToString("X8"));
                }
                pointers.Add(searchPointer);
                count++;
            }
        }

        if (debug)
        {
            Debug.Log("XPDATA search done.");
        }

        return pointers;
    }

    bool IsPointer(int value, int start, int end)
    {
        if (value >= start && value < end)
        {
            return true;
        }
        return false;
    }

    bool IsBetween(int value, int min, int max)
    {
        if (value >= min && value < max)
        {
            return true;
        }
        return false;
    }

    public ModelData ImportNightsModelFromMemory(List<int> xpData, int offsetTranslations = 0,
                                            float gridOffset = 24f, int width = 256, int height = 256, bool debugOutput = false,
                                            bool pointHeaderCheck = false, string textureFolder = ""
                                            /*, List<TextureReplacements> textureReplacements = null*/)

    {
        ModelData model = new ModelData();
        model.Init();

        ModelTexture modelTexture = new ModelTexture();
        modelTexture.Init(false, width, height);
        model.ModelTexture = modelTexture;

        for (int count = 0; count < xpData.Count; count++)
        {
            //    TextureReplacements activeTextureReplacements = null;

            //    if (textureReplacements != null)
            //    {
            //        foreach (TextureReplacements replacement in textureReplacements)
            //        {
            //            if (replacement.ObjIndex == count)
            //            {
            //                activeTextureReplacements = replacement;
            //                break;
            //            }
            //        }
            //    }

            //if (count != 8)
            //{
            //    continue;
            //}

            if (debugOutput)
            {
                Debug.Log("--------------------");
                Debug.Log("OBJNR: " + count);
            }

            try
            {
                ModelPart part = new ModelPart();
                part.Init();
                model.Parts.Add(part);  // add part to part list

                int offsetXPData = xpData[count];
                MemoryManager memory = GetMemoryManager(offsetXPData);

                int offsetPoints = memory.GetInt32(offsetXPData);
                int numPoints = memory.GetInt32(offsetXPData + 4);
                int offsetPolygons = memory.GetInt32(offsetXPData + 8);
                int numPolygons = memory.GetInt32(offsetXPData + 12);
                int offsetAttributes = memory.GetInt32(offsetXPData + 16);
                int offsetNormals = memory.GetInt32(offsetXPData + 20);

                // TEMP for eggman chec
                //bool pointHeaderCheck = true;
                if (pointHeaderCheck && (offsetPoints != 0))
                {
                    int value1 = memory.GetInt32(offsetPoints);
                    int value2 = memory.GetInt32(offsetPoints + 4);
                    int value3 = memory.GetInt32(offsetPoints + 8);

                    int deltaPoints = offsetPolygons - offsetPoints;
                    if (debugOutput)
                    {
                        Debug.Log("delta.points: " + deltaPoints + " should be: " + (numPoints * 12));
                    }

                    //if (deltaPoints > 0)
                    {
                        //if (deltaPoints != (numPoints * 12))
                        if ((deltaPoints - 12) == (numPoints * 12))
                        {
                            offsetPoints += 12;
                        }
                        else if (value2 == 0 && value3 == 0)
                        {
                            offsetPoints += 12;
                        }
                    }

                    if (debugOutput)
                    {
                        Debug.Log("  POINT-HEADER_1: " + value1.ToString("X8"));
                        Debug.Log("  POINT-HEADER_2: " + value2.ToString("X8"));
                        Debug.Log("  POINT-HEADER_3: " + value3.ToString("X8"));
                    }
                }
                // temp for eggman check end

                if (offsetPoints == 0)
                {
                    continue;   // skip
                    //offsetPoints = offsetPolygons - (numPoints * 12);   // predict
                }

                if (debugOutput)
                {
                    Debug.Log("obj: " + count + "  points: " + numPoints + " offs: " + offsetPoints.ToString("X6"));
                    Debug.Log("  polys:  " + numPolygons + " offs: " + offsetPolygons.ToString("X6"));
                    Debug.Log("  attrib:  " + offsetAttributes.ToString("X6"));
                    Debug.Log("  normals:  " + offsetNormals.ToString("X6"));
                }

                bool gotNormals = false;
                if (memory.IsAddressValid(offsetNormals))
                {
                    gotNormals = true;
                }
                else
                {
                    if (debugOutput)
                    {
                        Debug.Log(" MISSING NORMALS!");
                    }
                    part.DidNotProvideNormals = true;
                }
                part.DidNotProvideNormals = true;

                bool hasPointHeader = false;

                if ((offsetPoints >> 28) == 1)
                {
                    hasPointHeader = true;
                    offsetPoints = offsetPoints & 0x0fffffff;
                    offsetPoints += 12; // skip header
                }

                List<Vector3> points = new List<Vector3>();

                //int value1 = memory.GetInt32(offsetPoints);
                //int value2 = memory.GetInt32(offsetPoints + 4);
                //int value3 = memory.GetInt32(offsetPoints + 8);

                //if (debugOutput)
                //{
                //    Debug.Log("  POINT-HEADER_1: " + value1.ToString("X8"));
                //    Debug.Log("  POINT-HEADER_2: " + value2.ToString("X8"));
                //    Debug.Log("  POINT-HEADER_3: " + value3.ToString("X8"));
                //}

                //hasNoPointHeader = true;
                //if (hasNoPointHeader == false)
                //{
                //    if (value1 != 0 && value2 == 0 && value3 == 0)
                //    {
                //        if (debugOutput)
                //        {
                //            Debug.Log("  -> SKIP HEADER!");
                //        }
                //        offsetPoints += 12;
                //    }
                //    else if (value1 != 0 && value1 == value2 && value3 == 0)
                //    {
                //        if (debugOutput)
                //        {
                //            Debug.Log("  -> SKIP HEADER!");
                //        }
                //        offsetPoints += 12;
                //    }
                //}

                //if (gotNormals == false)
                //{
                //    Debug.Log("  -> NO NORMALS -> SKIP HEADER!");
                //    offsetPoints += 12;
                //}

                // read POINTS
                //
                for (int countPoints = 0; countPoints < numPoints; countPoints++)
                {
                    float x, y, z;

                    x = memory.GetFloat(offsetPoints);
                    y = memory.GetFloat(offsetPoints + 4);
                    z = memory.GetFloat(offsetPoints + 8);

                    offsetPoints += 12;

                    Vector3 point = new Vector3(x, y, z);
                    points.Add(point);

                    //Debug.Log(countPoints + ": " + x + " | " + y + " | " + z);
                }

                part.SourcePoints = points;

                // read NORMALS
                //
                List<Vector3> normals = new List<Vector3>();

                for (int countPoints = 0; countPoints < numPoints; countPoints++)
                {
                    if (gotNormals)
                    {
                        float x = memory.GetFloat(offsetNormals);
                        float y = memory.GetFloat(offsetNormals + 4);
                        float z = memory.GetFloat(offsetNormals + 8);

                        offsetNormals += 12;

                        Vector3 normal = new Vector3(x, y, z);
                        normals.Add(normal);

                        //Debug.Log(countPoints + ": " + x + " | " + y + " | " + z);
                    }
                    else
                    {
                        normals.Add(Vector3.up);
                    }

                }

                // read POLYGONS
                //
                //                Debug.Log(offsetPoints.ToString("X8") + " num: " + numPoints);
                //                Debug.Log(offsetPolygons.ToString("X8") + " num: " + numPolygons);

                //Debug.Log(offsetAttributes.ToString("X8") + " num: " + numPoints);

                for (int countPolygons = 0; countPolygons < numPolygons; countPolygons++)
                {
                    //                    Debug.Log(a + ", " + b + ", " + c + ", " + d);

                    ushort flag_sort = (ushort)memory.GetInt16(offsetAttributes);
                    ushort texno = (ushort)memory.GetInt16(offsetAttributes + 2);

                    ushort atrb = (ushort)memory.GetInt16(offsetAttributes + 4);
                    ushort color = (ushort)memory.GetInt16(offsetAttributes + 6);

                    ushort gstb = (ushort)memory.GetInt16(offsetAttributes + 8);
                    ushort dir = (ushort)memory.GetInt16(offsetAttributes + 10);

                    ushort v1 = (ushort)memory.GetInt16(offsetAttributes + 12);
                    ushort v2 = (ushort)memory.GetInt16(offsetAttributes + 14);

                    if (debugOutput)
                    {
                        Debug.Log("   FLAG_SORT: " + flag_sort.ToString("X4") +
                                  " TEXNO: " + (texno - 1) +
                                    " atrb: " + atrb.ToString("X4") +
                                    " color: " + color.ToString("X4") +
                                    " gstb: " + gstb.ToString("X4") +
                                    " dir: " + dir.ToString("X4") +
                                    " v1: " + v1.ToString("X4") +
                                    " v2: " + v2.ToString("X4")
                                    );
                    }

                    bool cutOut = false;
                    bool halftransparent = false;

                    //const int CL_Shadow = 1;
                    //const int CL_Half = 2;
                    //const int CL_Trans = 3;
                    //const int CL_Gouraud = 4;

                    //int clBits = atrb & 3;
                    //if (clBits == CL_Trans)
                    //{
                    //    transparent = true;
                    //}
                    //else if (clBits == CL_Half)
                    //{
                    //    halftransparent = true;
                    //}

                    // INFO FROM SGL
                    //
                    // ATTRB bits:
                    //      MSBon (1 << 15)     MSB, write to frame buffer
                    //      HSSon (1 << 12)     high speed shrink on, should always be set
                    //      Window_In  (2 << 9)	display in window
                    //      Window_Out (3 << 9)	display outside window
                    //      MESHon(1 << 8)      display as mesh
                    //      ECdis (1 << 7)      use endcode as palette -> meaning?
                    //      SPdis (1 << 6)      display clear pixels (disable cutout)

                    int sPdis = 1 << 6;
                    if ((atrb & sPdis) == 0)
                    {
                        cutOut = true;
                    }

                    bool wireFrame = false;
                    int mESHon = 1 << 8;
                    if ((atrb & mESHon) != 0)
                    {
                        wireFrame = true;
                        halftransparent = true;
                        if (debugOutput)
                        {
                            Debug.Log("WIREFRAME! at " + offsetAttributes.ToString("X6"));
                        }
                    }

                    //if ((flag_sort & (1 << 2)) != 0 && (flag_sort & 1) == 0)
                    //{
                    //    transparent = true;
                    //}
                    //else if ((flag_sort & (1 << 1)) != 0)
                    //{
                    //    halftransparent = true;
                    //}

                    //if ((dir & (1 << 7)) != 0)
                    //{
                    //    //halftransparent = true;
                    //}

                    // flag
                    bool doubleSided = false;
                    if (((flag_sort >> 8) & 1) == 1)
                    {
                        doubleSided = true;
                    }

                    // atrb
                    int colorMode = (atrb & 0b111000) >> 3;
                    // dir
                    bool hflip = false;
                    bool vflip = false;
                    if ((dir & (1 << 4)) != 0)
                    {
                        hflip = true;
                    }
                    if ((dir & (1 << 5)) != 0)
                    {
                        vflip = true;
                    }

                    // geometry
                    //
                    int a, b, c, d;
                    bool negA, negB, negC, negD;

                    Vector3 faceNormal = Vector3.one;
                    //float x = GetFloat(bytePtr + offsetPolygons);
                    //float y = GetFloat(bytePtr + offsetPolygons + 4);
                    //float z = GetFloat(bytePtr + offsetPolygons + 8);
                    //faceNormal = new Vector3(x, y, z);
                    //offsetPolygons += 12;

                    a = memory.GetPolygonIndex(offsetPolygons, out negA);
                    b = memory.GetPolygonIndex(offsetPolygons + 2, out negB);
                    c = memory.GetPolygonIndex(offsetPolygons + 4, out negC);
                    d = memory.GetPolygonIndex(offsetPolygons + 6, out negD);
                    offsetPolygons += 8;

                    //Debug.Log("Polygon: " + countPolygons + " a: " + a + " b: " + b + " c: " + c + " d: " + d);

                    // color
                    Color rgbColor = ColorConversion.ConvertColor(color);
                    Color colorA = Color.gray;
                    Color colorB = Color.gray;
                    Color colorC = Color.gray;
                    Color colorD = Color.gray;

                    if (debugOutput)
                    {
                        //Debug.Log("word-obj: " + count + " = texture: " + texno + " gstb: " + gstb);
                        Debug.Log("TEXTURE: " + (texno - 1));
                        //Debug.Log("Colormode: " + colorMode + " tr: " + cutOut + " htr: " + halftransparent);
                        //Debug.Log("Color: " + rgbColor + " GSTBL: " + gstb);
                    }

                    if (Textures == null)
                    {
                        texno = 0;
                    }
                    else
                    {
                        // texno out of bounds?
                        if ((texno > 0) && ((texno - 1) >= Textures.Count))
                        {
                            texno = 0; // => no texture
                        }
                    }

                    Vector2 uvA, uvB, uvC, uvD;
                    uvA = Vector2.zero;
                    uvB = Vector2.zero;
                    uvC = Vector2.zero;
                    uvD = Vector2.zero;

                    if (texno > 0 && (texno - 1) < Textures.Count)
                    {
                        // texture handling
                        //if (activeTextureReplacements != null)
                        //{
                        //    if (activeTextureReplacements.Replacement.ContainsKey(texno - 1))
                        //    {
                        //        //Debug.Log("replacing " + (texno - 1) + " with: " + activeTextureReplacements.Replacement[texno - 1]);
                        //        texno = (ushort)activeTextureReplacements.Replacement[texno - 1];
                        //        texno++;
                        //    }
                        //}

                        Texture2D texture = Textures[texno - 1];

                        if (texture != null)
                        {
                            if (modelTexture.ContainsTexture(texture) == false)
                            {
                                modelTexture.AddTexture(texture, cutOut, halftransparent);
                            }

                            // add texture uv
                            bool rotate = false;
                            modelTexture.AddUv(texture, hflip, vflip, rotate, out uvA, out uvB, out uvC, out uvD);

                            colorA = Color.white;
                            colorB = Color.white;
                            colorC = Color.white;
                            colorD = Color.white;

                            rgbColor = Color.white;
                        }
                    }
                    else
                    {
                        // no texture, just color => create colored texture
                        //
                        Texture2D colorTex = new Texture2D(2, 2);
                        Color[] colors = new Color[4];
                        colors[0] = rgbColor;
                        colors[1] = rgbColor;
                        colors[2] = rgbColor;
                        colors[3] = rgbColor;
                        colorTex.SetPixels(colors);
                        colorTex.Apply();

                        modelTexture.AddTexture(colorTex, true, false);
                        bool rotate = false;
                        modelTexture.AddUv(colorTex, hflip, vflip, rotate, out uvA, out uvB, out uvC, out uvD);

                        colorA = Color.white;
                        colorB = Color.white;
                        colorC = Color.white;
                        colorD = Color.white;
                    }

                    offsetAttributes += 16;

                    Vector3 vA, vB, vC, vD;
                    Vector3 nA, nB, nC, nD;
                    //nA = faceNormal;
                    //nB = faceNormal;
                    //nC = faceNormal;
                    //nD = faceNormal;

                    nA = normals[a];
                    nB = normals[b];
                    nC = normals[c];
                    nD = normals[d];

                    if (a < points.Count && b < points.Count && c < points.Count && d < points.Count)
                    {
                        vA = points[a];
                        vB = points[b];
                        vC = points[c];
                        vD = points[d];

                        //part.AddPolygon(vA, vB, vC, vD,
                        //                halftransparent, doubleSided,
                        //                colorA, colorB, colorC, colorD,
                        //                uvA, uvB, uvC, uvD,
                        //                nA, nB, nC, nD, s_SubDivide);

                        part.AddPolygon(vA, vB, vC, vD,
                                        halftransparent, false,
                                        colorA, colorB, colorC, colorD,
                                        uvA, uvB, uvC, uvD,
                                        nA, nB, nC, nD, _subDivide);

                        if (doubleSided)
                        {
                            part.AddPolygon(vA, vD, vC, vB,
                                            halftransparent, false,
                                            colorA, colorD, colorC, colorB,
                                            uvA, uvD, uvC, uvB,
                                            -nA, -nD, -nC, -nB, _subDivide);
                        }
                    }
                }

                // get translation
                //
                float transX = 0f, transY = 0f, transZ = 0f;
                if (offsetTranslations != 0)
                {
                    transX = memory.GetFloat(offsetTranslations);
                    transY = memory.GetFloat(offsetTranslations + 4);
                    transZ = memory.GetFloat(offsetTranslations + 8);
                    offsetTranslations += 12;
                }
                else
                {
                    transX = (count / 5) * gridOffset;
                    transZ = (count % 5) * gridOffset;
                    transY = 0f;
                }

                part.Translation = new Vector3(transX, transY, transZ);
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message + " on OBJ: " + count);
            }
        }

        model.ModelTexture.ApplyTexture();

        byte[] bytes = model.ModelTexture.Texture.EncodeToPNG();
        File.WriteAllBytes("textures/" + textureFolder + "/ModelTexture.png", bytes);

        //Debug.Log("quads: " + _quadCount);

        return model;
    }

    public int LoadTexturesFromMemory(string name, int tableMemory, int tableMemoryEnd, int texIndexOffset,
                                    bool addTextures, bool debugOutput = false,
                                    bool increaseIndexOnZero = false, int paletteOffset = 0)
    {
        if (debugOutput)
        {
            Debug.Log("Loading textures to id: " + texIndexOffset);
        }

        if (addTextures == false)
        {
            // clear texture array
            Textures = new List<Texture2D>();
            _paletteReferences = new List<int>();    // list to track stored palettes

            for (int count = 0; count < texIndexOffset; count++)
            {
                Textures.Add(null);
            }
        }
        else
        {
            if (Textures == null)
            {
                // clear texture array
                Textures = new List<Texture2D>();
            }

            // add mode handle texture and palette index lists
            if (texIndexOffset > Textures.Count)
            {
                // add up to texIndexOffset
                for (int count = Textures.Count; count < texIndexOffset; count++)
                {
                    Textures.Add(null);
                }
            }
            else if (texIndexOffset < Textures.Count)
            {
                // remove all up to texIndexOffset
                Textures.RemoveRange(texIndexOffset, Textures.Count - texIndexOffset);
            }
        }

        List<int> paletteList = new List<int>();

        int prevCGOffset = 0;

        int index = texIndexOffset;

        int tableMemoryPointer = tableMemory;

        int tableCount = 0;
        bool tableEnd = false;

        while (tableEnd == false)
        {
            if (tableMemoryEnd != -1 && tableMemoryPointer >= tableMemoryEnd)
            {
                tableEnd = true;
                continue;
            }

            if (debugOutput)
            {
                Debug.Log("-> " + tableMemoryPointer.ToString("X6"));
            }

            int cgOffset = _dataHW.GetInt32(tableMemoryPointer);

            bool isGroundTexture = false;

            if (cgOffset == -1)
            {
                if (debugOutput)
                {
                    Debug.Log("End of table reached: 0xffffffff");
                }
                //tableMemoryPointer += 4;
                tableEnd = true;
                continue;
            }
            else if (cgOffset == 0)
            {
                if (debugOutput)
                {
                    Debug.Log("Skip Zero cgOffset");
                }
                tableMemoryPointer += 4;

                if (increaseIndexOnZero)
                {
                    Textures.Add(null);
                    //int prevPalette = paletteList[paletteList.Count - 1];
                    paletteList.Add(0);
                    index++;
                }

                continue;
            }
            else if (cgOffset == 0x40000000)
            {
                if (debugOutput)
                {
                    Debug.Log("0x40000000 -> reusing prev cgOffset");
                }
                cgOffset = prevCGOffset;
            }
            else if ((uint)cgOffset == 0x80000000)
            {
                isGroundTexture = true;
            }

            Texture2D texture = null;
            bool animationFrame = false;

            if (isGroundTexture == true)
            {
                int groundTextureId = _dataHW.GetInt32(tableMemoryPointer + 4);
                groundTextureId--;

                if (debugOutput)
                {
                    Debug.Log("0x80000000 -> ground texture id: " + groundTextureId);
                }

                int groundPaletteMemory = _groundPalettes[groundTextureId];
                paletteList.Add(groundPaletteMemory);

                texture = _groundTextures[groundTextureId];
                Textures.Add(texture);

                tableMemoryPointer += 8;
            }
            else
            {
                prevCGOffset = cgOffset;

                int pltData = _dataHW.GetInt32(tableMemoryPointer + 4);
                int hvSize = _dataHW.GetInt16(tableMemoryPointer + 8);
                int attrib = _dataHW.GetInt16(tableMemoryPointer + 10);

                int width = (hvSize >> 8) * 8;
                int height = (hvSize & 0xff);

                if (debugOutput)
                {
                    Debug.Log("TEX: " + Textures.Count +
                                " -> CG: " + cgOffset.ToString("X6") +
                                " PLT: " + pltData.ToString("X8") + " ATTRIB: " + attrib + " W: " + width + " H: " + height);
                }

                int pltOffset = 0;
                bool colorBank = false;
                bool noPalette = false;
                bool mode256Colors = false;

                if ((pltData & 0xffff0000) == 0x10000000)
                {
                    // animation frame -> reuse last palette
                    if (paletteList.Count > 0)
                    {
                        pltOffset = paletteList[paletteList.Count - 1];
                    }

                    animationFrame = true;

                    if (debugOutput)
                    {
                        Debug.Log("ANIMATION-FRAME: " + tableCount);
                    }
                }
                //else if (pltData > 0x06000000 && pltData < 0x06100000)
                //{
                //    pltOffset = pltData;

                //    if (debugOutput)
                //    {
                //        Debug.Log("PALETTE IN HWRAM REGION!");
                //    }
                //}
                else if ((pltData & 0xf0000000) == 0x80000000)
                {
                    pltOffset = pltData & 0x0fffffff;
                    //noPalette = true;
                    if (debugOutput)
                    {
                        if (attrib == 2)
                        {
                            Debug.Log("TexId: " + index + " HI-NIBBLE 8 -> 256 color bank mode!");
                        }
                        else
                        {
                            Debug.Log("TexId: " + index + " HI-NIBBLE 8 -> 16 color bank mode!");
                        }
                    }

                    colorBank = true;

                    if (attrib == 2)
                    {
                        mode256Colors = true;
                    }
                }
                else if (((pltData & 0xf0000000) == 0x20000000) ||
                         ((pltData & 0xf0000000) == 0x40000000)) 
                {
                    pltOffset = pltData & 0x0fffffff;

                    if (pltOffset == 0)
                    {
                        pltOffset = paletteList[0];
                    }

                    pltOffset += paletteOffset;

                    if (debugOutput)
                    {
                        Debug.Log("HI-NIBBLE 2 or 4 -> TODO: check what this is used for");
                    }
                }
                //else if (pltData >= data.Length || ((romOffset == 0) && (pltData > 0) && (pltData < data.Length)))
                //{
                //    pltOffset = (pltData & 0x00ffffff) - romOffset;
                //}
                else if (_dataLW.IsAddressValid(pltData) || _dataHW.IsAddressValid(pltData))
                {
                    pltOffset = pltData;
                    pltOffset += paletteOffset;
                }
                else
                {
                    // re-use palette (backward reference)
                    if (pltData < 0)
                    {
                        pltData = 1;
                    }

                    int paletteIndex = (index - texIndexOffset) - pltData;

                    if (paletteIndex < 0)
                    {
                        Debug.Log("Paletteindex out of range; " + paletteIndex);

                        paletteIndex = 0;
                    }

                    pltOffset = paletteList[paletteIndex];
                }

                //if (animationFrame == false)
                {
                    paletteList.Add(pltOffset);
                }

                MemoryManager cgMemory = GetMemoryManager(cgOffset);
                MemoryManager pltMemory = GetMemoryManager(pltOffset);

                texture = new Texture2D(width, height);
                texture.filterMode = FilterMode.Point;

                //if (animationFrame == false)    // dont store if anim-frame
                {
                    Textures.Add(texture);
                }

                //if (attrib == 1 || attrib == 8)    // 16 color?
                {
                    // read color lookup table (fixed to 16 colors for now)
                    //
                    Color[] colors = new Color[16];

                    bool debugColors = false;

                    if (debugColors == true || noPalette == true)
                    {
                        colors[0] = Color.black;
                        colors[1] = Color.white;
                        colors[2] = Color.red;
                        colors[3] = Color.green;
                        colors[4] = Color.blue;
                        colors[5] = Color.gray;
                        colors[6] = Color.cyan;
                        colors[7] = Color.magenta;
                        colors[8] = Color.yellow;
                        colors[9] = Color.red * 0.5f;
                        colors[10] = Color.green * 0.5f;
                        colors[11] = Color.blue * 0.5f;
                        colors[12] = Color.cyan * 0.5f;
                        colors[13] = Color.magenta * 0.5f;
                        colors[14] = Color.yellow * 0.5f;
                        colors[15] = Color.red * 0.25f;
                    }
                    else
                    {
                        if (colorBank == true)
                        {
                            int bankOffset = (pltOffset & 0x7ffff) * 2;
                            //int bankOffset = 0x12000 / 4;
                            //bankOffset = 0x22880 / 4;
                            //Debug.Log("ColorBank-Offset: " + bankOffset.ToString("X6"));

                            colors = new Color[256];

                            //for (int color = 0; color < 16; color++)
                            for (int color = 0; color < 256; color++)
                            {
                                //int colorValue = GetInt16(colorData, bankOffset + (color * 2));
                                //Color colorRgb = ConvertColor(colorValue);
                                //Color colorRgb = Color.black;

                                int colorValue = ByteArray.GetInt16(_colorRam, bankOffset + (color * 2));
                                Color colorRgb = ColorConversion.ConvertColor(colorValue);

                                //colorRgb = colorRgb.gamma;

                                //if (color == 0)
                                //{
                                //    colorRgb.a = 0f;
                                //}

                                colors[color] = colorRgb;
                            }
                        }
                        else
                        {
                            for (int color = 0; color < 16; color++)
                            {
                                int colorValue = pltMemory.GetInt16(pltOffset + (color * 2));

                                Color colorRgb = ColorConversion.ConvertColor(colorValue);

                                if (color == 0)
                                {
                                    colorRgb.a = 0f;
                                }

                                colors[color] = colorRgb;
                            }
                        }
                    }

                    // read texture data
                    //
                    if (mode256Colors == false)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < (width / 2); x++)
                            {
                                byte value = cgMemory.GetByte(cgOffset);
                                int clr = value >> 4;
                                texture.SetPixel(x * 2, y, colors[clr]);

                                clr = value & 0x0f;
                                texture.SetPixel((x * 2) + 1, y, colors[clr]);

                                cgOffset++;
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte value = cgMemory.GetByte(cgOffset);
                                int clr = value;
                                texture.SetPixel(x, y, colors[clr]);

                                cgOffset++;
                            }
                        }
                    }
                }

                tableMemoryPointer += 12;
            }

            texture.Apply();

            byte[] bytes = FlipYAndRemoveAlpha(texture).EncodeToPNG();
            File.WriteAllBytes("textures/" + name + "/tex_" + index + ".png", bytes);

            //if (animationFrame == false)    // dont increase index if anim-frame
            //{
            //    //File.WriteAllBytes("textures/" + name + "/tex_" + index + ".png", bytes);
            //    index++;
            //}
            //else
            //{
            //    //File.WriteAllBytes("textures/" + name + "/_anim_" + tableCount + ".png", bytes);
            //}

            index++;
            tableCount++;
        }

        return index;
    }

    public Texture2D FlipYAndRemoveAlpha(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height, TextureFormat.ARGB32, false);

        int xN = original.width;
        int yN = original.height;

        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                Color pixel = original.GetPixel(i, j);
                pixel.a = 1f;
                flipped.SetPixel(i, yN - j - 1, pixel);
            }
        }

        flipped.Apply();

        return flipped;
    }

    public void LoadGroundTextures(string gtxName, string grdName)
    {
        _groundTextures = LoadGTX(VersionChecker.GetFilePath(gtxName),
                                  VersionChecker.GetFilePath(grdName));
    }

    List<Texture2D> LoadGTX(string filePath, string colorFilePath)
    {
        byte[] data = Prs.Decompress(filePath);

        int colorFileMemory = 0x00200000;
        _dataLW.LoadFile(colorFilePath, colorFileMemory, true);

        int paletteMemory = _dataLW.GetInt32(colorFileMemory + 8);
        int textureTableEnd = _dataLW.GetInt32(colorFileMemory);
        //Debug.Log(paletteMemory.ToString("X8"));

        List<Texture2D> textures = new List<Texture2D>();

        //return textures;

        int width = 24;
        int height = 24;

        int cgOffset = 0;
        int textureIndex = 0;

        _groundPalettes = new List<int>();

        while (cgOffset < data.Length)
        {
            int colorPointer = colorFileMemory + 0x10 + (textureIndex * 4);

            if (colorPointer >= textureTableEnd)
            {
                break;
            }

            Texture2D texture = new Texture2D(width, height);

            // read colors
            int colorOffset = _dataLW.GetInt16(colorPointer) * 8;
            colorOffset -= 0x7a800; // vdp1 storage offset

            int groundPaletteMemory = paletteMemory + colorOffset;
            _groundPalettes.Add(groundPaletteMemory);

            Color[] colors = new Color[16];
            for (int count = 0; count < 16; count++)
            {
                int colorValue = _dataLW.GetInt16(groundPaletteMemory + (count * 2));
                colors[count] = ColorConversion.ConvertColor(colorValue);
                //colors[count] = Color.white;  // clear textures
            }

            // read texture data
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < (width / 2); x++)
                {
                    byte value = data[cgOffset];
                    int clr = value >> 4;
                    texture.SetPixel(x * 2, y, colors[clr]);

                    clr = value & 0x0f;
                    texture.SetPixel((x * 2) + 1, y, colors[clr]);

                    cgOffset++;
                }
            }

            texture.Apply();

            textures.Add(texture);

            // Encode texture into PNG
            //
            //byte[] bytes = texture.EncodeToPNG();
            //File.WriteAllBytes("textures/" + Path.GetFileNameWithoutExtension(filePath) + "/_" + textureIndex + ".png", bytes);

            textureIndex++;
        }

        Debug.Log("END OF GROUND TEXTURES: " + cgOffset.ToString("X8"));

        return textures;
    }

    public GameObject CreateObject(ModelData modelData, string name, bool mapShader = true,
                                bool noRoot = false, List<int> partList = null, List<Vector3> translatonList = null,
                                Material customMaterial = null, string textureFolder = "")
    {
        GameObject parent = new GameObject(name);
        GameObject root;

        if (noRoot == false)
        {
            root = new GameObject("root");
            modelData.Root = root;
            root.transform.parent = parent.transform;
            root.transform.localPosition = Vector3.zero;
            root.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            root = parent;
        }

        //Color albedo = new Color(brightness, brightness, brightness, 1.0f);

        if (customMaterial)
        {
            modelData.OpaqueMaterial = customMaterial;
        }
        else
        {
            if (mapShader)
            {
                modelData.OpaqueMaterial = new Material(_mapMaterial);
            }
            else
            {
                modelData.OpaqueMaterial = new Material(_objectMaterial);
            }
        }
        // BlendMode.Cutout:
        //modelData.OpaqueMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //modelData.OpaqueMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        //modelData.OpaqueMaterial.SetInt("_ZWrite", 1);
        //modelData.OpaqueMaterial.renderQueue = 2450;
        //modelData.OpaqueMaterial.SetColor("_Color", albedo);
        modelData.OpaqueMaterial.mainTexture = modelData.ModelTexture.Texture;
        //modelData.OpaqueMaterial.enableInstancing = true;

        if (mapShader)
        {
            modelData.TransparentMaterial = new Material(_mapTransparentMaterial);
        }
        else
        {
            modelData.TransparentMaterial = new Material(_objectTransparentMaterial);
        }
        // fade
        //modelData.TransparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        //modelData.TransparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //modelData.TransparentMaterial.SetInt("_ZWrite", 0);
        //modelData.TransparentMaterial.renderQueue = 3000;
        //modelData.TransparentMaterial.SetColor("_Color", albedo);
        modelData.TransparentMaterial.mainTexture = modelData.ModelTexture.Texture;
        //modelData.TransparentMaterial.enableInstancing = true;

        GameObject partObject;

        int parts = modelData.Parts.Count;

        if (partList != null)
        {
            parts = partList.Count;
        }

        for (int partIndex = 0; partIndex < parts; partIndex++)
        {
            ModelPart part = modelData.Parts[partIndex];

            if (partList != null)
            {
                part = modelData.Parts[partList[partIndex]];
            }

            //if (partIndex == 381)
            //{
            //    Debug.Log("abcd");
            //}

            Mesh mesh = part.CreateMesh();

            if (mesh != null)
            {
                if (part.DidNotProvideNormals)
                {
                    mesh.RecalculateNormals(60f);
                    //Debug.Log(" CALC NORMALS FOR PART: " + partIndex);
                }
            }

            partObject = new GameObject("part" + partIndex);
            partObject.SetActive(true);

            part.OpaqueObject = partObject;
            if (part.Parent == -1)
            {
                partObject.transform.parent = root.transform;
            }
            else
            {
                //partObject.transform.parent = modelData.Parts[part.Parent].OpaqueObject.transform;
                partObject.transform.parent = modelData.Parts[part.Parent].Pivot.transform;
            }
            partObject.transform.localPosition = part.Translation;
            partObject.transform.localScale = new Vector3(1f, 1f, 1f);

            if (mesh != null)
            {
                MeshFilter filter = partObject.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                MeshRenderer renderer = partObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = modelData.OpaqueMaterial;
            }

            // transparent
            mesh = part.CreateTransparentMesh();

            if (mesh != null)
            {
                if (part.DidNotProvideNormals)
                {
                    mesh.RecalculateNormals(60f);
                    //Debug.Log(" CALC NORMALS FOR PART: " + partIndex);
                }

                partObject = new GameObject("part_trans_" + partIndex);
                partObject.SetActive(true);

                part.TransparentObject = partObject;

                if (part.Parent == -1)
                {
                    partObject.transform.parent = root.transform;
                }
                else
                {
                    //partObject.transform.parent = modelData.Parts[part.Parent].OpaqueObject.transform;
                    partObject.transform.parent = modelData.Parts[part.Parent].Pivot.transform;
                }
                partObject.transform.localPosition = part.Translation;
                partObject.transform.localScale = new Vector3(1f, 1f, 1f);

                MeshFilter filter = partObject.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                MeshRenderer renderer = partObject.AddComponent<MeshRenderer>();

                renderer.sharedMaterial = modelData.TransparentMaterial;
            }

            if (part.OpaqueObject || part.TransparentObject)
            {
                // pivoting
                GameObject partPivot = new GameObject("pivot" + partIndex);

                if (part.OpaqueObject)
                {
                    partPivot.transform.position = part.OpaqueObject.transform.position;
                    partPivot.transform.parent = part.OpaqueObject.transform.parent;
                }
                else
                {
                    partPivot.transform.position = part.TransparentObject.transform.position;
                    partPivot.transform.parent = part.TransparentObject.transform.parent;
                }

                partPivot.transform.localEulerAngles = Vector3.zero;

                if (part.OpaqueObject)
                {
                    part.OpaqueObject.transform.parent = partPivot.transform;
                }

                if (part.TransparentObject)
                {
                    part.TransparentObject.transform.parent = partPivot.transform;
                }

                part.Pivot = partPivot;

                // translations provided?
                if (translatonList != null)
                {
                    partPivot.transform.position = translatonList[partIndex];
                }
            }
        }

        parent.transform.eulerAngles = new Vector3(180f, 0f, 0f);
        parent.transform.localScale = new Vector3(-0.1f, 0.1f, 0.1f);
        return parent;
    }

    private Mesh CreateMesh(ModelPart part)
    {
        List<Vector3> vertices = part.Vertices;
        List<Vector3> normals = part.Normals;
        List<int> indices = part.Indices;
        List<Color> colors = part.Colors;
        List<Vector2> uvs = part.Uvs;

        Mesh mesh = new Mesh();

        if (indices.Count <= 65535)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
        }
        else
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);

        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        mesh.name = "mesh";

        return mesh;
    }
}
