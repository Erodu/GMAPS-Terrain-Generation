using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public NoiseData NoiseData;
    public int LOD;
    public float heightMultiplier;
    public AnimationCurve aniCurve;

    public MeshFilter mf;


    public void GenerateMap()
    {
        float[,] heightMap;

        //check the noise type and do the appropriate function
        switch (NoiseData.noiseType)
        {
            case NoiseType.PERLIN:
                heightMap = Noise.Perlin.GenerateNoise(NoiseData, new Vector2(0, 0));
                break;

            case NoiseType.WORLEY:
                heightMap = Noise.Worley.GenerateNoise(NoiseData, new Vector2(0, 0));
                break;

            default:
                heightMap = Noise.Perlin.GenerateNoise(NoiseData, new Vector2(0, 0));
                break;
        }

        Color[] colorMap = new Color[NoiseData.resolution * NoiseData.resolution];

        DrawMesh(MeshManipulator.GenerateTerrainMesh(heightMap, heightMultiplier, aniCurve, LOD));
    }

    public void DrawMesh(MeshData meshData)
    {
        mf.sharedMesh = meshData.CreateMesh();
    }

    private void OnValidate()
    {
        if(mf.gameObject.activeSelf)
            GenerateMap();
    }
}


public struct MapData
{
    float[,] heightMap;
    Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
