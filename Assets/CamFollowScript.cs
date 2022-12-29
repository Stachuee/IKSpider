using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowScript : MonoBehaviour
{
    [SerializeField]
    Transform turret;
    [SerializeField]
    Vector2 camOffset;

    private void Update()
    {
        transform.position = turret.position + camOffset.x * -turret.forward;// + camOffset.x * -turret.forward + new Vector3(0,camOffset.y,0);
        transform.rotation = Quaternion.LookRotation(turret.position - transform.position, turret.up);
    }
}
