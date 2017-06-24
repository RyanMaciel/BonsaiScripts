using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fern : MonoBehaviour {
	public Material material;
	public Material fernBranchMaterial;
	// Use this for initialization
	/*void Start () {
		MeshFilter filter = (MeshFilter)this.GetComponent<MeshFilter> ();
		if (filter == null)filter = gameObject.AddComponent<MeshFilter> ();
		filter.mesh = CylinderGenerator.generateCylinder (1f, 0.004f, 0.003f, 5);
		gameObject.AddComponent<MeshRenderer> ().material = material;

		int numberOfNodes = 35;
		float rotationAngle = 75;
		float leafWidth = 0.2f;
		for (int i = 0; i < numberOfNodes; i++) {
			Leaf newLeaf = new GameObject ("BranchLeaf").AddComponent<Leaf> ().Initialize (fernBranchMaterial, leafWidth, 0.6f, null);
			newLeaf.transform.parent = this.transform;
			newLeaf.transform.localScale = Vector3.one * ((0.3f/(((float)i/(float)numberOfNodes)+0.3f))-0.16f);//Mathf.Pow(1.13f, i/numberOfNodes);
			newLeaf.transform.localPosition = new Vector3 (-(leafWidth/2f) * newLeaf.transform.localScale.x, Mathf.Log10((((float)i/(float)numberOfNodes)+0.1f)*10f), 0f);
			newLeaf.transform.localRotation = Quaternion.Euler (0f, 0f, 0f);
			//newLeaf.transform.RotateAround (newLeaf.transform.position + new Vector3((leafWidth/2f) * newLeaf.transform.localScale.x, 0f, 0f), this.transform.rotation.eulerAngles, -rotationAngle + ((i % 2) * (rotationAngle * 2)));
			rotateAroundLocal (newLeaf.transform, newLeaf.transform.position + new Vector3 ((leafWidth / 2f) * newLeaf.transform.localScale.x, 0f, 0f), Vector3.forward, -rotationAngle + ((i % 2) * (rotationAngle * 2)));
			/*newLeaf.transform.localRotation = Quaternion.Euler(0, 0, );

			//Correct for rotation anchor point.
			float angle = Mathf.Deg2Rad * (newLeaf.transform.localRotation.eulerAngles.z);
			float hypo = .1f;
			newLeaf.transform.localPosition += new Vector3(Mathf.Sin(angle) * hypo, Mathf.Cos(angle) * hypo);

		}


	}
*/
	
	// Update is called once per frame
	void Update () {
		
	}

	void rotateAroundLocal(Transform rotationTransform, Vector3 point, Vector3 axis, float amount){
		rotationTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
		rotationTransform.RotateAround (point, axis, amount);
		rotationTransform.localRotation = rotationTransform.rotation;


	}
}
