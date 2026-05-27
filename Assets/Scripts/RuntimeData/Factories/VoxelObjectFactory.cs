using System;
using System.Linq;
using Common.ObjectsPool;
using CoreData;
using MeshGeneration;
using UnityEngine;
using Views;

namespace RuntimeData.Factories
{
	public class VoxelObjectFactory
	{
		private readonly ChunkFactory _chunkFactory;
		private readonly VoxelObjectFactory _objectFactory;
		private ObjectsPool<VoxelObjectView> _pool;

		public VoxelObjectFactory(VoxelObjectFactory objectFactory, TMeshAlgorithm algorithm)
		{
			_objectFactory = objectFactory;
			_chunkFactory = new ChunkFactory(algorithm);
		}


		public void SetBuildMode(VoxelBuildMode mode)
		{
			_chunkFactory.SetBuildMode(mode);
		}

		public ObjectRuntime CreateFromData(ObjectData data)
		{
			if (data == null || _pool == null)
			{
				return null;
			}

			var rt = new ObjectRuntime();

			var view = _pool.GetItem();
			rt.View = view;
			view.Initialize(rt);
			LoadVoxels(rt, data);
			return rt;
		}

		public void RemoveItem(ObjectRuntime rt)
		{
			if (!rt?.View || !rt.View)
				return;

			ClearItem(rt);
			if (rt.View.gameObject.activeSelf)
				_pool.ReturnToPool(rt.View);
		}

		public void ClearItem(ObjectRuntime rt, bool rebuild = true)
		{
			ResetChunks(rt, rebuild);
		}

		private void LoadVoxels(ObjectRuntime rt, ObjectData data)
		{
			foreach (var voxel in data.Voxels)
			{
				VoxelData.UnpackPos(voxel.PackedPos, out int x, out int y, out int z);
				var wp = new Vector3Int(x, y, z);

				rt.Voxels[wp] = voxel;

				var cp = ObjectRuntime.WorldToChunk(x, y, z);
				var lp = ObjectRuntime.WorldToLocal(x, y, z);
				int index = ObjectRuntime.ToIndex(lp.x, lp.y, lp.z);

				CreateChunk(rt, voxel, cp, index);
			}

			RebuildChunks(rt);
		}

		public Chunk CreateChunk(ObjectRuntime rt, VoxelData voxel, Vector3Int cp, int index)
		{
			var chunk = _chunkFactory.CreateChunk(rt, cp);
			if (chunk.Grid[index] == null)
				chunk.VoxelCount++;

			chunk.Grid[index] = voxel;
			chunk.SetDirty(true);
			rt.ChunkMap.Add(cp);
			rt.UpdateQueue.MarkDirty(cp);
			return chunk;
		}

		public void UpdateFromData(ObjectRuntime rt, ObjectData data)
		{
			ResetChunks(rt);
			foreach (var voxel in data.Voxels)
			{
				VoxelData.UnpackPos(voxel.PackedPos, out int x, out int y, out int z);
				var wp = new Vector3Int(x, y, z);

				rt.Voxels[wp] = voxel;

				var cp = ObjectRuntime.WorldToChunk(x, y, z);
				var lp = ObjectRuntime.WorldToLocal(x, y, z);

				var chunk = _chunkFactory.CreateChunk(rt, cp);

				int index = ObjectRuntime.ToIndex(lp.x, lp.y, lp.z);

				if (chunk.Grid[index] == null)
					chunk.VoxelCount++;

				chunk.Grid[index] = voxel;
				chunk.SetDirty(true);

				rt.ChunkMap.Add(cp);
				rt.UpdateQueue.MarkDirty(cp);
			}

			rt.View.gameObject.name = data.Name;
			RebuildChunks(rt);
		}

		private void ResetChunks(ObjectRuntime rt, bool rebuild = true)
		{
			foreach (var chunk in rt.Chunks.Values)
			{
				for (int i = 0; i < chunk.Grid.Length; i++)
					chunk.Grid[i] = null;

				chunk.VoxelCount = 0;
				chunk.SetDirty(true);
			}

			rt.Voxels.Clear();
			rt.ChunkMap.Clear();

			if (rebuild) DrainRebuildChunks(rt);
		}

		private void DrainRebuildChunks(ObjectRuntime rt)
		{
			foreach (var pos in rt.Chunks.Keys.ToList())
			{
				rt.UpdateQueue.MarkDirty(pos);
			}

			while (rt.UpdateQueue.Count > 0)
			{
				RebuildChunks(rt, int.MaxValue);
			}
		}

		public void RebuildChunks(ObjectRuntime rt, int maxPerFrame = 100)
		{
			foreach (var kv in rt.Chunks)
			{
				var pos = kv.Key;
				var chunk = kv.Value;

				if (chunk.IsDirty)
					rt.UpdateQueue.MarkDirty(pos);
			}

			int processed = 0;

			while (processed < maxPerFrame && rt.UpdateQueue.TryDequeue(out var pos))
			{
				if (!rt.Chunks.TryGetValue(pos, out var chunk))
					continue;

				if (chunk.VoxelCount == 0)
				{
					_chunkFactory.DestroyChunk(rt, chunk);
					processed++;
					continue;
				}

				if (chunk.IsDirty)
				{
					_chunkFactory.RebuildChunk(rt, chunk);
					chunk.SetDirty(false);
				}

				processed++;
			}
		}

		public void RebuildFromVoxels(ObjectRuntime rt)
		{
			DestroyAllChunks(rt);

			foreach (var kv in rt.Voxels)
			{
				var voxel = kv.Value;
				VoxelData.UnpackPos(voxel.PackedPos, out int x, out int y, out int z);

				var cp = ObjectRuntime.WorldToChunk(x, y, z);
				var lp = ObjectRuntime.WorldToLocal(x, y, z);
				int index = ObjectRuntime.ToIndex(lp.x, lp.y, lp.z);

				var chunk = CreateChunk(rt, voxel, cp, index);
				rt.ChunkMap.Add(cp);
				rt.UpdateQueue.MarkDirty(cp);
			}

			RebuildChunks(rt);
		}

		private void DestroyAllChunks(ObjectRuntime rt)
		{
			for (int i = 0; i < rt.Chunks.Count; i++)
			{
				var entry = rt.Chunks.ElementAt(i);
				i--;
				_chunkFactory.DestroyChunk(rt, entry.Value);
			}

			rt.Chunks.Clear();
			rt.ChunkMap.Clear();
			rt.UpdateQueue.Clear();
		}

		public ObjectRuntime CloneRuntime(ObjectRuntime rt)
		{
			var cloneView = _pool.GetItem();
			cloneView.Initialize(rt);
			var clone = new ObjectRuntime()
			{
				View = cloneView,
			};
			CopyRuntimeData(rt, clone);
			RebuildChunks(clone);
			return clone;
		}

		public void CopyRuntimeData(ObjectRuntime from, ObjectRuntime to)
		{
			foreach (var kv in from.Voxels)
			{
				to.Voxels[kv.Key] = kv.Value;
			}

			foreach (var kv in from.Chunks)
			{
				var chunkPos = kv.Key;
				var srcChunk = kv.Value;
				var chunkClone = _chunkFactory.CreateChunk(to, chunkPos);

				var grid = new VoxelData?[srcChunk.Grid.Length];
				Array.Copy(srcChunk.Grid, grid, srcChunk.Grid.Length);
				chunkClone.Grid = grid;
				to.Chunks[chunkPos] = chunkClone;
				chunkClone.VoxelCount = kv.Value.VoxelCount;
				chunkClone.SetDirty(true);
				to.UpdateQueue.MarkDirty(chunkPos);
			}

			foreach (var pos in from.ChunkMap)
				to.ChunkMap.Add(pos);
		}
	}
}