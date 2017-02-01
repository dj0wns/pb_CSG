using UnityEngine;
using System.Collections;
using Parabox.CSG;
using System.Linq;

/**
 * Simple demo of CSG operations.
 */
public class Demo : MonoBehaviour
{
	GameObject composite, composite_normal,  temp;
	bool wireframe = false;

	Vector3 origin = new Vector3(-0.5f, -1f, -4.5f);
	public Material wireframeMaterial = null;


	void Awake()
	{
		subtract_test();
	}

	void subtract_test(){

		//successfully subtracts the sphere from the cube
		//translation layer seems to be what ruins the operations
		GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
		GameObject right = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Mesh m;
		
		left.transform.position = origin;
		right.transform.position =origin;
		
		left.transform.localScale = Vector3.one * 1.0f;
		right.transform.localScale = Vector3.one * 1.3f;

		m = CSG.Subtract(left, right);
		
		Destroy (right);
		Destroy (left);

		composite = new GameObject();
		composite.AddComponent<MeshFilter>().sharedMesh = m;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;
	}


	void wiki_sample(){

		//right side of tree
		// Initialize two new meshes in the scene
		GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		Mesh m;
		right.transform.Rotate(90, 90, 0);


		left.transform.position = origin;
		right.transform.position =origin;

		// Perform boolean operation
		m = CSG.Union(left, right);

		Destroy (right);
		// Create a gameObject to render the result
		composite = new GameObject();
		composite.AddComponent<MeshFilter>().sharedMesh = m;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;

		left.transform.Rotate(0, 90, 90);

		m = CSG.Union(left, composite);
		Destroy (left);
		Destroy (composite);

		composite= new GameObject();
		composite.AddComponent<MeshFilter>().sharedMesh = m;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;


		//left side of tree
		left = GameObject.CreatePrimitive(PrimitiveType.Cube);
		right = GameObject.CreatePrimitive(PrimitiveType.Sphere);

		right.transform.localScale = Vector3.one * 2.1f;
		left.transform.localScale = Vector3.one * 1.6f;

		left.transform.position = origin;
		right.transform.position =origin;
		m = CSG.Intersect(left, right);

		temp = new GameObject(); 
		temp.AddComponent<MeshFilter>().sharedMesh = m;
		temp.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;

		Destroy (left);
		Destroy (right);

		//finish
		m = CSG.Subtract(temp, composite);
		Mesh m_normal = CSG.Subtract (temp, composite);
		Destroy (composite);

		composite= new GameObject();
		composite.AddComponent<MeshFilter>().sharedMesh = m;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;

		m_normal.triangles = m.triangles.Reverse ().ToArray ();

		composite_normal= new GameObject();
		composite_normal.AddComponent<MeshFilter>().sharedMesh = m_normal;
		composite_normal.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;

		Destroy (temp);



	}

}
