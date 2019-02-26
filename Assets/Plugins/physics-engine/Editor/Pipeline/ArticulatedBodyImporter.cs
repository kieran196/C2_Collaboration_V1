using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
	public class ArticulatedBodyImporter : AssetPostprocessor
	{
        public static bool flipLeftRightMeshes = false;
		public static bool flipFrontBackMeshes = false;
        public static bool useJointLimitsAndMotor = true;
		public static bool pivotAtJoint = false;		// by default pivots/COMs are at the centers of colliders
        public static float dimensionScale = 1.0f;

        public static List<ArticulatedRigidBody> ABs;
        public static List<ArticulatedFigure> AFs;
        public static Dictionary<string, ArticulatedRigidBody> NameToAB;

        //public static float fHandnessFlip = 1.0f;

		public ArticulatedBodyImporter()
		{
			ABs = new List<ArticulatedRigidBody>();
			AFs = new List<ArticulatedFigure>();
			NameToAB = new Dictionary<string, ArticulatedRigidBody>();
		}

        public static float HandnessFlip() { return 1.0f; /*fHandnessFlip;*/ } // deprecated
        public static float DimensionScale() { return dimensionScale; }
        public static float MOIScale() { return dimensionScale * dimensionScale; }
        public static ArticulatedRigidBody getARBByName(string name) { return NameToAB[name]; }
        public static string PhysicsResourceRoot() { return "Assets/shared-assets-lfs/ApeModels"; }
		/// <summary>
		/// Handles when ANY asset is imported, deleted, or moved.  Each parameter is the full path of the asset, including filename and extension.
		/// </summary>
		/// <param name="importedAssets">The array of assets that were imported.</param>
		/// <param name="deletedAssets">The array of assets that were deleted.</param>
		/// <param name="movedAssets">The array of assets that were moved.  These are the new file paths.</param>
		/// <param name="movedFromPath">The array of assets that were moved.  These are the old file paths.</param>
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
		{
			foreach(string asset in importedAssets)
			{
				//Debug.Log("Imported: " + asset);
				var ext = (asset.Substring(asset.LastIndexOf(".") + 1));
				if(ext.ToLower()=="rbs")
				{
					ImportRBS(asset);
				}
			}
		}

		private static GameObject CreateRigidBody(GameObject rootObject,
		                                          string name, List<string> meshNames,
		                                 		  Vector3 position,
		                                 		  float mass, Vector3 moi)
		{
			GameObject rigidbody = new GameObject(name);

			tntBase tntBody = rigidbody.AddComponent<tntBase>();
			tntBody.m_mass = mass;
			tntBody.m_moi = moi;
			for (int i = 0; i < meshNames.Count; ++i)
			{
                string path = ArticulatedBodyImporter.PhysicsResourceRoot() + "/" + meshNames[i] + ".obj";
                Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(Object));

				GameObject mesh  = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
				mesh.transform.parent = rigidbody.transform;
			}
			rigidbody.transform.parent = rootObject.transform;

			rigidbody.transform.position = position;

			return rigidbody;
		}

		private static void CreateSphere(GameObject rootObject, string name, List<string> meshNames,
		                                 Vector3 center, float radius,
		                                 float mass, Vector3 moi)
		{
			GameObject sphere = CreateRigidBody(rootObject, name, meshNames, center, mass, moi);

			SphereCollider sphereCollider = sphere.AddComponent<SphereCollider>();
			sphereCollider.center = center;
			sphereCollider.radius = radius;
		}

		private static void CreateCapsule(GameObject rootObject, string name, List<string> meshNames,
		                                  Vector3 p1, Vector3 p2, float radius,
		                                  float mass, Vector3 moi)
		{
			Vector3 center = (p1 + p2) / 2;
			GameObject body = CreateRigidBody(rootObject, name, meshNames, center, mass, moi);
			CapsuleCollider capsule = body.AddComponent<CapsuleCollider>();
			capsule.center = center;
			capsule.height = (p2 - p1).magnitude;
			capsule.radius = radius;
		}

		private static void ImportRBS(string fileName)
		{
			Debug.Log(">>>>>>>>>>>>> Importing RBS File: " + fileName);
			string[] stringValues = fileName.Split('/');
			string shortName = stringValues[stringValues.Length - 1];
			shortName = shortName.Substring(0, shortName.Length - 4);
			GameObject rootObject = new GameObject(shortName);

			var sr = new StreamReader(fileName);
			ArticulatedRigidBody arb = null;
			ArticulatedFigure af = null;
			
            //fHandnessFlip = 1.0f;
            flipFrontBackMeshes = false;
			flipLeftRightMeshes = false;

			for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
			{
				string[] sValues = line.Split(' ');
				RB_TYPE rbType = RBTagParser.ParseTag(line);
				// Debug.Log("line: " + line + " =>" + rbType);
				switch (rbType)
				{
					case RB_TYPE.RB_RB:
						break;
					case RB_TYPE.RB_ARB:
						arb = new ArticulatedRigidBody();
						string name = arb.Deserialize(sr);
						ABs.Add(arb);
                        if (NameToAB.ContainsKey(name))
                        {
                            Debug.LogError("Duplicated Articulated Rigid Body name: " + name);
                        } else
                        {
						    NameToAB.Add(name, arb);
                        }
						break;
					case RB_TYPE.RB_ARTICULATED_FIGURE:
						af = new ArticulatedFigure();
						af.Deserialize(sr, rootObject);
						AFs.Add(af);
						break;
                    // make the handedness a changeable static attribute of this class
                    case RB_TYPE.RB_HANDEDNESSFLIP:
                        //fHandnessFlip = float.Parse(sValues[1]);
                        break;
					case RB_TYPE.RB_LEFTRIGHTMESHFLIP:
						flipLeftRightMeshes = bool.Parse(sValues[1]);
						break;
					case RB_TYPE.RB_FRONTBACKMESHFLIP:
						flipFrontBackMeshes = bool.Parse(sValues[1]);
						break;
					case RB_TYPE.RB_PIVOTATJOINT:
						pivotAtJoint = bool.Parse(sValues[1]);
						break;
					default:
						break;
				}
			}
			sr.Close();
		}
	}
}