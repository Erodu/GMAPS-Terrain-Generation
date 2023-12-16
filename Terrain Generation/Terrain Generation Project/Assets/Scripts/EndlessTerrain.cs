using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//TAKING REFERENCE FROM SEBASTIAN LAGUE'S TERRAIN GENERATION PLAYLIST
//STRAYED OFF FROM EP08 OF THE PLAYLIST

public class EndlessTerrain : MonoBehaviour
{
    // Using struct LODInfo
    public LODInfo[] levelsOfDetail;
    public static float maxViewDistance;
    public Transform player;
    public Transform meshGroup;
    public static Vector2 playerPosition;
    int chunkSize;
    int chunksVisibleInViewDistance;

    public MapGenerator mapGen;
    public Material material;
    public static Material mat;

    //keeps all the record of all the chunks that has been generated
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new();
    //keep the list of all the chunk that was visible
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    private void Start()
    {
        mat = material;
        // Set the maximum view distance to the last element in levelsOfDetail's viewDistance.
        maxViewDistance = levelsOfDetail[levelsOfDetail.Length - 1].viewDistance;
        //get the chunk's information
        chunkSize = mapGen.NoiseData.resolution - 1;
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
        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
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
                    //create a new instance of the chunk and add it into the dictionary
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, meshGroup, mapGen, levelsOfDetail));
                }
            }
        }
    }


    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        LODInfo[] levelsOfDetail;
        int LODCurrent;
        MapGenerator mapG;
        public TerrainChunk(Vector2 coord, int size, Transform parent, MapGenerator mapG, LODInfo[] levelsOfDetail)
        {
            this.levelsOfDetail = levelsOfDetail;
            this.mapG = mapG;
            position = coord * size;
            bounds = new(position, Vector2.one * size);
            Vector3 positionV3 = new(position.x, 0, position.y);

            //creating a new plane and positioning it in the right spot
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            //meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;

            //generate the mesh for this chunk
            float[,] heightMap = Noise.Perlin.GenerateNoise(mapG.NoiseData, position); // generate height map for the terrain

            MeshFilter mf = meshObject.GetComponent<MeshFilter>();
            //use the height map to adjust the vertices of the mesh to match the height map's value
            MeshData meshData = MeshManipulator.GenerateTerrainMesh(heightMap, mapG.heightMultiplier, mapG.aniCurve, levelsOfDetail[LODCurrent].detailLevel);
            //create the mesh and apply it to the object
            mf.sharedMesh = meshData.CreateMesh();
            //update the collider
            meshObject.GetComponent<MeshCollider>().sharedMesh = mf.sharedMesh;

            //apply the material to the mesh
            Renderer rend = meshObject.GetComponent<Renderer>();
            rend.material = mat;

            SetVisible(false);
        }

        int CheckLOD()
        {
            float playerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            // Check if player's distance is greater than the view distance within LODInfo[].
            // If so, return its corresponding detailLevel.
            // Otherwise, we return the last level of detail in the array.
            for (int i = 0; i < levelsOfDetail.Length; i++)
            {
                if (playerDistFromNearestEdge > levelsOfDetail[i].viewDistance)
                {
                    return i;
                }
            }
            return levelsOfDetail.Length - 1;
        }

        //find points on its perimeter clsoest to the player's position
        //find distance between that point and the player
        //and if distance < maxViewDistance, enable the mesh object
        //else it will be disabled

        /// <summary>
        /// Before doing all of that, we first check to see if we need to
        /// update the level of detail on the map. If the new level of detail
        /// does not match our current one, we update it appropriately. We can
        /// check the new LOD with the CheckLOD() function.
        /// </summary>
        public void UpdateTerrainChunk()
        {
            int LODCheck = CheckLOD();
            if (LODCheck != LODCurrent)
            {
                LODCurrent = LODCheck;
                LODUpdate();
            }
            float playerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            bool visible = playerDistFromNearestEdge <= maxViewDistance;
            SetVisible(visible);

        }

        void LODUpdate()
        {
            float[,] heightMap = Noise.Perlin.GenerateNoise(mapG.NoiseData, position); // generate height map for the terrain

            MeshFilter mf = meshObject.GetComponent<MeshFilter>();
            //use the height map to adjust the vertices of the mesh to match the height map's value
            MeshData meshData = MeshManipulator.GenerateTerrainMesh(heightMap, mapG.heightMultiplier, mapG.aniCurve, levelsOfDetail[LODCurrent].detailLevel);
            //create the mesh and apply it to the object
            mf.sharedMesh = meshData.CreateMesh();
            //update the collider
            meshObject.GetComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
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
    // This struct contains information for different levels of detail.
    [System.Serializable]
    public struct LODInfo
    {
        public int detailLevel;
        public float viewDistance;
    }

    //class LODMesh
    //{
    //    public Mesh mesh;
    //    public bool hasRequestedMesh;
    //    public bool hasMesh;
    //    int LOD;

    //    public LODMesh(int LOD)
    //    {
    //        this.LOD = LOD;
    //    }

    //    public void RequestMesh(MapData mapData)
    //    {
    //        hasRequestedMesh = true;
    //        MapGenerator.
    //    }
    //}
}