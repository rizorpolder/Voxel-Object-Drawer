using System.Collections.Generic;
using CoreData;
using UnityEngine;
using Views;

namespace RuntimeData
{
	public class ObjectRuntime
	{
		public const int CHUNK_SIZE = 16;

		public Dictionary<Vector3Int, VoxelData> Voxels = new();

		public Dictionary<Vector3Int, Chunk> Chunks = new();

		public HashSet<Vector3Int> ChunkMap = new();

		public VoxelObjectView View;

		public readonly ChunkUpdateQueue UpdateQueue = new();

		public static Vector3Int WorldToChunk(int x, int y, int z)
		{
			int s = CHUNK_SIZE;

			int cx = Mathf.FloorToInt(x / (float) s);
			int cy = Mathf.FloorToInt(y / (float) s);
			int cz = Mathf.FloorToInt(z / (float) s);

			return new Vector3Int(cx, cy, cz);
		}

		public static Vector3Int WorldToLocal(int x, int y, int z)
		{
			int s = CHUNK_SIZE;

			return new Vector3Int(
				x - Mathf.FloorToInt(x / (float) s) * s,
				y - Mathf.FloorToInt(y / (float) s) * s,
				z - Mathf.FloorToInt(z / (float) s) * s
			);
		}

		public static int ToIndex(int x, int y, int z)
		{
			return (x * CHUNK_SIZE + y) * CHUNK_SIZE + z;
		}

		public static Vector3 DecodePivotCenter(sbyte pivotX, byte pivotY, sbyte pivotZ) =>
			new Vector3(pivotX * 0.5f, pivotY * 0.5f, pivotZ * 0.5f);
	}
}