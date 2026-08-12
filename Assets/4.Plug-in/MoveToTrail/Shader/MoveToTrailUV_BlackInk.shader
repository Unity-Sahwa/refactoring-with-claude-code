/*
Trail ������Ʈ�� Tiling ���� �����Ǿ����. (�׷��� ��ũ���� ���� UV�� Trail ���׸�Ʈ ũ��� ��� ���� �����ϰ� ����)
*/

Shader "MoveToTrailUV/MoveToTrailUV_BlackInk"
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
		_MainScrollSpeedU("Main Scroll U Speed", Float) = 10
		_MainScrollSpeedV("Main Scroll V Speed", Float) = 0
		_ScatterTiling("Scatter Noise Tiling", Vector) = (3, 1, 0, 0)
		_ScatterAmount("Scatter UV Amount", Range(0, 1)) = 0.15
		_ScatterSpeed("Scatter Noise Speed", Float) = 0.5
		_DissolveAmount("Tail Dissolve Amount", Range(0, 2)) = 1
		_Alpha("Alpha", Range(0, 1)) = 1
	}
		SubShader
		{
			Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}
			Blend SrcAlpha OneMinusSrcAlpha // Alpha (ink darkens the screen)
			ZWrite Off
			Cull Off

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
					float2 uvOrigin : TEXCOORD1; // ���� UV
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
					half _MainScrollSpeedU;
					half _MainScrollSpeedV;
					half4 _ScatterTiling;
					half _ScatterAmount;
					half _ScatterSpeed;
					half _DissolveAmount;
					half _Alpha;

					// MoveToMaterialUV ��ũ��Ʈ���� ���޹޴� UV ��ũ�� ��.
					// ������Ƽ���� �Ϻη� ���� ����. ������Ƽ�� ���� ��� �����Ϳ��� �̸������ ���޵Ǵ� ������ ��� ���� ���� �������� �νĵǾ ������Ƽ ���� �۵��ϴ� ������� ����
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
					// 0 = head, 1 = tail (needs TrailRenderer color gradient alpha 1 -> 0)
					half tail = 1 - IN.color.a;

					// scatter: reuse _MainTex as cheap noise, distort uv more toward the tail
					half2 noiseUV = IN.uvOrigin * _ScatterTiling.xy + half2(_Time.x * _ScatterSpeed, _Time.x * _ScatterSpeed * 0.37);
					half noise = tex2D(_MainTex, noiseUV).r;
					half2 scatteredUV = IN.uv + (noise - 0.5) * _ScatterAmount * tail;

					half4 mainTex = tex2D(_MainTex, scatteredUV);

					// ���� �ؽ��� ����
					half vFade = 1 - abs(IN.uvOrigin.y - 0.5) * 2; // ���� uv �������� A �׷��� ����
					vFade = pow(abs(vFade), _MainTexVFadePow); // A ����� ���� �����ϰ� Ȥ�� �ձ۰�
					vFade = lerp(1, vFade, _MainTexVFade);
					mainTex.rgb *= vFade; // �ϴ� �ؽ��Ŀ� ���� ���̵�ƿ����� ����
					mainTex.rgb = pow(abs(mainTex.rgb), _MainTexPow) * _MainTexMultiplier; // ���� �ؽ��� 1�� ����
					
					// ���ý� ���Ŀ� _Multiplier�� �̿�ȭ�� ���� �ϳ��� ����
					half intensity = _Multiplier * IN.color.a;

					// Tint
					half avr = mainTex.r * 0.3333 + mainTex.g * 0.3334 + mainTex.b * 0.3333;
					avr = saturate(avr * intensity); // intensity 1�� �Ѵ� ������ �ϴ� 1�� ���ø�
					half4 col = tex2D(_TintTex, half2(avr, 0.5));

					// tail dissolve: noise punches holes that grow toward the tail
					half dissolve = saturate(noise * 2 - tail * _DissolveAmount);

					col.rgb *= IN.color.rgb;
					col.a = avr * dissolve * _Alpha;
					return col;
				}
				ENDHLSL
			}
		}
}
