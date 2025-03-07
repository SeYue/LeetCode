using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public class OdinAttributeExampleItem
	{
		private static GUIStyle headerGroupStyle;

		private static GUIStyle tabGroupStyle;

		private static Color backgroundColor = Color32.op_Implicit(new Color32((byte)195, (byte)195, (byte)195, byte.MaxValue));

		private Type attributeType;

		private OdinRegisterAttributeAttribute registration;

		private AttributeExamplePreview[] examples;

		private GUITabGroup tabGroup;

		public readonly string Name;

		public bool DrawCodeExample { get; set; }

		public OdinAttributeExampleItem(Type attributeType, OdinRegisterAttributeAttribute registration)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			this.attributeType = attributeType;
			this.registration = registration;
			Name = this.attributeType.GetNiceName().SplitPascalCase();
			DrawCodeExample = true;
			AttributeExampleInfo[] attributeExampleInfos = AttributeExampleUtilities.GetAttributeExampleInfos(attributeType);
			examples = new AttributeExamplePreview[attributeExampleInfos.Length];
			for (int i = 0; i < attributeExampleInfos.Length; i++)
			{
				examples[i] = new AttributeExamplePreview(attributeExampleInfos[i]);
			}
			tabGroup = new GUITabGroup
			{
				ToolbarHeight = 30f
			};
			for (int j = 0; j < attributeExampleInfos.Length; j++)
			{
				tabGroup.RegisterTab(attributeExampleInfos[j].Name);
			}
		}

		[OnInspectorGUI]
		public void Draw()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			object obj = headerGroupStyle;
			if (obj == null)
			{
				GUIStyle val = new GUIStyle();
				obj = (object)val;
				val.set_padding(new RectOffset(4, 6, 10, 4));
			}
			headerGroupStyle = (GUIStyle)obj;
			object obj2 = tabGroupStyle;
			if (obj2 == null)
			{
				GUIStyle val2 = new GUIStyle(SirenixGUIStyles.BoxContainer);
				obj2 = (object)val2;
				val2.set_padding(new RectOffset(0, 0, 0, 0));
			}
			tabGroupStyle = (GUIStyle)obj2;
			GUILayout.BeginVertical(headerGroupStyle, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.Label(Name, SirenixGUIStyles.SectionHeader, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (!string.IsNullOrEmpty(registration.DocumentationUrl))
			{
				Rect val3 = GUILayoutUtility.GetLastRect().AlignCenterY(20f).AlignRight(120f);
				if (GUI.Button(val3, "Documentation", SirenixGUIStyles.MiniButton))
				{
					Help.BrowseURL(registration.DocumentationUrl);
				}
			}
			SirenixEditorGUI.DrawThickHorizontalSeparator(4f, 10f);
			if (!string.IsNullOrEmpty(registration.Description))
			{
				GUILayout.Label(registration.Description, SirenixGUIStyles.MultiLineLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				SirenixEditorGUI.DrawThickHorizontalSeparator(10f, 10f);
			}
			if (examples.Length != 0)
			{
				Color val4 = GUI.get_backgroundColor();
				GUI.set_backgroundColor(backgroundColor);
				tabGroup.BeginGroup(drawToolbar: true, tabGroupStyle);
				GUI.set_backgroundColor(val4);
				AttributeExamplePreview[] array = examples;
				foreach (AttributeExamplePreview attributeExamplePreview in array)
				{
					GUITabPage gUITabPage = tabGroup.RegisterTab(attributeExamplePreview.ExampleInfo.Name);
					if (gUITabPage.BeginPage())
					{
						attributeExamplePreview.Draw(DrawCodeExample);
					}
					gUITabPage.EndPage();
				}
				tabGroup.EndGroup();
			}
			else
			{
				GUILayout.Label("No examples available.", (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			GUILayout.EndVertical();
		}

		public void OnDeselected()
		{
			AttributeExamplePreview[] array = examples;
			foreach (AttributeExamplePreview attributeExamplePreview in array)
			{
				attributeExamplePreview.OnDeselected();
			}
		}
	}
}
