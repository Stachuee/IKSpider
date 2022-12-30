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
        transform.position = turret.position + camOffset.x * -turret.forward + camOffset.y * turret.up;
        transform.rotation = Quaternion.LookRotation((turret.position + camOffset.y * turret.up) - transform.position, turret.up);
    }
}
