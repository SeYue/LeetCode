using System;

namespace Sirenix.OdinInspector.Editor
{
	[Flags]
	internal enum SerializationBackendFlags
	{
		None = 0x0,
		Unity = 0x1,
		Odin = 0x2,
		UnityAndOdin = 0x3
	}
}
