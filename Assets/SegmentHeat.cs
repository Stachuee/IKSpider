using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentHeat : MonoBehaviour
{
    [SerializeField]
    Renderer myRenderer;
    public void UpdateHeat(float heat)
    {
        myRenderer.material.SetFloat("_HeatLevel", heat);
    }
}
