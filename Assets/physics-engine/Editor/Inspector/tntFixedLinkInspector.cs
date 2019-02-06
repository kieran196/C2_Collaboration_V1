using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(tntFixedLink))]
[CanEditMultipleObjects]

public class tntFixedLinkInspector : tntChildLinkInspector
{
	public void OnEnable()
	{
		BindProperties();
	}

	public override void OnInspectorGUI()
	{
		UpdateGUI();
		if (GUI.changed)
	    {
			SaveOldProperties();
			serializedObject.ApplyModifiedProperties();
			CheckModifiedProperties();
	    }
	}
}

