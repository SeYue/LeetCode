using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws an enum in a horizontal button group instead of a dropdown.
	/// </summary>
	public class EnumToggleButtonsAttributeDrawer<T> : OdinAttributeDrawer<EnumToggleButtonsAttribute, T>
	{
		private static bool DoManualColoring = UnityVersion.IsVersionOrGreater(2019, 3);

		private static Color ActiveColor = (Color)(EditorGUIUtility.get_isProSkin() ? Color.get_white() : new Color(0.802f, 0.802f, 0.802f, 1f));

		private static Color InactiveColor = (Color)(EditorGUIUtility.get_isProSkin() ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.get_white());

		private GUIContent[] Names;

		private ulong[] Values;

		private float[] NameSizes;

		private bool IsFlagsEnum;

		private List<int> ColumnCounts;

		private float PreviousControlRectWidth;

		private Color?[] SelectionColors;

		/// <summary>
		/// Returns <c>true</c> if the drawer can draw the type.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			return type.IsEnum;
		}

		protected override void Initialize()
		{
			Type typeOfValue = base.ValueEntry.TypeOfValue;
			string[] names = Enum.GetNames(typeOfValue);
			Names = ((IEnumerable<string>)names).Select((Func<string, GUIContent>)((string x) => new GUIContent(x.SplitPascalCase()))).ToArray();
			Values = new ulong[Names.Length];
			IsFlagsEnum = typeOfValue.IsDefined<FlagsAttribute>();
			NameSizes = Names.Select((GUIContent x) => SirenixGUIStyles.MiniButtonMid.CalcSize(x).x).ToArray();
			ColumnCounts = new List<int> { NameSizes.Length };
			GUIHelper.RequestRepaint();
			for (int i = 0; i < Values.Length; i++)
			{
				Values[i] = TypeExtensions.GetEnumBitmask(Enum.Parse(typeOfValue, names[i]), typeOfValue);
			}
			if (DoManualColoring)
			{
				SelectionColors = new Color?[Names.Length];
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Invalid comparison between Unknown and I4
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0342: Unknown result type (might be due to invalid IL or missing references)
			//IL_0348: Invalid comparison between Unknown and I4
			//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0400: Invalid comparison between Unknown and I4
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			Type type = propertyValueEntry.WeakValues[0].GetType();
			int i;
			for (i = 1; i < propertyValueEntry.WeakValues.Count; i++)
			{
				if (type != propertyValueEntry.WeakValues[i].GetType())
				{
					SirenixEditorGUI.ErrorMessageBox("ToggleEnum does not support multiple different enum types.");
					return;
				}
			}
			ulong num = TypeExtensions.GetEnumBitmask(propertyValueEntry.SmartValue, typeof(T));
			Rect val = default(Rect);
			i = 0;
			for (int j = 0; j < ColumnCounts.Count; j++)
			{
				SirenixEditorGUI.GetFeatureRichControlRect((j == 0) ? label : GUIContent.none, out var _, out var _, out var valueRect);
				if (j == 0)
				{
					val = valueRect;
				}
				else
				{
					((Rect)(ref valueRect)).set_xMin(((Rect)(ref val)).get_xMin());
				}
				float xMax = ((Rect)(ref valueRect)).get_xMax();
				((Rect)(ref valueRect)).set_width(((Rect)(ref valueRect)).get_width() / (float)ColumnCounts[j]);
				((Rect)(ref valueRect)).set_width((float)(int)((Rect)(ref valueRect)).get_width());
				int num2 = i;
				for (int num3 = i + ColumnCounts[j]; i < num3; i++)
				{
					bool flag;
					if (IsFlagsEnum)
					{
						ulong enumBitmask = TypeExtensions.GetEnumBitmask(Values[i], typeof(T));
						flag = ((num == 0L) ? (enumBitmask == 0) : (enumBitmask != 0L && (enumBitmask & num) == enumBitmask));
					}
					else
					{
						flag = Values[i] == num;
					}
					Color? val2 = null;
					if (DoManualColoring)
					{
						Color val3 = (flag ? ActiveColor : InactiveColor);
						val2 = SelectionColors[i];
						if (!val2.HasValue)
						{
							val2 = val3;
						}
						else if (val2.Value != val3 && (int)Event.get_current().get_type() == 8)
						{
							float num4 = EditorTimeHelper.Time.DeltaTime * 4f;
							val2 = new Color(Mathf.MoveTowards(val2.Value.r, val3.r, num4), Mathf.MoveTowards(val2.Value.g, val3.g, num4), Mathf.MoveTowards(val2.Value.b, val3.b, num4), Mathf.MoveTowards(val2.Value.a, val3.a, num4));
							GUIHelper.RequestRepaint();
						}
						SelectionColors[i] = val2;
					}
					Rect val4 = valueRect;
					GUIStyle val5;
					if (i == num2 && i == num3 - 1)
					{
						val5 = (flag ? SirenixGUIStyles.MiniButtonSelected : SirenixGUIStyles.MiniButton);
						((Rect)(ref val4)).set_x(((Rect)(ref val4)).get_x() - 1f);
						((Rect)(ref val4)).set_xMax(xMax + 1f);
					}
					else if (i == num2)
					{
						val5 = (flag ? SirenixGUIStyles.MiniButtonLeftSelected : SirenixGUIStyles.MiniButtonLeft);
					}
					else if (i == num3 - 1)
					{
						val5 = (flag ? SirenixGUIStyles.MiniButtonRightSelected : SirenixGUIStyles.MiniButtonRight);
						((Rect)(ref val4)).set_xMax(xMax);
					}
					else
					{
						val5 = (flag ? SirenixGUIStyles.MiniButtonMidSelected : SirenixGUIStyles.MiniButtonMid);
					}
					if (DoManualColoring)
					{
						GUIHelper.PushColor(val2.Value * GUI.get_color());
					}
					if (GUI.Button(val4, Names[i], val5))
					{
						GUIHelper.RemoveFocusControl();
						if (!IsFlagsEnum || Event.get_current().get_button() == 1 || (int)Event.get_current().get_modifiers() == 2)
						{
							propertyValueEntry.WeakSmartValue = Enum.ToObject(typeof(T), Values[i]);
						}
						else
						{
							num = ((Values[i] == 0L) ? 0 : ((!flag) ? (num | Values[i]) : (num & ~Values[i])));
							propertyValueEntry.WeakSmartValue = Enum.ToObject(typeof(T), num);
						}
						GUIHelper.RequestRepaint();
					}
					if (DoManualColoring)
					{
						GUIHelper.PopColor();
					}
					((Rect)(ref valueRect)).set_x(((Rect)(ref valueRect)).get_x() + ((Rect)(ref valueRect)).get_width());
				}
			}
			if ((int)Event.get_current().get_type() != 7 || PreviousControlRectWidth == ((Rect)(ref val)).get_width())
			{
				return;
			}
			PreviousControlRectWidth = ((Rect)(ref val)).get_width();
			float num5 = 0f;
			int num6 = 0;
			ColumnCounts.Clear();
			ColumnCounts.Add(0);
			for (i = 0; i < NameSizes.Length; i++)
			{
				float num7 = NameSizes[i] + 3f;
				int num8 = ++ColumnCounts[num6];
				float num9 = ((Rect)(ref val)).get_width() / (float)num8;
				num5 = Mathf.Max(num7, num5);
				if (num5 > num9 && num8 > 1)
				{
					ColumnCounts[num6]--;
					ColumnCounts.Add(1);
					num6++;
					num5 = num7;
				}
			}
		}
	}
}
