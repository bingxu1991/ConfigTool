using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ExcelToolUI : MonoBehaviour
{
    public GameObject contCell;
    string excelRoot;

    Transform bgTrans;
    Transform exportTrans;
    Transform resultTrans;
    Button checkBtn;
    Button csBtn;
    Button csInterBtn;
    Button jsonBtn;
    Button allBtn;

    void Awake()
    {
        bgTrans = transform.Find("Bg");
        exportTrans = bgTrans.Find("Export");
        resultTrans = bgTrans.Find("Result/View/Viewport/Content");

        checkBtn = exportTrans.Find("CheckBtn").GetComponent<Button>();
        csBtn = exportTrans.Find("CSBtn").GetComponent<Button>();
        csInterBtn = exportTrans.Find("CSInterBtn").GetComponent<Button>(); 
        jsonBtn = exportTrans.Find("JsonBtn").GetComponent<Button>();
        allBtn = exportTrans.Find("AllBtn").GetComponent<Button>();

        // 注册 CodePages 支持，解决 Encoding 1252 问题
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //优先检测创建配置全量  总根文件夹
        string configRoot = WinPathUtils.Ins.RootPath("ConfigAbout");
        WinPathUtils.Ins.CreateRootFolder(configRoot);

        //优先检测创建Excel根文件夹
        excelRoot = WinPathUtils.Ins.ConfigRootPath("ExcelFiles");
        WinPathUtils.Ins.CreateRootFolder(excelRoot);
    }

    private void OnEnable()
    {
        EventPublisher.OnNotice += ShowResult;
    }

    private void Start()
    {
        checkBtn.onClick.AddListener(OnCheck);
        csBtn.onClick.AddListener(OnExportCS);
        csInterBtn.onClick.AddListener(OnExportCSByInterface);
        jsonBtn.onClick.AddListener(OnExportJson);
        allBtn.onClick.AddListener(OnExportAll);
    }

    private void OnDisable()
    {
        EventPublisher.OnNotice -= ShowResult;
        checkBtn.onClick.RemoveListener(OnCheck);
        csBtn.onClick.RemoveListener(OnExportCS);
        csInterBtn.onClick.RemoveListener(OnExportCSByInterface);
        jsonBtn.onClick.RemoveListener(OnExportJson);
        allBtn.onClick.RemoveListener(OnExportAll);
    }

    void OnCheck()
    {
        ClearResult();
        CheckExcel();
    }

    void OnExportCS()
    {
        ClearResult();
        ExportCS();
    }

    void OnExportCSByInterface()
    {
        ClearResult();
        ExportCSByInterface();
    }

    void OnExportJson()
    {
        ClearResult();
        ExportJson();
    }

    void OnExportAll()
    {
        ClearResult();
        ExportAll();
    }

    /// <summary>
    /// 校验 Excel
    /// </summary>
    void CheckExcel()
    {
        if (ExcelIsNull(out string[] subFolders)) return;

        for (int i = 0; i < subFolders.Length; i++)
        {
            //Excel分类文件夹
            string excelFolderPath = Path.Combine(excelRoot, subFolders[i]);
            ConfigToolProcessor.ValidateAll(excelFolderPath);
        }
        EventPublisher.Message("所有Excel 校验完毕\n");
    }

    /// <summary>
    /// 导出 C#
    /// </summary>
    void ExportCS()
    {
        if (ExcelIsNull(out string[] subFolders)) return;

        //创建检测Cs根文件夹
        string csRootPath = WinPathUtils.Ins.ConfigRootPath("CsFiles");
        WinPathUtils.Ins.CreateRootFolder(csRootPath);

        for (int i = 0; i < subFolders.Length; i++)
        {
            //Excel分类文件夹
            string excelFolderPath = Path.Combine(excelRoot, subFolders[i]);

            //创建Cs分类文件夹
            string csFolderPath = Path.Combine(csRootPath, subFolders[i]);
            WinPathUtils.Ins.CreateRootFolder(csFolderPath);

            ConfigToolProcessor.GenerateAllCS(excelFolderPath, csFolderPath);
        }
        EventPublisher.Message("所有C# 代码生成完毕\n");
    }

    /// <summary>
    /// 导出 C#(带接口)
    /// </summary>
    void ExportCSByInterface()
    {
        if (ExcelIsNull(out string[] subFolders)) return;

        //创建检测Cs根文件夹
        string csRootPath = WinPathUtils.Ins.ConfigRootPath("CsFiles");
        WinPathUtils.Ins.CreateRootFolder(csRootPath);

        for (int i = 0; i < subFolders.Length; i++)
        {
            //Excel分类文件夹
            string excelFolderPath = Path.Combine(excelRoot, subFolders[i]);

            //创建Cs分类文件夹
            string csFolderPath = Path.Combine(csRootPath, subFolders[i]);
            WinPathUtils.Ins.CreateRootFolder(csFolderPath);

            ConfigToolProcessor.GenerateAllCSByInterface(excelFolderPath, csFolderPath);
        }
        EventPublisher.Message("所有C# 代码生成完毕\n");
    }

    /// <summary>
    /// 导出 Json
    /// </summary>
    void ExportJson()
    {
        if (ExcelIsNull(out string[] subFolders)) return;

        //创建检测Json根文件夹
        string jsonRootPath = WinPathUtils.Ins.ConfigRootPath("JsonFiles");
        WinPathUtils.Ins.CreateRootFolder(jsonRootPath);

        for (int i = 0; i < subFolders.Length; i++)
        {
            //Excel分类文件夹
            string excelFolderPath = Path.Combine(excelRoot, subFolders[i]);

            //创建Json分类文件夹
            string jsonFolderPath = Path.Combine(jsonRootPath, subFolders[i]);
            WinPathUtils.Ins.CreateRootFolder(jsonFolderPath);

            ConfigToolProcessor.ExportAllJson(excelFolderPath, jsonFolderPath);
        }
        EventPublisher.Message("所有Json 导出完毕\n");
    }

    /// <summary>
    /// 全量导出
    /// </summary>
    void ExportAll()
    {
        if (ExcelIsNull(out string[] subFolders)) return;

        CheckExcel();
        ExportCS();
        ExportJson();
        EventPublisher.Message("所有 配置 校验并 导出完成");
        //ConfigToolProcessor.RunAll(excelFolder, outputCs, outputJson);
    }

    /// <summary>
    /// 判断 Excel 目录是否为空
    /// </summary>
    private bool ExcelIsNull(out string[] subFolders)
    {
        excelRoot = WinPathUtils.Ins.ConfigRootPath("ExcelFiles");
        //获取所有子文件夹
        subFolders = WinPathUtils.Ins.GetRootSubFolders("ExcelFiles");
        if (subFolders.Length == 0)
        {
            EventPublisher.Message("ExcelFiles目录下 不存在任何文件\n");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 显示结果
    /// </summary>
    void ShowResult(string str)
    {
        GameObject obj = Instantiate(contCell, resultTrans);
        ContCell cell = obj.AddComponent<ContCell>();
        cell.SetData(str);
    }

    /// <summary>
    /// 清理结果
    /// </summary>
    void ClearResult()
    {
        int count = resultTrans.childCount;
        for (int i = 0; i < count; i++)
        {
            DestroyImmediate(resultTrans.GetChild(0).gameObject);
        }
    }
}
