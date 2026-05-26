using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshGeneration
{
	public class MeshBuilder
	{
		private readonly List<Vector3> _vertices = new();
		private readonly List<Color32> _colors = new();
		private readonly List<List<int>> _submeshTris = new();
		private readonly List<Vector3> _normals = new();

		private void EnsureSubmesh(int submesh)
		{
			while (_submeshTris.Count <= submesh)
				_submeshTris.Add(new List<int>());
		}

		public void AddQuad(Vector3[] v, Vector3 normal, int submesh, Color32 color)
		{
			EnsureSubmesh(submesh);

			int start = _vertices.Count;

			_vertices.Add(v[0]);
			_vertices.Add(v[1]);
			_vertices.Add(v[2]);
			_vertices.Add(v[3]);

			_normals.Add(normal);
			_normals.Add(normal);
			_normals.Add(normal);
			_normals.Add(normal);

			_colors.Add(color);
			_colors.Add(color);
			_colors.Add(color);
			_colors.Add(color);

			var tris = _submeshTris[submesh];

			tris.Add(start + 0);
			tris.Add(start + 2);
			tris.Add(start + 1);

			tris.Add(start + 2);
			tris.Add(start + 0);
			tris.Add(start + 3);
		}

		public Mesh Build()
		{
			var mesh = new Mesh
			{
				indexFormat = _vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
			};

			mesh.SetVertices(_vertices);
			mesh.SetColors(_colors);
			mesh.SetNormals(_normals);
			mesh.subMeshCount = _submeshTris.Count;

			for (int i = 0; i < _submeshTris.Count; i++)
				mesh.SetTriangles(_submeshTris[i], i);

			mesh.RecalculateBounds();

			return mesh;
		}
	}
}