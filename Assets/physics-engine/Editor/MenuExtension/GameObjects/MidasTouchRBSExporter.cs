using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;
using Pipeline;
using System.Collections.Generic;

public class MidasTouchRBSExporter : ScriptableObject
{
    const string floatSerializeFormat = "F4";
    const string sErrDialogTitle = "RBS Exporter Error";

    public MidasTouchRBSExporter()
    {

    }

    [MenuItem ("GameObject/Articulated Physics/Export/MidasTouch RBS")]
    static void DoExportRBSEntry()
    {
        DoExportRBS();
    }

    static bool CheckMeshFrontBackOrientation() // returns true if flip is NOT required, returns false otherwise
    {
        bool bMechOrientationNeedToBeFlipped = false;
        tntLink[] joints = Selection.transforms[0].GetComponentsInChildren<tntLink>();
        int flipCount = 0;
        int meshCount = 0;
        foreach (tntLink joint in joints)
        {
            MeshRenderer[] staticMeshes = joint.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in staticMeshes)
            {
                GameObject go = mr.transform.parent.gameObject;
                meshCount++;
                if (go.transform.localEulerAngles == Vector3.up * 180)
                {
                    bMechOrientationNeedToBeFlipped = true;
                    flipCount++;
                }
            }
        }
        
        if (flipCount != 0 && flipCount != meshCount)
        {
            bMechOrientationNeedToBeFlipped = false; // we can't decide the front-back flip so set it to false
            EditorUtility.DisplayDialog(sErrDialogTitle, "Error: Not all the meshes have the same front-back orientation!", "OK");
        }
        
        return (bMechOrientationNeedToBeFlipped == false);
    }

    private static void DoExportRBS()
    {
        if (Selection.transforms.Length == 0)
        {
            EditorUtility.DisplayDialog(sErrDialogTitle, "Nothing was selected!\nPlease select one object to export.", "OK");
            return;
        }

        if (Selection.transforms.Length > 1)
        {
            EditorUtility.DisplayDialog(sErrDialogTitle, "Please select only one object to export!", "OK");
            return;
        }

        // Remove the rotation and scale of the root game object so that the exporting operations are done in world frame
        Quaternion savedRotationRoot = Selection.transforms[0].transform.rotation;
        Vector3 savedScaleRoot = Selection.transforms[0].transform.localScale;
        Selection.transforms [0].transform.rotation = Quaternion.identity;
        Selection.transforms [0].transform.localScale = new Vector3 (1, 1, 1);

        string rbsName = Selection.transforms[0].name;
        string fileName = EditorUtility.SaveFilePanel("Export .rbs file", "", rbsName, "rbs");
 
        StringBuilder rbsString = new StringBuilder();
 
        rbsString.Append("#" + rbsName + ".rbs"
            + "\n#" + System.DateTime.Now.ToLongDateString() 
            + "\n#" + System.DateTime.Now.ToLongTimeString()
            + "\n#-------" 
            + "\n\n");

        rbsString.Append("# Define the rigid bodies and joints that will make up the character\n");

        using (StreamWriter sw = new StreamWriter(fileName))
        {
            sw.WriteLine(rbsString);

            IndentModifer im = new IndentModifer();

            if ( !CheckMeshFrontBackOrientation() )
            {
                sw.Write(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_FRONTBACKMESHFLIP));
                sw.WriteLine(" true");
                sw.Write("\n");
            }

            // serialize rigid bodies
            tntLink[] joints = Selection.transforms[0].GetComponentsInChildren<tntLink>();
            foreach(tntLink joint in joints)
            {
                SerializeRigidBody(sw, im, joint);
            }

            // serialize articulated figure
            im.Reset();
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_ARTICULATED_FIGURE));
            im.Indent();
            SkinnedMeshRenderer skmesh = Selection.transforms[0].GetComponentInChildren<SkinnedMeshRenderer>();
            if (skmesh == null)
            {
                Debug.LogError("Failed to find SkinnedMeshRenderer under the GameObject selected. Move the mesh render under the model !\n");
            }
            else
            {
                string relativePath = AssetDatabase.GetAssetPath(skmesh.sharedMesh.GetInstanceID());
                // FIXME: get rid of Assets/Resources, should be better way to get relative path
                relativePath = relativePath.Substring(Pipeline.ArticulatedBodyImporter.PhysicsResourceRoot().Length);
                sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_SKINNED_MESH) + " .." + relativePath);
            }

            sw.Write("\n");
            foreach(tntLink joint in joints)
            {
                SerializeJoint(sw, im, joint);
            }

            im.Raise();
            sw.WriteLine(RBTagParser.GenerateTag(RB_TYPE.RB_END_ARTICULATED_FIGURE));
        }

        // Restore the rotation and scale of the root game object
        Selection.transforms [0].transform.rotation = savedRotationRoot;
        Selection.transforms [0].transform.localScale = savedScaleRoot;
        Debug.Log("Exported RBS " + fileName);
    }
    
    // 
    // SerializeRigidBody is responsible for serializing the shape, orienation and offset of
    // the collider to .rbs file. The tricky part is that the future reference frame (when imported)
    // of the container bone will be co-located at the origin of the current reference frame (when exported)
    // of the container bone but with its axis directions aligned with the world frame. For that reason
    // we need to record the size and orientation of the collider in world frame and the position of the
    // collider in the container bone's reference frame.
    //
    private static void SerializeRigidBody(StreamWriter sw, IndentModifer im, tntLink link)
    {
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_ARB));
        im.Indent();
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_NAME) + " " + link.name);
        MeshRenderer[] staticMeshes = link.GetComponentsInChildren<MeshRenderer>();
        HashSet<string> meshRelativePathes = new HashSet<string>();
        foreach (MeshRenderer mr in staticMeshes) // order doesn't matter so use foreach
        {
            if (mr.enabled)
            {
                MeshFilter mf = mr.gameObject.GetComponentInChildren<MeshFilter>();
                string relativePath = AssetDatabase.GetAssetPath(mf.sharedMesh.GetInstanceID());
                meshRelativePathes.Add(relativePath);
            }
        }
        foreach(string path in meshRelativePathes)
        {
            if(path.Contains(Pipeline.ArticulatedBodyImporter.PhysicsResourceRoot()))
            {
                // FIXME: get rid of Assets/Resources, should be better way to get relative path
                string relativePath = path.Substring(Pipeline.ArticulatedBodyImporter.PhysicsResourceRoot().Length);
                sw.WriteLine (im.GetIndent () + RBTagParser.GenerateTag (RB_TYPE.RB_MESH_NAME) + " .." + relativePath);
            }
        }
        tntRigidBody rb = link.GetComponentInChildren<tntRigidBody>();
        if (rb != null) // read mass from rigid body instead of from joint 
        {
            // read mass from tntRigidBody since we use it as a mass holder
            sw.WriteLine (im.GetIndent () + RBTagParser.GenerateTag (RB_TYPE.RB_MASS) + " " + rb.m_mass.ToString (floatSerializeFormat));
        }
        else
        {
            sw.WriteLine (im.GetIndent () + RBTagParser.GenerateTag (RB_TYPE.RB_MASS) + " 0.0");
        }
        sw.WriteLine (im.GetIndent () + RBTagParser.GenerateTag (RB_TYPE.RB_MOI) + " " + link.m_moi.x.ToString (floatSerializeFormat) + " " + link.m_moi.y.ToString (floatSerializeFormat) + " " + link.m_moi.z.ToString (floatSerializeFormat));

		Collider collider = link.GetComponentInChildren<Collider>();

        // Since during the import link transform will be normalized to have neither rotation nor scaling
        // we can't simply use Transform.InverseTransformPoint() here to transform the collider center to the container
        // link's frame because the Unity API considers the scaler and will output an offset assuming the scaler
        // in the container's transform is in action.
        // Just do it in the old school way disregarding scaler.
        Vector3 center = collider.transform.position - link.transform.position;
        Vector3 axisXInWorld = collider.transform.TransformVector (new Vector3 (1, 0, 0));
        Vector3 axisYInWorld = collider.transform.TransformVector (new Vector3 (0, 1, 0));
        Vector3 axisZInWorld = collider.transform.TransformVector (new Vector3 (0, 0, 1));
        Vector3 scale = new Vector3 (axisXInWorld.magnitude, axisYInWorld.magnitude, axisZInWorld.magnitude);

        if (collider as CapsuleCollider != null)
        {
            CapsuleCollider c = (CapsuleCollider)collider;

            Vector3 rotationAxisLocalSpace = Vector3.zero;
            Vector3 radialAxis1LocalSpace = Vector3.zero;
            Vector3 radialAxis2LocalSpace = Vector3.zero; // These two are use to calculate the scaling on radius
            switch (c.direction)
            {
                case 0: //Capsule height is along the x-axis
                    rotationAxisLocalSpace = new Vector3(1.0f, 0.0f, 0.0f);
                    radialAxis1LocalSpace = new Vector3(0.0f, 1.0f, 0.0f);
                    radialAxis2LocalSpace = new Vector3(0.0f, 0.0f, 1.0f);
                    break;
                case 1: //Capsule height is along the y-axis
                    rotationAxisLocalSpace = new Vector3(0.0f, 1.0f, 0.0f);
                    radialAxis1LocalSpace = new Vector3(1.0f, 0.0f, 0.0f);
                    radialAxis2LocalSpace = new Vector3(0.0f, 0.0f, 1.0f);
                    break;
                case 2: //Capsule height is along the z-axis
                    rotationAxisLocalSpace = new Vector3(0.0f, 0.0f, 1.0f);
                    radialAxis1LocalSpace = new Vector3(1.0f, 0.0f, 0.0f);
                    radialAxis2LocalSpace = new Vector3(0.0f, 1.0f, 0.0f);
                    break;
            }
            Vector3 radialAxis1 = c.transform.TransformVector(radialAxis1LocalSpace);
            Vector3 radialAxis2 = c.transform.TransformVector(radialAxis2LocalSpace);
            Vector3 rotationAxis = c.transform.TransformVector(rotationAxisLocalSpace);
            float radius = c.radius * Mathf.Max(radialAxis1.magnitude, radialAxis2.magnitude);
            float height = c.height * rotationAxis.magnitude;
            Vector3 p1 = center;
            Vector3 p2 = center;
            if (height > 2 * radius) // check if this is still a capsule, otherwise degenerate to a sphere so p1 = p2 = center
            {
                float ab = height - 2.0f * radius;
                Vector3 pVector = rotationAxis.normalized * ab * 0.5f;
                p1 = center - pVector;
                p2 = center + pVector;
            }
            sw.Write(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_CAPSULE));
            sw.Write(" " + p1.x.ToString(floatSerializeFormat) + " " + p1.y.ToString(floatSerializeFormat) + " " + p1.z.ToString(floatSerializeFormat));
            sw.Write(" " + p2.x.ToString(floatSerializeFormat) + " " + p2.y.ToString(floatSerializeFormat) + " " + p2.z.ToString(floatSerializeFormat));
            sw.WriteLine(" " + radius.ToString(floatSerializeFormat));
        } 
        else if (collider as SphereCollider != null)
        {
            SphereCollider c = (SphereCollider)collider;

            float radius = c.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
            sw.Write(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_SPHERE));
            sw.Write(" " + center.x.ToString(floatSerializeFormat) + " " + center.y.ToString(floatSerializeFormat) + " " + center.z.ToString(floatSerializeFormat));
            sw.WriteLine(" " + radius.ToString(floatSerializeFormat));
        }
        else if (collider as BoxCollider != null)
        {
            BoxCollider c = (BoxCollider)collider;
			Vector3 orientedExtent = axisXInWorld * c.size.x + axisYInWorld * c.size.y + axisZInWorld * c.size.z;

            Vector3 p1 = center + orientedExtent * 0.5f;
            Vector3 p2 = center - orientedExtent * 0.5f;
            Quaternion q = c.transform.rotation;
            sw.Write(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_BOX));
            sw.Write(" " + p1.x.ToString(floatSerializeFormat) + " " + p1.y.ToString(floatSerializeFormat) + " " + p1.z.ToString(floatSerializeFormat));
            sw.Write(" " + p2.x.ToString(floatSerializeFormat) + " " + p2.y.ToString(floatSerializeFormat) + " " + p2.z.ToString(floatSerializeFormat));
            sw.WriteLine(" " + q.x + " " + q.y + " " + q.z + " " + q.w); // rotation of the box, local frame's origin need to be at parent's origin
        }
        // colour is not stored by importer, ignored.
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_COLOUR) + " " + "1.0 1.0 1.0 1.0");
        // TBD: frictionCoefficient is not stored by importer, ignored. "collider.material" doesn't seem to work
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_FRICTION_COEFF) + " " + "0.5");
        // restitutionCoefficient is not stored by importer, ignored.
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_RESTITUTION_COEFF) + " " + "0.5");
        if (link as tntBase != null) // root
        {
            Vector3 pos = link.transform.localPosition;
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_POSITION) + " " + pos.x + " " + pos.y + " " + pos.z);
        }

        if (link.m_mark != null && link.m_mark.Length > 0)
        {
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_MARK) + " " + link.m_mark);
        }

        im.Raise();
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_END_RB) + "\n");
    }

    // 
    // SerializeRigidBody is responsible for serializing the relative position between parent link and child link.
    // The tricky part is that the future reference frame (when imported) of the links will be co-located at the origins
    // of the current reference frames (when exported) of the links but with their axis directions aligned with the
    // world frame. For that reason, we need transform both pivotA (in parent frame) and pivot B (in child frame)
    // and axisA(in parent frame) and axisB(in child frame)
    //
    private static void SerializeJoint(StreamWriter sw, IndentModifer im, tntLink link)
    {
        if (link as tntBase != null)
        {
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_ROOT) + " " + link.name + "\n");
            return;
        }

        RB_TYPE jointType = RB_TYPE.RB_JOINT_TYPE_UNIVERSAL;
        string jointTypeSubString = null;
        string jointParentSubString = null;
        string jointPposSubString = null;
        string jointCposSubString = null;
        string jointLimitsSubString = null;
        string jointUseLimitsSubString = null; // per DOF use limits options
        string jointUseMotorSubString = null; // per DOF use motor options

        // APE tntLink API doesn't consider the "scale" factor of the parent and child link's transforms. Thus
        // whatever "scale" factors in the parent and child's link transforms are already baked into pivotA and pivotB
        // in order for APE to take them. For that reason we can't simply use Transform.TransformPoint() to transform
        // the joint pivot into world frame because the Unity API performs scaling.
        //
        tntChildLink childLink = link as tntChildLink;
        Vector3 pivotB = childLink.transform.rotation * childLink.PivotB;
		
		tntLink parentLink = childLink.m_parent;
		Vector3 pivotA = parentLink.transform.rotation * childLink.PivotA;

        if (link as tntUniversalLink != null)
        {
            tntUniversalLink ulink = (tntUniversalLink)link;
            jointType = RB_TYPE.RB_JOINT_TYPE_UNIVERSAL;
			Vector3 axisA = parentLink.transform.rotation * ulink.m_axisA;
			Vector3 axisB = childLink.transform.rotation * ulink.m_axisB;

            jointTypeSubString = axisA.x + " " + axisA.y + " " + axisA.z + " ";
            jointTypeSubString = jointTypeSubString + axisB.x + " " + axisB.y + " " + axisB.z;
            jointParentSubString = ulink.m_parent.name;

            jointPposSubString = pivotA.x.ToString(floatSerializeFormat) + " " + pivotA.y.ToString(floatSerializeFormat) + " " + pivotA.z.ToString(floatSerializeFormat);
            jointCposSubString = pivotB.x.ToString(floatSerializeFormat) + " " + pivotB.y.ToString(floatSerializeFormat) + " " + pivotB.z.ToString(floatSerializeFormat);
            float minAngleA = ulink.m_dofData[1].m_limitLow * Mathf.Deg2Rad;
            float maxAngleA = ulink.m_dofData[1].m_limitHigh * Mathf.Deg2Rad;
            float minAngleB = ulink.m_dofData[0].m_limitLow * Mathf.Deg2Rad;
            float maxAngleB = ulink.m_dofData[0].m_limitHigh * Mathf.Deg2Rad;
            jointLimitsSubString = minAngleA.ToString(floatSerializeFormat) + " " + maxAngleA.ToString(floatSerializeFormat) + " " + minAngleB.ToString(floatSerializeFormat) + " " + maxAngleB.ToString(floatSerializeFormat);
            jointUseLimitsSubString = ulink.m_dofData[1].m_useLimit.ToString() + " " + ulink.m_dofData[0].m_useLimit.ToString();
            jointUseMotorSubString = ulink.m_dofData[1].m_useMotor.ToString() + " " + ulink.m_dofData[0].m_useMotor.ToString();
        }
        else if (link as tntBallLink != null)
        {
            tntBallLink blink = (tntBallLink)link;
            jointType = RB_TYPE.RB_JOINT_TYPE_BALL_IN_SOCKET;
            // look at BallInSocketJoint.readAxes(), swingAxis1, swingAxis2, and twistAxis are never being referenced so the string below doesn't matter
            jointTypeSubString = "1 0 0 0 1 0";
            jointParentSubString = blink.m_parent.name;
            jointPposSubString = pivotA.x.ToString(floatSerializeFormat) + " " + pivotA.y.ToString(floatSerializeFormat) + " " + pivotA.z.ToString(floatSerializeFormat);
            jointCposSubString = pivotB.x.ToString(floatSerializeFormat) + " " + pivotB.y.ToString(floatSerializeFormat) + " " + pivotB.z.ToString(floatSerializeFormat);
            float minSwingAngle1 = blink.m_dofData[1].m_limitLow * Mathf.Deg2Rad;
            float maxSwingAngle1 = blink.m_dofData[1].m_limitHigh * Mathf.Deg2Rad;
            float minSwingAngle2 = blink.m_dofData[2].m_limitLow * Mathf.Deg2Rad;
            float maxSwingAngle2 = blink.m_dofData[2].m_limitHigh * Mathf.Deg2Rad;
            float minTwistAngle = blink.m_dofData[0].m_limitLow * Mathf.Deg2Rad;
            float maxTwistAngle = blink.m_dofData[0].m_limitHigh * Mathf.Deg2Rad;
            jointLimitsSubString = minSwingAngle1.ToString(floatSerializeFormat) + " " + maxSwingAngle1.ToString(floatSerializeFormat) + " " + minSwingAngle2.ToString(floatSerializeFormat) + " " + maxSwingAngle2.ToString(floatSerializeFormat) + " " + minTwistAngle.ToString(floatSerializeFormat) + " " + maxTwistAngle.ToString(floatSerializeFormat);
            jointUseLimitsSubString = blink.m_dofData[1].m_useLimit.ToString() + " " + blink.m_dofData[2].m_useLimit.ToString() + " " + blink.m_dofData[0].m_useLimit.ToString();
            jointUseMotorSubString = blink.m_dofData[1].m_useMotor.ToString() + " " + blink.m_dofData[2].m_useMotor.ToString() + " " + blink.m_dofData[0].m_useMotor.ToString();
        }
        else if (link as tntHingeLink != null)
        {
            tntHingeLink hlink = (tntHingeLink)link;
            jointType = RB_TYPE.RB_JOINT_TYPE_HINGE;
            Vector3 axisA = parentLink.transform.rotation * hlink.m_axisA;
            jointTypeSubString = axisA.x + " " + axisA.y + " " + axisA.z;
            jointParentSubString = hlink.m_parent.name;
            jointPposSubString = pivotA.x.ToString(floatSerializeFormat) + " " + pivotA.y.ToString(floatSerializeFormat) + " " + pivotA.z.ToString(floatSerializeFormat);
            jointCposSubString = pivotB.x.ToString(floatSerializeFormat) + " " + pivotB.y.ToString(floatSerializeFormat) + " " + pivotB.z.ToString(floatSerializeFormat);
            float minAngle = hlink.m_dofData[0].m_limitLow * Mathf.Deg2Rad;
            float maxAngle = hlink.m_dofData[0].m_limitHigh * Mathf.Deg2Rad;
            jointLimitsSubString = minAngle.ToString(floatSerializeFormat) + " " + maxAngle.ToString(floatSerializeFormat);
            jointUseLimitsSubString = hlink.m_dofData[0].m_useLimit.ToString();
            jointUseMotorSubString = hlink.m_dofData[0].m_useMotor.ToString();
        }
        else if (link as tntFixedLink != null)
        {
            tntFixedLink flink = (tntFixedLink)link;
            jointType = RB_TYPE.RB_JOINT_TYPE_FIXED;
            jointParentSubString = flink.m_parent.name;
			Vector3 deltaCOM = childLink.transform.position - parentLink.transform.position;
			pivotA = deltaCOM * 0.5f;
			pivotB = -deltaCOM * 0.5f;
			jointPposSubString = pivotA.x.ToString(floatSerializeFormat) + " " + pivotA.y.ToString(floatSerializeFormat) + " " + pivotA.z.ToString(floatSerializeFormat);
			jointCposSubString = pivotB.x.ToString(floatSerializeFormat) + " " + pivotB.y.ToString(floatSerializeFormat) + " " + pivotB.z.ToString(floatSerializeFormat);
        }

        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(jointType) + " " + jointTypeSubString);
        im.Indent();
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_NAME) + " " + link.name);
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_PARENT) + " " + jointParentSubString);
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_CHILD) + " " + link.name);
        if (jointPposSubString != null)
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_PPOS) + " " + jointPposSubString);
        if (jointCposSubString != null)
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_CPOS) + " " + jointCposSubString);
        if (jointLimitsSubString != null)
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_JOINT_LIMITS) + " " + jointLimitsSubString);
        if (jointUseLimitsSubString != null)
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_JOINT_USE_LIMITS) + " " + jointUseLimitsSubString);
        if (jointUseMotorSubString != null)
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_JOINT_USE_MOTOR) + " " + jointUseMotorSubString);
        if(link.m_material) // output physics material
        {
            string relativePath = AssetDatabase.GetAssetPath(link.m_material.GetInstanceID());
            // FIXME: get rid of Assets/Resources, should be better way to get relative path
            relativePath = relativePath.Substring(Pipeline.ArticulatedBodyImporter.PhysicsResourceRoot().Length);
            sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_PHYSICSMATERIAL) + " .." + relativePath);
        }
        im.Raise();
        sw.WriteLine(im.GetIndent() + RBTagParser.GenerateTag(RB_TYPE.RB_END_JOINT));
        sw.Write("\n");
    }
}