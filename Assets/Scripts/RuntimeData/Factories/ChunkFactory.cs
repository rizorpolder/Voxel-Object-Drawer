using System;
using CoreData;
using MeshGeneration;
using MeshGeneration.FaceCulling;
using MeshGeneration.Greedy;
using MeshGeneration.Interfaces;
using MeshGeneration.Mesher;
using UnityEngine;
using Views;

namespace RuntimeData.Factories
{
	public class ChunkFactory
	{
		private VoxelBuildMode _currentBuildMode = VoxelBuildMode.ColliderByType;

		private readonly ICombinedMesher _mesher;

		public ChunkFactory(TMeshAlgorithm aglAlgorithm)
		{
			_mesher = aglAlgorithm switch
			{
				TMeshAlgorithm.FaceCulling => new FaceCullingCombinedMesher(),
				TMeshAlgorithm.GreedyMesh => new GreedyCombinedMesher(),
				_ => new FaceCullingCombinedMesher()
			};
		}

		public void SetBuildMode(VoxelBuildMode mode)
		{
			_currentBuildMode = mode;
		}

		public Chunk CreateChunk(ObjectRuntime rt, Vector3Int pos)
		{
			if (rt.Chunks.TryGetValue(pos, out var existing))
				return existing;

			var chunk = rt.View.GetEmptyChunk();

			chunk.name = $"Chunk_{pos.x}_{pos.y}_{pos.z}";
			chunk.transform.localPosition = new Vector3(
				pos.x * ObjectRuntime.CHUNK_SIZE,
				pos.y * ObjectRuntime.CHUNK_SIZE,
				pos.z * ObjectRuntime.CHUNK_SIZE
			);

			chunk.ChunkPos = pos;

			chunk.Grid = new VoxelData?[ObjectRuntime.CHUNK_SIZE * ObjectRuntime.CHUNK_SIZE * ObjectRuntime.CHUNK_SIZE];
			chunk.VoxelCount = 0;

			rt.Chunks[pos] = chunk;

			LinkNeighbors(rt, chunk);

			return chunk;
		}

		private void LinkNeighbors(ObjectRuntime rt, Chunk chunk)
		{
			var pos = chunk.ChunkPos;

			TryLink(rt, pos + Vector3Int.right, c => chunk.Xp = c, c => c.Xm = chunk);
			TryLink(rt, pos + Vector3Int.left, c => chunk.Xm = c, c => c.Xp = chunk);
			TryLink(rt, pos + Vector3Int.up, c => chunk.Yp = c, c => c.Ym = chunk);
			TryLink(rt, pos + Vector3Int.down, c => chunk.Ym = c, c => c.Yp = chunk);
			TryLink(rt, pos + new Vector3Int(0, 0, 1), c => chunk.Zp = c, c => c.Zm = chunk);
			TryLink(rt, pos + new Vector3Int(0, 0, -1), c => chunk.Zm = c, c => c.Zp = chunk);
		}

		private void TryLink(ObjectRuntime rt, Vector3Int pos, Action<Chunk> setA, Action<Chunk> setB)
		{
			if (rt.Chunks.TryGetValue(pos, out var c))
			{
				setA(c);
				setB(c);
			}
		}

		public void RebuildChunk(ObjectRuntime rt, Chunk chunk)
		{
			if (chunk.VoxelCount == 0)
			{
				DestroyChunk(rt, chunk);
				return;
			}

			var neighbors = new ChunkNeighbors
			{
				Xp = chunk.Xp?.Grid,
				Xm = chunk.Xm?.Grid,
				Yp = chunk.Yp?.Grid,
				Ym = chunk.Ym?.Grid,
				Zp = chunk.Zp?.Grid,
				Zm = chunk.Zm?.Grid
			};

			var result = _mesher.Build(chunk.Grid, neighbors, _currentBuildMode);

			ApplyVisual(chunk, rt, result.Visual);
			
			if(_currentBuildMode != VoxelBuildMode.VisualOnly)
				chunk.ChunkMeshContainer.Collider.sharedMesh = result.Collider;

			chunk.SetDirty(false);
		}

		public void DestroyChunk(ObjectRuntime rt, Chunk chunk)
		{
			rt.Chunks.Remove(chunk.ChunkPos);
			rt.View.RemoveChunk(chunk);
		}

		private void ApplyVisual(Chunk chunk, ObjectRuntime rt, VoxelVisualMeshResult visual)
		{
			var opaqueMaterials = rt.View.GetOpaqueMaterials();
			var transparentMaterials = rt.View.GetTransparent();

			FixSubmeshesCount(visual.OpaqueMesh, opaqueMaterials.Length);
			FixSubmeshesCount(visual.TransparentMesh, transparentMaterials.Length);

			chunk.ChunkMeshContainer.OpaqueFilter.sharedMesh = visual.OpaqueMesh;
			chunk.ChunkMeshContainer.TransparentFilter.sharedMesh = visual.TransparentMesh;

			chunk.ChunkMeshContainer.OpaqueRenderer.sharedMaterials = opaqueMaterials;
			chunk.ChunkMeshContainer.TransparentRenderer.sharedMaterials = transparentMaterials;
		}

		private void FixSubmeshesCount(Mesh mesh, int materials)
		{
			var subs = mesh.subMeshCount;
			if (subs <= 0)
				return;

			for (var i = 0; i < materials; i++)
			{
				if (i < subs)
					continue;
				mesh.subMeshCount++;
			}
		}
	}
}