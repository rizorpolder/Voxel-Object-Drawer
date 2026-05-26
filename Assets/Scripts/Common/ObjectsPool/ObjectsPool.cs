using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.ObjectsPool
{
	public class ObjectsPool<T> where T : MonoBehaviour
	{
		private Transform _poolRoot;
		private T _prefab;
		public T Prefab => _prefab;

		private List<T> _items = new List<T>();
		private Queue<T> _freeElements = new Queue<T>();

		public int ActiveItems => _items.Count - _freeElements.Count;

		[Inject] private IObjectResolver _resolver;

		private int _initializeItemsCount = 0;

		public ObjectsPool(Transform parent, T prefab)
		{
			_poolRoot = parent;
			_prefab = prefab;
		}

		public void InitializePool()
		{
			ResetPool();

			for (var i = 0; i < _initializeItemsCount; i++)
			{
				var item = CreateItem();
				_items.Add(item);
				_freeElements.Enqueue(item);
			}
		}

		public void InitializePool(int initializeItemsCount)
		{
			_initializeItemsCount = initializeItemsCount;
			InitializePool();
		}

		private T CreateItem()
		{
			var item = _resolver.Instantiate<T>(_prefab, _poolRoot);
			item.gameObject.SetActive(false);
			return item;
		}

		public T GetItem()
		{
			if (!_freeElements.TryDequeue(out var item))
			{
				item = CreateItem();
				_items.Add(item);
			}

			item.gameObject.SetActive(true);
			return item;
		}

		public List<T> GetActiveItemsList()
		{
			return _items;
		}

		public IEnumerable<T> GetActiveItems()
		{
			return _items.Where(i => i != null && i.gameObject.activeSelf);
		}

		public void ReturnToPool(T item)
		{
			_freeElements.Enqueue(item);
			item.gameObject.SetActive(false);
			item.transform.SetParent(_poolRoot);
		}

		public void ResetPool()
		{
			_freeElements.Clear();

			foreach (var item in _items)
			{
				ReturnToPool(item);
			}
		}
	}
}