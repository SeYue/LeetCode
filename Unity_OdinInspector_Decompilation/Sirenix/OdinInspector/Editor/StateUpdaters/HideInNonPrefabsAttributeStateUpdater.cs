using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInNonPrefabsAttributeStateUpdater : AttributeStateUpdater<HideInNonPrefabsAttribute>
	{
		private bool hide;

		protected override void Initialize()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Invalid comparison between Unknown and I4
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Invalid comparison between Unknown and I4
			object obj = base.Property.Tree.WeakTargets[0];
			Object val = obj as Object;
			if (val != (Object)null)
			{
				PrefabType prefabType = PrefabUtility.GetPrefabType(val);
				hide = (int)prefabType == 0 || (int)prefabType == 5 || (int)prefabType == 7 || (int)prefabType == 6;
			}
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = !hide;
		}
	}
}
