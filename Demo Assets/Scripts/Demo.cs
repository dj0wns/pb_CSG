using UnityEngine;
using System;
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
	public Material genericMaterial = null;
	public Material wireframeMaterial_red = null;
	public Material wireframeMaterial_blue = null;
	public Material wireframeMaterial_green = null;


	void Awake()
	{
		objects = new List<GameObject>();

		Silo_Test();
		//Mesh_Generator_Test();
		//CSG_Tree_Test();
		
	}
	
	void Silo_Test(){
		SiloData sd = SiloReader.ReadFile("Assets/pb_CSG/Demo Assets/DATA/csg.fake");
	//	SiloReader.PrintStructure(sd);
		List<CSG_Tree> csgtree = SiloReader.GenerateTree(sd);
		for(int i = 0; i < csgtree.Count; i++){
			try{
				csgtree[i].render();

				//create new game object from result
  				composite= new GameObject();
  				composite.transform.position = origin;
				composite.AddComponent<MeshFilter>().sharedMesh = csgtree[i].getMesh();
	  			composite.AddComponent<MeshRenderer>().material = genericMaterial;
				composite.GetComponent<MeshRenderer>().material.color = sd.materials[sd.matlist[i]].color;
			
				objects.Add(composite);
			} catch(Exception e) {
				Debug.Log(e);
			}
			csgtree[i].remove_references();
		}

	}

	void Mesh_Generator_Test(){
		Color color = new Color(0.484f, 0.984f, 0.0f, 1);
//		composite= new GameObject();
//		composite.transform.position = origin;
//		composite.AddComponent<MeshRenderer>().material = wireframeMaterial_blue;
//		composite.AddComponent<MeshFilter>().sharedMesh = 
//			MeshGenerator.generate_cube(origin.x+12, origin.y, origin.z, 
//				1.4f, 1.4f, 1.4f).ToMesh();
//		
//		objects.Add(composite);
		
//		composite= new GameObject();
//		composite.transform.position = origin;
//		composite.AddComponent<MeshRenderer>().material = wireframeMaterial_blue;
//		composite.AddComponent<MeshFilter>().sharedMesh = 
//		MeshGenerator.generate_sphere(origin.x+8, origin.y, origin.z, 1f, 10).ToMesh();
//		objects.Add(composite);

	  	composite = new GameObject();
	  	composite.transform.position = origin;
	 	composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_blue;
	 	composite.AddComponent<MeshFilter>().sharedMesh = 
		MeshGenerator.generate_axis_alligned_cylinder(origin.x+4, origin.y, origin.z, 
	  		0.8f, 2.0f, MeshGenerator.Axis.Y_AXIS,  2).ToMesh();
	  
	 	objects.Add(composite);
	  
//		composite = new GameObject();
//		composite.transform.position = origin;
//		composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial_blue;
//		composite.AddComponent<MeshFilter>().sharedMesh = 
//		MeshGenerator.generate_axis_alligned_cone(origin.x, origin.y, origin.z, 
//				0.8f, 2.0f, MeshGenerator.Axis.Y_AXIS,  3).ToMesh();
//		
//		objects.Add(composite);

	}
	

	void CSG_Tree_Test(){
		
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

}
