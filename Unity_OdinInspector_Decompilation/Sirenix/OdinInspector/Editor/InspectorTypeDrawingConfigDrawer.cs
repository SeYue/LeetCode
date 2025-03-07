using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Draws an <see cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" /> instance, and contains methods getting all types that should be drawn by Odin.</para>
	/// <para>Note that this class keeps a lot of static state, and is only intended to draw the instance of <see cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" /> that exists in the <see cref="T:Sirenix.OdinInspector.Editor.InspectorConfig" /> singleton asset. If used to draw other instances, odd behaviour may occur.</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" />.
	/// <seealso cref="!:EditorCompilation" />.
	public class InspectorTypeDrawingConfigDrawer : OdinValueDrawer<InspectorTypeDrawingConfig>
	{
		private enum DisplayType
		{
			AllUnityObjects,
			AllComponents,
			AllScriptableObjects,
			UserScripts
		}

		private class TypeGroup
		{
			public class TypePair
			{
				public readonly Type DrawnType;

				public readonly Type PreExistingEditorType;

				public TypePair(Type drawnType, Type preExistingEditorType)
				{
					DrawnType = drawnType;
					PreExistingEditorType = preExistingEditorType;
				}
			}

			public readonly string Name;

			public readonly List<TypeGroup> SubGroups = new List<TypeGroup>();

			public readonly List<TypePair> SubTypes = new List<TypePair>();

			private readonly List<bool> SubTypesVisible = new List<bool>();

			public bool HasConflict { get; private set; }

			public bool AllSubTypesVisible { get; private set; }

			public bool IsSearchVisible { get; private set; }

			public bool IsExpanded { get; set; }

			public bool HasEligibleTypes { get; private set; }

			public TypeGroup(string name)
			{
				Name = name;
				HasEligibleTypes = false;
			}

			public TypeGroup GetChildGroup(string name)
			{
				for (int i = 0; i < SubGroups.Count; i++)
				{
					if (SubGroups[i].Name == name)
					{
						return SubGroups[i];
					}
				}
				TypeGroup typeGroup = new TypeGroup(name);
				SubGroups.Add(typeGroup);
				return typeGroup;
			}

			public void ExpandAll()
			{
				IsExpanded = true;
				for (int i = 0; i < SubGroups.Count; i++)
				{
					SubGroups[i].ExpandAll();
				}
			}

			public void SetSharedEditorType(Type editorType)
			{
				HasConflict = false;
				for (int i = 0; i < SubTypes.Count; i++)
				{
					if (SubTypes[i].PreExistingEditorType == null)
					{
						GlobalConfig<InspectorConfig>.Instance.DrawingConfig.SetEditorType(SubTypes[i].DrawnType, editorType);
					}
				}
				for (int j = 0; j < SubGroups.Count; j++)
				{
					SubGroups[j].SetSharedEditorType(editorType);
				}
			}

			public void ClearEditorTypes()
			{
				HasConflict = false;
				for (int i = 0; i < SubTypes.Count; i++)
				{
					GlobalConfig<InspectorConfig>.Instance.DrawingConfig.ClearEditorEntryForDrawnType(SubTypes[i].DrawnType);
				}
				for (int j = 0; j < SubGroups.Count; j++)
				{
					SubGroups[j].ClearEditorTypes();
				}
			}

			public Type GetSharedEditorType()
			{
				if (HasConflict)
				{
					return null;
				}
				if (SubTypes.Count > 0)
				{
					for (int i = 0; i < SubTypes.Count; i++)
					{
						if (SubTypes[i].PreExistingEditorType == null)
						{
							return GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(SubTypes[i].DrawnType);
						}
					}
				}
				for (int j = 0; j < SubGroups.Count; j++)
				{
					Type sharedEditorType = SubGroups[j].GetSharedEditorType();
					if (sharedEditorType != null)
					{
						return sharedEditorType;
					}
				}
				return null;
			}

			public void UpdateHasEligibleTypes()
			{
				HasEligibleTypes = false;
				for (int i = 0; i < SubGroups.Count; i++)
				{
					SubGroups[i].UpdateHasEligibleTypes();
					if (SubGroups[i].HasEligibleTypes)
					{
						HasEligibleTypes = true;
					}
				}
				for (int j = 0; j < SubTypes.Count; j++)
				{
					if (SubTypes[j].PreExistingEditorType == null)
					{
						HasEligibleTypes = true;
						break;
					}
				}
			}

			public void UpdateConflicts()
			{
				HasConflict = false;
				for (int i = 0; i < SubGroups.Count; i++)
				{
					SubGroups[i].UpdateConflicts();
					if (SubGroups[i].HasConflict)
					{
						HasConflict = true;
					}
				}
				Type type = null;
				if (!HasConflict)
				{
					for (int j = 0; j < SubTypes.Count; j++)
					{
						if (SubTypes[j].PreExistingEditorType == null)
						{
							type = GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(SubTypes[j].DrawnType);
						}
					}
					for (int k = 0; k < SubTypes.Count; k++)
					{
						if (SubTypes[k].PreExistingEditorType == null && GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(SubTypes[k].DrawnType) != type)
						{
							HasConflict = true;
							break;
						}
					}
				}
				if (HasConflict || SubGroups.Count <= 0)
				{
					return;
				}
				Type type2 = null;
				for (int l = 0; l < SubGroups.Count; l++)
				{
					if (SubGroups[l].HasEligibleTypes)
					{
						type2 = SubGroups[l].GetSharedEditorType();
						break;
					}
				}
				bool flag = SubTypes.Count > 0;
				if (flag && type2 != type)
				{
					HasConflict = true;
				}
				if (HasConflict)
				{
					return;
				}
				for (int m = 0; m < SubGroups.Count; m++)
				{
					if (SubGroups[m].HasEligibleTypes)
					{
						Type sharedEditorType = SubGroups[m].GetSharedEditorType();
						if ((flag && sharedEditorType != type) || sharedEditorType != type2)
						{
							HasConflict = true;
							break;
						}
					}
				}
			}

			public void Sort()
			{
				SubGroups.Sort((TypeGroup a, TypeGroup b) => a.Name.CompareTo(b.Name));
				SubTypes.Sort((TypePair a, TypePair b) => a.DrawnType.Name.CompareTo(b.DrawnType.Name));
				foreach (TypeGroup subGroup in SubGroups)
				{
					subGroup.Sort();
				}
			}

			public bool IsTypeVisible(Type type)
			{
				for (int i = 0; i < SubTypes.Count; i++)
				{
					if (SubTypes[i].DrawnType == type)
					{
						if (SubTypesVisible.Count > i)
						{
							return SubTypesVisible[i];
						}
						return false;
					}
				}
				return false;
			}

			public void UpdateSearch(string search, DisplayType displayType)
			{
				IsSearchVisible = false;
				AllSubTypesVisible = true;
				SubTypesVisible.SetLength(SubTypes.Count);
				foreach (TypeGroup subGroup in SubGroups)
				{
					subGroup.UpdateSearch(search, displayType);
					if (subGroup.IsSearchVisible)
					{
						IsSearchVisible = true;
					}
					if (!subGroup.AllSubTypesVisible)
					{
						AllSubTypesVisible = false;
					}
				}
				bool flag = search.IsNullOrWhitespace();
				if (flag && displayType == DisplayType.AllUnityObjects)
				{
					IsSearchVisible = true;
					AllSubTypesVisible = true;
					for (int i = 0; i < SubTypesVisible.Count; i++)
					{
						SubTypesVisible[i] = true;
					}
					return;
				}
				for (int j = 0; j < SubTypes.Count; j++)
				{
					Type drawnType = SubTypes[j].DrawnType;
					if ((displayType == DisplayType.AllScriptableObjects && !typeof(ScriptableObject).IsAssignableFrom(drawnType)) || (displayType == DisplayType.AllComponents && !typeof(Component).IsAssignableFrom(drawnType)) || (displayType == DisplayType.UserScripts && (drawnType.Assembly.GetAssemblyTypeFlag() & AssemblyTypeFlags.CustomTypes) == 0))
					{
						SubTypesVisible[j] = false;
						AllSubTypesVisible = false;
					}
					else if (flag || drawnType.FullName.Contains(search, StringComparison.InvariantCultureIgnoreCase))
					{
						IsSearchVisible = true;
						SubTypesVisible[j] = true;
					}
					else
					{
						SubTypesVisible[j] = false;
						AllSubTypesVisible = false;
					}
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct ProfileSection : IDisposable
		{
			public static ProfileSection Start(string name)
			{
				return default(ProfileSection);
			}

			public void Dispose()
			{
			}
		}

		private static readonly HashSet<Type> NeverDrawTypes;

		private static bool initializedForDrawing;

		private static readonly TypeGroup UserTypesRootGroup;

		private static readonly TypeGroup PluginTypesRootGroup;

		private static readonly TypeGroup UnityTypesRootGroup;

		private static readonly TypeGroup OtherTypesRootGroup;

		private static readonly List<Type> PossibleEditorTypes;

		private static readonly HashSet<Type> PossibleDrawnTypes;

		private static GUIStyle iconStyle;

		private string searchText;

		private DisplayType displayType;

		private Vector2 scrollPos;

		private static GUIStyle IconStyle
		{
			get
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				//IL_0020: Expected O, but got Unknown
				if (iconStyle == null)
				{
					GUIStyle val = new GUIStyle();
					val.set_margin(new RectOffset(5, 0, 4, 0));
					iconStyle = val;
				}
				return iconStyle;
			}
		}

		static InspectorTypeDrawingConfigDrawer()
		{
			initializedForDrawing = false;
			UserTypesRootGroup = new TypeGroup("User Types");
			PluginTypesRootGroup = new TypeGroup("Plugin Types");
			UnityTypesRootGroup = new TypeGroup("Unity Types");
			OtherTypesRootGroup = new TypeGroup("Other Types");
			PossibleEditorTypes = new List<Type>();
			PossibleDrawnTypes = new HashSet<Type>();
			using (ProfileSection.Start("InspectorTypeDrawingConfigDrawer static constructor"))
			{
				NeverDrawTypes = new HashSet<Type>(FastTypeComparer.Instance);
				Type type = typeof(Object).Assembly.GetType("UnityEngine.NetworkView");
				if (type != null)
				{
					NeverDrawTypes.Add(type);
				}
				Type type2 = typeof(Object).Assembly.GetType("UnityEngine.GUIText");
				if (type2 != null)
				{
					NeverDrawTypes.Add(type2);
				}
				List<Type> list;
				using (ProfileSection.Start("Finding all UnityObject types"))
				{
					if (UnityTypeCacheUtility.IsAvailable)
					{
						IList<Type> typesDerivedFrom = UnityTypeCacheUtility.GetTypesDerivedFrom(typeof(Object));
						list = new List<Type>(typesDerivedFrom.Count);
						foreach (Type item in typesDerivedFrom)
						{
							if (!item.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) && !item.IsDefined(typeof(ObsoleteAttribute), inherit: false) && !NeverDrawTypes.Contains(item) && !typeof(Joint).IsAssignableFrom(item))
							{
								list.Add(item);
							}
						}
					}
					else
					{
						list = new List<Type>(1024);
						Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
						foreach (Assembly assembly in assemblies)
						{
							if (assembly.IsDynamic())
							{
								continue;
							}
							Type[] types = assembly.GetTypes();
							Type[] array = types;
							foreach (Type type3 in array)
							{
								if (typeof(Object).IsAssignableFrom(type3) && !type3.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) && !type3.IsDefined(typeof(ObsoleteAttribute), inherit: false) && !NeverDrawTypes.Contains(type3) && !typeof(Joint).IsAssignableFrom(type3))
								{
									list.Add(type3);
								}
							}
						}
					}
				}
				Dictionary<Type, Type> dictionary = new Dictionary<Type, Type>(FastTypeComparer.Instance);
				Dictionary<Type, Type> dictionary2 = new Dictionary<Type, Type>(FastTypeComparer.Instance);
				using (ProfileSection.Start("Search for editors"))
				{
					foreach (Type item2 in list)
					{
						if (!typeof(Editor).IsAssignableFrom(item2))
						{
							continue;
						}
						try
						{
							bool editorForChildClasses;
							Type editorDrawnType = InspectorTypeDrawingConfig.GetEditorDrawnType(item2, out editorForChildClasses);
							if (editorDrawnType != null)
							{
								if (!dictionary.ContainsKey(editorDrawnType))
								{
									dictionary.Add(editorDrawnType, item2);
								}
								if (editorForChildClasses && !dictionary2.ContainsKey(editorDrawnType))
								{
									dictionary2.Add(editorDrawnType, item2);
								}
							}
							if (InspectorTypeDrawingConfig.UnityInspectorEditorIsValidBase(item2, null))
							{
								PossibleEditorTypes.Add(item2);
							}
						}
						catch (TypeLoadException)
						{
						}
						catch (ReflectionTypeLoadException)
						{
						}
					}
				}
				using (ProfileSection.Start("Assign editors to Unity objects"))
				{
					HashSet<Type> hashSet = new HashSet<Type>(FastTypeComparer.Instance)
					{
						typeof(object),
						typeof(Component),
						typeof(Behaviour),
						typeof(MonoBehaviour),
						typeof(Object),
						typeof(ScriptableObject),
						typeof(StateMachineBehaviour)
					};
					if (UnityNetworkingUtility.NetworkBehaviourType != null)
					{
						hashSet.Add(UnityNetworkingUtility.NetworkBehaviourType);
					}
					foreach (Type item3 in list)
					{
						if (item3.IsAbstract || item3.IsGenericTypeDefinition || item3.IsGenericType || typeof(Editor).IsAssignableFrom(item3) || typeof(EditorWindow).IsAssignableFrom(item3))
						{
							continue;
						}
						Type value;
						bool flag = dictionary.TryGetValue(item3, out value);
						if (!flag)
						{
							Type baseType = item3.BaseType;
							while (baseType != null && !hashSet.Contains(baseType))
							{
								if (dictionary2.TryGetValue(baseType, out var value2))
								{
									flag = true;
									value = value2;
									break;
								}
								baseType = baseType.BaseType;
							}
						}
						if (!flag)
						{
							PossibleDrawnTypes.Add(item3);
						}
						AddTypeToGroups(item3, value);
					}
				}
				using (ProfileSection.Start("Remove non-eligible editor entries"))
				{
					bool flag2 = false;
					foreach (Type allDrawnTypesWithEntry in GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetAllDrawnTypesWithEntries())
					{
						if (!PossibleDrawnTypes.Contains(allDrawnTypesWithEntry))
						{
							GlobalConfig<InspectorConfig>.Instance.DrawingConfig.ClearEditorEntryForDrawnType(allDrawnTypesWithEntry);
							flag2 = true;
						}
					}
					if (flag2)
					{
						AssetDatabase.SaveAssets();
					}
				}
			}
		}

		internal static void UpdateRootGroupHasEligibletypes()
		{
			UserTypesRootGroup.UpdateHasEligibleTypes();
			PluginTypesRootGroup.UpdateHasEligibleTypes();
			UnityTypesRootGroup.UpdateHasEligibleTypes();
			OtherTypesRootGroup.UpdateHasEligibleTypes();
		}

		internal static void UpdateRootGroupConflicts()
		{
			UserTypesRootGroup.UpdateConflicts();
			PluginTypesRootGroup.UpdateConflicts();
			UnityTypesRootGroup.UpdateConflicts();
			OtherTypesRootGroup.UpdateConflicts();
		}

		private static void SortRootGroups()
		{
			UserTypesRootGroup.Sort();
			PluginTypesRootGroup.Sort();
			UnityTypesRootGroup.Sort();
			OtherTypesRootGroup.Sort();
		}

		private static void UpdateRootGroupsSearch(string search, DisplayType displayType)
		{
			UserTypesRootGroup.UpdateSearch(search, displayType);
			PluginTypesRootGroup.UpdateSearch(search, displayType);
			UnityTypesRootGroup.UpdateSearch(search, displayType);
			OtherTypesRootGroup.UpdateSearch(search, displayType);
		}

		private static void AddTypeToGroups(Type type, Type preExistingEditorType)
		{
			TypeGroup typeGroup;
			switch (type.Assembly.GetAssemblyTypeFlag())
			{
			case AssemblyTypeFlags.UserTypes:
			case AssemblyTypeFlags.UserEditorTypes:
				typeGroup = UserTypesRootGroup;
				break;
			case AssemblyTypeFlags.PluginTypes:
			case AssemblyTypeFlags.PluginEditorTypes:
				typeGroup = PluginTypesRootGroup;
				break;
			case AssemblyTypeFlags.UnityTypes:
			case AssemblyTypeFlags.UnityEditorTypes:
				typeGroup = UnityTypesRootGroup;
				break;
			default:
				typeGroup = OtherTypesRootGroup;
				break;
			}
			if (type.Namespace != null)
			{
				string[] array = type.Namespace.Split('.');
				for (int i = 0; i < array.Length; i++)
				{
					typeGroup = typeGroup.GetChildGroup(array[i]);
				}
			}
			typeGroup.SubTypes.Add(new TypeGroup.TypePair(type, preExistingEditorType));
		}

		/// <summary>
		/// Determines whether Odin is capable of creating a custom editor for a given type.
		/// </summary>
		public static bool OdinCanCreateEditorFor(Type type)
		{
			return PossibleDrawnTypes.Contains(type);
		}

		/// <summary>
		/// Gets an array of all assigned editor types, and the types they have to draw.
		/// </summary>
		public static TypeDrawerPair[] GetEditors()
		{
			return GetEditorsForCompilation(UserTypesRootGroup).AppendWith(GetEditorsForCompilation(PluginTypesRootGroup)).AppendWith(GetEditorsForCompilation(UnityTypesRootGroup)).AppendWith(GetEditorsForCompilation(OtherTypesRootGroup))
				.ToArray();
		}

		private static IEnumerable<TypeDrawerPair> GetEditorsForCompilation(TypeGroup group)
		{
			foreach (TypeGroup.TypePair subType in group.SubTypes)
			{
				Type editorType = GlobalConfig<InspectorConfig>.Instance.DrawingConfig.GetEditorType(subType.DrawnType);
				if (editorType != null && editorType != typeof(InspectorTypeDrawingConfig.MissingEditor))
				{
					yield return new TypeDrawerPair(subType.DrawnType, editorType);
				}
			}
			foreach (TypeGroup subGroup in group.SubGroups)
			{
				foreach (TypeDrawerPair item in GetEditorsForCompilation(subGroup))
				{
					yield return item;
				}
			}
		}

		protected override void Initialize()
		{
			if (!initializedForDrawing)
			{
				UpdateRootGroupHasEligibletypes();
				UpdateRootGroupConflicts();
				SortRootGroups();
				UpdateRootGroupsSearch("", DisplayType.AllUnityObjects);
				initializedForDrawing = true;
			}
			searchText = "";
			displayType = DisplayType.AllUnityObjects;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Expected O, but got Unknown
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			SirenixEditorGUI.BeginHorizontalToolbar();
			GUILayout.Label("Draw Odin for", (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false));
			GUILayout.FlexibleSpace();
			SirenixEditorGUI.VerticalLineSeparator();
			GUI.set_changed(false);
			searchText = SirenixEditorGUI.ToolbarSearchField(searchText);
			if (GUI.get_changed())
			{
				UpdateRootGroupsSearch(searchText, displayType);
			}
			if (SirenixEditorGUI.ToolbarButton(new GUIContent(" Reset to default ")))
			{
				InspectorConfig instance = GlobalConfig<InspectorConfig>.Instance;
				if (EditorUtility.DisplayDialog("Reset " + ((Object)instance).get_name() + " to default", "Are you sure you want to reset all settings on " + ((Object)instance).get_name() + " to default values? This cannot be undone.", "Yes", "No"))
				{
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath((Object)(object)instance));
					AssetDatabase.Refresh();
					Object.DestroyImmediate((Object)(object)instance);
				}
			}
			SirenixEditorGUI.EndHorizontalToolbar();
			SirenixEditorGUI.BeginVerticalList(drawBorder: true, drawDarkBg: false, (GUILayoutOption[])GUILayoutOptions.ExpandHeight(expand: false));
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, (GUILayoutOption[])GUILayoutOptions.ExpandWidth());
			DrawRootTypeGroup(InspectorDefaultEditors.UserTypes, base.ValueEntry, searchText);
			DrawRootTypeGroup(InspectorDefaultEditors.PluginTypes, base.ValueEntry, searchText);
			DrawRootTypeGroup(InspectorDefaultEditors.UnityTypes, base.ValueEntry, searchText);
			DrawRootTypeGroup(InspectorDefaultEditors.OtherTypes, base.ValueEntry, searchText);
			EditorGUILayout.EndScrollView();
			SirenixEditorGUI.EndVerticalList();
		}

		private void DrawRootTypeGroup(InspectorDefaultEditors editorCategory, IPropertyValueEntry<InspectorTypeDrawingConfig> entry, string searchText)
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			TypeGroup typeGroup = editorCategory switch
			{
				InspectorDefaultEditors.UserTypes => UserTypesRootGroup, 
				InspectorDefaultEditors.PluginTypes => PluginTypesRootGroup, 
				InspectorDefaultEditors.UnityTypes => UnityTypesRootGroup, 
				_ => OtherTypesRootGroup, 
			};
			if (typeGroup.SubTypes.Count == 0 && typeGroup.SubGroups.Count == 0)
			{
				SirenixEditorGUI.BeginListItem(true, null);
				SirenixEditorGUI.BeginIndentedHorizontal();
				GUIHelper.PushGUIEnabled(enabled: false);
				SirenixEditorGUI.IconButton(EditorIcons.TriangleRight, IconStyle, 16);
				GUILayoutUtility.GetRect(16f, 16f, EditorStyles.get_toggle(), (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false).Width(16f));
				GUILayout.Label(typeGroup.Name, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PopGUIEnabled();
				SirenixEditorGUI.EndIndentedHorizontal();
				SirenixEditorGUI.EndListItem();
				return;
			}
			bool flag = true;
			Rect val = SirenixEditorGUI.BeginListItem(true, null);
			bool flag2 = false;
			SirenixEditorGUI.BeginIndentedHorizontal();
			EditorIcon icon = ((typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()) ? EditorIcons.TriangleDown : EditorIcons.TriangleRight);
			flag2 = SirenixEditorGUI.IconButton(icon, IconStyle, 16);
			if (flag)
			{
				EditorGUI.set_showMixedValue(typeGroup.HasConflict);
				bool flag3 = typeGroup.HasConflict || typeGroup.GetSharedEditorType() == typeof(OdinEditor);
				GUI.set_changed(false);
				flag3 = EditorGUI.Toggle(GUILayoutUtility.GetRect(16f, 16f, EditorStyles.get_toggle(), (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false).Width(16f)), flag3);
				if (GUI.get_changed())
				{
					typeGroup.ClearEditorTypes();
					if (flag3)
					{
						GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour |= editorCategory;
					}
					else
					{
						GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour = GlobalConfig<InspectorConfig>.Instance.DefaultEditorBehaviour & ~editorCategory;
					}
					EditorUtility.SetDirty((Object)(object)GlobalConfig<InspectorConfig>.Instance);
					GlobalConfig<InspectorConfig>.Instance.UpdateOdinEditors();
				}
				EditorGUI.set_showMixedValue(false);
			}
			else
			{
				GUILayout.Label("TODO: DROPDOWN!", (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			GUILayout.Label(typeGroup.Name, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			SirenixEditorGUI.EndIndentedHorizontal();
			if (flag2 || ((int)Event.get_current().get_type() == 0 && ((Rect)(ref val)).Contains(Event.get_current().get_mousePosition())))
			{
				typeGroup.IsExpanded = !typeGroup.IsExpanded;
				Event.get_current().Use();
			}
			SirenixEditorGUI.EndListItem();
			if (SirenixEditorGUI.BeginFadeGroup(typeGroup, typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()))
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				foreach (TypeGroup.TypePair subType in typeGroup.SubTypes)
				{
					if (typeGroup.IsTypeVisible(subType.DrawnType))
					{
						DrawType(subType, entry);
					}
				}
				foreach (TypeGroup subGroup in typeGroup.SubGroups)
				{
					DrawTypeGroup(subGroup, entry, searchText);
				}
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawTypeGroup(TypeGroup typeGroup, IPropertyValueEntry<InspectorTypeDrawingConfig> entry, string searchText)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			if (!typeGroup.IsSearchVisible)
			{
				return;
			}
			bool flag = true;
			Rect val = SirenixEditorGUI.BeginListItem(true, null);
			bool flag2 = false;
			SirenixEditorGUI.BeginIndentedHorizontal();
			EditorIcon icon = ((typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()) ? EditorIcons.TriangleDown : EditorIcons.TriangleRight);
			flag2 = SirenixEditorGUI.IconButton(icon, IconStyle, 16);
			if (!typeGroup.HasEligibleTypes)
			{
				flag2 |= SirenixEditorGUI.IconButton(EditorIcons.Transparent, 20);
			}
			else if (flag)
			{
				EditorGUI.set_showMixedValue(typeGroup.HasConflict);
				bool flag3 = typeGroup.HasConflict || typeGroup.GetSharedEditorType() == typeof(OdinEditor);
				GUI.set_changed(false);
				flag3 = EditorGUI.Toggle(GUILayoutUtility.GetRect(16f, 16f, EditorStyles.get_toggle(), (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false).Width(16f)), flag3);
				if (GUI.get_changed())
				{
					typeGroup.SetSharedEditorType(flag3 ? typeof(OdinEditor) : null);
					UpdateRootGroupConflicts();
					GlobalConfig<InspectorConfig>.Instance.UpdateOdinEditors();
				}
				EditorGUI.set_showMixedValue(false);
			}
			else
			{
				GUILayout.Label("TODO: DROPDOWN!", (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			GUILayout.Label(typeGroup.Name, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			SirenixEditorGUI.EndIndentedHorizontal();
			if (flag2 || ((int)Event.get_current().get_type() == 0 && ((Rect)(ref val)).Contains(Event.get_current().get_mousePosition())))
			{
				typeGroup.IsExpanded = !typeGroup.IsExpanded;
				Event.get_current().Use();
			}
			SirenixEditorGUI.EndListItem();
			if (SirenixEditorGUI.BeginFadeGroup(typeGroup, typeGroup.IsExpanded || !searchText.IsNullOrWhitespace()))
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				foreach (TypeGroup.TypePair subType in typeGroup.SubTypes)
				{
					if (typeGroup.IsTypeVisible(subType.DrawnType))
					{
						DrawType(subType, entry);
					}
				}
				foreach (TypeGroup subGroup in typeGroup.SubGroups)
				{
					DrawTypeGroup(subGroup, entry, searchText);
				}
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawType(TypeGroup.TypePair typeToDraw, IPropertyValueEntry<InspectorTypeDrawingConfig> entry)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			Type type = typeToDraw.PreExistingEditorType;
			bool showMixedValue = false;
			if (type == null)
			{
				for (int i = 0; i < entry.Values.Count; i++)
				{
					Type editorType = entry.Values[i].GetEditorType(typeToDraw.DrawnType);
					if (i == 0)
					{
						type = editorType;
					}
					else if (editorType != type)
					{
						type = null;
						showMixedValue = true;
						break;
					}
				}
			}
			bool flag = true;
			SirenixEditorGUI.BeginListItem(true, null);
			SirenixEditorGUI.BeginIndentedHorizontal();
			SirenixEditorGUI.IconButton(EditorIcons.Transparent, IconStyle, 16);
			if (typeToDraw.PreExistingEditorType != null)
			{
				SirenixEditorGUI.IconButton(EditorIcons.Transparent, IconStyle, 16);
				GUILayout.Label(typeToDraw.DrawnType.GetNiceName(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.Label(string.Concat("Drawn by '", typeToDraw.PreExistingEditorType, "'"), SirenixGUIStyles.RightAlignedGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				for (int j = 0; j < entry.Values.Count; j++)
				{
					if (entry.Values[j].HasEntryForType(typeToDraw.DrawnType))
					{
						entry.Values[j].ClearEditorEntryForDrawnType(typeToDraw.DrawnType);
					}
				}
			}
			else
			{
				EditorGUI.set_showMixedValue(showMixedValue);
				if (flag)
				{
					bool flag2 = type == typeof(OdinEditor);
					GUI.set_changed(false);
					flag2 = EditorGUI.Toggle(GUILayoutUtility.GetRect(16f, 16f, EditorStyles.get_toggle(), (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false).Width(16f)), flag2);
					if (GUI.get_changed())
					{
						for (int k = 0; k < entry.Values.Count; k++)
						{
							entry.Values[k].SetEditorType(typeToDraw.DrawnType, flag2 ? typeof(OdinEditor) : null);
						}
						UpdateRootGroupConflicts();
						GlobalConfig<InspectorConfig>.Instance.UpdateOdinEditors();
					}
					GUILayout.Label(typeToDraw.DrawnType.GetNiceName(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				else
				{
					GUILayout.Label("TODO: DROPDOWN!", (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
			}
			SirenixEditorGUI.EndIndentedHorizontal();
			EditorGUI.set_showMixedValue(false);
			SirenixEditorGUI.EndListItem();
		}
	}
}
