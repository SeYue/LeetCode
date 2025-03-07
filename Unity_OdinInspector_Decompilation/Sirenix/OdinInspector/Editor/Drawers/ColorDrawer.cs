using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Color property drawer.
	/// </summary>
	public sealed class ColorDrawer : PrimitiveCompositeDrawer<Color>, IDefinesGenericMenuItems
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyField(IPropertyValueEntry<Color> entry, GUIContent label)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			Rect val = EditorGUILayout.GetControlRect(label != null, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (label != null)
			{
				val = EditorGUI.PrefixLabel(val, label);
			}
			bool flag = false;
			if (Event.get_current().OnMouseDown(val, 1, useEvent: false))
			{
				GUIHelper.PushEventType((EventType)12);
				flag = true;
			}
			entry.SmartValue = EditorGUI.ColorField(val, entry.SmartValue);
			if (flag)
			{
				GUIHelper.PopEventType();
			}
		}

		internal static void PopulateGenericMenu<T>(IPropertyValueEntry<T> entry, GenericMenu genericMenu)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00a3: Expected O, but got Unknown
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_00c0: Expected O, but got Unknown
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Expected O, but got Unknown
			//IL_00dd: Expected O, but got Unknown
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Expected O, but got Unknown
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Expected O, but got Unknown
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected O, but got Unknown
			Color color;
			if (entry.TypeOfValue == typeof(Color))
			{
				color = (Color)(object)entry.SmartValue;
			}
			else
			{
				color = Color32.op_Implicit((Color32)(object)entry.SmartValue);
			}
			Color colorInClipboard;
			bool flag = ColorExtensions.TryParseString(EditorGUIUtility.get_systemCopyBuffer(), out colorInClipboard);
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			genericMenu.AddItem(new GUIContent("Copy RGBA"), false, (MenuFunction)delegate
			{
				EditorGUIUtility.set_systemCopyBuffer(entry.SmartValue.ToString());
			});
			genericMenu.AddItem(new GUIContent("Copy HEX"), false, (MenuFunction)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				EditorGUIUtility.set_systemCopyBuffer("#" + ColorUtility.ToHtmlStringRGBA(color));
			});
			genericMenu.AddItem(new GUIContent("Copy Color Code Declaration"), false, (MenuFunction)delegate
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				EditorGUIUtility.set_systemCopyBuffer(color.ToCSharpColor());
			});
			if (flag)
			{
				genericMenu.ReplaceOrAdd("Paste", on: false, (MenuFunction)delegate
				{
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						SetEntryValue(entry, colorInClipboard);
					});
					GUIHelper.RequestRepaint();
				});
			}
			else if (Clipboard.CanPaste(typeof(Color)) || Clipboard.CanPaste(typeof(Color32)))
			{
				genericMenu.ReplaceOrAdd("Paste", on: false, (MenuFunction)delegate
				{
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						SetEntryValue(entry, Clipboard.Paste());
					});
					GUIHelper.RequestRepaint();
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Paste"));
			}
		}

		private static void SetEntryValue<T>(IPropertyValueEntry<T> entry, object value)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			Type type = value.GetType();
			T value2 = ((typeof(T) == typeof(Color)) ? ((type != typeof(Color)) ? ((T)(object)Color32.op_Implicit((Color32)value)) : ((T)value)) : ((type != typeof(Color)) ? ((T)value) : ((T)(object)Color32.op_Implicit((Color)value))));
			for (int i = 0; i < entry.ValueCount; i++)
			{
				entry.Values[i] = value2;
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			PopulateGenericMenu((IPropertyValueEntry<Color>)property.ValueEntry, genericMenu);
		}
	}
}
