using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public float radius;
    public float mass;
    public float forceWeight;
    public float perceptionRadius;
    public bool isLeaderFollowingAgent;

    private List<Vector3> path;
    private NavMeshAgent nma;
    private Rigidbody rb;

    private HashSet<GameObject> perceivedNeighbors = new HashSet<GameObject>();
    private List<Vector3> NeighborPaths = new List<Vector3>();

    void Start()
    {
        path = new List<Vector3>();
        nma = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        gameObject.transform.localScale = new Vector3(2 * radius, 1, 2 * radius);
        nma.radius = radius;
        rb.mass = mass;
        GetComponent<SphereCollider>().radius = perceptionRadius / 2;
    }

    private void Update()
    {
        if (path.Count > 1 && Vector3.Distance(transform.position, path[0]) < 1.1f)
        {
            path.RemoveAt(0);
        } else if (path.Count == 1 && Vector3.Distance(transform.position, path[0]) < 2f)
        {
            path.RemoveAt(0);

            if (path.Count == 0)
            {
                if(isLeaderFollowingAgent){
                    GameObject Lead = GameObject.Find("Agents");
                    Agent leader = Lead.GetComponentInChildren(typeof(Agent)) as Agent;
                    if (leader == null){
                        gameObject.SetActive(false);
                        AgentManager.RemoveAgent(gameObject);    
                    }
                }
                else{
                    gameObject.SetActive(false);
                    AgentManager.RemoveAgent(gameObject);           
                }
            }
        }

        #region Visualization

        if (false)
        {
            if (path.Count > 0)
            {
                Debug.DrawLine(transform.position, path[0], Color.green);
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i + 1], Color.yellow);
            }
        }

        if (false)
        {
            foreach (var neighbor in perceivedNeighbors)
            {
                Debug.DrawLine(transform.position, neighbor.transform.position, Color.yellow);
            }
        }

        #endregion
    }

    #region Public Functions

    public void ComputePath(Vector3 destination)
    {
        nma.enabled = true;
        var nmPath = new NavMeshPath();
        nma.CalculatePath(destination, nmPath);
        path = nmPath.corners.Skip(1).ToList();
        //path = new List<Vector3>() { destination };
        //nma.SetDestination(destination);
        //nma.enabled = false;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    public Vector3 averageNeighborPath(){
        Vector3 average = new Vector3();
        int neighborCount =0;
        foreach (var agent in perceivedNeighbors){
            NeighborPaths.Add(agent.GetComponent<Agent>().getPath());
            neighborCount++;
        }
        foreach (var path in NeighborPaths){
            average += path;
        }
        average /= neighborCount;
        return average;
    }
        public bool hasNeighbors(){
        return perceivedNeighbors.Count() > 3;
    }

    public Vector3 getPath(){
            return path[0];
    }

    public bool hasPath(){
        if (path.Count > 0){
            return true;
        }
        return false;
    }

    #endregion

    #region Incomplete Functions
    public float speedLimit = 0.0f;
    private HashSet<GameObject> perceivedWalls = new HashSet<GameObject>();

    private Vector3 ComputeForce()
    {
        var force = Vector3.zero;
        return CalculateGoalForce() + CalculateAgentForce() + CalculateWallForce();
        if (force != Vector3.zero)
        {
            return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
        } else
        {
            return Vector3.zero;
        }
    }
     private Vector3 ComputeCFForce(float cfparam)
    {
        var force = Vector3.zero;
        return CalculateCFGoalForce(cfparam) + CalculateAgentForce() + CalculateWallForce();
        if (force != Vector3.zero)
        {
            return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
        } else
        {
            return Vector3.zero;
        }
    }

    private Vector3 CalculateGoalForce()
    {
        if (path.Count() == 0)
        {
            return Vector3.zero;
        }
        Vector3 goal = path[0];
        goal.y = transform.position.y;
        Vector3 direction = goal - transform.position;
        direction = direction / direction.magnitude;
        //float speed = nma.speed;
        float speed = speedLimit;
        Vector3 force = rb.mass * (speed * direction - GetVelocity()) / Parameters.T;
        return force;
    }

    private Vector3 CalculateCFGoalForce(float cfparam){
        if (path.Count() == 0)
        {
            return Vector3.zero;
        }
        Vector3 goal = path[0];
        goal.y = transform.position.y;
        Vector3 direction = new Vector3();
        if (hasNeighbors()){
            direction = ((1-cfparam)*goal + (cfparam*CalculateAverageNeighborForce())) - transform.position;
        }
        else{
            direction = goal - transform.position;
        }
        
        direction = direction / direction.magnitude;
        
        float speed = speedLimit;
        Vector3 force = rb.mass * (speed * direction - GetVelocity()) / Parameters.T;
        return force;
    }

        private Vector3 CalculateAverageNeighborForce(){
        Vector3 average = new Vector3();
        int count = 0;
        foreach (var agent in perceivedNeighbors){
            if (agent.GetComponent<Agent>().hasPath()){
                average += agent.GetComponent<Agent>().getPath();
                count++;
            }
        }
        average /= count;
        return average;
    }

    private Vector3 CalculateAgentForce()
    {
        // find the nearby agents within the radius
        if(!isLeaderFollowingAgent){
            return Vector3.zero;
        }
        Vector3 totalForce = Vector3.zero;
        foreach (var agent in perceivedNeighbors)
        {
            // check whether the collider is an object and not self
            Vector3 vec = agent.transform.position - transform.position;
            vec[1] = 0;  // set y to be zero
            float distance = vec.magnitude;
            float radiusSum = radius * 2;
            float penetrationDistance = radiusSum - distance;  // penetration >= 0: penetrate

            float repelForce = Parameters.A * Mathf.Exp(penetrationDistance / Parameters.B);
            float penetrated = (penetrationDistance > 0 ? penetrationDistance : 0);
            float penForce = penetrated * Parameters.k;
            Vector3 normal = -vec / vec.magnitude;
            Vector3 awayForce = normal * (repelForce + penForce);
            // compute sliding force
            // tangent direction
            Vector3 tangent = new Vector3(-normal.z, normal.y, normal.x);
            tangent = tangent / tangent.magnitude;
            // relative velocity self - the other
            Vector3 relative_vel = GetVelocity() - agent.GetComponent<Agent>().GetVelocity();        
            float tangent_vel = Vector3.Dot(relative_vel, tangent);        
            tangent_vel = -tangent_vel; // in the direction of friction
            Vector3 frictionForce = Parameters.Kappa * penetrated * tangent_vel * tangent;
            totalForce = totalForce + awayForce + frictionForce;
        }

        return totalForce;
    }

    private Vector3 CalculateWallForce()
    {
        // find the nearby agents within the radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, perceptionRadius);
        Vector3 totalForce = Vector3.zero;
        foreach (GameObject wall in perceivedWalls)
        {
            // check whether the collider is an object and not self
            // get the nearest face
            Vector3 wallPosition = wall.transform.position;
            Vector3 wallScale = wall.transform.localScale/2;
            Vector3 faceNorm1 = new Vector3(-1, 0, 0);  // at x_min
            Vector3 faceNorm2 = new Vector3(1, 0, 0);  // at x_max
            Vector3 faceNorm3 = new Vector3(0, 0, -1);  // at z_min
            Vector3 faceNorm4 = new Vector3(0, 0, 1);  // at z_max
                
            Vector3 vec = transform.position - wall.transform.position;
            vec[1] = 0;  // set y to be zero

            // find the min distance face
            float facePen1 = wallPosition[0] - wallScale[0] - transform.position[0] - radius;
            float facePen2 = transform.position[0] - radius - wallScale[0] - wallPosition[0];
            float facePen3 = wallPosition[2] - wallScale[2] - transform.position[2] - radius;
            float facePen4 = transform.position[2] - radius - wallScale[2] - wallPosition[2];

            float maxPen = -1000;
            Vector3 maxNorm = Vector3.zero;
                
            if (facePen1 > maxPen)
            {
                maxPen = facePen1;
                maxNorm = faceNorm1;
            }
            if (facePen2 > maxPen)
            {
                maxPen = facePen2;
                maxNorm = faceNorm2;
            }
            if (facePen3 > maxPen)
            {
                maxPen = facePen3;
                maxNorm = faceNorm3;
            }
            if (facePen4 > maxPen)
            {
                maxPen = facePen4;
                maxNorm = faceNorm4;
            }
            maxPen = -maxPen;
            float repelForce = Parameters.WALL_A * Mathf.Exp(maxPen / Parameters.WALL_B);        
            float penetrated = (maxPen > 0 ? maxPen : 0);
            float penForce = penetrated * Parameters.WALL_k;
            Vector3 normal = maxNorm;
            Vector3 awayForce = normal * (repelForce + penForce);
            // compute sliding force            
            // tangent direction
            Vector3 tangent = new Vector3(-normal.z, normal.y, normal.x);
            tangent = tangent / tangent.magnitude;
            // relative velocity self - the other
            Vector3 relative_vel = -GetVelocity();
            float tangent_vel = Vector3.Dot(relative_vel, tangent);
            Vector3 frictionForce = Parameters.WALL_Kappa * penetrated * tangent_vel * tangent;
            totalForce = totalForce + awayForce + frictionForce;

            //Debug.Break();
        }
        return totalForce;
    }

    public void ApplyForce(float crowd_param)
    {
        var force = ComputeForce();
                if (crowd_param > 0){
            force = ComputeCFForce(crowd_param);
            Debug.Log("Calculated cf force: " + force);
        }
        else{
            force = ComputeForce();
        }

        rb.AddForce(force * forceWeight, ForceMode.Force);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (AgentManager.IsAgent(other.gameObject))
        {
            perceivedNeighbors.Add(other.gameObject);
        }
        else if (WallManager.IsWall(other.gameObject))
        {
            perceivedWalls.Add(other.gameObject);
        }
         else if(LeaderFollowingAgentManager.IsAgent(other.gameObject)){
            perceivedNeighbors.Add(other.gameObject);
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (AgentManager.IsAgent(other.gameObject))
        {
            perceivedNeighbors.Remove(other.gameObject);
        }
        else if (WallManager.IsWall(other.gameObject))
        {
            perceivedWalls.Remove(other.gameObject);
        }
         else if(LeaderFollowingAgentManager.IsAgent(other.gameObject)){
            perceivedNeighbors.Remove(other.gameObject);
        }
    }



    public void OnCollisionEnter(Collision collision)
    {
        
    }

    public void OnCollisionExit(Collision collision)
    {
        
    }

    #endregion
}
