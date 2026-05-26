using CoreData;

namespace Serializer.StorageData
{
	public class StoredObjectData
	{
		public string Name;
		public StoredPaletteEntry[] Palette;
		public StoredRleRun[] RleRuns;

		public sbyte OriginX; // RLE и Morton считают отбъект от 0, поэтому нужно хранить точку смещения всех chunk
		public byte OriginY;
		public sbyte OriginZ;

		public sbyte PivotX;
		public byte PivotY;
		public sbyte PivotZ;

		public static StoredObjectData ConvertObject(ObjectData obj)
		{
			var (palette, colorIds, w, h, d, ox, oy, oz) = StoredPaletteEntry.BuildPalette(obj);
			var rle = RleBuilder.BuildHybridRle(colorIds, w, h, d);

			return new StoredObjectData
			{
				Name = obj.Name,
				Palette = palette,
				RleRuns = rle,
				OriginX = ox,
				OriginY = oy,
				OriginZ = oz,
				PivotX = obj.PivotX,
				PivotY = obj.PivotY,
				PivotZ = obj.PivotZ,
			};
		}

		public ObjectData ConvertStoredObject()
		{
			VoxelData[] voxels = StoredRleRun.ExpandRleToVoxels(this);
			return new ObjectData
			{
				Name = Name,
				Voxels = voxels,
				PivotX = PivotX,
				PivotY = PivotY,
				PivotZ = PivotZ,
			};
		}
	}
}