using System.IO;
using System.Text;
using Serializer.StorageData;

namespace Serializer.CustomBinarySerializer
{
	public class ObjectBinarySerializer
	{
		public byte[] Serialize(StoredObjectData obj)
		{
			using var ms = new MemoryStream();
			using var bw = new BinaryWriter(ms);

			WriteString(bw, obj.Name);
			
			bw.Write(obj.OriginX);
			bw.Write(obj.OriginY);
			bw.Write(obj.OriginZ);
			bw.Write(obj.PivotX);
			bw.Write(obj.PivotY);
			bw.Write(obj.PivotZ);

			bw.Write((ushort) obj.Palette.Length);
			foreach (var p in obj.Palette)
			{
				bw.Write(p.RGB565);
				bw.Write(p.Type);
			}

			bw.Write((ushort) obj.RleRuns.Length);
			foreach (var r in obj.RleRuns)
			{
				uint m = r.StartMorton;

				bw.Write((byte) (m & 0xFF));
				bw.Write((byte) ((m >> 8) & 0xFF));
				bw.Write((byte) ((m >> 16) & 0xFF));

				bw.Write(r.Length);
				bw.Write(r.ColorId);
			}
		
			return ms.ToArray();
		}

		public StoredObjectData Deserialize(byte[] data)
		{
			using var ms = new MemoryStream(data);
			using var br = new BinaryReader(ms);
			
			var obj = new StoredObjectData
			{
				Name = ReadString(br),
				OriginX = br.ReadSByte(),
				OriginY = br.ReadByte(),
				OriginZ = br.ReadSByte(),
				PivotX = br.ReadSByte(),
				PivotY = br.ReadByte(),
				PivotZ = br.ReadSByte(),
			};

			int paletteCount = br.ReadUInt16();
			obj.Palette = new StoredPaletteEntry[paletteCount];
			for (int i = 0; i < paletteCount; i++)
			{
				obj.Palette[i] = new StoredPaletteEntry
				{
					RGB565 = br.ReadUInt16(),
					Type = br.ReadByte()
				};
			}

			int rleCount = br.ReadUInt16();
			obj.RleRuns = new StoredRleRun[rleCount];
			for (int i = 0; i < rleCount; i++)
			{
				byte m0 = br.ReadByte();
				byte m1 = br.ReadByte();
				byte m2 = br.ReadByte();

				uint morton = (uint) (m0 | (m1 << 8) | (m2 << 16));

				byte len = br.ReadByte();
				byte colorId = br.ReadByte();

				obj.RleRuns[i] = new StoredRleRun
				{
					StartMorton = morton,
					Length = len,
					ColorId = colorId
				};
			}

			return obj;
		}

		private void WriteString(BinaryWriter bw, string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				bw.Write((ushort) 0);
				return;
			}

			var bytes = Encoding.UTF8.GetBytes(s);
			bw.Write((ushort) bytes.Length);
			bw.Write(bytes);
		}

		private string ReadString(BinaryReader br)
		{
			ushort len = br.ReadUInt16();
			if (len == 0) return string.Empty;
			return Encoding.UTF8.GetString(br.ReadBytes(len));
		}
	}
}