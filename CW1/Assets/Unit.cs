using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Unit : MonoBehaviour
{
    public Transform target;
    float speed = 2.54f;
    Vector3[] path;
    int targetIndex;

    private float pathRequestTimestamp;

    
    

    public float patrolDistance = 20f;
    public float chaseDistance = 15f;
    [SerializeField] TextMesh currentStateUI, ID_UI;
    public string ID = "Enemy";

    private float lastStateChangeTime;

    public TerrainGenv1 gen;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 100f;
    public float retreatDistance = 5f;

    public float fireRate = 1f; // Add this line to set the fire rate
    private float nextFireTime;

    public Terrain terrain;


    private Vector3 lastTargetPosition;

   // public TextMeshProUGUI stateText;
    public TMP_Text agentId;
    public TMP_Text stateText;
    

    private enum AIState { Patrolling, Chasing,Attack, Retreat };
    private AIState currentState;
    private Health health;

    private void Start()
    {
        currentState = AIState.Patrolling;
        lastTargetPosition = target.position;
        GenerateRandomPatrolPoint();
        //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        health = GetComponent<Health>();
    }

    void Update()
    {

        currentStateUI.text = $"State : {currentState}";
        ID_UI.text = $"ID : {ID}";

        if (target == null) {

            return;
        }
        


       

        //stateText.text = currentState.ToString();
        //stateText.color = Color.red;
        //stateText.text = currentState.ToString();
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

       


        switch (currentState) 
        { 
            case AIState.Patrolling:

                if (distanceToTarget <= chaseDistance)
                {
                    Debug.Log("chase");
                    ChangeState(AIState.Chasing);
                    // currentState = AIState.Chasing;
                    pathRequestTimestamp = Time.realtimeSinceStartup;

                    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                }


                break;

            case AIState.Chasing:
                if (distanceToTarget > chaseDistance + patrolDistance)
                {
                    Debug.Log("exiting Chase into patrol");
                    ChangeState(AIState.Patrolling);
                    //currentState = AIState.Patrolling;
                    GenerateRandomPatrolPoint();
                }
                else if (distanceToTarget <= chaseDistance )
                {
                    Debug.Log("Attacking");
                    ChangeState(AIState.Attack);
                   // currentState = AIState.Attack;
                }
                else if (distanceToTarget <= retreatDistance)
                {
                    Debug.Log("Retreating");
                    ChangeState(AIState.Retreat);
                   // currentState = AIState.Retreat;
                    Vector3 retreatPoint = transform.position - (target.position - transform.position).normalized * patrolDistance;
                    pathRequestTimestamp = Time.realtimeSinceStartup;
                    PathRequestManager.RequestPath(transform.position, retreatPoint, OnPathFound);
                }

                else if (lastTargetPosition != target.position)
                {
                    Debug.Log("chasing!!!!");
                    lastTargetPosition = target.position;
                    pathRequestTimestamp = Time.realtimeSinceStartup;
                    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                }


                break;
            case AIState.Attack:
                Debug.Log("attack!!!!!!");

               FaceTarget(target);
                ShootBullet();

                if (distanceToTarget > patrolDistance)
                {
                    Debug.Log("Exiting attack into chase");
                    ChangeState(AIState.Chasing);
                   // currentState = AIState.Chasing;
                }
                else if (distanceToTarget <= retreatDistance)
                {
                    Debug.Log("Retreating");
                    ChangeState(AIState.Retreat);
                    //currentState = AIState.Retreat;
                    Vector3 retreatPoint = transform.position - (target.position - transform.position).normalized * patrolDistance;
                    pathRequestTimestamp = Time.realtimeSinceStartup;
                    PathRequestManager.RequestPath(transform.position, retreatPoint, OnPathFound);
                }


                break;
            case AIState.Retreat:
                if (distanceToTarget >= retreatDistance)
                {
                    Debug.Log("Exiting retreat into patrol");
                    ChangeState(AIState.Patrolling);
                   // currentState = AIState.Patrolling;
                    GenerateRandomPatrolPoint();
                }
                break;


            


        }

    }


    private void ChangeState(AIState newState)
    {
        float stateChangeDuration = Time.time - lastStateChangeTime;
        Debug.Log($"State changed from {currentState} to {newState} in {stateChangeDuration} seconds");
        currentState = newState;
        lastStateChangeTime = Time.time;
    }


    private void FaceTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void ShootBullet()
    {
        //GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
       // Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        // bulletRb.AddForce(bulletSpawnPoint.forward * bulletSpeed, ForceMode.Impulse);
        // bulletRb.velocity = bulletSpawnPoint.forward * bulletSpeed;

        // if (bullet.TryGetComponent<Bullet>(out Bullet b))
        //  {  b.init(this.transform); }

        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate; // Set the next fire time
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.velocity = bulletSpawnPoint.forward * bulletSpeed;
            Destroy(bullet, 3f);
        }




       // Destroy(bullet, 3f);
    }

    private void GenerateRandomPatrolPoint() 
    {

        Debug.Log("generate");
        Vector3 randomPoint = gen.GetRandomWalkablePosition1();
        //Debug.Log(randomPoint);

        if (currentState == AIState.Retreat)
        {
            Vector3 retreatPoint = transform.position - (target.position - transform.position).normalized * patrolDistance;
            pathRequestTimestamp = Time.realtimeSinceStartup;
            PathRequestManager.RequestPath(transform.position, retreatPoint, OnPathFound);
        }
        else
        {
            pathRequestTimestamp = Time.realtimeSinceStartup;
            PathRequestManager.RequestPath(transform.position, randomPoint, OnPathFound);
        }

       // PathRequestManager.RequestPath(transform.position, randomPoint, OnPathFound);

    }

    private float CalculatePathLength(Vector3[] path)
    {
        float totalLength = 0;
        for (int i = 1; i < path.Length; i++)
        {
            totalLength += Vector3.Distance(path[i - 1], path[i]);
        }
        return totalLength;
    }


    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {



            float pathLength = CalculatePathLength(newPath);
            float directDistance = Vector3.Distance(transform.position, target.position);
            Debug.Log($"Generated path length: {pathLength}, Direct distance: {directDistance}");

            float pathGenerationTime = Time.realtimeSinceStartup - pathRequestTimestamp;
            Debug.Log($"Path generation time: {pathGenerationTime} seconds");


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

            if (currentState == AIState.Retreat) // Modify speed when retreating
            {
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * 3f * Time.deltaTime); // Use a faster speed when retreating
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
            }




           // transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
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
