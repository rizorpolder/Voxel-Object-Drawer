using System;
using System.Collections.Generic;
using CoreData;
using UnityEngine;

namespace Serializer.StorageData
{
	public struct StoredPaletteEntry
	{
		public ushort RGB565;
		public byte Type;

		public static (StoredPaletteEntry[] palette, byte[] colorIds, byte w, byte h, byte d,
			sbyte originX, byte originY, sbyte originZ)
			BuildPalette(ObjectData obj)
		{
			int minX = 255, minY = 255, minZ = 255;
			int maxX = 0, maxY = 0, maxZ = 0;

			foreach (var v in obj.Voxels)
			{
				VoxelData.UnpackPos(v.PackedPos, out int x, out int y, out int z);

				if (x < minX) minX = x;
				if (y < minY) minY = y;
				if (z < minZ) minZ = z;

				if (x > maxX) maxX = x;
				if (y > maxY) maxY = y;
				if (z > maxZ) maxZ = z;
			}

			int wInt = maxX - minX + 1;
			int hInt = maxY - minY + 1;
			int dInt = maxZ - minZ + 1;

			if (wInt > 255 || hInt > 255 || dInt > 255)
				throw new Exception("Object dimensions exceed 255, cannot pack into byte");

			byte w = (byte) wInt;
			byte h = (byte) hInt;
			byte d = (byte) dInt;


			var colorIds = new byte[w * h * d];

			var palette = new List<StoredPaletteEntry>
			{
				new() {RGB565 = 0, Type = 0}
			};

			byte GetColorId(Color32 c, byte type)
			{
				ushort rgb = ToRGB565(c);

				for (byte i = 1; i < palette.Count; i++)
					if (palette[i].RGB565 == rgb && palette[i].Type == type)
						return i;

				palette.Add(new StoredPaletteEntry {RGB565 = rgb, Type = type});
				return (byte) (palette.Count - 1);
			}

			foreach (var v in obj.Voxels)
			{
				VoxelData.UnpackPos(v.PackedPos, out int x, out int y, out int z);

				int lx = x - minX;
				int ly = y - minY;
				int lz = z - minZ;

				int idx = lx + ly * w + lz * w * h;

				colorIds[idx] = GetColorId(v.Color, v.Type);
			}

			return (palette.ToArray(), colorIds, w, h, d,
				(sbyte)minX, (byte)minY, (sbyte)minZ);

		}

		public static ushort ToRGB565(Color32 c)
		{
			ushort r = (ushort) (c.r >> 3);
			ushort g = (ushort) (c.g >> 2);
			ushort b = (ushort) (c.b >> 3);
			return (ushort) ((r << 11) | (g << 5) | b);
		}

		public static Color32 FromRGB565(ushort rgb)
		{
			byte r = (byte) ((rgb >> 11) & 0x1F);
			byte g = (byte) ((rgb >> 5) & 0x3F);
			byte b = (byte) (rgb & 0x1F);

			return new Color32(
				(byte) (r << 3),
				(byte) (g << 2),
				(byte) (b << 3),
				255
			);
		}
	}
}