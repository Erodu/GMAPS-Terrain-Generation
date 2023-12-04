Shader "Terrain/TerrainVertexShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Seed("Seed",int) = 0
        _Scale("Scale",float) = 0
        _Strength("Strength",float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _Seed;
            float _Scale;
            float _Strength;

            float hash(float2 p)
            {
                //change this hash function later on to be better
                p = floor(p);
                p = 50.0 * frac(p * 0.3183099 / _Seed + float2(0.71, 0.113));
                return -1.0 + 2.0 * frac(p.x * p.y * (p.x + p.y) + _Seed);
            }

            float mix(float x, float y, float a)
            {
                return dot(x, (1 - a)) + dot(y, a);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                //quintic interpolate

                float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

                return mix(mix(hash(i + float2(0.0, 0.0)),
                    hash(i + float2(1.0, 0.0)), u.x),
                    mix(hash(i + float2(0.0, 1.0)),
                        hash(i + float2(1.0, 1.0)), u.x), u.y);
            }

            v2f vert (appdata v)
            {
                v2f o;

                float2 pos = float2(v.vertex.x, v.vertex.z) / _Scale;
                v.vertex.y = noise(pos) / _Strength;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
