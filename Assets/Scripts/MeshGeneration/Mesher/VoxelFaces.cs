using RuntimeData;
using UnityEngine;

namespace MeshGeneration.Mesher
{
	public static class VoxelFaces
	{
		public const int S = ObjectRuntime.CHUNK_SIZE;

		public static readonly int[] DX = {1, -1, 0, 0, 0, 0};
		public static readonly int[] DY = {0, 0, 1, -1, 0, 0};
		public static readonly int[] DZ = {0, 0, 0, 0, 1, -1};

		public static readonly Vector3[] NORMALS =
		{
			new(1, 0, 0), // +X
			new(-1, 0, 0), // -X
			new(0, 1, 0), // +Y
			new(0, -1, 0), // -Y
			new(0, 0, 1), // +Z
			new(0, 0, -1) // -Z
		};

		public static readonly Vector3[][] FACE_VERTS =
		{
			// +X
			new[]
			{
				new Vector3(1, 0, 0), new Vector3(1, 0, 1),
				new Vector3(1, 1, 1), new Vector3(1, 1, 0)
			},
			// -X
			new[]
			{
				new Vector3(0, 0, 0), new Vector3(0, 1, 0),
				new Vector3(0, 1, 1), new Vector3(0, 0, 1)
			},
			// +Y
			new[]
			{
				new Vector3(0, 1, 0), new Vector3(1, 1, 0),
				new Vector3(1, 1, 1), new Vector3(0, 1, 1)
			},
			// -Y
			new[]
			{
				new Vector3(0, 0, 1), new Vector3(1, 0, 1),
				new Vector3(1, 0, 0), new Vector3(0, 0, 0)
			},
			// +Z
			new[]
			{
				new Vector3(0, 0, 1), new Vector3(0, 1, 1),
				new Vector3(1, 1, 1), new Vector3(1, 0, 1)
			},
			// -Z
			new[]
			{
				new Vector3(1, 0, 0), new Vector3(1, 1, 0),
				new Vector3(0, 1, 0), new Vector3(0, 0, 0)
			}
		};
	}
}