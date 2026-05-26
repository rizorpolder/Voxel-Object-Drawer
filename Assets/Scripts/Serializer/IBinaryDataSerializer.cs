namespace Serializer
{
	public interface IBinaryDataSerializer
	{
		public T Deserialize<T>(byte [] data);
		public byte[] Serialize<T>(T data);
	}
}