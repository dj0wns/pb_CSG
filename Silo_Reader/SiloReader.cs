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
	public List<int> Types;
	public List<float> Coefficients;
	public int nRegions;
	public List<region> Regions;
	public int nZones;
	public List<int> Zones;
}

public class SiloReader
{
    private FileInfo theSourceFile = null;
    private StreamReader reader = null;
 
    public SiloData ReadFile (string FileName) {
        theSourceFile = new FileInfo (FileName);
        reader = theSourceFile.OpenText();
		SiloData sd = new SiloData();
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
			sd.Types.Add(Int32.Parse(reader.ReadLine()));
		}

		for(int i = 0; i < sd.nCoefficients; i++){
			sd.Coefficients.Add(Single.Parse(reader.ReadLine()));
		}
		sd.nRegions = Int32.Parse(reader.ReadLine());
		for(int i = 0; i < sd.nCoefficients; i++){
			sd.Regions.Add(new region(
						reader.ReadLine(),
						Int32.Parse(reader.ReadLine()),
						Int32.Parse(reader.ReadLine())));
		}
		sd.nZones = Int32.Parse(reader.ReadLine());
		for(int i = 0; i < sd.nZones; i++){
			sd.Zones.Add(Int32.Parse(reader.ReadLine()));
		}
		return sd;
	}

	public void PrintStructure(SiloData sd){
		Debug.Log(sd.CSGName);
		Debug.Log(sd.BlockNumber);
		Debug.Log(sd.Cycle);
		Debug.Log(sd.Units);
		Debug.Log(sd.Lables);
		Debug.Log(sd.Dimensions);
		Debug.Log(sd.nCoefficients);
		Debug.Log(sd.nTypes);
		for(int i = 0; i < sd.nTypes; i++){
			Debug.Log(sd.Types[i]);
		}
		for(int i = 0; i < sd.nCoefficients; i++){
			Debug.Log(sd.Coefficients[i]);
		}
		Debug.Log(sd.nRegions);
		for(int i = 0; i < sd.nRegions; i++){
			Debug.Log(sd.Regions[i]);
		}
		Debug.Log(sd.nZones);
		for(int i = 0; i < sd.nZones; i++){
			Debug.Log(sd.Zones[i]);
		}

	}
   
	~SiloReader(){
		reader.Close();
	}
}
