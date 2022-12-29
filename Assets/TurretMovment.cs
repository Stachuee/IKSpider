using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretMovment : MonoBehaviour
{
    [SerializeField]
    Transform turret;

    [SerializeField, Range(1, 50)]
    private float sensitivity;


    private float rotationX;
    private float rotationY;

    private void Update()
    {
        float axisX = Input.GetAxis("Mouse X") * sensitivity;
        float axisY = -Input.GetAxis("Mouse Y") * sensitivity;

        rotationX += axisX;
        rotationY += axisY;

        Vector3 newRotation = new Vector3(rotationY, rotationX, 0);

        turret.localEulerAngles = newRotation;
    }
}
