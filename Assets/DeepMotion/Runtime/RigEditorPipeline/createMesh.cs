using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

//class that maps the FBX mesh to the simulation bones
public class createMesh : MonoBehaviour {

	public GameObject FBX_file;
	[HideInInspector]public List<string> boneList;
	[HideInInspector]public List<string> targetList;

	// Use this for initialization
	void Start () {
		MatchBones();
	}

	//called before the start of this class, it populates the local lists of bons and their corresponding target bones
	public void setBoneMapping(List<string> boneListIn, List<string> targetListIn)
	{
		boneList = boneListIn;
		targetList = targetListIn;
	}

	// matches the simulation bones to the right mesh based on target bones
	public void MatchBones()
	{
		GameObject boneHierarchy = this.gameObject;
		if (FBX_file != null)
		{
			var rigChildren = FBX_file.GetComponentsInChildren<Transform>();
			FBX_file.transform.position = this.transform.position;

			foreach (var rigChild in rigChildren)
			{
				if (rigChild.GetComponent<SkinnedMeshRenderer>() != null)
				{
					rigChild.transform.parent = boneHierarchy.transform;
				}

				for(int i=0;i<targetList.Count;i++)
				{
					if (rigChild.name == targetList[i])
					{
						rigChild.transform.parent = transform.Find(boneList[i]).transform;
					}
				}
			}
		}
	}
}
