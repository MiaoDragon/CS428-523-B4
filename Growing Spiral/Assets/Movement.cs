using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public GameObject center;

    private void Update()
    {
        Vector3 direction = center.transform.position - transform.position;
        direction = Quaternion.Euler(0, 95, 0) * direction;
        float distanceThisFrame = 5 * Time.deltaTime;

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

    }
}

