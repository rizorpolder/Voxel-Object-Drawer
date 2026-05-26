using CoreData;
using MeshGeneration.Mesher;

namespace MeshGeneration.Utils
{
	public static class NeighborUtils
	{
		public static VoxelData? Get(int x, int y, int z, VoxelData?[] grid, ChunkNeighbors nb)
		{
			int S = VoxelFaces.S;

			if ((uint) x < S && (uint) y < S && (uint) z < S)
				return grid[(x * S + y) * S + z];

			// X-
			if (x < 0)
			{
				if (nb.Xm != null && (uint) y < S && (uint) z < S)
					return nb.Xm[((S - 1) * S + y) * S + z];

				return null;
			}

			// X+
			if (x >= S)
			{
				if (nb.Xp != null && (uint) y < S && (uint) z < S)
					return nb.Xp[(0 * S + y) * S + z];

				return null; 
			}

			// Y-
			if (y < 0)
			{
				if (nb.Ym != null && (uint) x < S && (uint) z < S)
					return nb.Ym[(x * S + (S - 1)) * S + z];

				return null; 
			}

			// Y+
			if (y >= S)
			{
				if (nb.Yp != null && (uint) x < S && (uint) z < S)
					return nb.Yp[(x * S + 0) * S + z];

				return null; 
			}

			// Z-
			if (z < 0)
			{
				if (nb.Zm != null && (uint) x < S && (uint) y < S)
					return nb.Zm[(x * S + y) * S + (S - 1)];

				return null; 
			}

			// Z+
			if (z >= S)
			{
				if (nb.Zp != null && (uint) x < S && (uint) y < S)
					return nb.Zp[(x * S + y) * S + 0];

				return null;
			}

			return null;
		}
	}
}