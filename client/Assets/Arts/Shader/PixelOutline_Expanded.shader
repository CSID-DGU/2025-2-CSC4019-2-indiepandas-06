Shader "UI/PixelOutline_Expanded"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        [PerRendererData]_Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.5
        _Thickness ("Outline Thickness (Pixels)", Range(0,10)) = 1

        // --- UI / Mask & Stencil ---
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
            Name "UIPixelOutline_Expanded"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _AlphaThreshold;
            float _Thickness;

            float4 _ClipRect;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

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
                float4 worldPos : TEXCOORD1;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                
                // [수정된 부분 시작] ----------------------------------------------------
                // 1. 버텍스 확장: 메쉬 자체를 외곽선 두께만큼 밖으로 밀어냅니다.
                // (주의: Image Type이 Simple일 때 가장 잘 작동합니다)
                
                // 중심점(0.5, 0.5)을 기준으로 방향을 구합니다 (-1 ~ 1)
                float2 center = float2(0.5, 0.5);
                float2 dir = sign(v.uv - center); 

                // 버텍스 위치 확장 (UI 단위)
                // 보통 Canvas Scaler 설정에 따라 1 Unit = 1 Pixel에 근접합니다.
                v.vertex.xy += dir * _Thickness;

                // 2. UV 보정: 메쉬가 늘어난 만큼 UV 범위를 넓혀서 이미지가 늘어지지 않게 합니다.
                // UV를 0~1 바깥(-0.x ~ 1.x)으로 확장하여 빈 공간을 만듭니다.
                // _Thickness(픽셀) / 전체 텍스처 크기(픽셀) = UV 공간에서의 두께
                float2 uvExtension = _Thickness * _MainTex_TexelSize.xy;
                
                // UV 좌표도 바깥쪽으로 밀어줍니다.
                v.uv += dir * uvExtension;
                // [수정된 부분 끝] ------------------------------------------------------

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                o.worldPos = v.vertex;
                return o;
            }

            // RectMask2D 처리용
            float ComputeMask(in float4 worldPos)
            {
                #ifdef UNITY_UI_CLIP_RECT
                float2 inside = step(_ClipRect.xy, worldPos.xy) * step(worldPos.xy, _ClipRect.zw);
                float2 dist = min(worldPos.xy - _ClipRect.xy, _ClipRect.zw - worldPos.xy);
                float2 softness = float2(_UIMaskSoftnessX, _UIMaskSoftnessY);
                float2 t = saturate(dist / max(softness, 1e-5));
                float clipSoft = min(t.x, t.y);
                return min(clipSoft, min(inside.x, inside.y));
                #else
                return 1.0;
                #endif
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                float a = c.a;

                float2 t = float2(_MainTex_TexelSize.x * _Thickness,
                                  _MainTex_TexelSize.y * _Thickness);

                // 4방향 샘플링
                float aN = tex2D(_MainTex, i.uv + float2( 0,  t.y)).a;
                float aS = tex2D(_MainTex, i.uv + float2( 0, -t.y)).a;
                float aE = tex2D(_MainTex, i.uv + float2( t.x,  0)).a;
                float aW = tex2D(_MainTex, i.uv + float2(-t.x,  0)).a;

                float maxNeighbor = max(max(aN, aS), max(aE, aW));

                fixed4 outCol;

                if (a > _AlphaThreshold)
                {
                    outCol = c;
                }
                else if (maxNeighbor > _AlphaThreshold)
                {
                    outCol = fixed4(_OutlineColor.rgb, _OutlineColor.a * i.color.a);
                }
                else
                {
                    outCol = fixed4(0,0,0,0);
                }

                float mask = ComputeMask(i.worldPos);
                outCol.a *= mask;
                outCol.rgb *= outCol.a;

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