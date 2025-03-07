using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInPrefabAssetsAttributeStateUpdater : AttributeStateUpdater<HideInPrefabAssetsAttribute>
	{
		private bool hide;

		protected override void Initialize()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Invalid comparison between Unknown and I4
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Invalid comparison between Unknown and I4
			object obj = base.Property.Tree.WeakTargets[0];
			Object val = obj as Object;
			if (val != (Object)null)
			{
				PrefabType prefabType = PrefabUtility.GetPrefabType(val);
				hide = (int)prefabType == 1 || (int)prefabType == 2;
			}
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = !hide;
		}
	}
}
