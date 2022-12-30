using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SnowManager : MonoBehaviour
{
    Texture2D snow;

    [SerializeField]
    int2 mapSize;

    [SerializeField]
    Material mat;

    private void Awake()
    {
        snow = new Texture2D(mapSize.x, mapSize.y);

        for(int y =0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.y; x++)
            {
                snow.SetPixel(x, y, Color.white);
            }
        }
    }

    private void Update()
    {
        mat.SetTexture("_Deformation", snow);
    }
}
