using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puerta : MonoBehaviour
{
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
    }
}
