using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SnowManager : MonoBehaviour
{
    List<MeshRenderer> chunkMeshes;

    [SerializeField]
    int2 mapChunkSize;
    [SerializeField]
    int2 chunkSize;
    [SerializeField]
    GameObject plane;


    private void Start()
    {
        for(int y = 0; y < mapChunkSize.y; y++)
        {
            for (int x = 0; x < mapChunkSize.x; x++)
            {
                Instantiate(plane, new Vector3(x * chunkSize.x, 0, y * chunkSize.y), Quaternion.identity);
            }
        }
    }

}
