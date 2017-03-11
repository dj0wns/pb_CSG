using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Parabox.CSG;
using System.Linq;

//TODO implement these objects as a graph and convert to mesh

public class MeshNode
{
	public Vector3 position;
	public Vector3 normal;
	public Vector2 uv;
	public List<MeshNode> connections;
	int vertexNumber;
	public bool visited;

	public MeshNode (Vector3 position, Vector3 normal, Vector2 uv){
		this.position = position;
		this.normal = normal;
		this.uv = uv;
		vertexNumber = -1;
		connections = new List<MeshNode>();
		visited = false;
	}

	public void AddConnection(MeshNode node){
		connections.Add(node);
		node.connections.Add(this);
	}

	//assumes all nodes are currently in the visited state
	//recursively resets all nodes to not visited
	public void ResetVisited(){
		visited = false;
		for(int i = 0; i < connections.Count; i++){
			if(connections[i].visited != false){
				connections[i].ResetVisited();
			}

		}

	}

	//TODO removed reset visited by using a dynamic visited state
	public Mesh ToMesh(){
		ResetVisited();
		
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangles = new List<int>();
		Mesh result = new Mesh();
		
		GetPoints(vertices, normals, uvs, 0);
		ResetVisited();

		//build tris
		//if two neighbors to current node share a node other than the current one then its a triangle
		//mark current node visited, if any of the neighbors are visited then the tri is already accounted for 
		//use pointer to do equality, but vertex number can be used i guess
		GetTris(triangles);
	
		//Assign to mesh
		
		result.vertices = vertices.ToArray();
		result.normals = normals.ToArray();
		result.uv = uvs.ToArray();
		result.triangles = triangles.ToArray();
		
		return result;
	}

	private void GetTris(List<int> triangles){
		visited = true;
		//linear search because its unsorted
		//for each 2 connections of this
		//look for common connections 
		for(int i = 0; i < connections.Count ; i++){
			if(connections[i].visited == false){	
				for(int j = i+1; j < connections.Count ; j++){
					if(connections[j].visited == false){
						//TODO check for counter clockwise ness
						if(connections[i].connections.Contains(connections[j])){
							Vector3 dir = Vector3.Cross(connections[i].position - position,
									connections[j].position - position);
							Vector3 norm = Vector3.Normalize(dir);
							Vector3 norm2 = Vector3.Normalize(normal);
							if(Vector3.Dot(norm2, norm)>0){

								triangles.Add(vertexNumber);
								triangles.Add(connections[i].vertexNumber);
								triangles.Add(connections[j].vertexNumber);
							} else {
								triangles.Add(vertexNumber);
								triangles.Add(connections[j].vertexNumber);
								triangles.Add(connections[i].vertexNumber);
							}
						}

					}
				
				}
			} 

		}
		//recurse through
		for(int i = 0; i < connections.Count; i++){
			if(connections[i].visited == false){
				connections[i].GetTris(triangles);
			}

		}

	}


	private void GetPoints(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, int number){
		visited = true;
		vertexNumber = number;
		vertices.Add(position);
		normals.Add(normal);
		uvs.Add(uv);
		int increment = 1;
		for(int i = 0; i < connections.Count; i++){
			if(connections[i].visited == false){
				connections[i].GetPoints(vertices, normals, uvs, number + increment);
				increment++;
			} 

		}
	}
}

public class MeshGenerator
{

	public static MeshNode generate_sphere(float x, float y, float z, float radius, int generations){
		
		//cube length
		//TODO: check Math
		float length = radius / Mathf.Sqrt(3.0f);
		
		//first generate cube;
		MeshNode head = generate_cube(x, y, z, length, length, length);
		//TODO
		//Recursively add points between all connections
		//extend each point out to radius length
		//calculate normals and uv for each point
		return head;
	}
	
	//use meshnode n0 to return the non directional graph of a cube
	public static MeshNode generate_cube(float x, float y, float z, float a0, float a1, float a2){
		Vector3 center = new Vector3(x, y, z);
		//statically declare al 8 vertices
		//TODO figure out UV
		MeshNode n0 = new MeshNode(
				new Vector3(x-0.5f*a0, y-0.5f*a1, z-0.5f*a2), 
				new Vector3(x-0.5f*a0 - center.x, y-0.5f*a1 - center.y, z-0.5f*a2 - center.z),
				new Vector2(0,0));
		
		MeshNode n1 = new MeshNode(
				new Vector3(x+0.5f*a0, y-0.5f*a1, z-0.5f*a2), 
				new Vector3(x+0.5f*a0 - center.x, y-0.5f*a1 - center.y, z-0.5f*a2 - center.z),
				new Vector2(1,0));
		
		MeshNode n2 = new MeshNode(
				new Vector3(x-0.5f*a0, y+0.5f*a1, z-0.5f*a2), 
				new Vector3(x-0.5f*a0 - center.x, y+0.5f*a1 - center.y, z-0.5f*a2 - center.z),
				new Vector2(0,1));
		
		MeshNode n3 = new MeshNode(
				new Vector3(x+0.5f*a0, y+0.5f*a1, z-0.5f*a2), 
				new Vector3(x+0.5f*a0 - center.x, y+0.5f*a1 - center.y, z-0.5f*a2 - center.z),
				new Vector2(1,1));
		
		MeshNode n4 = new MeshNode(
				new Vector3(x-0.5f*a0, y-0.5f*a1, z+0.5f*a2), 
				new Vector3(x-0.5f*a0 - center.x, y-0.5f*a1 - center.y, z+0.5f*a2 - center.z),
				new Vector2(0,0));
		
		MeshNode n5 = new MeshNode(
				new Vector3(x+0.5f*a0, y-0.5f*a1, z+0.5f*a2), 
				new Vector3(x+0.5f*a0 - center.x, y-0.5f*a1 - center.y, z+0.5f*a2 - center.z),
				new Vector2(1,0));
		
		MeshNode n6 = new MeshNode(
				new Vector3(x-0.5f*a0, y+0.5f*a1, z+0.5f*a2), 
				new Vector3(x-0.5f*a0 - center.x, y+0.5f*a1 - center.y, z+0.5f*a2 - center.z),
				new Vector2(0,1));

		MeshNode n7 = new MeshNode(
				new Vector3(x+0.5f*a0, y+0.5f*a1, z+0.5f*a2), 
				new Vector3(x+0.5f*a0 - center.x, y+0.5f*a1 - center.y, z+0.5f*a2 - center.z),
				new Vector2(1,1));
		//cube vertices
		//4______6
		//|\     |\
		//| \    | \
		//|  \5__|__7   
		//0___|__2  |
		// \  |  \  |
		//  \ |   \ | 
		//   \1_____3
		
		//create connections all basic triangles <3
		n0.AddConnection(n1);
		n0.AddConnection(n2);
		n0.AddConnection(n3);
		n0.AddConnection(n4);
		n0.AddConnection(n5);
		n0.AddConnection(n6);

		n1.AddConnection(n3);
		n1.AddConnection(n5);
	
		n2.AddConnection(n3);
		n2.AddConnection(n6);
		
		n3.AddConnection(n5);
		n3.AddConnection(n6);
		n3.AddConnection(n7);
	
		n4.AddConnection(n5);
		n4.AddConnection(n6);
		n4.AddConnection(n7);
		
		n5.AddConnection(n7);
		n6.AddConnection(n7);

		return n0;
	}

	public static Mesh generate_cube_old(float x, float y, float z, float a0, float a1, float a2){
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		Mesh result = new Mesh();

		Vector3 center = new Vector3(x, y, z);
	
		//generate the 8 vertices
		vertices.Add(new Vector3(x-0.5f*a0, y-0.5f*a1, z-0.5f*a2));
		vertices.Add(new Vector3(x+0.5f*a0, y-0.5f*a1, z-0.5f*a2));
		vertices.Add(new Vector3(x-0.5f*a0, y+0.5f*a1, z-0.5f*a2));
		vertices.Add(new Vector3(x+0.5f*a0, y+0.5f*a1, z-0.5f*a2));
		vertices.Add(new Vector3(x-0.5f*a0, y-0.5f*a1, z+0.5f*a2));
		vertices.Add(new Vector3(x+0.5f*a0, y-0.5f*a1, z+0.5f*a2));
		vertices.Add(new Vector3(x-0.5f*a0, y+0.5f*a1, z+0.5f*a2));
		vertices.Add(new Vector3(x+0.5f*a0, y+0.5f*a1, z+0.5f*a2));
		//generate the 8 uv
		//x,z
		uv.Add(new Vector2(0, 0));
		uv.Add(new Vector2(1, 0));
		uv.Add(new Vector2(0, 0));
		uv.Add(new Vector2(1, 0));
		uv.Add(new Vector2(0, 1));
		uv.Add(new Vector2(1, 1));
		uv.Add(new Vector2(0, 1));
		uv.Add(new Vector2(1, 1));
		
		//calculate normals
		//point normals out from center	
		for(int i = 0; i < vertices.Count; i++){
			normals.Add(vertices[i]-center);

		}
		//cube vertices
		//4______6
		//|\     |\
		//| \    | \
		//|  \5__|__7   
		//0___|__2  |
		// \  |  \  |
		//  \ |   \ | 
		//   \1_____3
		// clockwise points out
		//build the triangles
		triangles.Add(0);
		triangles.Add(2);
		triangles.Add(3);

		triangles.Add(0);
		triangles.Add(3);
		triangles.Add(1);

		triangles.Add(0);
		triangles.Add(1);
		triangles.Add(5);

		triangles.Add(0);
		triangles.Add(5);
		triangles.Add(4);
		
		triangles.Add(0);
		triangles.Add(6);
		triangles.Add(2);

		triangles.Add(0);
		triangles.Add(4);
		triangles.Add(6);

		triangles.Add(1);
		triangles.Add(3);
		triangles.Add(7);

		triangles.Add(1);
		triangles.Add(7);
		triangles.Add(5);

		triangles.Add(7);
		triangles.Add(6);
		triangles.Add(4);

		triangles.Add(7);
		triangles.Add(4);
		triangles.Add(5);

		triangles.Add(7);
		triangles.Add(3);
		triangles.Add(2);

		triangles.Add(7);
		triangles.Add(2);
		triangles.Add(6);

		result.vertices = vertices.ToArray();
		result.normals = normals.ToArray();
		result.uv = uv.ToArray();
		result.triangles = triangles.ToArray();

		return result;
	}



}
