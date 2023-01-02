using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class IKMovment : MonoBehaviour
{
    [System.Serializable]
    struct Leg
    {
        //Local space
        public Vector3 destination;
        public Vector3 root;
        [HideInInspector]
        public Vector3 translatedRoot;
        public Vector3 desiredPos;
        [HideInInspector]
        public Vector3 translatedDesiredPos;

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

        [HideInInspector]
        public Vector3 currentPosition;
        [HideInInspector]
        public Vector3 startPosition;
        [HideInInspector]
        public float moveStartTime;
        [HideInInspector]
        public float moveDuration;
        [HideInInspector]
        public Vector3 moveInterpolationPoint;
        [HideInInspector]
        public bool isMoving;
    }

    [SerializeField]
    Transform spider;
    [SerializeField]
    Transform turret;

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

    [SerializeField]
    float legMoveSpeed;
    [SerializeField]
    float legMoveHeight;

    Vector3 movmentAxis;

    Vector3 desiredRotation;
    Vector3 desiredPosition;

    Vector3 rayCastLeanOffset;


    public float smoothTime = 0.05F;
    private Vector3 velocity = Vector3.zero;


    [SerializeField]
    Renderer body;

    [SerializeField]
    float heatPerSecond;
    [SerializeField]
    float heatDispersePerSecond;
    float heat;

    private void Awake()
    {
        Innit();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movmentAxis = spider.forward * vertical + spider.right * horizontal;

        Vector3 forwardMoveDirection = Vector3.Project(turret.forward, Vector3.ProjectOnPlane(turret.forward, spider.up).normalized).normalized;
        Vector3 rightMoveDirection = Vector3.Project(turret.right, Vector3.ProjectOnPlane(turret.right, spider.up).normalized).normalized;


        Vector3 input = forwardMoveDirection * vertical + rightMoveDirection * horizontal;
        input = input.magnitude > 1 ? input.normalized : input;
        spider.position += input * speed * Time.deltaTime;

        TranslatePoitns();
        MoveLegsTargetPosition();
        MoveBodyTargetPosition();
        spider.up = Vector3.SmoothDamp(spider.up, desiredRotation, ref velocity, smoothTime);
        spider.position = Vector3.SmoothDamp(spider.position, desiredPosition, ref velocity, smoothTime);
        CalculateLegsPosition();
        CalculateIK();
        SetLegPosition();

        heat = Mathf.Clamp(heat + (movmentAxis.magnitude * heatPerSecond - heatDispersePerSecond) * Time.deltaTime , 0, 1);
        body.material.SetFloat("_HeatLevel", heat);
    }


    private void Innit()
    {
        for(int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            legs[legNumber].bones = new Vector3[legs[legNumber].bonesLength.Length + 1];
            legs[legNumber].segments = new Transform[legs[legNumber].bonesLength.Length];

            Vector3 dir = -spider.up;
            //Debug.Log(legs[legNumber].bones[legNumber]);
            legs[legNumber].bones[0] = legs[legNumber].translatedRoot;
            Vector3 dist = Vector3.zero;

            for (int i = 1; i < legs[legNumber].bones.Length; i++)
            {
                legs[legNumber].bones[i] = legs[legNumber].translatedRoot + dir * legs[legNumber].bonesLength[i - 1] + dist;
                dist += dir * legs[legNumber].bonesLength[i - 1];
            }
            legs[legNumber].allLength = dist.magnitude;

            for(int i = 0; i < legs[legNumber].bonesLength.Length; i++)
            {
                GameObject temp = Instantiate(segmentPrefab, transform.position, quaternion.identity);
                legs[legNumber].segments[i] = temp.transform;
                legs[legNumber].startPosition = legs[legNumber].bones[i] + spider.position;
                legs[legNumber].desirePosInWorld = legs[legNumber].bones[i] + spider.position;
            }
        }
        
    }

    private void TranslatePoitns()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s¹ dla ka¿dej nogi osobno
        {
            legs[legNumber].translatedDesiredPos = spider.TransformPoint(legs[legNumber].desiredPos) - spider.position;
            legs[legNumber].translatedRoot = spider.TransformPoint(legs[legNumber].root) - spider.position;
        }
    }

    private void MoveLegsTargetPosition()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s¹ dla ka¿dej nogi osobno
        {

            RaycastHit hit;
            Vector3 posFromCenterOfSphere = Vector3.positiveInfinity;
            if (Physics.Raycast(legs[legNumber].translatedDesiredPos + spider.position, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                posFromCenterOfSphere = hit.point;
            }


            Vector3 nextStep = posFromCenterOfSphere - legs[legNumber].desirePosInWorld;
            nextStep = nextStep.normalized * legs[legNumber].maxAcceptableLegDistanceFromDesire * 0.5f;
            Vector3 normailzedMovment = new Vector3(Mathf.Abs(movmentAxis.x), Mathf.Abs(movmentAxis.y), Mathf.Abs(movmentAxis.z)).normalized;
            nextStep = legs[legNumber].translatedDesiredPos + Vector3.Scale(nextStep, normailzedMovment); ; //new Vector3(nextStep.x * normailzedMovment.x, nextStep.y * normailzedMovment.y, nextStep.z * normailzedMovment.z);
            nextStep = nextStep + spider.position;

            Debug.DrawLine(nextStep, nextStep + spider.up, Color.red);

            if(!legs[legNumber - 1 >= 0 ? legNumber - 1 : legs.Length - 1].isMoving && !legs[legNumber + 1 < legs.Length ? legNumber + 1 : 0].isMoving)
            if (Vector3.Distance(legs[legNumber].desirePosInWorld, posFromCenterOfSphere) > legs[legNumber].maxAcceptableLegDistanceFromDesire 
                || legs[legNumber].allLength < Vector3.Distance(legs[legNumber].translatedRoot, legs[legNumber].desirePosInWorld - spider.position)
                || Vector3.Distance(legs[legNumber].translatedDesiredPos, hit.point) < desireHeigth * 0.5f)
            {
                if (posFromCenterOfSphere != Vector3.positiveInfinity)  
                {
                    if (Physics.Raycast(nextStep, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity)) //legs[legNumber].desiredPos + spider.position
                    {
                        if(Vector3.Distance(hit.point, legs[legNumber].translatedRoot + spider.position) <= legs[legNumber].allLength)
                        {
                            legs[legNumber].currentPosition = legs[legNumber].desirePosInWorld;
                            legs[legNumber].startPosition = legs[legNumber].desirePosInWorld;
                            legs[legNumber].moveStartTime = Time.time;
                            legs[legNumber].moveDuration = Vector3.Distance(legs[legNumber].currentPosition, hit.point) / legMoveSpeed;
                            legs[legNumber].moveInterpolationPoint = legs[legNumber].currentPosition + (hit.point - legs[legNumber].currentPosition) / 2 + spider.up * legMoveHeight;
                            legs[legNumber].desirePosInWorld = hit.point;
                            legs[legNumber].isMoving = true;
                            continue;
                        }
                    }
                }
                if(Physics.Raycast(nextStep, (spider.TransformDirection(Vector3.down) + rayCastLeanOffset), out hit, Mathf.Infinity))
                {
                    if (Vector3.Distance(hit.point, legs[legNumber].translatedRoot + spider.position) <= legs[legNumber].allLength)
                    {
                        legs[legNumber].currentPosition = legs[legNumber].desirePosInWorld;
                        legs[legNumber].startPosition = legs[legNumber].desirePosInWorld;
                        legs[legNumber].moveStartTime = Time.time;
                        legs[legNumber].moveDuration = Vector3.Distance(legs[legNumber].currentPosition, hit.point) / legMoveSpeed;
                        legs[legNumber].moveInterpolationPoint = legs[legNumber].currentPosition + (hit.point - legs[legNumber].currentPosition) / 2 + spider.up * legMoveHeight;
                        legs[legNumber].desirePosInWorld = hit.point;
                        legs[legNumber].isMoving = true;
                        continue;
                    }
                }
            }
        }
    }

    [SerializeField]
    [Range(0, 1)]
    float leanInfluence;
    private void MoveBodyTargetPosition()
    {
        Vector3 heigth = Vector3.zero;
        Vector3 rotation = Vector3.zero;
        Vector3 rotationLean = Vector3.zero;

        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s¹ dla ka¿dej nogi osobno
        {
            Vector3 down = -spider.up;
            Vector3 myHeigth = Vector3.Project(spider.position - legs[legNumber].desirePosInWorld, spider.up);
            heigth += myHeigth;
           // Debug.DrawLine(legs[legNumber].desirePosInWorld, myHeigth + legs[legNumber].desirePosInWorld, Color.magenta);

            #region rotatnion
            Vector3 firstLeg = legs[legNumber + 2 > legs.Length - 1 ? legNumber + 2 - legs.Length : legNumber + 2].desirePosInWorld;
            Vector3 secondLeg = legs[legNumber + 1 > legs.Length - 1 ? 0 : legNumber + 1].desirePosInWorld;
            Vector3 thirdLeg = legs[legNumber].desirePosInWorld;
            Vector3 vectorUp = Vector3.Cross(secondLeg - firstLeg, thirdLeg - firstLeg);
            rotation += vectorUp.normalized;

            rotationLean += spider.position - legs[legNumber].desirePosInWorld;

            // Debug.DrawLine(firstLeg, firstLeg + vectorUp.normalized * 1, Color.green);
            #endregion
        }

        heigth /= legs.Length;
        rotation = (rotation / legs.Length).normalized;
        rotationLean = -(rotationLean / legs.Length).normalized;
        float heightScalar = desireHeigth - heigth.magnitude;

        Debug.DrawLine(spider.position, spider.position + rotation, Color.green);
        Debug.DrawLine(spider.position, spider.up * heightScalar + spider.position, Color.magenta);
        Debug.DrawLine(spider.position, spider.position + rotationLean, Color.yellow);

        rayCastLeanOffset = rotationLean;
        desiredPosition = spider.position + spider.up * heightScalar;
        desiredRotation = rotation;
    }


    private void CalculateLegsPosition()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenaia wykonywane s¹ dla ka¿dej nogi osobno
        {
            float t = (Time.time - legs[legNumber].moveStartTime) / legs[legNumber].moveDuration;
            if(legs[legNumber].isMoving)
            {
                if(t <= 1) legs[legNumber].currentPosition = Mathf.Pow((1 - t), 2) * legs[legNumber].startPosition + 2*(1-t)*t* legs[legNumber].moveInterpolationPoint + Mathf.Pow(t, 2) * legs[legNumber].desirePosInWorld;
                else legs[legNumber].isMoving = false;
            }
        }
    }

    private void CalculateIK()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenaia wykonywane s¹ dla ka¿dej nogi osobno
        {
            //  Jeœli odleg³oœæ od celu jest wiêksza ni¿ d³ugoœæ nogi obliczamy kierunek w którym powinna byæ wyci¹gniêta a nastêpnie ustawiamy koœci prosto w kierunku celu z wyj¹tkiem pierwszej, która jest translatedRootem
            if (Vector3.Magnitude((legs[legNumber].currentPosition - spider.position) - legs[legNumber].translatedRoot) > legs[legNumber].allLength) 
            {
                Vector3 dir = ((legs[legNumber].currentPosition - spider.position) - legs[legNumber].translatedRoot).normalized;
                Vector3 dist = Vector3.zero;

                for(int bone = 1; bone < legs[legNumber].bones.Length; bone++)
                {
                    legs[legNumber].bones[bone] = legs[legNumber].translatedRoot + dir * legs[legNumber].bonesLength[bone - 1] + dist;
                    dist += dir * legs[legNumber].bonesLength[bone - 1];
                }

            }
            else
            {
                for (int iterationsCount = 0; iterationsCount < iterations; iterationsCount++)
                {
                    legs[legNumber].bones[legs[legNumber].bones.Length - 1] = legs[legNumber].currentPosition - spider.position;

                    for(int boneIndex = legs[legNumber].bones.Length - 2; boneIndex >= 0; boneIndex--) //back
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex + 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex + 1]).normalized * legs[legNumber].bonesLength[boneIndex];
                    }

                    
                    legs[legNumber].bones[0] = legs[legNumber].translatedRoot;

                    for (int boneIndex = 1; boneIndex < legs[legNumber].bones.Length - 1; boneIndex++) // forward
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex - 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex - 1]).normalized * legs[legNumber].bonesLength[boneIndex - 1];
                    }

                    if (Vector3.Distance(legs[legNumber].bones[legs[legNumber].bones.Length - 1], (legs[legNumber].currentPosition - spider.position)) < delta) break;

                }
            }
        }
    }

    private void SetLegPosition()
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
            Gizmos.DrawSphere(legs[legNumber].translatedRoot + spider.position, 0.2f);
            Gizmos.DrawWireSphere(legs[legNumber].currentPosition, 0.3f);

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

            RaycastHit hit;
            if (Physics.Raycast(legs[legNumber].translatedDesiredPos + spider.position, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                Gizmos.DrawLine(legs[legNumber].translatedDesiredPos + spider.position, hit.point);
                Gizmos.DrawSphere(legs[legNumber].translatedDesiredPos + spider.position, 0.1f);
                Gizmos.DrawWireSphere(hit.point, legs[legNumber].maxAcceptableLegDistanceFromDesire);

            }
        }

      

    }
}
