using System.Collections.Generic;
using UnityEngine;

namespace Sample.ShapeGenerator.Strategies
{
	public class SphereStrategy : IShapeStrategy
	{
		private readonly int _radius;

		public SphereStrategy(int radius)
		{
			_radius = radius;
		}

		public IEnumerable<Vector3Int> Apply(Vector3Int start)
		{
			if (_radius <= 0)
				yield break;

			float smoothR = _radius - 0.5f;
			float r2 = smoothR * smoothR;

			int cx = start.x;
			int cy = start.y + _radius;
			int cz = start.z;

			int min = -_radius;
			int max = +_radius;

			for (int dx = min; dx <= max; dx++)
			for (int dy = 0; dy <= 2 * _radius; dy++)
			for (int dz = min; dz <= max; dz++)
			{
				int x = start.x + dx;
				int y = start.y + dy;
				int z = start.z + dz;

				int rx = x - cx;
				int ry = y - cy;
				int rz = z - cz;

				float d2 = rx * rx + ry * ry + rz * rz;
				if (d2 > r2)
					continue;

				yield return new Vector3Int(x, y - 1, z);
			}
		}
	}
}