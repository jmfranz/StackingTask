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

    private void OnCollisionStay(Collision collision)
    {
        collidedObj = collision;
        var joint = this.gameObject.GetComponent<FixedJoint>();
        if (joint != null && joint.connectedBody.gameObject.name.Equals(collision.gameObject.name))
            isColliding = false;
        else {
            isColliding = true;
            
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        collidedObj = collision;
    }
}
