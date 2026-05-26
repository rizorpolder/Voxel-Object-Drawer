using System;
using System.Collections.Generic;
using Common.ObjectsPool;
using RuntimeData;
using UnityEngine;
using VContainer;

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
		private bool _originalIsVisible;
		private bool _modeSubscribed;

		[Inject] private ObjectsPoolFactory _poolFactory;

		public List<ChunkMeshContainer> Containers => _containers;
		public Material[] GetOpaqueMaterials() => _opaqueMaterials;
		public Material[] GetTransparent() => _transparentMaterials;
		public Guid InstanceID { get; private set; }
		public Guid TemplateID { get; private set; }

		public void Initialize(ObjectRuntime runtime)
		{
			SetupViewObject();
		}

		private void SetupViewObject()
		{
			if (_isInitialized)
				return;
			_containers.Clear();

			_chunksPool = _poolFactory.CreatePool(_chunkPrefab, transform);
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