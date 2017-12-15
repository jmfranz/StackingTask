using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jointConnector : MonoBehaviour {

    public Transform target;

    
    private bool isColliding;
    public Vector3 startLocalPos;

    
    private ConfigurableJoint joint;

    // Use this for initialization
    void Start () {

        
        isColliding = true;
        
        joint = GetComponent<ConfigurableJoint>();
        joint.connectedBody = target.GetComponent<Rigidbody>();

    }

    public void collisionExit()
    {
        isColliding = false;
        joint.connectedBody = null;
        joint.connectedAnchor = Vector3.zero;
        // target.GetComponent<Stackable>().enabled = false;

    }

    public void collisionEnter()
    {
        isColliding = true;
        joint.connectedBody = target.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update () {

        if (!isColliding)
        {
            target.position = this.transform.position;
            target.rotation = this.transform.rotation;
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("STOP MOTHEFUKA!"))
            GetComponent<Rigidbody>().isKinematic = false;
            //target.GetComponent<Rigidbody>().velocity = Vector3.zero;
            
    }
}

