using ExcelDataReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ConfigToolProcessor
{
    /// <summary>
    /// 执行全部操作：校验、生成 C#、导出 Json
    /// </summary>
    public static void RunAll(string excelDir, string csDir, string jsonDir)
    {
        ValidateAll(excelDir);
        GenerateAllCS(excelDir, csDir);
        GenerateAllCSByInterface(excelDir, csDir);
        ExportAllJson(excelDir, jsonDir);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        //Debug.Log("配置导出完成");
        EventPublisher.Message("配置校验并导出完成");
    }

    /// <summary>
    /// 校验指定目录下的所有 Excel 文件
    /// </summary>
    public static void ValidateAll(string excelDir)
    {
        foreach (var file in Directory.GetFiles(excelDir, "*.xlsx"))
        {
            ValidateExcel(file);
        }
        //Debug.Log("Excel 校验完成");
    }

    /// <summary>
    /// 验证指定Excel文件中的所有ID是否唯一
    /// </summary>
    /// <remarks>该方法从Excel文件的第五行开始检查是否存在重复的ID。如果
    /// 发现重复的ID，则每次出现都会记录一个错误。</remarks>
    /// <param name="path">要验证的Excel文件的文件路径。不得为null或空。</param>
    static void ValidateExcel(string path)
    {
        var rows = Read(path);
        var idSet = new HashSet<string>();

        for (int i = 4; i < rows.Count; i++)
        {
            string id = rows[i][0];
            if (!idSet.Add(id))
            {
                Debug.LogError($"[{Path.GetFileName(path)}] id 重复: {id}");
                EventPublisher.Message($"[{Path.GetFileName(path)}] id 重复: {id}");
            }
            //else
            //{
            //    EventPublisher.Message($"[{Path.GetFileName(path)}] id 校验完成: {id}");
            //}
        }
        //Debug.Log($"[{Path.GetFileName(path)}] 校验通过");
        EventPublisher.Message($"[{Path.GetFileName(path)}] 校验完毕");
    }

    /// <summary>
    /// 导出所有 C# 配置类
    /// </summary>
    public static void GenerateAllCS(string excelDir, string csDir)
    {
        Directory.CreateDirectory(csDir);

        foreach (var file in Directory.GetFiles(excelDir, "*.xlsx"))
        {
            var className = Path.GetFileNameWithoutExtension(file).Replace("Config_", "");
            var rows = Read(file);

            var fields = rows[0];
            var types = rows[1];
            var comments = rows[2];

            string code = GenerateClass(className + "Config", fields, types, comments);
            File.WriteAllText(Path.Combine(csDir, className + "Config.cs"), code);

            EventPublisher.Message($"{className}Config.cs 导出完成");
        }
    }

    /// <summary>
    /// 导出所有 C# 配置类(带接口)
    /// </summary>
    public static void GenerateAllCSByInterface(string excelDir, string csDir)
    {
        Directory.CreateDirectory(csDir);

        foreach (var file in Directory.GetFiles(excelDir, "*.xlsx"))
        {
            var className = Path.GetFileNameWithoutExtension(file).Replace("Config_", "");
            var rows = Read(file);

            var fields = rows[0];
            var types = rows[1];
            var comments = rows[2];

            string code = GenerateClassByInterface(className + "Config", fields, types, comments);
            File.WriteAllText(Path.Combine(csDir, className + "Config.cs"), code);

            EventPublisher.Message($"{className}Config.cs 导出完成(带接口)");
        }
    }

    /// <summary>
    /// 导出所有 Json 配置文件
    /// </summary>
    public static void ExportAllJson(string excelDir, string jsonDir)
    {
        Directory.CreateDirectory(jsonDir);

        foreach (var file in Directory.GetFiles(excelDir, "*.xlsx"))
        {
            var rows = Read(file);
            var fields = rows[0];
            var types = rows[1];
            var list = new List<Dictionary<string, object>>();

            for (int i = 3; i < rows.Count; i++)
            {
                var dic = new Dictionary<string, object>();
                for (int j = 0; j < fields.Count; j++)
                {
                    dic[fields[j]] = ParseValue(types[j], rows[i][j]);
                }
                list.Add(dic);
            }

            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            string str = $"{{\"Data\":{json}}}";

            string name = Path.GetFileNameWithoutExtension(file).Replace("Config_", "");
            //屏蔽 BOM
            var utf8NoBom = new UTF8Encoding(false);
            File.WriteAllText(Path.Combine(jsonDir, name + "Config.json"), str, utf8NoBom);

            EventPublisher.Message($"{name}Config.json 导出完成");
        }
        //Debug.Log("Json 导出完成");
    }

    /// <summary>
    /// 控制不同类型的解析
    /// </summary>
    private static object ParseValue(string type, string raw)
    {
        raw = raw?.Trim() ?? string.Empty;

        switch (type)
        {
            case "int":
                return int.TryParse(raw, out var i) ? i : 0;
            case "float":
                return float.TryParse(raw, out var f) ? f : 0f;
            case "double":
                return double.TryParse(raw, out var d) ? d : 0d;
            case "bool":
                return raw == "1" || raw.ToLower() == "true";
            case "string":
                return raw;
            case "List<int>":
                return ParseList<int>(raw, int.Parse);
            case "List<string>":
                return ParseList<string>(raw, s => s);
            case "Dictionary<int,string>":
                return ParseDictionary<int, string>(raw, int.Parse, s => s);
            default:
                Debug.LogError($"不支持的类型: {type}");
                return null;
        }
    }

    /// <summary>
    /// 格式化解析字典类型
    /// </summary>
    private static List<T> ParseList<T>(string raw, Func<string, T> converter)
    {
        var list = new List<T>();
        if (string.IsNullOrEmpty(raw)) return list;

        var parts = raw.Split('|');
        foreach (var p in parts)
        {
            list.Add(converter(p));
        }
        return list;
    }

    /// <summary>
    /// 读取 Excel 内容
    /// </summary>
    private static List<List<string>> Read(string path)
    {
        var result = new List<List<string>>();
        using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
        using (var reader = ExcelReaderFactory.CreateReader(stream))
        {
            while (reader.Read())
            {
                var row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetValue(i)?.ToString() ?? "");
                }
                result.Add(row);
            }
        }
        return result;
    }

    /// <summary>
    /// 生成 C# 类代码
    /// </summary>
    private static string GenerateClass(string className, List<string> fields, List<string> types, List<string> comments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated> 请勿修改 </auto-generated>"); 
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("namespace Suplike.Config");
        sb.AppendLine("{");
        sb.AppendLine("    [Serializable]");
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");

        for (int i = 0; i < fields.Count; i++)
        {
            sb.AppendLine($"        //{comments[i]}");
            sb.AppendLine($"        public {types[i]} {fields[i]};");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// 生成C#类（带接口）
    /// </summary>
    private static string GenerateClassByInterface(string className, List<string> fields, List<string> types, List<string> comments)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated> 请勿修改 </auto-generated>");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine("namespace Suplike.Config");
        sb.AppendLine("{");
        sb.AppendLine("    [Serializable]");
        sb.AppendLine($"    public class {className} : IConfigWithId");
        sb.AppendLine("    {");

        // ===== IConfigWithId =====
        if (!string.IsNullOrEmpty(comments[0]))
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// {comments[0]}");
            sb.AppendLine("        /// </summary>");
        }
        sb.AppendLine("        public int Id { get; set; }");
        sb.AppendLine();

        // ===== 字段 =====
        for (int i = 0; i < fields.Count; i++)
        {
            // 跳过 Id，避免重复
            if (fields[i].Equals("Id", StringComparison.OrdinalIgnoreCase)) continue;

            if (!string.IsNullOrEmpty(comments[i]))
            {
                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// {comments[i]}");
                sb.AppendLine("        /// </summary>");
            }
            sb.AppendLine($"        public {types[i]} {fields[i]};");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// 解析字典类型
    /// </summary>
    private static Dictionary<TKey, TValue> ParseDictionary<TKey, TValue>(string raw, Func<string, TKey> keyConverter, Func<string, TValue> valueConverter)
    {
        var dict = new Dictionary<TKey, TValue>();
        if (string.IsNullOrEmpty(raw)) return dict;

        var pairs = raw.Split('|');
        foreach (var pair in pairs)
        {
            var kv = pair.Split(':');
            if (kv.Length != 2) continue;

            dict[keyConverter(kv[0])] = valueConverter(kv[1]);
        }
        return dict;
    }

}