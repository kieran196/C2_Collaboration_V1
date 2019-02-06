Shader "Unlit/JointVisualization"
{
		SubShader
	{
		Tags
		{
			"RenderType" = "TransparentCutout"
			"Queue" = "Transparent"
		}
		LOD 100

		Pass
	{ 
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull off
		ColorMask RGB
		Offset -1, -1

		CGPROGRAM
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"
			
		//globals
		fixed4 _psColor;		
		float _psRadius;

		//uniforms
		float4 _psJoints[10]; //adjacent joints for each joint

		struct appdata
		{
			float4 vertex : POSITION;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float3 worldPos : TEXCOORD0;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = _psColor;

			float maxAlpha = 0;

			for (int j = 0; j < 10; j++)
			{
				float d = length(i.worldPos - _psJoints[j].xyz);
				float a = 1.0 - clamp(d, 0.0, _psRadius) / _psRadius;
				a = a * _psJoints[j].w; //a value to make it seems off
				float cmp = step(maxAlpha, a);
				maxAlpha = lerp(maxAlpha, a, cmp);
			}

			col.a = maxAlpha;
			return col;
		}
		ENDCG
		}
	}
}
