using UnityEngine;
using System.Collections;
using System;

//Build a tool to help constrain this game object by another source object without a hierachy structure
public class TransformConstraint : MonoBehaviour
{
    [SerializeField]
    private GameObject constraintSource = null;
    [Serializable]
    private enum ConstraintType { Parent, Point, Orientation, Aim, }
    [SerializeField]
    private ConstraintType constraintType = ConstraintType.Parent;
    [SerializeField]
    bool keepOffset = false;
    [SerializeField]
    private bool updateConstraint = true;
    private Vector3 posOffset = Vector3.zero;               // in parent's frame
    private Quaternion rotOffset = Quaternion.identity;     // in parent's frame

    void Start()
    {
        if (keepOffset)
        {
            posOffset = Quaternion.Inverse(constraintSource.transform.rotation) * (constraintSource.transform.position - transform.position);
            rotOffset = Quaternion.Inverse(constraintSource.transform.rotation) * transform.rotation;
        }
        Constrain();
    }

    void LateUpdate()
    {
        if (constraintSource != null)
        {
            if (updateConstraint) { Constrain(); }
        }
        else { Destroy(gameObject); }
    }

    public void SetConstrainSource(GameObject go)
    {
        constraintSource = go;
    }

    public void Constrain()
    {
        if (constraintSource != null)
        {
            switch (constraintType)
            {
                case ConstraintType.Parent:                    
                    transform.position = constraintSource.transform.position - constraintSource.transform.rotation * posOffset;
                    transform.rotation = constraintSource.transform.rotation * rotOffset;
                    break;
                case ConstraintType.Point:
                    transform.position = constraintSource.transform.position - constraintSource.transform.rotation * posOffset;
                    break;
                case ConstraintType.Orientation:
                    transform.rotation = constraintSource.transform.rotation * rotOffset;
                    break;
                case ConstraintType.Aim:
                    break;
                default:
                    break;
            }
        }
        else { Destroy(gameObject); }
    }
}
