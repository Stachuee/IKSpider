using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowScript : MonoBehaviour
{
    [SerializeField]
    Transform turret;
    [SerializeField]
    Vector3 camOffset;

    private void Update()
    {
        transform.position = turret.position + camOffset.x * -turret.forward + camOffset.y * turret.up + camOffset.z * turret.right;
        transform.rotation = Quaternion.LookRotation((turret.position + camOffset.y * turret.up + camOffset.z * turret.right) - transform.position, turret.up);
    }
}
