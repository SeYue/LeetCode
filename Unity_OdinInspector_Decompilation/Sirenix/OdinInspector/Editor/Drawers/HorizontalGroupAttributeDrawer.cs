using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the <see cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" />
	public class HorizontalGroupAttributeDrawer : OdinGroupDrawer<HorizontalGroupAttribute>
	{
		private float[] widths;

		private float[] minWidths;

		private float[] maxWidths;

		private float[] labelWidths;

		private float totalWidth;

		private Vector2[] margins;

		private Vector2[] paddings;

		private ValueResolver<string> titleGetter;

		private int containsPercentageWidth;

		protected override void Initialize()
		{
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			if (base.Attribute.Title != null)
			{
				titleGetter = ValueResolver.GetForString(base.Property, base.Attribute.Title);
			}
			widths = new float[base.Property.Children.Count];
			minWidths = new float[base.Property.Children.Count];
			maxWidths = new float[base.Property.Children.Count];
			labelWidths = new float[base.Property.Children.Count];
			margins = (Vector2[])(object)new Vector2[base.Property.Children.Count];
			paddings = (Vector2[])(object)new Vector2[base.Property.Children.Count];
			float num = 0f;
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				InspectorProperty inspectorProperty = base.Property.Children[i];
				HorizontalGroupAttribute horizontalGroupAttribute = inspectorProperty.Children.Recurse().AppendWith(inspectorProperty).SelectMany((InspectorProperty a) => a.GetAttributes<HorizontalGroupAttribute>())
					.FirstOrDefault((HorizontalGroupAttribute x) => x.GroupID == base.Attribute.GroupID);
				if (horizontalGroupAttribute == null)
				{
					widths[i] = -1f;
					continue;
				}
				widths[i] = horizontalGroupAttribute.Width;
				minWidths[i] = horizontalGroupAttribute.MinWidth;
				maxWidths[i] = horizontalGroupAttribute.MaxWidth;
				labelWidths[i] = horizontalGroupAttribute.LabelWidth;
				if (horizontalGroupAttribute.Width > 0f && horizontalGroupAttribute.Width < 1f)
				{
					containsPercentageWidth++;
					num += horizontalGroupAttribute.Width;
					if ((double)num >= 0.97)
					{
						num -= horizontalGroupAttribute.Width;
						widths[i] = 0f;
						horizontalGroupAttribute.Width = 0f;
					}
				}
				if (horizontalGroupAttribute.MinWidth > 0f && horizontalGroupAttribute.MinWidth <= 1f)
				{
					containsPercentageWidth++;
					num += horizontalGroupAttribute.MinWidth;
					if ((double)num >= 0.97)
					{
						num -= horizontalGroupAttribute.MinWidth;
						minWidths[i] = 0f;
						horizontalGroupAttribute.MinWidth = 0f;
					}
				}
				margins[i] = new Vector2(horizontalGroupAttribute.MarginLeft, horizontalGroupAttribute.MarginRight);
				paddings[i] = new Vector2(horizontalGroupAttribute.PaddingLeft, horizontalGroupAttribute.PaddingRight);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0306: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0355: Invalid comparison between Unknown and I4
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_035c: Unknown result type (might be due to invalid IL or missing references)
			if (titleGetter != null)
			{
				if (titleGetter.HasError)
				{
					SirenixEditorGUI.ErrorMessageBox(titleGetter.ErrorMessage);
				}
				else
				{
					SirenixEditorGUI.Title(titleGetter.GetValue(), null, (TextAlignment)0, horizontalLine: false);
				}
			}
			SirenixEditorGUI.BeginIndentedHorizontal();
			if (base.Attribute.LabelWidth > 0f)
			{
				GUIHelper.PushLabelWidth(base.Attribute.LabelWidth);
			}
			float fieldWidth = EditorGUIUtility.get_fieldWidth();
			EditorGUIUtility.set_fieldWidth(40f);
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				InspectorProperty inspectorProperty = base.Property.Children[i];
				if (!inspectorProperty.State.VisibleLastLayout)
				{
					continue;
				}
				float num2;
				float num3;
				Vector2 val;
				Vector2 val2;
				float num;
				if (containsPercentageWidth > 1 && totalWidth == 0f)
				{
					num = 20f;
					num2 = 0f;
					num3 = 0f;
					val = default(Vector2);
					val2 = default(Vector2);
				}
				else
				{
					num = widths[i];
					num2 = minWidths[i];
					num3 = maxWidths[i];
					val2 = margins[i];
					val = paddings[i];
					if (val.x > 0f && val.x <= 1f)
					{
						val.x *= totalWidth;
					}
					if (val.y > 0f && val.y <= 1f)
					{
						val.y *= totalWidth;
					}
					if (val2.x > 0f && val2.x <= 1f)
					{
						val2.x *= totalWidth;
					}
					if (val2.y > 0f && val2.y <= 1f)
					{
						val2.y *= totalWidth;
					}
					if (num <= 1f)
					{
						num *= totalWidth;
					}
					num -= val.x + val.y;
					if (num2 > 0f)
					{
						if (num2 <= 1f)
						{
							num2 *= totalWidth;
						}
						num2 -= val.x + val.y;
					}
					if (num3 > 0f)
					{
						if (num3 <= 1f)
						{
							num3 *= totalWidth;
						}
						num3 -= val.x + val.y;
					}
				}
				GUILayoutOptions.GUILayoutOptionsInstance gUILayoutOptionsInstance = null;
				if (num2 > 0f)
				{
					gUILayoutOptionsInstance = GUILayoutOptions.MinWidth(num2);
				}
				if (num3 > 0f)
				{
					gUILayoutOptionsInstance = ((gUILayoutOptionsInstance == null) ? GUILayoutOptions.MaxWidth(num3) : gUILayoutOptionsInstance.MaxWidth(num3));
				}
				if (gUILayoutOptionsInstance == null)
				{
					gUILayoutOptionsInstance = GUILayoutOptions.Width((num < 0f) ? 0f : num);
				}
				if (val2.x + val.x != 0f)
				{
					GUILayout.Space(val2.x + val.x);
				}
				GUILayout.BeginVertical((GUILayoutOption[])gUILayoutOptionsInstance);
				inspectorProperty.Draw(inspectorProperty.Label);
				GUILayout.Space(-3f);
				GUILayout.EndVertical();
				if (val2.y + val.y != 0f)
				{
					GUILayout.Space(val2.y + val.y);
				}
			}
			if ((int)Event.get_current().get_type() == 7)
			{
				Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
				float width = ((Rect)(ref currentLayoutRect)).get_width();
				if (totalWidth != width)
				{
					GUIHelper.RequestRepaint();
				}
				totalWidth = width;
			}
			EditorGUIUtility.set_fieldWidth(fieldWidth);
			if (base.Attribute.LabelWidth > 0f)
			{
				GUIHelper.PopLabelWidth();
			}
			SirenixEditorGUI.EndIndentedHorizontal();
		}
	}
}
