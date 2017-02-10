# pb_CSG

A fork of Evan W's port of [CSG.js](http://evanw.github.io/csg.js/) for the Unity Game engine with a more efficient and correct interface.

## Description of Changes

This fork of pb_CSG adds a CSG Binary Tree to remove problems of converting to and from GameObjects for complex shapes (In the original code, objects with more than one Subtraction operation would lose their volume and become flat). Moreover this provides a more efficient workflow to creating complex shapes.




##Example use:

To build this shape:
![]("https://en.wikipedia.org/wiki/Constructive_solid_geometry#/media/File:Csg_tree.png")

	// Include the library
	using Parabox.CSG;
	
	//Generate Primitives
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
	composite.AddComponent<MeshRenderer>().sharedMaterial = wireframeMaterial;
	
	//remove primatives
	Destroy(sphere);
	Destroy(cube);
	Destroy(cyl1);
	Destroy(cyl2);
	Destroy(cyl3);



Result:

![]("Images/CSG_Tree_Example_1.png")

