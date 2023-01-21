using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderControll : MonoBehaviour
{
    BorderBox borderBox;
    [SerializeField]
    Transform spider;

    [SerializeField]
    Material borderControllMat;

    [SerializeField]
    GameObject text;
    private void Start()
    {
        borderBox = BorderBox.borderBox;
    }


    private void Update()
    {
        if (!PointInOABB(spider.position, borderBox.center, borderBox.size))
        {
            borderControllMat.SetFloat("_TooFarFromTarget", 1);
            text.SetActive(true);
            if(!PointInOABB(spider.position, borderBox.center, borderBox.killSize))
            {
                GameManager.gameManager.KillPlayer();
            }
        }
        else
        {
            borderControllMat.SetFloat("_TooFarFromTarget", 0);
            text.SetActive(false);
        }
    }


    bool PointInOABB(Vector3 point, Vector3 boxPos, Vector3 boxSize)
    {
        float halfX = (boxSize.x * 0.5f);
        float halfY = (boxSize.y * 0.5f);
        float halfZ = (boxSize.z * 0.5f);
        if (point.x < halfX + boxPos.x && point.x > -halfX + boxPos.x &&
           point.y < halfY + boxPos.y && point.y > -halfY + boxPos.y &&
           point.z < halfZ + boxPos.z && point.z > -halfZ + boxPos.z)
            return true;
        else
            return false;
    }
}
