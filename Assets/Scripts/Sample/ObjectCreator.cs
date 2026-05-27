using CoreData;
using Sample.ShapeGenerator;
using UnityEngine;

namespace Sample
{
	public class ObjectCreator : MonoBehaviour
	{
		public TVoxelType Type;
		public TShape ShapeType;
		public int VoxelSize;

		private void Start()
		{
			//проиницилизировать фабрики 
			// создать объект по нажатию кнопки
			//добавить кнопку сохранения
			//добавить меню импорта vox файла и проекцию его на сцену
		}

		public void GenerateObject()
		{
			var shape = ShapeFactory.Get(ShapeType, VoxelSize);
			var points = shape.Apply(Vector3Int.zero);
		}
	}
}
