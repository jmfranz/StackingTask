using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class FreeFallingManager : MonoBehaviour
{

    public float mass;
    FixedJoint joint;
    Interactable otherHand;
    List<Stackable> stackList;


    // Use this for initialization
    void Start()
    {
        stackList = new List<Stackable>();

    }
    //ALWAYS TOP
    private void OnCollisionEnter(Collision collision)
    {
        RecursiveFind(collision.gameObject.GetComponent<Stackable>(), ref stackList);
        Debug.Log("isflying: " + stackList.Count);

        if (collision.gameObject.name.Equals("Podium") || collision.gameObject.name.Equals("Platform"))
        {
            Destroy(this);
            return;

        }


        this.GetComponent<Rigidbody>().mass = 0;
        joint = this.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = collision.gameObject.GetComponent<Rigidbody>();

        otherHand = collision.gameObject.GetComponent<Stackable>().VisualRepresentation.gameObject.GetComponent<Interactable>();
        otherHand.onDetachedFromHand += OtherDeatch;


        //Destroy(this, 1);
    }

    private void OtherDeatch(Hand hand)
    {
        Destroy(joint);
        otherHand.onDetachedFromHand -= OtherDeatch;
        this.GetComponent<Rigidbody>().mass = 1;
        Debug.Log(this.gameObject.name);
        Destroy(this);
    }


    void RecursiveFind(Stackable baseObject, ref List<Stackable> list)
    {
        if (baseObject.baseStackable != null)
        {
            if (baseObject.baseStackable.gameObject.name.Equals("Podium"))
            {
                list.Add(baseObject.baseStackable.GetComponent<Stackable>());
                return;
            }
            RecursiveFind(baseObject.baseStackable.GetComponent<Stackable>(), ref list);
        }
            
        
        return ;

    }


}
