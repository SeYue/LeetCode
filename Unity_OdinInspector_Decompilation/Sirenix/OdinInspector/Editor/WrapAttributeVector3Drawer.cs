using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws Vector3 properties marked with <see cref="T:Sirenix.OdinInspector.WrapAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.WrapAttribute" />
	[DrawerPriority(0.3, 0.0, 0.0)]
	public class WrapAttributeVector3Drawer : OdinAttributeDrawer<WrapAttribute, Vector3>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			CallNextDrawer(label);
			base.ValueEntry.SmartValue = new Vector3(MathUtilities.Wrap(base.ValueEntry.SmartValue.x, (float)base.Attribute.Min, (float)base.Attribute.Max), MathUtilities.Wrap(base.ValueEntry.SmartValue.y, (float)base.Attribute.Min, (float)base.Attribute.Max), MathUtilities.Wrap(base.ValueEntry.SmartValue.z, (float)base.Attribute.Min, (float)base.Attribute.Max));
		}
	}
}
