// #define FIX_BODY_ORIENTATION

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
	public class ArticulatedFigure
	{
		ArticulatedRigidBody root;
        GameObject rootGameObject;
        string m_skinnedMeshName;	// skine mesh relative path

		public string name;
		public float mass;

		public ArticulatedFigure()
		{
            root = null;
            rootGameObject = null;
            m_skinnedMeshName = null;
		}

        // pass bAsProxy as true to create mesh proxy, basically only create empty game objects to make biped controllers work correctly
        private void CreateMeshes(GameObject go, Quaternion q, List<string> meshNames, bool bAsProxy)
		{
            for (int i = 0; i < meshNames.Count; ++i)
            {
                string meshName = meshNames[i];
                //if (!bAsProxy) // even skinned mesh needs this left/right swap, look at tntBipedController.Initialize()
                {
                    if (ArticulatedBodyImporter.flipLeftRightMeshes)
                    {
                        for (int j = meshName.Length - 2; j >= 0; --j)
                        {
                            if (meshName[j] == '/')
                            {                 
                                // these exclusions (lower, rear, etc.) are so ugly, 
                                // if we still need to swap, we may consider to use "l_" and "r_" as reserved prefix
                                if (meshName[j + 1] == 'l' && meshName[j + 2] != 'o')  // exclude "lower"
                                {
                                    char[] array = meshName.ToCharArray();
                                    array[j + 1] = 'r';
                                    meshName = new string(array);
                                }
                                else if (meshName[j + 1] == 'r' && meshName[j + 2] != 'e')  // exclude "rear"
                                {
                                    char[] array = meshName.ToCharArray();
                                    array[j + 1] = 'l';
                                    meshName = new string(array);
                                }
                                break;
                            }
                        }
                    }
                }

                GameObject mesh;
                if (bAsProxy)
                {
                    mesh = new GameObject();
                    mesh.name = Path.GetFileName(meshName); // create correct name from a path
                }
                else
                {
                    string path = ArticulatedBodyImporter.PhysicsResourceRoot() + "/" + meshName + ".obj";
                    Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    if (prefab == null)
                    {
                        Debug.LogError("Fail to load :" + meshName);
                        return;
                    }
                    mesh  = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                }

                mesh.transform.parent = go.transform;
                mesh.transform.localPosition = Vector3.zero;
                mesh.transform.localEulerAngles = Vector3.zero;
                mesh.transform.localScale *= ArticulatedBodyImporter.DimensionScale();

                if (!bAsProxy) // Let the sectional meshes follow the collider
                {
					if (ArticulatedBodyImporter.flipFrontBackMeshes)
						mesh.transform.localEulerAngles = Vector3.up * 180;
                    else
						mesh.transform.rotation = q;

				}
            }
		}

		private GameObject CreateCollider(GameObject container, ArticulatedRigidBody rigidBody)
		{
            tntCompoundCollider compound = null;
            GameObject childObject = null;
            tntRigidBody massHolder = null;

            switch (rigidBody.shapeType)
            {
                case RB_TYPE.RB_SPHERE:
                	compound = container.AddComponent<tntCompoundCollider>();
                    childObject = new GameObject("sphere");
                	childObject.transform.parent = compound.transform;
                	SphereCollider ball = childObject.AddComponent<SphereCollider>();
                	ball.radius = rigidBody.radius;
                	childObject.transform.localPosition = rigidBody.p1;
                	// The following two lines will be ignored by the engine because MOI
                	// is directly set at the compound shape level
                	massHolder = childObject.AddComponent<tntRigidBody>();
                	massHolder.m_mass = rigidBody.mass;
                	break;
                case RB_TYPE.RB_CAPSULE:
                    compound = container.AddComponent<tntCompoundCollider>();
                    childObject = new GameObject("capsule");
                    childObject.transform.parent = compound.transform;
                    CapsuleCollider capsule = childObject.AddComponent<CapsuleCollider>();
                    capsule.radius = rigidBody.radius;
                    // p1 and p2 are the centers of the top and bottom surfaces of the
                    // cylinder part of the capsule
					Vector3 ab = rigidBody.p2 - rigidBody.p1;
                    capsule.height = ab.magnitude + 2 * rigidBody.radius;
                    ab.Normalize();

                    childObject.transform.localPosition = (rigidBody.p1 + rigidBody.p2) / 2;

                    // If the Capsule's main axis is not aligned to the local Y axis
                    if (Mathf.Abs(Vector3.Dot(ab, Vector3.up) - 1f) > 1e-6)
                    {
    					Vector3 axis = Vector3.Cross(Vector3.up, ab);	
    					axis.Normalize();
    					float rotAngle = Vector3.Angle(Vector3.up, ab);
    					childObject.transform.localRotation = Quaternion.AngleAxis(rotAngle, axis);
                    }

                    // The following two lines will be ignored by the engine because MOI
                    // is directly set at the compound shape level
                    massHolder = childObject.AddComponent<tntRigidBody>();
                    massHolder.m_mass = rigidBody.mass;
                	break;
				case RB_TYPE.RB_BOX:
                    compound = container.AddComponent<tntCompoundCollider>();
                    childObject = new GameObject("box");
                    childObject.transform.parent = compound.transform;
                    BoxCollider box = childObject.AddComponent<BoxCollider>();
                    box.size = new Vector3(Mathf.Abs(rigidBody.p2.x - rigidBody.p1.x),
                                           Mathf.Abs(rigidBody.p2.y - rigidBody.p1.y),
                                           Mathf.Abs(rigidBody.p2.z - rigidBody.p1.z));

                    childObject.transform.localPosition = (rigidBody.p1 + rigidBody.p2) / 2;
                    childObject.transform.localRotation = rigidBody.rotation;
                    // The following two lines will be ignored by the engine because MOI
                    // is directly set at the compound shape level
                    massHolder = childObject.AddComponent<tntRigidBody>();
                    massHolder.m_mass = rigidBody.mass;
                	break;
                default:
        	        break;
            }
			return childObject;
		}

		private void AddJointToScene(ArticulatedJoint joint, ArticulatedRigidBody parent, ArticulatedRigidBody child)
        {
                // TBD: Set child orientation from parent orietnation and joint
                //
                // Derive child position from parent position and the joint
                Vector3 jointWorld = parent.GetWorldCoordinates(joint.pJPos);
                child.position = jointWorld - joint.cJPos;

                // Create a game object hosting the tntLink
                GameObject baseObject = new GameObject(joint.name);
                tntChildLink childLink = null;

#if FIX_BODY_ORIENTATION
                Quaternion qRel = Quaternion.Inverse(parent.orientation) * child.orientation;
#endif
                if (joint as ArticulatedHingeJoint != null)
                {
                    ArticulatedHingeJoint hingeJoint = joint as ArticulatedHingeJoint;

#if FIX_BODY_ORIENTATION
                    // Calibrate child orientation
                    Vector3 jointAxis;
                    float jointAngle;
                    qRel.ToAngleAxis(out jointAngle, out jointAxis);
                    jointAngle *= Vector3.Dot(jointAxis, hingeJoint.axis);
                    child.orientation = parent.orientation * Quaternion.AngleAxis(jointAngle, jointAxis);
#endif
                    childLink = baseObject.AddComponent<tntHingeLink>();
                    childLink.m_parent = parent.m_tntLink;
                    tntHingeLink hinge = childLink as tntHingeLink;
                    hinge.m_axisA = hingeJoint.axis; 
                    hinge.PivotA = hingeJoint.pJPos;
                    hinge.PivotB = hingeJoint.cJPos;
                    //if (hingeJoint.useJointLimits)
                    {
                        hinge.m_dofData[0].m_limitLow = hingeJoint.minAngle * Mathf.Rad2Deg;
                        hinge.m_dofData[0].m_limitHigh = hingeJoint.maxAngle * Mathf.Rad2Deg;
                        hinge.m_dofData[0].m_useLimit = hingeJoint.useAngleLimit;
                        hinge.m_dofData[0].m_maxLimitForce = 100f;
                        hinge.m_dofData[0].m_useMotor = hingeJoint.useAngleMotor;
                        hinge.m_dofData[0].m_maxMotorForce = 20f;
                    }
                } else if (joint as ArticulatedUniversalJoint != null)
                {
                    ArticulatedUniversalJoint universalJoint = joint as ArticulatedUniversalJoint;
#if FIX_BODY_ORIENTATION
                    //compute two rotations, such that qRel = tmpQ1 * tmpQ2, and tmpQ2 is a rotation
                    // about the vector axisB (expressed in child coordinates)
                    // qRel.decomposeRotation(&tmpQ1, &tmpQ2, b);

                    // Rotate axisB by the quaternion qRel
                    Vector3 axisBInP = qRel * universalJoint.axisB;
                    Vector3 rotAxis = Vector3.Cross(axisBInP, universalJoint.axisB);
                    rotAxis.Normalize();
                    float rotAngle = -Mathf.Acos(Vector3.Dot(axisBInP, rotAxis));

                    Quaternion qA = Quaternion.AngleAxis(rotAngle, rotAxis);
                    Quaternion qB = Quaternion.Inverse(qA) * qRel;

                    qA.ToAngleAxis(out rotAngle, out rotAxis);
                    float mod = Mathf.Abs(Vector3.Dot(rotAxis, universalJoint.axisA));
                    rotAngle *= mod;
                    child.orientation = parent.orientation * 
                        Quaternion.AngleAxis(rotAngle, universalJoint.axisA) *
                        qB;
#endif
                    childLink = baseObject.AddComponent<tntUniversalLink>();
                    childLink.m_parent = parent.m_tntLink;
                    tntUniversalLink universal = childLink as tntUniversalLink;
                    universal.m_axisA = universalJoint.axisA;
                    // Hack: For some reason the 2nd Universal Axis of .rbs file is 90 degree
                    // off
                    // universal.m_axisB = Vector3.Cross(universalJoint.axisA, universalJoint.axisB); 
                    universal.m_axisB = universalJoint.axisB;
                    universal.PivotA = universalJoint.pJPos;
                    universal.PivotB = universalJoint.cJPos;

                    //if (universalJoint.useJointLimits)
                    {
                        universal.m_dofData[0].m_limitLow = universalJoint.minAngleB * Mathf.Rad2Deg;
                        universal.m_dofData[0].m_limitHigh = universalJoint.maxAngleB * Mathf.Rad2Deg;
                        universal.m_dofData[0].m_useLimit = universalJoint.useAngleBLimit;;
                        universal.m_dofData[0].m_maxLimitForce = 100f;
                        universal.m_dofData[0].m_useMotor = universalJoint.useAngleBMotor;
                        universal.m_dofData[0].m_maxMotorForce = 40f;
                        universal.m_dofData[1].m_limitLow = universalJoint.minAngleA * Mathf.Rad2Deg;
                        universal.m_dofData[1].m_limitHigh = universalJoint.maxAngleA * Mathf.Rad2Deg;
                        universal.m_dofData[1].m_useLimit = universalJoint.useAngleALimit;;
                        universal.m_dofData[1].m_maxLimitForce = 100f;
                        universal.m_dofData[1].m_useMotor = universalJoint.useAngleAMotor;
                        universal.m_dofData[1].m_maxMotorForce = 40f;
                    }
                } else if (joint as BallInSocketJoint != null)
                {
                    BallInSocketJoint ballJoint = joint as BallInSocketJoint;
                    childLink = baseObject.AddComponent<tntBallLink>();
                    childLink.m_parent = parent.m_tntLink;
                    tntBallLink ball = childLink as tntBallLink;
                    ball.PivotA = ballJoint.pJPos;
                    ball.PivotB = ballJoint.cJPos;
                    //if (ballJoint.useJointLimits)
                    {
                        ball.m_dofData[0].m_limitLow = ballJoint.minTwistAngle * Mathf.Rad2Deg;
                        ball.m_dofData[0].m_limitHigh = ballJoint.maxTwistAngle * Mathf.Rad2Deg;
                        ball.m_dofData[0].m_useLimit = ballJoint.useTwistAngleLimit;
                        ball.m_dofData[0].m_maxLimitForce = 100f;
                        ball.m_dofData[0].m_useMotor = ballJoint.useTwistAngleMotor;
                        ball.m_dofData[0].m_maxMotorForce = 20f;
                        ball.m_dofData[1].m_limitLow = ballJoint.minSwingAngle1 * Mathf.Rad2Deg;
                        ball.m_dofData[1].m_limitHigh = ballJoint.maxSwingAngle1 * Mathf.Rad2Deg;
                        ball.m_dofData[1].m_useLimit = ballJoint.useSwingAngle1Limit;
                        ball.m_dofData[1].m_maxLimitForce = 100f;
                        ball.m_dofData[1].m_useMotor = ballJoint.useSwingAngle1Motor;
                        ball.m_dofData[1].m_maxMotorForce = 20f;
                        ball.m_dofData[2].m_limitLow = ballJoint.minSwingAngle2 * Mathf.Rad2Deg;
                        ball.m_dofData[2].m_limitHigh = ballJoint.maxSwingAngle2 * Mathf.Rad2Deg;
                        ball.m_dofData[2].m_useLimit = ballJoint.useSwingAngle2Limit;
                        ball.m_dofData[2].m_maxLimitForce = 100f;
                        ball.m_dofData[2].m_useMotor = ballJoint.useSwingAngle2Motor;
                        ball.m_dofData[2].m_maxMotorForce = 20f;
                    }
                } else if (joint as ArticulatedFixedJoint != null)
                {
                    childLink = baseObject.AddComponent<tntFixedLink>();
                    childLink.m_parent = parent.m_tntLink;
                }
                childLink.m_material = Resources.Load<PhysicMaterial> (joint.phyMatPath);
                childLink.ParseMaterial ();
				childLink.m_collideWithParent = false;
                if (child.mark != null)
                {
                    childLink.m_mark = child.mark;
                }
                child.m_tntLink = childLink;
                childLink.m_mass = child.mass;
                childLink.m_moi = child.moi;
                    
                CreateCollider(baseObject, child);
                bool bCreateAsProxy = (m_skinnedMeshName != null);
                CreateMeshes(baseObject, child.rotation, child.meshNames, bCreateAsProxy);
                    
                baseObject.transform.position = child.position;
                //baseObject.transform.rotation = child.rotation;
                baseObject.transform.parent = rootGameObject.transform;
		}

        private void BuildArticulatedFigure(ArticulatedRigidBody root)
        {
			List<ArticulatedRigidBody> bodies = new List<ArticulatedRigidBody>();
			bodies.Add(root);
			int currentBody = 0;

			// Breadh first traversal of the skeleton tree
			while (currentBody < bodies.Count)
			{
				ArticulatedRigidBody parent = bodies[currentBody];
				//add all the children joints to scene
	            for (int i = 0; i < parent.cJoints.Count; ++i)
	            {	
					ArticulatedJoint joint = parent.cJoints[i];
					ArticulatedRigidBody child = joint.child;
					bodies.Add(child);
					AddJointToScene(joint, parent, child);
	            }
				++currentBody;
            }
        }

		// load and instantiate skinned mesh and bind it to physics rig
		// destroy the empty skinned mesh object at last
		private void CreateSkinnedMeshAndBind()
		{
            string path = ArticulatedBodyImporter.PhysicsResourceRoot() + "/" + m_skinnedMeshName + ".fbx";
            Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            GameObject skinnedMesh  = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			skinnedMesh.transform.localPosition = rootGameObject.transform.localPosition;
			skinnedMesh.transform.localRotation = rootGameObject.transform.localRotation;
			skinnedMesh.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f); // scale the mesh using this

			for (int i = 0; i < skinnedMesh.transform.childCount; ++i) 
			{
				skinnedMesh.transform.GetChild(i).localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				Debug.Log("name" + skinnedMesh.transform.GetChild(i).gameObject.name);
			}

			for (int i = 0; i < rootGameObject.transform.childCount; ++i) 
			{
				// primise: skinned mesh and physics rig uses same joints names
				string namePhysicRigJoint = rootGameObject.transform.GetChild(i).name;
				Transform t = skinnedMesh.transform.Find(namePhysicRigJoint);
				if (t)
				{
					t.parent = rootGameObject.transform.GetChild(i);
					t.name = t.name + "_skin";
				}
			}

            // put game objects that can't be processed into the physics rig root
			for (int i = 0; i < skinnedMesh.transform.childCount; ++i) 
			{
				skinnedMesh.transform.GetChild(i).parent = rootGameObject.transform;
			}
			GameObject.DestroyImmediate(skinnedMesh);
		}

		public void Deserialize(StreamReader sr, GameObject rootObject)
		{
            Debug.Log("~~~~~~~~~~importing rbs~~~~~~~~~~~~~");
			//this is where it happens.
			for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
			{
				char[] chars = new char[] {' ', '\t'};
				line = line.TrimStart(chars);
				RB_TYPE lineType = RBTagParser.ParseTag(line);
				string[] stringValues = line.Split(' ');
				ArticulatedJoint tempJoint = null;

                switch (lineType)
                {
                    case RB_TYPE.RB_ROOT:
                        root = ArticulatedBodyImporter.getARBByName(stringValues[1]);
                        rootGameObject = rootObject;

                        // Create base link game object
                        GameObject baseObject = new GameObject(stringValues[0]);    // Put in joint's name

                        // Add base link
                        tntBase theBase = baseObject.AddComponent<tntBase>();
                        root.m_tntLink = theBase;
                        theBase.m_mass = root.mass;
                        theBase.m_moi = root.moi;
                        theBase.m_enableSelfCollision = false;
                        theBase.m_useGlobalVel = true;
                        theBase.m_highDefIntegrator = true;
                        theBase.m_simulationFrequencyMultiplier = 1;
                    
                        CreateCollider(baseObject, root);
                        bool bCreateAsProxy = (m_skinnedMeshName != null);
                        CreateMeshes(baseObject, Quaternion.identity, root.meshNames, bCreateAsProxy);

                        baseObject.transform.position = root.position;
                        // TBD: Set orientation of base link if it's specified in the RBS file
                        baseObject.transform.parent = rootGameObject.transform;
                        break;
                    case RB_TYPE.RB_JOINT_TYPE_UNIVERSAL:
                        tempJoint = new ArticulatedUniversalJoint(line.Substring(stringValues[0].Length + 1));
                        tempJoint.Deserialize(sr);
                        break;
                    case RB_TYPE.RB_JOINT_TYPE_HINGE:
                        tempJoint = new ArticulatedHingeJoint(line.Substring(stringValues[0].Length + 1));
                        tempJoint.Deserialize(sr);	
                    	break;
                    case RB_TYPE.RB_JOINT_TYPE_BALL_IN_SOCKET:
                        tempJoint = new BallInSocketJoint(line.Substring(stringValues[0].Length + 1));
                        tempJoint.Deserialize(sr);							
                        break;
                    case RB_TYPE.RB_JOINT_TYPE_FIXED:
                        tempJoint = new ArticulatedFixedJoint();
                        tempJoint.Deserialize(sr);                         
                        break;
                    case RB_TYPE.RB_END_ARTICULATED_FIGURE:
                        // Now we parsed all joints, type to traverse the skeleton from root to leaves
                        BuildArticulatedFigure(root);
                        if (ArticulatedBodyImporter.pivotAtJoint)
                            APEShiftPivot.FixGameObjectPivots(rootGameObject);
						if (m_skinnedMeshName != null) // load, instantiate, and bind skinned mesh
						{
							CreateSkinnedMeshAndBind();
						}
                        break;
					case RB_TYPE.RB_SKINNED_MESH: // parse skinned mesh referrence
						string skinnedMeshName = stringValues[1];
						m_skinnedMeshName = skinnedMeshName.Substring(3, skinnedMeshName.Length - 7);
						Debug.Log("=> Skinned mesh name : " + m_skinnedMeshName);
						break;
                    default:
                        break;
                }
			}
		}

        public static void Serialize(StreamWriter sw, GameObject rootObject)
        {
            sw.WriteLine(RBTagParser.GenerateTag(RB_TYPE.RB_ARTICULATED_FIGURE));

            //Transform[] children = rootObject.transforms[0].GetComponentsInChildren<Transform>();
            
            //foreach (Transform child in children)
            //{
            //}

            sw.WriteLine(RBTagParser.GenerateTag(RB_TYPE.RB_END_ARTICULATED_FIGURE));
        }
	}
}