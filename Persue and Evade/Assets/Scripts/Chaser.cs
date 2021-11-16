using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Chaser : MonoBehaviour
{
    private NavMeshAgent Mob;
    public GameObject Player;


    public float MobDistanceRun = 4.0f;
    public Vector3 position;

    // Start is called before the first frame update
    void Start()
    {
        Mob = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Runner");
        GameObject closestTarget = null;

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach(GameObject target in targets)
        {
            Vector3 diff = target.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if(curDistance < distance)
            {
                closestTarget = target;
                distance = curDistance;

            }
        }

        Player = closestTarget;




       


        float distance2 = Vector3.Distance(transform.position, Player.transform.position);

        if (distance2 < MobDistanceRun)
        {
            Vector3 dirToPlayer = transform.position - Player.transform.position;
            Vector3 newPos = transform.position - dirToPlayer;

            Mob.SetDestination(newPos);
        }
    }
}
