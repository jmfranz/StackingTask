using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;



[RequireComponent (typeof(Interactable))]
public class Grab : MonoBehaviour {
    public GameObject imaginaryPrefab;
    public Material hoverMat;


    private GameObject imaginary;
    private GameObject logicObject;
    private List<Stackable> stackList;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
    private Material startMaterial;

    //Drag values from the original logic objects
    float oldDrag, oldAngularDrag;

    //-------------------------------------------------
    void Awake()
    {
    }

    void Start()
    {
        startMaterial = this.GetComponent<MeshRenderer>().material;
    }


    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand)
    {
        this.GetComponent<MeshRenderer>().material = hoverMat;
    }


    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand)
    {
        this.GetComponent<MeshRenderer>().material = startMaterial;
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

                //hand.controller.TriggerHapticPulse();
                    

                //Find the equivalent logic obj
                logicObject = GameObject.Find(this.transform.name + " Logic");
                findStack();

                //Creates a pivot point for the manipulation
                var pivot = new GameObject("Pivot");
                //Sets the pivot as child of the hand
                pivot.transform.position = hand.transform.position;
                pivot.transform.rotation = hand.transform.rotation;
                pivot.transform.parent = hand.transform;

                //Instantiate and imaginary god-object
                imaginary = Instantiate(imaginaryPrefab);
                var simpleSpring = imaginary.GetComponent<SimpleSpring>();
                
                //Attaches a simple spring joint from the god-objce to the logic representation
                simpleSpring.logic = logicObject;
                simpleSpring.pivot = pivot;
                simpleSpring.offset = logicObject.transform.rotation;

                //Sets the god-object position to the same as the visual representation
                imaginary.transform.position = gameObject.transform.position;
                imaginary.transform.rotation = gameObject.transform.rotation;
                imaginary.transform.parent = gameObject.transform;

                //Stores the logic object drags
                var logicRb = logicObject.GetComponent<Rigidbody>();
                oldAngularDrag = logicRb.angularDrag;
                oldDrag = logicRb.drag;
                //Sets to values that helps our spring to converge
                logicRb.drag = 15;
                logicRb.angularDrag = 5;

                //Disable the logic object gravity so we can move it around freely
                logicObject.GetComponent<Rigidbody>().useGravity = false;
                //Add a script that inform us if the object is colliding
                var notify = logicObject.AddComponent<notifyCollision>();
                notify.collisionEnterEvent += simpleSpring.collisionEnter;
                notify.collisionExitEvent += simpleSpring.collisionExit;

                // Call this to continue receiving HandHoverUpdate messages,
                // and prevent the hand from hovering over anything else
                hand.HoverLock(GetComponent<Interactable>());

                // Attach this object to the hand
                hand.AttachObject(gameObject, attachmentFlags);
                
                
            }
        }
        if (hand.GetStandardInteractionButtonUp())// || hand.controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_Grip) && hand.currentAttachedObject != null)
        {
            Destroy(imaginary);
            Destroy(logicObject.GetComponent<notifyCollision>());
            //Resets original settings of the logic object
            logicObject.GetComponent<Rigidbody>().useGravity = true;
            var logicRb = logicObject.GetComponent<Rigidbody>();
            logicRb.drag = oldDrag;
            logicRb.angularDrag = oldAngularDrag;

            // Detach this object from the hand
            hand.DetachObject(gameObject);
            // Call this to undo HoverLock
            hand.HoverUnlock(GetComponent<Interactable>());
            foreach (var obj in stackList)
            {
                obj.transform.parent = GameObject.Find("Logic Representations (DO NOT EDIT)").transform.GetChild(1).transform;
                obj.GetComponent<Rigidbody>().isKinematic = false;
                obj.GetComponent<Collider>().isTrigger = false;
                var visual = GameObject.Find(obj.name.Substring(0, obj.name.Length - 6));
                visual.gameObject.GetComponent<MeshRenderer>().material = startMaterial;
            }
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

    private void OnGUI()
    {
        if (GUILayout.Button("HUE"))
            findStack();
    }

    void findStack()
    {
        stackList = new List<Stackable>();
        RecursiveFind(logicObject.GetComponent<Stackable>(), ref stackList);
        foreach(var obj in stackList)
        {
            obj.transform.parent = logicObject.transform;
            obj.GetComponent<Rigidbody>().isKinematic = true;
            obj.GetComponent<Collider>().isTrigger = true;
            var visual = GameObject.Find(obj.name.Substring(0, obj.name.Length - 6));
            visual.gameObject.GetComponent<MeshRenderer>().material = hoverMat;
        }
    }

    void RecursiveFind(Stackable baseObject, ref List<Stackable> list)
    {
        var logics = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in logics)
        {
            if (obj.baseStackable != null)
                if(obj != baseObject)
                    if (obj.baseStackable.name.Equals(baseObject.name))
                    {
                        RecursiveFind(obj, ref list);
                        list.Add(obj);
                    }
        }
        
    }




}

