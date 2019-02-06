using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
	public class ArticulatedRigidBody
	{
		public string name;
		public List<string> meshNames;
		public float mass;
		public float friction;
		public float restitution;
		public Vector3 moi;
		public Vector3 position;
		public Vector3 velocity;
		public Vector3 angularVelocity;
		public Quaternion orientation;
		public bool locked;
        public string mark;

		//temporary variables that we may end up populating
		public Vector3 color;
		public float radius;
		public Vector3 p1;
		public Vector3 p2;
		public Vector3 normal;
        public Quaternion rotation;

		// joints
		public ArticulatedJoint pJoint;
		public List<ArticulatedJoint> cJoints;
		
		public RB_TYPE shapeType = RB_TYPE.RB_NOT_IMPORTANT;

		public tntLink m_tntLink;

        public ArticulatedRigidBody()
        {
            meshNames = new List<string>();
            mass = 0f;
            moi = new Vector3();
            locked = false;
            radius = 0f;
            p1 = new Vector3();
            p2 = new Vector3();
            rotation = Quaternion.identity;
            m_tntLink = null;
            mark = null;

            position = Vector3.zero;
            orientation = Quaternion.identity;

            pJoint = null;
            cJoints = new List<ArticulatedJoint>();
        }

        public Vector3 GetWorldCoordinates(Vector3 localPoint)
        {
            return position + orientation * localPoint;
        }

        public Vector3 GetLocalCoordinates(Vector3 worldPoint)
        {
            return Quaternion.Inverse(orientation) * worldPoint - Quaternion.Inverse(orientation) * position;
        }

            public  string Deserialize(StreamReader sr)
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
                        Debug.Log("=> NAME : " + name);
                        break;
                    case RB_TYPE.RB_MESH_NAME:
                        string meshName = stringValues[1];
                        meshName = meshName.Substring(3, meshName.Length - 7);
                        Debug.Log("=> MESH_NAME : " + meshName);
                        meshNames.Add(meshName);
                        break;
                    case RB_TYPE.RB_MASS:
                        mass = float.Parse(stringValues[1]);
                        Debug.Log("=> MASS : " + mass);
                        break;
                    case RB_TYPE.RB_MOI:
                        moi = new Vector3(float.Parse(stringValues[1]),
                                          float.Parse(stringValues[2]),
                                          float.Parse(stringValues[3]));
                        moi *= ArticulatedBodyImporter.MOIScale();
                        Debug.Log("=> MOI : " + moi);
                        break;
                    case RB_TYPE.RB_END_RB:
                        Debug.Log("-------- Done parsing " + shapeType + " ------------");
                        return name;					//and... done
                    case RB_TYPE.RB_COLOUR:
                        color = new Vector3(float.Parse(stringValues[1]),
                                            float.Parse(stringValues[2]),
                                            float.Parse(stringValues[3]));
                        Debug.Log("=> COLOR : " + color);
                        break;
                    case RB_TYPE.RB_SPHERE:
                        p1.x = float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip();
                        p1.y = float.Parse(stringValues[2]);
                        p1.z = float.Parse(stringValues[3]);
                        p1 *= ArticulatedBodyImporter.DimensionScale();

                        radius = float.Parse(stringValues[4]);
                        radius *= ArticulatedBodyImporter.DimensionScale();
                        shapeType = RB_TYPE.RB_SPHERE;
                        Debug.Log("=> SPHERE : center at " + p1 + " of radius " + radius);
                        break;
                    case RB_TYPE.RB_CAPSULE:
                        p1.x = float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip();
                        p1.y = float.Parse(stringValues[2]);
                        p1.z = float.Parse(stringValues[3]);
                        p1 *= ArticulatedBodyImporter.DimensionScale();
                        p2.x = float.Parse(stringValues[4]) * ArticulatedBodyImporter.HandnessFlip();
                        p2.y = float.Parse(stringValues[5]);
                        p2.z = float.Parse(stringValues[6]);
                        p2 *= ArticulatedBodyImporter.DimensionScale();
                        radius = float.Parse(stringValues[7]);
                        radius *= ArticulatedBodyImporter.DimensionScale();
                        shapeType = RB_TYPE.RB_CAPSULE;
                        Debug.Log("=> CAPSULE : " + p1 + " " + p2 + " radius " + radius);
                        break;
                    case RB_TYPE.RB_BOX:
                        p1.x = float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip();
                        p1.y = float.Parse(stringValues[2]);
                        p1.z = float.Parse(stringValues[3]);
                        p1 *= ArticulatedBodyImporter.DimensionScale();
                        p2.x = float.Parse(stringValues[4]) * ArticulatedBodyImporter.HandnessFlip();
                        p2.y = float.Parse(stringValues[5]);
                        p2.z = float.Parse(stringValues[6]);
                        p2 *= ArticulatedBodyImporter.DimensionScale();
                        if (stringValues.Length == 11)
                        {
                            rotation.x = float.Parse(stringValues[7]);
                            rotation.y = float.Parse(stringValues[8]);
                            rotation.z = float.Parse(stringValues[9]);
                            rotation.w = float.Parse(stringValues[10]);
                        }
                        shapeType = RB_TYPE.RB_BOX;
                        Debug.Log("=> BOX : " + p1 + " " + p2);
                        break;
                    case RB_TYPE.RB_PLANE:
                        normal.x = float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip();
                        normal.y = float.Parse(stringValues[2]);
                        normal.z = float.Parse(stringValues[3]);
                        p1.x = float.Parse(stringValues[4]) * ArticulatedBodyImporter.HandnessFlip();
                        p1.y = float.Parse(stringValues[5]);
                        p1.z = float.Parse(stringValues[6]);
                        p1 *= ArticulatedBodyImporter.DimensionScale();
                        shapeType = RB_TYPE.RB_PLANE;
                        Debug.Log("=> PLANE : " + p1 + " normal " + normal);
                        break;
                    case RB_TYPE.RB_NOT_IMPORTANT:
                        if (line.Length != 0 && line[0] != '#')
                        	Debug.Log("Ignoring input line: " + line);
                        break;
                    case RB_TYPE.RB_LOCKED:
                        locked = bool.Parse(stringValues[1]);
                        Debug.Log("=> LOCKED : " + locked);
                        break;
                    case RB_TYPE.RB_POSITION:
                        position.x = float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip();
                        position.y = float.Parse(stringValues[2]);
                        position.z = float.Parse(stringValues[3]);
                        position *= ArticulatedBodyImporter.DimensionScale();
                        Debug.Log("=> POSITION : " + position);
                        break;
                    case RB_TYPE.RB_ORIENTATION:
                        float angle = float.Parse(stringValues[1]);
                        orientation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg,
                            new Vector3(float.Parse(stringValues[2]) * ArticulatedBodyImporter.HandnessFlip(),
                                        float.Parse(stringValues[3]),
                                        float.Parse(stringValues[4])));
                        Debug.Log("=> ORIENTATION : " + orientation);
                        break;
                    case RB_TYPE.RB_VELOCITY:
                        velocity = new Vector3(float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip(),
                                               float.Parse(stringValues[2]),
                                               float.Parse(stringValues[3]));
                        Debug.Log("=> VELOCITY : " + velocity);
                        break;
                    case RB_TYPE.RB_ANGULAR_VELOCITY:
                        angularVelocity = new Vector3(float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip(),
                                                      float.Parse(stringValues[2]),
                                                      float.Parse(stringValues[3]));
                        Debug.Log("=> ANGULAR_VELOCITY : " + angularVelocity);
                        break;
                    case RB_TYPE.RB_FRICTION_COEFF:
                        friction = float.Parse(stringValues[1]);
                        Debug.Log("=> FRICTION : " + friction);
                        break;
                    case RB_TYPE.RB_RESTITUTION_COEFF:
                        restitution = float.Parse(stringValues[1]);
                        Debug.Log("=> RESTITUION : " + restitution);
                        break;
                    case RB_TYPE.RB_ODE_GROUND_COEFFS:
                        Debug.Log("RB_ODE_GROUND_COEFFS not supported");
                        break;
                    case RB_TYPE.RB_PLANAR:
                        Debug.Log("RB_PLANAR not supported");
                        break;
                    case RB_TYPE.RB_MARK:
                        mark = stringValues[1];
                        break;
                    default:
                        Debug.LogError("Incorrect rigid body input file with nexpected line: " + line);
                        break;
                }
            }
			return name;
		}
	}
}