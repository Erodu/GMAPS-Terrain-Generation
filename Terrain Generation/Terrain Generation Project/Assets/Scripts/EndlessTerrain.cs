using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//TAKING REFERENCE FROM SEBASTIAN LAGUE'S TERRAIN GENERATION PLAYLIST
//STRAYED OFF FROM EP08 OF THE PLAYLIST

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 300;
    public Transform player;
    public Transform meshGroup;
    public static Vector2 playerPosition;
    
    int chunkSize;
    int chunksVisibleInViewDistance;

    public MapGenerator mapGen;
    public Material material;
    public static Material mat;
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    private void Start()
    {
        mat = material;
        chunkSize = mapGen.nd.resolution - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        
    }

    private void Update()
    {
        playerPosition = new(player.position.x, player.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        //get the chunk's coordinate in relation to the centre
        // e.g. [0,0] is centre of the world
        // [-1, 1] [0, 1] [1, 1]
        // [-1, 0] [0, 0] [1, 0]
        // [-1,-1] [0,-1] [1,-1]
        int currentChunkCoordX = Mathf.RoundToInt(playerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(playerPosition.y / chunkSize);

        //get the surrounding chunk
        for(int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                // the chunk surrounding the chunk the player is on
                Vector2 viewedChunkCoord = new(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    //update the chunk to be visible
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();

                    //if it is visible already
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        //add it into the list of last visible chunks
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    //create a new instance of the chunk
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize,meshGroup,mapGen));
                    
                }
                    
            }
        }
    }


    public class TerrainChunk 
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size,Transform parent,MapGenerator mapG)
        {
            position = coord * size;
            bounds = new(position, Vector2.one * size);
            Vector3 positionV3 = new(position.x, 0, position.y);

            //creating a new plane and positioning it in the right spot
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            //meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;

            //generate the mesh for this chunk
            float[,] heightMap = Noise.Perlin.GenerateNoise(mapG.nd,position);
            MeshFilter mf = meshObject.GetComponent<MeshFilter>();
            MeshData meshData = MeshManipulator.GenerateTerrainMesh(heightMap, mapG.heightMultiplier, mapG.aniCurve, mapG.LOD);
            mf.sharedMesh = meshData.CreateMesh();
            meshObject.GetComponent<MeshCollider>().sharedMesh = mf.sharedMesh;

            Renderer rend = meshObject.GetComponent<Renderer>();
            rend.material = mat;

            SetVisible(false);
        }

        //find points on its perimeter clsoest to the player's position
        //find distance between that point and the player
        //and if distance < maxViewDistance, enable the mesh object
        //else it will be disabled
        public void UpdateTerrainChunk()
        {
            float playerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            bool visible = playerDistFromNearestEdge <= maxViewDistance;
            SetVisible(visible);

        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}


