using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VersionChecker
{
    public static string ImagePath = "";

    public static bool _usVersion = false;
    public static bool _euVersion = false;
    public static bool _jpVersion = false;
    public static bool _prototypeVersion = false;
    public static bool _finalVersion = false;
    public static bool _christmasVersion = false;

    public static string GetFilePath(string fileName)
    {
        return ImagePath + "/" + fileName;
    }

    public static void CheckImagePath()
    {
        string imageFolder = "Image";

        bool pathValid = IsPathValid(imageFolder);

        if (pathValid == true)
        {
            ImagePath = imageFolder;
            return;
        }

        DriveInfo[] allDrives = DriveInfo.GetDrives();

        foreach (DriveInfo drive in allDrives)
        {
            pathValid = IsPathValid(drive.Name);

            if (pathValid == true)
            {
                ImagePath = drive.Name;
                return;
            }
        }

        Debug.LogError("No valid image path found!");
    }

    private static bool IsPathValid(string imageFolder)
    {
        string mainFile = "0NIGHTS";
        int mainFileRequiredSizeUSandEU = 447868;
        int mainFileRequiredSizePrototype = 437667;
        int mainFileRequiredSizeChristmas = 409324;

        int mainFileRequiredSizeJP = 429056;

        string mainFilePath = imageFolder + "/" + mainFile;

        if (File.Exists(mainFilePath))
        {
            FileInfo fileInfo = new FileInfo(mainFilePath);

            //Debug.Log("length: " + fileInfo.Length);

            if (fileInfo.Length == mainFileRequiredSizeUSandEU)
            {
                Debug.Log("Valid image path: " + imageFolder);

                Debug.Log("Final version");

                //if (IsUSVersion(imageFolder))
                //{
                //    _usVersion = true;
                //    Debug.Log("US version");
                //}
                //else
                //{
                //    _euVersion = true;
                //    Debug.Log("EU version");
                //}

                _finalVersion = true;

                return true;
            }
            //else if (fileInfo.Length == mainFileRequiredSizeJP)
            //{
            //    Debug.Log("Valid image path: " + imageFolder);

            //    _jpVersion = true;

            //    Debug.Log("JP version");

            //    return true;
            //}
            else if (fileInfo.Length == mainFileRequiredSizePrototype)
            {
                Debug.Log("Valid image path: " + imageFolder);

                //_jpVersion = true;
                _prototypeVersion = true;

                Debug.Log("Prototype version");

                return true;
            }
            else if (fileInfo.Length == mainFileRequiredSizeChristmas)
            {
                Debug.Log("Valid image path: " + imageFolder);

                //_jpVersion = true;
                _christmasVersion = true;

                Debug.Log("Christmas version");

                return true;
            }
        }

        return false;
    }

    //private bool IsUSVersion(string imageFolder)
    //{
    //    string loadDataFile = "LOADDATA.BIN";
    //    int loadDataFileRequiredSizeUS = 96398;
    //    //int loadDataFileRequiredSizeEU = 98516;

    //    string loadDataFilePath = imageFolder + "/" + loadDataFile;

    //    if (File.Exists(loadDataFilePath))
    //    {
    //        FileInfo fileInfo = new FileInfo(loadDataFilePath);

    //        if (fileInfo.Length == loadDataFileRequiredSizeUS)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}
}
