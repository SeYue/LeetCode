using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Odin drawer for <see cref="T:Sirenix.OdinInspector.ColorPaletteAttribute" />.
	/// </summary>
	[DrawerPriority(DrawerPriorityLevel.AttributePriority)]
	public sealed class ColorPaletteAttributeDrawer : OdinAttributeDrawer<ColorPaletteAttribute, Color>
	{
		private int paletteIndex;

		private string currentName;

		private LocalPersistentContext<string> persistentName;

		private bool showAlpha;

		private string[] names;

		private ValueResolver<string> nameGetter;

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			paletteIndex = 0;
			currentName = base.Attribute.PaletteName;
			showAlpha = base.Attribute.ShowAlpha;
			names = GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.Select((ColorPalette x) => x.Name).ToArray();
			if (base.Attribute.PaletteName == null)
			{
				persistentName = base.ValueEntry.Context.GetPersistent<string>(this, "ColorPaletteName", null);
				List<string> list = names.ToList();
				currentName = persistentName.Value;
				if (currentName != null && list.Contains(currentName))
				{
					paletteIndex = list.IndexOf(currentName);
				}
			}
			else
			{
				nameGetter = ValueResolver.GetForString(base.Property, base.Attribute.PaletteName);
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_029d: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<Color> propertyValueEntry = base.ValueEntry;
			ColorPaletteAttribute colorPaletteAttribute = base.Attribute;
			SirenixEditorGUI.BeginIndentedHorizontal();
			if (label != null)
			{
				GUILayout.Label(label, (GUILayoutOption[])GUILayoutOptions.Width(GUIHelper.BetterLabelWidth - 4f).ExpandWidth(expand: false));
			}
			else
			{
				GUILayout.Space(5f);
			}
			Rect rect = EditorGUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			((Rect)(ref rect)).set_x(((Rect)(ref rect)).get_x() - 3f);
			((Rect)(ref rect)).set_width(25f);
			propertyValueEntry.SmartValue = SirenixEditorGUI.DrawColorField(rect, propertyValueEntry.SmartValue, useAlphaInPreview: false, showAlpha);
			bool flag = false;
			GUILayout.Space(28f);
			SirenixEditorGUI.BeginInlineBox();
			if (colorPaletteAttribute.PaletteName == null || GlobalConfig<ColorPaletteManager>.Instance.ShowPaletteName)
			{
				SirenixEditorGUI.BeginToolbarBoxHeader();
				if (colorPaletteAttribute.PaletteName == null)
				{
					int num = EditorGUILayout.Popup(paletteIndex, names, (GUILayoutOption[])GUILayoutOptions.ExpandWidth());
					if (paletteIndex != num)
					{
						paletteIndex = num;
						currentName = names[num];
						persistentName.Value = currentName;
						GUIHelper.RemoveFocusControl();
					}
				}
				else
				{
					GUILayout.Label(currentName, (GUILayoutOption[])(object)new GUILayoutOption[0]);
					GUILayout.FlexibleSpace();
				}
				flag = true;
				if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
				{
					GlobalConfig<ColorPaletteManager>.Instance.OpenInEditor();
				}
				SirenixEditorGUI.EndToolbarBoxHeader();
			}
			ColorPalette colorPalette = ((colorPaletteAttribute.PaletteName != null) ? GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.FirstOrDefault((ColorPalette x) => x.Name == nameGetter.GetValue()) : GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.FirstOrDefault((ColorPalette x) => x.Name == names[paletteIndex]));
			if (colorPalette == null)
			{
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				if (colorPaletteAttribute.PaletteName != null && GUILayout.Button("Create color palette: " + nameGetter.GetValue(), (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.Add(new ColorPalette
					{
						Name = nameGetter.GetValue()
					});
					GlobalConfig<ColorPaletteManager>.Instance.OpenInEditor();
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				currentName = colorPalette.Name;
				showAlpha = colorPaletteAttribute.ShowAlpha && colorPalette.ShowAlpha;
				if (!flag)
				{
					GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				Color color = propertyValueEntry.SmartValue;
				bool stretchPalette = GlobalConfig<ColorPaletteManager>.Instance.StretchPalette;
				int swatchSize = GlobalConfig<ColorPaletteManager>.Instance.SwatchSize;
				int swatchSpacing = GlobalConfig<ColorPaletteManager>.Instance.SwatchSpacing;
				if (DrawColorPaletteColorPicker(propertyValueEntry, colorPalette, ref color, colorPalette.ShowAlpha, stretchPalette, swatchSize, 20f, swatchSpacing))
				{
					propertyValueEntry.SmartValue = color;
				}
				if (!flag)
				{
					GUILayout.Space(4f);
					if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
					{
						GlobalConfig<ColorPaletteManager>.Instance.OpenInEditor();
					}
					GUILayout.EndHorizontal();
				}
			}
			SirenixEditorGUI.EndInlineBox();
			EditorGUILayout.EndHorizontal();
			SirenixEditorGUI.EndIndentedHorizontal();
		}

		internal static bool DrawColorPaletteColorPicker(object key, ColorPalette colorPalette, ref Color color, bool drawAlpha, bool stretchPalette, float width = 20f, float height = 20f, float margin = 0f)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Invalid comparison between Unknown and I4
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			bool result = false;
			Rect val = SirenixEditorGUI.BeginHorizontalAutoScrollBox(key, (GUILayoutOption[])GUILayoutOptions.ExpandWidth().ExpandHeight(expand: false));
			if (stretchPalette)
			{
				((Rect)(ref val)).set_width(((Rect)(ref val)).get_width() - (margin * (float)colorPalette.Colors.Count - margin));
				width = Mathf.Max(width, ((Rect)(ref val)).get_width() / (float)colorPalette.Colors.Count);
			}
			bool flag = (int)Event.get_current().get_type() == 0;
			Rect rect = GUILayoutUtility.GetRect((width + margin) * (float)colorPalette.Colors.Count, height, GUIStyle.get_none());
			float num = width + margin;
			Rect val2 = rect;
			((Rect)(ref val2)).set_width(width);
			for (int i = 0; i < colorPalette.Colors.Count; i++)
			{
				((Rect)(ref val2)).set_x(num * (float)i);
				if (drawAlpha)
				{
					EditorGUIUtility.DrawColorSwatch(val2, colorPalette.Colors[i]);
				}
				else
				{
					Color color2 = colorPalette.Colors[i];
					color2.a = 1f;
					SirenixEditorGUI.DrawSolidRect(val2, color2);
				}
				if (flag && ((Rect)(ref val2)).Contains(Event.get_current().get_mousePosition()))
				{
					color = colorPalette.Colors[i];
					result = true;
					GUI.set_changed(true);
					Event.get_current().Use();
				}
			}
			SirenixEditorGUI.EndHorizontalAutoScrollBox();
			return result;
		}
	}
}
