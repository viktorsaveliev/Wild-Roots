Shader "Custom/ItemHighlight" {
    Properties {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _Range ("Range", Range (0.0, 1.0)) = 1.0
        _Softness ("Softness", Range (0.01, 0.5)) = 0.1
        _Lightness ("Lightness", Range (0.0, 2.0)) = 1.0
        _Speed ("Speed", Range (0.0, 10.0)) = 1.0
        _Alpha ("Alpha", Range (0.0, 1.0)) = 1.0
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}
        Blend SrcAlpha OneMinusSrcAlpha
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _BaseColor;
            float4 _HighlightColor;
            float _Range;
            float _Softness;
            float _Lightness;
            float _Speed;
            float _Alpha;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float dist = length(i.uv - 0.5);
                float smoothDist = smoothstep(_Range - _Softness, _Range, dist);
                float sinVal = sin(_Time.y * _Speed);
                float lightness = (_Lightness + sinVal) * smoothDist;

                float4 color = lerp(_BaseColor, _HighlightColor, lightness);
                color.a = _Alpha;

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}