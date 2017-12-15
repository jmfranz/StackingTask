using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackingTask : MonoBehaviour {

	public Stackable CubeLarge;
	public Stackable CubeMedium;
	public Stackable CubeSmall;
	public Stackable CylinderLarge;
	public Stackable CylinderMedium;
	public Stackable CylinderSmall;

	private Vector3 topPosition;
	private Quaternion topRotation;
	private float topTimer;
	private float timerThreshold = 5.0f;

	public bool taskCompleted;

	// Use this for initialization
	void Start () {

		CubeLarge = GameObject.Find ("Cube Large Logic").GetComponent<Stackable> ();
		CubeMedium = GameObject.Find ("Cube Medium Logic").GetComponent<Stackable> ();
		CubeSmall = GameObject.Find ("Cube Small Logic").GetComponent<Stackable> ();
		CylinderLarge = GameObject.Find ("Cylinder Large Logic").GetComponent<Stackable> ();
		CylinderMedium = GameObject.Find ("Cylinder Medium Logic").GetComponent<Stackable> ();
		CylinderSmall = GameObject.Find ("Cylinder Small Logic").GetComponent<Stackable> ();

		topPosition = new Vector3 ();
		topRotation = new Quaternion ();
		topTimer = 0.0f;

		taskCompleted = false;
	}

	// Update is called once per frame
	void Update () {

		if (!taskCompleted) {
			CheckCompletion ();
		} else {
			GameObject.Find ("Stacking Task").SetActive (false);
		}
	}

	private void CheckCompletion() {

		Debug.Log ("check called");

		Stackable topStackable = CubeLarge;

		if (CubeLarge.transform.position.y > topStackable.transform.position.y) {
			topStackable = CubeLarge;
		}
		if (CubeMedium.transform.position.y > topStackable.transform.position.y) {
			topStackable = CubeMedium;
		}
		if (CubeSmall.transform.position.y > topStackable.transform.position.y) {
			topStackable = CubeSmall;
		}
		if (CylinderLarge.transform.position.y > topStackable.transform.position.y) {
			topStackable = CylinderLarge;
		}
		if (CylinderMedium.transform.position.y > topStackable.transform.position.y) {
			topStackable = CylinderMedium;
		}
		if (CylinderSmall.transform.position.y > topStackable.transform.position.y) {
			topStackable = CylinderSmall;
		}

		if (topStackable.transform.position.y > 1.55f) {

			if (topStackable.baseStackable != null) {

				if (topPosition == topStackable.transform.position &&
					topRotation == topStackable.transform.rotation) {

					topTimer += Time.deltaTime;

					if (topTimer >= timerThreshold) {
						taskCompleted = true;
					}
				} else {
					topTimer = 0.0f;
				}
					
				topPosition = topStackable.transform.position;
				topRotation = topStackable.transform.rotation;
			}
		}
	}
}
