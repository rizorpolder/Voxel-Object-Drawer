using System.Collections.Generic;
using UnityEngine;

namespace Sample.ShapeGenerator
{
	public interface IShapeStrategy
	{
		IEnumerable<Vector3Int> Apply(Vector3Int startPoint);
	}
}