using System;

namespace CoreData
{
	[Serializable]
	public class ObjectData
	{
		public Guid Id;
		public string Name;
		public VoxelData[] Voxels;
		public byte[] Image;
		public sbyte PivotX;
		public byte PivotY;
		public sbyte PivotZ;

		public ObjectData Clone()
		{
			var clone = new ObjectData
			{
				Id = Id,
				Name = Name,
				PivotX = PivotX,
				PivotY = PivotY,
				PivotZ = PivotZ
			};

			if (Voxels != null)
			{
				var v = new VoxelData[Voxels.Length];
				for (int i = 0; i < Voxels.Length; i++)
					v[i] = Voxels[i];
				clone.Voxels = v;
			}
			else
			{
				clone.Voxels = new VoxelData[] { };
			}

			if (Image != null)
			{
				var img = new byte[Image.Length];
				Buffer.BlockCopy(Image, 0, img, 0, Image.Length);
				clone.Image = img;
			}
			else
			{
				clone.Image = new byte[] { };
			}

			return clone;
		}

		public ObjectData Copy()
		{
			var clone = Clone();
			clone.Id = Guid.NewGuid();
			return clone;
		}

		public static ObjectData Default => new ObjectData()
		{
			Id = Guid.NewGuid(),
			Name = "Объект",
			Voxels = new VoxelData[] { },
			Image = new byte[] { },
			PivotX = 0,
			PivotY = 0,
			PivotZ = 0
		};
	}
}