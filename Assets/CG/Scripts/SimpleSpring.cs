using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpring : MonoBehaviour {

    public GameObject logic;
    public GameObject pivot;
    public Quaternion offset;

    public float debugv = 0;
    public GameObject[] stack;

    public const int DEFAULT_K = 150;

    public bool isColliding;

    [Range(0, 1000)]
    public int k;

	// Use this for initialization
	void Start () {

        k = DEFAULT_K;
	}

    // Update is called once per frame
    void FixedUpdate () {
        if (pivot == null)
            return;
        isColliding = logic.GetComponent<NotifyCollision>().isColliding;
        Vector3 dir = logic.transform.position - pivot.transform.position;

        //Nao desliza :)

        if (isColliding && dir.y > 0.0f) {
            dir = new Vector3(dir.x,0.014f, dir.z);
            k = 50;
        } else
            k = DEFAULT_K;
        //isColiding = false;
        if (dir.magnitude > 0.1f) { dir.Normalize(); dir *= 0.1f; }
        logic.GetComponent<Rigidbody>().velocity = dir * -k * 0.1f;
        
        //logic.GetComponent<Rigidbody>().AddForce(dir * -k * 0.1f);

        //var inv = Quaternion.Inverse(offset) *  pivot.transform.rotation;
        ////inv.w = -1 * inv.w;
        logic.transform.rotation = pivot.transform.rotation;
	}



}
