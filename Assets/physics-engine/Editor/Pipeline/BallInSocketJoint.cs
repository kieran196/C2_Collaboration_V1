using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
    class BallInSocketJoint : ArticulatedJoint
    {
        public Vector3 swingAxis1, swingAxis2;
        //and this one is stored in child coordinates - this is the twist axis
        public Vector3 twistAxis;
        //and the min and max allowed angles along the two swing axes (define an ellipsoid that
        //can be offset if the min/max angles are not equal in magnitude)
        public float minSwingAngle1, maxSwingAngle1, minSwingAngle2, maxSwingAngle2;
        //and limits around the twist axis
        public float minTwistAngle, maxTwistAngle;
        public bool useSwingAngle1Limit, useSwingAngle2Limit, useTwistAngleLimit;
        public bool useSwingAngle1Motor, useSwingAngle2Motor, useTwistAngleMotor;

        public BallInSocketJoint(string axes)
        {
            readAxes(axes);
            useSwingAngle1Limit = useSwingAngle2Limit = useTwistAngleLimit = false;
            useSwingAngle1Motor = useSwingAngle2Motor = useTwistAngleMotor = false;
        }

		// This method is used to pass in information regarding the rotation axes. The string
		// that is passed in is expected to have been read from an input file.
		//
		public override void readAxes(string axes)
		{
            axes = axes.TrimStart();
            string[] stringValues = axes.Split(' ');
            if (stringValues.Length == 9)
            {
                swingAxis1.x = float.Parse(stringValues[0]) * ArticulatedBodyImporter.HandnessFlip();
                swingAxis1.y = float.Parse(stringValues[1]);
                swingAxis1.z = float.Parse(stringValues[2]);
                swingAxis2.x = float.Parse(stringValues[3]) * ArticulatedBodyImporter.HandnessFlip();
                swingAxis2.y = float.Parse(stringValues[4]);
                swingAxis2.z = float.Parse(stringValues[5]);
                twistAxis.x = float.Parse(stringValues[6]) * ArticulatedBodyImporter.HandnessFlip();
                twistAxis.y = float.Parse(stringValues[7]);
                twistAxis.z = float.Parse(stringValues[8]);
            } else if (stringValues.Length == 6)
            {
                swingAxis1.x = float.Parse(stringValues[0]) * ArticulatedBodyImporter.HandnessFlip();
                swingAxis1.y = float.Parse(stringValues[1]);
                swingAxis1.z = float.Parse(stringValues[2]);
                twistAxis.x = float.Parse(stringValues[3]) * ArticulatedBodyImporter.HandnessFlip();
                twistAxis.y = float.Parse(stringValues[4]);
                twistAxis.z = float.Parse(stringValues[5]);
                swingAxis2 = Vector3.Cross(swingAxis1, twistAxis);
            }

            swingAxis1.Normalize();
            swingAxis2.Normalize();
            twistAxis.Normalize();
		}

		// This method is used to pass information regarding the joint limits for a joint.
		// The string that is passed in is expected to
		// Have been read from an input file.
		public override void readJointLimits(string limits)
		{
			limits = limits.TrimStart();
			string[] stringValues = limits.Split(' ');
			minSwingAngle1 = float.Parse(stringValues[0]);
			maxSwingAngle1 = float.Parse(stringValues[1]);
			minSwingAngle2 = float.Parse(stringValues[2]);
			maxSwingAngle2 = float.Parse(stringValues[3]);
			if (stringValues.Length > 4) {
				minTwistAngle = float.Parse (stringValues [4]);
				maxTwistAngle = float.Parse (stringValues [5]);
			}
			//useJointLimits = true;
		}

        public override void readJointUseLimits (string useLimits)
        {
            useLimits = useLimits.TrimStart ();
            string[] stringValues = useLimits.Split (' ');
            useSwingAngle1Limit = bool.Parse (stringValues [0]);
            useSwingAngle2Limit = bool.Parse (stringValues [1]);
            if (stringValues.Length > 2)
				useTwistAngleLimit = bool.Parse(stringValues [2]);
        }
        
        public override void readJointUseMotor (string useMotor)
        {
            useMotor = useMotor.TrimStart ();
            string[] stringValues = useMotor.Split (' ');
            useSwingAngle1Motor = bool.Parse (stringValues [0]);
            useSwingAngle2Motor = bool.Parse (stringValues [1]);
            useTwistAngleMotor = bool.Parse (stringValues [2]);
        }
	}
}