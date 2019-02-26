using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
    class ArticulatedHingeJoint : ArticulatedJoint
    {
        public Vector3 axis;
        public float minAngle, maxAngle;
        public bool useAngleLimit, useAngleMotor;

        public ArticulatedHingeJoint(string axes)
        {
            readAxes(axes);
            useAngleLimit = useAngleMotor = false;
        }

        // This method is used to pass in information regarding the rotation axes. The string
        // that is passed in is expected to have been read from an input file.
        //
        public override void readAxes(string axes)
        {
            axes = axes.TrimStart();
            string[] stringValues = axes.Split(' ');
            axis.x = float.Parse(stringValues[0]) * ArticulatedBodyImporter.HandnessFlip();
            axis.y = float.Parse(stringValues[1]);
            axis.z = float.Parse(stringValues[2]);
            axis.Normalize();
        }

        // This method is used to pass information regarding the joint limits for a joint.
        // The string that is passed in is expected to
        // Have been read from an input file.
        public override void readJointLimits(string limits)
        {
            limits = limits.TrimStart();
            string[] stringValues = limits.Split(' ');
            minAngle = float.Parse(stringValues[0]);
            maxAngle = float.Parse(stringValues[1]);

            //useJointLimits = true;
        }

        public override void readJointUseLimits (string useLimits)
        {
            useLimits = useLimits.TrimStart ();
            string[] stringValues = useLimits.Split (' ');
            useAngleLimit = bool.Parse (stringValues [0]);
        }
        
        public override void readJointUseMotor (string useMotor)
        {
            useMotor = useMotor.TrimStart ();
            string[] stringValues = useMotor.Split (' ');
            useAngleMotor = bool.Parse (stringValues [0]);
        }
	}
}