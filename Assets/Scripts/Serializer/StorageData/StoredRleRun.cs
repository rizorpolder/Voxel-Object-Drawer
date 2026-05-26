using System.Collections.Generic;
using CoreData;

namespace Serializer.StorageData
{
	public struct StoredRleRun
	{
		public uint StartMorton;
		public byte Length;
		public byte ColorId;

		public static VoxelData[] ExpandRleToVoxels(StoredObjectData stored)
		{
			var runs = stored.RleRuns;
			var palette = stored.Palette;

			var voxels = new List<VoxelData>();

			foreach (var run in runs)
			{
				var pal = palette[run.ColorId];
				var color = StoredPaletteEntry.FromRGB565(pal.RGB565);
				var type = pal.Type;

				for (uint i = 0; i < run.Length; i++)
				{
					uint morton = run.StartMorton + i;
					Decode8(morton, out int nx, out int ny, out int nz);

					int localX = nx - 128;
					int localY = ny;
					int localZ = nz - 128;

					int x = localX + stored.OriginX;
					int y = localY + stored.OriginY;
					int z = localZ + stored.OriginZ;

					voxels.Add(new VoxelData
					{
						Type = type,
						Color = color,
						PackedPos = VoxelData.PackPos(x, y, z)
					});
				}
			}

			return voxels.ToArray();
		}

		public static uint Encode8(int x, int y, int z)
		{
			uint xx = (uint) x & 0xFF;
			uint yy = (uint) y & 0xFF;
			uint zz = (uint) z & 0xFF;

			xx = (xx | (xx << 8)) & 0x00F00F;
			xx = (xx | (xx << 4)) & 0x0C30C3;
			xx = (xx | (xx << 2)) & 0x249249;

			yy = (yy | (yy << 8)) & 0x00F00F;
			yy = (yy | (yy << 4)) & 0x0C30C3;
			yy = (yy | (yy << 2)) & 0x249249;

			zz = (zz | (zz << 8)) & 0x00F00F;
			zz = (zz | (zz << 4)) & 0x0C30C3;
			zz = (zz | (zz << 2)) & 0x249249;

			return xx | (yy << 1) | (zz << 2);
		}

		private static void Decode8(uint morton, out int x, out int y, out int z)
		{
			uint xx = morton;
			uint yy = morton >> 1;
			uint zz = morton >> 2;

			xx &= 0x249249;
			yy &= 0x249249;
			zz &= 0x249249;

			xx = (xx ^ (xx >> 2)) & 0x0C30C3;
			xx = (xx ^ (xx >> 4)) & 0x00F00F;
			xx = (xx ^ (xx >> 8)) & 0x0000FF;

			yy = (yy ^ (yy >> 2)) & 0x0C30C3;
			yy = (yy ^ (yy >> 4)) & 0x00F00F;
			yy = (yy ^ (yy >> 8)) & 0x0000FF;

			zz = (zz ^ (zz >> 2)) & 0x0C30C3;
			zz = (zz ^ (zz >> 4)) & 0x00F00F;
			zz = (zz ^ (zz >> 8)) & 0x0000FF;

			x = (int) xx;
			y = (int) yy;
			z = (int) zz;
		}
	}
}