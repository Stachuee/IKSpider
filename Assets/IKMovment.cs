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
        public Transform[] knees;
        [HideInInspector]
        public Transform feet;
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
    GameObject kneePrefab;
    [SerializeField]
    GameObject footPrefab;

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

    GameManager gameManager;

    List<SegmentHeat> legsParts = new List<SegmentHeat>();
    private void Awake()
    {
        Innit();
    }

    private void Start()
    {
        gameManager = GameManager.gameManager;
    }

    private void Update()
    {
        float horizontal, vertical;
        if (gameManager.playerAlive && !gameManager.pause)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            movmentAxis = spider.forward * vertical + spider.right * horizontal;
            Vector3 forwardMoveDirection = Vector3.Project(turret.forward, Vector3.ProjectOnPlane(turret.forward, spider.up).normalized).normalized; // Get forward vector
            Vector3 rightMoveDirection = Vector3.Project(turret.right, Vector3.ProjectOnPlane(turret.right, spider.up).normalized).normalized; // Get right vector


            Vector3 input = forwardMoveDirection * vertical + rightMoveDirection * horizontal;
            input = input.magnitude > 1 ? input.normalized : input;
            if(input.magnitude > 0 && CheckIfCanMove(input.normalized)) spider.position += input * speed * Time.deltaTime; // Normalize to not exeed speed if moving diagonal 
        }

      

        TranslatePoitns();
        MoveLegsTargetPosition();
        MoveBodyTargetPosition();
        spider.up = Vector3.SmoothDamp(spider.up, desiredRotation, ref velocity, smoothTime); // rotate spider smoothly 
        spider.position = Vector3.SmoothDamp(spider.position, desiredPosition, ref velocity, smoothTime); // move spider smoothly
        CalculateLegsPosition();
        CalculateIK();
        SetLegPosition();

        heat = Mathf.Clamp(heat + (movmentAxis.magnitude * heatPerSecond - heatDispersePerSecond) * Time.deltaTime , 0, 1);
        body.material.SetFloat("_HeatLevel", heat);
        legsParts.ForEach(x => x.UpdateHeat(heat));
    }

    [SerializeField]
    float colliderRadius;
    private bool CheckIfCanMove(Vector3 dir) // prevent spider from walking into walls 
    {
        RaycastHit hit; 
        if(Physics.Raycast(spider.position, dir, out hit, colliderRadius))
        {
            return false;
        }
        Debug.DrawLine(spider.position, spider.position + dir * colliderRadius);
        return true;
    }


    private void Innit()
    {
        for(int legNumber = 0; legNumber < legs.Length; legNumber++)
        {
            legs[legNumber].bones = new Vector3[legs[legNumber].bonesLength.Length + 1];
            legs[legNumber].segments = new Transform[legs[legNumber].bonesLength.Length];
            legs[legNumber].knees = new Transform[legs[legNumber].bonesLength.Length];

            Vector3 dir = -spider.up;
            //Debug.Log(legs[legNumber].bones[legNumber]);
            legs[legNumber].bones[0] = legs[legNumber].translatedRoot;
            Vector3 dist = Vector3.zero;

            for (int i = 1; i < legs[legNumber].bones.Length; i++) //set up all joints in desired length
            {
                legs[legNumber].bones[i] = legs[legNumber].translatedRoot + dir * legs[legNumber].bonesLength[i - 1] + dist;
                dist += dir * legs[legNumber].bonesLength[i - 1];
            }
            legs[legNumber].allLength = dist.magnitude;

            for(int i = 0; i < legs[legNumber].bonesLength.Length; i++) // instantiate all bones
            {
                GameObject temp = Instantiate(segmentPrefab, transform.position, quaternion.identity);
                legsParts.Add(temp.GetComponent<SegmentHeat>());
                legs[legNumber].segments[i] = temp.transform;
                legs[legNumber].startPosition = legs[legNumber].bones[i] + spider.position;
                legs[legNumber].desirePosInWorld = legs[legNumber].bones[i] + spider.position;
            }

            for (int i = 0; i < legs[legNumber].bonesLength.Length; i++) // instantiate all joints
            {
                GameObject temp = Instantiate(kneePrefab, transform.position, quaternion.identity);
                legsParts.Add(temp.GetComponent<SegmentHeat>());
                legs[legNumber].knees[i] = temp.transform;
            }

            GameObject tempFeet = Instantiate(footPrefab, transform.position, quaternion.identity); // instantiate foot
            legsParts.Add(tempFeet.GetComponent<SegmentHeat>());
            legs[legNumber].feet = tempFeet.transform;


        }

    }

    private void TranslatePoitns()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // translate points into local space from world space
        {
            legs[legNumber].translatedDesiredPos = spider.TransformPoint(legs[legNumber].desiredPos) - spider.position;
            legs[legNumber].translatedRoot = spider.TransformPoint(legs[legNumber].root) - spider.position;
        }
    }

    private void MoveLegsTargetPosition()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) 
        {

            RaycastHit hit;
            Vector3 posFromCenterOfSphere = Vector3.positiveInfinity;
            if (Physics.Raycast(legs[legNumber].translatedDesiredPos + spider.position, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                posFromCenterOfSphere = hit.point; // get point to calculate distance from
            }


            // get candidate foot position
            Vector3 nextStep = posFromCenterOfSphere - legs[legNumber].desirePosInWorld;
            nextStep = nextStep.normalized * legs[legNumber].maxAcceptableLegDistanceFromDesire * 0.7f;
            Vector3 normailzedMovment = new Vector3(Mathf.Abs(movmentAxis.x), Mathf.Abs(movmentAxis.y), Mathf.Abs(movmentAxis.z)).normalized;
            nextStep = legs[legNumber].translatedDesiredPos + Vector3.Scale(nextStep, normailzedMovment); ; 
            nextStep = nextStep + spider.position;

            Debug.DrawLine(nextStep, nextStep + spider.up, Color.red);

            
            if(!legs[legNumber - 1 >= 0 ? legNumber - 1 : legs.Length - 1].isMoving && !legs[legNumber + 1 < legs.Length ? legNumber + 1 : 0].isMoving) // if next and previous legs are not moving
                if (Vector3.Distance(legs[legNumber].desirePosInWorld, posFromCenterOfSphere) > legs[legNumber].maxAcceptableLegDistanceFromDesire  // if foot is too far from body 
                || legs[legNumber].allLength < Vector3.Distance(legs[legNumber].translatedRoot, legs[legNumber].desirePosInWorld - spider.position) 
                || Vector3.Distance(legs[legNumber].translatedDesiredPos, hit.point) < desireHeigth * 0.5f) // or foot is too clos to body
            {
                if (posFromCenterOfSphere != Vector3.positiveInfinity)  // if there is a floor underneath
                    {
                    if (Physics.Raycast(nextStep, spider.TransformDirection(Vector3.down), out hit, Mathf.Infinity)) // check if there is any ground under next step
                    {
                        if(Vector3.Distance(hit.point, legs[legNumber].translatedRoot + spider.position) <= legs[legNumber].allLength) // check if next step is reachable 
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
                if(Physics.Raycast(nextStep, (spider.TransformDirection(Vector3.down) + rayCastLeanOffset), out hit, Mathf.Infinity)) // check if there is any ground under spider at an angle (to allow walking over hills)
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

        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenia wykonywane s� dla ka�dej nogi osobno
        {
            Vector3 down = -spider.up;
            Vector3 myHeigth = Vector3.Project(spider.position - legs[legNumber].desirePosInWorld, spider.up); // vector up
            heigth += myHeigth;

            #region rotatnion
            Vector3 firstLeg = legs[legNumber + 2 > legs.Length - 1 ? legNumber + 2 - legs.Length : legNumber + 2].desirePosInWorld;
            Vector3 secondLeg = legs[legNumber + 1 > legs.Length - 1 ? 0 : legNumber + 1].desirePosInWorld;
            Vector3 thirdLeg = legs[legNumber].desirePosInWorld;
            Vector3 vectorUp = Vector3.Cross(secondLeg - firstLeg, thirdLeg - firstLeg); // get vectors from 3 legs and calculate up vector
            rotation += vectorUp.normalized;

            rotationLean += spider.position - legs[legNumber].desirePosInWorld; // calculate angle to cast ray when walking over terrain

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
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenaia wykonywane s� dla ka�dej nogi osobno
        {
            float t = (Time.time - legs[legNumber].moveStartTime) / legs[legNumber].moveDuration;
            if(legs[legNumber].isMoving)
            {
                // Use bezier curves to interpolate feet position
                if(t <= 1) legs[legNumber].currentPosition = Mathf.Pow((1 - t), 2) * legs[legNumber].startPosition + 2*(1-t)*t* legs[legNumber].moveInterpolationPoint + Mathf.Pow(t, 2) * legs[legNumber].desirePosInWorld;
                else legs[legNumber].isMoving = false;
            }
        }
    }

    private void CalculateIK()
    {
        for (int legNumber = 0; legNumber < legs.Length; legNumber++) // Obliczenaia wykonywane s� dla ka�dej nogi osobno
        {
            //  If desired leg position is further than reachable, then streach leg to it 
            if (Vector3.Magnitude((legs[legNumber].currentPosition - spider.position) - legs[legNumber].translatedRoot) > legs[legNumber].allLength) 
            {
                Vector3 dir = ((legs[legNumber].currentPosition - spider.position) - legs[legNumber].translatedRoot).normalized;
                Vector3 dist = Vector3.zero;

                for(int bone = 1; bone < legs[legNumber].bones.Length; bone++)
                {
                    dist += dir * legs[legNumber].bonesLength[bone - 1];
                    legs[legNumber].bones[bone] = legs[legNumber].translatedRoot + dir + dist;

                }

            }
            else
            {
                for (int iterationsCount = 0; iterationsCount < iterations; iterationsCount++)
                {
                    legs[legNumber].bones[legs[legNumber].bones.Length - 1] = legs[legNumber].currentPosition - spider.position; // set feet at destination

                    for(int boneIndex = legs[legNumber].bones.Length - 2; boneIndex >= 0; boneIndex--) // going backwards calculate next joint
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex + 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex + 1]).normalized * legs[legNumber].bonesLength[boneIndex];
                    }

                    
                    legs[legNumber].bones[0] = legs[legNumber].translatedRoot; // set last joint at root point

                    for (int boneIndex = 1; boneIndex < legs[legNumber].bones.Length - 1; boneIndex++) // going forward calculate next joint
                    {
                        legs[legNumber].bones[boneIndex] = legs[legNumber].bones[boneIndex - 1] + (legs[legNumber].bones[boneIndex] - legs[legNumber].bones[boneIndex - 1]).normalized * legs[legNumber].bonesLength[boneIndex - 1];
                    }


                    //check if distance from foot to destinatnion is acceptable 
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
                legs[legNumber].segments[i].position = legs[legNumber].bones[i] + spider.position; // set bone postion
                legs[legNumber].segments[i].rotation = Quaternion.LookRotation((legs[legNumber].bones[i] - legs[legNumber].bones[i + 1]), Vector3.up); // set bone rotation
            }

            for (int i = 0; i < legs[legNumber].segments.Length; i++)
            {
                legs[legNumber].knees[i].position = legs[legNumber].bones[i] + spider.position; // set joint postion
            }

            legs[legNumber].feet.position = legs[legNumber].bones[legs[legNumber].segments.Length] + spider.position; // set foot position and rotation
            legs[legNumber].feet.rotation = Quaternion.LookRotation((legs[legNumber].bones[legs[legNumber].segments.Length - 1] - legs[legNumber].bones[legs[legNumber].segments.Length]), Vector3.up);
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
