// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Grid"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ObjPos ("ObjPos", Vector) = (1,1,1,1)
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		_Radius ("HoleRadius", Range(0.1,5)) = 2
	}
	SubShader
	{
	Tags {"RenderType"="Opaque" "Queue"="Transparent" "RenderType"="TransparentCutout"}
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha 
			Lighting Off
			//ZWrite Off
			LOD 200

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 worldPos : POSITION1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _ObjPos;
			uniform float _Radius;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul (unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				//half3 col = tex2D (_MainTex, i.uv).rgb;
				float dx = length(_ObjPos.x-i.worldPos.x);
				float dy = 0; //length(_ObjPos.y-i.worldPos.y);
				float dz = length(_ObjPos.z-i.worldPos.z);
				float dist = (dx*dx+dy*dy+dz*dz)*_Radius;
				//dist = clamp(dist,0,1);

				dist = clamp(1 - dist,0,1);
				col.a = col.a * dist;

				//col.Albedo = col.rgb; // color is from texture
				//col.a = 1 -  dist > 0;  // alpha is from distance to the mouse

				return col;
			}
			ENDCG
		}
	}
}
