// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Grass" {

	Properties{
		_MainTex("ParticleTexture", 2D) = "white"{}
		_Color("Color", Color) = (1,1,1,1)
		_Width("Width", Range(0, 1)) = 1
		_Length("Length", Range(0, 10)) = 1
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGINCLUDE

		#pragma target 5.0
#ifndef SHADOW
		#pragma multi_compile_fwdbase
#else
		#pragma multi_compile_shadowcaster
#endif

		#include "UnityCG.cginc"
		#include "Assets/Common/Shaders/SimplexNoiseGrad2D.cginc"

		#ifndef SHADOW
		#include "AutoLight.cginc"
		#endif

		float _Width;
		sampler2D _MainTex;
		fixed4 _Color;
		float _Length;
		float _Distribution;
		float3 _Gravity;
		float _DebugFloat;

		StructuredBuffer<float3> _Particles;

		struct appdata {
			float4 vertex: POSITION;
		};

		struct v2g {
			float4 vertex : POSITION;
		};

#define G2F_COMMON float3 normal : NORMAL; float3 lightDir : TEXCOORD2; float3 viewDir: TEXCOORD3;
		struct g2f {
#ifdef SHADOW
			V2F_SHADOW_CASTER;
			G2F_COMMON
#else 
			float4 pos : POSITION;
			G2F_COMMON
			LIGHTING_COORDS(5, 6)
#endif
		};

		v2g vert(appdata v, uint id: SV_VertexID) {
			v2g OUT;

			OUT.vertex = float4(_Particles[id],1);

			return OUT;
		}


		float snoise(float2 v)
		{
			const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
				0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
				-0.577350269189626,  // -1.0 + 2.0 * C.x
				0.024390243902439); // 1.0 / 41.0
									// First corner
			float2 i = floor(v + dot(v, C.yy));
			float2 x0 = v - i + dot(i, C.xx);

			// Other corners
			float2 i1;
			i1.x = step(x0.y, x0.x);
			i1.y = 1.0 - i1.x;

			// x1 = x0 - i1  + 1.0 * C.xx;
			// x2 = x0 - 1.0 + 2.0 * C.xx;
			float2 x1 = x0 + C.xx - i1;
			float2 x2 = x0 + C.zz;

			// Permutations
			i = mod289(i); // Avoid truncation effects in permutation
			float3 p =
				permute(permute(i.y + float3(0.0, i1.y, 1.0))
					+ i.x + float3(0.0, i1.x, 1.0));

			float3 m = max(0.5 - float3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
			m = m * m;
			m = m * m;

			// Gradients: 41 points uniformly over a line, mapped onto a diamond.
			// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)
			float3 x = 2.0 * frac(p * C.www) - 1.0;
			float3 h = abs(x) - 0.5;
			float3 ox = floor(x + 0.5);
			float3 a0 = x - ox;

			// Normalise gradients implicitly by scaling m
			m *= taylorInvSqrt(a0 * a0 + h * h);

			// Compute final noise value at P
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.y = a0.y * x1.x + h.y * x1.y;
			g.z = a0.z * x2.x + h.z * x2.y;
			return 130.0 * dot(m, g);
		}


		[maxvertexcount(40)]
		void geom_square(point v2g IN[1], inout TriangleStream<g2f> OUT) {

			float3 inputPos = IN[0].vertex.xyz;
			float3 randomXZRotation = normalize(float3(snoise(IN[0].vertex.xz), 0, snoise(IN[0].vertex.zx)));

			float3 growUp = normalize(float3(snoise_grad(float2(IN[0].vertex.x, _Time.z / 10)).x*0.1, 10, snoise_grad(float2(IN[0].vertex.x, _Time.z / 10)).y*0.1));	// y の値で曲がり具合が変わる

			// float3 right = normalize(cross(ObjSpaceViewDir(inputPos).xyz, growUp));	// ビルボード
			float3 right = normalize(cross(randomXZRotation, growUp));	

			float3 g = _Gravity * 0.01;	// Gravity

			g2f pIn;
			v2g v;

			float myLength = _Length * lerp(0.2, 1, (1+snoise(inputPos.xz*_Distribution))*0.5);

			int resolution = 10;
			for (int i = 0; i < resolution; i++) {

				float3 p = IN[0].vertex + lerp(_Width, 0, (float)(i+1)/resolution) * right + i * myLength /(float)resolution * growUp + lerp(0, 1, i / (float)resolution)*lerp(0, 1, i / (float)resolution)*g;
				pIn.pos = UnityObjectToClipPos(p);
				pIn.normal = normalize(cross((i+1)*growUp, right));
				pIn.lightDir = ObjSpaceLightDir(float4(p, 1));
				pIn.viewDir = ObjSpaceViewDir(float4(p,1));
#ifdef SHADOW
				v.vertex = float4(p,1);
				TRANSFER_SHADOW_CASTER(pIn)
#else
				TRANSFER_VERTEX_TO_FRAGMENT(pIn);
				TRANSFER_SHADOW(pIn);
#endif
				OUT.Append(pIn);

				p = IN[0].vertex - lerp(_Width, 0, (float)(i + 1) / resolution) * right + i * (float)(myLength / resolution) * growUp + lerp(0, 1, (float)i / resolution)*lerp(0, 1, (float)i / resolution)*g;
				pIn.pos = UnityObjectToClipPos(p);
				pIn.normal = normalize(cross((i+1)*growUp, right));
				pIn.lightDir = ObjSpaceLightDir(float4(p, 1));
				pIn.viewDir = ObjSpaceViewDir(float4(p, 1));
#ifdef SHADOW
				v.vertex = float4(p, 1);
				TRANSFER_SHADOW_CASTER(pIn)
#else
				TRANSFER_VERTEX_TO_FRAGMENT(pIn);
				TRANSFER_SHADOW(pIn);
#endif
				OUT.Append(pIn);
			}

			OUT.RestartStrip();

//			for (int i = 0; i < resolution; i++) {
//
//				float3 p = IN[0].vertex - lerp(_Width, 0, (float)(i + 1) / resolution) * right + i * (float)(myLength / resolution) * growUp + lerp(0, 1, (float)i / resolution)*lerp(0, 1, (float)i / resolution)*g;
//				pIn.pos = UnityObjectToClipPos(p);
//				pIn.normal = normalize(cross((i + 1)*growUp, right));
//				pIn.lightDir = ObjSpaceLightDir(float4(p, 1));
//				pIn.viewDir = ObjSpaceViewDir(float4(p, 1));
//#ifdef SHADOW
//				v.vertex = float4(p, 1);
//				TRANSFER_SHADOW_CASTER(pIn)
//#else
//				TRANSFER_VERTEX_TO_FRAGMENT(pIn);
//				TRANSFER_SHADOW(pIn);
//#endif
//				OUT.Append(pIn);
//
//				p = IN[0].vertex + lerp(_Width, 0, (float)(i + 1) / resolution) * right + i * myLength / (float)resolution * growUp + lerp(0, 1, i / (float)resolution)*lerp(0, 1, i / (float)resolution)*g;
//				pIn.pos = UnityObjectToClipPos(p);
//				pIn.normal = normalize(cross((i + 1)*growUp, right));
//				pIn.lightDir = ObjSpaceLightDir(float4(p, 1));
//				pIn.viewDir = ObjSpaceViewDir(float4(p, 1));
//#ifdef SHADOW
//				v.vertex = float4(p, 1);
//				TRANSFER_SHADOW_CASTER(pIn)
//#else
//				TRANSFER_VERTEX_TO_FRAGMENT(pIn);
//				TRANSFER_SHADOW(pIn);
//#endif
//				OUT.Append(pIn);
//			}
//
			OUT.RestartStrip();
		}

		ENDCG

		Pass{
			Tags{ "LightMode" = "ForwardBase" }
			Lighting On ZWrite On

			CGPROGRAM

			
			#pragma vertex vert
			#pragma geometry geom_square
			#pragma fragment frag

			fixed4 frag(g2f IN) : SV_Target{

				fixed4 col = _Color;

				float3 normal = IN.normal;

				float3 lightDir = normalize(IN.lightDir);
				float3 viewDir = normalize(IN.viewDir);
				float3 halfDir = normalize(lightDir + viewDir);

				float nh = dot(normal, halfDir);

				float atten = LIGHT_ATTENUATION(IN);
				float3 spec = max(0.1, pow(saturate(nh), 1));
				col.rgb *= spec *atten;

				return col;
			}

			ENDCG
		}

		Pass{

			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On ZTest LEqual

			CGPROGRAM

			#define SHADOW 1
			
			#pragma vertex vert
			#pragma geometry geom_square
			#pragma fragment shadow_frag

			float4 shadow_frag(g2f IN) : COLOR{
				SHADOW_CASTER_FRAGMENT(IN)
			}

			ENDCG
		}

	}
}