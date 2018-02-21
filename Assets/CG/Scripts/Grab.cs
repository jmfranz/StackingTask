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
    public List<Stackable> stackList;
    public Dictionary<string, float> masses;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
    private Color materialOriginalColor;

    //Drag values from the original logic objects
    float oldDrag, oldAngularDrag;

    //-------------------------------------------------
    void Awake()
    {
    }

    void Start()
    {


        materialOriginalColor = GetComponent<Renderer>().material.color;

        masses = new Dictionary<string, float>();
        foreach (var obj in FindObjectsOfType<Stackable>())
            masses.Add(obj.name, obj.GetComponent<Rigidbody>().mass);
    }


    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand)
    {
        this.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
    }


    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand)
    {
        this.GetComponent<Renderer>().material.SetColor("_Color", materialOriginalColor);
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

                hand.controller.TriggerHapticPulse();

                //Find the equivalent logic obj
                logicObject = GameObject.Find(this.transform.name + " Logic");
                FindStack();

                //Instantiate and imaginary god-object
                imaginary = Instantiate(imaginaryPrefab);

                //Sets the god-object position to the same as the visual representation
                imaginary.transform.position = this.transform.position;
                imaginary.transform.rotation = this.transform.rotation;
                imaginary.transform.parent = hand.transform;

                //Disable the logic object gravity so we can move it around freely
                var logicRb = logicObject.GetComponent<Rigidbody>();
                logicRb.useGravity = false;

                //Disable the gravity of stacked objects
                foreach (var obj in stackList) {
                    obj.GetComponent<Rigidbody>().useGravity = false;
                }

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
            //var logicRb = logicObject.GetComponent<Rigidbody>();
            //logicRb.drag = oldDrag;
            //logicRb.angularDrag = oldAngularDrag;

            //Remove its mass and set tag to free falling option
            //NOPE!

            //if (logicObject.gameObject.GetComponent<FreeFallingManager>() == null)
            //    logicObject.gameObject.AddComponent<FreeFallingManager>();


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
                //obj.GetComponent<Rigidbody>().mass = masses[obj.name];
                obj.GetComponent<Rigidbody>().useGravity = true;


                var visual = GameObject.Find(obj.name.Substring(0, obj.name.Length - 6));
                visual.gameObject.GetComponent<Renderer>().material.SetColor("_Color", materialOriginalColor);
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
            //joint.connectedBody.mass = 0;

            for (int i = 0; i < stackList.Count; i++)
            {
                var obj = stackList[i];

                var visual = GameObject.Find(obj.name.Substring(0, obj.name.Length - 6));
                visual.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);

                if (i < stackList.Count - 1) {
                    var innerJoint = obj.gameObject.AddComponent<FixedJoint>();    
                    innerJoint.connectedBody = stackList[i+1].GetComponent<Rigidbody>();
                    //innerJoint.connectedBody.mass = 0;
                }
                
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

    void Update() {
    }

}

