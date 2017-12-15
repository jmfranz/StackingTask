using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalObject : MonoBehaviour {

	public GameObject VisualRepresentation;
	
	// Update is called once per frame
	void Update () {

		// Keep the object on the platform
		Vector3 position = transform.position;

		if (position.x < -1.5f)
			position.x = -1.0f;

		if (position.x > 1.5f)
			position.x = 1.0f;

		if (position.y < 0.0f)
			position.y = 0.25f;

		if (position.z < -1.5f)
			position.z = -1.0f;

		if (position.z > 1.5f)
			position.z = 1.0f;

		transform.position = position;
	
	}

	// LateUpdate is called once per frame
	void LateUpdate () {

		VisualRepresentation.transform.position = transform.position;
		VisualRepresentation.transform.rotation = transform.rotation;
	}

}
