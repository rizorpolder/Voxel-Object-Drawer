using System;
using UnityEngine;

namespace CoreData
{
	public struct VoxelData : IEquatable<VoxelData>
	{
		public byte Type;
		public uint PackedPos;
		public Color32 Color;

		public static uint PackPos(int x, int y, int z)
		{
			unchecked
			{
				byte bx = (byte) x;
				byte by = (byte) y;
				byte bz = (byte) z;

				return (uint) (bx << 16 | by << 8 | bz);
			}
		}

		public static void UnpackPos(uint pos, out int x, out int y, out int z)
		{
			unchecked
			{
				x = (sbyte) ((pos >> 16) & 0xFF);
				y = (sbyte) ((pos >> 8) & 0xFF);
				z = (sbyte) (pos & 0xFF);
			}
		}

		public bool Equals(VoxelData other)
		{
			return Type == other.Type && PackedPos == other.PackedPos && Color.Equals(other.Color);
		}

		public override bool Equals(object obj)
		{
			return obj is VoxelData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, PackedPos, Color);
		}
	}
}