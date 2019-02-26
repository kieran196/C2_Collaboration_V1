using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransformInfoSimple
{
    public Vector3 position;
    public Quaternion rotation;

    public TransformInfoSimple(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
