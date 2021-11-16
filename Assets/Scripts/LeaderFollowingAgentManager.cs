using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderFollowingAgentManager : MonoBehaviour
{
    private GameObject Lead;
    private UnityEngine.AI.NavMeshAgent nma;
    private UnityEngine.AI.NavMeshAgent leadnma;
    private Agent leader;
    public float constant;

    private static List<Agent> agents = new List<Agent>();
    public const float UPDATE_RATE = 0.0f;
    private const int PATHFINDING_FRAME_SKIP = 25;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(Run());
    }

    void Start(){
        nma = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Lead =  GameObject.Find("Agents");
        leader = Lead.GetComponentInChildren(typeof(Agent)) as Agent;
        agents.AddRange(GetComponentsInChildren<Agent>());   
        leadnma = Lead.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
    }
    IEnumerator Run()
    {
        yield return null;

        for (int iterations = 0; ; iterations++)
        {
            if (iterations % PATHFINDING_FRAME_SKIP == 0)
            {
            
            Vector3 leaderPos = leader.transform.position;
            
            SetAgentDestinations(leader.transform.position - constant*leadnma.velocity.normalized);

            foreach (var agent in agents)
            {
                agent.ApplyForce(0);
            }

            if (UPDATE_RATE == 0)
            {
                yield return null;
            } else
            {
                yield return new WaitForSeconds(UPDATE_RATE);
            }
        }
    }
    }
       public void SetAgentDestinations(Vector3 destination)
    {
        UnityEngine.AI.NavMeshHit hit;
        UnityEngine.AI.NavMesh.SamplePosition(destination, out hit, 10, UnityEngine.AI.NavMesh.AllAreas);
        
        foreach (var agent in agents)
        {
            agent.ComputePath(hit.position);
        }
    }

        public static bool IsAgent(GameObject obj)
    {
        return obj.name.Contains("Leader Following");
    }

}
