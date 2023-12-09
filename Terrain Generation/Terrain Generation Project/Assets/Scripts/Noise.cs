using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static class Perlin
    {
        //using perlin noise to generate a 2d heightmap
        public static float[,] GenerateNoise(NoiseData nd,Vector2 offset)
        {
            float[,] noiseMap = new float[nd.resolution,nd.resolution];
            int resolution = nd.resolution;

            System.Random prng = new(nd.seed);
            Vector2[] octaveOffsets = new Vector2[nd.octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < nd.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + nd.offset.x + offset.x;
                float offsetY = prng.Next(-100000, 100000) - nd.offset.y - offset.y;
                octaveOffsets[i] = new(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= nd.persistance;

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
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < nd.octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / nd.noiseScale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / nd.noiseScale * frequency;

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
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight);
                    noiseMap[x, y] = normalizedHeight;
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
