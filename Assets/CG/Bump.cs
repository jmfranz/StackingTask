using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bump : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnGUI()
    {
        if (GUILayout.Button("Bump"))
            this.GetComponent<Rigidbody>().AddForce(Vector3.up * 10000);
    }
}
