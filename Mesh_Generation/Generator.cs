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
	public List<MeshNode> edges;
	public int vertexNumber;
	public bool visited;

	public MeshNode (Vector3 position, Vector3 normal, Vector2 uv){
		this.position = position;
		this.normal = normal;
		this.normal.Normalize();
		this.uv = uv;
		vertexNumber = -1;
		edges = new List<MeshNode>();
		visited = false;
	}

	public void AddEdge(MeshNode node){
		edges.Add(node);
		node.edges.Add(this);
	}
	
	public void RemoveEdge(MeshNode node){
		edges.Remove(node);
		node.edges.Remove(this);
	}

	// Add a point on the edge between this and n0
	public Vector3 GetEdgeCenter(MeshNode node){
		Vector3 difference = new Vector3((position.x + node.position.x)/2.0f,
				(position.y + node.position.y)/2.0f,
				(position.z + node.position.z)/2.0f);

		return difference;
	}

	// Add a point on the edge between this and n0
	public MeshNode SplitEdge(MeshNode n0, Vector3 position, Vector3 normal){
		MeshNode insert = new MeshNode(position, normal, 
				new Vector2((uv.x + n0.uv.x)/2.0f,(uv.y + n0.uv.y)/2.0f));

		//add insert in the middle
		insert.AddEdge(this);
		insert.AddEdge(n0);
		this.RemoveEdge(n0);

		//TODO check this
		//set insert as visited initially
		insert.visited = false;

		//now add shared edges to create new tris
		for(int i = 0; i < n0.edges.Count ; i++){
			if(this.edges.Contains(n0.edges[i]) && n0.edges[i] != insert){
				insert.AddEdge(n0.edges[i]);
			}
		}
		return insert;
	}
	
	public bool IsHypotenuse(MeshNode node){
		float dist = Mathf.Abs(Vector3.Distance(this.position, node.position));
		for(int i = 0; i < edges.Count ; i++){
			if(node.edges.Contains(this.edges[i])){
				//if any triangle edges this touches are larger than this edge
				//it is not the hypot
				if(Mathf.Abs(Vector3.Distance(this.edges[i].position, this.position)) > dist){
					return false; 
				} 
				if(Mathf.Abs(Vector3.Distance(this.edges[i].position, node.position)) > dist){
					return false; 
				} 
			}
		}	
		return true;
	}

	//TODO removed reset visited by using a dynamic visited state
	public Mesh ToMesh(){
		ResetVisited();
		
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> triangles = new List<int>();
		Mesh result = new Mesh();
		int number = 0;
		GetPoints(vertices, normals, uvs, ref number);
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
		
		Debug.Log("Tris: " + triangles.Count/3.0);
		Debug.Log("Verts: " + vertices.Count);

		return result;
	}
	
	//assumes all nodes are currently in the visited state
	//recursively resets all nodes to not visited
	public void ResetVisited(){
		visited = false;
		for(int i = 0; i < edges.Count; i++){
			if(edges[i].visited != false){
				edges[i].ResetVisited();
			}

		}

	}
	//assumes all nodes are currently in the not visited state
	//recursively resets all nodes to visited
	public void SetVisited(){
		visited = true;
		for(int i = 0; i < edges.Count; i++){
			if(edges[i].visited != true){
				edges[i].SetVisited();
			}

		}

	}

	private void GetTris(List<int> triangles){
		visited = true;
		//linear search because its unsorted
		//for each 2 edges of this
		//look for common edges 
		for(int i = 0; i < edges.Count ; i++){
			if(edges[i].visited == false){	
				for(int j = i+1; j < edges.Count ; j++){
					if(edges[j].visited == false){
						//TODO check for counter clockwise ness
						if(edges[i].edges.Contains(edges[j])){
							Vector3 dir = Vector3.Cross(edges[i].position - position,
									edges[j].position - position);
							Vector3 norm = Vector3.Normalize(dir);
							Vector3 norm2 = Vector3.Normalize(normal);
							//if vectors point the same direction then this is CCW, 
							//otherwise switch
							if(Vector3.Dot(norm2, norm)>0){

								triangles.Add(vertexNumber);
								triangles.Add(edges[i].vertexNumber);
								triangles.Add(edges[j].vertexNumber);
							} else {
								triangles.Add(vertexNumber);
								triangles.Add(edges[j].vertexNumber);
								triangles.Add(edges[i].vertexNumber);
							}
						}

					}
				
				}
			} 

		}
		//recurse through
		for(int i = 0; i < edges.Count; i++){
			if(edges[i].visited == false){
				edges[i].GetTris(triangles);
			}

		}

	}


	private void GetPoints(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, ref int number){
		visited = true;
		vertexNumber = number;
		vertices.Add(position);
		normals.Add(normal);
		uvs.Add(uv);
		number += 1;
		for(int i = 0; i < edges.Count; i++){
			if(edges[i].visited == false){
				edges[i].GetPoints(vertices, normals, uvs, ref number);
			} 

		}
	}
}

public class MeshGenerator
{

	public enum Axis {X_AXIS, Y_AXIS, Z_AXIS};
	
	public static MeshNode generate_axis_alligned_cone(float x, float y, float z, 
			float r, float length, Axis axis, int generations){
		Vector3 center = new Vector3(x,y,z);
		MeshNode c1, c2;
		Vector3 v2; 
		switch (axis){
			case Axis.X_AXIS:
					 c1 = generate_circle(x+0.5f*length, y, z, 
							r, axis, center, generations, 1);
					 v2 = new Vector3(x-0.5f*length, y, z);
				break;
			case Axis.Y_AXIS:
					 c1 = generate_circle(x, y+0.5f*length, z, 
							r, axis, center, generations, 1);
					 v2 = new Vector3(x, y-0.5f*length, z);
				break;
			case Axis.Z_AXIS:
					 c1 = generate_circle(x, y, z+0.5f*length, 
							r, axis, center, generations, 1);
					 v2 = new Vector3(x, y, z-0.5f*length);
				break;
			default:
				c1=null;
				v2= new Vector3(0,0,0);
				break;
		}
		
		c2 = new MeshNode(v2, v2 - center, new Vector2(0,0));
		
		//ignore head
		c1.visited = true;
		c2.visited = true;
		cone_recurse(c1.edges[0], c2);
		c1.ResetVisited();


		return c1;
	}
	private static void cone_recurse(MeshNode n0, MeshNode n1){
		List<MeshNode> edgesCopy = new List<MeshNode>(n0.edges);
		n0.visited = true;
		n0.AddEdge(n1);
		for(int i = 0 ; i < edgesCopy.Count; i++){
			if(edgesCopy[i].visited == false){
				cone_recurse(edgesCopy[i],n1);
			}
		}
	}

	public static MeshNode generate_axis_alligned_cylinder(float x, float y, float z, 
			float r, float length, Axis axis, int generations){
		Vector3 center = new Vector3(x,y,z);
		MeshNode c1, c2;
		switch (axis){
			case Axis.X_AXIS:
					 c1 = generate_circle(x+0.5f*length, y, z, 
							r, axis, center, generations, 1);
					 c2 = generate_circle(x-0.5f*length, y, z, 
							r, axis, center, generations, -1);
				break;
			case Axis.Y_AXIS:
					 c1 = generate_circle(x, y+0.5f*length, z, 
							r, axis, center, generations, 1);
					 c2 = generate_circle(x, y-0.5f*length, z, 
							r, axis, center, generations, -1);
				break;
			case Axis.Z_AXIS:
					 c1 = generate_circle(x, y, z+0.5f*length, 
							r, axis, center, generations, 1);
					 c2 = generate_circle(x, y, z-0.5f*length, 
							r, axis, center, generations, -1);
				break;
			default:
				c1=null;
				c2=null;
				break;
		}
		
		//ignore head
		c1.visited = true;
		c2.visited = true;
		//assumed circles are identical
		cylinder_recurse(c1.edges[0], c2.edges[0], c1.edges[0]);
		c1.ResetVisited();
		c2.ResetVisited();


		return c1;
	}
	//TODO write this function to draw the cross values
	private static void cylinder_recurse(MeshNode n0, MeshNode n1, MeshNode back){
		List<MeshNode> edgesCopy = new List<MeshNode>(n0.edges);
		List<MeshNode> edgesCopy2 = new List<MeshNode>(n1.edges);
		bool last_node = true;
		n0.visited = true;
		n1.visited = true;
		n0.AddEdge(n1);
		for(int i = 0 ; i < edgesCopy.Count; i++){
			if(edgesCopy[i].visited == false){
				edgesCopy[i].AddEdge(n1);
				cylinder_recurse(edgesCopy[i],edgesCopy2[i], back);
				last_node = false;
			}
		}
		if(last_node == true){
			n1.AddEdge(back);	
			
		}
	}

	public static MeshNode generate_circle(float x, float y, float z, float r, Axis axis, Vector3 center, int generations, float uvMult){
		//generate square
		MeshNode head; 
		float length = 2*r / Mathf.Sqrt(2.0f);
		head = generate_square(x, y, z, length, axis, center, uvMult); 
		for(int i = 0; i < generations; i++){
			//ignore center item
			head.visited = true;
		 	circle_recurse(center, head.position, r, head.edges[0]);
			head.ResetVisited();
		}
		return head;
	}

	private static void circle_recurse(Vector3 center, Vector3 circleCenter, float radius, MeshNode node){
		List<MeshNode> edgesCopy = new List<MeshNode>(node.edges);
		node.visited = true;
		for(int i = 0 ; i < edgesCopy.Count; i++){
			if(edgesCopy[i].visited == false){
				Vector3 position = node.GetEdgeCenter(edgesCopy[i]);
				position = circleCenter + (position-circleCenter)*
					Mathf.Abs(radius/((position-circleCenter).magnitude));
				node.SplitEdge(edgesCopy[i], position, position-center).visited = true;
			}
		}
		
		for(int i = 0 ; i < edgesCopy.Count; i++){
			if(edgesCopy[i].visited == false){
				circle_recurse(center, circleCenter, radius, edgesCopy[i]);
			}
		}
	}
	
	public static MeshNode generate_arbitrary_quad(Vector3 v0, Vector3 v1, 
			Vector3 v2, Vector3 v3, Vector3 center, float uvMult){
		
		MeshNode n0 = new MeshNode(v0, v0 - center, new Vector2(1.0f*uvMult,1.0f*uvMult));
		MeshNode n1 = new MeshNode(v1, v1 - center, new Vector2(2.0f*uvMult,1.0f*uvMult));
		MeshNode n2 = new MeshNode(v2, v2 - center, new Vector2(1.0f*uvMult,2.0f*uvMult));
		MeshNode n3 = new MeshNode(v3, v3 - center, new Vector2(2.0f*uvMult,2.0f*uvMult));
		
		//connect square
		//0_____1
		//|\    |
		//| \   |
		//|  \  |
		//|   \ |
		//|    \|
		//2_____3
		//

		//border edges
		n0.AddEdge(n1);
		n0.AddEdge(n2);
		
		n1.AddEdge(n3);
		n2.AddEdge(n3);

		//cross edge
		n0.AddEdge(n3);

		//return v0 as center point
		return n0;
	}
	
	public static MeshNode generate_square(float x, float y, float z, float width, Axis axis, Vector3 center, float uvMult){
		Vector3 v0, v1, v2, v3;
		//center point
		Vector3 v4 = new Vector3(x,y,z);
		
		switch (axis){
			case Axis.X_AXIS:
				v0 = new Vector3(x, y - 0.5f*width, z - 0.5f*width);
				v1 = new Vector3(x, y + 0.5f*width, z - 0.5f*width);
				v2 = new Vector3(x, y - 0.5f*width, z + 0.5f*width);
				v3 = new Vector3(x, y + 0.5f*width, z + 0.5f*width);
				break;
			case Axis.Y_AXIS:
				v0 = new Vector3(x - 0.5f*width, y, z - 0.5f*width);
				v1 = new Vector3(x + 0.5f*width, y, z - 0.5f*width);
				v2 = new Vector3(x - 0.5f*width, y, z + 0.5f*width);
				v3 = new Vector3(x + 0.5f*width, y, z + 0.5f*width);
				break;

			case Axis.Z_AXIS:
				v0 = new Vector3(x - 0.5f*width, y - 0.5f*width, z);
				v1 = new Vector3(x + 0.5f*width, y - 0.5f*width, z);
				v2 = new Vector3(x - 0.5f*width, y + 0.5f*width, z);
				v3 = new Vector3(x + 0.5f*width, y + 0.5f*width, z);
				break;
			default:
				v0 = new Vector3(0,0,0);
				v1 = new Vector3(0,0,0);
				v2 = new Vector3(0,0,0);
				v3 = new Vector3(0,0,0);

				break;
		}
		MeshNode n0 = new MeshNode(v0, v0 - center, new Vector2(1.0f*uvMult,1.0f*uvMult));
		MeshNode n1 = new MeshNode(v1, v1 - center, new Vector2(2.0f*uvMult,1.0f*uvMult));
		MeshNode n2 = new MeshNode(v2, v2 - center, new Vector2(1.0f*uvMult,2.0f*uvMult));
		MeshNode n3 = new MeshNode(v3, v3 - center, new Vector2(2.0f*uvMult,2.0f*uvMult));
		MeshNode n4 = new MeshNode(v4, v4 - center, new Vector2(1.5f*uvMult,1.5f*uvMult));
		
		//connect square
		//0_____1
		//|\   /|
		//| \ / |
		//|  4  |
		//| / \ |
		//|/   \|
		//2_____3
		//

		//border edges
		n0.AddEdge(n1);
		n0.AddEdge(n2);
		
		n1.AddEdge(n3);
		n2.AddEdge(n3);

		//cross edges
		n4.AddEdge(n0);
		n4.AddEdge(n1);
		n4.AddEdge(n2);
		n4.AddEdge(n3);

		//return head as center point
		return n4;
	}

	public static MeshNode generate_sphere(float x, float y, float z, float radius, int generations){
		
		//cube length
		float length = 2*radius / Mathf.Sqrt(3.0f);
		Vector3 center = new Vector3(x,y,z);
		//first generate cube;
		MeshNode head = generate_cube(x, y, z, length, length, length);
		
		for(int i = 0; i < generations; i++){
			Queue<KeyValuePair<MeshNode, MeshNode>> splitQueue = new Queue<KeyValuePair<MeshNode, MeshNode>>();
			sphere_recurse(center, radius, head, splitQueue);
			while(splitQueue.Count > 0){
				KeyValuePair<MeshNode, MeshNode> n = splitQueue.Dequeue();
				Vector3 position = n.Key.GetEdgeCenter(n.Value);
				position = center + (position-center)*
					Mathf.Abs(radius/((position-center).magnitude));
				n.Key.SplitEdge(n.Value, position, position-center);
			}
			head.ResetVisited();
		}

		return head;
	}

	private static void sphere_recurse(Vector3 center, float radius, MeshNode node, Queue<KeyValuePair<MeshNode, MeshNode>> splitQueue){
		List<MeshNode> edgesCopy = new List<MeshNode>(node.edges);
		node.visited = true;
		for(int i = 0 ; i < edgesCopy.Count; i++){
			if(edgesCopy[i].visited == false && node.IsHypotenuse(edgesCopy[i])){
				splitQueue.Enqueue(new KeyValuePair<MeshNode, MeshNode>(node, edgesCopy[i]));
			}
		}
		
		for(int i = 0 ; i < edgesCopy.Count; i++){
			if(edgesCopy[i].visited == false){
				sphere_recurse(center, radius, edgesCopy[i], splitQueue);
			}
		}
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
		  		new Vector2(0,1));
		
		MeshNode n5 = new MeshNode(
				new Vector3(x+0.5f*a0, y-0.5f*a1, z+0.5f*a2), 
				new Vector3(x+0.5f*a0 - center.x, y-0.5f*a1 - center.y, z+0.5f*a2 - center.z),
		  		new Vector2(1,1));
		
		MeshNode n6 = new MeshNode(
				new Vector3(x-0.5f*a0, y+0.5f*a1, z+0.5f*a2), 
				new Vector3(x-0.5f*a0 - center.x, y+0.5f*a1 - center.y, z+0.5f*a2 - center.z),
		  		new Vector2(0,0));
	
		MeshNode n7 = new MeshNode(
				new Vector3(x+0.5f*a0, y+0.5f*a1, z+0.5f*a2), 
				new Vector3(x+0.5f*a0 - center.x, y+0.5f*a1 - center.y, z+0.5f*a2 - center.z),
				new Vector2(1,0));
		//cube vertices
		//4______6
		//|\     |\
		//| \    | \
		//|  \5__|__7   
		//0___|__2  |
		// \  |  \  |
		//  \ |   \ | 
		//   \1_____3
		
		//create edges all basic triangles <3
		//direct
		n0.AddEdge(n1);
		n0.AddEdge(n2);
		n0.AddEdge(n4);

		n1.AddEdge(n3);
		n1.AddEdge(n5);
	
		n2.AddEdge(n3);
		n2.AddEdge(n6);
		
		n3.AddEdge(n7);
	
		n4.AddEdge(n5);
		n4.AddEdge(n6);
		
		n5.AddEdge(n7);
		n6.AddEdge(n7);

		//diags
		n0.AddEdge(n6);
		n1.AddEdge(n4);
		n2.AddEdge(n7);
		n3.AddEdge(n5);
		
		
		n5.AddEdge(n6);
		
		n1.AddEdge(n2);

		return n0;
	}
}
