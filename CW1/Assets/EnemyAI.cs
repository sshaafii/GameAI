using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    public Transform target;
    float speed = 2.54f;
    Vector3[] path;
    int targetIndex;
    public float patrolDistance = 20f;
    public float chaseDistance = 15f;
    public TerrainGenv1 gen;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 100f;
    private Vector3 lastTargetPosition;

    public TextMeshProUGUI stateText;
    private enum AIState { Patrolling, Chasing, Attack };
    private AIState currentState;

    private void Start()
    {
        currentState = AIState.Patrolling;
        lastTargetPosition = target.position;
        GenerateRandomPatrolPoint();
        //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    void Update()
    {
        if (target == null)
        {

            return;
        }
       // stateText.text = currentState.ToString();
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case AIState.Patrolling:
                if (distanceToTarget <= chaseDistance)
                {
                    Debug.Log("chase");
                    currentState = AIState.Chasing;

                    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                }
                break;

            case AIState.Chasing:
                if (distanceToTarget > chaseDistance + patrolDistance)
                {
                    Debug.Log("exiting Chase into patrol");
                    currentState = AIState.Patrolling;
                    GenerateRandomPatrolPoint();
                }
                else if (distanceToTarget <= chaseDistance && distanceToTarget <= patrolDistance)
                {
                    Debug.Log("Attacking");
                    currentState = AIState.Attack;
                }
                else if (lastTargetPosition != target.position)
                {
                    Debug.Log("chasing!!!!");
                    lastTargetPosition = target.position;
                    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                }

                break;
            case AIState.Attack:
                Debug.Log("attack!!!!!!");

                //FaceTarget(target);
               // ShootBullet();

                if (distanceToTarget > patrolDistance)
                {
                    Debug.Log("Exiting attack into chase");
                    currentState = AIState.Chasing;
                }
                break;

        }

    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void ShootBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        
        bulletRb.velocity = bulletSpawnPoint.forward * bulletSpeed;

        Destroy(bullet, 3f);
    }

    private void GenerateRandomPatrolPoint()
    {

        Debug.Log("generate");
        Vector3 randomPoint = gen.GetRandomWalkablePosition1();
        Debug.Log(randomPoint);

        PathRequestManager.RequestPath(transform.position, randomPoint, OnPathFound);

    }


    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];
        while (true)
        {

            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                if (targetIndex >= path.Length)
                {

                    if (currentState == AIState.Patrolling)
                    {
                        GenerateRandomPatrolPoint();
                    }
                    yield break;

                }
                currentWaypoint = path[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            yield return null;

        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }





}
