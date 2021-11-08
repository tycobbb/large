Shader "Custom/Desert" {
    Properties {
        _Scale ("Scale", Float) = 1.0
        _Period ("Period", Float) = 0.1
        _Bands ("Bands", Int) = -1
        _FogMin ("Fog Min", Float) = 0.0
        _FogHeight ("Fog Height", Float) = 500.0
        _HueMin ("Hue Min", Range(0.0, 2.0)) = 0.0
        _HueMax ("Hue Max", Range(0.0, 2.0)) = 0.0
        _Saturation ("Saturation", Range(0.0, 1.0)) = 0.4
        _Value ("Value", Range(0.0, 1.0)) = 0.9
    }

    SubShader {
        Tags {
            "RenderType"="Opaque"
        }

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"
            #include "../Post/Core/Color.cginc"

            // -- types --
            /// the vertex shader input
            struct VertIn {
                float4 pos : POSITION;
            };

            /// the fragment shader input
            struct FragIn {
                float4 cPos : SV_POSITION;
                float4 wPos : TEXCOORD0;
                float3 vDir : TEXCOORD2;
            };

            // -- props --
            /// the noise scale
            float _Scale;

            /// the noise period
            float _Period;

            /// the number of bands
            float _Bands;

            // the min y of the fog band
            float _FogMin;

            // the height of the fog band
            float _FogHeight;

            /// the minimum hue
            float _HueMin;

            /// the maximum hue
            float _HueMax;

            /// the saturation
            float _Saturation;

            /// the value
            float _Value;

            // -- noise --
            /// get random value at pt
            float2 Gradient(float2 st) {
                // 2d to 1d
                int n = st.x + st.y * 11111;

                // hugo elias hash
                n = (n << 13) ^ n;
                n = (n * (n * n * 15731 + 789221) + 1376312589) >> 16;

                // perlin style vectors
                n &= 7;
                float2 gr = float2(n & 1, n >> 1) * 2.0 - 1.0;
                if (n >= 6) {
                    return float2(0.0, gr.x);
                } else if (n >= 4) {
                    return float2(gr.x, 0.0f);
                } else {
                    return gr;
                }
            }

            float Noise(float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);
                float2 u = f * f * (3.0f - 2.0f * f);

                float res = lerp(
                    lerp(
                        dot(Gradient(i + float2(0.0f, 0.0f)), f - float2(0.0f, 0.0f)),
                        dot(Gradient(i + float2(1.0f, 0.0f)), f - float2(1.0f, 0.0f)),
                        u.x
                    ),
                    lerp(
                        dot(Gradient(i + float2(0.0f, 1.0f)), f - float2(0.0f, 1.0f)),
                        dot(Gradient(i + float2(1.0f, 1.0f)), f - float2(1.0f, 1.0f)),
                        u.x
                    ),
                    u.y
                );

                return res;
            }

            float Image(float2 st) {
                float2x2 m = float2x2(1.6, 1.2, -1.2, 1.6);

                // blend noise
                float c = 0.0f;
                c  = 0.5000f * Noise(st);
                st = mul(st, m);
                c += 0.2500f * Noise(st);
                st = mul(st, m);
                c += 0.1250f * Noise(st);
                st = mul(st, m);
                c += 0.0625f * Noise(st);
                st = mul(st, m);

                // shift range
                c = frac(c + 0.5f);

                // apply banding
                if (_Bands != -1.0f) {
                    c = floor(c * _Bands) / (_Bands - 1.0f);
                }

                return c;
            }

            // -- program --
            FragIn DrawVert(VertIn v) {
                float3 pos = v.pos.xyz;

                FragIn o;
                o.cPos = UnityObjectToClipPos(pos);
                o.wPos = mul(unity_ObjectToWorld, float4(pos, 1.0));
                o.vDir = WorldSpaceViewDir(v.pos);

                return o;
            }

            fixed4 DrawFrag(FragIn f) : SV_Target {
                // scale by uniform
                float2 st = float2(
                    dot(normalize(f.wPos), f.vDir),
                    dot(normalize(f.cPos), f.vDir)
                );

                st *= _Scale;

                // shift w/ time
                st.x += _Time * _Period;
                st.y += _Time * _Period;

                // generate color
                float3 c = float3(
                    lerp(_HueMin, _HueMax, Image(st)),
                    _Saturation,
                    _Value
                );

                // apply fog
                float fog = saturate((_FogHeight - _FogMin - f.wPos.y) / _FogHeight);
                fog *= fog * fog;
                c.y -= fog;
                c.z += fog;

                // produce color
                return fixed4(IntoRgb(c), 1.0f);
            }
            ENDCG
        }
    }
}
