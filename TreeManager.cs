using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf:MonoBehaviour{
	public Leaf Initialize(Material leafMaterial, float width, float height, Mesh mesh){

		//Check for mesh filter and make one if none exists.
		MeshFilter filter = (MeshFilter)this.GetComponent<MeshFilter> ();
		if (filter == null)
			filter = gameObject.AddComponent<MeshFilter> ();

		if (mesh != null) {
			filter.mesh = mesh;
		} else {
			//set up the mesh.
			Mesh leafMesh;
			filter.mesh = leafMesh = new Mesh ();
			leafMesh.vertices = leafVerts (width, height);
			leafMesh.triangles = new int[]{ 0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7 };
			leafMesh.RecalculateNormals ();

			//UV.
			Vector2[] uvs = new Vector2[8];
			uvs [0] = new Vector2 (0, 0);
			uvs [1] = new Vector2 (0, 1);
			uvs [2] = new Vector2 (1, 1);
			uvs [3] = new Vector2 (1, 0);
			uvs [4] = new Vector2 (0, 0);
			uvs [5] = new Vector2 (0, 1);
			uvs [6] = new Vector2 (1, 1);
			uvs [7] = new Vector2 (1, 0);
			leafMesh.uv = uvs;
		}
		gameObject.AddComponent<MeshRenderer> ().material = leafMaterial;

		return this;
	}
	//return verts
	Vector3[] leafVerts(float width, float height){
		Vector3[] returnVerts = new Vector3[8];
		returnVerts [0] = new Vector3 (0f, 0f, 0f);
		returnVerts [1] = new Vector3 (0f, height, 0f);
		returnVerts [2] = new Vector3 (width, height, 0f);
		returnVerts [3] = new Vector3 (width, 0f, 0f);
		returnVerts [4] = new Vector3 (0f, 0f, -0.0001f);
		returnVerts [5] = new Vector3 (0f, height, -0.0001f);
		returnVerts [6] = new Vector3 (width, height, -0.0001f);
		returnVerts [7] = new Vector3 (width, 0f, -0.0001f);
		return returnVerts;
	}

}

public class FractalBranch:MonoBehaviour
{
	float width;

	//Current length
	public float length;

	//Length that will branch will eventually reach.
	public float ultimateLength;

	//Cytokinin is a chemical in plants that promotes growth. This variable is meant to help with pruning,
	//specifically, a branch with few offshoots should grow longer. Practically this meants that this variable 
	//affects length and has an inverse relation to the branches number of siblings.
	float cytoFac;

	//The recursion depth.
	int branchDepth;

	//The number of times grow children has been called on it.
	int branchAge;

	//If the branch should grow from the top of its parent.
	bool directBranch;

	public FractalBranch parentBranch;
	private List<FractalBranch> childBranches = new List<FractalBranch> ();

	public TreeManager manager;

	public Mesh mesh;
	public Material material;

	// Use this for initialization (called before init.)
	void Start ()
	{
		gameObject.AddComponent<MeshRenderer> ().material = material;
	}

	public FractalBranch Initialize (float widthParam, float lengthParam, float cytoParam, FractalBranch parent)
	{
		this.parentBranch = parent;
		this.width = widthParam;
		this.ultimateLength = lengthParam;
		this.length = lengthParam / 4;
		this.cytoFac = cytoParam;

		this.branchDepth = 0;
		return this;

	}
	//Build the actual shape of the branch.
	public void build(){
		if (parentBranch != null) {
			this.manager = parentBranch.manager;
			this.material = parentBranch.material;
			this.transform.parent = parentBranch.transform;

			this.branchDepth = parentBranch.branchDepth + 1;


			//These constants control the appearance of the tree.
			updateLength();
			this.width = parentBranch.width * 0.5f;

			//Move the current branch so that it is stacked right above its parent.
			if (directBranch) {
				transform.localPosition = Vector3.up * parentBranch.length;
			} else {
				transform.localPosition = Vector3.up * Random.Range(parentBranch.length - ((parentBranch.length) * (0.8f - this.branchDepth/10)), parentBranch.length);
			}


			//when we rotate we have to transform again so that the branch keeps its bottom connected to its parent's top.
			//float hypo = this.length/2f;
			//float opposite = Mathf.Sin(Mathf.Deg2Rad * transform.localRotation.eulerAngles.z) * hypo;
			//float adjacent = Mathf.Cos (Mathf.Deg2Rad * transform.localRotation.eulerAngles.y) * hypo;
			//float oppositeX = Mathf.Sin (Mathf.Deg2Rad * transform.localRotation.eulerAngles.x) * 2f *adjacent;
			//transform.localPosition = transform.localPosition + new Vector3 (0f, adjacent, 0f);

		}

		//Check for mesh filter and make one if none exists.
		MeshFilter filter = (MeshFilter)this.GetComponent<MeshFilter> ();
		if (filter == null)
			filter = gameObject.AddComponent<MeshFilter> ();

		//Create a new cylinder mesh.
		filter.mesh = mesh = new Mesh ();
		int numberOfRingVerts = 6 - this.branchDepth;
		if (numberOfRingVerts < 2)numberOfRingVerts = 2;

		filter.mesh = mesh = CylinderGenerator.generateCylinder (this.length, this.width, this.width * 0.5f, numberOfRingVerts);
		//transform.localRotation = Quaternion.Euler(0f, 0f, weightedRand() * 30f);
	}
	public void branch ()
	{
		int numberOfBranches = Random.Range (2, (this.branchDepth*2)+4);
		for (int i = 0; i < numberOfBranches; i++) {
			//float theta = weightedRand ();
			//float branchLength = length * 0.7f;
			FractalBranch newChildBranch = new GameObject ("BranchChild").AddComponent<FractalBranch> ().Initialize (width * this.manager.childWidthFactor, this.ultimateLength * this.manager.childLengthFactor,  calculateCyto(this.cytoFac, numberOfBranches), this);
			if (i == 0)newChildBranch.directBranch = true;
			newChildBranch.build ();
			newChildBranch.transform.localRotation = Quaternion.Euler (weightedRand () * 120f, newChildBranch.transform.rotation.eulerAngles.y, weightedRand () * 120f);

			addChildBranch (newChildBranch);
		}

		//spread out the branches on the first level.
		bool isBaseBranch = this.parentBranch == null;

		if (isBaseBranch && this.childBranches.Count > 0) {
			//Not completely correct, but having the golden angle lower looks better.
			float GoldenAngle = (((1f + Mathf.Sqrt (5f)) / 2f) * 100f)-80f;
			float lastRotation = this.childBranches [0].transform.localRotation.eulerAngles.y;
			for (int i = 1; i < this.childBranches.Count; i++) {
				this.childBranches [i].transform.localRotation = Quaternion.Euler (this.childBranches[0].transform.localRotation.eulerAngles.x, lastRotation + GoldenAngle, this.childBranches[0].transform.localRotation.eulerAngles.z);
				lastRotation = lastRotation + GoldenAngle;
			}
		}
	}


	public void growChildren ()
	{
		branchAge++;
		float originalLength = this.length;
		this.width = this.width * this.manager.widthFactor;
		if (parentBranch)this.width = this.parentBranch.width*this.manager.childWidthFactor;

		if(branchAge < 3)this.length = this.ultimateLength/4 + (this.ultimateLength/4) * branchAge;

		//Check for mesh filter and make one if none exists.
		MeshFilter filter = (MeshFilter)this.GetComponent<MeshFilter> ();
		if (filter == null)
			filter = gameObject.AddComponent<MeshFilter> ();

		//Create a new cylinder mesh to represent a larger version of the branch.
		filter.mesh = mesh = CylinderGenerator.generateCylinder (this.length, this.width, this.width * 0.65f, 6);

		if (childBranches.Count == 0) {
			branch ();
		} else {
			for (int i = 0; i < childBranches.Count; i++) {
				//preserves the child branch's position on the parent branch.
				childBranches [i].transform.localPosition = Vector3.up * (childBranches [i].transform.localPosition.y * this.length) / originalLength; 
				childBranches [i].growChildren ();
			}
		}
	}
	public void growLeafs(){
		if (this.childBranches.Count == 0 && ((int)Random.Range(0, 2)) == 0) {
			Leaf newLeaf = new GameObject ("Leaf").AddComponent<Leaf> ().Initialize (this.manager.leafMaterial, 0.1f, 0.25f, this.manager.leafMesh);
			newLeaf.transform.parent = this.transform;
			newLeaf.transform.localPosition = Vector3.up * this.length;
			newLeaf.transform.rotation = Quaternion.Euler (125 - Random.Range(0, 70),Random.Range(0, 360),0);


			if (Random.Range (0, 10) == 0) {
				GameObject flower = new GameObject ("Flower");
				flower.AddComponent<MeshFilter> ().mesh = this.manager.flowerMesh;
				flower.AddComponent<MeshRenderer> ().material = this.manager.flowerMaterial;
				flower.transform.parent = this.transform;
				flower.transform.localScale = Vector3.one * 0.1f;
				flower.transform.localPosition = Vector3.up * this.length * 1.4f;
				flower.transform.localRotation = Quaternion.Euler (Random.Range (0, 360), Random.Range (0, 360), Random.Range (0, 360));
			}
		} else {
			for (int i = 0; i < this.childBranches.Count; i++) {
				childBranches [i].growLeafs ();
			}
		}
	}
	public void updateLength(){
		this.length = (this.parentBranch.length * (1f/((this.branchAge + 1f)*1.1f)) *  (this.cytoFac)) - Random.Range (0, this.parentBranch.length * 0.2f); 
	}
	public float calculateCyto(float parentCyto, int siblings){
		return parentCyto * ((1 - (siblings) / 80f));
	}
	public void updateCyto(float newCytoValue){
		this.cytoFac = newCytoValue;
	}
	//This will be called from a child branch when it gets pruned.
	public void recalculateChildrenCyto(){
		for (int i = 0; i < this.childBranches.Count; i++) {
			this.childBranches [i].updateCyto (calculateCyto (this.cytoFac, this.childBranches.Count));
		}
	}

	void addChildBranch (FractalBranch childBranch)
	{
		childBranches.Add (childBranch);
	}

	public void OnMouseDown(){
		this.removeFromParent ();
	}
	//used for pruning.
	public void removeFromParent ()
	{
		Destroy (this);
	}

	float weightedRand ()
	{
		float rand = Random.Range (-1f, 1f);
		return (0.5f * Mathf.Pow (rand, 2f)) * (rand / Mathf.Abs (rand));
	}
		
}

public class TreeManager : MonoBehaviour
{

	public Mesh mesh;
	public Mesh flowerMesh;
	public Material flowerMaterial;
	public Material material;
	public Mesh leafMesh;
	public Material leafMaterial;
	public float startWidth;
	public float startHeight;
	public float childWidthFactor; //The number multiplied by to find the children's initial width.
	public float widthFactor; //the number multiplied by each branches' width
	public float childLengthFactor;

	private FractalBranch baseBranch;
	private float timeUntilGrowth = 70f; //time until growChildren() is called.
	private float waterLevel=1;
	private float lightLevel;
	private float fertilizationLevel;
	private int treeAge; //The number of times growChildren () has been called.

	// Use this for initialization
	void Start ()
	{
		this.setup ();	
	}
	void setup(){
		baseBranch = new GameObject ("TreeBaseBranch").AddComponent<FractalBranch> ().Initialize (startWidth, startHeight, 1f, null);
		baseBranch.manager = this;
		baseBranch.build();
		baseBranch.transform.parent = this.transform;
		baseBranch.transform.localPosition = Vector3.up * this.transform.localPosition.y;
		MeshCollider collider = gameObject.AddComponent<MeshCollider> ();
		collider.sharedMesh = baseBranch.mesh;
		baseBranch.material = material;

		//baseBranch.transform.localScale = new Vector3 (0.25f, 1f, 0.25f);		}
	}
	// Update is called once per frame
	void Update ()
	{
		/*if (timeUntilGrowth <= 0f) {
			timeUntilGrowth = 70f;
			if (treeAge < 5) {
				baseBranch.growChildren ();
				if (treeAge >= 1) {
					for (int i = 0; i < 5 - treeAge; i++) {
						baseBranch.growLeafs ();
					}
				}
				treeAge++;
			}
		} else {
			timeUntilGrowth -= 1f;
		}*/
		if (Input.GetKeyUp (KeyCode.P)) {
			baseBranch.growChildren ();
		}
		if (Input.GetKeyUp (KeyCode.O)) {
			baseBranch.growLeafs ();
		}
		if (Input.GetKeyUp (KeyCode.X)) {
			DestroyImmediate (baseBranch.gameObject);
			this.setup ();
		}

	}
}
