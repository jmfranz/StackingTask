using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class printInertiaTensor : MonoBehaviour {
    void Update()
    {
        Debug.Log(GetComponent<Rigidbody>().inertiaTensor);
    }
}
