using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

//static class called by the AVT converter to allow creation of new Player if valifd file exists
public static class DeserializeJSON
{
	//creates the instance of the class that de-serializes and creates the bone hierarchy
	public static void CallDeserialize(string fileName)
	{
		if (File.Exists(fileName))
		{
		    createInstance.Create(fileName);
		}
		else
		{
			Debug.LogError("Cannot find file!");
		}
	}
}
