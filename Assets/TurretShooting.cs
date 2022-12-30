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


    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && delayRemain < 0)
        {
            delayRemain = delayBetweenShots;
            Instantiate(bulletPrefab, barrel.position, Quaternion.LookRotation(barrel.position - turret.position, turret.up));
        }
        delayRemain -= Time.deltaTime;
    }

}
