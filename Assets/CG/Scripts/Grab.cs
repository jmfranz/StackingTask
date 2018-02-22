using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;



[RequireComponent(typeof(Interactable))]
public class Grab : MonoBehaviour {
    public GameObject imaginaryPrefab;
    public Material hoverMat;


    private GameObject imaginary;
    private GameObject logicObject;

    public List<Stackable> stackList;


    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
    private Color materialOriginalColor;


    //-------------------------------------------------
    void Awake() {
    }

    void Start() {

        materialOriginalColor = GetComponent<Renderer>().material.color;

    }


    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand) {
        this.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
    }


    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand) {
        this.GetComponent<Renderer>().material.SetColor("_Color", materialOriginalColor);
    }


    //-------------------------------------------------
    // Called every Update() while a Hand is hovering over this object
    //-------------------------------------------------
    private void HandHoverUpdate(Hand hand) {

        if (hand.GetStandardInteractionButtonDown()) {// || ((hand.controller != null) && hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))) {
            if (hand.currentAttachedObject != gameObject) {

                hand.controller.TriggerHapticPulse();

                //Find the equivalent logic obj
                logicObject = GameObject.Find(this.transform.name + " Logic");

                // Check if the other hand has objects attached. 
                // If so, we need to find out if the other hand is grabbing the object grabbed with this hand.
                // If it is been grabbed, we need to remove the joint that are fixing it to the other hand and attach to this hand.

                if (hand.otherHand.gameObject.GetComponentInChildren<SimpleSpring>() != null) { //if the other hand is grabbing an object
                    //Find this object in the other hand
                    var otherHandObj = hand.otherHand.gameObject.GetComponentInChildren<SimpleSpring>().logic;
                    DetachFromOtherHand(logicObject, otherHandObj);                    
                }
                   
                
                AttachAboveObjects(logicObject.GetComponent<Stackable>());

                //Instantiate and imaginary god-object
                imaginary = Instantiate(imaginaryPrefab);

                //Sets the god-object position to the same as the visual representation
                imaginary.transform.position = this.transform.position;
                imaginary.transform.rotation = this.transform.rotation;
                imaginary.transform.parent = hand.transform;

                //Disable the logic object gravity so we can move it around freely
                var logicRb = logicObject.GetComponent<Rigidbody>();
                logicRb.useGravity = false;

                //Add a script that inform us if the object is colliding
                logicObject.AddComponent<NotifyCollision>();

                var simpleSpring = imaginary.GetComponent<SimpleSpring>();
                //Attaches a simple spring joint from the god-objce to the logic representation
                simpleSpring.logic = logicObject;
                simpleSpring.pivot = imaginary;
                simpleSpring.offset = logicObject.transform.rotation;

                // Call this to continue receiving HandHoverUpdate messages,
                // and prevent the hand from hovering over anything else
                hand.HoverLock(GetComponent<Interactable>());

                // Attach this object to the hand
                hand.AttachObject(imaginary, attachmentFlags);


            }
        }
        if (hand.GetStandardInteractionButtonUp())// || hand.controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Grip) && hand.currentAttachedObject != null)
        {
            Destroy(imaginary);
            Destroy(logicObject.GetComponent<NotifyCollision>());
            //Resets original settings of the logic object
            logicObject.GetComponent<Rigidbody>().useGravity = true;
            DetachAboveObjects(logicObject);

            //Remove its mass and set tag to free falling option
            //NOPE!

            //if (logicObject.gameObject.GetComponent<FreeFallingManager>() == null)
            //    logicObject.gameObject.AddComponent<FreeFallingManager>();


            // Detach this object from the hand
            hand.DetachObject(imaginary);
            // Call this to undo HoverLock
            hand.HoverUnlock(GetComponent<Interactable>());

        }

    }


    void DetachFromOtherHand(GameObject thisHandObj, GameObject otherHandObj) {

        var joints = otherHandObj.GetComponents<FixedJoint>();
        if (joints == null) return;
        foreach (var joint in joints) {
            if (joint != null) {
                var aboveObjInOtherHand = joint.connectedBody.gameObject;
                if (thisHandObj.name.Equals(aboveObjInOtherHand.name)) {
                    Destroy(joint);
                    break;
                } else
                    DetachFromOtherHand(thisHandObj, aboveObjInOtherHand);

            }
        }
    }

    //-------------------------------------------------
    // Called when this GameObject becomes attached to the hand
    //-------------------------------------------------
    private void OnAttachedToHand(Hand hand) {
        Debug.Log("OnAttachedToHand");
    }


    //-------------------------------------------------
    // Called when this GameObject is detached from the hand
    //-------------------------------------------------
    private void OnDetachedFromHand(Hand hand) {
        Debug.Log("OnDetachedFromHand");
    }


    //-------------------------------------------------
    // Called every Update() while this GameObject is attached to the hand
    //-------------------------------------------------
    private void HandAttachedUpdate(Hand hand) {
        Debug.Log("HandAttachedUpdate");
    }


    //-------------------------------------------------
    // Called when this attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusAcquired(Hand hand) {
    }


    //-------------------------------------------------
    // Called when another attached GameObject becomes the primary attached object
    //-------------------------------------------------
    private void OnHandFocusLost(Hand hand) {
    }

    //-------------------------------------------------
    // Search for objects stacked above that are fixed with a joint and detach
    //-------------------------------------------------
    public void DetachAboveObjects(GameObject obj) {

        var joints = obj.GetComponents<FixedJoint>();
        if (joints == null) return;
        foreach (var joint in joints) {
            if (joint != null) {
                var connectedObj = joint.connectedBody.gameObject;
                DetachAboveObjects(connectedObj);
                Destroy(joint);
                connectedObj.GetComponent<Rigidbody>().useGravity = true;
                var objVisual = connectedObj.GetComponent<Stackable>().VisualRepresentation;
                objVisual.gameObject.GetComponent<Renderer>().material.SetColor("_Color", objVisual.GetComponent<Grab>().materialOriginalColor);
            }
        }
    }

    //-------------------------------------------------
    // Search for objects stacked above the grabbed object, attach them with the grabbed object and change their material color.
    //-------------------------------------------------
    public void AttachAboveObjects(Stackable baseObj) {

        var logics = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in logics) {
            if (obj.baseStackable != null) // if the obj has an obj below
                if (obj != baseObj) //if the obj is not the grabbed obj
                    if (obj.baseStackable.name.Equals(baseObj.name)) {
                        AttachAboveObjects(obj);
                        var joint = baseObj.gameObject.AddComponent<FixedJoint>();
                        joint.connectedBody = obj.GetComponent<Rigidbody>();
                        obj.VisualRepresentation.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                        obj.GetComponent<Rigidbody>().useGravity = false;
                    }
        }
    }


}

