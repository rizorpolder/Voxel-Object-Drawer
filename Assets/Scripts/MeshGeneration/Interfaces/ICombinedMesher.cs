using CoreData;
using MeshGeneration.Mesher;
using RuntimeData.Factories;

namespace MeshGeneration.Interfaces
{
	public interface ICombinedMesher
	{
		public VoxelTotalResult Build(VoxelData?[] grid, ChunkNeighbors nb, VoxelBuildMode buildMode);
	}
}