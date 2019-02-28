/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, April 2017
*/

using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	public class ParametersGenerator
	{
		/// <summary>
		/// Prepares JSON with parameters required for avatar calculation.
		/// </summary>
		public virtual string GetAvatarCalculationParamsJson(AvatarResources avatarResources)
		{
			JSONObject resourcesJson = new JSONObject();

			if (avatarResources != null)
			{
				if (!IsListNullOrEmpty(avatarResources.blendshapes))
					resourcesJson["blendshapes"] = ListToJsonNode(avatarResources.blendshapes);

				if (!IsListNullOrEmpty(avatarResources.haircuts))
					resourcesJson["haircuts"] = ListToJsonNode(avatarResources.haircuts);
			}

			return resourcesJson.ToString(4);
		}

		/// <summary>
		/// Converts list with resources to JSONNode
		/// </summary>
		protected JSONNode ListToJsonNode(List<string> list)
		{
			Dictionary<string, JSONArray> groups = new Dictionary<string, JSONArray>();

			foreach (string item in list)
			{
				string[] subItems = item.Split(new char[] { '\\', '/' });
				if (subItems.Length == 2)
				{
					if (!groups.ContainsKey(subItems[0]))
						groups.Add(subItems[0], new JSONArray());
					groups[subItems[0]][""] = subItems[1];
				}
				else
					Debug.LogErrorFormat("Invalid resource name: {0}", item);
			}

			JSONObject baseNode = new JSONObject();
			foreach (var group in groups)
			{
				baseNode[group.Key] = group.Value;
			}

			return baseNode;
		}

		/// <summary>
		/// Checks if list is null or empty
		/// </summary>
		protected bool IsListNullOrEmpty(List<string> list)
		{
			return list == null || list.Count == 0;
		}
	}
}
