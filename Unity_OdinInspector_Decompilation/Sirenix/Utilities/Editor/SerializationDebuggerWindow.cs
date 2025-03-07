using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// The Odin Inspector Serialization Debugger Window.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public sealed class SerializationDebuggerWindow : OdinEditorWindow
	{
		private const string TargetTypePrefKey = "SerializationDebuggerWindow.TargetType";

		[SerializeField]
		[HideInInspector]
		private Type targetType;

		[SerializeField]
		[HideInInspector]
		private bool odinContext;

		[NonSerialized]
		private OdinMenuTree serializationInfoTree;

		[NonSerialized]
		private SerializationBackendFlags backendFlags;

		[NonSerialized]
		private GUIStyle noteStyle;

		[OnInspectorGUI]
		private void DrawTopBar()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Expected O, but got Unknown
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Invalid comparison between Unknown and I4
			Rect rect = SirenixEditorGUI.BeginHorizontalToolbar();
			Rect rect2 = rect.AlignLeft(20f).AlignMiddle(20f);
			((Rect)(ref rect2)).set_x(((Rect)(ref rect2)).get_x() + 8f);
			GUI.set_color((Color)(((backendFlags & SerializationBackendFlags.Odin) != 0) ? Color.get_white() : new Color(1f, 1f, 1f, 0.2f)));
			GUI.DrawTexture(rect2.Padding(2f), (Texture)(object)EditorIcons.OdinInspectorLogo, (ScaleMode)2);
			((Rect)(ref rect2)).set_x(((Rect)(ref rect2)).get_x() + 28f);
			GUI.set_color((Color)(((backendFlags & SerializationBackendFlags.Unity) != 0) ? Color.get_white() : new Color(1f, 1f, 1f, 0.2f)));
			GUI.DrawTexture(rect2.Padding(2f), (Texture)(object)EditorIcons.UnityLogo, (ScaleMode)2);
			GUI.set_color(Color.get_white());
			string text = "   " + ((targetType == null) ? "Select Type" : targetType.GetNiceName().SplitPascalCase()) + "   ";
			GUILayout.Space(((Rect)(ref rect2)).get_xMax() + 3f);
			bool flag = SirenixEditorGUI.ToolbarButton(new GUIContent(text));
			GUILayout.FlexibleSpace();
			bool flag2 = SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleDown);
			if (flag2 || flag)
			{
				Rect rect3 = GUIHelper.GetCurrentLayoutRect().HorizontalPadding(20f).AlignTop(20f);
				rect3 = rect3.AlignRight(400f);
				IOrderedEnumerable<Type> types2 = from x in AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
					where !x.IsAbstract && x.IsClass && x.InheritsFrom<Object>()
					where !x.Assembly.FullName.StartsWith("Sirenix")
					orderby x.Assembly.GetAssemblyTypeFlag()
					orderby x.Assembly.GetAssemblyTypeFlag(), x.Namespace, x.Name descending
					select x;
				TypeSelector typeSelector = new TypeSelector(types2, supportsMultiSelect: false);
				typeSelector.SelectionChanged += delegate(IEnumerable<Type> types)
				{
					Type type = types.FirstOrDefault();
					if (type != null)
					{
						targetType = type;
						odinContext = targetType.IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(inherit: true);
						CreateMenuTree(force: true);
					}
				};
				typeSelector.SetSelection(targetType);
				typeSelector.ShowInPopup(300f);
				if ((int)Application.get_platform() == 16)
				{
					GUIUtility.ExitGUI();
				}
			}
			SirenixEditorGUI.EndHorizontalToolbar();
		}

		private void CreateMenuTree(bool force)
		{
			if (!force && (targetType == null || serializationInfoTree != null))
			{
				return;
			}
			EditorPrefs.SetString("SerializationDebuggerWindow.TargetType", TwoWaySerializationBinder.Default.BindToName(targetType));
			backendFlags = ((!targetType.IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(inherit: true)) ? SerializationBackendFlags.Unity : SerializationBackendFlags.UnityAndOdin);
			List<MemberSerializationInfo> list = MemberSerializationInfo.CreateSerializationOverview(targetType, backendFlags, odinContext);
			serializationInfoTree = new OdinMenuTree(supportsMultiSelect: false);
			serializationInfoTree.DefaultMenuStyle.Offset = 64f;
			serializationInfoTree.DefaultMenuStyle.Height = 27;
			serializationInfoTree.DefaultMenuStyle.BorderPadding = 0f;
			serializationInfoTree.Config.DrawSearchToolbar = true;
			serializationInfoTree.Config.AutoHandleKeyboardNavigation = true;
			foreach (MemberSerializationInfo item in list)
			{
				serializationInfoTree.MenuItems.Add(new SerializationInfoMenuItem(serializationInfoTree, item.MemberInfo.Name, item));
			}
		}

		[OnInspectorGUI]
		private void DrawSerializationInfoTree()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			EditorGUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.ExpandHeight());
			CreateMenuTree(force: false);
			if (serializationInfoTree != null)
			{
				serializationInfoTree.DrawMenuTree();
			}
			EditorGUILayout.EndVertical();
		}

		[OnInspectorGUI]
		private void DrawInfos()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Expected O, but got Unknown
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			if (serializationInfoTree == null)
			{
				return;
			}
			if (noteStyle == null)
			{
				noteStyle = new GUIStyle(SirenixGUIStyles.MultiLineLabel);
				noteStyle.get_active().set_textColor(noteStyle.get_normal().get_textColor());
				noteStyle.get_onActive().set_textColor(noteStyle.get_normal().get_textColor());
				noteStyle.get_onFocused().set_textColor(noteStyle.get_normal().get_textColor());
				noteStyle.get_focused().set_textColor(noteStyle.get_normal().get_textColor());
				noteStyle.set_margin(new RectOffset(20, 4, 0, 4));
				noteStyle.set_padding(new RectOffset(0, 0, 0, 0));
			}
			if (serializationInfoTree.Selection.Count > 0)
			{
				MemberSerializationInfo memberSerializationInfo = serializationInfoTree.Selection[0].Value as MemberSerializationInfo;
				GUILayout.Space(10f);
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				Rect rect = GUIHelper.GetCurrentLayoutRect().Expand(0f, 10f);
				SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.DarkEditorBackground);
				SirenixEditorGUI.DrawBorders(rect, 0, 0, 1, 0);
				GUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.MinHeight(80f));
				string[] notes = memberSerializationInfo.Notes;
				foreach (string text in notes)
				{
					Rect rect2 = GUILayoutUtility.GetRect(GUIHelper.TempContent(text), noteStyle);
					Rect rect3 = rect2;
					((Rect)(ref rect3)).set_x(((Rect)(ref rect3)).get_x() - 8f);
					((Rect)(ref rect3)).set_y(((Rect)(ref rect3)).get_y() + 5f);
					((Rect)(ref rect3)).set_height(4f);
					((Rect)(ref rect3)).set_width(4f);
					SirenixEditorGUI.DrawSolidRect(rect3, EditorGUIUtility.get_isProSkin() ? Color.get_white() : Color.get_black());
					EditorGUI.SelectableLabel(rect2, text, noteStyle);
					GUILayout.Space(4f);
				}
				GUILayout.EndVertical();
				Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
				SirenixEditorGUI.DrawVerticalLineSeperator(((Rect)(ref currentLayoutRect)).get_x(), ((Rect)(ref currentLayoutRect)).get_y(), ((Rect)(ref currentLayoutRect)).get_height());
				GUILayout.EndHorizontal();
				GUILayout.Space(10f);
			}
		}

		[OnInspectorGUI]
		private void DrawGettingStartedHelp()
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			if (targetType == null)
			{
				GUIContent val = GUIHelper.TempContent("Select your script here to begin debugging the serialization.", (Texture)(object)EditorIcons.UnityInfoIcon);
				Rect position = ((EditorWindow)this).get_position();
				Vector2 val2 = default(Vector2);
				((Vector2)(ref val2))._002Ector(Mathf.Max(((Rect)(ref position)).get_width() - 100f, 200f), 0f);
				val2.y = SirenixGUIStyles.MessageBox.CalcHeight(val, val2.x);
				GUI.Label(new Rect(50f, 40f, val2.x, val2.y), val, SirenixGUIStyles.MessageBox);
				EditorIcons.ArrowUp.Draw(new Rect(95f, 25f, 20f, 20f), (Texture)(object)EditorIcons.ArrowUp.Raw);
			}
		}

		/// <summary>
		/// Opens the Serialization Debugger Window with the last debugged type.
		/// </summary>
		public static void ShowWindow()
		{
			Type type = null;
			string @string = EditorPrefs.GetString("SerializationDebuggerWindow.TargetType", (string)null);
			if (@string != null)
			{
				type = TwoWaySerializationBinder.Default.BindToType(@string);
			}
			ShowWindow(type);
		}

		/// <summary>
		/// Opens the Serialization Debugger Window and debugs the given type.
		/// </summary>
		/// <param name="type">The type to debug serialization of.</param>
		public static void ShowWindow(Type type)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			SerializationDebuggerWindow serializationDebuggerWindow = Resources.FindObjectsOfTypeAll<SerializationDebuggerWindow>().FirstOrDefault();
			if ((Object)(object)serializationDebuggerWindow == (Object)null)
			{
				serializationDebuggerWindow = EditorWindow.GetWindow<SerializationDebuggerWindow>("Serialization Debugger");
				((EditorWindow)serializationDebuggerWindow).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(500f, 400f));
			}
			serializationDebuggerWindow.targetType = type;
			((EditorWindow)serializationDebuggerWindow).Show();
			if (serializationDebuggerWindow.targetType != null)
			{
				serializationDebuggerWindow.CreateMenuTree(force: true);
				((EditorWindow)serializationDebuggerWindow).Repaint();
			}
		}

		private static void ComponentContextMenuItem(MenuCommand menuCommand)
		{
			ShowWindow(((object)menuCommand.context).GetType());
		}

		/// <summary>
		/// Initializes the Serialization Debugger Window.
		/// </summary>
		protected override void OnEnable()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnable();
			WindowPadding = default(Vector4);
			((EditorWindow)this).set_minSize(new Vector2(300f, 300f));
		}
	}
}
