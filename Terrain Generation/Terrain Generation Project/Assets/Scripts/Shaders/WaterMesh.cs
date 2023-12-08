using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMesh : MonoBehaviour
{
    MeshFilter mf;
    public Transform player;

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        MeshData md = MeshManipulator.GenerateTerrainMesh(256, 1);
        mf.sharedMesh = md.CreateMesh();
    }

    private void Update()
    {
        transform.position = new(player.position.x, transform.position.y, player.position.z);
    }
}
