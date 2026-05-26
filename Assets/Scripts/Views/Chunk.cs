using CoreData;
using UnityEngine;

namespace Views
{
	public class Chunk : MonoBehaviour
	{
		private bool _isDirty;
		public bool IsDirty => _isDirty;

		[Header("Opaque")]
		[SerializeField] private MeshFilter _opaqueMeshFilter;

		[SerializeField] private MeshRenderer _opaqueMeshRenderer;

		[Header("Transparent")]
		[SerializeField] private MeshFilter _transparentMeshFilter;

		[SerializeField] private MeshRenderer _transparentMeshRenderer;

		[SerializeField] private MeshCollider _meshCollider;


		public VoxelData?[] Grid;
		public int VoxelCount;

		public Chunk Xp, Xm, Yp, Ym, Zp, Zm;

		public Vector3Int ChunkPos;

		private ChunkMeshContainer _chunkMeshContainer;
		public ChunkMeshContainer ChunkMeshContainer => _chunkMeshContainer ??= CreateMeshContainer();

		private ChunkMeshContainer CreateMeshContainer()
		{
			return new ChunkMeshContainer(_meshCollider,
				_opaqueMeshRenderer,
				_opaqueMeshFilter,
				_transparentMeshRenderer,
				_transparentMeshFilter);
		}

		public void SetDirty(bool value)
		{
			_isDirty = value;
		}

		public void Clear()
		{
			_opaqueMeshFilter.mesh = null;
			_transparentMeshFilter.mesh = null;
			_opaqueMeshRenderer.sharedMaterials = System.Array.Empty<Material>();
			_transparentMeshRenderer.sharedMaterials = System.Array.Empty<Material>();
			_meshCollider.sharedMesh = null;

			Grid = null;
			VoxelCount = 0;
			_isDirty = false;

			Xp = Xm = Yp = Ym = Zp = Zm = null;

			ChunkPos = default;
			transform.localPosition = Vector3.zero;
			name = "Chunk_Pooled";
		}
	}
}