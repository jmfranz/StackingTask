using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpring : MonoBehaviour {

    public GameObject logic;
    public GameObject pivot;
    public Quaternion offset;

    public float debugv = 0;

    public const float DEFAULT_K = 150.0f;

    public bool isColliding;

    [Range(0, 1000)]
    public float k;

    // Use this for initialization
    void Start() {

        k = DEFAULT_K;
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (pivot == null) return;
        if (logic.GetComponent<NotifyCollision>() == null) return;

        isColliding = logic.GetComponent<NotifyCollision>().isColliding;

        Vector3 dir = logic.transform.position - pivot.transform.position;
        var qntObjs = logic.GetComponent<Stackable>().VisualRepresentation.GetComponent<Grab>().qntObjsAbove;
        //Nao desliza :)

        if (isColliding && dir.y > 0.0f) {
            dir = new Vector3(dir.x, 0.014f, dir.z);
            k = 50.0f;
        } else {
            if (qntObjs == 0)
                k = DEFAULT_K;
            else //adapatative spring resistence
                k = (DEFAULT_K * qntObjs * 0.5f) + DEFAULT_K;
        }


        if (dir.magnitude > 0.1f) { dir.Normalize(); dir *= 0.1f; }
        logic.GetComponent<Rigidbody>().velocity = dir * -k * 0.1f;

        //logic.GetComponent<Rigidbody>().AddForce(dir * -k * 0.1f);

        //var inv = Quaternion.Inverse(offset) *  pivot.transform.rotation;
        ////inv.w = -1 * inv.w;
        logic.transform.rotation = pivot.transform.rotation;
    }



}
