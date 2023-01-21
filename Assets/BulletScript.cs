using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    [SerializeField]
    GameObject explosionPrefab;
    [SerializeField]
    float lifeDuration;
    float lifeDurationRemain;

    [SerializeField]
    float speed;
    Vector3 prevPos;

    [SerializeField]
    Vector3 gravity;

    private void Start()
    {
        lifeDurationRemain = Time.time + lifeDuration;
        prevPos = transform.position;
    }

    private void Update()
    {
        if (lifeDurationRemain < Time.time) Destroy(gameObject);


        transform.position += (transform.forward * speed + gravity) * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(transform.position - prevPos);
        RaycastHit hit;
        if (Physics.Linecast(prevPos, transform.position, out hit))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.DestroyMe();
            }
            Explode(hit.point);
            Destroy(gameObject);
        }

        prevPos = transform.position;
    }

    void Explode(Vector3 pos)
    {
        Destroy(Instantiate(explosionPrefab, pos, Quaternion.identity), 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, prevPos);
    }
}
