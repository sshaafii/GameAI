
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Boss : MonoBehaviour
{

    public Transform target;
    float speed = 2.54f;


    float fleeSpeed = 5f;

    private Dictionary<AIState, int> stateChangeCounts = new Dictionary<AIState, int>();
    private Dictionary<AIState, float> stateDurations = new Dictionary<AIState, float>();
    private AIState previousState;
    private float stateStartTime;
    private float lastStateChangeTime;


    Vector3[] path;
    int targetIndex;

    public float patrolDistance = 20f;
    public float chaseDistance = 15f;
    public string ID = "Boss";
    [SerializeField] TextMesh currentStateUI, ID_UI;
  

    public TerrainGenv1 gen;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 100f;
    private float originalSpeed;
    public float healThreshold = 0.7f;
    public float healRate = 100f;

    private bool hasEnteredFleeState = false;
    public float fleeDuration = 5f;
    private float fleeEndTime;


    public float fireRate = 1f; // Add this line to set the fire rate
    private float nextFireTime;

    public Terrain terrain;



    private Vector3 lastTargetPosition;

    // public TextMeshProUGUI stateText;
   // public TMP_Text agentId;
    //public TMP_Text stateText;
    private Health health;

    



    private enum AIState { Patrolling, Chasing, Attack,Healing, Fleeing };
    private AIState currentState;

    


    private void Start()
    {
        currentState = AIState.Patrolling;
        lastTargetPosition = target.position;
        GenerateRandomPatrolPoint();
        //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        originalSpeed = speed;

        health = GetComponent<Health>();

       
    }

   

    void Update()
    {

        currentStateUI.text = $"State : {currentState}";
        ID_UI.text = $"ID : {ID}";


        if (target == null)
        {

            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            PrintStateData();
        }


        if (health.GetCurrentHealthRatio() <= healThreshold && currentState != AIState.Healing && currentState != AIState.Fleeing)
        {

            if (UnityEngine.Random.value < 0.2f)
            {
                ChangeState(AIState.Healing);

                //currentState = AIState.Healing;
            }
            else {
                 Debug.Log("FLEEEEING");
                ChangeState(AIState.Fleeing);
                //currentState = AIState.Fleeing;
                //FleeFromTarget();   
            }
          
        }
       
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

      


        switch (currentState)
        {
            case AIState.Patrolling:

                if (distanceToTarget <= chaseDistance)
                {
                    Debug.Log("chase");
                    ChangeState(AIState.Chasing);
                   // currentState = AIState.Chasing;

                    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                }


                break;

            case AIState.Chasing:
                if (distanceToTarget > chaseDistance + patrolDistance)
                {
                    Debug.Log("exiting Chase into patrol");
                    ChangeState(AIState.Patrolling);
                   // currentState = AIState.Patrolling;
                    GenerateRandomPatrolPoint();
                }
                else if (distanceToTarget <= chaseDistance)
                {
                    Debug.Log("Attacking");
                    ChangeState(AIState.Attack);
                   // currentState = AIState.Attack;
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

                FaceTarget(target);
                ShootBullet();

                if (distanceToTarget > patrolDistance)
                {
                    Debug.Log("Exiting attack into chase");
                    ChangeState(AIState.Chasing);
                    //currentState = AIState.Chasing;
                }


                break;

            case AIState.Healing:
                Debug.Log("Healing");

                Debug.Log("before :" + health.GetCurrentHealthRatio());
                Heal();

                if (distanceToTarget <= chaseDistance)
                {
                    ChangeState(AIState.Chasing);
                    //currentState = AIState.Chasing;
                    PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                }

               

                Debug.Log("Current :" + health.GetCurrentHealthRatio());


                break;

            case AIState.Fleeing:

                if (health.GetCurrentHealthRatio() >= 1 - healThreshold)
                {
                    Debug.Log("Healt good going into patrol");
                    ChangeState(AIState.Patrolling);
                   // currentState = AIState.Patrolling;
                    GenerateRandomPatrolPoint();

                }
                else {
                    FleeFromTarget();
                    Debug.Log("Fleeing");

                    if(distanceToTarget >= chaseDistance) 
                    {

                        if (UnityEngine.Random.value < 0.3f)
                        {
                            Debug.Log("Healing 30% chance");
                            ChangeState(AIState.Healing);
                            // currentState = AIState.Healing;
                        }
                        else
                        {
                            //  Debug.Log("Patrollin coz no chance");
                            ChangeState(AIState.Patrolling);
                         //   currentState = AIState.Patrolling;
                            GenerateRandomPatrolPoint();
                        }


                    }  
                }
                

                break;

            

        }

    }


    public void PrintStateData()
    {
        Debug.Log("AI State Data:");
        Debug.Log("AIState | Frequency | Total Duration | Avg. Duration");

        foreach (AIState state in Enum.GetValues(typeof(AIState)))
        {
            int frequency = stateChangeCounts.ContainsKey(state) ? stateChangeCounts[state] : 0;
            float totalDuration = stateDurations.ContainsKey(state) ? stateDurations[state] : 0;
            float avgDuration = frequency > 0 ? totalDuration / frequency : 0;

            Debug.Log($"{state} | {frequency} | {totalDuration} s | {avgDuration} s");
        }
    }







    private void ChangeState(AIState newState)
    {
        if (currentState == newState)
            return;

        previousState = currentState;
        currentState = newState;

        // Track state change counts and durations
        if (stateChangeCounts.ContainsKey(previousState))
        {
            stateChangeCounts[previousState]++;
        }
        else
        {
            stateChangeCounts[previousState] = 1;
        }

        float previousStateDuration = Time.time - stateStartTime;
        stateStartTime = Time.time;

        if (stateDurations.ContainsKey(previousState))
        {
            stateDurations[previousState] += previousStateDuration;
        }
        else
        {
            stateDurations[previousState] = previousStateDuration;
        }
    }








    private void ChangeState1(AIState newState)
    {
        float stateChangeDuration = Time.time - lastStateChangeTime;
        Debug.Log($"State changed from {currentState} to {newState} in {stateChangeDuration} seconds");
        currentState = newState;
        lastStateChangeTime = Time.time;
    }


    private void FleeFromTarget() {

        Debug.Log("fleeing from the target");
        Vector3 fleeDirection = (transform.position - target.position).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * patrolDistance;

        //speed *= 3f;
        speed = fleeSpeed;

        PathRequestManager.RequestPath(transform.position, fleeTarget, OnPathFound);






    }

    void ReturnToPatrol()
    {
        currentState = AIState.Patrolling;
       hasEnteredFleeState = false;
        GenerateRandomPatrolPoint();
        speed = originalSpeed;
    }


   


    private void Heal()
    {
        // Call the Heal method in the Health class
        health.Heal(2f);
      //  health.Heal(healRate * Time.deltaTime);

        // Transition back to patrolling if health is above the heal threshold
        if (health.GetCurrentHealthRatio() >= 1 - healThreshold)
        {
            currentState = AIState.Patrolling;
            GenerateRandomPatrolPoint();
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
