using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//class that imports the AVT files into the scene
public class AvatarImporter : AssetPostprocessor
{
	public AvatarImporter()
	{
			
	}
	//called whenever assets are imported into the scene in editing mode
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
	{
		foreach (string asset in importedAssets)
		{
			var ext = (asset.Substring(asset.LastIndexOf(".") + 1));
			if (ext.ToLower() == "avt")
			{
				ImportAVT(asset);
			}
		}
	}
	
	//calls the function that de-serializes and forms the player
	private static void ImportAVT(string fileName)
	{
		Debug.Log("Importing AVT File: " + fileName);
		DeserializeJSON.CallDeserialize(fileName);

	}
}