using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Crane2 : MonoBehaviour {
	// Public Properties
	private float TwistLeftSpeed = 50f;
	private float TwistRightSpeed = -50f;

	// Internal
	private tntHingeLink mast;
	private tntSliderLink arm;
	private tntHingeLink trolley;

    private Dictionary<string, tntBallLink> cubes = new Dictionary<string, tntBallLink>();

	private tntBallLink hookhead;
	private tntHingeLink hook1;
	private tntHingeLink hook2;
	private tntHingeLink hook3;
	private tntHingeLink hook4;
	private bool clamp;

	// Use this for initialization
	void Start () {
		mast = transform.Find("Mast").GetComponent<tntHingeLink>();
		arm = transform.Find("Arm").GetComponent<tntSliderLink>();
		trolley = transform.Find("Trolley").GetComponent<tntHingeLink>();
		hookhead = transform.Find("Chain/HookHead").GetComponent<tntBallLink>();
		hook1 = transform.Find("Chain/Hook1").GetComponent<tntHingeLink>();
		hook2 = transform.Find("Chain/Hook2").GetComponent<tntHingeLink>();
		hook3 = transform.Find("Chain/Hook3").GetComponent<tntHingeLink>();
		hook4 = transform.Find("Chain/Hook4").GetComponent<tntHingeLink>();

        for (int i = 10; i < 52; ++i)
        {
            if (1 == i % 2)
            {
                string name = "Chain/Cube" + i.ToString();
                cubes.Add(name, transform.Find(name).GetComponent<tntBallLink>());
            }
        }
	}
	
	void TwistLeft()
	{
		hookhead.SetMotorDesiredSpeed (0, TwistRightSpeed);

        foreach (tntBallLink cube in cubes.Values)
        {
            cube.SetMotorDesiredSpeed(0, TwistRightSpeed);
        }
        //cubes["cube32"].SetMotorDesiredSpeed(0, -TwistRightSpeed);
	}
	
	void TwistRight(){
		hookhead.SetMotorDesiredSpeed (0, TwistLeftSpeed);

        foreach (tntBallLink cube in cubes.Values)
        {
            cube.SetMotorDesiredSpeed(0, TwistLeftSpeed);
        }
	}
	
	void StopTwist(){
		hookhead.SetMotorDesiredSpeed (0, 0);

        foreach (tntBallLink cube in cubes.Values)
        {
            cube.SetMotorDesiredSpeed(0, 0);
        }
	}
	void Update ()
	{

		// Trolley in/out //
		if (Input.GetKeyDown(KeyCode.W)){
			arm.SetMotorDesiredSpeed (0, -5);
		}
		
		if (Input.GetKeyUp(KeyCode.W)){
			arm.SetMotorDesiredSpeed (0, 0);
		}
		
		if (Input.GetKeyDown(KeyCode.S)){
			arm.SetMotorDesiredSpeed (0, 5);
		}
		
		if (Input.GetKeyUp(KeyCode.S)){
			arm.SetMotorDesiredSpeed (0, 0);
		}
		//////////////////////////////////

		// Arm Left/Right
		if (Input.GetKeyDown(KeyCode.A)){
			mast.SetMotorDesiredSpeed (0, 15);
		}
		
		if (Input.GetKeyUp(KeyCode.A)){
			mast.SetMotorDesiredSpeed (0, 0);
		}
		
		if (Input.GetKeyDown(KeyCode.D)){
			mast.SetMotorDesiredSpeed (0, -15);
		}
		
		if (Input.GetKeyUp(KeyCode.D)){
			mast.SetMotorDesiredSpeed (0, 0);
		}
		//////////////////////////////////
		
		// Chain In/Out
		if (Input.GetKeyDown(KeyCode.UpArrow)){
			trolley.SetMotorDesiredSpeed (0, 50);
		}
		
		if (Input.GetKeyUp(KeyCode.UpArrow)){
			trolley.SetMotorDesiredSpeed (0, 0);
		}
		
		if (Input.GetKeyDown(KeyCode.DownArrow)){
			trolley.SetMotorDesiredSpeed (0, -50);
		}
		
		if (Input.GetKeyUp(KeyCode.DownArrow)){
			trolley.SetMotorDesiredSpeed (0, 0);
		}
		//////////////////////////////////
		
		// Chain Twist
		if (Input.GetKeyDown(KeyCode.RightArrow)){
			TwistRight();
		}
		
		if (Input.GetKeyUp(KeyCode.RightArrow)){
			StopTwist();
		}
		
		if (Input.GetKeyDown(KeyCode.LeftArrow)){
			TwistLeft();
		}
		
		if (Input.GetKeyUp(KeyCode.LeftArrow)){
			StopTwist();
		}

		
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (!clamp) {
				clamp = true;
				hook1.SetMotorDesiredSpeed (0, 100);
				hook2.SetMotorDesiredSpeed (0, 100);
				hook3.SetMotorDesiredSpeed (0, 100);
				hook4.SetMotorDesiredSpeed (0, 100);
				hook1.SetMotorDesiredPosition (0, 25);
				hook2.SetMotorDesiredPosition (0, 25);
				hook3.SetMotorDesiredPosition (0, 25);
				hook4.SetMotorDesiredPosition (0, 25);
			} else {
				clamp = false;
				hook1.SetMotorDesiredSpeed (0, 20);
				hook2.SetMotorDesiredSpeed (0, 20);
				hook3.SetMotorDesiredSpeed (0, 20);
				hook4.SetMotorDesiredSpeed (0, 20);
				hook1.SetMotorDesiredPosition (0, 0);
				hook2.SetMotorDesiredPosition (0, 0);
				hook3.SetMotorDesiredPosition (0, 0);
				hook4.SetMotorDesiredPosition (0, 0);
			}
		}






	}
}
