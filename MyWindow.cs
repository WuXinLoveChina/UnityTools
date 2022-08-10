using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Text;
public class MyWindow : EditorWindow
{
    [MenuItem("Tools/OpenCreateCSVWindow")]
    static void Open()
    {
        MyWindow window = EditorWindow.GetWindow<MyWindow>("生成CSV的C#Data类");
        window.Show();
    }
    char tab = '\t';
    char row = '\r';
    string[] fileTexts;
    string[] fileNames;
    private void OnGUI()
    {
        EditorGUILayout.LabelField("读取Resources/Config下 所有配置文件，生成对应数据类和管理类");
        if (GUILayout.Button("创建"))
        {
            ReadAllFile();
            if (fileTexts != null)
            {
                for (int i = 0; i < fileTexts.Length; i++)
                {
                    CreateCSVClass(fileTexts[i],fileNames[i]);
                }
                AssetDatabase.Refresh();
            }
            else
            {
                ChinarMessage.MessageBox(IntPtr.Zero, "Config文件夹下没有文件", "错误！", 0);
            }
        }
    }
    /// <summary>
    /// 读取config 文件夹下所有文件 
    /// </summary>
    private void ReadAllFile()
    {
        string[] files= Directory.GetFiles(Application.dataPath + "/Resources/Config", "*.txt");
        fileTexts = new string[files.Length];
        fileNames = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            fileTexts[i] = File.ReadAllText(files[i]);
            fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
    }



    /// <summary>
    /// 判断 输入的字符串是否全是字母
    /// </summary>
    private bool IsALLLetter(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        char[] arr = str.ToCharArray();
        foreach (char item in arr)
        {
            if (!char.IsLetter(item))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 第一行  变量名称
    /// 第二行  变量类型
    /// </summary>
    private void CreateCSVClass(string str,string className)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        if (File.Exists(Application.dataPath + "/Scripts/ConfigData/" + className + ".cs"))
        {
            ChinarMessage.MessageBox(IntPtr.Zero, "该文件已存在", "错误提示!", 0);
            return;
        }
        string[] data = str.Split(row);
        string[] one = data[0].Split(tab);
        string[] two = data[1].Split(tab);
        if (one.Length != two.Length)
        {
            ChinarMessage.MessageBox(IntPtr.Zero, "变量名和变量类型数量不一致！", "错误提示!", 0);
            return;
        }
        StringBuilder sb = new StringBuilder();
        sb.Append("using System;\n");
        sb.Append("public class ");
        sb.Append(className);
        sb.Append("{\n");
        string[] fieldTypes = new string[two.Length];
        for (int i = 0; i < one.Length - 1; i++)
        {
            
            sb.Append("\tpublic ");

            char[] chars = two[i].ToCharArray();
            if (i == 0)
            {
                char[] arr = new char[chars.Length - 1];
                for (int j = 1; j < chars.Length; j++)
                {
                    arr[j - 1] = char.ToLower(chars[j]);
                }
                fieldTypes[i] = new string(arr);
                sb.Append(arr);
            }
            else
            {
                for (int j = 0; j < chars.Length; j++)
                {
                    chars[j] = char.ToLower(chars[j]);
                }
                fieldTypes[i] = new string(chars);
                sb.Append(chars);
            }
           
            sb.Append(" ");
            sb.Append(one[i]);
            sb.Append(";\n");
        }
        //因为含有\r符号，所以要进行额外处理

        sb.Append("}");
        WriteFile(className, sb.ToString());

        CreateDataList(className,fieldTypes,one);
    }
    /// <summary>
    /// 生成数据 管理
    /// </summary>
    private void CreateDataList(string className,string[] fieldType,string[] fieldName)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("using System;\nusing System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n ");
        sb.Append("public class ");
        sb.Append(className);
        sb.Append("List\n{\n");

        sb.Append($"public const string DataPath=\"Config/{className}\";");
        sb.Append($"\nDictionary<int,{className}> dic=new Dictionary<int,{className}>();\n");

        #region 添加方法
        sb.Append($"public void Add(int id,{className} data)\n");
        sb.Append("{\n");
        sb.Append("if(!dic.ContainsKey(id))\n{\ndic.Add(id,data);\n}\n}\n");
        #endregion
        #region 删除方法
        sb.Append("public void Remove(int id)\n{\n");
        sb.Append("if(dic.ContainsKey(id))\n{\ndic.Remove(id);\n}\n}\n");
        #endregion
        #region 查找
        sb.Append($"public {className} GetData(int id)\n");
        sb.Append("{\n");
        sb.Append($"{className} data;\n");
        sb.Append("if(dic.TryGetValue(id,out data))\n{\nreturn data;\n}\n");
        sb.Append("return null;\n");
        sb.Append("}\n");
        #endregion
        #region 修改
        sb.Append($"public void SetValue(int id,{className} data)\n");
        sb.Append("{\n");
        sb.Append($"{className} d;\n");
        sb.Append("if(dic.TryGetValue(id,out data))\n{\n d=data;\n}\n");
        sb.Append("}\n");
        #endregion
        #region 初始化
        sb.Append("public void Init()\n{\n");
        sb.Append("string str=Resources.Load<TextAsset>(DataPath).ToString();\n");
        sb.Append("string[] arr=str.Split('\\n');\n");
        sb.Append("for (int i=5;i<arr.Length-1;i++)\n{\n");
        sb.Append("string[] data=arr[i].Split('\\t');\n");
        sb.Append($"{className} a=new {className}();\n");
        for (int i = 0; i < fieldType.Length; i++)
        {
            switch (fieldType[i])
            {
                case "int":sb.Append($"a.{fieldName[i]}=int.Parse(data[{i}]);\n"); break;
                case "string": sb.Append($"a.{fieldName[i]}=data[{i}];\n"); break;
                case "float": sb.Append($"a.{fieldName[i]}=float.Parse(data[{i}]);\n"); break;
                 default:
                    break;
            }
            
        }
        sb.Append($"Add(a.{fieldName[0]},a);\n");
        sb.Append("}\n");
        sb.Append("}\n");
        #endregion
        sb.Append("}");
        WriteFile(className + "List", sb.ToString());
    }
    private void WriteFile(string name, string str)
    {
        File.WriteAllText(Application.dataPath + "/Scripts/ConfigData/" + name + ".cs", str);
    }
}
