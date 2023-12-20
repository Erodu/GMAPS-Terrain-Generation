using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


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

    public static class Worley
    {
        //similar concept to the one in perlin noise but with a different noise function
        public static float[,] GenerateNoise(NoiseData nd, Vector2 offset)
        {
            float[,] noiseMap = new float[nd.resolution, nd.resolution];
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
            //value using the worley noise function
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    //goes through all the octaves and adds up the noise values
                    //generated from the worley noise function
                    for (int i = 0; i < nd.octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[i].x) / nd.noiseScale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[i].y) / nd.noiseScale * frequency;

                        float perlinValue = WorleyFunction(new Vector2(sampleX,sampleY),nd.seed) * 2 - 1;
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

        static float WorleyFunction(Vector2 samplePos,int seed)
        {
            //represents the current sector the pixel is in
            Vector2 floor_pixel = Vector2Int.FloorToInt(samplePos);
            //represents the position inside the sector in which the pixel is in
            Vector2 frac_pixel = new(samplePos.x - floor_pixel.x , samplePos.y - floor_pixel.y);

            float min_dist = float.MaxValue;

            //goes through the surrounding sectors to check which random point
            //the sample pixel is closest to
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector2 neighbour = new(x, y);

                    //the sector being checked
                    float2 sector = neighbour + floor_pixel;

                    //get a random vector based off the current sector
                    Vector2 randpoint = RandomVector2(sector,seed);

                    //get the difference
                    Vector2 diff = neighbour + randpoint - frac_pixel;

                    //distance between the pixel and the point checked
                    float dist = diff.magnitude;

                    //check if the distance is smaller than the current minimun distance
                    min_dist = Mathf.Min(min_dist, dist);
                }
            }

            return min_dist;
        }

        //very ugly way to generate a pseudo-random point within the sector
        //works by randomly using math functions to increasingly randomise
        //the value whilst making sure that the value inputted is consistent so that
        //the return value is always the same with the same input
        static Vector2 RandomVector2(Vector2 p, int seed)
        {
            return math.frac(math.sin(new float2(math.dot(p, new float2(seed, seed - 0.361f)), math.dot(p, new float2(seed / 5.251f, seed + 183.3f)))) * 43758.5453f);
        }
    }

}

[Serializable]
public struct NoiseData
{
    public NoiseType noiseType;
    public int seed;
    public int resolution;
    public int octaves;
    public Vector2 offset;
    public float noiseScale;
    public float persistance;
    public float lacunarity;
}


public enum NoiseType
{
    PERLIN,
    WORLEY
}
