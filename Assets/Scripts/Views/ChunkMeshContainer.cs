using UnityEngine;

namespace Views
{
	public class ChunkMeshContainer
	{
		public readonly MeshCollider Collider;
		public readonly MeshRenderer OpaqueRenderer;
		public readonly MeshFilter OpaqueFilter;
		public readonly MeshRenderer TransparentRenderer;
		public readonly MeshFilter TransparentFilter;

		public ChunkMeshContainer(MeshCollider collider,
			MeshRenderer opaqueRenderer,
			MeshFilter opaqueFilter,
			MeshRenderer transparentRenderer,
			MeshFilter transparentFilter)
		{
			Collider = collider;
			Collider.enabled = true;
			OpaqueRenderer = opaqueRenderer;
			OpaqueFilter = opaqueFilter;
			TransparentRenderer = transparentRenderer;
			TransparentFilter = transparentFilter;
		}

		public void SetRenderersEnabled(bool enabled)
		{
			OpaqueRenderer.enabled = enabled;
			TransparentRenderer.enabled = enabled;
		}

		public void SetColliderLayer(int layer)
		{
			if (layer < 0 || Collider == null)
				return;

			Collider.gameObject.layer = layer;
		}
	}
}
