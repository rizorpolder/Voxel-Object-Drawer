using System.Collections.Generic;
using Common.ObjectsPool;
using RuntimeData;
using UnityEngine;

namespace Views
{
	public class VoxelObjectView : MonoBehaviour
	{
		private readonly List<ChunkMeshContainer> _containers = new();

		[SerializeField] private Material[] _opaqueMaterials;
		[SerializeField] private Material[] _transparentMaterials;
		[SerializeField] private Chunk _chunkPrefab;
		private ObjectsPool<Chunk> _chunksPool;

		private bool _isInitialized;


		public List<ChunkMeshContainer> Containers => _containers;
		public Material[] GetOpaqueMaterials() => _opaqueMaterials;
		public Material[] GetTransparent() => _transparentMaterials;


		private void Start()
		{
			_chunksPool = new ObjectsPool<Chunk>(transform, _chunkPrefab);
		}

		public void Initialize(ObjectRuntime runtime)
		{
			SetupViewObject();
		}

		private void SetupViewObject()
		{
			if (_isInitialized)
				return;
			_containers.Clear();

			_isInitialized = true;
		}

		public Chunk GetEmptyChunk()
		{
			var chunk = _chunksPool.GetItem();
			var container = chunk.ChunkMeshContainer;
			_containers.Add(container);
			return chunk;
		}

		public void RemoveChunk(Chunk chunk)
		{
			_containers.Remove(chunk.ChunkMeshContainer);
			chunk.Clear();
			_chunksPool.ReturnToPool(chunk);
		}

		public IEnumerable<Chunk> GetActiveChunks() => _chunksPool.GetActiveItems();
	}
}