using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    THIS MESH GENERATION IS BASED OFF SEBASTIAN LAGUE'S TUTORIAL ON TERRAIN GENERATION.
    IT HAS BEEN MODIFIED AND SIMPLIFIED AS THE OTHER FEATURES ARE NOT BEING USED.
*/
public class MeshManipulator : MonoBehaviour
{
    //resolution of the mesh
    public int Resolution;
    
    //LEVEL OF DETAIL HAS TO BE A FACTOR OF THE RESOLUTION FOR IT TO WORK OR IT WILL BE OUT OF BOUNDS
    public int LOD;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public MeshData GenerateTerrainMesh(int resolution, int levelOfDetail)
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

    //updates the mesh to the new mesh created
    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

    }

    //runs on the editor so there is no need to enter play mode
    void OnValidate()
    {
        MeshData md = GenerateTerrainMesh(Resolution, LOD);
        //DrawMesh(md);
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