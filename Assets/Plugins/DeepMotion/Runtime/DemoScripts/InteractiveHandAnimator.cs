using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveHandAnimator : MonoBehaviour {

	[HideInInspector]
	public SteamVR_TrackedController trackedController;
	private tntHingeLink hinge;
	private float desiredVelocity = -800f;
	public float maxLowerLimit = -120f;
	private bool reset = true;
	private bool init = false;

	private float defaultLimitLow = -120f;
	private float defaultLimitHigh = 0f;
	private float defaultMaxLimitForce = 100f;

	void Start () {
		hinge = GetComponent<tntHingeLink> ();
	}

	void Update () {
		if (trackedController!=null && hinge!=null)
		{
			if (!init) {
				init = true;
				defaultLimitLow = hinge.m_dofData [0].m_limitLow;
				defaultLimitHigh = hinge.m_dofData [0].m_limitHigh;
				defaultMaxLimitForce = hinge.m_dofData [0].m_maxLimitForce;
			}

			var system = Valve.VR.OpenVR.System;
			Valve.VR.VRControllerState_t controllerState = new Valve.VR.VRControllerState_t();
			if (system != null && system.GetControllerState(trackedController.controllerIndex, ref controllerState, (uint)System.Runtime.InteropServices.Marshal.SizeOf(controllerState)))
			{
				float triggerAnalogState = controllerState.rAxis1.x;
				if (triggerAnalogState > 0) {
					if (reset) {
						reset = false;
						hinge.EnableMotor (0);
						hinge.AddLimits (0,defaultLimitLow,defaultLimitHigh,defaultMaxLimitForce);
					}

					hinge.SetMotorDesiredSpeed (0, desiredVelocity);
					hinge.SetLimitLower(0, triggerAnalogState * maxLowerLimit);

				} else if (!reset){
					reset = true;
					hinge.SetMotorDesiredSpeed (0, 0f);
					hinge.SetLimitLower(0, defaultLimitLow);

					hinge.RemoveLimit (0);
					hinge.DisableMotor (0);
				}
			}
		}
	}
}
