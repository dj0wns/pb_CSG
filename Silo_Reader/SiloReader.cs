using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
 

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
