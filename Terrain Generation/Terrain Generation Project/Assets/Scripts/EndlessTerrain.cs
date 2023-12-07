using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//TAKING REFERENCE FROM SEBASTIAN LAGUE'S TERRAIN GENERATION PLAYLIST

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 300;
    public MeshManipulator meshManipulator;
    public Transform player;
    public Transform meshGroup;
    public Material material;
    public Shader shader;
    public static Vector2 playerPosition;
    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    private void Start()
    {
        chunkSize = meshManipulator.Resolution - 1;
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
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize,meshGroup));
                    
                }
                    
            }
        }
    }


    public class TerrainChunk 
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        NoiseData nd;
        public TerrainChunk(Vector2 coord, int size,Transform parent)
        {
            position = coord * size;
            bounds = new(position, Vector2.one * size);
            Vector3 positionV3 = new(position.x, 0, position.y);

            //creating a new plane and positioning it in the right spot
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;

            /* for shaders (currently not really working) 
            Renderer rend = meshObject.GetComponent<Renderer>();
            rend.material = mat;
            rend.material.SetVector("_Offset",new(position.x,position.y));
            */

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


