using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////////////////////////
//  
//  Use with tntHumanoidController for best results
//
///////////////////////////////////////////////////////////////////////////////////////////////////

public class CreatePDParamSetWindow : EditorWindow {
    private CreatePDParamSet PDParamsClass;
    private static CreatePDParamSetWindow window;

    [MenuItem("Assets/Create/PDParamSet Object")]
    static void Init() {
        window = (CreatePDParamSetWindow)EditorWindow.GetWindow(typeof(CreatePDParamSetWindow));
        window.PDParamsClass = new CreatePDParamSet();
        window.PDParamsClass.PDParamSetName = "PD-new";

        GameObject tmpSelection = null;
        if (Selection.transforms.Length > 0) {
            tmpSelection = Selection.transforms[0].gameObject;
        }
        if (tmpSelection != null && tmpSelection.GetComponentInChildren<tntBase>() != null) {
            window.PDParamsClass.selected = Selection.transforms[0].gameObject;
        }
        window.PDParamsClass.PDParamSetName = window.PDParamsClass.selected.name + "PD";
        window.Show();
    }

    void OnGUI() {
        if (PDParamsClass == null)
            return;
        PDParamsClass.OnGUI();
    }
}

public class CreatePDParamSet {
    private const string sObjectPath = "Assets/shared-assets-lfs/Monster/Script Objects/PDParams/";
    public string PDParamSetName;
    public GameObject selected;

    void SaveScriptObject()
    {
        PDParamSet asset = new PDParamSet();
        asset.FillPDParamSet(selected);
        string actualPath = sObjectPath + PDParamSetName + ".asset";
        AssetDatabase.CreateAsset(asset, actualPath);
        EditorUtility.DisplayDialog("PDParams Created", actualPath, "OK");
    }

    public void SaveTxtObject() {
        PDParamSet asset = ScriptableObject.CreateInstance<PDParamSet>();
        asset.FillPDParamSet(selected);
        string txtPath = sObjectPath + selected.name + "_PD.txt";
        string txtData = selected.name + "\nLink Count : " + asset.m_numLinks + "\n";
        for (int i = 0; i < asset.m_numLinks; i++) {
            txtData += WriteLinkPDParams(asset, i);
        }
        System.IO.File.WriteAllText(txtPath, txtData);
        EditorUtility.DisplayDialog("PDParams Created", txtPath, "OK");
    }

    string WriteLinkPDParams(PDParamSet asset, int linkIndex)
    {
        string paramLines = "";
        int offset = linkIndex * (PDParamSet.PD_REL + 1);
        paramLines += "Link : " + asset.m_linkNames[linkIndex] + "\n";
        paramLines += "\tControlled : " + (asset.m_pdParams[offset + PDParamSet.PD_CONTROLLED] == 1 ? "true" : "false") + "\n";
        paramLines += "\tKp : " + asset.m_pdParams[offset + PDParamSet.PD_KP] + "\n";
        paramLines += "\tKpModifier : " + asset.m_pdParams[offset + PDParamSet.PD_KPMOD] + "\n";
        paramLines += "\tKd : " + asset.m_pdParams[offset + PDParamSet.PD_KD] + "\n";
        paramLines += "\tKdModifier : " + asset.m_pdParams[offset + PDParamSet.PD_KDMOD] + "\n";
        paramLines += "\tMaxAbsoluteTorque : " + asset.m_pdParams[offset + PDParamSet.PD_MAX_ABS_TORQUE] + "\n";
        paramLines += "\tScale : (" + asset.m_pdParams[offset + PDParamSet.PD_SCALE] + ", " +
            asset.m_pdParams[offset + PDParamSet.PD_SCALE + 1] + ", " +
            asset.m_pdParams[offset + PDParamSet.PD_SCALE + 2] + ")\n";
        paramLines += "\tRelToCharFrame : " + (asset.m_pdParams[offset] == 1 ? "true" : "false") + "\n";
        return paramLines;
    }

    public void OnGUI() {
        selected = (GameObject)EditorGUILayout.ObjectField("Character top level", selected as Object, typeof(GameObject), true);
        PDParamSetName = EditorGUILayout.TextField("ParamSet file name", PDParamSetName);
        if (GUILayout.Button("Create PD control params")) {
            SaveScriptObject();
        }
        if (GUILayout.Button("Create PD param text")) {
            SaveTxtObject();
        }
    }
}