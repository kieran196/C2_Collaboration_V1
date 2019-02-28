using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
	public class ArticulatedJoint
	{
        public string name;
        //this is the parent link
        public ArticulatedRigidBody parent;
        //this is the location of the joint on the parent body - expressed in the parent's local coordinates
        public Vector3 pJPos;
        //this is the child link
        public ArticulatedRigidBody child;
        //this is the location of the joint on the child body - expressed in the child's local coordinates 
        //NOTE: the locations of the parent and child joint locations must overlap in world coordinates
        public Vector3 cJPos;
        //this variable is used to indicate if this joint has joint limits or not (the details regarding
        //the limits are specified on a per joint type basis)
        //public bool useJointLimits;
        public string phyMatPath;

        public ArticulatedJoint()
        {
            pJPos = new Vector3();
            cJPos = new Vector3();
            //useJointLimits = false;
            parent = null;
            child = null;
            phyMatPath = null;
        }

		// This method is used to compute the relative orientation between the parent and the child rigid bodies,
		// expressed in the coordinate frame of the parent.
		public Quaternion computeRelativeOrientation()
		{
			//if qp is the quaternion that gives the orientation of the parent, and qc gives the orientation
			//of the child, then  qp^-1 * qc gives the relative orientation between the child and the parent,
			//expressed in the parent's coordinates (child to parent)
			return Quaternion.Inverse(parent.orientation) * child.orientation;
		}

		public virtual void readJointLimits(string limits) {}

        public virtual void readJointUseLimits (string limits) {}
        
        public virtual void readJointUseMotor (string limits) {}

		public virtual void readAxes(string line) {}

        public void Deserialize(StreamReader sr)
        {
        	//this is where it happens.
        	for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
        	{
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                RB_TYPE lineType = RBTagParser.ParseTag(line);
                string[] stringValues = line.Split(' ');

                switch (lineType)
                {
                    case RB_TYPE.RB_NAME:
                        name = stringValues[1];
                        break;
                    case RB_TYPE.RB_PARENT:
                        parent = ArticulatedBodyImporter.getARBByName(stringValues[1]);
                        break;
                    case RB_TYPE.RB_CHILD:
                        child = ArticulatedBodyImporter.getARBByName(stringValues[1]);
                        break;
                    case RB_TYPE.RB_CPOS:
                        cJPos.x = float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip();
                        cJPos.y = float.Parse(stringValues[2]);
                        cJPos.z = float.Parse(stringValues[3]);
                        cJPos *= ArticulatedBodyImporter.DimensionScale();
                        break;
                    case RB_TYPE.RB_PPOS:
                        pJPos.x = float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip();
                        pJPos.y = float.Parse(stringValues[2]);
                        pJPos.z = float.Parse(stringValues[3]);
                        pJPos *= ArticulatedBodyImporter.DimensionScale();
                        break;
                    case RB_TYPE.RB_END_JOINT:
                        parent.cJoints.Add(this);
                        child.pJoint = this;
                        return;
                    case RB_TYPE.RB_JOINT_LIMITS:
                        readJointLimits(line.Substring(stringValues[0].Length + 1));
                        break;
                    case RB_TYPE.RB_JOINT_USE_LIMITS:
                        readJointUseLimits (line.Substring (stringValues [0].Length + 1));
                        break;
                    case RB_TYPE.RB_JOINT_USE_MOTOR:
                        readJointUseMotor (line.Substring (stringValues [0].Length + 1));
                        break;
                    case RB_TYPE.RB_PHYSICSMATERIAL:
                        string physicsMatPath = line.Substring (stringValues [0].Length + 1);
                        phyMatPath = physicsMatPath.Substring ("../".Length, physicsMatPath.Length - "../".Length - ".physicmaterial".Length);
                        break;
                    default:
                        break;
                }
            }
        }
	}
}