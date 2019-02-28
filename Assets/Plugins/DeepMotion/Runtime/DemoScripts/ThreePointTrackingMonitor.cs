using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/**
 * This class finds the controllers for head, left hand, and right hand 
 * And make the 3 point trackers follow the movement of the controllers
 */
public class ThreePointTrackingMonitor : MonoBehaviour
{
    private Transform lHandTarget;
    private Transform rHandTarget;
    private Transform headTarget;
    private tntBase rootBase;

    private ThreePointTrackingGameMgr gm;
    private ThreePointTrackingSettings settings;

    public bool enablePointsDeviationCheck;
    private float maxPointsAvgDeviation = 1.475f;
    public float pointsDeviateMaxTime = 3.75f;

    public bool enableRootSpinCheck = true;
    private float maxRootAngSpeed = 720f;
    public float rootSpinMaxTime = 0.875f;

    public bool enableRootRushCheck = true;    
    private float maxRootLineSpeed = 6.0f;
    public float rootRushMaxTime = 0.33f;

    private float pointsDeviateTimer = 0;
    private float rootSpinTimer = 0;
    private float rootRushTimer = 0;

    public float OutOfControlLifetime = 4.5f;
    [System.NonSerialized]
    public bool outOfControl = false; //used to act as a flag lock to prevent overlapping logics in edge cases

    // Use this for initialization
    void Start()
    {
        gm = ThreePointTrackingGameMgr.GetInstance();
        settings = FindObjectOfType<ThreePointTrackingSettings>();
        if (gm && gm.humanoidController) // gm should be available 
        {
            rootBase = gm.humanoidController.GetComponentInParent<tntBase>();
        }
    }

    public void SetTargets(Transform _lHandController, Transform _rHandController, Transform _hmd)
    {
        lHandTarget = _lHandController;
        rHandTarget = _rHandController;
        headTarget = _hmd;
    }

    public void RunUpdate()
    {
        if (MonitoringUnstableCondition())
        {
            outOfControl = true;
            if (settings.behaviorOnChaos == ThreePointTrackingSettings.BehaviorOnChaos.Death)
            {
                if (gm)
                {
                    //Nullify target links
                    gm.humanoidController.m_limbs.m_lHandTarget = null;
                    gm.humanoidController.m_limbs.m_rHandTarget = null;
                    gm.humanoidController.m_limbs.m_headTarget = null;

                    if (settings.VRMode)
                    {
                        //Switch Camera
                        gm.SwitchCamera(ThreePointTrackingGameMgr.SwitchType.ToSecondCamera);
                    }
                }
                MakeCharacterDead();
                Invoke("Disconnect", OutOfControlLifetime);
            }
            else if(settings.behaviorOnChaos == ThreePointTrackingSettings.BehaviorOnChaos.ResyncHead)
            {
                if (gm)
                {
                    ResyncCharacter(); //Snap back character head towards head tracker to achieve resync
                }
            }
        }
    }

    //This is not actually respawning in multiplayer mode right now
    private void Disconnect()
    {
        outOfControl = false;
        if (gm)
            gm.SwitchCamera(ThreePointTrackingGameMgr.SwitchType.ToHMD);

        ConnectionClient thisClient = FindObjectOfType<ConnectionClient>();          
        if (thisClient)
            thisClient.Disconnect();      
    }

    private void MakeCharacterDead()
    {
        var NP3PTDataSync = GetComponentInChildren<NetworkPlayer3PTDataSync>();
        if (NP3PTDataSync != null)
        {
            NP3PTDataSync.SetCharacterDead(); //Sync dead all
        }
    }

    private void ResyncCharacter()
    {
        var NP3PTDataSync = GetComponentInChildren<NetworkPlayer3PTDataSync>();
        if (NP3PTDataSync != null)
        {
            NP3PTDataSync.ResyncCharacter(); //re-sync all
        }
    }

    private bool MonitoringUnstableCondition()
    {
        tntLink head = gm.humanoidController.m_limbs.m_neck;
        tntLink lHand = gm.humanoidController.m_limbs.m_lHand;
        tntLink rHand = gm.humanoidController.m_limbs.m_rHand;

        //If any of the three point target tracker is not properly linked, don't do condition monitor since the situation is unpredictable
        //This need to be adapted later to a better form where you can calculate the average difference no matter you use one hand or two
        if (!gm.humanoidController.m_limbs.m_headTarget ||
            !gm.humanoidController.m_limbs.m_lHandTarget ||
            !gm.humanoidController.m_limbs.m_rHandTarget)
        {
            return false;
        }

        if (enablePointsDeviationCheck && !outOfControl)
        {
            float avgPtsDist = 0;
            if (headTarget && lHandTarget && rHandTarget)
            {
                avgPtsDist = ((headTarget.position - head.transform.position).magnitude +
                    (lHandTarget.position - lHand.transform.position).magnitude +
                    (rHandTarget.position - rHand.transform.position).magnitude);
            }

            if ((avgPtsDist / 3) > maxPointsAvgDeviation)
            {
                pointsDeviateTimer += Time.deltaTime;
            }
            else
            {
                if (pointsDeviateTimer < 0) { pointsDeviateTimer = 0; }
                else { pointsDeviateTimer -= Time.deltaTime; }
            }
            if (pointsDeviateTimer >= pointsDeviateMaxTime)
            {
                ResetTimers();
                Debug.Log("Out of balance due to big average points distance");
                return true;
            }
        }

        if (enableRootRushCheck && !outOfControl)
        {
            if (rootBase && (rootBase.GetLinearVelocity().magnitude > maxRootLineSpeed))
            {
                rootRushTimer += Time.deltaTime;
            }
            else
            {
                if (rootRushTimer < 0) { rootRushTimer = 0; }
                else { rootRushTimer -= Time.deltaTime; }
            }
            if (rootRushTimer >= rootRushMaxTime)
            {
                ResetTimers();
                Debug.Log("Out of balance due to rushing root");
                return true;
            }
        }

        if (enableRootSpinCheck && !outOfControl)
        {
            if (rootBase && (rootBase.GetAngularVelocity().magnitude > maxRootAngSpeed))
            {
                rootSpinTimer += Time.deltaTime;
            }
            else
            {
                if (rootSpinTimer < 0) { rootSpinTimer = 0; }
                else { rootSpinTimer -= Time.deltaTime; }
            }
            if (rootSpinTimer >= rootSpinMaxTime)
            {
                ResetTimers();
                Debug.Log("Out of balance due to spinning root");
                return true;
            }
        }

        return false;
    }

    private void ResetTimers()
    {
        pointsDeviateTimer = 0;
        rootRushTimer = 0;
        rootSpinTimer = 0;
    }
}
