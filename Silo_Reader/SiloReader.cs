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
		reader.Close();
		return sd;
	}

	public static List<CSG_Tree> GenerateTree(SiloData sd){
		List<CSG_Tree> csgtree = new List<CSG_Tree>();
		List<CSG_Tree> nodetree = new List<CSG_Tree>();
		List<CSG_Tree> rendertree = new List<CSG_Tree>();
		Debug.Log(sd.Types.Count);
		//build nodes
		for(int i =0 ; i < sd.Types.Count; i++){
			switch(sd.Types[i].type.ToLower()){
				case "quadric":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "sphere":
					{
					//	GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        			//	sphere.transform.position = new Vector3(
					//			sd.Types[i].coefficients[0],
					//			sd.Types[i].coefficients[1],
					//			sd.Types[i].coefficients[2]);

					//	sphere.transform.localScale = new Vector3(
					//			sd.Types[i].coefficients[3],
					//			sd.Types[i].coefficients[3],
					//			sd.Types[i].coefficients[3]);
					//	csgtree.Add(new CSG_Tree(sphere));
						//Destroy(sphere);
					}
					csgtree.Add(new CSG_Tree(MeshGenerator.generate_sphere(
									sd.Types[i].coefficients[0], 
									sd.Types[i].coefficients[1], 
									sd.Types[i].coefficients[2],
									sd.Types[i].coefficients[3],0).ToMesh()));
					break;
				case "ellipsoid":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "plane_g":
					Debug.Log(sd.Types[i].type + " - Not Implemented");
					break;
				case "plane_x":
					csgtree.Add(new CSG_Tree(MeshGenerator.generate_square(
									sd.Types[i].coefficients[0], 
									0, 0, 100, MeshGenerator.Axis.X_AXIS, 
									new Vector3(0,0,0), 1).ToMesh())); 
					break;
				case "plane_y":
					csgtree.Add(new CSG_Tree(MeshGenerator.generate_square(
									0, sd.Types[i].coefficients[0], 
									0, 100, MeshGenerator.Axis.Y_AXIS, 
									new Vector3(0,0,0), 1).ToMesh())); 
					break;
				case "plane_z":
					csgtree.Add(new CSG_Tree(MeshGenerator.generate_square(
									0, 0,sd.Types[i].coefficients[0], 
									100, MeshGenerator.Axis.Z_AXIS, 
									new Vector3(0,0,0), 1).ToMesh())); 
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
						//TODO : Assumes cylinder is axis aligned, will fail if not
						if(sd.Types[i].coefficients[3] != 0){	
							axis = MeshGenerator.Axis.X_AXIS;
						} else if(sd.Types[i].coefficients[4] != 0){
							axis = MeshGenerator.Axis.Y_AXIS;
						} else if(sd.Types[i].coefficients[5] != 0){
							axis = MeshGenerator.Axis.Z_AXIS;
						} else {
							axis = MeshGenerator.Axis.X_AXIS;
						}
						csgtree.Add(new CSG_Tree(
									MeshGenerator.generate_axis_alligned_cylinder(
										sd.Types[i].coefficients[0],
										sd.Types[i].coefficients[1],
										sd.Types[i].coefficients[2],
										sd.Types[i].coefficients[7],
										sd.Types[i].coefficients[6],
										axis, 2).ToMesh()));

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
		Debug.Log(csgtree.Count);
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
						Debug.Log(sd.Regions[i].typeflag + " - Not Implemented");
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
		Debug.Log(nodetree.Count);
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
			t.coefficients.Add(Single.Parse(reader.ReadLine()));
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
