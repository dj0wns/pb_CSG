// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/
// 
// C++ port by Tomasz Dabrowski (http://28byteslater.com), under the MIT license.
// GitHub: https://github.com/dabroz/csgjs-cpp/
//
// C# port by Karl Henkel (parabox.co), under MIT license.
//  
// Constructive Solid Geometry (CSG) is a modeling technique that uses Boolean
// operations like union and intersection to combine 3D solids. This library
// implements CSG operations on meshes elegantly and concisely using BSP trees,
// and is meant to serve as an easily understandable implementation of the
// algorithm. All edge cases involving overlapping coplanar polygons in both
// solids are correctly handled.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Parabox.CSG
{
	/**
	 * Base class for CSG operations.  Contains GameObject level methods for Subtraction, Intersection, and Union operations.
	 * The GameObjects passed to these functions will not be modified.
	 */
	public enum CSG_Operation {
		no_op,
		Inner,
		Outer,
		On,
		Compliment,
		Union,
		Intersect,
		Subtract,
	};

	public class CSG_Tree
	{
		public CSG_Tree left;
		public CSG_Tree right;
		private CSG_Operation operation;
		private CSG_Node current_object;
		private Mesh m;	

		//for leaf nodes
		public CSG_Tree(GameObject obj){
			operation = CSG_Operation.no_op;
			CSG_Model csg_model_a = new CSG_Model(obj);
			current_object = new CSG_Node( csg_model_a.ToPolygons());
			left = null;
			right = null;
		}
		
		public CSG_Tree(Mesh m){
			operation = CSG_Operation.no_op;
			this.m = m;
			left = null;
			right = null;
		}

		public CSG_Tree(CSG_Tree lhs, CSG_Tree rhs, CSG_Operation op){
			left = lhs;
			right = rhs;
			operation = op;
			current_object = null;

		}

		public Mesh getMesh(){
			CSG_Model result = new CSG_Model(current_object.AllPolygons());
			return result.ToMesh();
		}
		public void render(){
			current_object = render_tree();
		}
		
		public void clean(){
			this.current_object = null;
			if(left != null){
				left.clean();
			}
			if(right != null){
				right.clean();
			}
		}
		
		public void remove_references(){
			this.current_object = null;
			if(left != null){
				left.remove_references();
				left = null;
			}	
			if(right != null){
				right.remove_references();
				right = null;
			}
		}
		


		public void print(){
			int[] i = new int[] {0};
			print(i);
		}


		public void print(int[] i){
			if(operation == CSG_Operation.no_op){
				Debug.Log(i[0] + ": " + m.vertexCount);

			} else {

				Debug.Log(i[0] + ": " + operation);

			}
			if(left != null){
				i[0]++;
				left.print(i);
			}
			if(right != null){
				i[0]++;
				right.print(i);
			}
			
		}
	

		internal CSG_Node render_tree(){
			if(operation == CSG_Operation.no_op){	
				CSG_Model csg_model_a = new CSG_Model(m);
				current_object = new CSG_Node( csg_model_a.ToPolygons());
				return current_object;
			} else {
				switch (operation){
					case CSG_Operation.Inner:
						current_object = CSG_Node.Inner(left.render_tree());
						return current_object;
					
					case CSG_Operation.Outer:
						current_object = CSG_Node.Outer(left.render_tree());
						return current_object;
					
					case CSG_Operation.On:
						current_object = CSG_Node.On(left.render_tree());
						return current_object;
					
					case CSG_Operation.Compliment:
						current_object = CSG_Node.Compliment(left.render_tree());
						return current_object;
					
					case CSG_Operation.Union:
						current_object = CSG_Node.Union(left.render_tree(), 
									right.render_tree());
						return current_object;
					
					case CSG_Operation.Intersect:
						current_object = CSG_Node.Intersect(left.render_tree(), 
									right.render_tree());
						return current_object;
					
					case CSG_Operation.Subtract:
						current_object = CSG_Node.Subtract(left.render_tree(), 
									right.render_tree());
						return current_object;
				}
		
			}
			return null;
			
		} 


	}


	public class CSG
	{
		public const float EPSILON = 0.00001f; /// Tolerance used by `splitPolygon()` to decide if a point is on the plane.

		/**
		 * Returns a new mesh by merging @lhs with @rhs.
		 */
		public static Mesh Union(GameObject lhs, GameObject rhs)
		{
			CSG_Model csg_model_a = new CSG_Model(lhs);
			CSG_Model csg_model_b = new CSG_Model(rhs);

			CSG_Node a = new CSG_Node( csg_model_a.ToPolygons() );
			CSG_Node b = new CSG_Node( csg_model_b.ToPolygons() );

			List<CSG_Polygon> polygons = CSG_Node.Union(a, b).AllPolygons();

			CSG_Model result = new CSG_Model(polygons);

			return result.ToMesh();
		}

		/**
		 * Returns a new mesh by subtracting @rhs from @lhs.
		 */
		public static Mesh Subtract(GameObject lhs, GameObject rhs)
		{
			CSG_Model csg_model_a = new CSG_Model(lhs);
			CSG_Model csg_model_b = new CSG_Model(rhs);

			CSG_Node a = new CSG_Node( csg_model_a.ToPolygons() );
			CSG_Node b = new CSG_Node( csg_model_b.ToPolygons() );

			List<CSG_Polygon> polygons = CSG_Node.Subtract(a, b).AllPolygons();

			CSG_Model result = new CSG_Model(polygons);

			return result.ToMesh();
		}

		/**
		 * Return a new mesh by intersecting @lhs with @rhs.  This operation
		 * is non-commutative, so set @lhs and @rhs accordingly.
		 */
		public static Mesh Intersect(GameObject lhs, GameObject rhs)
		{
			CSG_Model csg_model_a = new CSG_Model(lhs);
			CSG_Model csg_model_b = new CSG_Model(rhs);

			CSG_Node a = new CSG_Node( csg_model_a.ToPolygons() );
			CSG_Node b = new CSG_Node( csg_model_b.ToPolygons() );

			List<CSG_Polygon> polygons = CSG_Node.Intersect(a, b).AllPolygons();

			CSG_Model result = new CSG_Model(polygons);

			return result.ToMesh();
		}
	}
}
