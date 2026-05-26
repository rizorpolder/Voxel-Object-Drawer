using CoreData;

namespace MeshGeneration.Mesher
{
	public struct ChunkNeighbors
	{
		public VoxelData?[] Xp; // +X
		public VoxelData?[] Xm; // -X
		public VoxelData?[] Yp; // +Y
		public VoxelData?[] Ym; // -Y
		public VoxelData?[] Zp; // +Z
		public VoxelData?[] Zm; // -Z
	}
}