Shader "Custom/TimeBubbleEffect" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BubbleCenter ("Bubble Center", Vector) = (0.5, 0.5, 0, 0)
        _BubbleRadius ("Bubble Radius", Float) = 250
        _EdgeSoftness ("Edge Softness", Float) = 50
        _BubbleColor ("Bubble Color", Color) = (0.5, 0.8, 1, 0.3)
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
            float4 _BubbleCenter;
            float _BubbleRadius;
            float _EdgeSoftness;
            float4 _BubbleColor;
            float _Opacity;
            
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // Get screen position in pixels
                float2 screenPixel = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                
                // Calculate distance from bubble center
                float dist = distance(screenPixel, _BubbleCenter.xy);
                
                // Calculate alpha based on distance (INVERTED from darkness)
                float alpha = 0.0;
                if (dist < _BubbleRadius) {
                    // Inside bubble - show colored effect
                    alpha = 1.0;
                } else if (dist < _BubbleRadius + _EdgeSoftness) {
                    // Edge gradient - smooth transition
                    float fadeDistance = dist - _BubbleRadius;
                    alpha = 1.0 - (fadeDistance / _EdgeSoftness);
                } else {
                    // Outside bubble - fully transparent (normal view)
                    alpha = 0.0;
                }
                
                // Return bubble color with calculated alpha
                return fixed4(_BubbleColor.rgb, alpha * _BubbleColor.a * _Opacity);
            }
            ENDCG
        }
    }
    
    Fallback "UI/Default"
}
