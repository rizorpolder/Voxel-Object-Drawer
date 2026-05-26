using System.Collections.Generic;

namespace MeshGeneration
{
	public struct VoxelRenderInfo
	{
		public bool IsTransparent;
		public bool HasCollider;
		public int SubmeshIndex;
	}

	public static class VoxelRegistry
	{
		public static readonly Dictionary<byte, VoxelRenderInfo> Info = new()
		{
			{0, new VoxelRenderInfo {IsTransparent = false, HasCollider = true, SubmeshIndex = 0}}, // Default
			{1, new VoxelRenderInfo {IsTransparent = false, HasCollider = true, SubmeshIndex = 1}}, // Metal
			{2, new VoxelRenderInfo {IsTransparent = false, HasCollider = true, SubmeshIndex = 2}}, // Light
			{3, new VoxelRenderInfo {IsTransparent = true, HasCollider = true, SubmeshIndex = 0}}, // Glass
			{4, new VoxelRenderInfo {IsTransparent = true, HasCollider = false, SubmeshIndex = 1}}, // Water
		};
	}
}