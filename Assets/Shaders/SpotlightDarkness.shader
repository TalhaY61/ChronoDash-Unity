Shader "Custom/SpotlightDarkness"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SpotlightCenter ("Spotlight Center", Vector) = (0.5, 0.5, 0, 0)
        _SpotlightRadius ("Spotlight Radius", Float) = 250
        _EdgeSoftness ("Edge Softness", Float) = 50
        _Opacity ("Opacity", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };
            sampler2D _MainTex;
            float4 _SpotlightCenter;
            float _SpotlightRadius;
            float _EdgeSoftness;
            float _Opacity;
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target {
                float2 screenPixel = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                float dist = distance(screenPixel, _SpotlightCenter.xy);
                float alpha = 1.0;
                if (dist < _SpotlightRadius) {
                    alpha = 0.0;
                } else if (dist < _SpotlightRadius + _EdgeSoftness) {
                    float fadeDistance = dist - _SpotlightRadius;
                    alpha = fadeDistance / _EdgeSoftness;
                } else {
                    alpha = 1.0;
                }
                return fixed4(0, 0, 0, saturate(alpha * _Opacity));
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}
