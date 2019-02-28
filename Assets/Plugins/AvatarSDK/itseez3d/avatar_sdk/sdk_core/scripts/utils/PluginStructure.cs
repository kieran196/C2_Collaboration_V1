/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdk.Core
{
	public enum PathOriginOptions
	{
		FullPath,
		RelativeToAssetsFolder
	}

	/// <summary>
	/// This class contains methods to get path to the plugin folders
	/// All plugin files must be in the "itseez3d/avatar_sdk" directory. But the "itseez3d/avatar_sdk" can be located in any place of your project.
	/// </summary>
	public static class PluginStructure
	{
		#region Avatar SDK plugin folders
		public static readonly string MISC_AUTH_RESOURCES_DIR = "itseez3d_misc//auth//resources";
		public static readonly string MISC_OFFLINE_RESOURCES_DIR = "itseez3d_misc//sdk_offline//resources//avatar_sdk";
		public static readonly string OFFLINE_RESOURCES_DIR = "itseez3d//avatar_sdk//sdk_offline//resources//avatar_sdk";
		public static readonly string PREFABS_DIR = "itseez3d_prefabs";
        //public static readonly string VIEWER_SCENE_PATH = "Plugins/AvatarSDK/itseez3d/avatar_sdk/samples_core/scenes/avatar_viewer.unity";
        public static readonly string VIEWER_SCENE_PATH = "itseez3d/avatar_sdk/samples_core/scenes/avatar_viewer.unity";
        private static readonly string itseez3dDir = "itseez3d";
		private static readonly string avatarSdkDir = "avatar_sdk";
		private static readonly string assetsDir = "Assets";
		#endregion

		public static string GetPluginDirectoryPath(string dir, PathOriginOptions originOption)
		{
#if UNITY_EDITOR || UNITY_EDITOR_LINUX
			string pluginLocationPath = FindPluginLocation(Application.dataPath);
			if (string.IsNullOrEmpty(pluginLocationPath))
			{
				Debug.LogError("Avatar SDK plugin location can't be found!");
				return null;
			}

			if (originOption == PathOriginOptions.RelativeToAssetsFolder)
				pluginLocationPath = pluginLocationPath.Substring(pluginLocationPath.IndexOf(assetsDir));

			return Path.Combine(pluginLocationPath, dir);
#else
			return dir;
#endif
		}

		/// <summary>
		/// Creates the directory inside "Assets" folder if necessary.
		/// </summary>
		public static void CreatePluginDirectory(string dir)
		{
#if UNITY_EDITOR || UNITY_EDITOR_LINUX
			if (!dir.Contains(assetsDir))
			{
				Debug.LogErrorFormat("Invalid directory: {0}", dir);
			}

			dir = dir.Substring(dir.IndexOf(assetsDir));
			string[] folders = dir.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
			List<string> existingPath = new List<string> { "Assets" };
			for (int i = 1; i < folders.Length; ++i)
			{
				var prevPathStr = string.Join("/", existingPath.ToArray());
				existingPath.Add(folders[i]);
				var existingPathStr = string.Join("/", existingPath.ToArray());
				if (!AssetDatabase.IsValidFolder(existingPathStr))
					AssetDatabase.CreateFolder(prevPathStr, folders[i]);
				AssetDatabase.SaveAssets();
			}
			AssetDatabase.Refresh();
#endif
		}

		public static string GetViewerSceneName()
		{
			string viewerScenePath = GetPluginDirectoryPath(VIEWER_SCENE_PATH, PathOriginOptions.RelativeToAssetsFolder).Replace('\\', '/');
			viewerScenePath = viewerScenePath.Remove(viewerScenePath.IndexOf(".unity"));
#if UNITY_EDITOR || UNITY_EDITOR_LINUX
			viewerScenePath = viewerScenePath.Substring("Assets/".Length);
#endif
			return viewerScenePath;
		}

		/// <summary>
		/// Find location of the avatar plugin in the project structure
		/// </summary>
		private static string FindPluginLocation(string rootDir)
		{
			foreach (string dirPath in Directory.GetDirectories(rootDir))
			{
				string dir = Path.GetFileName(dirPath);
				if (dir == itseez3dDir)
				{
					foreach (string itseez3dSubdirPath in Directory.GetDirectories(dirPath))
					{
						if (itseez3dSubdirPath.EndsWith(avatarSdkDir))
							return rootDir;
					}
				}
				else
				{
					string location = FindPluginLocation(dirPath);
					if (!string.IsNullOrEmpty(location))
						return location;
				}
			}
			return string.Empty;
		}
	}
}
