using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainShoot : MonoBehaviour
{

    [SerializeField] float refreshRate = 0.1f;
    [SerializeField][Range(1, 10)] int maximunEnemiesInChain = 3;
    [SerializeField] float delayBetweenEacChain = 0.5f;
    [SerializeField] Transform playerFirePoint;
    [SerializeField] EnemyDetector playerEnemyDetector;
    [SerializeField] GameObject lineRendererPrefab;

    bool Shooting;
    bool shot;
    float counter = 1;
    GameObject currentClosestEnemy;

    List<GameObject> spawnedLineRenderers = new List<GameObject>();
    List<GameObject> enemiesInChain = new List<GameObject>();
    List<GameObject> activeEffects = new List<GameObject>();

    IEnumerator ChainReaction(GameObject closestEnemy)
    {
        yield return new WaitForSeconds(delayBetweenEacChain);

        if (counter == maximunEnemiesInChain)
        {
            yield return null;
        }
        else
        {
            if (Shooting)
            {
                counter++;
                enemiesInChain.Add(closestEnemy);
                if (!enemiesInChain.Contains(closestEnemy.GetComponent<EnemyDetector>().GetClosestEnemy()))
                {
                    NewLineRenderer(closestEnemy.transform, closestEnemy.GetComponent<EnemyDetector>().GetClosestEnemy().transform);
                    StartCoroutine(ChainReaction(closestEnemy.GetComponent<EnemyDetector>().GetClosestEnemy()));

                }
            }
        }
    }

    void NewLineRenderer(Transform startPos, Transform endPos, bool getClosestEnemyToPlayer = false)
    {
        GameObject lineR = Instantiate(lineRendererPrefab);
        spawnedLineRenderers.Add(lineR);
        StartCoroutine(UpdateLineRenderer(lineR, startPos, endPos, getClosestEnemyToPlayer));
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            if(playerEnemyDetector.GetEnemiesInRange().Count > 0)
            {
                if(!Shooting)
                {
                    StartShooting();
                }
            }
            else
            {
                StopShooting();
            }
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopShooting();
        }

    }

    IEnumerator UpdateLineRenderer(GameObject lineR, Transform startPos, Transform endPos, bool getClosestEnemyToPlayer = false)
    {
        if (Shooting && shot && lineR != null)
        {
            lineR.GetComponent<LineRandererController>().SetPosition(startPos, endPos);
            yield return new WaitForSeconds(refreshRate);

            if (getClosestEnemyToPlayer)
            {
                StartCoroutine(UpdateLineRenderer(lineR, startPos, playerEnemyDetector.GetClosestEnemy().transform, true));
                if (currentClosestEnemy != playerEnemyDetector.GetClosestEnemy())
                {
                    StopShooting();
                    StartShooting();
                }
            }
            else
            {
                StartCoroutine(UpdateLineRenderer(lineR, startPos, endPos));
            }
        }
    }
    void StartShooting()
    {
        Shooting = true;

        if (playerEnemyDetector != null && playerFirePoint != null && lineRendererPrefab != null)
        {
            if (!shot)
            {
                shot = true;
                currentClosestEnemy = playerEnemyDetector.GetClosestEnemy();
                NewLineRenderer(playerFirePoint, playerEnemyDetector.GetClosestEnemy().transform, true);

                if (maximunEnemiesInChain > 1)
                {
                    StartCoroutine(ChainReaction(playerEnemyDetector.GetClosestEnemy()));
                }
            }
        }
        
    }
    void StopShooting()
    {
        Shooting = false;
        shot = false;

        for (int i = 0; i < spawnedLineRenderers.Count; i++)
        {
            Destroy(spawnedLineRenderers[i]);
        }
        spawnedLineRenderers.Clear();
        enemiesInChain.Clear();

        for (int i = 0; i < activeEffects.Count; i++)
        {
            Destroy(activeEffects[i]);
        }

        activeEffects.Clear();
    }
}
