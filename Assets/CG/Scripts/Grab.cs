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
    private List<Vector3> pointsSuggestion = new List<Vector3>();

    private GameObject drawnPoints;
    private GameObject center;

    //-------------------------------------------------
    void Awake() {
    }

    void Start() {

        materialOriginalColor = GetComponent<Renderer>().material.color;
        hoverColor = new Color(0.7f, 1.0f, 0.7f, 1.0f);
        selectColor = new Color(0.1f, 1.0f, 0.1f, 1.0f);
        logicObject = GameObject.Find(this.transform.name + " Logic");
        
        CreateUpperBoundArea();


    }
    public void GetObjectBounds(Stackable obj, ref Vector3[] points) {

        var objBox = obj.VisualRepresentation.gameObject.GetComponent<Renderer>();
        if (objBox == null) return;

        var pointMinCube = objBox.bounds.min;
        var pointMaxCube = objBox.bounds.max;

        var point4 = new Vector3(pointMinCube.x, pointMaxCube.y, pointMinCube.z);
        var point6 = new Vector3(pointMinCube.x, pointMaxCube.y, pointMaxCube.z);
        var point8 = new Vector3(pointMaxCube.x, pointMaxCube.y, pointMinCube.z);

        points[0] = point6;
        points[1] = pointMaxCube;
        points[2] = point4;
        points[3] = point8;

    }
    //-------------------------------------------------
    // Create upper area the object
    //-------------------------------------------------
    public void CreateUpperBoundArea() {
        
        var boundMid = gameObject.transform.localScale/2;
        float step = 0.004f;

        Matrix4x4 objMat = Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, Vector3.one);

        for (float x = -boundMid.x + 0.002f; x <= boundMid.x; x += step) {
            for (float z = -boundMid.z + 0.002f; z <= boundMid.z; z += step) {
                var p = new Vector3(x, boundMid.z+0.001f, z);
                var p2 = objMat.MultiplyPoint3x4(p);
                
                if (Physics.Raycast(p2, -gameObject.transform.up))
                {
                    pointsSuggestion.Add(p);
                }
            }
        }
    }
    //-------------------------------------------------
    // Find upper direcction the object
    //-------------------------------------------------
    public Vector3 UpperDirecction(Stackable obj)
    {
        Vector3 upAbsolute = Vector3.up;
        if (obj.baseStackable != null)
        {
            var vup = Vector3.Project(obj.gameObject.transform.up, Vector3.up);
            var vri = Vector3.Project(obj.gameObject.transform.right, Vector3.up);
            var vfo = Vector3.Project(obj.gameObject.transform.forward, Vector3.up);

            Vector3 max;

            if (vup.magnitude > vri.magnitude) { max = vup; upAbsolute = obj.gameObject.transform.up; } else { max = vri; upAbsolute = obj.gameObject.transform.right; }
            if (max.magnitude < vfo.magnitude) { max = vfo; upAbsolute = obj.gameObject.transform.forward; }

            upAbsolute = max.y >= 0 ? upAbsolute : -upAbsolute;
        }
        return upAbsolute;
    }
    //-------------------------------------------------
    // Verifying the ray projection of the stack
    //-------------------------------------------------
    public void ProjectStackable(Stackable obj, int listSize) {

        var pS = obj.VisualRepresentation.GetComponent<Grab>().pointsSuggestion;

        Vector3 upAbsolute = UpperDirecction(obj);

        Quaternion objQua = Quaternion.FromToRotation(obj.gameObject.transform.up, upAbsolute) * obj.gameObject.transform.rotation;
        Matrix4x4 objMat = Matrix4x4.TRS(obj.gameObject.transform.position, objQua, Vector3.one);

        foreach (var point in pS) {
            RaycastHit[] hits;
            var down = -upAbsolute;
            Vector3 pointF = objMat.MultiplyPoint3x4(point);

            hits = Physics.RaycastAll(pointF, down, 1F);
            //Debug.Log(obj + " "+hits.Length + " " + listSize);

            if (hits.Length == listSize)
                DrawPoint(obj.VisualRepresentation, objQua, pointF);
        }
    }
    //-------------------------------------------------
    // Draw suggestion to stack correctly
    //-------------------------------------------------
    public void DrawPoint(GameObject obj,Quaternion rotation, Vector3 point)
    {
        var sphere1cube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        sphere1cube.GetComponent<Collider>().enabled = false;
        sphere1cube.transform.rotation = rotation;
        sphere1cube.transform.position = point;
        var scale = new Vector3(0.002f, 0.00005f, 0.002f);
        sphere1cube.transform.localScale = scale;
        sphere1cube.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
        sphere1cube.transform.parent = obj.GetComponent<Grab>().drawnPoints.transform;
    }
    //-------------------------------------------------
    // Draw axis objects
    //-------------------------------------------------
    public void DrawCenter()
    {
        Vector3 upAbsolute = UpperDirecction(logicObject.GetComponent<Stackable>());
        Quaternion rotation = Quaternion.FromToRotation(transform.up, upAbsolute) * transform.rotation;

        center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        center.transform.position = transform.position;
        center.transform.rotation = rotation;
        center.transform.localScale = new Vector3(0.005f, transform.localScale.y + 0.04f, 0.005f);
        center.GetComponent<Collider>().enabled = false;
        center.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
        center.transform.parent = this.gameObject.transform;
    }

    //-------------------------------------------------
    // Called when a Hand starts hovering over this object
    //-------------------------------------------------
    private void OnHandHoverBegin(Hand hand)
    {
        this.GetComponent<MeshRenderer>().material.SetColor("_Color", hoverColor);

        //*
        allStacks.Clear();
        var allObjects = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in allObjects)
        {
            //if (hand.gameObject.GetComponentInChildren<SimpleSpring>() != null && hand.gameObject.GetComponentInChildren<SimpleSpring>().logic.name.Equals(obj.name)) continue;
            obj.VisualRepresentation.GetComponent<Grab>().drawnPoints = new GameObject();
            obj.VisualRepresentation.GetComponent<Grab>().drawnPoints.transform.parent = obj.VisualRepresentation.gameObject.transform;
            if (obj.baseStackable == null || !obj.baseStackable.name.Equals("Podium")) continue;
            List<Stackable> stack = new List<Stackable>();
            stack.Add(obj);
            FindAboveObjects(obj, ref stack);
            allStacks.Add(stack);
        }

        foreach (var list in allStacks)
        {
            ProjectStackable(list[list.Count - 1], list.Count);
        }

        DrawCenter();
    }

    //-------------------------------------------------
    // Called when a Hand stops hovering over this object
    //-------------------------------------------------
    private void OnHandHoverEnd(Hand hand) {
        this.GetComponent<Renderer>().material.SetColor("_Color", materialOriginalColor);        
        Destroy(center);
        /*
        var allObjects = GameObject.FindObjectsOfType<Stackable>();
        foreach (var obj in allObjects)
            Destroy(obj.VisualRepresentation.GetComponent<Grab>().drawnPoints);//*/
        

    }

    //-------------------------------------------------
    // Called every Update() while a Hand is hovering over this object
    //-------------------------------------------------
    private void HandHoverUpdate(Hand hand) {

        if (hand.GetStandardInteractionButtonDown()) {// || ((hand.controller != null) && hand.controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_Grip))) {
            if (hand.currentAttachedObject != gameObject) {

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



                ////
                allStacks.Clear();
                if(hand.otherHand.gameObject.GetComponentInChildren<SimpleSpring>() != null) {
                    List<Stackable> stack = new List<Stackable>();
                    var logicStackable = logicObject.GetComponent<Stackable>();
                    logicStackable = hand.otherHand.gameObject.GetComponentInChildren<SimpleSpring>().logic.GetComponent<Stackable>();
                    logicStackable.VisualRepresentation.GetComponent<Grab>().drawnPoints = new GameObject();
                    logicStackable.VisualRepresentation.GetComponent<Grab>().drawnPoints.transform.parent = logicStackable.VisualRepresentation.gameObject.transform;
                    stack.Add(logicStackable);
                    FindAboveObjects(logicStackable, ref stack);
                    allStacks.Add(stack);
                    

                } else {
                    var allObjects = GameObject.FindObjectsOfType<Stackable>();
                    foreach (var obj in allObjects) {
                        if (hand.gameObject.GetComponentInChildren<SimpleSpring>() != null && hand.gameObject.GetComponentInChildren<SimpleSpring>().logic.name.Equals(obj.name)) continue;
                        obj.VisualRepresentation.GetComponent<Grab>().drawnPoints = new GameObject();
                        obj.VisualRepresentation.GetComponent<Grab>().drawnPoints.transform.parent = obj.VisualRepresentation.gameObject.transform;
                        if (obj.baseStackable == null || !obj.baseStackable.name.Equals("Podium")) continue;
                        List<Stackable> stack = new List<Stackable>();
                        stack.Add(obj);
                        FindAboveObjects(obj, ref stack);
                        allStacks.Add(stack);
                    }
                }


                foreach (var list in allStacks) {
                    ProjectStackable(list[list.Count - 1], list.Count);
                }

                DrawCenter();
                ////



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
            Destroy(logicObject.GetComponent<NotifyCollision>());

            if (hand.otherHand.gameObject.GetComponentInChildren<SimpleSpring>() == null) {
                var allObjects = GameObject.FindObjectsOfType<Stackable>();
                foreach (var obj in allObjects)
                    Destroy(obj.VisualRepresentation.GetComponent<Grab>().drawnPoints);
            } 

            Destroy(center);

            this.GetComponent<Renderer>().material.SetColor("_Color", hoverColor);

            // Detach this object from the hand
            hand.DetachObject(imaginary);
            // Call this to undo HoverLock
            hand.HoverUnlock(GetComponent<Interactable>());

        }
    }

    //-------------------------------------------------
    // Search for objects stacked on the other hand and detach
    //-------------------------------------------------
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
    // Find the above objects given a base object. Return the list of all above objects
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

