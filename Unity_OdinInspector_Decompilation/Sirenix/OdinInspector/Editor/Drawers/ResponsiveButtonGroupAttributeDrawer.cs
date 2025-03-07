using System.Linq;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the ResponsiveButtonGroupAttribute.
	/// </summary>
	public class ResponsiveButtonGroupAttributeDrawer : OdinGroupDrawer<ResponsiveButtonGroupAttribute>
	{
		private Vector2[] btnSizes;

		private int[] colCounts;

		private int prevWidth = 400;

		private bool isFirstFrame = true;

		private int innerWidth;

		/// <summary>
		/// Draws the property with GUILayout support.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Invalid comparison between Unknown and I4
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ba: Invalid comparison between Unknown and I4
			//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			ResponsiveButtonGroupAttribute responsiveButtonGroupAttribute = base.Attribute;
			if (btnSizes == null || btnSizes.Length != inspectorProperty.Children.Count)
			{
				colCounts = null;
				prevWidth = 400;
				btnSizes = (Vector2[])(object)new Vector2[inspectorProperty.Children.Count];
				for (int i = 0; i < btnSizes.Length; i++)
				{
					InspectorProperty inspectorProperty2 = inspectorProperty.Children[i].FindChild((InspectorProperty x) => x.Info.GetMemberInfo() is MethodInfo, includeSelf: true) ?? inspectorProperty.Children[i];
					ButtonAttribute buttonAttribute = inspectorProperty2.GetAttribute<ButtonAttribute>();
					int num = buttonAttribute?.ButtonHeight ?? ((int)responsiveButtonGroupAttribute.DefaultButtonSize);
					if (buttonAttribute == null)
					{
						inspectorProperty2.Context.GetGlobal("ButtonHeight", 0f).Value = (float)responsiveButtonGroupAttribute.DefaultButtonSize;
					}
					num = (int)SirenixGUIStyles.Button.CalcSize(inspectorProperty2.Label).x;
					btnSizes[i] = new Vector2((float)num, (float)(buttonAttribute?.ButtonHeight ?? ((int)responsiveButtonGroupAttribute.DefaultButtonSize)));
				}
				if (responsiveButtonGroupAttribute.UniformLayout)
				{
					float num2 = btnSizes.Max((Vector2 x) => x.x);
					for (int j = 0; j < btnSizes.Length; j++)
					{
						btnSizes[j] = new Vector2(num2, btnSizes[j].y);
					}
				}
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				bool flag = false;
				int num3 = innerWidth;
				if (isFirstFrame)
				{
					num3 = 999999;
					isFirstFrame = false;
					GUIHelper.RequestRepaint();
				}
				if (prevWidth != num3 || colCounts == null)
				{
					if (num3 > 0)
					{
						prevWidth = num3;
					}
					flag = true;
					num3 = prevWidth;
				}
				colCounts = colCounts ?? new int[inspectorProperty.Children.Count];
				if (flag)
				{
					for (int k = 0; k < colCounts.Length; k++)
					{
						colCounts[k] = 0;
					}
					int num4 = 0;
					Vector2 val = btnSizes[0];
					int num5 = 0;
					bool flag2 = false;
					for (int l = 0; l < btnSizes.Length; l++)
					{
						num5 = Mathf.Max((int)btnSizes[l].x, num5);
						int num6 = num5 * (colCounts[num4] + 1);
						flag2 = num6 > num3 || (int)val.y != (int)btnSizes[l].y;
						val = btnSizes[l];
						if (flag2)
						{
							num5 = (int)btnSizes[l].x;
							if (colCounts[num4] != 0)
							{
								num4++;
							}
						}
						colCounts[num4]++;
					}
				}
			}
			DefaultMethodDrawer.DontDrawMethodParameters = true;
			int num7 = 0;
			for (int m = 0; m < colCounts.Length; m++)
			{
				if (num7 >= inspectorProperty.Children.Count)
				{
					break;
				}
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				for (int n = 0; n < colCounts[m]; n++)
				{
					if (num7 >= inspectorProperty.Children.Count)
					{
						break;
					}
					InspectorProperty inspectorProperty3 = inspectorProperty.Children[num7];
					inspectorProperty3.Draw(inspectorProperty3.Label);
					num7++;
				}
				GUILayout.EndHorizontal();
			}
			DefaultMethodDrawer.DontDrawMethodParameters = false;
			if ((int)Event.get_current().get_type() == 7)
			{
				Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
				innerWidth = (int)((Rect)(ref currentLayoutRect)).get_width();
			}
		}
	}
}
