Shader "Unlit/leafs"
{
	Properties{
		_LeafColor ("Leaf Color", COLOR) = (1, 1, 1, 1)
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range (0, 0.1)) = .005
	}
	SubShader
	{
		Tags { 
			"Queue" = "Geometry"
			"RenderType"="Opaque" 
		}
		LOD 100

		Pass
		{
			//Draw Object
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			float4 _LeafColor;
			
			float4 vert (float4 position : POSITION) : SV_POSITION
			{
				return mul(UNITY_MATRIX_MVP, position);
			}
			
			float4 frag (float4 position : SVPOSITION) : SV_TARGET
			{
				return _LeafColor;
			}
			ENDCG
		}
	}
}
