#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ConfigToolWindow : EditorWindow
{
    private string excelFolder;
    private string outputCs = "Assets/Scripts/Config";
    private string outputJson = "Assets/StreamingAssets/Config";


    [MenuItem("Tools/Config Tool/打开配置工具")]
    public static void Open()
    {
        GetWindow<ConfigToolWindow>("Config Tool");
    }


    private void OnGUI()
    {
        GUILayout.Label("Excel 配置工具（公司级）", EditorStyles.boldLabel);


        EditorGUILayout.Space();


        excelFolder = EditorGUILayout.TextField("Excel 目录", excelFolder);
        if (GUILayout.Button("选择 Excel 目录"))
        {
            excelFolder = EditorUtility.OpenFolderPanel("选择 Excel 目录", "", "");
        }


        EditorGUILayout.Space();
        outputCs = EditorGUILayout.TextField("C# 输出目录", outputCs);
        outputJson = EditorGUILayout.TextField("Json 输出目录", outputJson);


        EditorGUILayout.Space();


        if (GUILayout.Button("校验 Excel"))
        {
            ConfigToolProcessor.ValidateAll(excelFolder);
        }


        if (GUILayout.Button("生成 C#"))
        {
            ConfigToolProcessor.GenerateAllCS(excelFolder, outputCs);
        }


        if (GUILayout.Button("导出 Json"))
        {
            ConfigToolProcessor.ExportAllJson(excelFolder, outputJson);
        }


        if (GUILayout.Button("全量导出"))
        {
            ConfigToolProcessor.RunAll(excelFolder, outputCs, outputJson);
        }
    }
}
#endif