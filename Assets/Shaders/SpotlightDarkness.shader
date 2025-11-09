Shader "Custom/SpotlightDarkness" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _SpotlightCenter ("Spotlight Center", Vector) = (0.5, 0.5, 0, 0)
        _SpotlightRadius ("Spotlight Radius", Float) = 250
        _EdgeSoftness ("Edge Softness", Float) = 50
        _Opacity ("Opacity", Range(0, 1)) = 1
    }
    SubShader {
        Tags { 
            "Queue"="Overlay" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        
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
                // Use UV coordinates directly for UI elements
                o.screenPos = float4(v.uv.x * _ScreenParams.x, v.uv.y * _ScreenParams.y, 0, 1);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // Get screen position in pixels (WebGL compatible)
                float2 screenPixel = float2(i.uv.x * _ScreenParams.x, i.uv.y * _ScreenParams.y);
                
                // Calculate distance from spotlight center
                float dist = distance(screenPixel, _SpotlightCenter.xy);
                
                // Calculate alpha based on distance
                float alpha = 0.0;
                if (dist < _SpotlightRadius) {
                    // Inside spotlight - fully transparent (see everything)
                    alpha = 0.0;
                } else if (dist < _SpotlightRadius + _EdgeSoftness) {
                    // Edge gradient - smooth transition
                    float fadeDistance = dist - _SpotlightRadius;
                    alpha = fadeDistance / _EdgeSoftness;
                } else {
                    // Outside spotlight - pitch black
                    alpha = 1.0;
                }
                
                // Return black color with calculated alpha
                return fixed4(0, 0, 0, alpha * _Opacity);
            }
            ENDCG
        }
    }
    
    Fallback "UI/Default"
}
