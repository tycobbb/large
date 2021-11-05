Shader "Custom/DesertHeight" {
    Properties {
        _Scale ("Scale", Float) = 1.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

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

            // -- helpers --
            /// get random value at pt
            float Rand(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // -- noise --
            float Noise(float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);
                float2 u = f * f * (3.0 - 2.0 * f);

                float res = lerp(
                    lerp(
                        dot(Rand(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                        dot(Rand(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)),
                        u.x
                    ),
                    lerp(
                        dot(Rand(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                        dot(Rand(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)),
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
