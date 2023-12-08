using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public NoiseData nd;
    public int LOD;
    public float heightMultiplier;
    public AnimationCurve aniCurve;

    public MeshFilter mf;


    public void GenerateMap()
    {
        float[,] heightMap = Noise.Perlin.GenerateNoise(nd,new Vector2(0,0));
        Color[] colorMap = new Color[nd.resolution * nd.resolution];

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
