using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class notifyCollision : MonoBehaviour {

    public delegate void collisionEnterDelegate();
    public event collisionEnterDelegate collisionEnterEvent;

    public delegate void collisionExitDelegate();
    public event collisionExitDelegate collisionExitEvent;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter");
        if (collisionEnterEvent != null)
            collisionEnterEvent.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit");
        if (collisionExitEvent != null)
            collisionExitEvent.Invoke();
    }



}
