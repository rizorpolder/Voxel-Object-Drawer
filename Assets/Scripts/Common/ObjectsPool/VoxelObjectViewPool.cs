using UnityEngine;
using Views;

namespace Common.ObjectsPool
{
	public class VoxelObjectViewPool : MonoBehaviour
	{
		[SerializeField] private VoxelObjectView _prefab;


		ObjectsPool<VoxelObjectView> _pool;

		private void Start()
		{
			_pool = new ObjectsPool<VoxelObjectView>(transform, _prefab);
		}

		public VoxelObjectView GetObject() => _pool.GetItem();
		public void ReturnObject(VoxelObjectView view) => _pool.ReturnToPool(view);
	}
}