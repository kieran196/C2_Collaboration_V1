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
using System.Collections.Generic;
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// Interface to notify HaircutRecoloring about changes in the displayed haircut
	/// </summary>
	public interface IHaircutViewer
	{
		event Action<string> displayedHaircutChanged;

		event Action<bool> shaderTypeChanged;

		GameObject HaircutObject { get; }
	}

	/// <summary>
	/// Interface to select the haircut color
	/// </summary>
	public interface IColorPicker
	{
		Color Color { get; set; }

		void SetOnValueChangeCallback(Action<Color> onValueChange);
	}

	public class HaircutRecoloring : MonoBehaviour
	{
		#region UI

		public GameObject colorPickerPanel;

		public GameObject colorPickerGameObject;

		#endregion

		public IColorPicker colorPicker;

		private IHaircutViewer avatarViewer = null;

		private Color averageColor = Color.clear;

		private Color defaultColor = Color.clear;

		public Color CurrentColor { get; private set; }

		public Vector4 CurrentTint { get; private set; }

		public void SetRendererValues(MeshRenderer renderer)
		{
			averageColor = CoreTools.CalculateAverageColor(renderer.material.mainTexture as Texture2D);
			CurrentTint = CoreTools.CalculateTint(CurrentColor, averageColor);
			renderer.material.SetVector("_ColorTarget", CurrentColor);
			renderer.material.SetVector("_ColorTint", CurrentTint);
			renderer.material.SetFloat("_TintCoeff", 0.8f);
		}

		void Start ()
		{
			if (colorPickerGameObject == null)
				Debug.LogWarning("Color picker is not set!");
			else
			{
				colorPicker = colorPickerGameObject.GetComponentInChildren<IColorPicker>();
				colorPicker.SetOnValueChangeCallback(OnColorChange);
			}

			avatarViewer = GetComponent<IHaircutViewer>();

			if (avatarViewer == null) {
				Debug.LogWarning ("Avatar viewer reference is null");
				return;
			}

			avatarViewer.displayedHaircutChanged += OnHaircutChanged;
			avatarViewer.shaderTypeChanged += OnShaderChanged;
		}

		void OnDestroy ()
		{
			if (avatarViewer != null)
				avatarViewer.displayedHaircutChanged -= OnHaircutChanged;
			Debug.LogFormat ("Haircut recolorer destroyed");
		}

		private void CalculateHaircutParameters ()
		{
			var haircutObject = avatarViewer.HaircutObject;
			if (haircutObject == null)
				return;

			var hairMeshRenderer = haircutObject.GetComponent<MeshRenderer> ();
			averageColor = CoreTools.CalculateAverageColor (hairMeshRenderer.material.mainTexture as Texture2D);
			Debug.LogFormat ("Haircut average color: {0}", averageColor.ToString ());
			ResetTint();
		}

		public void ResetTint ()
		{
			Color c = defaultColor == Color.clear ? averageColor : defaultColor;
			colorPicker.Color = c;
			OnColorChange(c);
		}

		public Color DefaultColor
		{
			set
			{
				defaultColor = value;
			}
		}

		private bool EnableRecoloring ()
		{
			var haircutObject = avatarViewer.HaircutObject;
			return haircutObject != null;
		}

		private void UpdateRecoloring ()
		{
			bool enable = EnableRecoloring();
			CalculateHaircutParameters();
			OnColorChange(colorPicker.Color);
			colorPickerPanel.SetActive(enable);
		}

		private void OnHaircutChanged (string newHaircutId)
		{
			UpdateRecoloring ();
		}

		private void OnShaderChanged (bool isUnlit)
		{
			UpdateRecoloring ();
		}

		private void OnColorChange (Color color)
		{
			var haircutObject = avatarViewer.HaircutObject;
			if (haircutObject == null)
				return;

			var hairMeshRenderer = haircutObject.GetComponent<MeshRenderer> ();

			CurrentColor = color;
			CurrentTint = CoreTools.CalculateTint (color, averageColor);
			hairMeshRenderer.material.SetVector ("_ColorTarget", color);
			hairMeshRenderer.material.SetVector ("_ColorTint", CurrentTint);
			hairMeshRenderer.material.SetFloat ("_TintCoeff", 0.8f);
		}
	}
}
