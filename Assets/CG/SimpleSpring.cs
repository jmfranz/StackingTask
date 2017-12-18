using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpring : MonoBehaviour {

    public GameObject logic;
    public GameObject pivot;
    public Quaternion offset;

    public GameObject[] stack;

    [Range(0, 1000)]
    public int k;

    bool isColiding;

	// Use this for initialization
	void Start () {
        isColiding = false;
	}

    public void collisionExit()
    {
        isColiding = false;
    }

    public void collisionEnter()
    {
        isColiding = true;
    }

    // Update is called once per frame
    void Update () {
        Vector3 dir = logic.transform.position - pivot.transform.position;
        if (isColiding && dir.y > 0)
            dir = new Vector3(dir.x, 0, dir.z);

        logic.GetComponent<Rigidbody>().AddForce(dir * -k);
        //var inv = Quaternion.Inverse(offset) *  pivot.transform.rotation;
        ////inv.w = -1 * inv.w;
        logic.transform.rotation = pivot.transform.rotation;
	}



}
