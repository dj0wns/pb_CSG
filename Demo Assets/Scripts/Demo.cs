using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parabox.CSG;
using System.Linq;

/**
 * Simple demo of CSG operations.
 */
public class Demo : MonoBehaviour
{
	GameObject composite, composite_normal,  temp;
	bool wireframe = false;

	Vector3 origin = new Vector3(0.0f, 0f, 0.0f);
	Vector3 regionSize = new Vector3(5f,5f,5f);

	List<GameObject> objects; //keep track of objects created
	List<GameObject> points; //keep track of points created
	
	public Material wireframeMaterial = null;
	public Material wireframeMaterial_red = null;
	public Material wireframeMaterial_blue = null;
	public Material wireframeMaterial_green = null;


	void Awake()
	{
		//Silo_Test();
		Mesh_Generator_Test();
		//CSG_Tree_Test_With_Mesh_Gen();
	}
	
	void Silo_Test(){
		SiloData sd = SiloReader.ReadFile("/home/dj0wns/Documents/College/Senior_Year/Thesis/silo-practice/csg.fake");
		SiloReader.PrintStructure(sd);


	}

	void Mesh_Generator_Test(){
		objects = new List<GameObject>();
		composite= new GameObject();
		composite.transform.position = origin;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_green;
		composite.AddComponent<MeshFilter>().sharedMesh = 
			MeshGenerator.generate_cube(origin.x+8, origin.y, origin.z, 
				1.4f, 1.4f, 1.4f).ToMesh();

		objects.Add(composite);
		
		composite= new GameObject();
		composite.transform.position = origin;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_green;
		composite.AddComponent<MeshFilter>().sharedMesh = 
		MeshGenerator.generate_sphere(origin.x+6, origin.y, origin.z, 1.0f, 5).ToMesh();
		
		objects.Add(composite);

		composite = new GameObject();
		composite.transform.position = origin;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_green;
		composite.AddComponent<MeshFilter>().sharedMesh = 
		MeshGenerator.generate_axis_alligned_cylinder(origin.x+4, origin.y, origin.z, 
				0.8f, 2.0f, MeshGenerator.Axis.Y_AXIS,  3).ToMesh();
		
		objects.Add(composite);
		
		composite = new GameObject();
		composite.transform.position = origin;
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_green;
		composite.AddComponent<MeshFilter>().sharedMesh = 
		MeshGenerator.generate_axis_alligned_cone(origin.x+2, origin.y, origin.z, 
				0.8f, 2.0f, MeshGenerator.Axis.Y_AXIS,  3).ToMesh();
		
		objects.Add(composite);

	}
	

	void CSG_Tree_Test_With_Mesh_Gen(){
		
		//create leaf nodes
		CSG_Tree sphere_leaf = new CSG_Tree(
				MeshGenerator.generate_sphere(origin.x, origin.y, origin.z, 
					1.0f, 5).ToMesh());
		CSG_Tree cube_leaf = new CSG_Tree(
				MeshGenerator.generate_cube(origin.x, origin.y, origin.z, 
				1.4f, 1.4f, 1.4f).ToMesh());
		CSG_Tree cyl1_leaf = new CSG_Tree(
		MeshGenerator.generate_axis_alligned_cylinder(origin.x, origin.y, origin.z, 
				0.5f, 2.0f, MeshGenerator.Axis.X_AXIS,  2).ToMesh());
		CSG_Tree cyl2_leaf = new CSG_Tree(
		MeshGenerator.generate_axis_alligned_cylinder(origin.x, origin.y, origin.z, 
				0.5f, 2.0f, MeshGenerator.Axis.Y_AXIS,  2).ToMesh());
		CSG_Tree cyl3_leaf = new CSG_Tree(
		MeshGenerator.generate_axis_alligned_cylinder(origin.x, origin.y, origin.z, 
				0.5f, 2.0f, MeshGenerator.Axis.Z_AXIS,  2).ToMesh());

		//construct tree
		CSG_Tree cube_intersect_sphere = new CSG_Tree(cube_leaf, sphere_leaf, CSG_Operation.Intersect);
		CSG_Tree cyl1_union_cyl2 = new CSG_Tree(cyl1_leaf, cyl2_leaf, CSG_Operation.Union);
		CSG_Tree cyl3_union_cyl1_2 = new CSG_Tree(cyl3_leaf, cyl1_union_cyl2, CSG_Operation.Union);
		CSG_Tree CIS_subtract_cyls = new CSG_Tree(cube_intersect_sphere, cyl3_union_cyl1_2, CSG_Operation.Subtract);
		//render tree
		CIS_subtract_cyls.render();

		//create new game object from result
  		composite= new GameObject();
  		composite.transform.position = origin;
		composite.AddComponent<MeshFilter>().sharedMesh = CIS_subtract_cyls.getMesh();
	  	composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_green;
	
	}

	void CSG_Tree_Test(){
		
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		GameObject cyl1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		GameObject cyl2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		GameObject cyl3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		
		//rotations
		cyl2.transform.Rotate(90, 90, 0);
		cyl3.transform.Rotate(0, 90, 90);

		//sizing
		sphere.transform.localScale = Vector3.one * 2.1f;
		cube.transform.localScale = Vector3.one * 1.6f;
		cyl1.transform.localScale = Vector3.one * 1f;
		cyl2.transform.localScale = Vector3.one * 1f;
		cyl3.transform.localScale = Vector3.one * 1f;
		
		//create leaf nodes
		CSG_Tree sphere_leaf = new CSG_Tree(sphere);
		CSG_Tree cube_leaf = new CSG_Tree(cube);
		CSG_Tree cyl1_leaf = new CSG_Tree(cyl1);
		CSG_Tree cyl2_leaf = new CSG_Tree(cyl2);
		CSG_Tree cyl3_leaf = new CSG_Tree(cyl3);

		//construct tree
		CSG_Tree cube_intersect_sphere = new CSG_Tree(cube_leaf, sphere_leaf, CSG_Operation.Intersect);
		CSG_Tree cyl1_union_cyl2 = new CSG_Tree(cyl1_leaf, cyl2_leaf, CSG_Operation.Union);
		CSG_Tree cyl3_union_cyl1_2 = new CSG_Tree(cyl3_leaf, cyl1_union_cyl2, CSG_Operation.Union);
		CSG_Tree CIS_subtract_cyls = new CSG_Tree(cube_intersect_sphere, cyl3_union_cyl1_2, CSG_Operation.Subtract);
		//render tree
		CIS_subtract_cyls.render();

		//create new game object from result
		composite= new GameObject();
		composite.transform.position = origin;
		composite.AddComponent<MeshFilter>().sharedMesh = CIS_subtract_cyls.getMesh();
		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_green;

		//remove primatives
		Destroy(sphere);
		Destroy(cube);
		Destroy(cyl1);
		Destroy(cyl2);
		Destroy(cyl3);

	}
	
	void generate_random_points (int number, float size, Material wireframe_color){
		points = new List<GameObject>();
		GameObject temp;
		Rigidbody temp_rigid;
		float alpha, beta, gamma;
		for(int i = 0; i < number; i++){
			Vector3 position = new Vector3(
					Random.Range(origin.x - regionSize.x, origin.x + regionSize.x), 
					Random.Range(origin.y - regionSize.y, origin.y + regionSize.y), 
					Random.Range(origin.z - regionSize.z, origin.z + regionSize.z) );
			temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			temp.transform.position = position;
			temp.transform.localScale = Vector3.one * size;
			temp.GetComponent<Renderer>().material = wireframe_color;

			//generate isotropic random
			
    		float random_number = Random.Range(0f,1f);
    		gamma  = 1.0f - 2.0f*random_number;
    		float sine_gamma = Mathf.Sqrt(1.0f - gamma * gamma);
    		random_number = Random.Range(0f,1f);
    		float phi = Mathf.PI*(2.0f*random_number - 1.0f);
    		alpha = sine_gamma*Mathf.Cos(phi);
		    beta = sine_gamma*Mathf.Sin(phi);

			//set velocity
			temp_rigid = temp.AddComponent<Rigidbody>();
			temp_rigid.velocity = new Vector3(alpha, beta, gamma);
			temp_rigid.useGravity = false;
			temp_rigid.detectCollisions = false;

			points.Add(temp);
		}
	}

	void concentric_spheres(){

		objects = new List<GameObject>();
		objects.Add(generate_half_sphere(1, wireframeMaterial_blue));
		objects.Add(generate_half_sphere(2, wireframeMaterial_green, 1));
		objects.Add(generate_half_sphere(3, wireframeMaterial_red, 2));

	}

	GameObject generate_half_sphere(float size, Material wireframe_color){
		GameObject localObject;
		//outer sphere
		GameObject left = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Mesh m;

		left.transform.position = origin;
		right.transform.position = origin + new Vector3(0f, 0f, -size/2.0f);

		left.transform.localScale = Vector3.one * size;
		right.transform.localScale = Vector3.one * size;
	
		m = CSG.Subtract(left, right);
		
		Destroy (right);
		Destroy (left);
		
		localObject = new GameObject();
		localObject.AddComponent<MeshFilter>().sharedMesh = m;
		localObject.AddComponent<MeshRenderer>().sharedMaterial = wireframe_color;
		return localObject;
	}

	GameObject generate_half_sphere(float size, Material wireframe_color, float subtraction_size){
		GameObject localObject;
		//outer sphere
		GameObject left = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
		GameObject toSubtract = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Mesh m;

		left.transform.position = origin;
		right.transform.position = origin + new Vector3(0f, 0f, -size/2.0f);
		toSubtract.transform.position = origin;

		left.transform.localScale = Vector3.one * size;
		right.transform.localScale = Vector3.one * size;
		toSubtract.transform.localScale = Vector3.one * subtraction_size;
	
		m = CSG.Subtract(left, right);
		
		Destroy (right);
		Destroy (left);
		
		localObject = new GameObject();
		localObject.AddComponent<MeshFilter>().sharedMesh = m;

		m = CSG.Subtract(localObject, toSubtract);
		
		Destroy (localObject);
		Destroy (toSubtract);

		localObject = new GameObject();
		localObject.AddComponent<MeshFilter>().sharedMesh = m;
		localObject.AddComponent<MeshRenderer>().sharedMaterial = wireframe_color;
		return localObject;
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
