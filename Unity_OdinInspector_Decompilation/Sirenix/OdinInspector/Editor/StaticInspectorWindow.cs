using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Access the StaticInspectorWindow from Tools &gt; Odin Inspector &gt; Static Inspector.
	/// </summary>
	public class StaticInspectorWindow : OdinEditorWindow
	{
		/// <summary>
		/// Member filter for access modifiers.
		/// </summary>
		[Flags]
		public enum AccessModifierFlags
		{
			/// <summary>
			/// include public members.
			/// </summary>
			Public = 0x2,
			/// <summary>
			/// Include Non-public members.
			/// </summary>
			Private = 0x4,
			/// <summary>
			/// Include both public and non-public members.
			/// </summary>
			All = 0x6
		}

		/// <summary>
		/// Member filter for member types.
		/// </summary>
		[Flags]
		public enum MemberTypeFlags
		{
			/// <summary>
			/// No members included.
			/// </summary>
			None = 0x0,
			/// <summary>
			/// Include field members.
			/// </summary>
			Fields = 0x1,
			/// <summary>
			/// Include property members.
			/// </summary>
			Properties = 0x2,
			/// <summary>
			/// Include method members.
			/// </summary>
			Methods = 0x4,
			/// <summary>
			/// Include group members.
			/// </summary>
			Groups = 0x8,
			/// <summary>
			/// Include members from the base types.
			/// </summary>
			BaseTypeMembers = 0x10,
			/// <summary>
			/// Include members marked with the Obsolete attribute.
			/// </summary>
			Obsolete = 0x20,
			/// <summary>
			/// Include all members except members marked with the Obsolete attribute.
			/// </summary>
			AllButObsolete = 0x1F
		}

		private static GUIStyle btnStyle;

		private const string TargetTypeFlagsPrefKey = "OdinStaticInspectorWindow.TargetTypeFlags";

		private const string MemberTypeFlagsPrefKey = "OdinStaticInspectorWindow.MemberTypeFlags";

		private const string AccessModifierFlagsPrefKey = "OdinStaticInspectorWindow.AccessModifierFlags";

		[SerializeField]
		[HideInInspector]
		private Type targetType;

		[SerializeField]
		[HideInInspector]
		private AssemblyTypeFlags targetTypeFlags;

		[SerializeField]
		[HideInInspector]
		private MemberTypeFlags memberTypes;

		[SerializeField]
		[HideInInspector]
		private AccessModifierFlags accessModifiers;

		[SerializeField]
		[HideInInspector]
		private string showMemberNameFilter;

		[SerializeField]
		[HideInInspector]
		private string searchFilter;

		[NonSerialized]
		private PropertyTree tree;

		[NonSerialized]
		private AccessModifierFlags currAccessModifiers;

		[NonSerialized]
		private MemberTypeFlags currMemberTypes;

		[NonSerialized]
		private int focusSearch;

		/// <summary>
		/// Shows the window.
		/// </summary>
		public static void ShowWindow()
		{
			InspectType(null);
		}

		/// <summary>
		/// Opens a new static inspector window for the given type.
		/// </summary>
		public static StaticInspectorWindow InspectType(Type type, AccessModifierFlags? accessModifies = null, MemberTypeFlags? memberTypeFlags = null)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			StaticInspectorWindow staticInspectorWindow = ScriptableObject.CreateInstance<StaticInspectorWindow>();
			((EditorWindow)staticInspectorWindow).set_titleContent(new GUIContent("Static Inspector", EditorIcons.MagnifyingGlass.Highlighted));
			((EditorWindow)staticInspectorWindow).set_position(GUIHelper.GetEditorWindowRect().AlignCenter(700f, 400f));
			staticInspectorWindow.targetTypeFlags = (AssemblyTypeFlags)EditorPrefs.GetInt("OdinStaticInspectorWindow.TargetTypeFlags", 55);
			if (accessModifies.HasValue)
			{
				staticInspectorWindow.accessModifiers = accessModifies.Value;
			}
			else
			{
				staticInspectorWindow.accessModifiers = (AccessModifierFlags)EditorPrefs.GetInt("OdinStaticInspectorWindow.AccessModifierFlags", 6);
			}
			if (memberTypeFlags.HasValue)
			{
				staticInspectorWindow.memberTypes = memberTypeFlags.Value;
			}
			else
			{
				staticInspectorWindow.memberTypes = (MemberTypeFlags)EditorPrefs.GetInt("OdinStaticInspectorWindow.MemberTypeFlags", 15);
			}
			staticInspectorWindow.currMemberTypes = staticInspectorWindow.memberTypes;
			staticInspectorWindow.currAccessModifiers = staticInspectorWindow.accessModifiers;
			staticInspectorWindow.focusSearch = 0;
			staticInspectorWindow.targetType = type;
			((EditorWindow)staticInspectorWindow).Show();
			if (type != null)
			{
				((EditorWindow)staticInspectorWindow).set_titleContent(new GUIContent(type.GetNiceName()));
			}
			((EditorWindow)staticInspectorWindow).Repaint();
			return staticInspectorWindow;
		}

		private OdinSelector<Type> SelectType(Rect arg)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			TypeSelector typeSelector = new TypeSelector(targetTypeFlags, supportsMultiSelect: false);
			typeSelector.SelectionChanged += delegate(IEnumerable<Type> types)
			{
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Expected O, but got Unknown
				focusSearch = 0;
				Type type = types.FirstOrDefault();
				if (type != null)
				{
					targetType = type;
					((EditorWindow)this).set_titleContent(new GUIContent(targetType.GetNiceName()));
				}
			};
			typeSelector.SetSelection(targetType);
			((OdinSelector<Type>)typeSelector).ShowInPopup(new Rect(-300f, 0f, 300f, 0f));
			return typeSelector;
		}

		/// <summary>
		/// Draws the Odin Editor Window.
		/// </summary>
		protected override void OnGUI()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			btnStyle = (GUIStyle)(((object)btnStyle) ?? ((object)new GUIStyle(EditorStyles.get_toolbarDropDown())));
			btnStyle.set_fixedHeight(21f);
			btnStyle.set_stretchHeight(false);
			DrawFirstToolbar();
			if (targetType != null)
			{
				DrawSecondToolbar();
			}
			base.OnGUI();
		}

		private void DrawFirstToolbar()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Invalid comparison between Unknown and I4
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			GUILayout.Space(1f);
			string text = "       " + ((targetType == null) ? "Select Type" : targetType.GetNiceFullName()) + "   ";
			Rect rect = GUILayoutUtility.GetRect(0f, 21f, SirenixGUIStyles.ToolbarBackground);
			Rect rect2 = rect.AlignRight(80f);
			Rect rect3 = rect.SetXMax(((Rect)(ref rect2)).get_xMin());
			OdinSelector<Type>.DrawSelectorDropdown(rect3, new GUIContent(text), SelectType, btnStyle);
			EditorGUI.BeginChangeCheck();
			targetTypeFlags = EnumSelector<AssemblyTypeFlags>.DrawEnumField(rect2, null, new GUIContent("Type Filter"), targetTypeFlags, btnStyle);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetInt("OdinStaticInspectorWindow.TargetTypeFlags", (int)targetTypeFlags);
			}
			if ((int)Event.get_current().get_type() == 7)
			{
				Texture2D assetThumbnail = GUIHelper.GetAssetThumbnail(null, targetType ?? typeof(int), preferObjectPreviewOverFileIcon: false);
				if ((Object)(object)assetThumbnail != (Object)null)
				{
					((Rect)(ref rect3)).set_x(((Rect)(ref rect3)).get_x() + 8f);
					GUI.DrawTexture(rect3.AlignLeft(16f).AlignMiddle(16f), (Texture)(object)assetThumbnail, (ScaleMode)2);
				}
			}
		}

		private void DrawSecondToolbar()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			Rect rect = GUILayoutUtility.GetRect(0f, 21f);
			if ((int)Event.get_current().get_type() == 7)
			{
				SirenixGUIStyles.ToolbarBackground.Draw(rect, GUIContent.none, 0);
				SirenixEditorGUI.DrawBorders(rect, 0, 0, 0, 1);
			}
			Rect rect2 = rect.AlignRight(80f);
			Rect rect3 = rect2.SubX(100f).SetWidth(100f);
			Rect rect4 = rect.SetXMax(((Rect)(ref rect3)).get_xMin());
			EditorGUI.BeginChangeCheck();
			memberTypes = EnumSelector<MemberTypeFlags>.DrawEnumField(rect3, null, memberTypes, btnStyle);
			accessModifiers = EnumSelector<AccessModifierFlags>.DrawEnumField(rect2, null, accessModifiers, btnStyle);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetInt("OdinStaticInspectorWindow.AccessModifierFlags", (int)accessModifiers);
				EditorPrefs.SetInt("OdinStaticInspectorWindow.MemberTypeFlags", (int)memberTypes);
			}
			DrawSearchField(rect4);
		}

		private void DrawSearchField(Rect rect)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			rect = rect.HorizontalPadding(5f).AlignMiddle(16f);
			((Rect)(ref rect)).set_xMin(((Rect)(ref rect)).get_xMin() + 3f);
			searchFilter = SirenixEditorGUI.SearchField(rect, searchFilter, focusSearch++ < 4, "SirenixSearchField" + ((Object)this).GetInstanceID());
		}

		/// <summary>
		/// Draws the editor for the this.CurrentDrawingTargets[index].
		/// </summary>
		protected override void DrawEditor(int index)
		{
			DrawGettingStartedHelp();
			DrawTree();
			GUILayout.FlexibleSpace();
		}

		private void DrawGettingStartedHelp()
		{
			if (targetType == null)
			{
				SirenixEditorGUI.InfoMessageBox("Select a type here to begin static inspection.");
			}
		}

		private void DrawTree()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Expected O, but got Unknown
			if (targetType == null)
			{
				tree = null;
				return;
			}
			if ((int)Event.get_current().get_type() == 8)
			{
				currMemberTypes = memberTypes;
				currAccessModifiers = accessModifiers;
			}
			if (tree == null || tree.TargetType != targetType)
			{
				if (targetType.IsGenericType && !targetType.IsFullyConstructedGenericType())
				{
					SirenixEditorGUI.ErrorMessageBox("Cannot statically inspect generic type definitions");
					return;
				}
				tree = PropertyTree.CreateStatic(targetType);
			}
			bool flag = (currMemberTypes & MemberTypeFlags.Obsolete) == MemberTypeFlags.Obsolete;
			PropertyContext<bool> global = tree.RootProperty.Context.GetGlobal("ALLOW_OBSOLETE_STATIC_MEMBERS", defaultValue: false);
			if (global.Value != flag)
			{
				global.Value = flag;
				tree.RootProperty.RefreshSetup();
			}
			tree.BeginDraw(withUndo: false);
			bool flag2 = true;
			if (tree.AllowSearchFiltering && tree.RootProperty.Attributes.HasAttribute<SearchableAttribute>())
			{
				SearchableAttribute attribute = tree.RootProperty.GetAttribute<SearchableAttribute>();
				if (attribute.Recursive)
				{
					SirenixEditorGUI.WarningMessageBox("This type has been marked as recursively searchable. Be *CAREFUL* with using this search - recursively searching a static inspector can be *very dangerous* and can lead to freezes, crashes or other nasty errors if the static inspector search ends up recursing deeply into, for example, the .NET runtime internals, which would result in recursively searching through hundreds of thousands to millions of internal properties.");
				}
				if (tree.DrawSearch())
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				foreach (InspectorProperty item in tree.EnumerateTree(includeChildren: false))
				{
					if (DrawProperty(item))
					{
						if (item.Info.PropertyType != PropertyType.Group && item.Info.GetMemberInfo() != null && item.Info.GetMemberInfo().DeclaringType != targetType)
						{
							item.Draw(new GUIContent(item.Info.GetMemberInfo().DeclaringType.GetNiceName() + " -> " + item.NiceName));
						}
						else
						{
							item.Draw();
						}
					}
					else
					{
						item.Update();
					}
				}
			}
			tree.EndDraw();
		}

		private bool DrawProperty(InspectorProperty property)
		{
			if (!string.IsNullOrEmpty(searchFilter) && !property.NiceName.Replace(" ", "").Contains(searchFilter.Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase))
			{
				return false;
			}
			if (property.Info.PropertyType == PropertyType.Group)
			{
				return (currMemberTypes & MemberTypeFlags.Groups) == MemberTypeFlags.Groups;
			}
			MemberInfo memberInfo = property.Info.GetMemberInfo();
			if (memberInfo != null)
			{
				if ((currMemberTypes & MemberTypeFlags.BaseTypeMembers) != MemberTypeFlags.BaseTypeMembers && memberInfo.DeclaringType != null && memberInfo.DeclaringType != targetType)
				{
					return false;
				}
				bool flag = (currAccessModifiers & AccessModifierFlags.Public) == AccessModifierFlags.Public;
				bool flag2 = (currAccessModifiers & AccessModifierFlags.Private) == AccessModifierFlags.Private;
				bool flag3 = (currMemberTypes & MemberTypeFlags.Fields) == MemberTypeFlags.Fields;
				bool flag4 = (currMemberTypes & MemberTypeFlags.Properties) == MemberTypeFlags.Properties;
				if (!flag || !flag2)
				{
					bool flag5 = true;
					FieldInfo fieldInfo = memberInfo as FieldInfo;
					PropertyInfo propertyInfo = memberInfo as PropertyInfo;
					MethodInfo methodInfo = memberInfo as MethodInfo;
					if (fieldInfo != null)
					{
						flag5 = fieldInfo.IsPublic;
					}
					else if (propertyInfo != null)
					{
						flag5 = propertyInfo.GetGetMethod()?.IsPublic ?? false;
					}
					else if (methodInfo != null)
					{
						flag5 = methodInfo.IsPublic;
					}
					if (flag5 && !flag)
					{
						return false;
					}
					if (!flag5 && !flag2)
					{
						return false;
					}
				}
				if (memberInfo is FieldInfo && !flag3)
				{
					return false;
				}
				if (memberInfo is PropertyInfo && !flag4)
				{
					return false;
				}
			}
			if (property.Info.PropertyType == PropertyType.Method && (currMemberTypes & MemberTypeFlags.Methods) != MemberTypeFlags.Methods)
			{
				return false;
			}
			return true;
		}
	}
}
