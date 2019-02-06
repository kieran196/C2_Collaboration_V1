
namespace Pipeline
{
    public enum CON_TYPE
    {
        CON_NOT_IMPORTANT               =   1,
        CON_COMMENT                     =   2,
        CON_PD_GAINS_START              =   3,
        CON_PD_GAINS_END                =   4,
        CON_STATE_START                 =   5,
        CON_STATE_END                   =   6,
        CON_NEXT_STATE                  =   7,
        CON_STATE_DESCRIPTION           =   8,
        CON_TRANSITION_ON               =   9,
        CON_STATE_STANCE                =   10,
        CON_STARTING_STANCE             =   11,
        CON_START_AT_STATE              =   12,
        CON_CHARACTER_STATE             =   13,
        CON_STATE_TIME                  =   14,
        CON_TRAJECTORY_START            =   15,
        CON_TRAJECTORY_END              =   16,
        CON_REVERSE_ANGLE_ON_STANCE     =   17,
        CON_ROTATION_AXIS               =   18,
        CON_BASE_TRAJECTORY_START       =   19,
        CON_BASE_TRAJECTORY_END         =   20,
        CON_FEEDBACK_START              =   21,
        CON_FEEDBACK_END                =   22,
        CON_CD                          =   23,
        CON_CV                          =   24,
        CON_FEEDBACK_PROJECTION_AXIS    =   25,
        LOAD_RB_FILE                    =   26,
        LOAD_CON_FILE                   =   27,
        CON_TRAJ_COMPONENT              =   28,
        CON_TRAJ_COMPONENT_END          =   29,
      
        CON_STRENGTH_TRAJECTORY_START   =   30,
        CON_STRENGTH_TRAJECTORY_END     =   31,
        CON_CHAR_FRAME_RELATIVE         =   32,
        CON_STANCE_HIP_DAMPING          =   33,
        CON_STANCE_HIP_MAX_VELOCITY     =   34,
        
        CON_D_MIN                       =   35,
        CON_D_MAX                       =   36,
        CON_V_MIN                       =   37,
        CON_V_MAX                       =   38,
        
        CON_D_TRAJX_START               =   39,
        CON_D_TRAJX_END                 =   40,
        CON_D_TRAJZ_START               =   41,
        CON_D_TRAJZ_END                 =   42,
        CON_V_TRAJX_START               =   43,
        CON_V_TRAJX_END                 =   44,
        CON_V_TRAJZ_START               =   45,
        CON_V_TRAJZ_END                 =   46,
        CON_ROOT_PRED_TORQUE_SCALE      =   47,
        
        CON_MIN_FEEDBACK                =   48,
        CON_MAX_FEEDBACK                =   49,

        CON_EXTERNAL_FORCE_START        =   50,
        CON_EXTERNAL_FORCE_END          =   51,

		CON_FORCE_X_START				=	52,
		CON_FORCE_X_END					=	53,
		CON_FORCE_Y_START				=	54,
		CON_FORCE_Y_END					=	55,
		CON_FORCE_Z_START				=	56,
		CON_FORCE_Z_END					=	57,
		CON_TORQUE_X_START				=	58,
		CON_TORQUE_X_END				=	59,
		CON_TORQUE_Y_START				=	60,
		CON_TORQUE_Y_END				=	61,
		CON_TORQUE_Z_START				=	62,
		CON_TORQUE_Z_END				=	63,

        CON_IKVM_ON             		=   64,
        CON_MAX_GYRO                    =   65
	}

    public class CONTagParser
    {
        static string[] keywords =
        {
            "#",
            "PDParams",
            "/PDParams",
            "ConState",
            "/ConState",
            "nextState",
            "description",
            "transitionOn",
            "stateStance",
            "startingStance",
            "startAtState",
            "loadCharacterState",
            "time",
            "trajectory",
            "/trajectory",
            "reverseTargetAngleOnStance",
            "rotationAxis",
            "baseTrajectory",
            "/baseTrajectory",
            "feedback",
            "/feedback",
            "cd",
            "cv",
            "feedbackProjectionAxis",
            "loadRBFile",
            "loadController",
            "component",
            "/component",
            "strengthTrajectory",
            "/strengthTrajectory",
            "characterFrameRelative",
            "stanceHipDamping",
            "stanceHipMaxVelocity",
            "dMin",
            "dMax",
            "vMin",
            "vMax",
            "dTrajX",
            "/dTrajX",
            "dTrajZ",
            "/dTrajZ",
            "vTrajX",
            "/vTrajX",
            "vTrajZ",
            "/vTrajZ",
            "rootPredictiveTorqueScale",
            "minFeedback",
            "maxFeedback",
			"externalForce",
			"/externalForce",
			"forceX",
			"/forceX",
			"forceY",
			"/forceY",
			"forceZ",
			"/forceZ",
			"torqueX",
			"/torqueX",
			"torqueY",
			"/torqueY",
			"torqueZ",
			"/torqueZ",
            "IKVM",
            "maxGyro"
		};

        public static CON_TYPE ParseTag (string line)
        {
            for (int i = 0; i < keywords.Length; i++) {
                if (line == keywords [i])
                    return (CON_TYPE)(i + 2);
            }
            
            return CON_TYPE.CON_NOT_IMPORTANT;
        }

        public static string GenerateTag (CON_TYPE type)
        {
            int typeIndex = (int)type;
            if (typeIndex < keywords.Length + 2) {
                return keywords [typeIndex - 2];
            }
            return null;
        }
    }
}

