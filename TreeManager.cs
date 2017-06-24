using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf{
	public Mesh mesh;
	public Vector3 translation;
	public Vector3 rotation;
	public Leaf Initialize(Material leafMaterial, float width, float height, Mesh mesh){
		if (mesh == null) {
			//set up the mesh.
			this.mesh = new Mesh ();
			this.mesh.vertices = leafVerts (width, height);
			this.mesh.triangles = new int[]{ 0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7 };
			this.mesh.RecalculateNormals ();

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
			this.mesh.uv = uvs;
		} else {
			this.mesh = mesh;
		}
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

public class FractalBranch
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

	//Offset From the parent.
	public Vector3 translation;
	public Vector3 rotation;
	public Mesh mesh;

	public FractalBranch parentBranch;
	private List<FractalBranch> childBranches = new List<FractalBranch> ();
	private List<Leaf> leafs = new List<Leaf> ();

	public TreeManager manager;


	// Use this for initialization (called before init.)
	void Start ()
	{
	}

	public FractalBranch Initialize (float widthParam, float lengthParam, float cytoParam, FractalBranch parent)
	{
		this.parentBranch = parent;
		this.width = widthParam;
		this.ultimateLength = lengthParam;
		this.length = lengthParam / 4;
		this.cytoFac = cytoParam;
		translation = Vector3.zero;
		this.branchDepth = 0;
		return this;

	}
	//Build the actual shape of the branch.
	public void build(){
		if (parentBranch != null) {
			this.manager = parentBranch.manager;

			this.branchDepth = parentBranch.branchDepth + 1;

			updateLength();
			this.width = parentBranch.width * 0.5f;

			//Move the current branch so that it is stacked right above its parent.
			if (directBranch) {
				translation = Vector3.up * parentBranch.length;
			} else {
				translation = Vector3.up * Random.Range(parentBranch.length - ((parentBranch.length) * (0.8f - this.branchDepth/10)), parentBranch.length);
			}

			//when we rotate we have to transform again so that the branch keeps its bottom connected to its parent's top.
			//float hypo = this.length/2f;
			//float opposite = Mathf.Sin(Mathf.Deg2Rad * transform.localRotation.eulerAngles.z) * hypo;
			//float adjacent = Mathf.Cos (Mathf.Deg2Rad * transform.localRotation.eulerAngles.y) * hypo;
			//float oppositeX = Mathf.Sin (Mathf.Deg2Rad * transform.localRotation.eulerAngles.x) * 2f *adjacent;
			//transform.localPosition = transform.localPosition + new Vector3 (0f, adjacent, 0f);

		}

		//Check for mesh filter and make one if none exists.
		/*MeshFilter filter = (MeshFilter)this.GetComponent<MeshFilter> ();
		if (filter == null)
			filter = gameObject.AddComponent<MeshFilter> ();

		//Create a new cylinder mesh.
		filter.mesh = mesh = new Mesh ();*/
		int numberOfRingVerts = 6 - this.branchDepth;
		if (numberOfRingVerts < 2)numberOfRingVerts = 2;
		/*
		filter.mesh =*/ 
		this.mesh = CylinderGenerator.generateCylinder (this.length, this.width, this.width * 0.5f, numberOfRingVerts);
		//transform.localRotation = Quaternion.Euler(0f, 0f, weightedRand() * 30f);
	}
	public void branch ()
	{
		int numberOfBranches = Random.Range (2, (this.branchDepth*2)+4);
		for (int i = 0; i < numberOfBranches; i++) {
			//float theta = weightedRand ();
			//float branchLength = length * 0.7f;
			FractalBranch newChildBranch = new FractalBranch().Initialize(width * this.manager.childWidthFactor, this.ultimateLength * this.manager.childLengthFactor,  calculateCyto(this.cytoFac, numberOfBranches), this);
			if (i == 0)newChildBranch.directBranch = true;
			newChildBranch.build ();
			newChildBranch.rotation =  new Vector3(weightedRand () * 120f, newChildBranch.rotation.y, weightedRand () * 120f);

			addChildBranch (newChildBranch);
		}

		//spread out the branches on the first level.
		bool isBaseBranch = this.parentBranch == null;

		if (isBaseBranch && this.childBranches.Count > 0) {
			//Not completely correct, but having the golden angle lower looks better.
			float GoldenAngle = (((1f + Mathf.Sqrt (5f)) / 2f) * 100f)-80f;
			float lastRotation = this.childBranches [0].rotation.y;
			for (int i = 1; i < this.childBranches.Count; i++) {
				this.childBranches [i].rotation = new Vector3(this.childBranches[0].rotation.x, lastRotation + GoldenAngle, this.childBranches[0].rotation.z);
				lastRotation = lastRotation + GoldenAngle;
			}
		}
	}

	public Mesh combineMeshes(List<TransformMesh> meshes){
		CombineInstance[] combine = new CombineInstance[meshes.Count];
		int combineIndex = 0;
		for (int i = 0; i < meshes.Count; i++) {
			TransformMesh meshRep = meshes [i];
			if (meshRep.mesh != null) {
				combine [combineIndex].mesh = meshRep.mesh;
				Quaternion rotationQ = Quaternion.Euler (meshRep.rotation.x, meshRep.rotation.y, meshRep.rotation.z);
				combine [combineIndex].transform = Matrix4x4.TRS (meshRep.translation, rotationQ, Vector3.one);
				combineIndex++;
			}
		}
		Mesh returnMesh = new Mesh ();
		returnMesh.CombineMeshes (combine, true, true);
		return returnMesh;
	}

	//Recursive function to get the mesh for each branch and combine them into one for performance.
	public MeshTuple getMesh (){
		return this.recurseMesh ();
	}
	public struct TransformMesh{
		public Mesh mesh;
		public Vector3 translation;
		public Vector3 rotation;
		public TransformMesh(Mesh meshP, Vector3 translationP, Vector3 rotationP){
			mesh = meshP;
			translation = translationP;
			rotation = rotationP;
		}
	}
	public struct MeshTuple{
		public TransformMesh branches;
		public TransformMesh leafs;
		public MeshTuple(TransformMesh branchesP, TransformMesh leafsP){
			branches = branchesP;
			leafs = leafsP;
		}
	}
	public MeshTuple recurseMesh(){
		
		Mesh branchReturnMesh = new Mesh ();
		Mesh leafMesh = new Mesh ();
		TransformMesh branches;
		TransformMesh transformLeafs;
		//combine leafs.

		if (this.leafs != null && this.leafs.Count != 0) {
			List<TransformMesh> childLeafs = new List<TransformMesh> ();
			for (int i = 0; i < this.leafs.Count; i++) {
				Leaf leaf = this.leafs [i];
				childLeafs.Add (new TransformMesh (leaf.mesh, leaf.translation, leaf.rotation));
			}
			leafMesh = combineMeshes (childLeafs);
		}

		if (this.childBranches != null && this.childBranches.Count != 0) {
			Mesh branchTransferMesh = new Mesh ();

			CombineInstance[] leafCombine = new CombineInstance[this.childBranches.Count];
			List<TransformMesh> childTransformBranches = new List<TransformMesh> ();
			List<TransformMesh> childTransformLeafs = new List<TransformMesh> ();

			for (int i = 0; i < childBranches.Count; i++) {
				FractalBranch childBranch = childBranches [i];
				MeshTuple childMesh = childBranch.recurseMesh ();

				childTransformBranches.Add (childMesh.branches);

				childTransformLeafs.Add (childMesh.leafs);
			}
			Mesh childBranchesCombined = combineMeshes (childTransformBranches);
			Mesh finalBranchCombine = combineMeshes (new List<TransformMesh> {new TransformMesh (childBranchesCombined, Vector3.zero, Vector3.zero), new TransformMesh (this.mesh, Vector3.zero, Vector3.zero)});
			branches = new TransformMesh (finalBranchCombine, this.translation, this.rotation);

			Mesh childLeafsCombined = combineMeshes (childTransformLeafs);
			Mesh finalLeafsCombine = combineMeshes (new List<TransformMesh> {
				new TransformMesh (childLeafsCombined, Vector3.zero, Vector3.zero),
				new TransformMesh (leafMesh, Vector3.zero, Vector3.zero)
			});
			transformLeafs = new TransformMesh (finalLeafsCombine, this.translation, this.rotation);

		} else {
			branches = new TransformMesh (this.mesh, this.translation, this.rotation);
			transformLeafs = new TransformMesh (leafMesh, this.translation, this.rotation);
		
		}
		return new MeshTuple (branches, transformLeafs);
	}
	/*public Mesh getLeafMesh () {
		Mesh transferMesh = new Mesh ();
		Mesh returnMesh = new Mesh ();
		if (this.childBranches != null && this.childBranches.Count != 0) {
			CombineInstance[] combine = new CombineInstance[this.childBranches.Count];
			for (int i = 0; i < childBranches.Count; i++) {
				FractalBranch childBranch = childBranches [i];
				combine [i].mesh = childBranch.getLeafMesh();
				Quaternion rotationQ = Quaternion.Euler (leaf.rotation.x, leaf.rotation.y, leaf.rotation.z);
				combine [i].transform = Matrix4x4.TRS (leaf.translation, rotationQ, Vector3.one);
			}
			transferMesh.CombineMeshes (combine, true, true);
			CombineInstance[] finalCombine = new CombineInstance[2];
			Quaternion zeroRotation = Quaternion.Euler (0, 0, 0);
			finalCombine [0].mesh = transferMesh;
			finalCombine [0].transform = Matrix4x4.TRS (Vector3.zero, zeroRotation, Vector3.one);
			finalCombine [1].mesh = this.mesh;
			finalCombine [1].transform = Matrix4x4.TRS (Vector3.zero, zeroRotation, Vector3.one);

			returnMesh.CombineMeshes (finalCombine, true, true);
			return returnMesh;

		} else {
			CombineInstance[] combine = new CombineInstance[this.leafs.Count];
			for (int i = 0; i < leafs.Count; i++) {
				Leaf leaf = leafs [i];
				combine [i].mesh = leaf.mesh;
				Quaternion rotationQ = Quaternion.Euler (leaf.rotation.x, leaf.rotation.y, leaf.rotation.z);
				combine [i].transform = Matrix4x4.TRS (leaf.translation, rotationQ, Vector3.one);
			}
			transferMesh.CombineMeshes (combine, true, true);
			CombineInstance[] finalCombine = new CombineInstance[2];
			Quaternion zeroRotation = Quaternion.Euler (0, 0, 0);
			finalCombine [0].mesh = transferMesh;
			finalCombine [0].transform = Matrix4x4.TRS (Vector3.zero, zeroRotation, Vector3.one);
			finalCombine [1].mesh = this.mesh;
			finalCombine [1].transform = Matrix4x4.TRS (Vector3.zero, zeroRotation, Vector3.one);

			returnMesh.CombineMeshes (finalCombine, true, true);
			return returnMesh;
		}
	}*/
	public void growChildren ()
	{
		branchAge++;
		float originalLength = this.length;
		this.width = this.width * this.manager.widthFactor;
		if (parentBranch != null)this.width = this.parentBranch.width*this.manager.childWidthFactor;

		if (branchAge < 3)
			this.length = this.ultimateLength / 4 + (this.ultimateLength / 4) * branchAge;

		//Create a new cylinder mesh to represent a larger version of the branch.
		this.mesh = CylinderGenerator.generateCylinder (this.length, this.width, this.width * 0.65f, 6);

		if (childBranches.Count == 0) {
			branch ();
		} else {
			for (int i = 0; i < childBranches.Count; i++) {
				//preserves the child branch's position on the parent branch.
				childBranches [i].translation = Vector3.up * (childBranches [i].translation.y * this.length) / originalLength; 
				childBranches [i].growChildren ();
			}
		}
	}
	public void growLeafs(){
		if (this.childBranches.Count == 0 && ((int)Random.Range(0, 2)) == 0) {
			Leaf newLeaf = new Leaf().Initialize (this.manager.leafMaterial, 0.1f, 0.25f, this.manager.leafMesh);
			newLeaf.translation = Vector3.up * this.length;
			newLeaf.rotation = new Vector3(125 - Random.Range(0, 70),Random.Range(0, 360),0);
			this.leafs.Add (newLeaf);


			/*if (Random.Range (0, 10) == 0) {
				GameObject flower = new GameObject ("Flower");
				flower.AddComponent<MeshFilter> ().mesh = this.manager.flowerMesh;
				flower.AddComponent<MeshRenderer> ().material = this.manager.flowerMaterial;
				flower.transform.localScale = Vector3.one * 0.1f;
				flower.transform.localPosition = Vector3.up * this.length * 1.4f;
				flower.transform.localRotation = Quaternion.Euler (Random.Range (0, 360), Random.Range (0, 360), Random.Range (0, 360));
			}*/
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
	public GameObject tree;

	private GameObject branches;
	private GameObject leafs;
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

		//Create a new cylinder mesh.
		baseBranch = new FractalBranch().Initialize(startWidth, startHeight, 1f, null);
		baseBranch.manager = this;
		baseBranch.translation = this.transform.localPosition;
		baseBranch.build();


		tree = new GameObject ("TreeObject");//.AddComponent<> ().Initialize (startWidth, startHeight, 1f, null);
		tree.AddComponent<MeshRenderer> ().material = material;
		tree.transform.parent = this.transform;
		tree.transform.position = this.transform.localPosition;

		branches = new GameObject ("Branches");
		branches.AddComponent<MeshRenderer> ().material = material;
		branches.transform.parent = tree.transform;
		branches.transform.position = this.transform.localPosition;
		branches.AddComponent < MeshFilter> ();

		leafs = new GameObject ("Leafs");
		leafs.AddComponent<MeshRenderer> ().material = leafMaterial;
		leafs.transform.parent = tree.transform;
		leafs.transform.position = this.transform.localPosition;
		leafs.AddComponent < MeshFilter> ();
		updateMesh();

		//MeshCollider collider = gameObject.AddComponent<MeshCollider> ();
		//collider.sharedMesh = ((MeshFilter)tree.GetComponent<MeshFilter> ()).mesh;
	}

	void updateMesh(){
		MeshFilter filter;
		if (tree.GetComponent<MeshFilter> () == null) {
			filter = tree.AddComponent<MeshFilter> ();
		} else {
			filter = tree.GetComponent<MeshFilter> ();
		}
		FractalBranch.MeshTuple meshes = baseBranch.getMesh();

		MeshFilter branchFilter = branches.GetComponent<MeshFilter> ();
		branchFilter.mesh = meshes.branches.mesh;
		MeshFilter leafFilter = leafs.GetComponent<MeshFilter> ();
		leafFilter.mesh = meshes.leafs.mesh;
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
			this.updateMesh ();
		}
		if (Input.GetKeyUp (KeyCode.O)) {
			baseBranch.growLeafs ();
			this.updateMesh ();
		}
		if (Input.GetKeyUp (KeyCode.X)) {
			//DestroyImmediate (baseBranch.gameObject);
			//this.setup ();
		}

	}
}
