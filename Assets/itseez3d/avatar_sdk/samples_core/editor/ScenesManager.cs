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
using ItSeez3D.AvatarSdk.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItSeez3D.AvatarSdkSamples.Core.Editor
{
	[InitializeOnLoad]
	public class ScenesManager
	{
		private static List<string> scenesWithViewer = new List<string> () {
			"02_gallery_sample_cloud.unity",
			"06_webgl_sample.unity",
			"05_resources_sample_cloud.unity",
			"02_gallery_sample_offline.unity",
			"05_resources_sample_offline.unity"
		};

		static ScenesManager ()
		{
			EditorSceneManager.sceneOpened += (s, m) => {
				EnableOpenedScenesInBuildSettings ();
			};

			EditorSceneManager.sceneClosed += s => {
				EnableOpenedScenesInBuildSettings ();
			};
		}

		private static void EnableOpenedScenesInBuildSettings ()
		{
			List<string> openedScenes = new List<string> ();
			for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
				var s = EditorSceneManager.GetSceneAt (i);
				if (s.isLoaded)
					openedScenes.Add (s.path);
			}
			EnableScenesInBuildSettings (openedScenes);
		}

		private static void EnableScenesInBuildSettings (List<string> openedScenes)
		{
			List<EditorBuildSettingsScene> scenesInBuildSettings = EditorBuildSettings.scenes.ToList();

			// if we opened one of the samples that depends on a Viewer scene, let's add the viewer scene to build settings.
			foreach (string scene in openedScenes)
			{
				bool isViewerSceneRequired = false;
				foreach(string sceneWithViewer in scenesWithViewer)
				{
					if (scene.Contains(sceneWithViewer))
					{
						isViewerSceneRequired = true;
						break;
					}
				}

				if (isViewerSceneRequired)
				{
					string viewerScenePath = PluginStructure.GetPluginDirectoryPath(PluginStructure.VIEWER_SCENE_PATH, PathOriginOptions.RelativeToAssetsFolder);
					if (!ContainsScene(scenesInBuildSettings, scene))
						scenesInBuildSettings.Add(new EditorBuildSettingsScene(scene, true));
					if (!ContainsScene(scenesInBuildSettings, viewerScenePath))
						scenesInBuildSettings.Add(new EditorBuildSettingsScene(viewerScenePath, true));
				}
			}
			EditorBuildSettings.scenes = scenesInBuildSettings.ToArray();
		}

		private static bool ContainsScene(List<EditorBuildSettingsScene> scenesList, string scenePath)
		{
			EditorBuildSettingsScene existedScene = scenesList.FirstOrDefault (s => string.Compare (Path.GetFullPath(s.path), Path.GetFullPath(scenePath)) == 0);
			return existedScene != null;
		}
	}
}
#endif
