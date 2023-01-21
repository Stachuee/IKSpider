using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public void DestroyMe()
    {
        TargetCounter.destroyedTargets++;
        Destroy(gameObject);
    }
}
