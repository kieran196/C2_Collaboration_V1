/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using ItSeez3D.AvatarSdk.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// This sample demonstrates how to use the level-of-details functionality.
	/// </summary>
	public class LODSample : GettingStartedSample
	{
		#region UI
		public Text detailsLevelText = null;
		public Text currentBlendshapeText = null;
		public GameObject[] avatarControls = null;
		public ItemsSelectingView blendshapesSelectingView = null;

		public ToggleGroup lodToggleGroup = null;
		public Toggle lodTogglePrefab = null;
		public GameObject lodTogglesPanel = null;
		#endregion

		private int currentDetailsLevel = 0;
		private int currentBlendshape = 0;

		// Blendshapes names with their index in avatar mesh
		private Dictionary<int, string> availableBlendshapes = new Dictionary<int, string>();

		protected List<Toggle> toggles = new List<Toggle>();

		#region GettingStartedSample overrided methods
		protected override IEnumerator GenerateAvatarFunc(byte[] photoBytes)
		{
			EnableAvatarControls(false);
			InitLodToggles(pipelineType == PipelineType.FACE ? 9 : 8);
			yield return base.GenerateAvatarFunc(photoBytes);
			EnableAvatarControls(true);
		}

		protected override IEnumerator GenerateAndDisplayHead(byte[] photoBytes, PipelineType pipeline)
		{
			// Generate avatar with all available blendshapes and without haircuts
			var resourcesRequest = avatarProvider.ResourceManager.GetResourcesAsync(AvatarResourcesSubset.ALL, pipeline);
			yield return resourcesRequest;
			if (resourcesRequest.IsError)
				yield break;

			AvatarResources resources = resourcesRequest.Result;
			resources.haircuts.Clear();

			var initializeRequest = avatarProvider.InitializeAvatarAsync(photoBytes, "name", "description", pipeline, resources);
			yield return Await(initializeRequest);
			currentAvatarCode = initializeRequest.Result;

			StartCoroutine(SampleUtils.DisplayPhotoPreview(currentAvatarCode, photoPreview));

			var calculateRequest = avatarProvider.StartAndAwaitAvatarCalculationAsync(currentAvatarCode);
			yield return Await(calculateRequest);

			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(currentAvatarCode, true, currentDetailsLevel);
			yield return Await(avatarHeadRequest);

			DisplayHead(avatarHeadRequest.Result, null);

			//Retrieve blendshape names from the mesh and add an empty blendshape with index -1
			Mesh mesh = avatarHeadRequest.Result.mesh;
			availableBlendshapes.Clear();
			availableBlendshapes.Add(-1, "None");
			for (int i = 0; i < mesh.blendShapeCount; i++)
				availableBlendshapes.Add(i, mesh.GetBlendShapeName(i));
			ChangeCurrentBlendshape(-1);
			blendshapesSelectingView.InitItems(availableBlendshapes.Values.ToList());

			detailsLevelText.text = string.Format("Triangles count:\n{0}", avatarHeadRequest.Result.mesh.triangles.Length / 3);
		}

		protected override void SetControlsInteractable(bool interactable)
		{
			base.SetControlsInteractable(interactable);
			if (toggles != null)
			{
				foreach (Toggle t in toggles)
					t.interactable = interactable;
			}
		}

		#endregion

		#region UI handling
		public void PrevBlendshapeClick()
		{
			ChangeCurrentBlendshape(currentBlendshape - 1);
		}

		public void NextBlendshapeClick()
		{
			ChangeCurrentBlendshape(currentBlendshape + 1);
		}

		public void OnBlendshapeListButtonClick()
		{
			SetControlsInteractable(false);
			blendshapesSelectingView.Show(new List<string>() { availableBlendshapes[currentBlendshape] }, list =>
			{
				SetControlsInteractable(true);
				// Find KeyValuePair for selected blendshape name. Assume that returned list contains only one element.
				var pair = availableBlendshapes.FirstOrDefault(p => p.Value == list[0]);
				ChangeCurrentBlendshape(pair.Key);
			});
		}

		private void EnableAvatarControls(bool isEnabled)
		{
			if (avatarControls == null)
				return;

			for (int i=0; i<avatarControls.Length; i++)
				avatarControls[i].SetActive(isEnabled);
		}
		#endregion

		#region LOD methods
		private IEnumerator ChangeMeshDetailsLevel(int newDetailsLevel)
		{
			int numberOfLevels = pipelineType == PipelineType.FACE ? 9 : 8;
			if (newDetailsLevel < 0 || newDetailsLevel > numberOfLevels)
				yield break;

			currentDetailsLevel = newDetailsLevel;
			SetControlsInteractable(false);
			yield return ChangeMeshResolution(currentAvatarCode, currentDetailsLevel);
			SetControlsInteractable(true);
		}

		private IEnumerator ChangeMeshResolution(string avatarCode, int detailsLevel)
		{
			var headObject = GameObject.Find(HEAD_OBJECT_NAME);
			if (headObject == null)
				yield break;

			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(avatarCode, true, detailsLevel);
			yield return Await(avatarHeadRequest);

			SkinnedMeshRenderer meshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
			meshRenderer.sharedMesh = avatarHeadRequest.Result.mesh;
			detailsLevelText.text = string.Format("Triangles count:\n{0}", meshRenderer.sharedMesh.triangles.Length / 3);
		}

		private void InitLodToggles(int countDetailsLevels)
		{
			currentDetailsLevel = 0;

			foreach (Toggle t in toggles)
				Destroy(t.gameObject);
			toggles.Clear();

			for (int i=0; i<countDetailsLevels; i++)
			{
				Toggle toggle = Instantiate<Toggle>(lodTogglePrefab);
				toggle.isOn = i == 0;
				toggle.gameObject.transform.SetParent(lodTogglesPanel.transform);
				toggle.group = lodToggleGroup;
				ToggleId toggleId = toggle.gameObject.GetComponentInChildren<ToggleId>();
				toggleId.Text = string.Format("LOD{0}", i);
				toggleId.Id = i;
				toggle.onValueChanged.AddListener((isChecked) => 
				{
					if (isChecked)
						StartCoroutine(ChangeMeshDetailsLevel(toggleId.Id));
				});
				toggles.Add(toggle);
			}
		}
		#endregion

		#region blendshapes method

		private void ChangeCurrentBlendshape(int blendshapeIdx)
		{
			if (!availableBlendshapes.ContainsKey(blendshapeIdx))
				return;

			currentBlendshape = blendshapeIdx;

			var headObject = GameObject.Find(HEAD_OBJECT_NAME);
			var meshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
			foreach (int idx in availableBlendshapes.Keys)
			{
				if (idx >= 0)
					meshRenderer.SetBlendShapeWeight(idx, idx == currentBlendshape ? 100.0f : 0.0f);
			}

			currentBlendshapeText.text = availableBlendshapes[currentBlendshape];
		}

		#endregion
	}
}
