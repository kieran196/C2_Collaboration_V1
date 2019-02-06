using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
    public class BipedControllerExporter
    {
        public const string floatSerializeFormat = "F6";
        const string fieldDelimiter = "\t";

        public BipedControllerExporter()
        {
        }

        public static void WriteGains(StreamWriter sw, IndentModifer im, tntBipedController controller)
        {
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_PD_GAINS_START));
            im.Indent();
            sw.WriteLine ("#" + im.GetIndent () + "joint name" + fieldDelimiter + "Kp" + fieldDelimiter + "Kd"
                          + fieldDelimiter + "MaxTorque" + fieldDelimiter + "ScaleX" + fieldDelimiter + "ScaleY" + fieldDelimiter + "ScaleZ");
            WriteControlParam(sw, im, controller.rootControlParams);
            foreach (PDParams controlParam in controller.controlParams)
            {
                WriteControlParam(sw, im, controlParam);
            }
            im.Raise();
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_PD_GAINS_END));
            sw.WriteLine ("");
        }

        private static void WriteControlParam(StreamWriter sw, IndentModifer im, PDParams controlParam)
        {
            sw.Write(im.GetIndent () + controlParam.name);
            sw.Write(fieldDelimiter);
            sw.Write(controlParam.kp.ToString(floatSerializeFormat));
            sw.Write(fieldDelimiter);
            sw.Write(controlParam.kd.ToString(floatSerializeFormat));
            sw.Write(fieldDelimiter);
            sw.Write(controlParam.maxAbsTorque.ToString(floatSerializeFormat));
            sw.Write(fieldDelimiter);
            sw.Write(controlParam.scale.x.ToString(floatSerializeFormat));
            sw.Write(fieldDelimiter);
            sw.Write(controlParam.scale.y.ToString(floatSerializeFormat));
            sw.Write(fieldDelimiter);
            sw.Write(controlParam.scale.z.ToString(floatSerializeFormat));
            sw.WriteLine("");
        }

        public static void WriteState(StreamWriter sw, IndentModifer im, BipedConState state)
        {
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STATE_START) + " ");
            im.Indent();

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STATE_DESCRIPTION) + " ");
            sw.WriteLine (state.description);

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_NEXT_STATE) + " ");
            sw.WriteLine (state.nextStateIndex);

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_TRANSITION_ON) + " ");
            if (state.transitionOnFootContact) {
                sw.WriteLine ("footDown");
            }
            else
            {
                sw.WriteLine ("timeUp");
            }

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STATE_STANCE) + " ");
            if (state.keepStance)
            {
                sw.WriteLine ("same");
            }
            else if (state.reverseStance)
            {
                sw.WriteLine ("reverse");
            }
            else if (state.stateStance == StanceOrientation.LEFT_STANCE)
            {
                sw.WriteLine ("left");
            }
            else if (state.stateStance == StanceOrientation.RIGHT_STANCE)
            {
                sw.WriteLine ("right");
            }

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STATE_TIME) + " ");
            sw.WriteLine (state.stateTime.ToString (floatSerializeFormat));
            sw.WriteLine ("");

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_MAX_GYRO) + " ");
            sw.WriteLine (state.maxGyro.ToString (floatSerializeFormat));
            sw.WriteLine ("");

            foreach (Trajectory traj in state.sTraj)
            {
                WriteTrajectory(sw, im, traj);
            }

            foreach (ExternalForce force in state.sExternalForces)
            {
                WriteExternalForce(sw, im, force);
            }

            WriteTrajectory1D (sw, im, state.dTrajX, CON_TYPE.CON_D_TRAJX_START, CON_TYPE.CON_D_TRAJX_END);
            WriteTrajectory1D (sw, im, state.dTrajZ, CON_TYPE.CON_D_TRAJX_START, CON_TYPE.CON_D_TRAJX_END);
            WriteTrajectory1D (sw, im, state.vTrajX, CON_TYPE.CON_V_TRAJX_START, CON_TYPE.CON_V_TRAJX_END);
            WriteTrajectory1D (sw, im, state.vTrajZ, CON_TYPE.CON_V_TRAJZ_START, CON_TYPE.CON_V_TRAJZ_END);

            im.Raise();
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_STATE_END));
            sw.WriteLine ("");
        }

        private static void WriteTrajectory(StreamWriter sw, IndentModifer im, Trajectory traj)
        {
            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_TRAJECTORY_START) + " ");
            sw.WriteLine (traj.jName);
            im.Indent ();

            WriteTrajectory1D (sw, im, traj.strengthTraj, CON_TYPE.CON_STRENGTH_TRAJECTORY_START, CON_TYPE.CON_STRENGTH_TRAJECTORY_END);

            if (traj.relToCharFrame)
            {
                sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_CHAR_FRAME_RELATIVE));
            }

            foreach (TrajectoryComponent trajCompo in traj.components)
            {
                WriteTrajectoryComponent(sw, im, trajCompo);
            }

            im.Raise ();
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_TRAJECTORY_END));
            sw.WriteLine ("");
        }

        private static void WriteTrajectoryComponent(StreamWriter sw, IndentModifer im, TrajectoryComponent trajCompo)
        {
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_TRAJ_COMPONENT));
            im.Indent ();

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_ROTATION_AXIS) + " ");
            sw.WriteLine ((trajCompo.rotationAxis.x / ArticulatedBodyImporter.HandnessFlip()).ToString(floatSerializeFormat) + " " + trajCompo.rotationAxis.y.ToString(floatSerializeFormat)
                          + " " + trajCompo.rotationAxis.z.ToString(floatSerializeFormat));

            if (trajCompo.reverseAngleOnLeftStance)
            {
                sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_REVERSE_ANGLE_ON_STANCE) + " " + "left");
            }
            else if (trajCompo.reverseAngleOnRightStance)
            {
                sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_REVERSE_ANGLE_ON_STANCE) + " " + "right");
            }

            if (trajCompo.feedbackEnabled)
            {
                WriteFeedback(sw, im, trajCompo.balanceFeedback);
            }

            WriteTrajectory1D(sw, im, trajCompo.baseTraj, CON_TYPE.CON_BASE_TRAJECTORY_START, CON_TYPE.CON_BASE_TRAJECTORY_END);

            im.Raise ();
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_TRAJ_COMPONENT_END));
        }

        private static void WriteFeedback(StreamWriter sw, IndentModifer im, LinearBalanceFeedback fb)
        {
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_FEEDBACK_START) + " " + "linear");
            im.Indent ();

            sw.Write(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_FEEDBACK_PROJECTION_AXIS) + " ");
            sw.WriteLine ((fb.feedbackProjectionAxis.x / ArticulatedBodyImporter.HandnessFlip()).ToString (floatSerializeFormat) + " "
                + fb.feedbackProjectionAxis.y.ToString (floatSerializeFormat) + " "
                + fb.feedbackProjectionAxis.z.ToString (floatSerializeFormat));

            sw.Write (im.GetIndent () + CONTagParser.GenerateTag (CON_TYPE.CON_CV) + " ");
            sw.WriteLine (fb.cv.ToString(floatSerializeFormat));

            sw.Write (im.GetIndent () + CONTagParser.GenerateTag (CON_TYPE.CON_CD) + " ");
            sw.WriteLine (fb.cd.ToString(floatSerializeFormat));

            sw.Write (im.GetIndent () + CONTagParser.GenerateTag (CON_TYPE.CON_D_MIN) + " ");
            sw.WriteLine (fb.dMin.ToString(floatSerializeFormat));

            sw.Write (im.GetIndent () + CONTagParser.GenerateTag (CON_TYPE.CON_D_MAX) + " ");
            sw.WriteLine (fb.dMax.ToString(floatSerializeFormat));

            sw.Write (im.GetIndent () + CONTagParser.GenerateTag (CON_TYPE.CON_V_MIN) + " ");
            sw.WriteLine (fb.vMin.ToString(floatSerializeFormat));

            sw.Write (im.GetIndent () + CONTagParser.GenerateTag (CON_TYPE.CON_V_MAX) + " ");
            sw.WriteLine (fb.vMax.ToString(floatSerializeFormat));

            im.Raise ();
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_FEEDBACK_END));
        }

        private static void WriteExternalForce(StreamWriter sw, IndentModifer im, ExternalForce force)
        {
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_EXTERNAL_FORCE_START));
            im.Indent ();

            WriteTrajectory1D (sw, im, force.forceX, CON_TYPE.CON_FORCE_X_START, CON_TYPE.CON_FORCE_X_END);
            WriteTrajectory1D (sw, im, force.forceY, CON_TYPE.CON_FORCE_Y_START, CON_TYPE.CON_FORCE_Y_END);
            WriteTrajectory1D (sw, im, force.forceZ, CON_TYPE.CON_FORCE_Z_START, CON_TYPE.CON_FORCE_Z_END);
            WriteTrajectory1D (sw, im, force.torqueX, CON_TYPE.CON_TORQUE_X_START, CON_TYPE.CON_TORQUE_X_END);
            WriteTrajectory1D (sw, im, force.torqueY, CON_TYPE.CON_TORQUE_Y_START, CON_TYPE.CON_TORQUE_Y_END);
            WriteTrajectory1D (sw, im, force.torqueZ, CON_TYPE.CON_TORQUE_Z_START, CON_TYPE.CON_TORQUE_Z_END);

            im.Raise ();
            sw.WriteLine(im.GetIndent() + CONTagParser.GenerateTag(CON_TYPE.CON_EXTERNAL_FORCE_END));
        }

        private static void WriteTrajectory1D(StreamWriter sw, IndentModifer im, Trajectory1D traj,
                                             CON_TYPE startLineType, CON_TYPE endingLineType)
        {
            if (traj == null)
            {
                return;
            }

            if (traj.tValues.Count > 0)
            {
                sw.WriteLine (im.GetIndent () + CONTagParser.GenerateTag (startLineType));
                im.Indent ();
                for (int i = 0; i < traj.tValues.Count; i++) {
                    sw.WriteLine (im.GetIndent () + traj.tValues [i].ToString (floatSerializeFormat) + " " + traj.values [i].ToString (floatSerializeFormat));
                }
                im.Raise ();
                sw.WriteLine (im.GetIndent () + CONTagParser.GenerateTag (endingLineType));
            }
        }
    }
}