Shader "Custom/ForceField" {
        Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Speed ("Pulse Speed", Range(0.1, 10)) = 1.0
        _Strength ("Pulse Strength", Range(0, 2)) = 1.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityDeferredLibrary.cginc"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
            //#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityShaderVariables.hlsl"

            struct IN
            {
                float4 worldPos : TEXCOORD0;
            };

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TintColor;
            float _Speed;
            float _Strength;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float2 center = float2(0.5, 0.5);
                float2 distance = i.uv - center;
                float magnitude = saturate((1 - (_Time.y - (_Frequency * exp(-magnitude)))) * _Amplitude);

                float time = _Time.y * _Speed;
                float pulse = sin(time * 3.14159 * 2.0) * 0.5 + 0.5;
                strength *= pulse;

                float4 color = tex2D(_MainTex, i.uv) * _TintColor;
                color.a *= strength;

                return color;
            }
            ENDCG
        }
    }
}