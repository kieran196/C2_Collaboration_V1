#define COMPOUND
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Linq;

//class that creates the player instance
public static class createInstance {

    public static GameObject BlueRobot, humanoidController, root;
    static tntHumanoidControlParams controlParams;
    static JArray jsonColliderObj;
    static JObject jsonBoneObj, jsonParamsObj, jsonControllerObj;
    static List<string> boneList = new List<string>();
    static List<string> targetList = new List<string>();

    //creates the new Player character from JSON String
    public static void CreateFromJSON(string json)
    {
        JArray array = JArray.Parse(json);

        //de-serialize one by one
        jsonBoneObj = JObject.Parse(array[0].ToString());
        jsonColliderObj = JArray.Parse(array[1].ToString());
        jsonParamsObj = JObject.Parse(array[2].ToString());
        jsonControllerObj = JObject.Parse(array[3].ToString());

        //de-serialize the bone hierarchy
        DeserializeEach(jsonBoneObj);

        //count childLinks to populate the TnTBase member childLinks
        tntChildLink[] childLinks = UnityEngine.Object.FindObjectsOfType(typeof(tntChildLink)) as tntChildLink[];
        List<tntChildLink> childLinkList = new List<tntChildLink>(childLinks);
        root.GetComponent<tntBase>().m_childLinks = childLinkList;

        //taking care of the handed-ness difference in Unity and Maya/Web-pipeline
        var boneChildren = BlueRobot.GetComponentsInChildren<tntChildLink>();
        foreach (tntChildLink child in boneChildren)
        {
            child.transform.position = new Vector3(-child.transform.position.x, child.transform.position.y, child.transform.position.z);
        }

        //creating control paramters
        createControlParamObject(jsonParamsObj);

        //update and create the humaoid controller
        updateHumanoidController(jsonControllerObj);
    }

    //creates the new Player character
    public static void Create(string fileName)
	{
	    //check if file exists
	    if (File.Exists(fileName))
	    {
            Debug.Log(fileName);
		    //create parent GameObject
		    string[] words = fileName.Split('/');
		    int fileLength = words.Length;
		    string playerNameWithExt = words[fileLength - 1];
		    string[] temp = playerNameWithExt.Split('.');
		    string playerName = temp[0];
		    BlueRobot = new GameObject(playerName);

	        BlueRobot.AddComponent<DeepMotionAvatar>();

            // deserialize from the json string
            string dataAsJson = File.ReadAllText(fileName);
		    CreateFromJSON(dataAsJson);

	        //add custom mesh class that allows FBX to be mapped into the simulation rig
	        BlueRobot.AddComponent<createMesh>();
	        GenerateTargetBoneList(jsonBoneObj);
	        BlueRobot.GetComponent<createMesh>().setBoneMapping(boneList, targetList);
        }
		else
		{
			Debug.LogError("Cannot find file!");
		}
	}

	//recursive function to go through all the children and generate corresponding TnT classes
	public static void DeserializeEach(JObject jsonObjIn)
	{
		Dictionary<string, object> dictObj = jsonObjIn.ToObject<Dictionary<string, object>>();

		//root joint
		if (dictObj["joint_type"].ToString() == "base")
		{
			getBaseLink(dictObj);
		}

		//all joints other than root
		if (dictObj["children"] != null)
		{
			foreach (JObject child in jsonObjIn["children"])
			{
				Dictionary<string, object> childDictObj = child.ToObject<Dictionary<string, object>>();
				string joint = child["joint_type"].ToString();
				getChildLink(joint, childDictObj);
				DeserializeEach(child);
			}
		}
	}

	//recursive function that populates the lists of bone names and their correspoding target bone names for FBX matching
	public static void GenerateTargetBoneList(JObject jsonObjIn)
	{
		Dictionary<string, object> dictObjIn = jsonObjIn.ToObject<Dictionary<string, object>>();
		
		string targetBoneName = dictObjIn["target_bone"].ToString();
		string boneName = dictObjIn["boneName"].ToString();

		boneList.Add(boneName);
		targetList.Add(targetBoneName);
		if (dictObjIn["children"] != null)
		{
			foreach (JObject child in jsonObjIn["children"])
			{
				GenerateTargetBoneList(child);
			}
		}
	}

	//generates the TnTbase class instance for root joint
	public static void getBaseLink(Dictionary<string, object> dictObj)
	{
		root = new GameObject();
		root.name = dictObj["boneName"].ToString();

		GameObject collider = new GameObject();
		string collType = dictObj["collider_type"].ToString();
		if(collType == null)
			collType = dictObj["colliderType"].ToString();

		collider.transform.parent = root.transform;
		root.AddComponent<tntBase>();
#if COMPOUND
        root.AddComponent<tntCompoundCollider>();
        collider.AddComponent<tntRigidBody>();
#endif

        //add collider component based on the collider type
        if ((collType == "box") || (collType == "Box"))
		{
			collType = "Box";
			collider.AddComponent<BoxCollider>();
		}
		else if ((collType == "Sphere") || (collType == "sphere"))
		{
			collType = "Sphere";
			SphereCollider sphereCollider = collider.AddComponent(typeof(SphereCollider)) as SphereCollider;
			sphereCollider.radius = 0.1f;
		}
		else if ((collType == "capsule") || (collType == "Capsule"))
		{
			collType = "Capsule";
			CapsuleCollider capsuleCollider = collider.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;
			capsuleCollider.radius = 0.1f;
			capsuleCollider.height = 0.5f;
		}
		collider.name = root.name + "_" + collType;

        //iterate through the dictionary and update the corresponding values
        foreach (KeyValuePair<string, object> entry in dictObj)
		{
			string key = entry.Key;
			object value = entry.Value;
			tntBase rootScript = root.GetComponent<tntBase>();
			if (key == "mass")
			{
				rootScript.m_mass = Convert.ToSingle(value);
			}
			if (key == "IsKinematic")
			{
				rootScript.m_IsKinematic = Convert.ToBoolean(value);
			}
			if (key == "angularDamping")
			{
				rootScript.m_angularDamping = Convert.ToSingle(value);
			}
			if (key == "angularDrag")
			{
				rootScript.m_angularDrag = Convert.ToSingle(value);
			}
			if (key == "collidable")
			{
				rootScript.m_collidable = Convert.ToBoolean(value);
			}
			if (key == "computeMoiFromColliders")
			{
				rootScript.m_computeMoiFromColliders = Convert.ToBoolean(value);
			}
			if (key == "drag")
			{
				rootScript.m_drag = Convert.ToSingle(value);
			}
			if (key == "enableSelfCollision")
			{
				rootScript.m_enableSelfCollision = Convert.ToBoolean(value);
			}
			if (key == "highDefIntegrator")
			{
				rootScript.m_highDefIntegrator = Convert.ToBoolean(value);
			}
			if (key == "isOutOfSim")
			{
				rootScript.m_isOutOfSim = Convert.ToBoolean(value);
			}
			if (key == "linearDamping")
			{
				rootScript.m_linearDamping = Convert.ToSingle(value);
			}
			if (key == "maxCoordinateVelocity")
			{
				rootScript.m_maxCoordinateVelocity = Convert.ToSingle(value);
			}
			if (key == "simulationFrequencyMultiplier")
			{
				rootScript.m_simulationFrequencyMultiplier = (int)Convert.ToSingle(value);
			}
			if (key == "useGlobalVel")
			{
				rootScript.m_useGlobalVel = Convert.ToBoolean(value);
			}
			if (key == "moi")
			{
				JArray moiArray = JArray.Parse(Convert.ToString(value));
				rootScript.m_moi.x = (float)moiArray[0];
				rootScript.m_moi.y = (float)moiArray[1];
				rootScript.m_moi.z = (float)moiArray[2];
			}
			if (key == "bone_xform")
			{
				JArray xFormArray = JArray.Parse(Convert.ToString(value));
				float[] boneXformArr = new float[16];
				for (int i = 0; i < 16; i++)
				{
					boneXformArr[i] = (float)xFormArray[i];
				}
				calcTx(boneXformArr, root, root);
			}
#if !COMPOUND
            if (key == "compound_collider")
            {
				bool compoundColliderValue = Convert.ToBoolean(value);
				if(compoundColliderValue == true)
				{
					root.AddComponent<tntCompoundCollider>();
					collider.AddComponent<tntRigidBody>();
				}
			}
#endif
        }
		updateColliders(collType, root, "base");
		humanoidController = new GameObject();
		humanoidController.name = "Humanoid Controller";
		humanoidController.transform.parent = root.transform;
		humanoidController.AddComponent<tntHumanoidController>();
		root.transform.parent = BlueRobot.transform;
	}

	//generate TntChild class instance for the child joint based on joint type
	public static void getChildLink(string joint, Dictionary<string, object> dictObj)
	{
		GameObject child = new GameObject();
		child.name = dictObj["boneName"].ToString();

		GameObject collider = new GameObject();
		string collType = dictObj["collider_type"].ToString();
		if(collType == null)
			collType = dictObj["colliderType"].ToString();

		collider.transform.parent = child.transform;
#if COMPOUND
        child.AddComponent<tntCompoundCollider>();
        collider.AddComponent<tntRigidBody>();
#endif

        if ((collType == "box") || (collType == "Box"))
		{
			collType = "Box";
			collider.AddComponent<BoxCollider>();
		}
		else if ((collType == "Sphere") || (collType == "sphere"))
		{
			collType = "Sphere";
			collider.AddComponent<SphereCollider>();
		}
		else if ((collType == "capsule") || (collType == "Capsule"))
		{
			collType = "Capsule";
			collider.AddComponent<CapsuleCollider>();
		}
		collider.name = child.name + "_" + collType;

		string par = dictObj["parent"].ToString();
		GameObject parentObj = null;

		foreach (Transform childObj in BlueRobot.transform)
		{
			if(childObj.gameObject.name == par)
		{
				parentObj = childObj.gameObject;
		}

		}
		if (joint == "ball")
		{
			child.AddComponent<tntBallLink>();
			foreach (KeyValuePair<string, object> entry in dictObj)
			{
				string key = entry.Key;
				object value = entry.Value;
				tntBallLink childBallScript = child.GetComponent<tntBallLink>();
				if (key == "mass")
				{
					childBallScript.m_mass = Convert.ToSingle(value);
				}
				if (key == "collideWithParent")
				{
					childBallScript.m_collideWithParent = Convert.ToBoolean(value);
				}
				if (key == "breakingReactionImpulse")
				{
					childBallScript.m_breakingReactionImpulse = Convert.ToSingle(value);
				}
				if (key == "angularDrag")
				{
					childBallScript.m_angularDrag = Convert.ToSingle(value);
				}
				if (key == "collidable")
				{
					childBallScript.m_collidable = Convert.ToBoolean(value);
				}
				if (key == "computeMoiFromColliders")
				{
					childBallScript.m_computeMoiFromColliders = Convert.ToBoolean(value);
				}
				if (key == "drag")
				{
					childBallScript.m_drag = Convert.ToSingle(value);
				}
				if (key == "kp")
				{
					childBallScript.m_kp = Convert.ToSingle(value);
				}
				if (key == "kd")
				{
					childBallScript.m_kd = Convert.ToSingle(value);
				}
				if (key == "maxPDTorque")
				{
					childBallScript.m_maxPDTorque = Convert.ToSingle(value);
				}
				if (key == "parent")
				{
					childBallScript.m_parent = parentObj.GetComponent<tntLink>();
				}
				if (key == "pivotA")
				{
					JArray pivotAArray = JArray.Parse(Convert.ToString(value));
                    Vector3 vec;
					vec.x = -(float)pivotAArray[0] / 100;
					vec.y = (float)pivotAArray[1] / 100;
					vec.z = (float)pivotAArray[2] / 100;
                    childBallScript.PivotA = vec;
				}
				if (key == "pivotB")
				{
					JArray pivotBArray = JArray.Parse(Convert.ToString(value));
                    Vector3 vec;
                    vec.x = -(float)pivotBArray[0] / 100;
					vec.y = (float)pivotBArray[1] / 100;
					vec.z = (float)pivotBArray[2] / 100;
                    childBallScript.PivotB = vec;
                }
				if (key == "showJoint")
				{
					childBallScript.m_showJoint = false;
				}
				if (key == "useSoftConstraint")
				{
					childBallScript.m_useSoftConstraint = Convert.ToBoolean(value);
				}
				if (key == "moi")
				{
					JArray moiArray = JArray.Parse(Convert.ToString(value));
					childBallScript.m_moi.x = (float)moiArray[0];
					childBallScript.m_moi.y = (float)moiArray[1];
					childBallScript.m_moi.z = (float)moiArray[2];
				}
				if (key == "dofData")
				{
					JArray dofDataArray = JArray.Parse(Convert.ToString(value));
					updateDof(dofDataArray, childBallScript);
				}
				if (key == "bone_xform")
				{
					JArray xFormArray = JArray.Parse(Convert.ToString(value));
					float[] boneXformArr = new float[16];
					for (int i = 0; i < 16; i++)
					{
						boneXformArr[i] = (float)xFormArray[i];
					}
					calcTx(boneXformArr, child, parentObj);
				}
#if !COMPOUND
				if (key == "compound_collider")
				{
					bool compoundColliderValue = Convert.ToBoolean(value);
					if (compoundColliderValue == true)
					{
						child.AddComponent<tntCompoundCollider>();
						collider.AddComponent<tntRigidBody>();
					}
				}
#endif
            }
		}
		else if (joint == "hinge")
		{
			child.AddComponent<tntHingeLink>();
			foreach (KeyValuePair<string, object> entry in dictObj)
			{
				string key = entry.Key;
				object value = entry.Value;
				tntHingeLink childHingeScript = child.GetComponent<tntHingeLink>();
				if (key == "mass")
				{
					childHingeScript.m_mass = Convert.ToSingle(value);
				}
				if (key == "collideWithParent")
				{
					childHingeScript.m_collideWithParent = Convert.ToBoolean(value);
				}
				if (key == "breakingReactionImpulse")
				{
					childHingeScript.m_breakingReactionImpulse = Convert.ToSingle(value);
				}
				if (key == "angularDrag")
				{
					childHingeScript.m_angularDrag = Convert.ToSingle(value);
				}
				if (key == "collidable")
				{
					childHingeScript.m_collidable = Convert.ToBoolean(value);
				}
				if (key == "computeMoiFromColliders")
				{
					childHingeScript.m_computeMoiFromColliders = Convert.ToBoolean(value);
				}
				if (key == "drag")
				{
					childHingeScript.m_drag = Convert.ToSingle(value);
				}
				if (key == "kp")
				{
					childHingeScript.m_kp = Convert.ToSingle(value);
				}
				if (key == "kd")
				{
					childHingeScript.m_kd = Convert.ToSingle(value);
				}
				if (key == "maxPDTorque")
				{
					childHingeScript.m_maxPDTorque = Convert.ToSingle(value);
				}
				if (key == "parent")
				{
					childHingeScript.m_parent = parentObj.GetComponent<tntLink>();
				}
				if (key == "pivotA")
				{
					JArray pivotAArray = JArray.Parse(Convert.ToString(value));
                    Vector3 vec;
                    vec.x = -(float)pivotAArray[0] / 100;
                    vec.y = (float)pivotAArray[1] / 100;
                    vec.z = (float)pivotAArray[2] / 100;
                    childHingeScript.PivotA = vec;
				}
				if (key == "pivotB")
				{
					JArray pivotBArray = JArray.Parse(Convert.ToString(value));
                    Vector3 vec;
                    vec.x = -(float)pivotBArray[0] / 100;
                    vec.y = (float)pivotBArray[1] / 100;
                    vec.z = (float)pivotBArray[2] / 100;
                    childHingeScript.PivotB = vec;
                }
				if (key == "axisA")
				{
					JArray axisAArray = JArray.Parse(Convert.ToString(value));
					childHingeScript.m_axisA.x = (float)axisAArray[0];
					childHingeScript.m_axisA.y = (float)axisAArray[1];
					childHingeScript.m_axisA.z = (float)axisAArray[2];
				}
				if (key == "showJoint")
				{
					childHingeScript.m_showJoint = false;
				}
				if (key == "useSoftConstraint")
				{
					childHingeScript.m_useSoftConstraint = Convert.ToBoolean(value);
				}
				if (key == "moi")
				{
					JArray moiArray = JArray.Parse(Convert.ToString(value));
					childHingeScript.m_moi.x = (float)moiArray[0];
					childHingeScript.m_moi.y = (float)moiArray[1];
					childHingeScript.m_moi.z = (float)moiArray[2];
				}
				if (key == "dofData")
				{
					JArray dofDataArray = JArray.Parse(Convert.ToString(value));
					updateDof(dofDataArray, childHingeScript);
					float limitH = childHingeScript.m_dofData[0].m_limitHigh;
					float limitL = childHingeScript.m_dofData[0].m_limitLow;
					if ((limitH != 0) && (limitL != 0))
					{
						childHingeScript.m_dofData[0].m_limitHigh = -limitL;
						childHingeScript.m_dofData[0].m_limitLow = -limitH;
					}
				}
				if (key == "bone_xform")
				{
					JArray xFormArray = JArray.Parse(Convert.ToString(value));
					float[] boneXformArr = new float[16];
					for (int i = 0; i < 16; i++)
					{
						boneXformArr[i] = (float)xFormArray[i];
					}
					calcTx(boneXformArr, child, parentObj);
				}
#if !COMPOUND
				if (key == "compound_collider")
				{
					bool compoundColliderValue = Convert.ToBoolean(value);
					if (compoundColliderValue == true)
					{
						child.AddComponent<tntCompoundCollider>();
						collider.AddComponent<tntRigidBody>();
					}
				}
#endif
            }
		}
		else if (joint == "universal")
		{
			child.AddComponent<tntUniversalLink>();
			foreach (KeyValuePair<string, object> entry in dictObj)
			{
				string key = entry.Key;
				object value = entry.Value;
				tntUniversalLink childUniversalScript = child.GetComponent<tntUniversalLink>();
				if (key == "mass")
				{
					childUniversalScript.m_mass = Convert.ToSingle(value);
				}
				if (key == "collideWithParent")
				{
					childUniversalScript.m_collideWithParent = Convert.ToBoolean(value);
				}
				if (key == "breakingReactionImpulse")
				{
					childUniversalScript.m_breakingReactionImpulse = Convert.ToSingle(value);
				}
				if (key == "angularDrag")
				{
					childUniversalScript.m_angularDrag = Convert.ToSingle(value);
				}
				if (key == "collidable")
				{
					childUniversalScript.m_collidable = Convert.ToBoolean(value);
				}
				if (key == "computeMoiFromColliders")
				{
					childUniversalScript.m_computeMoiFromColliders = Convert.ToBoolean(value);
				}
				if (key == "drag")
				{
					childUniversalScript.m_drag = Convert.ToSingle(value);
				}
				if (key == "kp")
				{
					childUniversalScript.m_kp = Convert.ToSingle(value);
				}
				if (key == "kd")
				{
					childUniversalScript.m_kd = Convert.ToSingle(value);
				}
				if (key == "maxPDTorque")
				{
					childUniversalScript.m_maxPDTorque = Convert.ToSingle(value);
				}
				if (key == "parent")
				{
					childUniversalScript.m_parent = parentObj.GetComponent<tntLink>();
				}
				if (key == "pivotA")
				{
					JArray pivotAArray = JArray.Parse(Convert.ToString(value));
                    Vector3 vec;
					vec.x = -(float)pivotAArray[0] / 100;
					vec.y = (float)pivotAArray[1] / 100;
					vec.z = (float)pivotAArray[2] / 100;
                    childUniversalScript.PivotA = vec;
				}
				if (key == "pivotB")
				{
					JArray pivotBArray = JArray.Parse(Convert.ToString(value));
                    Vector3 vec;
                    vec.x = -(float)pivotBArray[0] / 100;
                    vec.y = (float)pivotBArray[1] / 100;
                    vec.z = (float)pivotBArray[2] / 100;
                    childUniversalScript.PivotB = vec;
                }
				if (key == "axisA")
				{
					JArray axisAArray = JArray.Parse(Convert.ToString(value));
					childUniversalScript.m_axisA.x = (float)axisAArray[0];
					childUniversalScript.m_axisA.y = (float)axisAArray[1];
					childUniversalScript.m_axisA.z = (float)axisAArray[2];
				}
				if (key == "axisB")
				{
					JArray axisBArray = JArray.Parse(Convert.ToString(value));
					childUniversalScript.m_axisB.x = (float)axisBArray[0];
					childUniversalScript.m_axisB.y = (float)axisBArray[1];
					childUniversalScript.m_axisB.z = (float)axisBArray[2];
				}
				if (key == "showJoint")
				{
					childUniversalScript.m_showJoint = false;
				}
				if (key == "useSoftConstraint")
				{
					childUniversalScript.m_useSoftConstraint = Convert.ToBoolean(value);
				}
				if (key == "moi")
				{
					JArray moiArray = JArray.Parse(Convert.ToString(value));
					childUniversalScript.m_moi.x = (float)moiArray[0];
					childUniversalScript.m_moi.y = (float)moiArray[1];
					childUniversalScript.m_moi.z = (float)moiArray[2];
				}
				if (key == "dofData")
				{
					JArray dofDataArray = JArray.Parse(Convert.ToString(value));
					updateDof(dofDataArray, childUniversalScript);
				}
				if (key == "bone_xform")
				{
					JArray xFormArray = JArray.Parse(Convert.ToString(value));
					float[] boneXformArr = new float[16];
					for (int i = 0; i < 16; i++)
					{
						boneXformArr[i] = (float)xFormArray[i];
					}
					calcTx(boneXformArr, child, parentObj);
				}
#if !COMPOUND
				if (key == "compound_collider")
				{
					bool compoundColliderValue = Convert.ToBoolean(value);
					if (compoundColliderValue == true)
					{
						child.AddComponent<tntCompoundCollider>();
						collider.AddComponent<tntRigidBody>();
					}
				}
#endif
            }
		}

		updateColliders(collType, child, joint);
		child.transform.parent = BlueRobot.transform;

	}

	//update dof data array
	public static void updateDof(JArray dofDataArray, tntChildLink childScript)
	{
		int numOfDof = childScript.m_dofData.Length;
		for (int i = 0; i < numOfDof; i++)
		{
			JObject dofIn = JObject.Parse(dofDataArray[i].ToString());

			childScript.m_dofData[i].m_desiredPosition = (float)dofIn["desiredPosition"];
            childScript.m_dofData[i].m_desiredVelocity = (float)dofIn["desiredVelocity"];
			childScript.m_dofData[i].m_isPositionMotor = (bool)dofIn["isPositionMotor"];
			childScript.m_dofData[i].m_limitHigh = (float)dofIn["limitHigh"];
			childScript.m_dofData[i].m_limitLow = (float)dofIn["limitLow"];
			childScript.m_dofData[i].m_maxLimitForce = (float)dofIn["maxLimitForce"];
			childScript.m_dofData[i].m_maxMotorForce = (float)dofIn["maxMotorForce"];
			//childScript.m_dofData[i].m_continuousForce = (float)dofIn["continuousForce"];
			childScript.m_dofData[i].m_neutralPoint = (float)dofIn["neutralPoint"];
			childScript.m_dofData[i].m_positionLockThreshold = (float)dofIn["positionLockThreshold"];
			childScript.m_dofData[i].m_springDamping = (float)dofIn["springDamping"];
			childScript.m_dofData[i].m_springStiffness = (float)dofIn["springStiffness"];
			childScript.m_dofData[i].m_useLimit = (bool)dofIn["useLimit"];
			childScript.m_dofData[i].m_useMotor = (bool)dofIn["useMotor"];
		}
	}

	//calculate the Transformation matrix given the flat 16 array
	public static void calcTx(float[] arrIn, GameObject boneObj, GameObject parentObj)
	{
		Matrix4x4 matrix = new Matrix4x4();
		int j = 0;
		for (int i = 0; i < arrIn.Length;)
		{
			Vector4 row = new Vector4(arrIn[i++], arrIn[i++], arrIn[i++], arrIn[i++]);
			matrix.SetRow(j++, row);
		}
		
		Vector3 position = matrix.GetRow(3) / 100.0f;

		Quaternion rotation = Quaternion.LookRotation(
			matrix.GetRow(2),
			matrix.GetRow(1)
		);
		
		Vector3 scale = new Vector3(
			matrix.GetRow(0).magnitude,
			matrix.GetRow(1).magnitude,
			matrix.GetRow(2).magnitude
		);

		boneObj.transform.position = position + parentObj.transform.position;
		boneObj.transform.rotation = rotation * parentObj.transform.rotation;
        Vector3 curEuler = boneObj.transform.eulerAngles;
        boneObj.transform.eulerAngles = new Vector3(curEuler.x, -curEuler.y, -curEuler.z);
		boneObj.transform.localScale = scale;
	}

	public static Vector3 GetPositionMatrix(this Matrix4x4 matrix)
	{
		var x = matrix.m03;
		var y = matrix.m13;
		var z = matrix.m23;

		return new Vector3(x, y, z);
	}

	//calculate the translation given the flat 16 array
	public static Vector3 calcTr(float[] arrIn)
	{
		Matrix4x4 matrix = new Matrix4x4();
		int j = 0;
		for (int i = 0; i < arrIn.Length;)
		{
			Vector4 row = new Vector4(arrIn[i++], arrIn[i++], arrIn[i++], arrIn[i++]);
			matrix.SetRow(j++, row);
		}
		return matrix.GetPositionMatrix();
	}

	//calculate the scale given the flat 16 array
	public static Vector3 calcSc(float[] arrIn)
	{
		Matrix4x4 matrix = new Matrix4x4();
		int j = 0;
		for (int i = 0; i < arrIn.Length;)
		{
			Vector4 row = new Vector4(arrIn[i++], arrIn[i++], arrIn[i++], arrIn[i++]);
			matrix.SetRow(j++, row);
		}
		Vector3 scale = new Vector3(
			matrix.GetRow(0).magnitude,
			matrix.GetRow(1).magnitude,
			matrix.GetRow(2).magnitude
		);
		return scale;
	}

	//calculate the rotation given the flat 16 array
	public static Quaternion calcRot(float[] arrIn)
	{
		Matrix4x4 matrix = new Matrix4x4();
		int j = 0;
		for (int i = 0; i < arrIn.Length;)
		{
			Vector4 row = new Vector4(arrIn[i++], arrIn[i++], arrIn[i++], arrIn[i++]);
			matrix.SetRow(j++, row);
		}

		Quaternion rotation = Quaternion.LookRotation(
			matrix.GetRow(2),
			matrix.GetRow(1)
		);
		return rotation;
	}

    //update collider information based on the collider type
    static void updateColliders(string collType, GameObject boneObj, string jointType)
	{
		int length = jsonColliderObj.Count;
		for (int i = 0; i < length; i++)
		{
			JObject collJsonIn = JObject.Parse(jsonColliderObj[i].ToString());
			string boneName = (string)collJsonIn["bone"];
			if (boneName == boneObj.name)
			{
				string collName = boneName + "_" + collType;
				GameObject colliderObj = null;

				foreach (Transform childObj in boneObj.transform)
				{
					if (childObj.gameObject.name == collName)
					{
						colliderObj = childObj.gameObject;
					}

				}

				//colliderObj= GameObject.Find(collName);
				float mass = 0;
				float drag = 0;
				float angularDrag = 0;

				if (jointType == "ball")
				{
					tntBallLink ballScript = boneObj.GetComponent<tntBallLink>();
					mass = ballScript.m_mass;
					drag = ballScript.m_drag;
					angularDrag = ballScript.m_angularDrag;
				}
				if (jointType == "hinge")
				{
					tntHingeLink hingeScript = boneObj.GetComponent<tntHingeLink>();
					mass = hingeScript.m_mass;
					drag = hingeScript.m_drag;
					angularDrag = hingeScript.m_angularDrag;
				}
				if (jointType == "universal")
				{
					tntUniversalLink universalScript = boneObj.GetComponent<tntUniversalLink>();
					mass = universalScript.m_mass;
					drag = universalScript.m_drag;
					angularDrag = universalScript.m_angularDrag;
				}
				if (jointType == "base")
				{
					tntBase baseScript = boneObj.GetComponent<tntBase>();
					mass = baseScript.mass;
					drag = baseScript.m_drag;
					angularDrag = baseScript.m_angularDrag;
				}

				Dictionary<string, object> dictObj = collJsonIn.ToObject<Dictionary<string, object>>();
				if (colliderObj)
				{
					if (collType == "Box")
					{
						BoxCollider colliderBox = colliderObj.GetComponent<BoxCollider>();
						tntRigidBody colliderScript;

						foreach (KeyValuePair<string, object> entry in dictObj)
						{
							string key = entry.Key;
							object value = entry.Value;

							if (colliderObj.GetComponent<tntRigidBody>() != null)
							{
								colliderScript = colliderObj.GetComponent<tntRigidBody>();
								if (key == "collidable")
								{
									colliderScript.m_collidable = Convert.ToBoolean(value);
								}
								if (key == "isKinematic")
								{
									colliderScript.m_IsKinematic = Convert.ToBoolean(value);
								}
								colliderScript.m_mass = mass;
								colliderScript.m_drag = drag;
								colliderScript.m_angularDrag = angularDrag;
							}
							
							if (key == "collider_xform")
							{
								JArray xFormArray = JArray.Parse(Convert.ToString(value));
								float[] colliderXformArr = new float[16];
								for (int c = 0; c < 16; c++)
								{
									colliderXformArr[c] = (float)xFormArray[c];
								}

								Vector3 colliderScale = calcSc(colliderXformArr);
								Quaternion colliderRot = calcRot(colliderXformArr);
                                Vector3 colliderTr = calcTr(colliderXformArr);

								colliderBox.transform.localRotation = colliderRot;
                                Vector3 curEuler = colliderBox.transform.localEulerAngles;
                                colliderBox.transform.localEulerAngles = new Vector3(curEuler.x, -curEuler.y, -curEuler.z);

								float boxLength = (float)collJsonIn["length"] * colliderScale.x * 0.01f;
								float boxHeight = (float)collJsonIn["height"] * colliderScale.y * 0.01f;
								float boxBreadth = (float)collJsonIn["breadth"] * colliderScale.z * 0.01f;
								colliderBox.size = new Vector3(boxLength, boxHeight, boxBreadth);
                                colliderBox.transform.localPosition = colliderTr * 0.01f;
							}
							/*
							if (key == "collider_scale")
							{
								JArray collScale = JArray.Parse(Convert.ToString(value));
								float collLength = (float)collScale[0] * 0.01f;
								float collBreadth = (float)collScale[1] * 0.01f;
								float collHeight = (float)collScale[2] * 0.01f;
								colliderBox.size = new Vector3(collLength, collBreadth, collHeight);
							}*/
							
						}
					}
					else if (collType == "Capsule")
					{
						CapsuleCollider colliderCapsule = colliderObj.GetComponent<CapsuleCollider>();
						tntRigidBody colliderScript;
						
						foreach (KeyValuePair<string, object> entry in dictObj)
						{
							string key = entry.Key;
							object value = entry.Value;

							if (colliderObj.GetComponent<tntRigidBody>() != null)
							{
								colliderScript = colliderObj.GetComponent<tntRigidBody>();
								if (key == "collidable")
								{
									colliderScript.m_collidable = Convert.ToBoolean(value);
								}
								if (key == "isKinematic")
								{
									colliderScript.m_IsKinematic = Convert.ToBoolean(value);
								}
								colliderScript.m_mass = mass;
								colliderScript.m_drag = drag;
								colliderScript.m_angularDrag = angularDrag;
							}

							if (key == "collider_xform")
							{
								JArray xFormArray = JArray.Parse(Convert.ToString(value));
								float[] colliderXformArr = new float[16];
								for (int c = 0; c < 16; c++)
								{
									colliderXformArr[c] = (float)xFormArray[c];
								}

								Vector3 colliderScale = calcSc(colliderXformArr);
								Quaternion colliderRot = calcRot(colliderXformArr);
                                Vector3 colliderTr = calcTr(colliderXformArr);

                                colliderCapsule.transform.localRotation = colliderRot;
                                Vector3 curEuler = colliderCapsule.transform.localEulerAngles;
                                colliderCapsule.transform.localEulerAngles = new Vector3(curEuler.x, -curEuler.y, -curEuler.z);

                                colliderCapsule.radius = (float)collJsonIn["radius"] * colliderScale.x * 0.004f;
								colliderCapsule.height = (float)collJsonIn["height"] * colliderScale.y * 0.008f;
                                colliderCapsule.transform.localPosition = colliderTr;
							}
						}
					}
					else if (collType == "Sphere")
					{
						SphereCollider colliderSphere = colliderObj.GetComponent<SphereCollider>();
						tntRigidBody colliderScript;
						foreach (KeyValuePair<string, object> entry in dictObj)
						{
							string key = entry.Key;
							object value = entry.Value;

							if (colliderObj.GetComponent<tntRigidBody>() != null)
							{
								colliderScript = colliderObj.GetComponent<tntRigidBody>();
								if (key == "collidable")
								{
									colliderScript.m_collidable = Convert.ToBoolean(value);
								}
								if (key == "isKinematic")
								{
									colliderScript.m_IsKinematic = Convert.ToBoolean(value);
								}
								colliderScript.m_mass = mass;
								colliderScript.m_drag = drag;
								colliderScript.m_angularDrag = angularDrag;
							}

							if (key == "collider_xform")
							{
								JArray xFormArray = JArray.Parse(Convert.ToString(value));
								float[] colliderXformArr = new float[16];
								for (int c = 0; c < 16; c++)
								{
									colliderXformArr[c] = (float)xFormArray[c];
								}

								Vector3 colliderScale = calcSc(colliderXformArr);
                                Vector3 colliderTr = calcTr(colliderXformArr);

                                colliderSphere.radius = (float)collJsonIn["radius"] * colliderScale.x * 01f;
                                colliderSphere.transform.localPosition = colliderTr;
							}
						}
					}
				}
			}
		}
	}

	//create the control paramter object
	public static void createControlParamObject(JObject jsonObjIn)
	{
		Dictionary<string, float> dictObj = jsonObjIn.ToObject<Dictionary<string, float>>();
		controlParams = new tntHumanoidControlParams();
		tntHumanoidController controllerScript = humanoidController.GetComponent<tntHumanoidController>();
		controllerScript.m_controlParams = controlParams;
		foreach (KeyValuePair<string, float> entry in dictObj)
		{
			string key = entry.Key;
			float value = entry.Value;
			//10
			if(key == "BFRAME_LEAN_FORWARD")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD] = value;
			if (key == "BFRAME_LEAN_FORWARD_TRAJ1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ1] = value;
			if (key == "BFRAME_LEAN_FORWARD_TRAJ2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ2] = value;
			if (key == "BFRAME_LEAN_FORWARD_TRAJ3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ3] = value;
			if (key == "BFRAME_LEAN_FORWARD_TRAJ4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ4] = value;
			if (key == "BFRAME_LEAN_FORWARD_TRAJ5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ5] = value;
			if (key == "BFRAME_LEAN_FORWARD_TRAJ6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_FORWARD_TRAJ6] = value;
			if (key == "BFRAME_LEAN_SIDEWAYS")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS] = value;
			if (key == "BFRAME_LEAN_SIDEWAYS_TRAJ1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ1] = value;
			if (key == "BFRAME_LEAN_SIDEWAYS_TRAJ2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ2] = value;

			//20
			if (key == "BFRAME_LEAN_SIDEWAYS_TRAJ3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ3] = value;
			if (key == "BFRAME_LEAN_SIDEWAYS_TRAJ4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ4] = value;
			if (key == "BFRAME_LEAN_SIDEWAYS_TRAJ5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ5] = value;
			if (key == "BFRAME_LEAN_SIDEWAYS_TRAJ6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BFRAME_LEAN_SIDEWAYS_TRAJ6] = value;
			if (key == "BODYFRAME_TORQUE_KD")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BODYFRAME_TORQUE_KD] = value;
			if (key == "BODYFRAME_TORQUE_KP")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_BODYFRAME_TORQUE_KP] = value;
			if (key == "DESIRED_HEIGHT")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_DESIRED_HEIGHT] = value;
			if (key == "DESIRED_HEIGHT_TRAJ1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ1] = value;
			if (key == "DESIRED_HEIGHT_TRAJ2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ2] = value;
			if (key == "DESIRED_HEIGHT_TRAJ3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ3] = value;

			//30
			if (key == "DESIRED_HEIGHT_TRAJ4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ4] = value;
			if (key == "DESIRED_HEIGHT_TRAJ5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ5] = value;
			if (key == "DESIRED_HEIGHT_TRAJ6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_DESIRED_HEIGHT_TRAJ6] = value;
			if (key == "EARLY_SWING_TERMINATE")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_EARLY_SWING_TERMINATE] = value;
			if (key == "GRF_REGULARIZER")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_GRF_REGULARIZER] = value;
			if (key == "GRF_TORQUE_TO_FORCE_OBJECTIVE_RATIO")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_GRF_TORQUE_TO_FORCE_OBJECTIVE_RATIO] = value;
			if (key == "GYRO_RATIO")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_GYRO_RATIO] = value;
			if (key == "LATE_SWING_TERMINATE")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_LATE_SWING_TERMINATE] = value;
			if (key == "LEFT_LEG_SWING_END")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_LEFT_LEG_SWING_END] = value;
			if (key == "LEFT_LEG_SWING_START")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_LEFT_LEG_SWING_START] = value;

			//40
			if (key == "LEG_PLANE_ANGLE_LEFT")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_LEG_PLANE_ANGLE_LEFT] = value;
			if (key == "LEG_PLANE_ANGLE_RIGHT")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_LEG_PLANE_ANGLE_RIGHT] = value;
			if (key == "MAX_CONTACT_POINTS_PER_FOOT")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_MAX_CONTACT_POINTS_PER_FOOT] = value;
			if (key == "RIGHT_LEG_SWING_END")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_RIGHT_LEG_SWING_END] = value;
			if (key == "RIGHT_LEG_SWING_START")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_RIGHT_LEG_SWING_START] = value;
			if (key == "ROTATION_FRICTION_KD")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_ROTATION_FRICTION_KD] = value;
			if (key == "SPINE_SLOUCH_FORWARD")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD] = value;
			if (key == "SPINE_SLOUCH_FORWARD_TRAJ1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ1] = value;
			if (key == "SPINE_SLOUCH_FORWARD_TRAJ2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ2] = value;
			if (key == "SPINE_SLOUCH_FORWARD_TRAJ3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ3] = value;

			//50
			if (key == "SPINE_SLOUCH_FORWARD_TRAJ4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ4] = value;
			if (key == "SPINE_SLOUCH_FORWARD_TRAJ5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ5] = value;
			if (key == "SPINE_SLOUCH_FORWARD_TRAJ6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_FORWARD_TRAJ6] = value;
			if (key == "SPINE_SLOUCH_SIDEWAYS")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS] = value;
			if (key == "SPINE_SLOUCH_SIDEWAYS_TRAJ1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ1] = value;
			if (key == "SPINE_SLOUCH_SIDEWAYS_TRAJ2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ2] = value;
			if (key == "SPINE_SLOUCH_SIDEWAYS_TRAJ3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ3] = value;
			if (key == "SPINE_SLOUCH_SIDEWAYS_TRAJ4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ4] = value;
			if (key == "SPINE_SLOUCH_SIDEWAYS_TRAJ5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ5] = value;
			if (key == "SPINE_SLOUCH_SIDEWAYS_TRAJ6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_SLOUCH_SIDEWAYS_TRAJ6] = value;

			//60
			if (key == "SPINE_TWIST")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_TWIST] = value;
			if (key == "SPINE_TWIST_TRAJ1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_TWIST_TRAJ1] = value;
			if (key == "SPINE_TWIST_TRAJ2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_TWIST_TRAJ2] = value;
			if (key == "SPINE_TWIST_TRAJ3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_TWIST_TRAJ3] = value;
			if (key == "SPINE_TWIST_TRAJ4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_TWIST_TRAJ4] = value;
			if (key == "SPINE_TWIST_TRAJ5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_TWIST_TRAJ5] = value;
			if (key == "SPINE_TWIST_TRAJ6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SPINE_TWIST_TRAJ6] = value;
			if (key == "STANCE_ANKLE_ROT1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_ANKLE_ROT1] = value;
			if (key == "STANCE_ANKLE_ROT2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_ANKLE_ROT2] = value;
			if (key == "STANCE_ANKLE_ROT3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_ANKLE_ROT3] = value;

			//70
			if (key == "STANCE_ANKLE_ROT4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_ANKLE_ROT4] = value;
			if (key == "STANCE_ANKLE_ROT5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_ANKLE_ROT5] = value;
			if (key == "STANCE_ANKLE_ROT6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_ANKLE_ROT6] = value;
			if (key == "STANCE_TOE_ROT1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_TOE_ROT1] = value;
			if (key == "STANCE_TOE_ROT2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_TOE_ROT2] = value;
			if (key == "STANCE_TOE_ROT3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_TOE_ROT3] = value;
			if (key == "STANCE_TOE_ROT4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_TOE_ROT4] = value;
			if (key == "STANCE_TOE_ROT5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_TOE_ROT5] = value;
			if (key == "STANCE_TOE_ROT6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STANCE_TOE_ROT6] = value;
			if (key == "STAND_STILL_THRESHOLD")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STAND_STILL_THRESHOLD] = value;

			//80
			if (key == "STEP_FORWARD_OFFSET")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_FORWARD_OFFSET] = value;
			if (key == "STEP_TARGET_INTERPOLATION_FUNCTION1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION1] = value;
			if (key == "STEP_TARGET_INTERPOLATION_FUNCTION2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION2] = value;
			if (key == "STEP_TARGET_INTERPOLATION_FUNCTION3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION3] = value;
			if (key == "STEP_TARGET_INTERPOLATION_FUNCTION4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION4] = value;
			if (key == "STEP_TARGET_INTERPOLATION_FUNCTION5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION5] = value;
			if (key == "STEP_TARGET_INTERPOLATION_FUNCTION6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_TARGET_INTERPOLATION_FUNCTION6] = value;
			if (key == "STEP_WIDTH")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STEP_WIDTH] = value;
			if (key == "STRIDE_DURATION")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_STRIDE_DURATION] = value;
			if (key == "SWING_ANKLE_ROT1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_ANKLE_ROT1] = value;

			//90
			if (key == "SWING_ANKLE_ROT2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_ANKLE_ROT2] = value;
			if (key == "SWING_ANKLE_ROT3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_ANKLE_ROT3] = value;
			if (key == "SWING_ANKLE_ROT4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_ANKLE_ROT4] = value;
			if (key == "SWING_ANKLE_ROT5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_ANKLE_ROT5] = value;
			if (key == "SWING_ANKLE_ROT6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_ANKLE_ROT6] = value;
			if (key == "SWING_FOOT_HEIGHT_TRAJ1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ1] = value;
			if (key == "SWING_FOOT_HEIGHT_TRAJ2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ2] = value;
			if (key == "SWING_FOOT_HEIGHT_TRAJ3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ3] = value;
			if (key == "SWING_FOOT_HEIGHT_TRAJ4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ4] = value;
			if (key == "SWING_FOOT_HEIGHT_TRAJ5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ5] = value;

			//100
			if (key == "SWING_FOOT_HEIGHT_TRAJ6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_HEIGHT_TRAJ6] = value;
			if (key == "SWING_FOOT_MAX_FORCE")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_MAX_FORCE] = value;
			if (key == "SWING_FOOT_VFORCE_KD")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_VFORCE_KD] = value;
			if (key == "SWING_FOOT_VFORCE_KP")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_FOOT_VFORCE_KP] = value;
			if (key == "SWING_TOE_ROT1")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_TOE_ROT1] = value;
			if (key == "SWING_TOE_ROT2")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_TOE_ROT2] = value;
			if (key == "SWING_TOE_ROT3")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_TOE_ROT3] = value;
			if (key == "SWING_TOE_ROT4")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_TOE_ROT4] = value;
			if (key == "SWING_TOE_ROT5")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_TOE_ROT5] = value;
			if (key == "SWING_TOE_ROT6")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_SWING_TOE_ROT6] = value;

			//107
			if (key == "VFORCE_FF_VERTICAL_WEIGHT_PERCENTAGE")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_VFORCE_FF_VERTICAL_WEIGHT_PERCENTAGE] = value;
			if (key == "VFORCE_KD_CORONAL")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_VFORCE_KD_CORONAL] = value;
			if (key == "VFORCE_KD_SAGITTAL")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_VFORCE_KD_SAGITTAL] = value;
			if (key == "VFORCE_KD_VERTICAL")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_VFORCE_KD_VERTICAL] = value;
			if (key == "VFORCE_KP_CORONAL")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_VFORCE_KP_CORONAL] = value;
			if (key == "VFORCE_KP_SAGITTAL")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_VFORCE_KP_SAGITTAL] = value;
			if (key == "VFORCE_KP_VERTICAL")
				controllerScript.m_controlParams.m_params[tntHumanoidControlParams.P_VFORCE_KP_VERTICAL] = value;
		}
	}

	//update the humanoid controller values
	public static void updateHumanoidController(JObject jsonObjIn)
	{
		Dictionary<string, object> dictObj = jsonObjIn.ToObject<Dictionary<string, object>>();

		foreach (KeyValuePair<string, object> entry in dictObj)
		{
			string key = entry.Key;
			object value = entry.Value;
			tntHumanoidController controllerScript = humanoidController.GetComponent<tntHumanoidController>();
			if (key == "controllerState")
			{
				JObject conState = JObject.Parse(Convert.ToString(value));
				if(controllerScript.controllerState != null)
				{
					tntHumanoidControllerState controllerS = new tntHumanoidControllerState();
					controllerS.grounded = (bool)conState["grounded"];
					controllerS.leftStancePhase = (float)conState["leftStancePhase"];
					controllerS.leftSwingPhase = (float)conState["leftSwingPhase"];
					controllerS.rightStancePhase = (float)conState["rightStancePhase"];
					controllerS.rightSwingPhase = (float)conState["rightSwingPhase"];
					controllerS.standStill = (bool)conState["standStill"];
					controllerS.stridePhase = (float)conState["stridePhase"];
					controllerScript.controllerState = controllerS;
				}
			}
			if (key == "disableGRFSolver")
			{
				controllerScript.m_disableGRFSolver = Convert.ToBoolean(value);
			}
			if (key == "useBlendspace")
			{
				controllerScript.m_useBlendSpace = Convert.ToBoolean(value);
			}
			if (key == "forward")
			{
				JArray forwardArray = JArray.Parse(Convert.ToString(value));
				controllerScript.m_forward.x = (float)forwardArray[0];
				controllerScript.m_forward.y = (float)forwardArray[1];
				controllerScript.m_forward.z = (float)forwardArray[2];
			}
			if (key == "keepRootPosition")
			{
				controllerScript.m_keepRootPosition = Convert.ToBoolean(value);
			}
			if (key == "protectControlParamsScriptObject")
			{
				controllerScript.m_protectControlParamsScriptObject = Convert.ToBoolean(value);
			}
			if (key == "protectDesiredPoseScriptObject")
			{
				controllerScript.m_protectDesiredPoseScriptObject = Convert.ToBoolean(value);
			}
			if (key == "protectPDParamSetScriptObject")
			{
				controllerScript.m_protectPDParamSetScriptObject = Convert.ToBoolean(value);
			}
			if (key == "reflectControllerState")
			{
				controllerScript.m_reflectControllerState = Convert.ToBoolean(value);
			}
			if (key == "reflectCurrentPose")
			{
				controllerScript.m_reflectCurrentPose = Convert.ToBoolean(value);
			}
			if (key == "reflectDesiredPose")
			{
				controllerScript.m_reflectDesiredPose = Convert.ToBoolean(value);
			}
			if (key == "right")
			{
				JArray rightArray = JArray.Parse(Convert.ToString(value));
				controllerScript.m_right.x = -(float)rightArray[0];
				controllerScript.m_right.y = (float)rightArray[1];
				controllerScript.m_right.z = (float)rightArray[2];
			}
			if (key == "rootPdParams")
			{
				JObject rootPdParams = JObject.Parse(Convert.ToString(value));
				PDParams rootPD = new PDParams();
				rootPD.controlled = (bool)rootPdParams["controlled"];
				rootPD.kd = (float)rootPdParams["kd"];
				rootPD.kd = (float)rootPdParams["kp"];
				rootPD.maxAbsTorque = (float)rootPdParams["maxAbsTorque"];
				rootPD.name = (string)rootPdParams["name"];
				rootPD.relToCharFrame = (bool)rootPdParams["relToCharFrame"];
				JArray scaleArray = JArray.Parse(Convert.ToString(rootPdParams["scale"]));
				rootPD.scale.x = (float)scaleArray[0];
				rootPD.scale.y = (float)scaleArray[1];
				rootPD.scale.z = (float)scaleArray[2];
				rootPD.strength = (float)rootPdParams["strength"];
				controllerScript.m_rootPdParams = rootPD;
			}
			if (key == "reflectDesiredPose")
			{
				controllerScript.m_useBlendSpace = Convert.ToBoolean(value);
			}
			if (key == "currentPose")
			{
			}
			if (key == "desiredPose")
			{
			}
			if (key == "limbs")
			{
				JObject conState = JObject.Parse(Convert.ToString(value));
				LimbConfiguration limbController = new LimbConfiguration();

				limbController.m_stepRelativeToCOM = (bool)conState["stepRelativeToCOM"];
				limbController.m_stepRelativeToRoot = (bool)conState["stepRelativeToRoot"];
				limbController.m_limbTrackingKd = (float)conState["limbTrackingKd"];
				limbController.m_limbTrackingKp = (float)conState["limbTrackingKp"];
				limbController.m_antiLegCrossing = (bool)conState["antiLegCrossing"];
				limbController.m_limbMaxTrackingForce = (float)conState["limbMaxTrackingForce"];
				limbController.m_deadThresholdGRF = (float)conState["deadThresholdGRF"];
				limbController.m_deadThresholdSwingLeg = (float)conState["deadThresholdSwingLeg"];
				limbController.m_kickIfDead = (bool)conState["kickIfDead"];

				foreach (Transform childObj in BlueRobot.transform)
				{
					if (childObj.gameObject.name == "rWrist")
					{
						limbController.m_lHand = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "rShoulder")
					{
						limbController.m_lShoulder = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "rToe")
					{
						limbController.m_lToes = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "pelvis_lowerback")
				{
						limbController.m_lowerBack = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "torso_head")
					{
						limbController.m_neck = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "lWrist")
					{
						limbController.m_rHand = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "lShoulder")
					{
						limbController.m_rShoulder = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "lToe")
					{
						limbController.m_rToes = childObj.gameObject.GetComponent<tntLink>();
					}
					if (childObj.gameObject.name == "lowerback_torso")
					{
						limbController.m_upperBack = childObj.gameObject.GetComponent<tntLink>();
					}
				}
				controllerScript.m_limbs = limbController;
			}

		}
	}
}
