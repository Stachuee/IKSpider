using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TurretShooting : MonoBehaviour
{
    [SerializeField]
    Transform barrel;
    [SerializeField]
    Transform turret;
    [SerializeField]
    Transform spider;
    [SerializeField]
    GameObject bulletPrefab;


    [SerializeField]
    float delayBetweenShots;
    float delayRemain;

    [SerializeField]
    Renderer vision;
    bool vison;

    [SerializeField]
    Renderer barrelMat;

    [SerializeField]
    float heatDispersionPerSecond;
    [SerializeField]
    float heatGenerationPerShot;
    float heat;

    [SerializeField]
    GameObject barrelHeatPrefab;

    [SerializeField]
    List<Transform> barrelsToAnimate;

    [SerializeField]
    float damping;
    [SerializeField]
    float forceToAdd;
    [SerializeField]
    float maxForce;
    [SerializeField][Range(0,1)]
    float forceDisperce;

    float force;
    Vector3 basePosition;

    Vector3 velocity = Vector3.zero;


    [SerializeField]
    ScriptableRendererFeature echo;
    [SerializeField]
    ScriptableRendererFeature glow;
    [SerializeField]
    Material echoMat;
    [SerializeField]
    Material glowMat;
    [SerializeField]
    float echoDuration;
    bool echoOn;

    float scanTime;


    private void Start()
    {
        basePosition = barrelsToAnimate[0].localPosition;
    }

    private void Update()
    {
        if (!GameManager.gameManager.playerAlive || GameManager.gameManager.pause) return;
        if (Input.GetMouseButton(0) && delayRemain < 0)
        {
            delayRemain = delayBetweenShots;
            force = Mathf.Clamp(force + forceToAdd, 0, maxForce); 
            Instantiate(bulletPrefab, barrel.position, Quaternion.LookRotation(barrel.position - turret.position, turret.up));
            GameObject temp = Instantiate(barrelHeatPrefab, barrel.position, Quaternion.LookRotation(barrel.position - turret.position, turret.up));
            Destroy(temp, 1);
            heat += heatGenerationPerShot;
        }
        if(Input.GetKeyDown(KeyCode.N))
        {
            if(vison)
            {
                vision.material.SetFloat("_Thermal", 0);
            }
            else
            {
                vision.material.SetFloat("_Thermal", 1);
            }
            vison = !vison;

        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopAllCoroutines();
            StartCoroutine("SendPulse");
        }

        delayRemain -= Time.deltaTime;

        force -= force * forceDisperce;
        barrelsToAnimate.ForEach(x => x.localPosition = Vector3.SmoothDamp(x.localPosition, basePosition - -x.right * force, ref velocity, damping));
        heat = Mathf.Clamp(heat - heatDispersionPerSecond * Time.deltaTime, 0, 1);
        barrelMat.material.SetFloat("_HeatLevel", heat);


        echoMat.SetVector("_scanPostion", spider.position);
        glowMat.SetVector("_scanPostion", spider.position);
        echoMat.SetFloat("_ScanTime", scanTime);
        glowMat.SetFloat("_ScanTime", scanTime);
        scanTime += Time.deltaTime;
    }


    IEnumerator SendPulse()
    {
        echo.SetActive(true);
        glow.SetActive(true);
        scanTime = 0;
        yield return new WaitForSeconds(echoDuration);
        glow.SetActive(false);
        echo.SetActive(false);
    }
}
