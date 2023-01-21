using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderBox : MonoBehaviour
{
    public static BorderBox borderBox;


    [SerializeField]
    public Vector3 center;
    [SerializeField]
    public Vector3 size;
    [SerializeField]
    public Vector3 killSize;

    private void Awake()
    {
        borderBox = this;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(center, killSize);
    }

}
