/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItSeez3D.AvatarSdkSamples.Core;
using UnityEditor;
using ItSeez3D.AvatarSdk.Core;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public static class AvatarPrefabBuilder
	{
		public static void CreateAvatarPrefab (GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId, string haircutId)
		{
			string prefabDir = Path.Combine(PluginStructure.GetPluginDirectoryPath(PluginStructure.PREFABS_DIR, PathOriginOptions.RelativeToAssetsFolder), avatarId);
			PluginStructure.CreatePluginDirectory(prefabDir);
			avatarObject = GameObject.Instantiate(avatarObject);
			SaveMeshAndMaterialForAvatarObject(prefabDir, avatarObject, headObjectName, haircutObjectName, avatarId, haircutId);

			string prefabPath = prefabDir + "/avatar.prefab";
			GameObject.DestroyImmediate(avatarObject.GetComponentInChildren<RotateByMouse>());
			PrefabUtility.CreatePrefab(prefabPath, avatarObject);
			GameObject.Destroy(avatarObject);
			EditorUtility.DisplayDialog ("Prefab created successfully!", string.Format ("You can find your prefab in '{0}' folder", prefabDir), "Ok");
		}

		public static void CreateFullbodyPrefab(GameObject bodyObject, GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId, string haircutId)
		{
			string prefabDir = Path.Combine(PluginStructure.GetPluginDirectoryPath(PluginStructure.PREFABS_DIR, PathOriginOptions.RelativeToAssetsFolder), avatarId);
			PluginStructure.CreatePluginDirectory(prefabDir);

			//Create a copy of the avatarObject
			GameObject copiedAvatarObject = GameObject.Instantiate(avatarObject);
			copiedAvatarObject.transform.SetParent(bodyObject.transform);
			copiedAvatarObject.name = avatarObject.name;

			//Further actions will change the mesh of the avatarObject. It won't contain bones and weights.
			//So we have preserve original avatarObject and recover it as is was before the creating of prefab.
			avatarObject.transform.SetParent(null);
			SaveMeshAndMaterialForAvatarObject(prefabDir, copiedAvatarObject, headObjectName, haircutObjectName, avatarId, haircutId);

			BodyAttachment bodyAttachment = bodyObject.GetComponentInChildren<BodyAttachment>();
			Matrix4x4[] currentBindPoses = bodyAttachment.GetCurrentBindPosesForHeadAndNeck();
			Matrix4x4[] originalBindPoses = { bodyAttachment.headBindPose, bodyAttachment.neckBindPose };
			bodyAttachment.headBindPose = currentBindPoses[0];
			bodyAttachment.neckBindPose = currentBindPoses[1];

			// Remove RotateByMouse script
			GameObject.DestroyImmediate(bodyObject.GetComponentInChildren<RotateByMouse>());
			// Save prefab
			PrefabUtility.CreatePrefab(prefabDir + "/fullbody.prefab", bodyObject);
			//Revert back RotateBuMouse script
			bodyObject.AddComponent<RotateByMouse>();

			//Remove the copy of the avatarObject and recover the original
			GameObject.DestroyImmediate(copiedAvatarObject);
			avatarObject.transform.SetParent(bodyObject.transform);

			bodyAttachment.headBindPose = originalBindPoses[0];
			bodyAttachment.neckBindPose = originalBindPoses[1];

			EditorUtility.DisplayDialog("Prefab created successfully!", string.Format("You can find your prefab in '{0}' folder", prefabDir), "Ok");
		}

		private static void SaveMeshAndMaterialForAvatarObject(string prefabDir, GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId, string haircutId)
		{
			GameObject headObject = GetChildByName(avatarObject, headObjectName);
			GameObject hairObject = GetChildByName(avatarObject, haircutObjectName);

			if (headObject != null)
			{
				SkinnedMeshRenderer headMeshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
				headMeshRenderer.material.mainTexture = SaveTextureAsset(headMeshRenderer.material.mainTexture, Path.Combine(prefabDir, "head_texture.jpg"));
				headMeshRenderer.material = InstantiateAndSaveMaterial(headMeshRenderer.material, Path.Combine(prefabDir, "head_material.mat"));
				headMeshRenderer.sharedMesh = SaveAvatarMeshAsFbxAsset(avatarId, Path.Combine(prefabDir, "head_mesh.fbx"));

				for (int i = 0; i < headMeshRenderer.sharedMesh.blendShapeCount; i++)
					headMeshRenderer.SetBlendShapeWeight(i, 0.0f);
			}

			if (hairObject != null)
			{
				MeshRenderer hairMeshRenderer = hairObject.GetComponentInChildren<MeshRenderer>();
				if (hairMeshRenderer != null)
				{
					hairMeshRenderer.material.mainTexture = SaveTextureAsset(hairMeshRenderer.material.mainTexture, Path.Combine(prefabDir, "haircut_texture.png"));
					hairMeshRenderer.material = InstantiateAndSaveMaterial(hairMeshRenderer.material, Path.Combine(prefabDir, "haircut_material.mat"));
					hairObject.GetComponentInChildren<MeshFilter>().mesh = SaveHaircutMeshAsFbxAsset(avatarId, haircutId, Path.Combine(prefabDir, "haircut_mesh.fbx"));
				}
				else
				{
					SkinnedMeshRenderer hairSkinnedMeshRenderer = hairObject.GetComponentInChildren<SkinnedMeshRenderer>();
					if (hairSkinnedMeshRenderer != null)
					{
						hairSkinnedMeshRenderer.material.mainTexture = SaveTextureAsset(hairSkinnedMeshRenderer.material.mainTexture, Path.Combine(prefabDir, "haircut_texture.png"));
						hairSkinnedMeshRenderer.material = InstantiateAndSaveMaterial(hairSkinnedMeshRenderer.material, Path.Combine(prefabDir, "haircut_material.mat"));
						hairSkinnedMeshRenderer.sharedMesh = SaveHaircutMeshAsFbxAsset(avatarId, haircutId, Path.Combine(prefabDir, "haircut_mesh.fbx"));
					}
				}
			}

			AssetDatabase.SaveAssets();
		}

		private static Material InstantiateAndSaveMaterial(Material material, string assetPath)
		{
			Material instanceMat = GameObject.Instantiate(material);
			AssetDatabase.CreateAsset(instanceMat, assetPath);
			return instanceMat;
		}

		private static Texture2D SaveTextureAsset(Texture texture, string texturePath)
		{
			Texture2D texture2D = texture as Texture2D;
			byte[] textureBytes = null;
			if (texturePath.ToLower().EndsWith(".png"))
			{
				textureBytes = texture2D.EncodeToPNG();
			}
			else if (texturePath.ToLower().EndsWith(".jpg"))
			{
				textureBytes = texture2D.EncodeToJPG();
			}
			else
			{
				Debug.LogErrorFormat("Unsupported texture format: {0}", texturePath);
				return texture2D;
			}
			File.WriteAllBytes(texturePath, textureBytes);
			AssetDatabase.Refresh();
			return (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
		}

		private static Mesh SaveAvatarMeshAsFbxAsset(string avatarId, string fbxPath)
		{
			CoreTools.ExportAvatarAsFbx(avatarId, fbxPath, false);
			AssetDatabase.Refresh();
			Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(fbxPath, typeof(Mesh));
			return mesh;
		}

		private static Mesh SaveHaircutMeshAsFbxAsset(string avatarId, string haircutId, string fbxPath)
		{
			CoreTools.HaircutPlyToFbx(avatarId, haircutId, fbxPath);
			AssetDatabase.Refresh();
			Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(fbxPath, typeof(Mesh));
			return mesh;
		}

		private static GameObject GetChildByName (GameObject obj, string name)
		{
			var children = obj.GetComponentsInChildren<Transform> ();
			foreach (var child in children) {
				if (child.name.ToLower () == name.ToLower ())
					return child.gameObject;
			}

			return null;
		}
	}
}
#endif
