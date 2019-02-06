using UnityEngine;
using System.Collections;

public class TankNormalTracks : MonoBehaviour {
	
	private tntHingeLink L1;
	private tntHingeLink L2;
	private tntHingeLink L3;
	private tntHingeLink L4;
	private tntHingeLink L5;
	private tntHingeLink L6;
	private tntHingeLink R1;
	private tntHingeLink R2;
	private tntHingeLink R3;
	private tntHingeLink R4;
	private tntHingeLink R5;
	private tntHingeLink R6;
	private bool left;
	private bool stopleft;
	private bool right;
	private bool stopright;
	private bool forward;
	private bool stopforward;
	private bool back;
	private bool stopback;

    void Start () {
		L1 = transform.Find("L1").GetComponent<tntHingeLink>();
		L2 = transform.Find("L2").GetComponent<tntHingeLink>();
		L3 = transform.Find("L3").GetComponent<tntHingeLink>();
		L4 = transform.Find("L4").GetComponent<tntHingeLink>();
		L5 = transform.Find("L5").GetComponent<tntHingeLink>();
		L6 = transform.Find("L6").GetComponent<tntHingeLink>();
		R1 = transform.Find("R1").GetComponent<tntHingeLink>();
		R2 = transform.Find("R2").GetComponent<tntHingeLink>();
		R3 = transform.Find("R3").GetComponent<tntHingeLink>();
		R4 = transform.Find("R4").GetComponent<tntHingeLink>();
		R5 = transform.Find("R5").GetComponent<tntHingeLink>();
		R6 = transform.Find("R6").GetComponent<tntHingeLink>();
		left = true;
		right = true;
		stopforward = true;
		stopback = true;

	}

	void LeftForward()
	{
		L1.SetMotorDesiredSpeed (0, 600);
		L2.SetMotorDesiredSpeed (0, 600);
		L3.SetMotorDesiredSpeed (0, 600);
		L4.SetMotorDesiredSpeed (0, 600);
		L5.SetMotorDesiredSpeed (0, 600);
		L6.SetMotorDesiredSpeed (0, 500);
	}

	void RightForward()
	{
		R1.SetMotorDesiredSpeed (0, 600);
		R2.SetMotorDesiredSpeed (0, 600);
		R3.SetMotorDesiredSpeed (0, 600);
		R4.SetMotorDesiredSpeed (0, 600);
		R5.SetMotorDesiredSpeed (0, 600);
		R6.SetMotorDesiredSpeed (0, 500);
	}

	void LeftBack()
	{
		L1.SetMotorDesiredSpeed (0, -600);
		L2.SetMotorDesiredSpeed (0, -600);
		L3.SetMotorDesiredSpeed (0, -600);
		L4.SetMotorDesiredSpeed (0, -600);
		L5.SetMotorDesiredSpeed (0, -600);
		L6.SetMotorDesiredSpeed (0, -500);
	}

	void RightBack()
	{
		R1.SetMotorDesiredSpeed (0, -600);
		R2.SetMotorDesiredSpeed (0, -600);
		R3.SetMotorDesiredSpeed (0, -600);
		R4.SetMotorDesiredSpeed (0, -600);
		R5.SetMotorDesiredSpeed (0, -600);
		R6.SetMotorDesiredSpeed (0, -500);
	}

	void LeftStop()
	{
		L1.SetMotorDesiredSpeed (0, 50);
		L2.SetMotorDesiredSpeed (0, 50);
		L3.SetMotorDesiredSpeed (0, 50);
		L4.SetMotorDesiredSpeed (0, 50);
		L5.SetMotorDesiredSpeed (0, 50);
		L6.SetMotorDesiredSpeed (0, 50);
	}

	void RightStop()
	{
		R1.SetMotorDesiredSpeed (0, 50);
		R2.SetMotorDesiredSpeed (0, 50);
		R3.SetMotorDesiredSpeed (0, 50);
		R4.SetMotorDesiredSpeed (0, 50);
		R5.SetMotorDesiredSpeed (0, 50);
		R6.SetMotorDesiredSpeed (0, 50);
		{
		}
	}
		
	// Update is called once per frame
	void Update ()
	{

		if (Input.GetKeyDown (KeyCode.W))
		{
			forward = true;
			stopforward = false;
			}

		if (Input.GetKeyUp(KeyCode.W))
		{
			forward = false;
			stopforward = true;
		}

		if (Input.GetKeyDown (KeyCode.S))
		{
			back = true;
			stopback = false;
		}

		if (Input.GetKeyUp(KeyCode.S))
		{
			back = false;
			stopback = true;
		}

		if (Input.GetKeyDown(KeyCode.D))
		{
			left = false;
			stopleft = true;
		}

		if (Input.GetKeyUp(KeyCode.D))
		{
			left = true;
			stopleft = false;
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			right = false;
			stopright = true;
		}

		if (Input.GetKeyUp(KeyCode.A))
		{
			right = true;
			stopright = false;
		}
			
		{
		
			if (forward)
			if (right)	
			{
				LeftForward();
			}
		
			if (forward)
			if (left)
			{
				RightForward();
			}
		
			if (back)
			if (right)
			{
				LeftBack();
			}
		
			if (back)
			if (left)
			{
				RightBack();
			}

			if (forward)
			if (stopright)
			{
				LeftStop();
			}

			if (forward)
			if (stopleft)
			{
				RightStop();
			}

			if (back)
			if (stopright)
			{
				LeftStop();
			}

			if (back)
			if (stopleft)
			{
				RightStop();
			}

			if (stopforward)
			if (stopback)
			{
				LeftStop();
			}

			if (stopforward)
			if (stopback)
			{
				RightStop();
			}

			if (stopforward)
			if (stopback)
			if (stopleft)
			if (right)
			{
				LeftForward();
				RightBack();
			}

			if (stopforward)
			if (stopback)
			if (stopright)
			if (left)
			{
				LeftBack();
				RightForward();
			}
				

		}

	}
}
