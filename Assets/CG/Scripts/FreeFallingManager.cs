using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class FreeFallingManager : MonoBehaviour
{

    public float mass;
    FixedJoint joint;
    Interactable otherHand;
    //List<Stackable> stackList;

    public bool isColliding;

    // Use this for initialization
    void Start()
    {
        //stackList = new List<Stackable>();

    }
    //ALWAYS TOP
    private void OnCollisionEnter(Collision collision)
    {

        //// Remove this script if the object collide with the podium or plataform 
        //if (collision.gameObject.name.Equals("Podium") || collision.gameObject.name.Equals("Platform"))
        //{
        //    Destroy(this);
        //    return;
        //}

        //RecursiveFind(collision.gameObject.GetComponent<Stackable>(), ref stackList);
        //Debug.Log("isflying: " + stackList.Count);


        //this.GetComponent<Rigidbody>().mass = 0;
        //joint = this.gameObject.AddComponent<FixedJoint>();
        //joint.connectedBody = collision.gameObject.GetComponent<Rigidbody>();



        ////Destroy(this);

        bool isOnPodium = FindPodium(collision.gameObject.GetComponent<Stackable>());


        // Remove this script if the object collide with the podium or plataform 
        if (isOnPodium || collision.gameObject.name.Equals("Podium") || collision.gameObject.name.Equals("Platform"))
        {
            Destroy(this);
            return;
        }

        // It should be an iteractable object 
        if (collision.gameObject.GetComponent<Stackable>() == null) return;

        if (collision.gameObject.GetComponent<FixedJoint>() == null)
        {

            StartCoroutine(WaitToAttach(collision));
        }

        

    }

    private void OtherDeatch(Hand hand)
    {
        Destroy(joint);
        otherHand.onDetachedFromHand -= OtherDeatch;
        this.GetComponent<Rigidbody>().mass = 1;
        Debug.Log(this.gameObject.name);
        Destroy(this);
    }


    bool FindPodium(Stackable baseObject)
    {
        bool found = false;
        if (baseObject !=null && baseObject.baseStackable != null)
        {
            if (baseObject.baseStackable.gameObject.name.Equals("Podium"))
            {
                //list.Add(baseObject.baseStackable.GetComponent<Stackable>());
                return true;
            }
            found = FindPodium(baseObject.baseStackable.GetComponent<Stackable>());
        }

        if (found)
            return true;
        else
            return false;

    }


    //Workaround because the OnCollisionEnter is notifing too early (before the object collision)
    IEnumerator WaitToAttach(Collision col)
    {
        
        yield return new WaitForSeconds(0.05f); 
        Debug.Log(col);
        this.GetComponent<Rigidbody>().mass = 0;
        joint = col.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = this.gameObject.GetComponent<Rigidbody>();

        col.gameObject.GetComponent<Stackable>().VisualRepresentation.gameObject.GetComponent<Grab>().stackList.Add(gameObject.GetComponent<Stackable>());
        //col.gameObject.GetComponent<Stackable>().VisualRepresentation.gameObject.GetComponent<Grab>().masses.Add(name, gameObject.GetComponent<Rigidbody>().mass);
        otherHand = col.gameObject.GetComponent<Stackable>().VisualRepresentation.gameObject.GetComponent<Interactable>();
        otherHand.onDetachedFromHand += OtherDeatch;
        
        Destroy(this);

    }


}
