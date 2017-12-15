using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follow : MonoBehaviour {

    private bool isAttached = false;
    public GameObject mediumCubeLogic;


    public void attach()
    {
        isAttached = true;
        mediumCubeLogic.GetComponent<Rigidbody>().isKinematic = true;
    }

    public void dettach()
    {
        isAttached = false;
        mediumCubeLogic.GetComponent<Rigidbody>().isKinematic = false;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isAttached)
        {
            mediumCubeLogic.transform.position = this.transform.position;
            mediumCubeLogic.transform.rotation = this.transform.rotation;
        }
	}
}
