using Sample.ShapeGenerator.Strategies;

namespace Sample.ShapeGenerator
{
	public static class ShapeFactory
	{
		public static IShapeStrategy Get(TShape mode, int size)
		{
			return mode switch
			{
				TShape.Cube => new CubeStrategy(size),
				TShape.Sphere => new SphereStrategy(size),
				TShape.Pyramid => new PyramidStrategy(size),
				TShape.Cylinder => new CylinderStrategy(size),
				_ => new CubeStrategy(1)
			};
		}
	}
}