using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyCollision : MonoBehaviour {

    //public delegate void collisionEnterDelegate();
    //public event collisionEnterDelegate collisionEnterEvent;

    //public delegate void collisionExitDelegate();
    //public event collisionExitDelegate collisionExitEvent;

    //public bool isColliding = false;

    GameObject imaginary;

    private void Start() {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        var imaginary = GameObject.FindGameObjectWithTag("Imaginary");
        if (imaginary.GetComponent<SimpleSpring>() != null)
            imaginary.GetComponent<SimpleSpring>().isColliding = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        var imaginary = GameObject.FindGameObjectWithTag("Imaginary");
        if (imaginary.GetComponent<SimpleSpring>() != null)
            imaginary.GetComponent<SimpleSpring>().isColliding = false;
    }
}
