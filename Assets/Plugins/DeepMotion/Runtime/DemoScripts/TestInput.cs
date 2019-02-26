using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInput : MonoBehaviour
{
    public Transform testTransform;

    [SerializeField]
    private float inputValue = .0f;

    public float InputValue
    {
        get
        {
            if (testTransform == null)
            {
                return inputValue;
            }
            else
            {
                return testTransform.localPosition.y;
            }
        }
    }
}
