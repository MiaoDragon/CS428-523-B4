using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class CameraController : MonoBehaviour
{
    public Camera camera;
    public GameObject manager;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // get input from right mouse click, and set agent destinations
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                //Transform objectHit = hit.transform;
                Vector3 hitPosition = hit.point;
                manager.GetComponent<AgentManager>().destination = hitPosition;
                manager.GetComponent<AgentManager>().SetAgentDestinations(hitPosition);

                
            }
        }

    }
}
