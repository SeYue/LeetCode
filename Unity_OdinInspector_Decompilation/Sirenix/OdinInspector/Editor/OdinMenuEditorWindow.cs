using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws an editor window with a menu tree.
	/// </summary>
	/// <example>
	/// <code>
	/// public class OdinMenuEditorWindowExample : OdinMenuEditorWindow
	/// {
	///     [SerializeField, HideLabel]
	///     private SomeData someData = new SomeData();
	///
	///     protected override OdinMenuTree BuildMenuTree()
	///     {
	///         OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
	///         {
	///             { "Home",                           this,                           EditorIcons.House       }, // draws the someDataField in this case.
	///             { "Odin Settings",                  null,                           EditorIcons.SettingsCog },
	///             { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
	///             { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
	///             { "Camera current",                 Camera.current                                          },
	///             { "Some Class",                     this.someData                                           }
	///         };
	///
	///         tree.AddAllAssetsAtPath("More Odin Settings", SirenixAssetPaths.OdinEditorConfigsPath, typeof(ScriptableObject), true)
	///             .AddThumbnailIcons();
	///
	///         tree.AddAssetAtPath("Odin Getting Started", SirenixAssetPaths.SirenixPluginPath + "Getting Started With Odin.asset");
	///
	///         var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
	///         tree.MenuItems.Insert(2, customMenuItem);
	///
	///         tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
	///         tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
	///
	///         // As you can see, Odin provides a few ways to quickly add editors / objects to your menu tree.
	///         // The API also gives you full control over the selection, etc..
	///         // Make sure to check out the API Documentation for OdinMenuEditorWindow, OdinMenuTree and OdinMenuItem for more information on what you can do!
	///
	///         return tree;
	///     }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public abstract class OdinMenuEditorWindow : OdinEditorWindow
	{
		[NonSerialized]
		private bool isDirty;

		[SerializeField]
		[HideInInspector]
		private OdinMenuTreeDrawingConfig menuTreeConfig;

		[SerializeField]
		[HideInInspector]
		private float menuWidth = 180f;

		[NonSerialized]
		private OdinMenuTree menuTree;

		[NonSerialized]
		private object trySelectObject;

		[SerializeField]
		[HideInInspector]
		private List<string> selectedItems = new List<string>();

		[SerializeField]
		[HideInInspector]
		private bool resizableMenuWidth = true;

		private EditorTimeHelper timeHelper;

		private OdinMenuTreeDrawingConfig MenuTreeConfig
		{
			get
			{
				menuTreeConfig = menuTreeConfig ?? new OdinMenuTreeDrawingConfig
				{
					DrawScrollView = true,
					DrawSearchToolbar = false,
					AutoHandleKeyboardNavigation = false
				};
				return menuTreeConfig;
			}
		}

		/// <summary>
		/// Gets or sets the width of the menu.
		/// </summary>
		public virtual float MenuWidth
		{
			get
			{
				return menuWidth;
			}
			set
			{
				menuWidth = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the menu is resizable.
		/// </summary>
		public virtual bool ResizableMenuWidth
		{
			get
			{
				return resizableMenuWidth;
			}
			set
			{
				resizableMenuWidth = value;
			}
		}

		/// <summary>
		/// Gets the menu tree.
		/// </summary>
		public OdinMenuTree MenuTree => menuTree;

		/// <summary>
		/// Gets or sets a value indicating whether to draw the menu search bar.
		/// </summary>
		public bool DrawMenuSearchBar
		{
			get
			{
				return MenuTreeConfig.DrawSearchToolbar;
			}
			set
			{
				MenuTreeConfig.DrawSearchToolbar = value;
			}
		}

		/// <summary>
		/// Gets or sets the custom search function.
		/// </summary>
		public Func<OdinMenuItem, bool> CustomSearchFunction
		{
			get
			{
				return MenuTreeConfig.SearchFunction;
			}
			set
			{
				MenuTreeConfig.SearchFunction = value;
			}
		}

		private void ProjectWindowChanged()
		{
			isDirty = true;
		}

		/// <summary>
		/// Called when the window is destroyed. Remember to call base.OnDestroy();
		/// </summary>
		protected override void OnDestroy()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Expected O, but got Unknown
			base.OnDestroy();
			if (UnityEditorEventUtility.HasOnProjectChanged)
			{
				UnityEditorEventUtility.OnProjectChanged -= ProjectWindowChanged;
				UnityEditorEventUtility.OnProjectChanged -= ProjectWindowChanged;
			}
			else
			{
				EditorApplication.projectWindowChanged = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.projectWindowChanged, (Delegate)new CallbackFunction(ProjectWindowChanged));
				EditorApplication.projectWindowChanged = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.projectWindowChanged, (Delegate)new CallbackFunction(ProjectWindowChanged));
			}
		}

		/// <summary>
		/// Builds the menu tree.
		/// </summary>
		protected abstract OdinMenuTree BuildMenuTree();

		/// <summary>
		/// Forces the menu tree rebuild.
		/// </summary>
		public void ForceMenuTreeRebuild()
		{
			menuTree = BuildMenuTree();
			if (selectedItems.Count == 0 && menuTree.Selection.Count == 0)
			{
				OdinMenuItem odinMenuItem = menuTree.EnumerateTree().FirstOrDefault((OdinMenuItem x) => x.Value != null);
				if (odinMenuItem != null)
				{
					odinMenuItem.GetParentMenuItemsRecursive(includeSelf: false).ForEach(delegate(OdinMenuItem x)
					{
						x.Toggled = true;
					});
					odinMenuItem.Select();
				}
			}
			else if (menuTree.Selection.Count == 0 && selectedItems.Count > 0)
			{
				foreach (OdinMenuItem item in menuTree.EnumerateTree())
				{
					if (selectedItems.Contains(item.GetFullPath()))
					{
						item.Select(addToSelection: true);
					}
				}
			}
			menuTree.Selection.SelectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged(SelectionChangedType type)
		{
			((EditorWindow)this).Repaint();
			GUIHelper.RemoveFocusControl();
			selectedItems = menuTree.Selection.Select((OdinMenuItem x) => x.GetFullPath()).ToList();
			EditorUtility.SetDirty((Object)(object)this);
		}

		/// <summary>
		/// Tries to select the menu item with the specified object.
		/// </summary>
		public void TrySelectMenuItemWithObject(object obj)
		{
			trySelectObject = obj;
		}

		/// <summary>
		/// Draws the menu tree selection.
		/// </summary>
		protected override IEnumerable<object> GetTargets()
		{
			if (menuTree == null)
			{
				yield break;
			}
			for (int i = 0; i < menuTree.Selection.Count; i++)
			{
				OdinMenuItem odinMenuItem = menuTree.Selection[i];
				if (odinMenuItem != null)
				{
					object obj = odinMenuItem.Value;
					Func<object> func = obj as Func<object>;
					if (func != null)
					{
						obj = func();
					}
					if (obj != null)
					{
						yield return obj;
					}
				}
			}
		}

		/// <summary>
		/// Draws the Odin Editor Window.
		/// </summary>
		protected override void OnGUI()
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Invalid comparison between Unknown and I4
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Expected O, but got Unknown
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Expected O, but got Unknown
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Expected O, but got Unknown
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			if (timeHelper == null)
			{
				timeHelper = new EditorTimeHelper();
			}
			EditorTimeHelper time = EditorTimeHelper.Time;
			EditorTimeHelper.Time = timeHelper;
			EditorTimeHelper.Time.Update();
			try
			{
				if ((int)Event.get_current().get_type() == 8)
				{
					bool flag = menuTree == null;
					if (menuTree == null || isDirty)
					{
						ForceMenuTreeRebuild();
						if (flag)
						{
							OdinMenuTree.ActiveMenuTree = menuTree;
						}
						if (UnityEditorEventUtility.HasOnProjectChanged)
						{
							UnityEditorEventUtility.OnProjectChanged -= ProjectWindowChanged;
							UnityEditorEventUtility.OnProjectChanged += ProjectWindowChanged;
						}
						else
						{
							EditorApplication.projectWindowChanged = (CallbackFunction)Delegate.Remove((Delegate)(object)EditorApplication.projectWindowChanged, (Delegate)new CallbackFunction(ProjectWindowChanged));
							EditorApplication.projectWindowChanged = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.projectWindowChanged, (Delegate)new CallbackFunction(ProjectWindowChanged));
						}
						isDirty = false;
					}
					if (trySelectObject != null && menuTree != null)
					{
						OdinMenuItem odinMenuItem = menuTree.EnumerateTree().FirstOrDefault((OdinMenuItem x) => x.Value == trySelectObject);
						if (odinMenuItem != null)
						{
							menuTree.Selection.Clear();
							odinMenuItem.Select();
							trySelectObject = null;
						}
					}
				}
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUILayout.BeginVertical((GUILayoutOption[])GUILayoutOptions.Width(MenuWidth).ExpandHeight());
				Rect currentLayoutRect = GUIHelper.GetCurrentLayoutRect();
				if (menuTree != null)
				{
					menuTree.MenuTreeActivationZone(currentLayoutRect);
				}
				EditorGUI.DrawRect(currentLayoutRect, SirenixGUIStyles.MenuBackgroundColor);
				Rect val = currentLayoutRect;
				((Rect)(ref val)).set_xMin(((Rect)(ref currentLayoutRect)).get_xMax() - 4f);
				((Rect)(ref val)).set_xMax(((Rect)(ref val)).get_xMax() + 4f);
				if (ResizableMenuWidth)
				{
					EditorGUIUtility.AddCursorRect(val, (MouseCursor)3);
					MenuWidth += SirenixEditorGUI.SlideRect(val, (MouseCursor)5).x;
				}
				DrawMenu();
				GUILayout.EndVertical();
				GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
				Rect currentLayoutRect2 = GUIHelper.GetCurrentLayoutRect();
				EditorGUI.DrawRect(currentLayoutRect2, SirenixGUIStyles.DarkEditorBackground);
				base.OnGUI();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				EditorGUI.DrawRect(val.AlignCenter(1f), SirenixGUIStyles.BorderColor);
				if (menuTree != null)
				{
					menuTree.HandleKeyboardMenuNavigation();
				}
				((EditorWindow)(object)this).RepaintIfRequested();
			}
			finally
			{
				EditorTimeHelper.Time = time;
			}
		}

		/// <summary>
		/// The method that draws the menu.
		/// </summary>
		protected virtual void DrawMenu()
		{
			if (menuTree != null)
			{
				menuTree.DrawMenuTree();
			}
		}
	}
}
