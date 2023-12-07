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
            float noiseScale = nd.noiseScale;

            System.Random prng = new(nd.seed);
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            float halfWidth = resolution / 2f;
            float halfHeight = resolution / 2f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float noiseHeight = 0;

                    float sampleX = (x - halfWidth) / noiseScale + offset.x;
                    float sampleY = (y - halfHeight) / noiseScale + offset.y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 -1;
                    noiseHeight += perlinValue;

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

            return noiseMap;
        }
    }
}

[Serializable]
public struct NoiseData
{
    public int seed;
    public int resolution;
    public Vector2 offset;
    public float noiseScale;

}
