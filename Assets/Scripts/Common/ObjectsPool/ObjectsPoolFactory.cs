using UnityEngine;
using VContainer;

namespace Common.ObjectsPool
{
	public class ObjectsPoolFactory
	{
		private IObjectResolver _resolver;
		private Transform _poolRoot;
		[Inject]
		public ObjectsPoolFactory(IObjectResolver resolver)
		{
			_resolver = resolver;
		}

		public ObjectsPool<T> CreatePool<T>(T prefab, Transform parent) where T : MonoBehaviour
		{
			_poolRoot = parent;
			var result = new ObjectsPool<T>(_poolRoot, prefab);
			_resolver.Inject(result);
			return result;
		}
		
		public ObjectsPool<T> CreatePool<T>(T prefab) where T : MonoBehaviour
		{
			var result = new ObjectsPool<T>(_poolRoot, prefab);
			_resolver.Inject(result);
			return result;
		}

		public ObjectsPoolFactory SetRoot(Transform parent)
		{
			_poolRoot = parent;
			return this;
		}
	}
}