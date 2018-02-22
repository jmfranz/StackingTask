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
    private GameObject pivot;
    public List<Stackable> stackList;
    //public Dictionary<string, float> masses;

    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers);
    private Color materialOriginalColor;

    //Drag values from the original logic objects
    //float oldDrag, oldAngularDrag;

    //-------------------------------------------------
    void Awake() {
    }

    void Start() {

        materialOriginalColor = GetComponent<Renderer>().material.color;

        //masses = new Dictionary<string, float>();
        //foreach (var obj in FindObjectsOfType<Stackable>())
        //    masses.Add(obj.name, obj.GetComponent<Rigidbody>().mass);
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
                AttachAboveObjects();

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
                //foreach (var obj in stackList) {
                //    obj.GetComponent<Rigidbody>().useGravity = false;
                //}

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

            foreach (var obj in stackList) {

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

    Dictionary<string, bool> visited;


    public void newAttachAboveObjects() {


    }

    public void DetachAboveObjects(GameObject obj) {

        var joint = obj.GetComponent<FixedJoint>();
        if ( joint != null) {
            var connectedObj = joint.connectedBody.gameObject;
            DetachAboveObjects(connectedObj);
            Destroy(joint);
            connectedObj.GetComponent<Rigidbody>().useGravity = true;
            //connectedObj. .VisualRepresentation.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        }
    }

    public void AttachAboveObjects() {


        //stackList = new List<Stackable>();
        //RecursiveFind(logicObject.GetComponent<Stackable>(), ref stackList);
        //stackList.Reverse();
        RecursiveFind2(logicObject.GetComponent<Stackable>());

        //visited = new Dictionary<string, bool>();
        //foreach(var s in stackList) {
        //    visited.Add(s.name, false);
        //}

        //foreach (var s in stackList)
        //    DFS(s);

        //if (stackList.Count > 0) {
        //    var joint = logicObject.gameObject.AddComponent<FixedJoint>();
        //    joint.connectedBody = stackList[0].GetComponent<Rigidbody>();
        //    //joint.connectedBody.mass = 0;

        //    for (int i = 0; i < stackList.Count; i++) {
        //        var obj = stackList[i];
        //        var visual = obj.VisualRepresentation;

        //        visual.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);

        //        if (i < stackList.Count - 1) {
        //            var innerJoint = obj.gameObject.AddComponent<FixedJoint>();
        //            innerJoint.connectedBody = stackList[i + 1].GetComponent<Rigidbody>();
        //            //innerJoint.connectedBody.mass = 0;
        //        }

        //    }
        //}


    }

    void RecursiveFind2(Stackable baseObject) {
        var logics = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in logics) {
            if (obj.baseStackable != null) // if the obj has an obj below
                if (obj != baseObject) //if the obj is not the grabbed obj
                    if (obj.baseStackable.name.Equals(baseObject.name)) {
                        RecursiveFind2(obj);
                        var joint = baseObject.gameObject.AddComponent<FixedJoint>();
                        joint.connectedBody = obj.GetComponent<Rigidbody>();
                        obj.VisualRepresentation.gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                        obj.GetComponent<Rigidbody>().useGravity = false;
                    }
        }
    }

    void RecursiveFind(Stackable baseObject, ref List<Stackable> list) {
        var logics = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in logics) {
            if (obj.baseStackable != null) // if the obj has an obj below
                if (obj != baseObject) //if the obj is not the grabbed obj
                    if (obj.baseStackable.name.Equals(baseObject.name)) {
                        RecursiveFind(obj, ref list);
                        list.Add(obj);
                    }
        }

    }

    void DFS(Stackable obj) {
        if (!visited[obj.name]) {
            visited[obj.name] = true;
            foreach (var o in obj.VisualRepresentation.GetComponent<Grab>().stackList)
                DFS(o);
        }
    }

    void Update() {
    }

}

