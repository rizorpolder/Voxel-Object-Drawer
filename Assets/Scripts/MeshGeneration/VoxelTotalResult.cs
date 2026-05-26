using UnityEngine;

namespace MeshGeneration
{
	public struct VoxelTotalResult
	{
		public VoxelVisualMeshResult Visual; //Opaque and Transparent Mesh
		public Mesh Collider;
	}
}