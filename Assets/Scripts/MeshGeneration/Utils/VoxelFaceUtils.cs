using CoreData;

namespace MeshGeneration.Utils
{
	public static class VoxelFaceUtils
	{
		public static bool ShouldShowFace(VoxelData? cur, VoxelData? neigh)
		{
			if (!cur.HasValue)
				return false;

			var currT = VoxelRegistry.Info[cur.Value.Type];
			bool curC = currT.HasCollider;
			bool currTransp = currT.IsTransparent; 
			
			if (!neigh.HasValue)
				return true;

			var neighT = VoxelRegistry.Info[neigh.Value.Type];
			bool neighC = neighT.HasCollider;
			bool neighTransp = neighT.IsTransparent; 
			
			
			return currTransp != neighTransp || curC != neighC;
		}
	}
}