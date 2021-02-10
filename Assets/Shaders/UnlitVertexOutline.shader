Shader "Unlit/Vertex Color Outline" {
	Properties{
	  _Color("Main Color", Color) = (1,1,1,1)
	  _OutlineColor("Outline Color", Color) = (0,1,0,1)
	  _Outline("Outline width", Range(0.01, 1)) = 0.01
	}
		SubShader{
		  Tags { "RenderType" = "Opaque" }
		  Lighting Off Fog { Mode Off }
		  ColorMask RGB

		  Tags { "RenderType" = "Opaque" }
	LOD 100

	Pass {
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(0)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _Color;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = _Color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
		ENDCG
	}
		  Pass {
				// this from https://github.com/ddionisio/MateUnity/blob/master/Shaders/ProBuilder/UnlitVertexColorTextureOutline.shader
			  Name "OUTLINE"
			   Tags { "LightMode" = "Always" }

			   CGPROGRAM
			   #pragma vertex vert
			   #pragma fragment frag_mult
			   #pragma fragmentoption ARB_precision_hint_fastest
			   #include "UnityCG.cginc"

			   struct appdata {
				   float4 vertex : POSITION;
				   float3 normal : NORMAL;
			   };

			   struct v2f {
				  float4 pos : POSITION;
				  float4 color : COLOR;
			   };

			   float _Outline;
			   float4 _OutlineColor;

			   v2f vert(appdata v) {
				  v2f o;
				  o.pos = UnityObjectToClipPos(v.vertex);
				  float3 norm = UnityObjectToViewPos(v.normal);
				  norm.x *= UNITY_MATRIX_P[0][0];
				  norm.y *= UNITY_MATRIX_P[1][1];
				  o.pos.xy += norm.xy * o.pos.z * _Outline;
				  o.pos.z += 0.001;

				  o.color = _OutlineColor;
				  return o;
			   }

			   fixed4 frag_mult(v2f i) : COLOR
			  {
				  return i.color;
			  }
			   ENDCG

			   Cull Front
			   ZWrite On
			   ColorMask RGB
			   Blend SrcAlpha OneMinusSrcAlpha
				  //? -Note: I don't remember why I put a "?" here 
				  SetTexture[_MainTex] { combine primary }
			 }
	  }
		  Fallback "Diffuse"
}