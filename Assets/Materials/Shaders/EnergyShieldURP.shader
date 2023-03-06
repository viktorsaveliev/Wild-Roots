Shader "Custom/EnergyShieldURP" {
    Properties {
        _MainColor ("Color", Color) = (1, 1, 1, 1)
        _EmissionColor ("Emission", Color) = (1, 1, 1, 1)
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _MainSpeed ("Main Speed", Range(0.0, 10.0)) = 1.0
        _NoiseSpeed ("Noise Speed", Range(0.0, 10.0)) = 1.0
        _MainSize ("Main Size", Range(0.0, 5.0)) = 1.0
        _NoiseSize ("Noise Size", Range(0.0, 5.0)) = 1.0
        _MainPower ("Main Power", Range(0.0, 10.0)) = 1.0
        _NoisePower ("Noise Power", Range(0.0, 10.0)) = 1.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Noise.hlsl"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float _MainSpeed;
            float _NoiseSpeed;
            float _MainSize;
            float _NoiseSize;
            float _MainPower;
            float _NoisePower;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target {
                float2 p = i.uv;
                float2 q = p * _MainSize + float2(_Time.y * _MainSpeed, _Time.x * _MainSpeed);
                float main = (snoise(q, _MainPower) + 1) / 2;
                float noise = (snoise(q * _NoiseSize, _NoisePower) + 1) / 2;
                float border = smoothstep(0.2, 0.0, main - noise);
                float4 color = lerp(_MainColor, _EdgeColor, border);
                color.rgb += _EmissionColor.rgb * main;
                color.a = border;
                return color;
            }
            
            ENDCG
        }
    }
    FallBack "Diffuse"
}