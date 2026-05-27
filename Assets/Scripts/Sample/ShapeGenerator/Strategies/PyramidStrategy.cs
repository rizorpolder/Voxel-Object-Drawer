using System.Collections.Generic;
using UnityEngine;

namespace Sample.ShapeGenerator.Strategies
{
	public class PyramidStrategy : IShapeStrategy
	{
		private readonly int _size;

		public PyramidStrategy(int size)
		{
			_size = size;
		}

		public IEnumerable<Vector3Int> Apply(Vector3Int start)
		{
			for (int dy = 1; dy <= _size; dy++)
			{
				float t = 1f - (float)dy / _size;
				float s = _size * t;
				int iside = Mathf.CeilToInt(s);

				for (int dx = -iside; dx <= iside; dx++)
				for (int dz = -iside; dz <= iside; dz++)
				{
					yield return new Vector3Int(
						start.x + dx,
						start.y + dy,
						start.z + dz
					);
				}
			}
		}
	}
}