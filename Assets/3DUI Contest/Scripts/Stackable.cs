using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stackable : PhysicalObject {

	public GameObject baseStackable;

	// OnCollisionStay is called every physics frame for an ongoing collision
	void OnCollisionStay(Collision collision) {

		Stackable collisionStackable = collision.gameObject.GetComponent<Stackable> ();
		PhysicalObject collisionObject = collision.gameObject.GetComponent<PhysicalObject> ();

		if (collisionStackable != null) {

			if (collisionStackable.baseStackable != null) {

				if (collision.gameObject.transform.position.y < transform.position.y) {

					baseStackable = collision.gameObject;
				}
			}
		} else if (collisionObject != null) {

			if (collision.gameObject.transform.position.y < transform.position.y) {

				baseStackable = collision.gameObject;

			}
		}
	}

	// OnCollisionExit is called every physics frame 
	void OnCollisionExit(Collision collision) {

		if (baseStackable == collision.gameObject) {

			baseStackable = null;
		}
	}

}
