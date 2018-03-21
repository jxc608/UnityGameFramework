using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;

public class AddAutoTestScript : EditorWindow
{
    static AddAutoTestScript m_AddScriptEditor = null;

    AutoTestEditor m_Owner = null;
    float m_ButtonWidth;

    public static void Open(AutoTestEditor owner, Vector2 position, float buttonWidth)
    {
        var rect = new Rect(position, new Vector2(300, 800));
        if (m_AddScriptEditor == null)
            m_AddScriptEditor = EditorWindow.GetWindowWithRect<AddAutoTestScript>(rect, true, "添加测试脚本");
        else
            m_AddScriptEditor.position = rect;

        m_AddScriptEditor.m_Owner = owner;
        m_AddScriptEditor.m_ButtonWidth = buttonWidth;
        m_AddScriptEditor.Show();
    }

    string _newScriptName = "";

    void OnGUI()
    {
        GUILayout.Label("脚本名");
        _newScriptName = GUILayout.TextField(_newScriptName, GUILayout.Width(m_ButtonWidth), GUILayout.ExpandWidth(true));
        if (GUILayout.Button("添加测试脚本", GUILayout.Width(m_ButtonWidth)))
        {
            m_Owner.CreateScript(_newScriptName);
            m_AddScriptEditor.Close();
        }
    }

}

public class AutoTestEditor : EditorWindow {
    
    static AutoTestEditor m_AutoTestEditor = null;

    static float LeftWidth = 100;
    static float MinHeight = 100;
    static float ButtonWidth = 90;
    static float Edge = 10f;
    static float DefaultX = 220f;
    static float WindowMinWidth = 800;
    static float WindowMinHeight = 300;


    [MenuItem("自定义/自动化/自动化脚本编辑器 %t", false, 4)]
    static void OpenAutoTestWindow()
    {
        m_AutoTestEditor = EditorWindow.GetWindow<AutoTestEditor>(false, "自动化脚本");
        m_AutoTestEditor.minSize = new Vector2(WindowMinWidth, WindowMinHeight);
        m_AutoTestEditor.Show();
    }

    private void Awake()
    {
        GetFileList();
    }

    void OnFocus()
    {
        if (m_AutoTestEditor)
            m_AutoTestEditor.Repaint();
    }

    AutoActionContainer m_CurrentAction;
    void OnGUI()
    {
        if (m_AutoTestEditor == null)
            return;

        //_lastScriptIndex = _selectedScriptIndex;
        //_lastKey = _currentKey;

        // 左侧脚本列表
        if (DrawScriptList())
            return;

        GUILayout.BeginArea(new Rect(Edge * 2 + LeftWidth, Edge,
                                   WindowMinWidth - Edge * 3 - LeftWidth, MinHeight),
                       GUI.skin.GetStyle("RL Background"));
        m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

        // 脚本信息
        if(DrawContainer(DefaultX, m_CurrentAction))
        {
            return;
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.EndArea();
    }

    List<FileInfo> m_FileList = new List<FileInfo>();
    void GetFileList()
    {
        m_FileList.Clear();
        m_FileList = MiscUtils.GetFileInfoFromFolder(Path.Combine(Application.dataPath, "Resources/AutoTest"), "*.json", SearchOption.AllDirectories, ".meta");
    }

    public void CreateScript(string newScriptName)
    {
        bool create = true;
        var fileFullPath = Path.Combine(Path.Combine(Application.dataPath, "Resources/AutoTest/"), newScriptName + ".json");
        if (File.Exists(fileFullPath))
        {
            if (!EditorUtility.DisplayDialog("⚠️ 警告!", string.Format("已存在名为{0}的脚本, 是否覆盖?", newScriptName), "确认", "取消"))
            {
                create = false;
            }
        }
        if (create)
        {
            var parentFolder = Path.GetDirectoryName(fileFullPath);
            if (!Directory.Exists(parentFolder))
                Directory.CreateDirectory(parentFolder);
            File.Create(fileFullPath).Close();
            AssetDatabase.Refresh();
            m_CurrentAction = null;
            GetFileList();
            SaveScript();
        }
    }

    void SaveScript()
    {
        if (m_CurrentAction != null)
        {
            string json = AutoActionJsonWriter.Write(m_CurrentAction);
            File.WriteAllText(m_FileList[m_SelectScriptIndex].FullName, json);
            AssetDatabase.Refresh();
        }
    }

    private Vector2 m_LeftScrollPos;
    private int m_SelectScriptIndex;
    private Vector2 m_ScrollPos;
    // 左侧脚本列表
    bool DrawScriptList()
    {
        GUILayout.BeginArea(new Rect(Edge, Edge,
                                     LeftWidth, m_AutoTestEditor.position.height - Edge * 2),
                             GUI.skin.GetStyle("AS TextArea"));
        m_LeftScrollPos = EditorGUILayout.BeginScrollView(m_LeftScrollPos);

        if (GUILayout.Button("添加脚本", GUILayout.ExpandWidth(true)))
        {
            AddAutoTestScript.Open(this, new Vector2(LeftWidth, Edge), ButtonWidth);
        }

        EditorGUILayout.Space();

        // 所有脚本列表
       
        var files = new string[m_FileList.Count];
        for (int i = 0; i < m_FileList.Count; ++i)
        {
            files[i] = m_FileList[i].Name;
        }

       
        int oldIndex = m_SelectScriptIndex;
        m_SelectScriptIndex = GUILayout.SelectionGrid(m_SelectScriptIndex, files, 1);
        if (oldIndex != m_SelectScriptIndex)
            PrepareCurrentScript();

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();

        return false;
    }

    //右侧Containner
    bool DrawContainer(float startX, AutoActionContainer container)
    {
        #region head
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("保存脚本", GUILayout.Width(ButtonWidth)))
        {
            SaveScript();
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("删除脚本", GUILayout.Width(ButtonWidth)))
        {
            if (EditorUtility.DisplayDialog("⚠️ 提示!", string.Format("确定要删除名为{0}的脚本?", m_FileList[m_SelectScriptIndex].Name),
                                                                                    "确认", "取消"))
            {
                File.Delete(m_FileList[m_SelectScriptIndex].FullName);
                AssetDatabase.Refresh();
                m_CurrentAction = null;
                GetFileList();
            }
            return true;
        }
      
        EditorGUILayout.EndHorizontal();
        #endregion
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("--Container--");
        EditorGUILayout.EndHorizontal();


        return false;
    }

    // 准备当前脚本信息
    void PrepareCurrentScript()
    {
        if(m_FileList.Count > 0)
        {
            string json = File.ReadAllText(m_FileList[m_SelectScriptIndex].FullName);
            JsonData data = JsonMapper.ToObject(json);
            m_CurrentAction = AutoActionJsonParser.Parse(data) as AutoActionContainer;
        }
    }
}
