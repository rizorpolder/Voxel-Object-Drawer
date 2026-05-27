using System.Collections.Generic;
using UnityEngine;

namespace Sample.ShapeGenerator.Strategies
{
	public class CylinderStrategy : IShapeStrategy
	{
		private readonly int _size;

		public CylinderStrategy(int size)
		{
			_size = size;
		}

		public IEnumerable<Vector3Int> Apply(Vector3Int start)
		{
			int R2 = _size * _size;

			for (int dy = 1; dy <= _size; dy++) 
			for (int dx = -_size; dx <= _size; dx++)
			for (int dz = -_size; dz <= _size; dz++)
			{
				if (dx * dx + dz * dz > R2)
					continue;

				yield return new Vector3Int(
					start.x + dx,
					start.y + dy,
					start.z + dz
				);
			}
		}
	}
}