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
            //declaring the noise map array size
            float[,] noiseMap = new float[nd.resolution,nd.resolution];
            int resolution = nd.resolution;

            //initializing the random number generator with a seed
            System.Random prng = new(nd.seed);

            
            Vector2[] octaveOffsets = new Vector2[nd.octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;

            //create random offsets for each octave so the resulting layers
            //of noise is more random
            for (int i = 0; i < nd.octaves; i++)
            {
                //make sure the actual offset is applied to make the noise tile
                //more coherently together
                float offsetX = prng.Next(-100000, 100000) + nd.offset.x + offset.x;
                float offsetY = prng.Next(-100000, 100000) - nd.offset.y - offset.y;
                octaveOffsets[i] = new(offsetX, offsetY);

                //get the highest possible height value which is going to be used later
                //to normalize the values
                maxPossibleHeight += amplitude;
                amplitude *= nd.persistance;
            }

            //make sure the scale is not 0
            if (nd.noiseScale <= 0)
            {
                nd.noiseScale = 0.0001f;
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            //gets the center of the map so scaling
            //can start from the middle of the map instead of
            //the corners
            float halfWidth = resolution / 2f;
            float halfHeight = resolution / 2f;

            //goes through each pixel in the map and generates a
            //value using the perlin noise function from Unity
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    //goes through all the octaves and adds up the noise values
                    //generated from the perlin noise function
                    for (int i = 0; i < nd.octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / nd.noiseScale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / nd.noiseScale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        //persistance affect the strength change in each octave
                        //generally the higher octave, the lower the strength
                        amplitude *= nd.persistance;

                        //lacunarity affects the scale of the noise in each octave
                        //generally the higher octave, the smaller the scale
                        frequency *= nd.lacunarity;
                    }

                    //change the value of the max/min noise accordingly
                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }

                    //assign the value received back into the noise map
                    noiseMap[x, y] = noiseHeight;
                }
            }

            //go through the map and normalize all the value
            //so each chunk can tile properly with minimal visible seams
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
