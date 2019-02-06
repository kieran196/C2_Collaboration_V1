using PhysicsAPI;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleHumanoidDriver : MonoBehaviour
{
    public float m_desiredHeading;
    public float m_velocity;
	public float m_sideVelocity;

    public float m_acceleration = 1.0f;
    public float m_maxSteering = 1.0f;
    public float m_maxStrafingAccel = 0.3f;
    public float m_maxStrafingVelocity = 2.5f;
    public float m_maxVelocity = 1.75f;
    public float m_minVelocity = -1.0f;
	public float m_strafeDistanceSqr = 100.0f;
    public float m_horizontal = 0;
    public float m_vertical = 0;

	private IntPtr m_engineController;
	protected tntHumanoidController m_controller;

    protected tntBase m_rootTntBase;
    protected Transform m_rootTrans;
			
    void Awake()
    {
        m_controller = GetComponentInChildren<tntHumanoidController>();
        m_rootTrans = m_controller.transform.parent;
		m_rootTntBase = m_rootTrans.GetComponent<tntBase>();
    }

    // Use this for initialization
    public void Start()
    {
		m_engineController = m_controller.GetEngineController();
		if (m_engineController != IntPtr.Zero)
		{
			m_desiredHeading = TNT.acGetDesiredHeading(m_engineController);
		}

        m_controller.UpdateHeading(m_desiredHeading);
		m_controller.UpdateVelocity(m_velocity, m_sideVelocity, 0);
    }

    // Update is called once per frame
    public virtual void Update()
    {
        // Grab input from keyboard simulated joystick: WASD
        m_horizontal = Input.GetAxis("Horizontal");
        m_vertical = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.Q))
        {
            m_sideVelocity += m_maxStrafingAccel * Time.deltaTime;
        } else if ((Input.GetKey(KeyCode.E)))
        {
            m_sideVelocity -= m_maxStrafingAccel * Time.deltaTime;
        }
        if (m_sideVelocity > m_maxStrafingVelocity)
            m_sideVelocity = m_maxStrafingVelocity;
        else if (m_sideVelocity < -m_maxStrafingVelocity)
            m_sideVelocity = -m_maxStrafingVelocity;

        if (m_horizontal != 0.0f)
		{
            m_desiredHeading += m_horizontal * m_maxSteering * Mathf.Deg2Rad;
			if (m_desiredHeading > Mathf.PI)
				m_desiredHeading -= 2 * Mathf.PI;
			else if (m_desiredHeading < -Mathf.PI)
				m_desiredHeading += 2 * Mathf.PI;
        }
		
		if (m_vertical != 0.0f)
		{
			float newVelocity = m_velocity + m_vertical * m_acceleration * Time.deltaTime;
			if (newVelocity < m_minVelocity)
				newVelocity = m_minVelocity;
			else if (newVelocity > m_maxVelocity)
			{
				newVelocity = m_maxVelocity;
			}
			m_velocity = newVelocity;
			if (m_velocity < m_minVelocity)
				m_velocity = m_minVelocity;
			else if (m_velocity > m_maxVelocity)
			{
				m_velocity = m_maxVelocity;
			}
        } 
			
		m_controller.UpdateVelocity(m_velocity, m_sideVelocity, 0);
		m_controller.UpdateHeading(m_desiredHeading);
	}
}
