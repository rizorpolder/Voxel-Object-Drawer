using System.Collections.Generic;
using UnityEngine;

namespace RuntimeData
{
	public class ChunkUpdateQueue
	{
		private readonly Queue<Vector3Int> _queue = new();
		private readonly HashSet<Vector3Int> _scheduled = new();

		public void MarkDirty(Vector3Int pos)
		{
			if (_scheduled.Add(pos))
				_queue.Enqueue(pos);
		}

		public int Count => _queue.Count;

		public bool TryDequeue(out Vector3Int pos)
		{
			if (_queue.Count > 0)
			{
				pos = _queue.Dequeue();
				_scheduled.Remove(pos);
				return true;
			}

			pos = default;
			return false;
		}

		public void Clear()
		{
			_queue.Clear();
			_scheduled.Clear();
		}
	}
}