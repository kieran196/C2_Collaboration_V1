using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Pipeline
{
	public class BipedControllerImporter : AssetPostprocessor
	{
		public BipedControllerImporter()
		{
		}

		/// <summary>
		/// Handles when ANY asset is imported, deleted, or moved.  Each parameter is the full path of the asset, including filename and extension.
		/// </summary>
		/// <param name="importedAssets">The array of assets that were imported.</param>
		/// <param name="deletedAssets">The array of assets that were deleted.</param>
		/// <param name="movedAssets">The array of assets that were moved.  These are the new file paths.</param>
		/// <param name="movedFromPath">The array of assets that were moved.  These are the old file paths.</param>
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
		{
			foreach(string asset in importedAssets)
			{
				//Debug.Log("Imported: " + asset);
				var ext = (asset.Substring(asset.LastIndexOf(".") + 1));
				if(ext.ToLower()=="sbc")
				{
					ImportSBC(asset);
				}
			}
		}

        private static void ReadGains(StreamReader sr, tntBipedController controller)
        {
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                string[] stringValues = line.Split(chars);
                CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);    

                switch (conType)
                {
                    case CON_TYPE.CON_PD_GAINS_END:
                        return;
                    case CON_TYPE.CON_PD_GAINS_START:
                        break;
                    case CON_TYPE.CON_COMMENT:
                        break;
                    default:
                        PDParams controlParams = new PDParams();
                        controlParams.name = stringValues[0];
                        controlParams.kp = float.Parse(stringValues[1]);
                        controlParams.kd = float.Parse(stringValues[2]);
                        controlParams.maxAbsTorque = float.Parse(stringValues[3]);
                        controlParams.scale =
                            new Vector3(
                                float.Parse(stringValues[4]),
                                float.Parse(stringValues[5]),
                                float.Parse(stringValues[6])
                            );
                        if (stringValues[0] == "root")
                            controller.rootControlParams = controlParams;
                        else
                            controller.controlParams.Add(controlParams);
                        break;
                }
            }
        }

        private static void ReadFeedback(StreamReader sr, LinearBalanceFeedback fb)
        {
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                string[] stringValues = line.Split(chars);
                CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);
                
                switch (conType) 
                {
                    case CON_TYPE.CON_FEEDBACK_END:
                        //we're done...
                        return;
                    case CON_TYPE.CON_COMMENT:
                        break;
                    case CON_TYPE.CON_CV:
                        if (stringValues.Length < 2)
                            Debug.LogError("A cv value must be specified!");
                        else
                            fb.cv = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_CD:
                        if (stringValues.Length < 2)
                            Debug.LogError("A cd value must be specified!");
                        else
                            fb.cd = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_D_MIN:
                        if (stringValues.Length < 2)
                            Debug.LogError("A dMin value must be specified!");
                        else
                            fb.dMin = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_D_MAX:
                        if (stringValues.Length < 2)
                            Debug.LogError("A dMax value must be specified!");
                        else
                            fb.dMax = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_V_MIN:
                        if (stringValues.Length < 2)
                            Debug.LogError("A vMin value must be specified!");
                        else
                            fb.vMin = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_V_MAX:
                        if (stringValues.Length < 2)
                            Debug.LogError("A vMax value must be specified!");
                        else
                            fb.vMax = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_FEEDBACK_PROJECTION_AXIS:
                        if (stringValues.Length < 4)
                            Debug.LogError("The axis for a trajectory is specified by three parameters!");
                        else
                            fb.feedbackProjectionAxis =
                                new Vector3(
                                    float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip(),
                                    float.Parse(stringValues[2]),
                                    float.Parse(stringValues[3])
                                    );
                        break;
                    case CON_TYPE.CON_NOT_IMPORTANT:
                        Debug.LogError("Ignoring input line: " + line);
                        break;
                    default:
                        Debug.LogError("Incorrect BipedController input file: " + line);
                        break;
                }
            }
        }

        /**
            This method is used to read the knots of a strength trajectory from the file, where
            they are specified one (knot) on a line
        */
        private static void ReadTrajectory1D(StreamReader sr, Trajectory1D traj,
                                             CON_TYPE endingLineType)
        {
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                string[] stringValues = line.Split(chars);
                CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);
                
                if( conType == endingLineType )
                    //we're done...
                    return;
                
                switch (conType)
                {
                    case CON_TYPE.CON_COMMENT:
                        break;
                    case CON_TYPE.CON_NOT_IMPORTANT:
                        //we expect pairs of numbers, one pair on each row, so see if we have a valid pair
                        
                        if (stringValues.Length == 2)
                        {
                            traj.addKnot(float.Parse(stringValues[0]), float.Parse(stringValues[1]));
                        } else
                            Debug.LogError("Ignoring input line: " + line);
                        break;
                    default:
                        Debug.LogError("Incorrect BipedController input file: " + line);
                        break;
                }
            }
            Debug.LogError("Incorrect BipedController input file: Trajectory not closed ");
        }

        
        /**
            This method is used to read a trajectory from a file
        */
        private static void ReadTrajectoryComponent(StreamReader sr, TrajectoryComponent comp)
        {
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                string[] stringValues = line.Split(chars);
                CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);
                
                switch (conType) 
                {           
                    case CON_TYPE.CON_TRAJ_COMPONENT_END:
                        //we're done...
                        return;
                    case CON_TYPE.CON_COMMENT:
                        break;
                    case CON_TYPE.CON_ROTATION_AXIS:
                        comp.rotationAxis = new Vector3(
                            float.Parse(stringValues[1]) * ArticulatedBodyImporter.HandnessFlip(),
                            float.Parse(stringValues[2]),
                            float.Parse(stringValues[3]));
                        comp.rotationAxis.Normalize();
                        break;
                    case CON_TYPE.CON_FEEDBACK_START:
                        //read the kind of feedback that is applicable to this state
                        if (stringValues.Length != 2)
                            Debug.LogError("The kind of feedback to be used for a trajectory must be specified (e.g. linear)");
                        
                        if (stringValues[1] == "linear")
                        {
                            comp.balanceFeedback = new LinearBalanceFeedback();
                            ReadFeedback(sr, comp.balanceFeedback);
                            comp.feedbackEnabled = true;
                        } else
                            Debug.LogError("Unrecognized type of feedback: " + line);
                        break;
                    case CON_TYPE.CON_BASE_TRAJECTORY_START:
                        //read in the base trajectory
                        ReadTrajectory1D(sr, comp.baseTraj, CON_TYPE.CON_BASE_TRAJECTORY_END); 
                        break;
                    case CON_TYPE.CON_REVERSE_ANGLE_ON_STANCE:
                        if (stringValues[1] == "left")
                            comp.reverseAngleOnLeftStance = true;
                        else if (stringValues[1] == "right")
                            comp.reverseAngleOnRightStance = true;
                        else 
                            Debug.LogError("When using the \'startingStance\' keyword, \'left\' or \'right\' must be specified!");
                        break;
                    case CON_TYPE.CON_NOT_IMPORTANT:
                        Debug.Log("Ignoring input line: " + line); 
                        break;
                    default:
                        Debug.LogError("Incorrect Biped Controller input file: " + line);
                        break;
                }
            }
            Debug.LogError("Incorrect Biped Controller input file: No \'/trajectory\' found ");
        }

        /**
            This method is used to read a trajectory from a file
        */
        private static void ReadTrajectory(StreamReader sr, Trajectory traj){
            TrajectoryComponent newComponent;
            
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                string[] stringValues = line.Split(chars);
                CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);
                
                switch (conType) 
                {           
                    case CON_TYPE.CON_STRENGTH_TRAJECTORY_START:
                        //read in the base trajectory
                        if (traj.strengthTraj != null)
                            Debug.LogError( "Two strength trajectory, this is illegal!" );
                        traj.strengthTraj = new Trajectory1D();
                        ReadTrajectory1D(sr, traj.strengthTraj, CON_TYPE.CON_STRENGTH_TRAJECTORY_END);
                        break;
                    case CON_TYPE.CON_TRAJECTORY_END:
                        //we're done...
                        return;
                    case CON_TYPE.CON_CHAR_FRAME_RELATIVE:
                        traj.relToCharFrame = true;
                        break;
                    case CON_TYPE.CON_COMMENT:
                        break;
                    case CON_TYPE.CON_TRAJ_COMPONENT:
                        //read in the base trajectory
                        newComponent = new TrajectoryComponent();
                        ReadTrajectoryComponent(sr, newComponent);
                        traj.components.Add(newComponent);
                        break;
                    case CON_TYPE.CON_NOT_IMPORTANT:
                        break;
                    default:
                        Debug.LogError("Incorrect Biped Controller input file:" + line);
                        break;
                }
            }
            Debug.LogError("Incorrect Biped Controller input file: No \'/trajectory\' found ");
        }
		/**
            This method is used to read the external forces on a joint from a file
        */
		private static void ReadExternalForce(StreamReader sr, ExternalForce force)
		{
			for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
			{
				char[] chars = new char[] {' ', '\t'};
				line = line.TrimStart(chars);
				string[] stringValues = line.Split(chars);
				CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);
				
				switch (conType) 
				{           
					case CON_TYPE.CON_FORCE_X_START:
						if (force.forceX != null)
							Debug.LogError( "Two forceX on one joint is illegal!" );
						force.forceX = new Trajectory1D();
						ReadTrajectory1D(sr, force.forceX, CON_TYPE.CON_FORCE_X_END);
						break;
					case CON_TYPE.CON_FORCE_Y_START:
						if (force.forceY != null)
							Debug.LogError( "Two forceY on one joint is illegal!" );
						force.forceY = new Trajectory1D();
						ReadTrajectory1D(sr, force.forceY, CON_TYPE.CON_FORCE_Y_END);
						break;
					case CON_TYPE.CON_FORCE_Z_START:
						if (force.forceZ != null)
							Debug.LogError( "Two forceZ on one joint is illegal!" );
						force.forceZ = new Trajectory1D();
						ReadTrajectory1D(sr, force.forceZ, CON_TYPE.CON_FORCE_Z_END);
						break;
					case CON_TYPE.CON_TORQUE_X_START:
						if (force.torqueX != null)
							Debug.LogError( "Two torqueX on one joint is illegal!" );
						force.torqueX = new Trajectory1D();
						ReadTrajectory1D(sr, force.torqueX, CON_TYPE.CON_TORQUE_X_END);
						break;
					case CON_TYPE.CON_TORQUE_Y_START:
						if (force.torqueY != null)
							Debug.LogError( "Two torqueY on one joint is illegal!" );
						force.torqueY = new Trajectory1D();
						ReadTrajectory1D(sr, force.torqueY, CON_TYPE.CON_TORQUE_Y_END);
						break;
					case CON_TYPE.CON_TORQUE_Z_START:
						if (force.torqueZ != null)
							Debug.LogError( "Two torqueZ on one joint is illegal!" );
						force.torqueZ = new Trajectory1D();
						ReadTrajectory1D(sr, force.torqueZ, CON_TYPE.CON_TORQUE_Z_END);
						break;
					case CON_TYPE.CON_EXTERNAL_FORCE_END:
						//we're done...
						return;
					case CON_TYPE.CON_COMMENT:
						break;
					case CON_TYPE.CON_NOT_IMPORTANT:
						break;
					default:
						Debug.LogError("Incorrect Biped Controller input file:" + line);
						break;
				}
			}
			Debug.LogError("Incorrect Biped Controller input file: No \'/ExternalForce\' found ");
		}

        private static void ReadState(StreamReader sr, BipedConState state)
        {
            for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                string[] stringValues = line.Split(chars);
                CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);
                
                switch (conType) 
                {           
                    case CON_TYPE.CON_STATE_END:
                        //we're done...
                        return;
                    case CON_TYPE.CON_NEXT_STATE:
                        if (stringValues.Length < 2)
                            Debug.LogError("An index must be specified when using the \'nextState\' keyword");
                        else
                            state.nextStateIndex = int.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_STATE_DESCRIPTION:
                        state.description = line.Substring(stringValues[0].Length + 1);
                        break;
                    case CON_TYPE.CON_STATE_TIME:
                        if (stringValues.Length < 2)
                            Debug.LogError("The time that is expected to be spent in this state needs to be provided.");
                        else
                            state.stateTime = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_MAX_GYRO:
                        if (stringValues.Length < 2)
                            Debug.LogError("The maximum gyro allowed by this state needs to be provided.");
                        else
                            state.maxGyro = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_STATE_STANCE:
                        state.reverseStance = false;
                        state.keepStance = false;
                        if (stringValues[1] == "left")
                            state.stateStance = StanceOrientation.LEFT_STANCE;
                        else if (stringValues[1] == "right")
                            state.stateStance = StanceOrientation.RIGHT_STANCE;
                        else if (stringValues[1] == "reverse")
                            state.reverseStance = true;
                        else if (stringValues[1] == "same")
                            state.keepStance = true;
                        else
                            Debug.LogError("When using the \'stateStance\' keyword, \'left\', \'right\' or \'reverse\' must be specified.");
                        break;
                    case CON_TYPE.CON_TRANSITION_ON:
                        if (stringValues[1] == "footDown")
                            state.transitionOnFootContact = true;
                        else if (stringValues[1] == "timeUp")
                            state.transitionOnFootContact = false;
                        else
                            Debug.LogError("When using the \'transitionOn\' keyword, \'footDown\' or \'timeUp\' must be specified.");
                        break;
                    case CON_TYPE.CON_TRAJECTORY_START:
                        //create a new trajectory, and read its information from the file
                        Trajectory tempTraj = new Trajectory();
                        tempTraj.jName = stringValues[1];
                        ReadTrajectory(sr, tempTraj);
                        state.sTraj.Add(tempTraj);
                        break;
					case CON_TYPE.CON_EXTERNAL_FORCE_START:
						ExternalForce tempForce = new ExternalForce();
						tempForce.jName = stringValues[1];
						ReadExternalForce(sr, tempForce);
						state.sExternalForces.Add(tempForce);
						break;
                    case CON_TYPE.CON_D_TRAJX_START:
                        if (state.dTrajX != null)
                            Debug.LogError( "Two dTrajX trajectory, this is illegal!" );
                        state.dTrajX = new Trajectory1D();
                        ReadTrajectory1D(sr, state.dTrajX, CON_TYPE.CON_D_TRAJX_END);
                        break;
                        
                    case CON_TYPE.CON_D_TRAJZ_START:
                        if (state.dTrajZ != null)
                            Debug.LogError( "Two dTrajZ trajectory, this is illegal!" );
                        state.dTrajZ = new Trajectory1D();
                        ReadTrajectory1D(sr, state.dTrajZ, CON_TYPE.CON_D_TRAJZ_END);
                        break;
                        
                    case CON_TYPE.CON_V_TRAJX_START:
                        if (state.vTrajX != null)
                            Debug.LogError( "Two vTrajX trajectory, this is illegal!" );
                        state.vTrajX = new Trajectory1D();
                        ReadTrajectory1D(sr, state.vTrajX, CON_TYPE.CON_V_TRAJX_END);
                        break;
                        
                    case CON_TYPE.CON_V_TRAJZ_START:
                        if (state.vTrajZ != null)
                            Debug.LogError( "Two vTrajZ trajectory, this is illegal!" );
                        state.vTrajZ = new Trajectory1D();
                        ReadTrajectory1D(sr, state.vTrajZ, CON_TYPE.CON_V_TRAJZ_END);
                        break;
                        
                    case CON_TYPE.CON_COMMENT:
                        break;
                        
                    case CON_TYPE.CON_NOT_IMPORTANT:
                        break;        
                        
                    default:
                        Debug.LogError("Incorrect SIMBICON input file:  - unexpected line :" + line);
                        break;
                }
            }
        }

		private static void ImportSBC(string fileName)
		{
			Debug.Log(">>>>>>>>>>>>> Importing SBC File: " + fileName);
			string[] stringValues = fileName.Split('/');
			string shortName = stringValues[stringValues.Length - 1];
			shortName = shortName.Substring(0, shortName.Length - 4);
			GameObject rootObject = new GameObject("BipedCon-"+shortName);
            tntBipedController controller = rootObject.AddComponent<tntBipedController>();

			var sr = new StreamReader(fileName);

			for (var line = sr.ReadLine(); line != null; line = sr.ReadLine())
			{
                char[] chars = new char[] {' ', '\t'};
                line = line.TrimStart(chars);
                stringValues = line.Split(chars);
                CON_TYPE conType = CONTagParser.ParseTag(stringValues[0]);
				// Debug.Log("line: " + line + " =>" + conType);

				switch (conType)
				{
                    case CON_TYPE.CON_IKVM_ON:
                        controller.isIKVMOn = true;
                        break;
                    case CON_TYPE.CON_PD_GAINS_START:
                        ReadGains(sr, controller);
                        break;
                    case CON_TYPE.CON_STATE_START:
                        BipedConState tempState = new BipedConState();
                        controller.states.Add(tempState);
                        ReadState(sr, tempState);
                        // TBD: Defer resolveJoints(tempState) to the engine
                        break;
                    case CON_TYPE.CON_STANCE_HIP_DAMPING:
                        controller.stanceHipDamping = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_STANCE_HIP_MAX_VELOCITY:
                        controller.stanceHipMaxVelocity = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_ROOT_PRED_TORQUE_SCALE:
                        controller.rootPredictiveTorqueScale = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_MAX_GYRO:
                        controller.maxGyro = float.Parse(stringValues[1]);
                        break;
                    case CON_TYPE.CON_CHARACTER_STATE:
                        // TBD: Implement .RBS (rigid body state) loading later. Currently character
                        // always initialized as "T" pose
                        //
                        // character->loadReducedStateFromFile(trim(line));
                        // strcpy(initialBipState, trim(line));
                        break;
                    case CON_TYPE.CON_START_AT_STATE:
                        controller.startingState = int.Parse(stringValues[1]);
                        // TBD: Transistion to the start state when the controller is started at the engine side
                        // transitionToState(tempStateNr);
                        break;
                    case CON_TYPE.CON_COMMENT:
                        break;
                    case CON_TYPE.CON_STARTING_STANCE:
                        if (stringValues[1] == "left") {
                            // TBD: Set the initial stance index at the engine side
                            // setStance(LEFT_STANCE);
                            controller.startingStance = StanceOrientation.LEFT_STANCE;
                        }
                        else if (stringValues[1] == "right") {
                            // TBD: Set the initial stance index at the engine side
                            // setStance(RIGHT_STANCE);
                            controller.startingStance = StanceOrientation.RIGHT_STANCE;
                        }
                        else 
                            Debug.LogError("When using the \'reverseTargetOnStance\' keyword, \'left\' or \'right\' must be specified!");
                        break;
                    case CON_TYPE.CON_NOT_IMPORTANT:
                        break;
                    default:
                        Debug.LogError("Incorrect SIMBICON input file: " + line);
						break;
				}
			}
			sr.Close();
		}
	}
}