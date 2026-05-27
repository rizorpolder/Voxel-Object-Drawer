using System;

namespace CoreData
{
	[Flags]
	public enum TVoxelType : byte
	{
		Default = 0,
		Metal = 1,
		Light = 2,
		Glass = 3,
		Water = 4,
	}
}