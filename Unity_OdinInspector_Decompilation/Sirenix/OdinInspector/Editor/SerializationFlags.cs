using System;

namespace Sirenix.OdinInspector.Editor
{
	[Flags]
	internal enum SerializationFlags
	{
		Public = 0x2,
		Field = 0x4,
		Property = 0x8,
		AutoProperty = 0x10,
		SerializedByUnity = 0x20,
		SerializedByOdin = 0x40,
		SerializeFieldAttribute = 0x80,
		OdinSerializeAttribute = 0x100,
		NonSerializedAttribute = 0x200,
		TypeSupportedByUnity = 0x400,
		DefaultSerializationPolicy = 0x800
	}
}
