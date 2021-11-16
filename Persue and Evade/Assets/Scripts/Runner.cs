using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

public class Runner : MonoBehaviour
{

    NavMeshAgent _agent;
    public float agentDistanceRun = 4.0f;
    public GameObject Player;
    public GameObject Home1;
    public float EnemyDistanceRun = 4.0f;
    public BoxCollider Corner1;
    public BoxCollider Corner2;
    public BoxCollider Corner3;
    public BoxCollider Corner4;
    public BoxCollider Home;
    public bool running = false; 

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {

        GameObject[] targets = GameObject.FindGameObjectsWithTag("Chaser");
        GameObject closestTarget = null;
   

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject target in targets)
        {
            Vector3 diff = target.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closestTarget = target;
                distance = curDistance;

            }
        }

        if(running == false)
        {
            Vector3 newPos = Home1.transform.position;
         _agent.SetDestination(newPos);
       
        }
        if(running == true)
        {
            Player = closestTarget;
            float distance2 = Vector3.Distance(transform.position, Player.transform.position);


            if (distance2 < EnemyDistanceRun)
            {
                Vector3 dirToPlayer = transform.position - Player.transform.position;
                Vector3 newPos = transform.position + dirToPlayer;

                _agent.SetDestination(newPos);
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
       if (other == Corner1)
        {
            Debug.Log("Hit Corner");
            running = false; 
        }
        if (other == Corner2)
        {
            Debug.Log("Hit Corner");
            running = false;
        }
        if (other == Corner3)
        {
            Debug.Log("Hit Corner");
            running = false;
        }
        if (other == Corner4)
        {
            Debug.Log("Hit Corner");
            running = false;
        }
        if (other == Home)
        {
            Debug.Log("Hit Home");

            running = true; 
        }
    }



}
