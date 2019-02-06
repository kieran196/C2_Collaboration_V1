using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
    class ArticulatedUniversalJoint : ArticulatedJoint
    {
        public Vector3 axisA;
        public Vector3 axisB;

        public float minAngleA, maxAngleA;
        public float minAngleB, maxAngleB;
        public bool useAngleALimit, useAngleBLimit;
        public bool useAngleAMotor, useAngleBMotor;

        public ArticulatedUniversalJoint(string axes)
        {
            readAxes(axes);
            useAngleALimit = useAngleBLimit = false;
            useAngleAMotor = useAngleBMotor = false;
        }

        // This method is used to pass in information regarding the rotation axes. The string
        // that is passed in is expected to have been read from an input file.
        //
        public override void readAxes(string axes)
        {
            axes = axes.TrimStart();
            string[] stringValues = axes.Split(' ');
            axisA.x = float.Parse(stringValues[0]) * ArticulatedBodyImporter.HandnessFlip();
            axisA.y = float.Parse(stringValues[1]);
            axisA.z = float.Parse(stringValues[2]);
            axisB.x = float.Parse(stringValues[3]) * ArticulatedBodyImporter.HandnessFlip();
            axisB.y = float.Parse(stringValues[4]);
            axisB.z = float.Parse(stringValues[5]);
            axisA.Normalize();
            axisB.Normalize();
        }

		// This method is used to pass information regarding the joint limits for a joint.
		// The string that is passed in is expected to
		// Have been read from an input file.
		public override void readJointLimits(string limits)
		{
			limits = limits.TrimStart();
			string[] stringValues = limits.Split(' ');
			minAngleA = float.Parse(stringValues[0]);
			maxAngleA = float.Parse(stringValues[1]);
			minAngleB = float.Parse(stringValues[2]);
			maxAngleB = float.Parse(stringValues[3]);

			//useJointLimits = true;
		}

        public override void readJointUseLimits (string useLimits)
        {
            useLimits = useLimits.TrimStart ();
            string[] stringValues = useLimits.Split (' ');
            useAngleALimit = bool.Parse (stringValues [0]);
            useAngleBLimit = bool.Parse (stringValues [1]);
        }
        
        public override void readJointUseMotor (string useMotor)
        {
            useMotor = useMotor.TrimStart ();
            string[] stringValues = useMotor.Split (' ');
            useAngleAMotor = bool.Parse (stringValues [0]);
            useAngleBMotor = bool.Parse (stringValues [1]);
        }
	}
}