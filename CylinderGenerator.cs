using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CylinderGenerator {

	public static Mesh generateCylinder(float length, float bottomRadius, float topRadius, int ringVerts){
		Mesh mesh = new Mesh ();
		Vector3[] cylinderVerts = cylinder (length, bottomRadius, topRadius, ringVerts);
		mesh.vertices = cylinderVerts;
		mesh.triangles = cylinderTriangles (cylinderVerts);
		mesh.RecalculateNormals ();
		return mesh;
	}

	/*
	 * Mesh Calculation 
	 */

	//Create the Mesh
	//calculate vertices of a circle with a number of vertices and radius.
	static Vector2[] circleWithVertices (float radius, int vertices)
	{
		//Angle of separation in radians.
		float separationAngle = (Mathf.PI * 2) / vertices;
		Vector2[] resultingVerts = new Vector2[vertices];
		for (int i = 0; i < vertices; i++) {
			float xPos = Mathf.Sin (separationAngle * i) * radius;
			float yPos = Mathf.Cos (separationAngle * i) * radius;
			Vector2 newVert = new Vector2 (xPos, yPos); 
			resultingVerts [i] = newVert;
		}
		return resultingVerts;
	}

	//Creates an array of vertices representing a cylinder.
	//The first half represents the bottom circle, the top the top circle.
	static Vector3[] cylinder (float length, float bottomRadius, float topRadius, int ringVerts)
	{
		Vector3[] resultingVerts = new Vector3[ringVerts * 2];

		Vector2[] circleVerts = new Vector2[ringVerts * 2];
		//Append the bottom circle and then the top.
		circleWithVertices (bottomRadius, ringVerts).CopyTo (circleVerts, 0);
		circleWithVertices (topRadius, ringVerts).CopyTo (circleVerts, ringVerts);

		//Add a third dimension to the circle verts;
		for (int i = 0; i < circleVerts.Length / 2; i++) {
			Vector2 currentVert = circleVerts [i];
			resultingVerts [i] = new Vector3 (currentVert.x, 0, currentVert.y);

			//Do the same to the vert directly above. (In the same loop for efficiency.)
			resultingVerts [i + circleVerts.Length / 2] = new Vector3 (circleVerts [i + circleVerts.Length / 2].x, length, circleVerts [i + circleVerts.Length / 2].y);

		}
		return resultingVerts;
	}

	static int[] cylinderTriangles (Vector3[] vertices)
	{

		//The number of traingle edges that should make up the sides and caps.
		int sideTriangles = ((vertices.Length / 2) * 6);
		int capTriangles = (((vertices.Length / 2) - 2) * 3 * 2);
		int[] cylinderTriangles = new int[sideTriangles + capTriangles];

		//for vertex n, n + circle offset should correspond to the vertex directly above it.
		int circleOffset = vertices.Length / 2;
		//The sides	
		int t = 0;
		for (int i = 0; t < sideTriangles - 6; t += 6, i += 1) {
			cylinderTriangles [t] = cylinderTriangles [t + 5] = i;
			cylinderTriangles [t + 1] = cylinderTriangles [t + 4] = i + circleOffset + 1;
			cylinderTriangles [t + 2] = i + circleOffset;
			cylinderTriangles [t + 3] = i + 1;
		}
		//Close the shape. This connects the last side to the first side.
		cylinderTriangles [t] = cylinderTriangles [t + 5] = circleOffset - 1;
		cylinderTriangles [t + 1] = cylinderTriangles [t + 4] = circleOffset;
		cylinderTriangles [t + 2] = (circleOffset * 2) - 1;
		cylinderTriangles [t + 3] = 0;
		t += 6;

		//fill the top and bottom circles.
		for (int i = 0, capT = 0; capT < capTriangles; capT += 6, i++) {
			cylinderTriangles [t + capT + 2] = 0;
			cylinderTriangles [t + capT + 1] = i + 1;
			cylinderTriangles [t + capT] = i + 2;
			cylinderTriangles [t + capT + 3] = circleOffset;
			cylinderTriangles [t + capT + 4] = i + circleOffset + 1;
			cylinderTriangles [t + capT + 5] = i + circleOffset + 2;

		}
		return cylinderTriangles;

	}
}
