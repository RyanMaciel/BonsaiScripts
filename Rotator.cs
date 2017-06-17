using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used for rotating the tree for viewing.
public class Rotator : MonoBehaviour {
	public float speed=3f;
	public float standardDistance = 3f;

	public GameObject Target;

	//What we will focus on
	private Vector3 currentTarget;



	private bool inTransition = false;
	private Vector3 transitionTarget;
	private Vector3 previousTarget;
	private float transitionSpeed = 0.01f; //the percentage per tick that the transition should occur.
	private float transitionPercentage = 0f;


	// Use this for initialization
	void Start () {
		currentTarget = Target.transform.position + Vector3.up * 3.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.D)) {
			this.transform.Translate (Vector3.right * Time.deltaTime * speed);
		}
		if (Input.GetKey (KeyCode.A)) {
			this.transform.Translate (Vector3.left * Time.deltaTime * speed);
		}
		if (Input.GetKey (KeyCode.W)) {
			this.transform.Translate (Vector3.up * Time.deltaTime * speed);
		}
		if (Input.GetKey (KeyCode.S)) {
			this.transform.Translate (Vector3.down * Time.deltaTime * speed);
		}

		this.transform.LookAt (currentTarget);


		if (Input.GetKey (KeyCode.E)) {
			standardDistance -= 0.1f;
		}
		if (Input.GetKey (KeyCode.Q)) {
			standardDistance += 0.1f;
		}

		//make sure the camera is within the correct distance to the target
		float distance = Vector3.Distance (currentTarget, this.transform.position);
		this.transform.Translate (Vector3.forward * (distance - standardDistance));
		/*if (distance > standardDistance + deviation) {
			//Move forward relative to where we are looking.
			this.transform.Translate (Vector3.forward * Time.deltaTime * speed);
		} else if(distance < standardDistance - deviation){
			this.transform.Translate (Vector3.back * Time.deltaTime * speed);
		}*/
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
			if (hit)
			{
				Target = hitInfo.transform.gameObject;
				previousTarget = currentTarget;
				transitionTarget = hitInfo.transform.position;
				transitionPercentage = 0f;
				inTransition = true;
			} else {
			}
		}
		if (inTransition)
			handleTransition ();
	}
	//Should be called on update, handles a transition between two objects to be focused on.
	//This is achieved by using linear interpoladstion to construct a point that moves between the two objects
	//This point will be focused on until the transition is over.
	void handleTransition(){
		Vector3 interp = Vector3.Lerp (previousTarget, transitionTarget, transitionPercentage);
		currentTarget = interp;
		this.transform.position += (interp - previousTarget)*transitionSpeed;
		transitionPercentage += transitionSpeed;
		if (transitionPercentage >= 1) {
			inTransition = false;
			transitionPercentage = 0f;
		}
	}
		
}
