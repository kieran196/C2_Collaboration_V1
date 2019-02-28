namespace Pipeline
{
    public enum RB_TYPE
    {
        RB_NOT_IMPORTANT            =    1,
        RB_RB                       =    2,
        RB_END_RB                   =    3,
        RB_ARB                      =    4,
        RB_MESH_NAME                =    5,
        RB_MASS                     =    6,
        RB_MOI                      =    7,
        RB_COLOUR                   =    8,
        RB_ROOT                     =   9,
        RB_ARTICULATED_FIGURE       =   10,
        RB_CHILD                    =   11,
        RB_PARENT                   =   12,
        RB_END_ARTICULATED_FIGURE   =   13,
        RB_NAME                     =   14,
        RB_JOINT                    =   15,
        RB_END_JOINT                =   16,
        RB_PPOS                     =   17,
        RB_CPOS                     =   18,
        RB_SPHERE                   =   19,
        RB_CAPSULE                  =   20,
        RB_PLANE                    =   21,
        RB_LOCKED                   =   22,
        RB_POSITION                 =   23,
        RB_ORIENTATION              =   24,
        RB_VELOCITY                 =   25,
        RB_ANGULAR_VELOCITY         =   26,
        RB_FRICTION_COEFF           =   27,
        RB_RESTITUTION_COEFF        =   28,
        RB_MIN_BDG_SPHERE           =   29,
        RB_JOINT_TYPE_HINGE         =   30,
        RB_JOINT_LIMITS             =   31,
        RB_JOINT_TYPE_UNIVERSAL     =   32,
        RB_JOINT_TYPE_BALL_IN_SOCKET=   33,
        RB_JOINT_TYPE_FIXED         =   34,
        RB_BOX                      =   35,
        RB_ODE_GROUND_COEFFS        =   36,
        RB_PLANAR                   =   37,
        RB_SOFT_BODY                =   38,
        RB_SKINNED_MESH             =   39, //to parse the skinned mesh reference
        RB_MARK                     =   40, //special mark for biped control to identify lfoot and rfoot, or similar use case
        RB_JOINT_USE_LIMITS         =   41,
        RB_JOINT_USE_MOTOR          =   42,
        RB_PHYSICSMATERIAL          =   43,
        RB_HANDEDNESSFLIP           =   44,
		RB_LEFTRIGHTMESHFLIP        =   45,  //Dirty hack to swap the sectional meshes used for left and right limbs
		RB_FRONTBACKMESHFLIP		=  	46,
		RB_PIVOTATJOINT				=	47
    }

    public class RBTagParser
    {
        static string[] keywords =
        {
            "RigidBody",
            "/End",
            "A_RigidBody",
            "mesh",
            "mass",
            "moi",
            "colour",
            "root",
            "ArticulatedFigure",
            "child",
            "parent",
            "/ArticulatedFigure",
            "name",
            "Joint",
            "/Joint",
            "jointPPos",
            "jointCPos",
            "CDP_Sphere",
            "CDP_Capsule",
            "CDP_Plane",
            "static",
            "position",
            "orientation",
            "velocity",
            "angularVelocity",
            "frictionCoefficient",
            "restitutionCoefficient",
            "minBoundingSphere",
            "hingeJoint",
            "jointLimits",
            "universalJoint",
            "ballInSocketJoint",
            "fixedJoint",
            "CDP_Box",
            "planar",
            "ODEGroundParameters",
            "softBody",
            "skinnedmesh",  //to parse the skinned mesh reference
            "mark",          //special mark for biped control to identify lfoot and rfoot, or similar use case
            "jointUseLimits",
            "jointUseMotor",
            "physicsMaterial",
            "HandednessFlip",
			"LeftRightMeshFlip",
			"FrontBackMeshFlip",
			"PivotAtJoint"
        };

        public static RB_TYPE ParseTag (string line)
        {
            for (int i = 0; i < keywords.Length; i++) {
                if (line.StartsWith (keywords [i]))
                    return (RB_TYPE)(i + 2);
            }
            
            return RB_TYPE.RB_NOT_IMPORTANT;
        }

        public static string GenerateTag (RB_TYPE type)
        {
            int typeIndex = (int)type;
            if (typeIndex < keywords.Length + 2) {
                return keywords [typeIndex - 2];
            }
            return null;
        }
    }
}

