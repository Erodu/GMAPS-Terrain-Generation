using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static class Perlin
    {
        public static float[,] GenerateNoise(NoiseData nd,Vector2 offset)
        {
            float[,] noiseMap = new float[nd.resolution,nd.resolution];
            int resolution = nd.resolution;

            System.Random prng = new(nd.seed);

            Vector2[] octaveOffsets = new Vector2[nd.octaves];

            for (int i = 0; i < nd.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + nd.offset.x;
                float offsetY = prng.Next(-100000, 100000) + nd.offset.y;
                octaveOffsets[i] = new(offsetX, offsetY);
            }

            if (nd.noiseScale <= 0)
            {
                nd.noiseScale = 0.0001f;
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = resolution / 2f;
            float halfHeight = resolution / 2f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {

                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;


                    for (int i = 0; i < nd.octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / nd.noiseScale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / nd.noiseScale * frequency + octaveOffsets[i].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= nd.persistance;
                        frequency *= nd.lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }


            return noiseMap;
        }
    }
}

[Serializable]
public struct NoiseData
{
    public int seed;
    public int resolution;
    public int octaves;
    public Vector2 offset;
    public float noiseScale;
    public float persistance;
    public float lacunarity;

}
