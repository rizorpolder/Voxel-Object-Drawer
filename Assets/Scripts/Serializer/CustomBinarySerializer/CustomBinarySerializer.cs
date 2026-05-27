using System;
using System.IO;
using K4os.Compression.LZ4;
using Serializer.StorageData;
using ZstdNet;

namespace Serializer.CustomBinarySerializer
{
	public class CustomBinarySerializer : IBinaryDataSerializer
	{
		private const uint MAGIC_OBJECT = 0x564F424A;
		private const uint MAGIC_LZ4 = 0x564C5A34;
		private const uint MAGIC_ZSTD = 0x565A5354;

		private const ushort VERSION = 1;
		private readonly ObjectBinarySerializer _objectSerializer;

		public CustomBinarySerializer()
		{
			_objectSerializer = new ObjectBinarySerializer();
		}

		public byte[] Serialize<T>(T data)
		{
			byte[] raw;
			uint magicType;

			switch (data)
			{
				case StoredObjectData obj:
					raw = _objectSerializer.Serialize(obj);
					magicType = MAGIC_OBJECT;
					break;

				default:
					throw new Exception($"Unsupported type {typeof(T)}");
			}

			return WrapWithCodec(raw, magicType);
		}

		public T Deserialize<T>(byte[] data)
		{
			if (data.Length == 0)
				return default(T);

			using var ms = new MemoryStream(data);
			using var br = new BinaryReader(ms);

			var magicCodec = br.ReadUInt32();
			var magicType = br.ReadUInt32();
			var version = br.ReadUInt16();

			if (version != VERSION)
				throw new Exception("Version mismatch");

			var rawLen = br.ReadInt32();
			var compLen = br.ReadInt32();

			var compressed = br.ReadBytes(compLen);
			var raw = UnwrapWithCodec(magicCodec, compressed);

			if (raw.Length != rawLen)
				throw new Exception("Length mismatch");

			if (typeof(T) == typeof(StoredObjectData))
			{
				if (magicType != MAGIC_OBJECT)
					throw new Exception("Data is not an object");
				return (T) (object) _objectSerializer.Deserialize(raw);
			}

			throw new Exception($"Unsupported type {typeof(T)}");
		}

		private Codec ChooseCodec(int rawSize)
		{
			if (rawSize < 32 * 1024)
				return Codec.LZ4;
			return Codec.Zstd;
		}

		private byte[] WrapWithCodec(byte[] raw, uint magicType)
		{
			var codec = ChooseCodec(raw.Length);

			var compressed = codec switch
			{
				Codec.LZ4 => LZ4Pickler.Pickle(raw),
				Codec.Zstd => new Compressor(new CompressionOptions(10)).Wrap(raw),
				_ => throw new Exception("Unknown codec")
			};

			var magicCodec = codec == Codec.LZ4 ? MAGIC_LZ4 : MAGIC_ZSTD;

			using var ms = new MemoryStream();
			using var bw = new BinaryWriter(ms);

			bw.Write(magicCodec);
			bw.Write(magicType);
			bw.Write(VERSION);
			bw.Write(raw.Length);
			bw.Write(compressed.Length);
			bw.Write(compressed);

			return ms.ToArray();
		}

		private byte[] UnwrapWithCodec(uint magicCodec, byte[] compressed)
		{
			return magicCodec switch
			{
				MAGIC_LZ4 => LZ4Pickler.Unpickle(compressed),
				MAGIC_ZSTD => new Decompressor().Unwrap(compressed),
				_ => throw new Exception("Unknown codec")
			};
		}

		private enum Codec
		{
			LZ4,
			Zstd
		}
	}
}