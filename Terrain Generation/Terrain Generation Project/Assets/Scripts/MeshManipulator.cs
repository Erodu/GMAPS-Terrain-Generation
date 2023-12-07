using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshManipulator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap,float heightMultiplier,AnimationCurve heightCurve, int levelOfDetail)
    {
        //getting the resolution for each axis.
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //makes sure that the level of detail is always at least 1 as you cant divide by 0
        int meshSimplificationIncrement = (levelOfDetail <= 0) ? 1 : levelOfDetail;

        //calculate the amount of vertices based off the current LOD
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        //instancing the new meshdata with the data we have
        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        //creating the new meshdata with the new LOD
        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                //check if on edge?
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;

            }
        }

        return meshData;
    }


    //reduced stuff for testing
    public static MeshData GenerateTerrainMesh(int resolution, int levelOfDetail)
    {
        //getting the resolution for each axis.
        int width = resolution;
        int height = resolution;
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        //makes sure that the level of detail is always at least 1 as you cant divide by 0
        int meshSimplificationIncrement = (levelOfDetail <= 0) ? 1 : levelOfDetail;

        //calculate the amount of vertices based off the current LOD
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        //instancing the new meshdata with the data we have
        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        //creating the new meshdata with the new LOD
        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, 0, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                //check if on edge?
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;

            }
        }

        return meshData;
    }
}

//Mesh data class to have all the data and function needed to manipulate the mesh
public class MeshData
{
    //holds all the vertices
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;
    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    //creates the triangle of the mesh
    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}