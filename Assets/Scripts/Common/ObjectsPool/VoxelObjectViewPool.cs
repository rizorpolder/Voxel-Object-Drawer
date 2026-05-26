using UnityEngine;
using VContainer;
using Views;

namespace Common.ObjectsPool
{
	public class VoxelObjectViewPool : MonoBehaviour
	{
		[SerializeField] private VoxelObjectView _prefab;

		[Inject] ObjectsPoolFactory _factory;

		ObjectsPool<VoxelObjectView> _pool;


		public ObjectsPool<VoxelObjectView> GetPool()
		{
			return _pool ?? _factory.CreatePool(_prefab, transform);
		}
	}
}