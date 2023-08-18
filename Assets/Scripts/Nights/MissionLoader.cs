using ModelBuilder;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MissionLoader
{
    const int _vdpTable = 0x060ED000;
    private static string _textureFolder = "";

    public static GameObject LoadCommon()
    {
        _textureFolder = "common";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x060516BC, 0x06051715);

        LoadFile(0x06051437, 0x25A0A000);
        LoadFile(0x060514F2, 0x00200000);
        LoadFile(0x06051761, 0x060B0000);
        LoadFile(0x06051686, 0x06080000);
        int size = LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);
        LoadFile(0x06051556, 0x06098000);
        LoadFile(0x060515A5, 0x00200010);

        LoadTextures(0x060B4174, 0x060ED6A0);
        LoadTextures(0x060B4180, 0x060EE9A8);

        return ImportModels(0x06080000, size);
    }

    public static GameObject LoadRd1_SpringValley()
    {
        _textureFolder = "rd1_springvalley";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x060516BC, 0x06051715);

        LoadFile(0x06051437, 0x25A0A000);
        LoadFile(0x060514F2, 0x00200000);
        LoadFile(0x06051761, 0x060B0000);
        LoadFile(0x06051686, 0x06080000);
        LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);
        int size = LoadFile(0x06051556, 0x06098000);
        LoadFile(0x060515A5, 0x00200010);

        LoadTextures(0x060B4174, 0x060ED6A0);
        LoadTextures(0x060B4180, 0x060EE9A8);

        return ImportModels(0x06098000, size);
    }

    public static GameObject LoadRd1_GillwingBoss()
    {
        Importer.Instance.LoadColorFile("rd1_boss_cram.bin");

        _textureFolder = "rd1_gillwing_boss";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadFile(0x060517A7, 0x060C0000);
        LoadFile(0x060515E4, 0x06080000);   // MCOMH.PRS
        int size = LoadFile(0x060515EE, 0x06086000);
        LoadFile(0x06051634, 0x0029F000);   // MCOM.PRS
        LoadFile(0x0605163D, 0x00200000);

        LoadTextures(0x060C571C, 0x060ED6A0);
        LoadTextures(0x060C5728, 0x060EE9A8);

        return ImportModels(0x06086000, size, 128f);
    }

    public static GameObject LoadRd2_FrozenBell()
    {
        _textureFolder = "rd2_frozenbell";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x060516C8, 0x06051721);

        LoadFile(0x0605144F, 0x25A0A000);
        LoadFile(0x060514FE, 0x00200000);
        LoadFile(0x0605176B, 0x060B0000);
        LoadFile(0x0605168F, 0x06080000);
        LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);
        int size = LoadFile(0x06051560, 0x06098000);
        LoadFile(0x060515AE, 0x00200010);

        LoadTextures(0x060B3A10, 0x060ED6A0);
        LoadTextures(0x060B3A1C, 0x060EE9A8);

        return ImportModels(0x06098000, size);
    }

    public static GameObject LoadRd2_PuffyBoss()
    {
        // puffy
        Importer.Instance.LoadColorFile("rd1_boss_cram.bin");

        _textureFolder = "rd2_puffy_boss";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadFile(0x060517B1, 0x060C0000);
        LoadFile(0x060515E4, 0x06080000);   // MCOMH.PRS
        int size = LoadFile(0x060515F8, 0x06086000);
        LoadFile(0x06051634, 0x0029F000);   // MCOM.PRS
        LoadFile(0x06051646, 0x00200000);

        LoadTextures(0x060C2FF0, 0x060ED6A0);
        LoadTextures(0x060C2FFC, 0x060EE9A8);

        return ImportModels(0x06086000, size, 32f);
    }

    public static GameObject LoadRd3_MysticForest()
    {
        _textureFolder = "rd3_mysticforest";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x060516D4, 0x0605172D);     // GRDFORES.PRS     GTXFORES.PRS

        LoadFile(0x06051466, 0x25A0A000);
        LoadFile(0x0605150A, 0x00200000);
        LoadFile(0x06051775, 0x060B0000);               // PRG3P.PRS 
        LoadFile(0x06051698, 0x06080000);
        LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);
        int size = LoadFile(0x0605156A, 0x06098000);   // RD3PH.PRS
        LoadFile(0x060515B7, 0x00200010);              // RD3P.PRS

        LoadTextures(0x060B570C, 0x060ED6A0);
        LoadTextures(0x060B5718, 0x060EE9A8);

        return ImportModels(0x06098000, size);
    }

    public static GameObject LoadRd3_ClawzBoss()
    {
        Importer.Instance.LoadColorFile("rd3_boss_cram.bin");

        _textureFolder = "rd3_clawz_boss";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadFile(0x060517BB, 0x060C0000);   // PRG3M.PRS
        LoadFile(0x060515E4, 0x06080000);   // MCOMH.PRS
        int size = LoadFile(0x06051602, 0x06086000);   // RD3MH.PRS
        LoadFile(0x06051634, 0x0029F000);   // MCOM.PRS
        LoadFile(0x0605164F, 0x00200000);   // RD3M.PRS 

        LoadTextures(0x060C3C50, 0x060ED6A0);
        LoadTextures(0x060C3C5C, 0x060EE9A8);

        return ImportModels(0x06086000, size, 32f);
    }

    public static GameObject LoadRd4_SplashGarden()
    {
        _textureFolder = "rd4_splashgarden";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x060516E1, 0x0605173A);

        LoadFile(0x0605147D, 0x25A0A000);
        LoadFile(0x06051516, 0x00200000);
        LoadFile(0x0605177F, 0x060B0000);
        LoadFile(0x060516A1, 0x06080000);
        LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);
        int size = LoadFile(0x06051574, 0x06098000);
        LoadFile(0x0060515C0, 0x00200010);

        LoadTextures(0x060B4A20, 0x060ED6A0);
        LoadTextures(0x060B4A2C, 0x060EE9A8);

        return ImportModels(0x06098000, size, 256f, true);
    }

    public static GameObject LoadRd4_RealaBoss()
    {
        Importer.Instance.LoadColorFile("rd3_boss_cram.bin");

        _textureFolder = "rd4_reala_boss";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x060516EE, 0x0605173A);

        LoadFile(0x060517C5, 0x060C0000);
        LoadFile(0x060515E4, 0x06080000);   // MCOMH.PRS
        int size = LoadFile(0x0605160C, 0x06086000);
        LoadFile(0x06051634, 0x0029F000);   // MCOM.PRS
        LoadFile(0x06051658, 0x00210000);

        LoadTextures(0x060C4CD0, 0x060ED6A0);
        LoadTextures(0x060C4CDC, 0x060EE9A8);

        return ImportModels(0x06086000, size, 32f);
    }

    public static GameObject LoadRd5_SoftMuseum()
    {
        _textureFolder = "rd5_softmuseum";
        Directory.CreateDirectory("textures/" + _textureFolder);
    
        LoadGroundTextures(0x060516FB, 0x06051747);

        LoadFile(0x06051494, 0x25A0A000);
        LoadFile(0x06051521, 0x00200000);
        LoadFile(0x06051789, 0x060B0000);               
        LoadFile(0x060516AA, 0x06080000);
        LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);
        int size = LoadFile(0x0605157E, 0x06098000);   
        LoadFile(0x060515C9, 0x00200010);             

        LoadTextures(0x060B3D2C, 0x060ED6A0);
        LoadTextures(0x060B3D38, 0x060EE9A8);

        return ImportModels(0x06098000, size, 256f, true);
    }

    public static GameObject LoadRd5_GulpoBoss()
    {
        Importer.Instance.LoadColorFile("rd5_boss_cram.bin");

        _textureFolder = "rd5_gulpo_boss";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadFile(0x060517D9, 0x060C0000);
        LoadFile(0x060515E4, 0x06080000);   // MCOMH.PRS
        int size = LoadFile(0x06051620, 0x06086000);
        LoadFile(0x06051634, 0x0029F000);   // MCOM.PRS
        LoadFile(0x0605166A, 0x00200000);

        LoadTextures(0x060C2790, 0x060ED6A0);
        LoadTextures(0x060C279C, 0x060EE9A8);

        return ImportModels(0x06086000, size, 256f);
    }

    public static GameObject LoadRd6_StickCanyon()
    {
        _textureFolder = "rd6_stickcanyon";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x06051708, 0x06051754);

        LoadFile(0x060514B8, 0x25A0A000);
        LoadFile(0x0605152D, 0x00200000);
        LoadFile(0x06051793, 0x060B0000);
        LoadFile(0x060516B3, 0x06080000);
        LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);
        int size = LoadFile(0x06051588, 0x06098000);
        LoadFile(0x060515D2, 0x00200010);

        LoadTextures(0x060B3F20, 0x060ED6A0);
        LoadTextures(0x060B3F2C, 0x060EE9A8);

        return ImportModels(0x06098000, size, 256f, true);
    }

    public static GameObject LoadRd6_JackleBoss()
    {
        Importer.Instance.LoadColorFile("rd1_boss_cram.bin");

        _textureFolder = "rd6_jackle_boss";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadFile(0x060517CF, 0x060C0000);
        LoadFile(0x060515E4, 0x06080000);   // MCOMH.PRS
        int size = LoadFile(0x06051616, 0x06086000);
        LoadFile(0x06051634, 0x0029F000);   // MCOM.PRS
        LoadFile(0x06051661, 0x00200000);

        LoadTextures(0x060C30E0, 0x060ED6A0);
        LoadTextures(0x060C30EC, 0x060EE9A8);

        return ImportModels(0x06086000, size, 32f);
    }

    public static GameObject LoadRd7_TwinSeeds()
    {
        Importer.Instance.LoadColorFile("rd7_twinseeds_cram.bin");

        _textureFolder = "rd7_twinseeds";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadGroundTextures(0x06051708, 0x06051754);

        LoadFile(0x060514CF, 0x25A0A000);
        LoadFile(0x0605179D, 0x060C0000);
        LoadFile(0x0605154C, 0x06080000);
        LoadFile(0x0605159C, 0x00270000);

        int size = LoadFile(0x06051592, 0x06098000);
        LoadFile(0x060515DB, 0x00200010);

        LoadTextures(0x060C420C, 0x060ED6A0);
        LoadTextures(0x060C4218, 0x060EE9A8);

        return ImportModels(0x06098000, size, 1024f, true);
    }

    public static GameObject LoadRd7_WizemanBoss()
    {
        Importer.Instance.LoadColorFile("rd7_boss_cram.bin");

        _textureFolder = "rd7_wizeman_boss";
        Directory.CreateDirectory("textures/" + _textureFolder);

        LoadFile(0x060517E3, 0x060C0000);
        LoadFile(0x060515E4, 0x06080000);   // MCOMH.PRS
        int size = LoadFile(0x0605162A, 0x06086000);
        LoadFile(0x06051634, 0x0029F000);   // MCOM.PRS
        LoadFile(0x06051673, 0x00200000);

        LoadTextures(0x060C8D8C, 0x060ED6A0);
        LoadTextures(0x060C8D98, 0x060EE9A8);

        LoadFile(0x0605167C, 0x00200000);
        LoadTextures(0x060C8DA4, 0x060ED8F0);

        return ImportModels(0x06086000, size, 128f);

        // additional bg textures -> missing colors
        //_textureFolder = "rd7_wizeman_boss_t2";
        //Directory.CreateDirectory("textures/" + _textureFolder);
        //LoadTextures(0x060C8DAC, 0x060ED8F0);
        //_textureFolder = "rd7_wizeman_boss_t3";
        //Directory.CreateDirectory("textures/" + _textureFolder);
        //LoadTextures(0x060C8DB4, 0x060ED8F0);
        //_textureFolder = "rd7_wizeman_boss_t4";
        //Directory.CreateDirectory("textures/" + _textureFolder);
        //LoadTextures(0x060C8DBC, 0x060ED8F0);
    }

    public static GameObject ImportModels(int memoryPointer, int size, float gridOffset = 256f, bool pointHeaderCheck = true)
    {
        bool debug = true;

        List<int> xpData = Importer.Instance.SearchForXPDataNights(memoryPointer, 0, size, debug);

        ModelData modelData = Importer.Instance.ImportNightsModelFromMemory(xpData, 0, gridOffset, 1024, 1024, debug, pointHeaderCheck, _textureFolder);

        GameObject obj = Importer.Instance.CreateObject(modelData, _textureFolder, false, false);
        return obj;
    }

    public static void LoadGroundTextures(int grdNamePointer, int gtxNamePointer)
    {
        string grdName = GetFileNameFromMemory(grdNamePointer);
        string gtxName = GetFileNameFromMemory(gtxNamePointer);

        Importer.Instance.LoadGroundTextures(gtxName, grdName);
    }

    public static int LoadFile(int fileNamePointer, int destination)
    {
        string fileName = GetFileNameFromMemory(fileNamePointer);

        Debug.Log(fileName + " -> " + destination.ToString("X8"));

        MemoryManager memory = Importer.Instance.GetMemoryManager(destination);

        if (memory != null)
        {
            return memory.LoadFile(VersionChecker.GetFilePath(fileName), destination, true);
        }
        else
        {
            Debug.Log("No memory for " + fileName);
            return -1;
        }
    }

    public static void LoadTextures(int tableListPointer, int destination)
    {
        bool debug = true;

        MemoryManager memory = Importer.Instance.GetMemoryManager(tableListPointer);

        int textureId = GetTextureIdFromVdpTable(_vdpTable, destination);

        bool tableEnd = false;
        while (tableEnd == false)
        {
            int tablePointer = memory.GetInt32(tableListPointer);

            if (tablePointer == -1)
            {
                tableEnd = true;
                continue;
            }

            Debug.Log("textureTable at " + tablePointer.ToString("X8"));

            bool increaseIndexOnZero = true;

            bool addTextures = true;
            textureId = Importer.Instance.LoadTexturesFromMemory(_textureFolder, tablePointer, -1, textureId, addTextures, debug, increaseIndexOnZero);

            tableListPointer += 4;
        }
    }

    public static string GetFileNameFromMemory(int fileNamePointer)
    {
        string fileName = "";

        MemoryManager memory = Importer.Instance.GetMemoryManager(fileNamePointer);

        bool nameEnd = false;
        while (nameEnd == false)
        {
            byte character = memory.GetByte(fileNamePointer);

            if (character == 0)
            {
                nameEnd = true;
                continue;
            }

            fileName += (char)memory.GetByte(fileNamePointer);
            fileNamePointer++;
        }

        return fileName;
    }

    public static int GetTextureIdFromVdpTable(int vdpTableStart, int vdpTablePointer)
    {
        return ((vdpTablePointer - vdpTableStart) / 8);
    }
}
