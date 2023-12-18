Shader "Terrain/TerrainShader"
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

        //this pass renders the terrain's colour
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            //struct that contains vertex data in the vertex shader
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            //struct that contains vertex data in the fragment shader
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

            //VERTEX SHADER
            v2f vert (appdata v)
            {
                v2f o;

                //get the height of the vertex
                float height = v.vertex.y;
                
                //set the colour of the vertex according to how high it is
                //the *0.5 + 0.5 normalizes the colour range
                o.colour = saturate(lerp(_TerrainColour1, _TerrainColour2, height)) *0.5 + 0.5;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //get the normal of the vertex here to be processed for shadow calculations later
                o.normal = UnityObjectToWorldNormal(v.normal);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture (not used right now)
                fixed4 col = tex2D(_MainTex, i.uv);

                //lambert diffuse
                //get the direction of the light
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                //get the dot product of the vertex's normal and direction of light
                //to get a value for shadow, with a limit at 0 as you can have <0 light
                float diff = max(0.0, dot(i.normal, lightDir));

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                //apply the colour received
                return i.colour * _TerrainColour * diff;
            }
            ENDCG
        }
        

        //this pass renders the water
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            //struct for vertex data in vertex shader
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            //struct for vertex data in fragment shader
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

            //function that takes in a vector and processes it
            //with a bunch of random math functions and value
            //to produce a pseudo-random vector to return
            float2 hash(float2 pos)
            {
                const float2 k = float2(0.3183099, 0.3678794);

                float2 h = dot(pos, float2(127.1, 311.7));
                h = frac(sin(h) * k);

                return frac(h * 2.0 - 1.0);
            }

            //function that returns a wave height
            //uses the wave equation of 
            //Amplitude * sin( [WaveDir . Pos] * [WaveLength/2] * Time * [Speed * 2 / WaveLength] )
            float WaveState(float2 pos,float2 dir,float spd,float amp)
            {
                float height = (_Amp * amp) * sin(dot(dir,pos) * (_WaveL / 2) + _Time * (_Spd * spd * 2 / _WaveL));
                return height;
            }

            // SUM OF SINES
            //generates multiple waves with varying ampitude, frequency and speed
            //similar concept to fractional brownian motion
            //collates the height of all the waves and the position inputted in
            float SoS(float2 pos)
            {
                float frequency = 1;
                float spd = 1;
                float freqInfluence = 0.15;
                float height = 0;
                float amp = _Amp;

                //general gist is that as there are more waves,
                //each waves get smaller which means they are also faster
                //smaller waves has lesser amplitude and frequency
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

            //vertex shader
            v2f vert(appdata v)
            {
                v2f o;

                //change the height of the vertex using the Sum Of Sines method
                v.vertex.y = SoS(float2(v.vertex.x,v.vertex.z) * _Scale) + _WaterH;
                
                //normalizes the value and adjust the values
                float height = (v.vertex.y / _NumWaves * 2.5) - (_WaterH/3);

                //change the colour of the water depending on the height of the vertex
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

                //apply the colour
                return col * _WaterColour;
            }
            ENDCG
        }
    }
}
