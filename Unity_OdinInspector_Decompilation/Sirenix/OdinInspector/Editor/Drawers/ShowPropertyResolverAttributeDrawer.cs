using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the ShowPropertyResolver attribute.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ShowPropertyResolverAttribute" />
	[DrawerPriority(10000.0, 0.0, 0.0)]
	public class ShowPropertyResolverAttributeDrawer : OdinAttributeDrawer<ShowPropertyResolverAttribute>
	{
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			string label2 = ((inspectorProperty.ChildResolver != null) ? inspectorProperty.ChildResolver.GetType().GetNiceName() : "None");
			SirenixEditorGUI.BeginToolbarBox(label2, false);
			CallNextDrawer(label);
			SirenixEditorGUI.EndToolbarBox();
		}
	}
}
