using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooting : MonoBehaviour
{
    [SerializeField]
    Transform barrel;
    [SerializeField]
    Transform turret;
    [SerializeField]
    GameObject bulletPrefab;


    [SerializeField]
    float delayBetweenShots;
    float delayRemain;

    [SerializeField]
    Renderer vision;
    bool vison;

    [SerializeField]
    Renderer barrelMat;

    [SerializeField]
    float heatDispersionPerSecond;
    [SerializeField]
    float heatGenerationPerShot;
    float heat;


    private void Update()
    {
        if(Input.GetMouseButton(0) && delayRemain < 0)
        {
            delayRemain = delayBetweenShots;
            Instantiate(bulletPrefab, barrel.position, Quaternion.LookRotation(barrel.position - turret.position, turret.up));
            heat += heatGenerationPerShot;
        }
        if(Input.GetKeyDown(KeyCode.N))
        {
            if(vison)
            {
                vision.material.SetFloat("_Thermal", 0);
            }
            else
            {
                vision.material.SetFloat("_Thermal", 1);
            }
            vison = !vison;

        }
        delayRemain -= Time.deltaTime;
        heat = Mathf.Clamp(heat - heatDispersionPerSecond * Time.deltaTime, 0, 1);
        barrelMat.material.SetFloat("_HeatLevel", heat);
    }

}
