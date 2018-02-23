using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyCollision : MonoBehaviour {

    //public delegate void collisionEnterDelegate();
    //public event collisionEnterDelegate collisionEnterEvent;

    //public delegate void collisionExitDelegate();
    //public event collisionExitDelegate collisionExitEvent;

    //public bool isColliding = false;

    //GameObject imaginary;

    public bool isColliding;
    public Collision collidedObj;

    private void Start() {
        isColliding = false;
        collidedObj = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
        collidedObj = collision;

    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        collidedObj = collision;
    }
}
