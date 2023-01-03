using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [SerializeField]
    float sphereRadius;
    [SerializeField]
    Vector3 middleOfArena;

    [SerializeField]
    float spawnDelay;

    [SerializeField]
    GameObject target;

    float targets = 100;

    private void Start()
    {
        StartCoroutine("Spawn");
    }

    IEnumerator Spawn()
    {
        while(targets > 0)
        {
            yield return new WaitForSeconds(spawnDelay);
            Vector3 randomPos = Random.insideUnitSphere;
            Vector3 pos = new Vector3(randomPos.x, Mathf.Abs(randomPos.y), randomPos.z) * sphereRadius + middleOfArena;
            Instantiate(target, pos, Quaternion.LookRotation(pos - middleOfArena));
            targets--;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(middleOfArena, sphereRadius);
    }
}
