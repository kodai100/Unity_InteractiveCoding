Shader "Ribon/Renderer_Ribon"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			Lighting On

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fwdbase
				
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Particle
			{
				float3 position;
				float3 velocity;
				float4 color;
			};

			struct v2g
			{
				float2 uv : TEXCOORD0;
				float3 pos : TEXCOORD4;
				float3 velocity : TEXCOORD5;
				float4 color : TEXCOORD3;
			};

			struct g2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD3;
				LIGHTING_COORDS(1,2)
			};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				StructuredBuffer<Particle> _Particles;
				float _Length = 20;
					
			v2g vert (appdata v, uint vid : SV_VertexID)
			{
				v2g o;
				o.pos = _Particles[vid].position;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.velocity = _Particles[vid].velocity;
				o.color = _Particles[vid].color;
				return o;
			}

			[maxvertexcount(2)]
			void geom(point v2g p[1], inout LineStream<g2f> lineStream)
			{
				g2f o;
				o.pos = mul(UNITY_MATRIX_VP, float4(p[0].pos, 1));
				o.uv = TRANSFORM_TEX(p[0].uv, _MainTex);
				o.color = float4(p[0].color.xyz, 1);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				lineStream.Append(o);

				o.pos = mul(UNITY_MATRIX_VP, float4(p[0].pos - (p[0].velocity*_Length), 1));
				o.uv = TRANSFORM_TEX(p[0].uv, _MainTex);
				o.color = float4(p[0].color.xyz, 1);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				lineStream.Append(o);
			}	
					
			fixed4 frag (g2f i) : SV_Target
			{
				// sample the texture
				float atten = LIGHT_ATTENUATION(i) + 0.2;
				return i.color * atten;
			}
			ENDCG
		}


		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }
			ZWrite On ZTest LEqual
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
				
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct Particle
			{
				float3 position;
				float3 velocity;
				float4 color;
			};

			struct v2g
			{
				float2 uv : TEXCOORD0;
				float3 pos : TEXCOORD4;
				float3 velocity : TEXCOORD5;
				float4 color : TEXCOORD3;
				float3 normal : NORMAL;
			};

			struct g2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD3;
				float4 hpos : TEXCOORD4;
			};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				StructuredBuffer<Particle> _Particles;
				float _Length = 20;
					
			v2g vert (appdata v, uint vid : SV_VertexID)
			{
				v2g o;
				o.pos = _Particles[vid].position;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.velocity = _Particles[vid].velocity;
				o.color = _Particles[vid].color;
				o.normal = v.normal;
				return o;
			}

			[maxvertexcount(2)]
			void geom(point v2g p[1], inout LineStream<g2f> lineStream)
			{
				g2f o;
				o.uv = TRANSFORM_TEX(p[0].uv, _MainTex);
				o.color = p[0].color;

				float4 lpos1 = mul(unity_WorldToObject, float4(p[0].pos, 1));
			    o.pos = UnityClipSpaceShadowCasterPos(lpos1, p[0].normal);
			    o.pos = UnityApplyLinearShadowBias(o.pos);
			    o.hpos = o.pos;
				lineStream.Append(o);


				float4 _wpos = float4(p[0].pos - (p[0].velocity*_Length), 1);
				o.uv = TRANSFORM_TEX(p[0].uv, _MainTex);
				o.color = p[0].color;

				float4 lpos2 = mul(unity_WorldToObject, _wpos);
			    o.pos = UnityClipSpaceShadowCasterPos(lpos2, p[0].normal);
			    o.pos = UnityApplyLinearShadowBias(o.pos);
			    o.hpos = o.pos;
				lineStream.Append(o);
			}	
					
			fixed4 frag (g2f i) : SV_Target
			{
				// sample the texture
				return i.hpos.zw.x / i.hpos.zw.y;
			}
			ENDCG
		}
	}
}
