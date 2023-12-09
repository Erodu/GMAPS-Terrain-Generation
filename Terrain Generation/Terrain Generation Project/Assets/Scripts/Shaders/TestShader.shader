Shader "Terrain/TestShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TerrainColour("Terrain Colour",COLOR) = (1,1,1,0)
        _TerrainColour1("Terrain Colour1",COLOR) = (1,1,1,0)
        _TerrainColour2("Terrain Colour2",COLOR) = (1,1,1,0)

        _WaterColour("Water Colour", Color) = (0,0,0,0)
        _WaterColour1("Water Colour1", Color) = (0,0,0,0)
        _WaterColour2("Water Colour2", Color) = (0,0,0,0)
        _NumWaves("Number Of Waves",Integer) = 0
        _WaterH("Water Height",float) = 0
        _Scale("Scale",float) = 0
        _Amp("Amplitude",float) = 0
        _WaveL("Wave Length",float) = 0
        _Spd("Wave Speed",float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 colour : COLOR;
                float3 normal: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TerrainColour;
            float4 _TerrainColour1;
            float4 _TerrainColour2;

            v2f vert (appdata v)
            {
                v2f o;
                float height = v.vertex.y;
                o.colour = saturate(lerp(_TerrainColour1, _TerrainColour2, height)) *0.5 + 0.5;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                //lambert diffuse
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float diff = max(0.0, dot(i.normal, lightDir));

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return i.colour * _TerrainColour * diff;
            }
            ENDCG
        }

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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float colour : COLOR;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _NumWaves;
            float4 _WaterColour;
            float4 _WaterColour1;
            float4 _WaterColour2;

            float _WaterH;
            float _Scale;
            float _Amp;
            float _WaveL;
            float _Spd;

            float2 hash(float2 pos)
            {
                const float2 k = float2(0.3183099, 0.3678794);

                float2 h = dot(pos, float2(127.1, 311.7));
                h = frac(sin(h) * k);

                return frac(h * 2.0 - 1.0);
            }

            float WaveState(float2 pos,float2 dir,float spd,float amp)
            {
                float height = (_Amp * amp) * sin(dot(dir,pos) * (_WaveL / 2) + _Time * (_Spd * spd * 2 / _WaveL));
                return height;
            }

            float SoS(float2 pos)
            {
                float frequency = 1;
                float spd = 1;
                float freqInfluence = 0.15;
                float height = 0;
                float amp = _Amp;
                for (int i = 0; i < _NumWaves; i++)
                {
                    float2 samplePos = pos / frequency;
                    float2 dir = hash(samplePos);
                    height += WaveState(samplePos, dir,spd,amp);

                    frequency *= freqInfluence;
                    amp *= freqInfluence;
                    spd /= frequency * (50);
                }

                return height;
            }

            v2f vert(appdata v)
            {
                v2f o;

                v.vertex.y = SoS(float2(v.vertex.x,v.vertex.z) * _Scale) + _WaterH;
                float height = (v.vertex.y / _NumWaves * 2.5) - (_WaterH/3);

                //o.colour = float4(height, height, height, 0) * 0.75 + 0.5;

                o.colour = lerp(_WaterColour1, _WaterColour2, height * 1.75 + 0.15);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = i.colour;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * _WaterColour;
            }
            ENDCG
        }
    }
}
