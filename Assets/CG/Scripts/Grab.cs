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
    private GameObject pivot;
    private List<Stackable> stackList;
    private Dictionary<string, float> masses;

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
        masses = new Dictionary<string, float>();
        foreach (var obj in FindObjectsOfType<Stackable>())
            masses.Add(obj.name, obj.GetComponent<Rigidbody>().mass);
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
                FindStack();

                //Creates a pivot point for the manipulation
                pivot = new GameObject("Pivot");

                //Sets the pivot as child of the hand
                pivot.transform.position = this.transform.position;
                pivot.transform.rotation = this.transform.rotation;
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

            //Remove its mass and set tag to free falling option
            //NOPE!
            if(logicObject.gameObject.GetComponent<FreeFallingManager>() == null)
                logicObject.gameObject.AddComponent<FreeFallingManager>();


            // Detach this object from the hand
            hand.DetachObject(gameObject);
            // Call this to undo HoverLock
            hand.HoverUnlock(GetComponent<Interactable>());

            var joint = logicObject.GetComponent<FixedJoint>();
            if (joint != null)
                Destroy(joint);

            foreach (var obj in stackList)
            {

                joint = obj.GetComponent<FixedJoint>();
                if (joint != null)
                    Destroy(joint);
                obj.GetComponent<Rigidbody>().mass = masses[obj.name];


                var visual = GameObject.Find(obj.name.Substring(0, obj.name.Length - 6));
                visual.gameObject.GetComponent<MeshRenderer>().material = startMaterial;
            }


            Destroy(pivot);
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


    public void FindStack()
    {
        stackList = new List<Stackable>();
        RecursiveFind(logicObject.GetComponent<Stackable>(), ref stackList);
        stackList.Reverse();

        if (stackList.Count > 0)
        {
            var joint = logicObject.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = stackList[0].GetComponent<Rigidbody>();
            joint.connectedBody.mass = 0;

            for (int i = 0; i < stackList.Count - 1; i++)
            {
                var obj = stackList[i];
                var innerJoint = obj.gameObject.AddComponent<FixedJoint>();
                innerJoint.connectedBody = stackList[i+1].GetComponent<Rigidbody>();
                innerJoint.connectedBody.mass = 0;

                var visual = GameObject.Find(obj.name.Substring(0, obj.name.Length - 6));
                visual.gameObject.GetComponent<MeshRenderer>().material = hoverMat;
            }

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

