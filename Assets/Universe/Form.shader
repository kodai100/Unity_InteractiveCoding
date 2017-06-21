Shader "Custom/Form" {

	Properties{
		_MainTex("ParticleTexture", 2D) = "white"{}
		_AlphaMap("AlphaMap", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Size("Size", Range(0, 0.01)) = 1
		_Rotation("Rotation", Vector) = (0,0,0)
		_Speed("RotationSpeed", Float) = 1
	}
	SubShader{
		Tags{ "RenderType" = "Transparent" }
		LOD 200

		CGINCLUDE

		#pragma target 5.0
		#include "UnityCG.cginc"

		#define Inv180 0.005555555555
		#define PI 3.14159265358979

		float _Size;
		sampler2D _MainTex;
		sampler2D _AlphaMap;
		fixed4 _Color;
		float3 _Rotation;
		float _Speed;

		struct appdata {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2g {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct g2f {
			float4 pos : SV_POSITION;
			float2 particle_uv : TEXCOORD0;
			float2 map_uv : TEXCOORD1;
		};

		float4x4 RotationMatrix() {
			float x = (_Time.x * _Speed)%360 * Inv180 * PI;
			float y = _Rotation.y * Inv180 * PI;
			float z = _Rotation.z * Inv180 * PI;
			float4x4 tmp;
			tmp._m00 = -sin(z)*sin(x) + cos(y)*cos(z)*cos(x);
			tmp._m01 = -sin(y)*cos(z);
			tmp._m02 = sin(z)*cos(x) + cos(y)*cos(z)*sin(x);
			tmp._m03 = 0;
			tmp._m10 = sin(y)*cos(x);
			tmp._m11 = cos(y);
			tmp._m12 = sin(y)*sin(x);
			tmp._m13 = 0;
			tmp._m20 = -cos(z)*sin(x) - cos(y)*sin(z)*cos(x);
			tmp._m21 = sin(y)*sin(z);
			tmp._m22 = cos(z)*cos(x) - cos(y)*sin(z)*sin(x);
			tmp._m23 = 0;
			tmp._m30 = 0;
			tmp._m31 = 0;
			tmp._m32 = 0;
			tmp._m33 = 1;
			return tmp;
		}

		v2g vert(appdata IN) {
			v2g OUT;

			OUT.vertex = mul(RotationMatrix(), IN.vertex);
			OUT.uv = IN.uv;

			return OUT;
		}

		[maxvertexcount(4)]
		void geom_square(point v2g IN[1], inout TriangleStream<g2f> OUT) {

			float size = _Size;
			float halfS = 0.5f * size;

			g2f pIn;

			for (int x = 0; x < 2; x++) {
				for (int y = 0; y < 2; y++) {
					float4x4 billboardMatrix = UNITY_MATRIX_V;
					billboardMatrix._m03 = billboardMatrix._m13 = billboardMatrix._m23 = billboardMatrix._m33 = 0;

					float2 uv = float2(x, y);

					pIn.pos = IN[0].vertex + mul(float4((uv * 2 - float2(1, 1)) * halfS, 0, 1), billboardMatrix);

					pIn.pos = mul(UNITY_MATRIX_VP, pIn.pos);

					pIn.particle_uv = uv;
					pIn.map_uv = IN[0].uv;

					OUT.Append(pIn);
				}
			}

		}

		ENDCG

		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On

			CGPROGRAM

			#pragma vertex vert
			#pragma geometry geom_square
			#pragma fragment frag

			fixed4 frag(g2f IN) : SV_Target{
				float alpha_map = tex2D(_AlphaMap, IN.map_uv).a;
				if (alpha_map < 5e-4f) discard;

				return _Color * tex2D(_MainTex, IN.particle_uv);
			}

			ENDCG
		}

	}
}