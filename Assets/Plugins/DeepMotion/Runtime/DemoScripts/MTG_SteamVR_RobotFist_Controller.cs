using UnityEngine;
using System.Collections.Generic;
using Valve.VR;

public class MTG_SteamVR_RobotFist_Controller : MonoBehaviour
{
	public SteamVR_TrackedObject.EIndex index = SteamVR_TrackedObject.EIndex.None;

	private Transform fist = null;
	private Transform fingure0j0 = null;
	private Transform fingure0j1 = null;
	private Transform fingure1j0 = null;
	private Transform fingure1j1 = null;
	private Transform fingure1j2 = null;
	private Transform fingure2j0 = null;
	private Transform fingure2j1 = null;
	private Transform fingure2j2 = null;

	void OnEnable()
	{
		fist = transform.Find ("RobotFist");
		if (fist != null) 
		{
			Transform[] children = fist.GetComponentsInChildren<Transform> ();
			foreach (Transform child in children) 
			{
				if (child.name == "fingure0_j0")
					fingure0j0 = child;
				else if (child.name == "fingure0_j1")
					fingure0j1 = child;
				else if (child.name == "fingure1_j0")
					fingure1j0 = child;
				else if (child.name == "fingure1_j1")
					fingure1j1 = child;
				else if (child.name == "fingure1_j2")
					fingure1j2 = child;
				else if (child.name == "fingure2_j0")
					fingure2j0 = child;
				else if (child.name == "fingure2_j1")
					fingure2j1 = child;
				else if (child.name == "fingure2_j2")
					fingure2j2 = child;
			}
		}
	}

	public void SetDeviceIndex(int index)
	{
		if (System.Enum.IsDefined(typeof(SteamVR_TrackedObject.EIndex), index))
			this.index = (SteamVR_TrackedObject.EIndex)index;
	}

	void Update()
	{
		if (fist == null)
			return;
		
		EVRButtonId buttonId = EVRButtonId.k_EButton_SteamVR_Trigger;
		Vector2 triggerAxis = new Vector2(0,0);
		if (SteamVR_Controller.Input((int)index).GetTouch(buttonId))
		{
			triggerAxis = SteamVR_Controller.Input((int)index).GetAxis(buttonId);
		}
			
		//Debug.Log("axis: " + triggerAxis);

		fingure0j0.localRotation = Quaternion.Euler(triggerAxis.x * -10f, 0, 0);
		fingure0j1.localRotation = Quaternion.Euler(triggerAxis.x * -30f, 0, 0);
		fingure1j0.localRotation = Quaternion.Euler(triggerAxis.x * 20f, 0, 0);
		fingure1j1.localRotation = Quaternion.Euler(triggerAxis.x * 40f, 0, 0);
		fingure1j2.localRotation = Quaternion.Euler(triggerAxis.x * 60f, 0, 0);
		fingure2j0.localRotation = Quaternion.Euler(triggerAxis.x * 20f, 0, 0);
		fingure2j1.localRotation = Quaternion.Euler(triggerAxis.x * 40f, 0, 0);
		fingure2j2.localRotation = Quaternion.Euler(triggerAxis.x * 60f, 0, 0);
	}
}

