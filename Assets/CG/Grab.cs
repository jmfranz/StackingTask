using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;



[RequireComponent (typeof(Interactable))]
public class Grab : MonoBehaviour {
    public GameObject imaginaryPrefab;

    private GameObject imaginary;
    private GameObject logicObject;
    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);

    //-------------------------------------------------
    void Awake()
    {
    }


    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called every Update() while a Hand is hovering over this object
    //-------------------------------------------------
    private void HandHoverUpdate(Hand hand)
    {
        if (hand.GetStandardInteractionButtonDown() || ((hand.controller != null) && hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip)))
        {
            if (hand.currentAttachedObject != gameObject)
            {
                logicObject = GameObject.Find(this.transform.name + " Logic");
                imaginary = Instantiate(imaginaryPrefab);
                var connector = imaginary.GetComponent<jointConnector>();


                imaginary.transform.position = gameObject.transform.position;
                imaginary.transform.rotation = gameObject.transform.rotation;
                imaginary.transform.parent = gameObject.transform;
                connector.target = logicObject.transform;

                
                logicObject.GetComponent<Rigidbody>().useGravity = false;
                var notify = logicObject.AddComponent<notifyCollision>();
                notify.collisionEnterEvent += connector.collisionEnter;
                notify.collisionExitEvent += connector.collisionExit;



                // Call this to continue receiving HandHoverUpdate messages,
                // and prevent the hand from hovering over anything else
                hand.HoverLock(GetComponent<Interactable>());

                // Attach this object to the hand
                hand.AttachObject(gameObject, attachmentFlags);
                
            }
        }
        if (hand.GetStandardInteractionButtonUp()  || hand.controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Grip) && hand.currentAttachedObject != null)
        {
            Destroy(imaginary);
            Destroy(logicObject.GetComponent<notifyCollision>());
            logicObject.GetComponent<Rigidbody>().useGravity = true;
            logicObject.GetComponent<Stackable>().enabled = true;
            // Detach this object from the hand
            hand.DetachObject(gameObject);
            // Call this to undo HoverLock
            hand.HoverUnlock(GetComponent<Interactable>());
        }
        
    }


    //-------------------------------------------------
    // Called when this GameObject becomes attached to the hand
    //-------------------------------------------------
    private void OnAttachedToHand(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called when this GameObject is detached from the hand
    //-------------------------------------------------
    private void OnDetachedFromHand(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called every Update() while this GameObject is attached to the hand
    //-------------------------------------------------
    private void HandAttachedUpdate(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called when this attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusAcquired(Hand hand)
    {
    }


    //-------------------------------------------------
    // Called when another attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusLost(Hand hand)
    {
    }
}

