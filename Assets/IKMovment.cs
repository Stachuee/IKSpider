using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class IKMovment : MonoBehaviour
{
    [System.Serializable]
    public enum LegSidePosition { Left, Right }
    [System.Serializable]
    public enum LegFrontPosition { Front, Back }

    [System.Serializable]
    struct Leg
    {
        //Local space
        public Vector3 destination;
        public Vector3 root;
        public Vector3 desiredPos;
        public LegSidePosition legSide;
        public LegFrontPosition legFront;


        [HideInInspector]
        public Vector3 desirePosInWorld;
        

        public float maxAcceptableLegDistanceFromDesire;

        public float[] bonesLength;

        [HideInInspector]
        public Transform[] segments;
        [HideInInspector]
        public Vector3[] bones;
        [HideInInspector]
        public float allLength;
    }

    [SerializeField]
    Transform spider;

    [SerializeField]
    GameObject segmentPrefab;

    [SerializeField]
    Leg[] legs;

    [SerializeField]
    float delta;
    [SerializeField]
    int iterations;


    [SerializeField]
    float speed;
    [SerializeField]
    float desireHeigth;

    private void Awake()
    {
        Innit();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = spider.forward * vertical + spider.right * horizontal;
        input = input.magnitude > 1 ? input.normalized : input;
        spider.position += input * speed * Time.deltaTime;

        MoveLegsTargetPosition();
        MoveBodyTargetPosition();
        CalculateIK();
        AnimateLegs();
    }


    private void Innit()
    {
        for(int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            legs[legNumber].bones = new Vector3[legs[legNumber].bonesLength.Length + 1];
            legs[legNumber].segments = new Transform[legs[legNumber].bonesLength.Length];

            Vector3 dir = (legs[legNumber].destination - legs[legNumber].root).normalized;
            //Debug.Log(legs[legNumber].bones[legNumber]);
            legs[legNumber].bones[0] = legs[legNumber].root;
            Vector3 dist = Vector3.zero;

            for (int i = 1; i < legs[legNumber].bones.Length; i++)
            {
                legs[legNumber].bones[i] = legs[legNumber].root + dir * legs[legNumber].bonesLength[i - 1] + dist;
                dist += dir * legs[legNumber].bonesLength[i - 1];
            }
            legs[legNumber].allLength = dist.magnitude;

            for(int i = 0; i < legs[legNumber].bonesLength.Length; i++)
            {
                GameObject temp = Instantiate(segmentPrefab, transform.position, quaternion.identity);
                legs[legNumber].segments[i] = temp.transform;
            }
        }
        
        //legs[0].bones = new Vector3[legs[0].bonesLength.Length + 1];
        //legs[1].bones = new Vector3[legs[1].bonesLength.Length + 1];


        //Vector3 dirLeft = (legs[0].destination - legs[0].root).normalized;
        //Vector3 dirRight = (legs[1].destination - legs[1].root).normalized;

        //legs[0].bones[0] = legs[0].root;
        //legs[1].bones[0] = legs[1].root;

        //Vector3 dist = Vector3.zero;
        //for (int i = 1; i < legs[0].bones.Length; i++)
        //{
        //    legs[0].bones[i] = legs[0].root + dirLeft * legs[0].bonesLength[i - 1] + dist;
        //    dist += dirLeft * legs[0].bonesLength[i - 1];
        //}
        //legs[0].allLength = dist.magnitude;

        //dist = Vector3.zero;
        //for (int i = 1; i < legs[0].bones.Length; i++)
        //{
        //    legs[1].bones[i] = legs[1].root + dirRight * legs[1].bonesLength[i - 1] + dist;
        //    dist += dirRight * legs[1].bonesLength[i - 1];
        //}
        //legs[1].allLength = dist.magnitude;
    }
    private void MoveLegsTargetPosition()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s¹ dla ka¿dej nogi osobno
        {
            RaycastHit hit;

            if (Physics.Raycast(legs[legNumber].desiredPos + spider.position, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {   
                if(Vector3.Distance(legs[legNumber].desirePosInWorld, hit.point) > legs[legNumber].maxAcceptableLegDistanceFromDesire)
                {
                    legs[legNumber].desirePosInWorld = hit.point;
                }

            }
        }
    }

    private void MoveBodyTargetPosition()
    {
        float heigth = 0;

        int leftCount = 0;
        Vector3 leftSideHeigth = Vector3.zero;
        int rightCount = 0;
        Vector3 rightSideHeigth = Vector3.zero;
        int frontCount = 0;
        Vector3 frontHeigth = Vector3.zero;
        int backCount = 0;
        Vector3 backHeigth = Vector3.zero;

        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s¹ dla ka¿dej nogi osobno
        {
            Vector3 myHeigth = (spider.position - legs[legNumber].desirePosInWorld);
            myHeigth = new Vector3(myHeigth.x, desireHeigth - myHeigth.y, myHeigth.z);
            heigth += myHeigth.y;

            //float myHeigth = desireHeigth - (spider.position - legs[legNumber].desirePosInWorld).y;
            //heigth += myHeigth;

            if (legs[legNumber].legSide == LegSidePosition.Left) { leftSideHeigth += myHeigth; leftCount++; }
            else if (legs[legNumber].legSide == LegSidePosition.Right) { rightSideHeigth += myHeigth; rightCount++; }
            if (legs[legNumber].legFront == LegFrontPosition.Front) { frontHeigth += myHeigth; frontCount++; }
            else if (legs[legNumber].legFront == LegFrontPosition.Back) { backHeigth += myHeigth;  backCount++; }

        }
        
        heigth /= legs.Length;

        leftSideHeigth /= leftCount;
        rightSideHeigth /= rightCount;
        backHeigth /= backCount;
        frontHeigth /= frontCount;

        
        spider.position += new Vector3(0, heigth, 0);
    }

    private void CalculateIK()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s¹ dla ka¿dej nogi osobno
        {
            //  Jeœli odleg³oœæ od celu jest wiêksza ni¿ d³ugoœæ nogi obliczamy kierunek w którym powinna byæ wyci¹gniêta a nastêpnie ustawiamy koœci prosto w kierunku celu z wyj¹tkiem pierwszej, która jest rootem
            if (Vector3.Magnitude(legs[legNumber].destination - legs[legNumber].root) > legs[legNumber].allLength) 
            {
                Vector3 dir = (legs[legNumber].destination - legs[legNumber].root).normalized;
                Vector3 dist = Vector3.zero;

                for(int bone = 1; bone < legs[legNumber].bones.Length; bone++)
                {
                    legs[legNumber].bones[bone] = legs[legNumber].root + dir * legs[legNumber].bonesLength[bone - 1] + dist;
                    dist += dir * legs[legNumber].bonesLength[bone - 1];
                }

            }
            else
            {
                for (int iterationsCount = 0; iterationsCount < iterations; iterationsCount++)
                {
                    legs[legNumber].bones[legs[legNumber].bones.Length - 1] = legs[legNumber].desirePosInWorld - spider.position;

                    for(int boneIndex = legs[legNumber].bones.Length - 2; boneIndex >= 0; boneIndex--) //back
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex + 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex + 1]).normalized * legs[legNumber].bonesLength[boneIndex];
                    }

                    
                    legs[legNumber].bones[0] = legs[legNumber].root;

                    for (int boneIndex = 1; boneIndex < legs[legNumber].bones.Length - 1; boneIndex++) // forward
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex - 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex - 1]).normalized * legs[legNumber].bonesLength[boneIndex - 1];
                    }

                    if (Vector3.Distance(legs[legNumber].bones[legs[legNumber].bones.Length - 1], legs[legNumber].destination) < delta) break;

                }
            }
        }
    }

    private void AnimateLegs()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            for (int i = 0; i < legs[legNumber].segments.Length; i++)
            {
                legs[legNumber].segments[i].position = legs[legNumber].bones[i] + spider.position;
                legs[legNumber].segments[i].rotation = Quaternion.LookRotation((legs[legNumber].bones[i] - legs[legNumber].bones[i + 1]), Vector3.up);
            }
        }
    }

    private void OnDrawGizmos()
    {

        for (int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(legs[legNumber].root + spider.position, 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(legs[legNumber].destination + spider.position, 0.1f);
            
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(legs[legNumber].desirePosInWorld, 0.2f);

            Gizmos.color = Color.yellow;
            foreach (Vector3 joint in legs[legNumber].bones)
            {
                Gizmos.DrawSphere(joint + spider.position, 0.1f);
            }

            for (int i = 1; i < legs[legNumber].bones.Length; i++)
            {
                Gizmos.DrawLine(legs[legNumber].bones[i - 1] + spider.position, legs[legNumber].bones[i] + spider.position);
            }

            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(legs[legNumber].desiredPos + spider.position, legs[legNumber].desiredPos + spider.position - new Vector3(0,2,0));
        }

      

    }
}
