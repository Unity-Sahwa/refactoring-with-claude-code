/*
MoveToTrailUV_Add의 Premultiplied Alpha 버전.
rgb는 Add와 똑같이 더해지고, _TintTex의 A만큼 배경을 깎아서 검은색이 실제로 보임.
TintTex A = 0이면 Add와 완전히 동일한 결과.
*/

Shader "MoveToTrailUV/MoveToTrailUV_Blend"
{
	Properties
	{
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
		_MainTexVFade("MainTex V Fade", Range(0, 1)) = 0
		_MainTexVFadePow("MainTex V Fade Pow", Float) = 1
		_MainTexPow("Main Texture Gamma", Float) = 1
		_MainTexMultiplier("Main Texture Multiplier", Float) = 1
		_TintTex("Tint Texture (RGB)", 2D) = "white" {}
		_Multiplier("Multiplier", Float) = 1
		_Opacity("Opacity (배경 가리는 정도)", Range(0, 1)) = 0
		_MainScrollSpeedU("Main Scroll U Speed", Float) = 10
		_MainScrollSpeedV("Main Scroll V Speed", Float) = 0
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}
			Blend One OneMinusSrcAlpha // Premultiplied Alpha
			ZWrite Off

			Pass
			{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

				struct Attributes
				{
					float4 positionOS : POSITION;
					float2 uv : TEXCOORD0;
					half4 color : COLOR;
				};

				struct Varyings
				{
					float2 uv : TEXCOORD0;
					float2 uvOrigin : TEXCOORD1;
					float4 positionHCS : SV_POSITION;
					half4 color : COLOR;
				};

				sampler2D _MainTex;
				sampler2D _TintTex;

				CBUFFER_START(UnityPerMaterial)
					half4 _MainTex_ST;
					half _MainTexVFade;
					half _MainTexVFadePow;
					half _MainTexPow;
					half _MainTexMultiplier;
					half _Multiplier;
					half _Opacity;
					half _MainScrollSpeedU;
					half _MainScrollSpeedV;

					// MoveToMaterialUV 스크립트에서 전달받는 UV 스크롤 값 (프로퍼티에 일부러 노출 안 함)
					half _MoveToMaterialUV;
				CBUFFER_END

				Varyings vert(Attributes IN)
				{
					Varyings o;
					o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
					o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
					o.uv.x -= frac(_Time.x * _MainScrollSpeedU) + _MoveToMaterialUV;
					o.uv.y -= frac(_Time.x * _MainScrollSpeedV);
					o.uvOrigin = IN.uv;
					o.color = IN.color;
					return o;
				}

				half4 frag(Varyings IN) : SV_Target
				{
					half4 mainTex = tex2D(_MainTex, IN.uv);

					half vFade = 1 - abs(IN.uvOrigin.y - 0.5) * 2;
					vFade = pow(abs(vFade), _MainTexVFadePow);
					vFade = lerp(1, vFade, _MainTexVFade);
					mainTex.rgb *= vFade;
					mainTex.rgb = pow(abs(mainTex.rgb), _MainTexPow) * _MainTexMultiplier;

					half intensity = _Multiplier * IN.color.a;

					// Tint
					half avr = mainTex.r * 0.3333 + mainTex.g * 0.3334 + mainTex.b * 0.3333;
					half mask = saturate(avr); // MainTex 밝기 = 트레일 형태 마스크
					half4 col = tex2D(_TintTex, half2(saturate(avr * intensity), 0.5));

					half intensityHigh = max(1, intensity);
					col.rgb *= intensityHigh * IN.color.rgb;

					// A는 배경을 얼마나 가릴지. TintTex의 A는 무시하고 형태 마스크로 대체
					col.a = mask * _Opacity * IN.color.a;
					return col;
				}
				ENDHLSL
			}
		}
}
