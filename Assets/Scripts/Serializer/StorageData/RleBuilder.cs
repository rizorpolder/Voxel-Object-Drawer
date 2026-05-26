using System;
using System.Collections.Generic;

namespace Serializer.StorageData
{
	public static class RleBuilder
	{
		private enum RleMode
		{
			Flat2D,
			Line1D,
			Morton3D
		}

		public static StoredRleRun[] BuildHybridRle(byte[] colorIds, int w, int h, int d)
		{
			return BuildRleMorton(colorIds, w, h, d);
			// var mode = DetectRleMode(w, h, d);
			// return mode switch
			// {
			// 	RleMode.Flat2D or RleMode.Line1D => BuildRleLinear(colorIds, w, h, d),
			// 	_ => 
			// };
		}

		// private static RleMode DetectRleMode(int w, int h, int d)
		// {
		// 	if (w == 1 || h == 1 || d == 1)
		// 		return RleMode.Flat2D;
		// 	return RleMode.Morton3D;
		// }

		private static StoredRleRun[] BuildRleLinear(byte[] colorIds, int w, int h, int d)
		{
			var runs = new List<StoredRleRun>();
			int total = w * h * d;

			byte prev = 0;
			byte len = 0;
			uint startIndex = 0;

			for (int idx = 0; idx < total; idx++)
			{
				byte c = colorIds[idx];

				if (c == 0)
				{
					if (len > 0)
					{
						runs.Add(new StoredRleRun
						{
							StartMorton = startIndex, // линейный индекс
							Length = len,
							ColorId = prev
						});
						len = 0;
					}

					continue;
				}

				if (len == 0)
				{
					startIndex = (uint)idx;
					prev = c;
					len = 1;
				}
				else if (c == prev && len < 255)
				{
					len++;
				}
				else
				{
					runs.Add(new StoredRleRun
					{
						StartMorton = startIndex,
						Length = len,
						ColorId = prev
					});

					startIndex = (uint)idx;
					prev = c;
					len = 1;
				}
			}

			if (len > 0)
			{
				runs.Add(new StoredRleRun
				{
					StartMorton = startIndex,
					Length = len,
					ColorId = prev
				});
			}

			return runs.ToArray();
		}

		private static StoredRleRun[] BuildRleMorton(byte[] colorIds, int w, int h, int d)
		{
			var list = new List<(uint morton, byte color)>();

			for (int z = 0; z < d; z++)
			for (int y = 0; y < h; y++)
			for (int x = 0; x < w; x++)
			{
				int idx = x + y * w + z * w * h;
				byte c = colorIds[idx];
				if (c == 0) continue;


				int nx = x + 128;
				int ny = y;
				int nz = z + 128;

				uint m = StoredRleRun.Encode8(nx, ny, nz);
				list.Add((m, c));
			}

			if (list.Count == 0)
				return Array.Empty<StoredRleRun>();

			list.Sort((a, b) => a.morton.CompareTo(b.morton));

			var runs = new List<StoredRleRun>();

			uint start = list[0].morton;
			byte prev = list[0].color;
			byte len = 1;

			for (int i = 1; i < list.Count; i++)
			{
				var (m, c) = list[i];

				if (c == prev && m == start + len && len < 255)
				{
					len++;
				}
				else
				{
					runs.Add(new StoredRleRun
					{
						StartMorton = start,
						Length = len,
						ColorId = prev
					});

					start = m;
					prev = c;
					len = 1;
				}
			}

			runs.Add(new StoredRleRun
			{
				StartMorton = start,
				Length = len,
				ColorId = prev
			});

			return runs.ToArray();
		}
	}
}