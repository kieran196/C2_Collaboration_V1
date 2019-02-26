using UnityEngine;
using System.Collections;

public class MonsterTruckPhysXController : MonoBehaviour {
	
	private HingeJoint steeringFL;
	private HingeJoint steeringFR;
	private HingeJoint wheelFL;
	private HingeJoint wheelFR;
	private HingeJoint wheelRL;
	private HingeJoint wheelRR;
//	private float frontWheelRadius;
//	private float rearWheelRadius;
//	private float frontRearRatio;
	
	//private float vertical;
	//private float horizontal;
	
//	private float velocity;
	
//	public Joystick joystick;
//	public float acceleration = 286.5f;     // degrees
//	public float breaking = 2865.0f;        // degrees
//	public float maxSteering = 50f;         // degrees
//	public float maxVelocity = 2865.0f;     // degrees
	
	// Use this for initialization
	void Start () {
		steeringFL = transform.Find("SteerFL").GetComponent<HingeJoint>();
		steeringFR = transform.Find("SteerFR").GetComponent<HingeJoint>();
		wheelFL = transform.Find("WheelFL").GetComponent<HingeJoint>();
		wheelFR = transform.Find("WheelFR").GetComponent<HingeJoint>();
		wheelRL = transform.Find("WheelRL").GetComponent<HingeJoint>();
		wheelRR = transform.Find("WheelRR").GetComponent<HingeJoint>();
//		frontWheelRadius = wheelFL.GetComponent<ShapeCylinder>().topRadius;
//		rearWheelRadius = wheelRL.GetComponent<ShapeCylinder>().bottomRadius;
		
		//vertical = horizontal = 0.0f;
//		velocity = 0.0f;
	}
	
	// Update is called once per frame
	void Update ()
	{

		if (Input.GetKeyDown(KeyCode.W)){
			JointMotor motorFL = wheelFL.motor;
			motorFL.targetVelocity = 300;
			wheelFL.motor= motorFL;

			JointMotor motorFR = wheelFR.motor;
			motorFR.targetVelocity = 300;
			wheelFR.motor= motorFR;

			JointMotor motorRL = wheelRL.motor;
			motorRL.targetVelocity = 300;
			wheelRL.motor= motorRL;

			JointMotor motorRR = wheelRR.motor;
			motorRR.targetVelocity = 300;
			wheelRR.motor= motorRL;
		}
		
		if (Input.GetKeyUp(KeyCode.W)){
			JointMotor motorFL = wheelFL.motor;
			motorFL.targetVelocity = 0;
			wheelFL.motor= motorFL;

			JointMotor motorFR = wheelFR.motor;
			motorFR.targetVelocity = 0;
			wheelFR.motor= motorFR;

			JointMotor motorRL = wheelRL.motor;
			motorRL.targetVelocity = 0;
			wheelRL.motor= motorRL;

			JointMotor motorRR = wheelRR.motor;
			motorRR.targetVelocity = 0;
			wheelRR.motor= motorRL;
		}
		
		if (Input.GetKeyDown(KeyCode.S)){
			JointMotor motorFL = wheelFL.motor;
			motorFL.targetVelocity = -300;
			wheelFL.motor= motorFL;

			JointMotor motorFR = wheelFR.motor;
			motorFR.targetVelocity = -300;
			wheelFR.motor= motorFR;

			JointMotor motorRL = wheelRL.motor;
			motorRL.targetVelocity = -300;
			wheelRL.motor= motorRL;

			JointMotor motorRR = wheelRR.motor;
			motorRR.targetVelocity = -300;
			wheelRR.motor= motorRL;
		}
		
		if (Input.GetKeyUp(KeyCode.S)){
			JointMotor motorFL = wheelFL.motor;
			motorFL.targetVelocity = 0;
			wheelFL.motor= motorFL;

			JointMotor motorFR = wheelFR.motor;
			motorFR.targetVelocity = 0;
			wheelFR.motor= motorFR;

			JointMotor motorRL = wheelRL.motor;
			motorRL.targetVelocity = 0;
			wheelRL.motor= motorRL;

			JointMotor motorRR = wheelRR.motor;
			motorRR.targetVelocity = 0;
			wheelRR.motor= motorRL;
		}







		if (Input.GetKeyDown(KeyCode.A)){
			JointMotor motorSFL = steeringFL.motor;
			motorSFL.targetVelocity = -200;
			steeringFL.motor= motorSFL;
			
			JointMotor motorSFR = steeringFR.motor;
			motorSFR.targetVelocity = -200;
			steeringFR.motor= motorSFR;
		}
		
		if (Input.GetKeyUp(KeyCode.A)){
			JointMotor motorSFL = steeringFL.motor;
			motorSFL.targetVelocity = 0;
			steeringFL.motor= motorSFL;
			
			JointMotor motorSFR = steeringFR.motor;
			motorSFR.targetVelocity = 0;
			steeringFR.motor= motorSFR;
		}
		
		if (Input.GetKeyDown(KeyCode.D)){
			JointMotor motorSFL = steeringFL.motor;
			motorSFL.targetVelocity = 200;
			steeringFL.motor= motorSFL;
			
			JointMotor motorSFR = steeringFR.motor;
			motorSFR.targetVelocity = 200;
			steeringFR.motor= motorSFR;
		}
		
		if (Input.GetKeyUp(KeyCode.D)){
			JointMotor motorSFL = steeringFL.motor;
			motorSFL.targetVelocity = 0;
			steeringFL.motor= motorSFL;
			
			JointMotor motorSFR = steeringFR.motor;
			motorSFR.targetVelocity = 0;
			steeringFR.motor= motorSFR;
		}


//		float vertical = 0.0f;
//		float horizontal = 0.0f;
//		#if UNITY_IPHONE && !UNITY_EDITOR
//		vertical = joystick.position.y;
//		horizontal = joystick.position.x;
//		#else
//		vertical = Input.GetAxis("Vertical");
//		horizontal = Input.GetAxis("Horizontal");
//		#endif
		
//		if (vertical != 0.0f)
//		{
//			// acceleratoing
//			if (Mathf.Abs(velocity) < maxVelocity)
//				velocity += vertical * acceleration * Time.deltaTime;
//		} else
//		{
//			// breaking
//			if (velocity > breaking * Time.deltaTime)
//				velocity -= breaking * Time.deltaTime;
//			else if (velocity < -breaking * Time.deltaTime)
//				velocity += breaking * Time.deltaTime;
//		}
//		
//		wheelFL.m_dofData[0].m_desiredVelocity =
//			wheelFR.m_dofData[0].m_desiredVelocity =
//				wheelRL.m_dofData[0].m_desiredVelocity =
//				wheelRR.m_dofData[0].m_desiredVelocity = velocity;
//		
//		wheelFL.SetMotorDesiredSpeed(0, velocity);
//		wheelFR.SetMotorDesiredSpeed(0, velocity);
//		wheelRL.SetMotorDesiredSpeed(0, velocity * frontRearRatio);
//		wheelRR.SetMotorDesiredSpeed(0, velocity * frontRearRatio);
//		
//		steeringFL.m_dofData[0].m_desiredPosition =
//			steeringFR.m_dofData[0].m_desiredPosition = horizontal * maxSteering;
//		
//		steeringFL.SetMotorDesiredPosition(0, steeringFL.m_dofData[0].m_desiredPosition);
//		steeringFR.SetMotorDesiredPosition(0, steeringFR.m_dofData[0].m_desiredPosition);
	}
}
