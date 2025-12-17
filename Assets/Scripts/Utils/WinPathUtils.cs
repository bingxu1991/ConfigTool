using System;
using System.IO;
using UnityEngine;

public class WinPathUtils : Singleton<WinPathUtils>
{
    // 获取系统桌面路径
    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    //获取打包PC的根目录路径
    string rootPath = Directory.GetParent(Application.dataPath).FullName;
    //获取配置总的根目录路径
    string configRootPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ConfigAbout");

    /// <summary>
    /// 桌面是否存在指定目录
    /// </summary>
    private string DesktopPath(string folderName)
    {
        return Path.Combine(desktopPath, folderName);
    }

    /// <summary>
    /// PC根目录 是否存在指定目录
    /// </summary>
    public string RootPath(string folderName)
    {
        return Path.Combine(rootPath, folderName);
    }

    /// <summary>
    /// 配置根目录中 是否存在指定目录
    /// </summary>
    public string ConfigRootPath(string folderName)
    {
        return Path.Combine(configRootPath, folderName);
    }

    /// <summary>
    /// 桌面 是否存在指定目录，不存在则创建   
    /// </summary>
    private string CreateDesktopFolder(string folderPath)
    {
        // 如果不存在则创建
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            //Debug.Log("已创建桌面文件夹：" + folderPath);
        }
        else
        {
            //Debug.Log("桌面文件夹已存在：" + folderPath);
        }

        return folderPath;
    }

    /// <summary>
    /// 根目录中 是否存在指定目录，不存在则创建   
    /// </summary>
    public string CreateRootFolder(string folderPath)
    {
        // 如果不存在则创建
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            //Debug.Log("已创建桌面文件夹：" + folderPath);
        }
        else
        {
            //Debug.Log("桌面文件夹已存在：" + folderPath);
        }

        return folderPath;
    }


    /// <summary>
    /// 获取桌面上  指定文件夹的 所有子文件夹名称
    /// </summary>
    private string[] GetDesktopSubFolders(string folderName)
    {
        // 桌面文件夹路径
        string path = Path.Combine(desktopPath, folderName);

        // 获取所有子文件夹路径
        string[] folderPaths = Directory.GetDirectories(path);

        // 只保留文件夹名称
        for (int i = 0; i < folderPaths.Length; i++)
        {
            folderPaths[i] = Path.GetFileName(folderPaths[i]);
        }

        return folderPaths;
    }

    /// <summary>
    /// 获取配置总的根目录上  指定文件夹的 所有子文件夹名称
    /// </summary>
    public string[] GetRootSubFolders(string folderName)
    {
        // 桌面文件夹路径
        string path = Path.Combine(configRootPath, folderName);

        // 获取所有子文件夹路径
        string[] folderPaths = Directory.GetDirectories(path);

        // 只保留文件夹名称
        for (int i = 0; i < folderPaths.Length; i++)
        {
            folderPaths[i] = Path.GetFileName(folderPaths[i]);
        }

        return folderPaths;
    }
}
