using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Show drawer chain attribute drawer.
	/// </summary>
	[DrawerPriority(10000.0, 0.0, 0.0)]
	public class ShowDrawerChainAttributeDrawer : OdinAttributeDrawer<ShowDrawerChainAttribute>
	{
		private int drawnDepth;

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			BakedDrawerChain activeDrawerChain = inspectorProperty.GetActiveDrawerChain();
			OdinDrawer[] bakedDrawerArray = activeDrawerChain.BakedDrawerArray;
			SirenixEditorGUI.BeginToolbarBox("Drawers for property '" + base.Property.Path + "'", false);
			for (int i = 0; i < bakedDrawerArray.Length; i++)
			{
				bool flag = bakedDrawerArray[i].GetType().Assembly != typeof(ShowDrawerChainAttributeDrawer).Assembly;
				if (flag)
				{
					GUIHelper.PushColor(Color.get_green());
				}
				if (i > drawnDepth)
				{
					GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.5f));
				}
				EditorGUILayout.LabelField(i + ": " + bakedDrawerArray[i].GetType().GetNiceName() + (bakedDrawerArray[i].SkipWhenDrawing ? " (skipped)" : ""), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (i > drawnDepth)
				{
					GUIHelper.PopColor();
				}
				GUI.Label(lastRect, DrawerUtilities.GetDrawerPriority(bakedDrawerArray[i].GetType()).ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
				if (flag)
				{
					GUIHelper.PopColor();
				}
			}
			SirenixEditorGUI.EndToolbarBox();
			CallNextDrawer(label);
			drawnDepth = activeDrawerChain.CurrentIndex;
		}
	}
}
