using System.Collections.Generic;
using UnityEngine;

namespace Sample.ShapeGenerator.Strategies
{
	public class CubeStrategy: IShapeStrategy
	{
		private readonly int _size;

		public CubeStrategy(int size)
		{
			_size = size;
		}

		public IEnumerable<Vector3Int> Apply(Vector3Int startPoint)
		{
			if (_size <= 0)
				yield break;

			int totalColumns = _size * _size;

			int[] yOffsets = new int[_size];
			for (int i = 0; i < _size; i++)
				yOffsets[i] = i;

			int[] xs = new int[totalColumns];
			int[] zs = new int[totalColumns];

			{
				int x = 0, z = 0;
				int dir = 0;
				int step = 1;
				int index = 0;

				xs[index] = 0;
				zs[index] = 0;
				index++;

				int[] dx = {1, 0, -1, 0};
				int[] dz = {0, 1, 0, -1};

				while (index < totalColumns)
				{
					for (int r = 0; r < 2; r++)
					{
						int ddx = dx[dir];
						int ddz = dz[dir];

						for (int s = 0; s < step; s++)
						{
							x += ddx;
							z += ddz;

							xs[index] = x;
							zs[index] = z;
							index++;

							if (index >= totalColumns)
								break;
						}

						dir = (dir + 1) & 3;
						if (index >= totalColumns)
							break;
					}

					step++;
				}
			}

			for (int i = 0; i < totalColumns; i++)
			{
				int baseX = startPoint.x + xs[i];
				int baseZ = startPoint.z + zs[i];

				for (int y = 0; y < _size; y++)
				{
					yield return new Vector3Int(
						baseX,
						startPoint.y + yOffsets[y],
						baseZ
					);
				}
			}
		}
	}
}