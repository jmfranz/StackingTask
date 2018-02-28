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
    private Color hoverColor, selectColor;

    public int qntObjsAbove = 0;
    List<List<Stackable>> allStacks = new List<List<Stackable>>();

    private GameObject center;

    //-------------------------------------------------
    void Awake() {
    }

    void Start() {

        materialOriginalColor = GetComponent<Renderer>().material.color;
        hoverColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);
        selectColor = new Color(0.1f, 1.0f, 0.1f, 1.0f);
        logicObject = GameObject.Find(this.transform.name + " Logic");
    }


    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand) {
        this.GetComponent<MeshRenderer>().material.SetColor("_Color", hoverColor);
        DrawCenter();

        ////       

        ////
        allStacks.Clear();
        var allObjects = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in allObjects)
        {
            if (!obj.baseStackable.name.Equals("Podium")) continue;

            //var objCollision = logicObject.GetComponent<NotifyCollision>().collidedObj.contacts;
            

            List<Stackable> stack = new List<Stackable>();
            stack.Add(obj);
            FindAboveObjects(obj, ref stack);
            allStacks.Add(stack);
        }

        foreach(var list in allStacks){
            ProjectStackable(list[list.Count-1], list.Count);
        }
        Debug.Log(allStacks[0].Count + " " + allStacks[1].Count);
        ////
    }

    public void ProjectStackable(Stackable obj, int sizeList)
    {
        var objBox = obj.VisualRepresentation.gameObject.GetComponent<Renderer>();
        if (objBox == null) return;

        var pointMinCube = objBox.bounds.min;
        var pointMaxCube = objBox.bounds.max;

        var point4 = new Vector3(pointMinCube.x, pointMaxCube.y, pointMinCube.z);
        var point6 = new Vector3(pointMinCube.x, pointMaxCube.y, pointMaxCube.z);
        var point8 = new Vector3(pointMaxCube.x, pointMaxCube.y, pointMinCube.z);



        //DrawPoint(pointMaxCube);
        //DrawPoint(point6);
        //DrawPoint(point4);
        //DrawPoint(point8);

        float step = 0.01f;


        for (float x = point6.x; x < pointMaxCube.x; x += step)
        {
            for (float z = point6.z; z > point4.z; z -= step)
            {
                var p = new Vector3(x, pointMaxCube.y, z);
                

                RaycastHit[] hits;
                var down = obj.gameObject.transform.up;
                down.y = -down.y;

                hits = Physics.RaycastAll(p, down, 100.0F);
                //Debug.Log("leght " +hits.Length + hits[0].collider.gameObject + hits[1].collider.gameObject);
                //Debug.Log(hits[1].collider.gameObject);
                //Debug.DrawRay(p,down);

                Debug.Log(hits.Length + " " + sizeList);
                if (hits.Length == sizeList)
                    DrawPoint(p);
                /*
                foreach ( var a in hits )
                {
                    Debug.Log(a.collider.gameObject);
                }*/
            }
        }
        /*
        float m = (pointMaxCube.z - point6.z) / (pointMaxCube.x - point6.x);
        for (float x = point6.x; x < pointMaxCube.x; x += step) {
            float z = m * (x - point6.x) + point6.z;
             DrawPoint(new Vector3(x, pointMaxCube.y, z));
        }


        float m2 = (point4.x - point6.x) / (point4.z - point6.z);
        Debug.Log(point6.z + " " + point4.z);
        for (float z = point6.z; z > point4.z; z -= step)
        {
            
            float x = m2 * (z - point6.z) + point6.x;
            DrawPoint(new Vector3(x, pointMaxCube.y, z));

        }*/


    }

    public void DrawPoint(Vector3 point)
    {
        var sphere1cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        sphere1cube.transform.position = point;

        var scale = new Vector3(0.02f, 0.02f, 0.02f);

        sphere1cube.transform.localScale = scale;
    }


    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand) {
        this.GetComponent<Renderer>().material.SetColor("_Color", materialOriginalColor);        
        Destroy(center);
    }    
    public void DrawCenter()
    {
        center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        center.transform.position = transform.position;
        center.transform.rotation = transform.rotation;
        center.transform.localScale = new Vector3(0.005f, transform.localScale.y + 0.04f, 0.005f);
        center.GetComponent<Collider>().enabled = false;
        center.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
    }

    //-------------------------------------------------
    // Called every Update() while a Hand is hovering over this object
    //-------------------------------------------------
    private void HandHoverUpdate(Hand hand) {

        if (hand.GetStandardInteractionButtonDown()) {// || ((hand.controller != null) && hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))) {
            if (hand.currentAttachedObject != gameObject) {

                //hand.controller.TriggerHapticPulse();
                this.GetComponent<Renderer>().material.SetColor("_Color", selectColor);
                //Find the equivalent logic obj
                logicObject = GameObject.Find(this.transform.name + " Logic");



                // Check if the other hand has objects attached. 
                // If so, we need to find out if the other hand is grabbing the object grabbed with this hand.
                // If it is been grabbed, we need to remove the joint that is fixing it to the other hand and attach to this hand.
                if (hand.otherHand.gameObject.GetComponentInChildren<SimpleSpring>() != null) { //if the other hand is grabbing an object
                    //Find this object in the other hand
                    var otherHandObj = hand.otherHand.gameObject.GetComponentInChildren<SimpleSpring>().logic;
                    DetachFromOtherHand(logicObject, otherHandObj);
                }
                qntObjsAbove = 0;  
                AttachAboveObjects(logicObject.GetComponent<Stackable>(), ref qntObjsAbove);

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
                //logicObject.AddComponent<NotifyCollision>();

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

            var getLogicObjectCollision = logicObject.GetComponent<NotifyCollision>();
            //Stack to the other hand
            //If the objects is colliding when it is released AND it is not colliding with the podium AND the stack that the object is being attached is not colliding with the podium 
            if (getLogicObjectCollision != null && getLogicObjectCollision.isColliding && !getLogicObjectCollision.collidedObj.gameObject.name.Equals("Podium") && !FindPodium(getLogicObjectCollision.collidedObj.gameObject.GetComponent<Stackable>())) {
                AttachObjectToStack(logicObject, getLogicObjectCollision.collidedObj.gameObject);
            } else {
                //Resets original settings of the logic object
                DetachAboveObjects(logicObject);
                logicObject.GetComponent<Rigidbody>().useGravity = true;
            }

            Destroy(imaginary);
            //Destroy(logicObject.GetComponent<NotifyCollision>());
            this.GetComponent<Renderer>().material.SetColor("_Color", hoverColor);

            // Detach this object from the hand
            hand.DetachObject(imaginary);
            // Call this to undo HoverLock
            hand.HoverUnlock(GetComponent<Interactable>());

        }
    }


    void AttachObjectToStack(GameObject thisObj, GameObject baseObj) {
        var joint = baseObj.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = thisObj.GetComponent<Rigidbody>();
        //thisObj.GetComponent<Rigidbody>().useGravity = false;
        
    }

    //-------------------------------------------------
    // Search for objects stacked on the other hand and detach
    //-------------------------------------------------
    void DetachFromOtherHand(GameObject thisHandObj, GameObject otherHandObj) {

        var joints = otherHandObj.GetComponents<FixedJoint>();
        if (joints == null) return;
        foreach (var joint in joints) {
            var aboveObjInOtherHand = joint.connectedBody.gameObject;
            if (thisHandObj.name.Equals(aboveObjInOtherHand.name)) {
                Destroy(joint);
                break;
            } else
                DetachFromOtherHand(thisHandObj, aboveObjInOtherHand);
        }
    }

    //-------------------------------------------------
    // Search for objects stacked above that are fixed with a joint and detach
    //-------------------------------------------------
    public void DetachAboveObjects(GameObject obj) {

        var joints = obj.GetComponents<FixedJoint>();
        if (joints == null) return;
        foreach (var joint in joints) {
            var connectedObj = joint.connectedBody.gameObject;
            DetachAboveObjects(connectedObj);
            connectedObj.GetComponent<Rigidbody>().useGravity = true;
            Destroy(joint);
            var objVisual = connectedObj.GetComponent<Stackable>().VisualRepresentation;
            objVisual.gameObject.GetComponent<Renderer>().material.SetColor("_Color", objVisual.GetComponent<Grab>().materialOriginalColor);
        }
    }

    //-------------------------------------------------
    // Search for objects stacked above the grabbed object, attach them with the grabbed object and change their material color.
    //-------------------------------------------------
    public void AttachAboveObjects(Stackable baseObj, ref int qntObjsAbove) {
        var logics = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in logics) {
            if (obj.baseStackable != null) // if the obj has an obj below
                if (obj != baseObj) //if the obj is not the grabbed obj
                    if (obj.baseStackable.name.Equals(baseObj.name)) {
                        AttachAboveObjects(obj, ref qntObjsAbove);
                        if (baseObj.gameObject.GetComponent<FixedJoint>() != null) return;
                        var joint = baseObj.gameObject.AddComponent<FixedJoint>();
                        joint.connectedBody = obj.GetComponent<Rigidbody>();
                        obj.VisualRepresentation.gameObject.GetComponent<Renderer>().material.SetColor("_Color", selectColor);
                        obj.GetComponent<Rigidbody>().useGravity = false;
                        qntObjsAbove++;
                    }
        }
    }

    //-------------------------------------------------
    // Search for the object with the name "podium"
    //-------------------------------------------------
    bool FindPodium(Stackable obj) {
        bool found = false;
        if (obj != null && obj.baseStackable != null) {
            if (obj.baseStackable.gameObject.name.Equals("Podium"))
                return true;
            found = FindPodium(obj.baseStackable.GetComponent<Stackable>());
        }

        if (found)
            return true;
        else
            return false;

    }

    //-------------------------------------------------
    // 
    //-------------------------------------------------
    public void FindAboveObjects(Stackable baseObj, ref List<Stackable> objsAbove)
    {
        var logics = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in logics)
        {
            if (obj.baseStackable != null) // if the obj has an obj below
                if (obj != baseObj) //if the obj is not the grabbed obj
                    if (obj.baseStackable.name.Equals(baseObj.name))
                    {
                        objsAbove.Add(obj);
                        FindAboveObjects(obj, ref objsAbove);
                    }
        }
    }

}

