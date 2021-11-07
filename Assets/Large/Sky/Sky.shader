Shader "Custom/Sky" {
    Properties {
        _Scale ("Scale", Float) = 1.0
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

            // -- types --
            /// the vertex shader input
            struct VertIn {
                float4 vertex : POSITION;
            };

            /// the fragment shader input
            struct FragIn {
                float4 pos : SV_POSITION;
            };

            // -- props --
            /// the noise scale
            float _Scale;

            // -- noise --
            /// get random value at pt
            float2 Gradient(float2 st) {
                // 2D to 1D  (feel free to replace by some other)
                int n = st.x + st.y * 11111;

                // Hugo Elias hash (feel free to replace by another one)
                n = (n << 13) ^ n;
                n = (n * (n * n * 15731 + 789221) + 1376312589) >> 16;

            #if 0
                // simple random vectors
                return vec2(cos(float(n)),sin(float(n)));
            #else
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
            #endif
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

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 DrawFrag(FragIn f) : SV_Target {
                // scale by uniform
                float2 st = f.pos * _Scale;
                st.x += _SinTime;
                st.y += _SinTime;

                // generate noise
                float c = Noise(st);
                // c = c * 0.5f + 0.5f;

                // produce color
                return fixed4(c, c, c, 1.0f);
            }
            ENDCG
        }
    }
}
