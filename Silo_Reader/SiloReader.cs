using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Parabox.CSG; 

public class region {
	public string typeflag;
	public int leftID;
	public int rightID;
	
	public region(string typeflag, int leftID, int rightID){
		this.typeflag = typeflag;
		this.leftID = leftID;
		this.rightID = rightID;
	}

}

public struct SiloMaterial {
	public string name;
	public Color color;
}

public struct Type {
	public string type;
	public List<float> coefficients;

}
public struct SiloData {
	public string CSGName;
	public int BlockNumber;
	public int GroupNumber;
	public int Cycle;
	public string Units;
	public string Lables;
	public int Dimensions;
	public int nCoefficients;
	public int nTypes;
	public List<Type> Types;
	public int nRegions;
	public int origin;
	public List<region> Regions;
	public int nZones;
	public List<int> Zones;
	public int nMats;
	public Dictionary<int, SiloMaterial> materials;
	public List<int> matlist;
}

public class SiloReader
{

    public static SiloData ReadFile (string FileName) {
        FileInfo theSourceFile = new FileInfo (FileName);
        StreamReader reader = theSourceFile.OpenText();
		SiloData sd = new SiloData();
		sd.Types = new List<Type>();
		sd.Regions = new List<region>();
		sd.Zones = new List<int>();
		sd.materials = new Dictionary<int, SiloMaterial>();
		sd.matlist = new List<int>();
		//parse
		sd.CSGName = reader.ReadLine();
		sd.BlockNumber = Int32.Parse(reader.ReadLine());
		sd.GroupNumber = Int32.Parse(reader.ReadLine());
		sd.Cycle = Int32.Parse(reader.ReadLine());
		sd.Units = reader.ReadLine();
		sd.Lables = reader.ReadLine();
		sd.Dimensions = Int32.Parse(reader.ReadLine());
		sd.nCoefficients = Int32.Parse(reader.ReadLine());
		sd.nTypes = Int32.Parse(reader.ReadLine());
		for(int i = 0; i < sd.nTypes; i++){
			sd.Types.Add(ReadType(reader));
		}
		sd.nRegions = Int32.Parse(reader.ReadLine());
		sd.origin = Int32.Parse(reader.ReadLine());
		for(int i = 0; i < sd.nRegions; i++){
			sd.Regions.Add(new region(
						reader.ReadLine(),
						Int32.Parse(reader.ReadLine()),
						Int32.Parse(reader.ReadLine())));
		}
		sd.nZones = Int32.Parse(reader.ReadLine());
		for(int i = 0; i < sd.nZones; i++){
			sd.Zones.Add(Int32.Parse(reader.ReadLine()));
		}
		sd.nMats = Int32.Parse(reader.ReadLine());
		for(int i = 0; i < sd.nMats; i++){
			String color;
			float r, g, b, a = 1;
			SiloMaterial mat = new SiloMaterial();
			int index = Int32.Parse(reader.ReadLine());
			mat.name = reader.ReadLine();
			color = reader.ReadLine();
			r = int.Parse(color.Substring(1,2), System.Globalization.NumberStyles.HexNumber);
			g = int.Parse(color.Substring(3,2), System.Globalization.NumberStyles.HexNumber);
			b = int.Parse(color.Substring(5,2), System.Globalization.NumberStyles.HexNumber);
			mat.color = new Color(r/256.0f,g/256.0f,b/256.0f,a);
			sd.materials.Add(index, mat);
		}

		for(int i = 0; i < sd.nZones; i++){
			sd.matlist.Add(Int32.Parse(reader.ReadLine()));

		}

		reader.Close();
		return sd;
	}

	public static List<CSG_Tree> GenerateTree(SiloData sd){
		float infinite = 10000f ;
		float scale = 0.01f;
		int sphere_gen = 7;
		int cylinder_gen = 5;
		List<CSG_Tree> csgtree = new List<CSG_Tree>();
		List<CSG_Tree> nodetree = new List<CSG_Tree>();
		List<CSG_Tree> rendertree = new List<CSG_Tree>();
		//build nodes
		for(int i =0 ; i < sd.Types.Count; i++){
			switch(sd.Types[i].type.ToLower()){
				case "quadric":
					{
						MeshGenerator.Axis axis;
						float x, y, z, radius, length;
						float a200 = sd.Types[i].coefficients[0];
						float a020 = sd.Types[i].coefficients[1];
						float a002 = sd.Types[i].coefficients[2];
						float a110 = sd.Types[i].coefficients[3];
						float a011 = sd.Types[i].coefficients[4];
						float a101 = sd.Types[i].coefficients[5];
						float a100 = sd.Types[i].coefficients[6];
						float a010 = sd.Types[i].coefficients[7];
						float a001 = sd.Types[i].coefficients[8];
						float a000 = sd.Types[i].coefficients[9];
						
						//cases axis alligned cylinder
						if(a110 == 0 && a011 == 0 && a101 == 0){
							
							if(a200 == 1 && a020 == 1 && a002 == 0){
							//z axis cylinder
								x = -a100/2.0f;
								y = -a010/2.0f;
								z = 0;
								length = infinite;
								radius = -(a000 - (x*x + y*y)) ;
								axis = MeshGenerator.Axis.Z_AXIS;
							
							} else if(a200 == 1 && a020 == 0 && a002 == 1){
							//y axis cylinder
								x = -a100/2.0f;
								y = 0;
								z = -a010/2.0f;
								length = infinite;
								radius = -(a000 - (x*x + z*z));
								axis = MeshGenerator.Axis.Y_AXIS;
								
							} else if(a200 == 0 && a020 == 1 && a002 == 1){
							//x axis cylinder
								x = 0;
								y = -a010/2.0f;
								z = -a010/2.0f;
								length = infinite;
								radius = -(a000 - (y*y + z*z)) ;
								axis = MeshGenerator.Axis.X_AXIS;
							
							} else{
								Debug.Log(sd.Types[i].type + " - Not Implemented");
								break;
							}

							csgtree.Add(new CSG_Tree(
									MeshGenerator.generate_axis_alligned_cylinder(
										x * scale, y * scale, z * scale, 
										Mathf.Sqrt(radius) * scale, length*scale,
										axis, cylinder_gen).ToMesh()));

						} else {
							Debug.Log(sd.Types[i].type + " - Not Implemented");
						}
					}
					break;
				case "sphere":
					{
						float x = sd.Types[i].coefficients[0]* scale; 
						float y = sd.Types[i].coefficients[1]* scale; 
						float z = sd.Types[i].coefficients[2]* scale;
						float r = sd.Types[i].coefficients[3]* scale;
						if(r > 10.0f){
							Debug.Log("Exceeds Sphere Max");
							csgtree.Add(new CSG_Tree(MeshGenerator.generate_sphere(
										x,y,z,r*1.4f,0).ToMesh()));
							break;
						}
						csgtree.Add(new CSG_Tree(MeshGenerator.generate_sphere(
										x,y,z,r,sphere_gen).ToMesh()));
					}
					break;
				case "ellipsoid":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "plane_g":
					{
						MeshGenerator.Axis axis;
						float x = sd.Types[i].coefficients[0];
						float y = sd.Types[i].coefficients[1];
						float z = sd.Types[i].coefficients[2];
						float c = sd.Types[i].coefficients[3];
						Vector3 v0, v1, v2, v3;
							v0 = new Vector3(0,0,0);
							v1 = new Vector3(0,0,0);
							v2 = new Vector3(0,0,0);
							v3 = new Vector3(0,0,0);
						if(x == 0 && y == 0){
							//z axis plane
							v0 = new Vector3(-infinite/2.0f, -infinite/2.0f, -c*z);
							v1 = new Vector3(-infinite/2.0f, infinite/2.0f, -c*z);
							v2 = new Vector3(infinite/2.0f, -infinite/2.0f, -c*z);
							v3 = new Vector3(infinite/2.0f, infinite/2.0f, -c*z);
						} else if(x == 0 && z == 0){
							//y axis plane
							v0 = new Vector3(-infinite/2.0f, -c*y,-infinite/2.0f);
							v1 = new Vector3(-infinite/2.0f, -c*y, infinite/2.0f);
							v2 = new Vector3(infinite/2.0f, -c*y,-infinite/2.0f);
							v3 = new Vector3(infinite/2.0f, -c*y, infinite/2.0f);
						} else if(y == 0 && z == 0){
							//x axis plane
							v0 = new Vector3(-c*x,-infinite/2.0f, -infinite/2.0f);
							v1 = new Vector3(-c*x,-infinite/2.0f, infinite/2.0f);
							v2 = new Vector3(-c*x,infinite/2.0f, -infinite/2.0f);
							v3 = new Vector3(-c*x,infinite/2.0f, infinite/2.0f);
						} else if(x == 0){
							//yz plane
							v0 = new Vector3(-infinite/2.0f, infinite/2.0f, 
									(c - infinite/2.0f * y)/z);
							v1 = new Vector3(infinite/2.0f, infinite/2.0f, 
									(c - infinite/2.0f * y)/z);
							v2 = new Vector3(-infinite/2.0f, -infinite/2.0f, 
									(c + infinite/2.0f * y)/z);
							v3 = new Vector3(infinite/2.0f, -infinite/2.0f, 
									(c + infinite/2.0f * y)/z);
						} else if(y == 0){
							//xz plane
							v0 = new Vector3(infinite/2.0f, -infinite/2.0f, 
									(c - infinite/2.0f * x)/y);
							v1 = new Vector3(infinite/2.0f, infinite/2.0f, 
									(c - infinite/2.0f * x)/y);
							v2 = new Vector3(-infinite/2.0f, -infinite/2.0f, 
									(c + infinite/2.0f * x)/y);
							v3 = new Vector3(-infinite/2.0f, infinite/2.0f, 
									(c + infinite/2.0f * x)/y);
						} else if(z == 0){
							//xy plane
							v0 = new Vector3(infinite/2.0f, (c - infinite/2.0f * z)/x, 
									-infinite/2.0f);
							v1 = new Vector3(infinite/2.0f, (c - infinite/2.0f * z)/x, 
									infinite/2.0f);
							v2 = new Vector3(-infinite/2.0f, (c - infinite/2.0f * z)/x, 
									-infinite/2.0f);
							v3 = new Vector3(-infinite/2.0f, (c - infinite/2.0f * z)/x, 
									infinite/2.0f);
						} else {
							Debug.Log(sd.Types[i].type + " - Not Implemented");
							//xyz plane

						}
						csgtree.Add(new CSG_Tree(MeshGenerator.generate_arbitrary_quad(
								v0 * scale, v1 * scale, v2 * scale, v3 * scale, 
								new Vector3((c-1)*x,(c-1)*y,(c-1)*z) * scale, 1).ToMesh())); 

					}
					break;
				case "plane_x":
					csgtree.Add(new CSG_Tree(MeshGenerator.generate_square(
									sd.Types[i].coefficients[0]* scale, 
									0, 0, infinite*scale, MeshGenerator.Axis.X_AXIS, 
									new Vector3(sd.Types[i].coefficients[0]* scale - 1.0f,0,0)
									, 1).ToMesh())); 
					break;
				case "plane_y":
					csgtree.Add(new CSG_Tree(MeshGenerator.generate_square(
									0, sd.Types[i].coefficients[0]* scale, 
									0, infinite*scale, MeshGenerator.Axis.Y_AXIS, 
									new Vector3(0, sd.Types[i].coefficients[0]* scale - 1.0f,0),
									1).ToMesh())); 
					break;
				case "plane_z":
					csgtree.Add(new CSG_Tree(MeshGenerator.generate_square(
									0, 0,sd.Types[i].coefficients[0]* scale, 
									infinite*scale, MeshGenerator.Axis.Z_AXIS, 
									new Vector3(0,0,sd.Types[i].coefficients[0]* scale - 1.0f),
									1).ToMesh())); 
					break;
				case "plane_pn":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "plane_ppp":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "cylinder_pnlr":
					{
						MeshGenerator.Axis axis;
						float x, y, z;
						float length, radius;
						length = sd.Types[i].coefficients[6];
						radius = sd.Types[i].coefficients[7];
						//TODO : Assumes cylinder is axis aligned, will fail if not
						if(sd.Types[i].coefficients[3] != 0){	
							axis = MeshGenerator.Axis.X_AXIS;
							x = sd.Types[i].coefficients[0] + 0.5f*length;
							y = sd.Types[i].coefficients[1];
							z = sd.Types[i].coefficients[2];
						} else if(sd.Types[i].coefficients[4] != 0){
							axis = MeshGenerator.Axis.Y_AXIS;
							x = sd.Types[i].coefficients[0];
							y = sd.Types[i].coefficients[1] + 0.5f*length;
							z = sd.Types[i].coefficients[2];
						} else if(sd.Types[i].coefficients[5] != 0){
							axis = MeshGenerator.Axis.Z_AXIS;
							x = sd.Types[i].coefficients[0];
							y = sd.Types[i].coefficients[1] + 0.5f*length;
							z = sd.Types[i].coefficients[2];
						} else {
							axis = MeshGenerator.Axis.X_AXIS;
							x = sd.Types[i].coefficients[0] + 0.5f*length;
							y = sd.Types[i].coefficients[1];
							z = sd.Types[i].coefficients[2];
						}
						csgtree.Add(new CSG_Tree(
									MeshGenerator.generate_axis_alligned_cylinder(
										x* scale, y* scale, z* scale, 
										radius* scale, length* scale,
										axis, cylinder_gen).ToMesh()));

					}
					break;
				case "cylinder_ppr":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "box":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "cone_pnla":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "cone_ppa":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "polyhedron":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "hex":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "tet":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "pyramid":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "prism":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
			}


		}
		//construct tree
		for(int i = 0 ; i < sd.Regions.Count; i++){
				switch(sd.Regions[i].typeflag.ToLower()){
					case "inner":
						nodetree.Add(new CSG_Tree (csgtree[sd.Regions[i].leftID],
									csgtree[sd.Regions[i].leftID], 
								CSG_Operation.Inner));
						break;
					case "outer":
						nodetree.Add(new CSG_Tree (csgtree[sd.Regions[i].leftID], 
									csgtree[sd.Regions[i].leftID], 
								CSG_Operation.Outer));
						break;
					case "on":
						nodetree.Add(new CSG_Tree (csgtree[sd.Regions[i].leftID], 
									csgtree[sd.Regions[i].leftID], 
								CSG_Operation.On));
						break;
					case "union":
						nodetree.Add(new CSG_Tree(
								nodetree[sd.Regions[i].leftID], 
								nodetree[sd.Regions[i].rightID], 
								CSG_Operation.Union));
						break;
					case "intersect":
						nodetree.Add(new CSG_Tree(
								nodetree[sd.Regions[i].leftID], 
								nodetree[sd.Regions[i].rightID], 
								CSG_Operation.Intersect));
						break;
					case "diff":
						nodetree.Add(new CSG_Tree(
								nodetree[sd.Regions[i].leftID], 
								nodetree[sd.Regions[i].rightID], 
								CSG_Operation.Subtract));
						break;
					case "compliment":
						nodetree.Add(new CSG_Tree(
								nodetree[sd.Regions[i].leftID], 
								nodetree[sd.Regions[i].leftID], 
								CSG_Operation.Compliment));
						break;
					case "xform":
						Debug.Log(sd.Regions[i].typeflag + " - Not Implemented");
						break;
					case "sweep":
						Debug.Log(sd.Regions[i].typeflag + " - Not Implemented");
						break;
				}
		}
		//extract render objects
		for(int i = 0; i < sd.Zones.Count; i++){
			rendertree.Add(nodetree[sd.Zones[i]]);
		}

		return rendertree;
	}

	public static Type ReadType(StreamReader reader){
		Type t = new Type();
		t.type = reader.ReadLine();
		t.coefficients = new List<float>();
		int coeffcount = 0;

		switch (t.type.ToLower()){
			case "quadric":
				coeffcount = 10;
				break;
			case "sphere":
				coeffcount = 4;
				break;
			case "ellipsoid":
				coeffcount = 6;
				break;
			case "plane_g":
				coeffcount = 4;
				break;
			case "plane_x":
				coeffcount = 1;
				break;
			case "plane_y":
				coeffcount = 1;
				break;
			case "plane_z":
				coeffcount = 1;
				break;
			case "plane_pn":
				coeffcount = 6;
				break;
			case "plane_ppp":
				coeffcount = 9;
				break;
			case "cylinder_pnlr":
				coeffcount = 8;
				break;
			case "cylinder_ppr":
				coeffcount = 7;
				break;
			case "box":
				coeffcount = 6;
				break;
			case "cone_pnla":
				coeffcount = 8;
				break;
			case "cone_ppa":
				coeffcount = -1;
				break;
			case "polyhedron":
				coeffcount = -1;
				break;
			case "hex":
				coeffcount = 36;
				break;
			case "tet":
				coeffcount = 24;
				break;
			case "pyramid":
				coeffcount = 30;
				break;
			case "prism":
				coeffcount = 30;
				break;
		}
		for(int i = 0; i < coeffcount; i++){
			t.coefficients.Add(Convert.ToSingle(Double.Parse(reader.ReadLine())));
		}
		return t;

	}
	public static void PrintStructure(SiloData sd){
		Debug.Log(sd.CSGName);
		Debug.Log(sd.BlockNumber);
		Debug.Log(sd.Cycle);
		Debug.Log(sd.Units);
		Debug.Log(sd.Lables);
		Debug.Log(sd.Dimensions);
		Debug.Log(sd.nCoefficients);
		Debug.Log(sd.nTypes);
		for(int i = 0; i < sd.nTypes; i++){
			Debug.Log(sd.Types[i].type);
			for(int j = 0; j < sd.Types[i].coefficients.Count; j++){
				Debug.Log(sd.Types[i].coefficients[j]);
			}
		}
		Debug.Log(sd.nRegions);
		Debug.Log(sd.origin);
		for(int i = 0; i < sd.nRegions; i++){
			Debug.Log(sd.Regions[i].typeflag);
			Debug.Log(sd.Regions[i].leftID);
			Debug.Log(sd.Regions[i].rightID);
		}
		Debug.Log(sd.nZones);
		for(int i = 0; i < sd.nZones; i++){
			Debug.Log(sd.Zones[i]);
		}

	}
   
}
