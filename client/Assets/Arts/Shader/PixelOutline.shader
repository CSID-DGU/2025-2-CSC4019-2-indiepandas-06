Shader "UI/PixelOutline"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData]_Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.5
        _Thickness ("Outline Thickness (texels)", Range(0,4)) = 1

        // --- UI / Mask & Stencil (Unity UI 관례 속성) ---
        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
        [HideInInspector]_UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [HideInInspector]_ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
        [HideInInspector]_UIMaskSoftnessX ("Mask SoftnessX", Float) = 0
        [HideInInspector]_UIMaskSoftnessY ("Mask SoftnessY", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIPixelOutline"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            // UI용 멀티컴파일 (RectMask2D, Alpha Clip 등)
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x=1/width, y=1/height
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _AlphaThreshold;
            float _Thickness;

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            float _UseUIAlphaClip;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
                float4 worldPos : TEXCOORD1; // UI 클리핑용(로컬/월드 좌표)
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                o.worldPos = v.vertex; // UI 클립 함수가 사용하는 좌표
                return o;
            }

            // RectMask2D 소프트니스 처리
            float ComputeMask(in float4 worldPos)
            {
                #ifdef UNITY_UI_CLIP_RECT
                // 기본 클립 팩터
                float2 inside = step(_ClipRect.xy, worldPos.xy) * step(worldPos.xy, _ClipRect.zw);
                float2 dist = min(worldPos.xy - _ClipRect.xy, _ClipRect.zw - worldPos.xy);
                float2 softness = float2(_UIMaskSoftnessX, _UIMaskSoftnessY);
                float2 t = saturate(dist / max(softness, 1e-5));
                float clipSoft = min(t.x, t.y);
                // inside=1이면 clipSoft 그대로, 밖이면 0
                return min(clipSoft, min(inside.x, inside.y));
                #else
                return 1.0;
                #endif
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 원본 스프라이트 샘플
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float a = c.a;

                // 외곽선 탐지용 텍셀 오프셋
                float2 t = float2(_MainTex_TexelSize.x * _Thickness,
                                  _MainTex_TexelSize.y * _Thickness);

                // 4방향 이웃 알파
                float aN  = tex2D(_MainTex, i.uv + float2( 0,  t.y)).a;
                float aS  = tex2D(_MainTex, i.uv + float2( 0, -t.y)).a;
                float aE  = tex2D(_MainTex, i.uv + float2( t.x,  0)).a;
                float aW  = tex2D(_MainTex, i.uv + float2(-t.x,  0)).a;
                // 8방향
                //float aNE = tex2D(_MainTex, i.uv + float2( t.x,  t.y)).a;
                //float aNW = tex2D(_MainTex, i.uv + float2(-t.x,  t.y)).a;
                //float aSE = tex2D(_MainTex, i.uv + float2( t.x, -t.y)).a;
                //float aSW = tex2D(_MainTex, i.uv + float2(-t.x, -t.y)).a;

                float maxNeighbor = max(max(aN, aS), max(aE, aW));

                fixed4 outCol;

                // 1) 원본 픽셀이 불투명: 원본 표시
                if (a > _AlphaThreshold)
                {
                    outCol = c;
                }
                // 2) 현재 픽셀은 투명 + 이웃이 불투명: 외곽선
                else if (maxNeighbor > _AlphaThreshold)
                {
                    // UI 전체 알파(i.color.a)도 반영
                    outCol = fixed4(_OutlineColor.rgb, _OutlineColor.a * i.color.a);
                }
                else
                {
                    outCol = fixed4(0,0,0,0);
                }

                // RectMask2D/캔버스 클리핑 반영 (소프트니스 포함)
                float mask = ComputeMask(i.worldPos);
                outCol.a *= mask;
                outCol.rgb *= outCol.a; // 프리멀티플라이는 아니지만 가장자리 깔끔하게

                // AlphaClip(옵션)
                #ifdef UNITY_UI_ALPHACLIP
                clip(outCol.a - 0.001);
                #endif

                return outCol;
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}