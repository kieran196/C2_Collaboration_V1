
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;
using Pipeline;
using System.Collections.Generic;

public class MidasTouchSBCExporter : ScriptableObject
{
    const string sErrDialogTitle = "RBS Exporter Error";

    public MidasTouchSBCExporter()
    {
        
    }
    
    [MenuItem ("GameObject/Articulated Physics/Export/MidasTouch SBC")]
    static void DoExportSBCEntry()
    {
        DoExportSBC();
    }
    
    private static void DoExportSBC()
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
        
        string sbcName = Selection.transforms[0].name;
        string fileName = EditorUtility.SaveFilePanel("Export .sbc file", "", sbcName, "sbc");

        StringBuilder sbcString = new StringBuilder();
        
        sbcString.Append("#" + sbcName + ".sbc"
            + "\n#" + System.DateTime.Now.ToLongDateString() 
            + "\n#" + System.DateTime.Now.ToLongTimeString()
            + "\n#-------" 
            + "\n\n");

        using (StreamWriter sw = new StreamWriter(fileName))
        {
            IndentModifer im = new IndentModifer();
            sw.WriteLine(sbcString);
            tntBipedController controller = Selection.transforms[0].GetComponent<tntBipedController>();

            BipedControllerExporter.WriteGains(sw, im, controller);

            for (int i = 0; i < controller.states.Count; ++i)
            {
                BipedConState s = controller.states[i];
                BipedControllerExporter.WriteState(sw, im, s);
            }

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STANCE_HIP_DAMPING) + " ");
            sw.WriteLine(controller.stanceHipDamping.ToString(BipedControllerExporter.floatSerializeFormat));

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STANCE_HIP_MAX_VELOCITY) + " ");
            sw.WriteLine(controller.stanceHipMaxVelocity.ToString(BipedControllerExporter.floatSerializeFormat));

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_ROOT_PRED_TORQUE_SCALE) + " ");
            sw.WriteLine(controller.rootPredictiveTorqueScale.ToString(BipedControllerExporter.floatSerializeFormat));

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_MAX_GYRO) + " ");
            sw.WriteLine(controller.maxGyro.ToString(BipedControllerExporter.floatSerializeFormat));

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_START_AT_STATE) + " ");
            sw.WriteLine(controller.startingState);

            if (controller.startingStance == StanceOrientation.LEFT_STANCE)
            {
                sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STARTING_STANCE) + " left");
            }
            else if (controller.startingStance == StanceOrientation.RIGHT_STANCE)
            {
                sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STARTING_STANCE) + " right");
            }
        }
    }
}