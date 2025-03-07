using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public class ExampleTransform : ScriptableObject
	{
		public Vector3 Position;

		public Vector3 Rotation;

		public Vector3 Scale = Vector3.get_one();

		public ExampleTransform()
			: this()
		{
		}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)

	}
}
