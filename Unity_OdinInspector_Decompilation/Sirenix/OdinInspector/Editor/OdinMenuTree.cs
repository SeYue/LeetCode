using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// OdinMenuTree provides a tree of <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />s, and helps with selection, inserting menu items into the tree, and can handle keyboard navigation for you.
	/// </summary>
	/// <example>
	/// <code>
	/// OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
	/// {
	///     { "Home",                           this,                           EditorIcons.House       },
	///     { "Odin Settings",                  null,                           EditorIcons.SettingsCog },
	///     { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
	///     { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
	///     { "Camera current",                 Camera.current                                          },
	///     { "Some Class",                     this.someData                                           }
	/// };
	///
	/// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
	///     .AddThumbnailIcons();
	///
	/// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
	///
	/// var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
	/// tree.MenuItems.Insert(2, customMenuItem);
	///
	/// tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
	/// tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
	/// </code>
	/// OdinMenuTrees are typically used with <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />s but is made to work perfectly fine on its own for other use cases.
	/// OdinMenuItems can be inherited and and customized to fit your needs.
	/// <code>
	/// // Draw stuff
	/// someTree.DrawMenuTree();
	/// // Draw stuff
	/// someTree.HandleKeybaordMenuNavigation();
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public class OdinMenuTree : IEnumerable
	{
		private static bool preventAutoFocus;

		/// <summary>
		/// Gets the currently active menu tree.
		/// </summary>
		public static OdinMenuTree ActiveMenuTree;

		private static HashSet<OdinMenuItem> cachedHashList = new HashSet<OdinMenuItem>();

		private readonly OdinMenuItem root;

		private readonly OdinMenuTreeSelection selection;

		private OdinMenuTreeDrawingConfig defaultConfig;

		private bool regainSearchFieldFocus;

		private bool hadSearchFieldFocus;

		private Rect outerScrollViewRect;

		private int hideScrollbarsWhileContentIsExpanding;

		private Rect innerScrollViewRect;

		private bool isFirstFrame = true;

		private int forceRegainFocusCounter;

		private bool requestRepaint;

		private GUIFrameCounter frameCounter = new GUIFrameCounter();

		private bool hasRepaintedCurrentSearchResult = true;

		private bool scollToCenter;

		private OdinMenuItem scrollToWhenReady;

		private string searchFieldControlName;

		private bool isDirty;

		private bool updateSearchResults;

		private bool regainFocusWhenWindowFocus;

		private bool currWindowHasFocus;

		private EditorTimeHelper timeHelper = new EditorTimeHelper();

		internal static Rect VisibleRect;

		internal static EditorTimeHelper CurrentEditorTimeHelper;

		internal static float CurrentEditorTimeHelperDeltaTime;

		internal static Event CurrentEvent;

		internal static EventType CurrentEventType;

		public List<OdinMenuItem> FlatMenuTree = new List<OdinMenuItem>();

		internal OdinMenuItem Root => root;

		/// <summary>
		/// Gets the selection.
		/// </summary>
		public OdinMenuTreeSelection Selection => selection;

		/// <summary>
		/// Gets the root menu items.
		/// </summary>
		public List<OdinMenuItem> MenuItems => root.ChildMenuItems;

		/// <summary>
		/// Gets the root menu item.
		/// </summary>
		public OdinMenuItem RootMenuItem => root;

		/// <summary>
		/// If true, all indent levels will be ignored, and all menu items with IsVisible == true will be drawn.
		/// </summary>
		public bool DrawInSearchMode { get; private set; }

		/// <summary>
		/// Gets or sets the default menu item style from Config.DefaultStyle.
		/// </summary>
		public OdinMenuStyle DefaultMenuStyle
		{
			get
			{
				return Config.DefaultMenuStyle;
			}
			set
			{
				Config.DefaultMenuStyle = value;
			}
		}

		/// <summary>
		/// Gets or sets the default drawing configuration.
		/// </summary>
		public OdinMenuTreeDrawingConfig Config
		{
			get
			{
				defaultConfig = defaultConfig ?? new OdinMenuTreeDrawingConfig
				{
					DrawScrollView = true,
					DrawSearchToolbar = false,
					AutoHandleKeyboardNavigation = false
				};
				return defaultConfig;
			}
			set
			{
				defaultConfig = value;
			}
		}

		/// <summary>
		/// Adds a menu item with the specified object instance at the the specified path.
		/// </summary>
		public void Add(string path, object instance)
		{
			this.AddObjectAtPath(path, instance);
		}

		/// <summary>
		/// Adds a menu item with the specified object instance and icon at the the specified path.
		/// </summary>
		public void Add(string path, object instance, Texture icon)
		{
			this.AddObjectAtPath(path, instance).AddIcon(icon);
		}

		/// <summary>
		/// Adds a menu item with the specified object instance and icon at the the specified path.
		/// </summary>
		public void Add(string path, object instance, Sprite sprite)
		{
			this.AddObjectAtPath(path, instance).AddIcon((Texture)(object)AssetPreview.GetAssetPreview((Object)(object)sprite));
		}

		/// <summary>
		/// Adds a menu item with the specified object instance and icon at the the specified path.
		/// </summary>
		public void Add(string path, object instance, EditorIcon icon)
		{
			this.AddObjectAtPath(path, instance).AddIcon(icon);
		}

		/// <summary>
		/// Adds a collection of objects to the menu tree and returns all menu items created in random order.
		/// </summary>
		public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath)
		{
			if (collection == null)
			{
				return Enumerable.Empty<OdinMenuItem>();
			}
			cachedHashList.Clear();
			foreach (T item in collection)
			{
				cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
			}
			return cachedHashList;
		}

		/// <summary>
		/// Adds a collection of objects to the menu tree and returns all menu items created in random order.
		/// </summary>
		public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath, Func<T, Texture> getIcon)
		{
			if (collection == null)
			{
				return Enumerable.Empty<OdinMenuItem>();
			}
			cachedHashList.Clear();
			foreach (T item in collection)
			{
				if (getIcon != null)
				{
					cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item).AddIcon(getIcon(item)));
				}
				else
				{
					cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
				}
			}
			return cachedHashList;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		public OdinMenuTree()
			: this(supportsMultiSelect: false, new OdinMenuStyle())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		/// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
		public OdinMenuTree(bool supportsMultiSelect)
			: this(supportsMultiSelect, new OdinMenuStyle())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		/// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
		/// <param name="defaultMenuStyle">The default menu item style.</param>
		public OdinMenuTree(bool supportsMultiSelect, OdinMenuStyle defaultMenuStyle)
		{
			DefaultMenuStyle = defaultMenuStyle;
			selection = new OdinMenuTreeSelection(supportsMultiSelect);
			root = new OdinMenuItem(this, "root", null);
			SetupAutoScroll();
			searchFieldControlName = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		public OdinMenuTree(bool supportsMultiSelect, OdinMenuTreeDrawingConfig config)
		{
			Config = config;
			selection = new OdinMenuTreeSelection(supportsMultiSelect);
			root = new OdinMenuItem(this, "root", null);
			SetupAutoScroll();
		}

		private void SetupAutoScroll()
		{
			selection.SelectionChanged += delegate(SelectionChangedType x)
			{
				if (Config.AutoScrollOnSelectionChanged && x == SelectionChangedType.ItemAdded)
				{
					requestRepaint = true;
					GUIHelper.RequestRepaint();
					if (isFirstFrame)
					{
						ScrollToMenuItem(selection.LastOrDefault(), centerMenuItem: true);
					}
					else
					{
						ScrollToMenuItem(selection.LastOrDefault());
					}
				}
			};
		}

		/// <summary>
		/// Scrolls to the specified menu item.
		/// </summary>
		public void ScrollToMenuItem(OdinMenuItem menuItem, bool centerMenuItem = false)
		{
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Invalid comparison between Unknown and I4
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			if (menuItem == null)
			{
				return;
			}
			scollToCenter = centerMenuItem;
			scrollToWhenReady = menuItem;
			if (!menuItem._IsVisible())
			{
				foreach (OdinMenuItem item in menuItem.GetParentMenuItemsRecursive(includeSelf: false))
				{
					item.Toggled = true;
				}
				return;
			}
			foreach (OdinMenuItem item2 in menuItem.GetParentMenuItemsRecursive(includeSelf: false))
			{
				item2.Toggled = true;
			}
			if (((Rect)(ref outerScrollViewRect)).get_height() == 0f)
			{
				return;
			}
			Rect rect = menuItem.Rect;
			if (!(((Rect)(ref rect)).get_height() <= 0.01f) && Event.get_current() != null && (int)Event.get_current().get_type() == 7)
			{
				OdinMenuTreeDrawingConfig config = Config;
				Rect rect2 = menuItem.Rect;
				float num;
				float num2;
				if (centerMenuItem)
				{
					Rect val = outerScrollViewRect.AlignCenterY(((Rect)(ref rect2)).get_height());
					num = ((Rect)(ref rect2)).get_yMin() - (((Rect)(ref innerScrollViewRect)).get_y() + config.ScrollPos.y - ((Rect)(ref val)).get_y());
					num2 = ((Rect)(ref rect2)).get_yMax() - ((Rect)(ref val)).get_height() + ((Rect)(ref innerScrollViewRect)).get_y() - (config.ScrollPos.y + ((Rect)(ref val)).get_y());
				}
				else
				{
					Rect val2 = outerScrollViewRect;
					((Rect)(ref val2)).set_y(0f);
					num = ((Rect)(ref rect2)).get_yMin() - (((Rect)(ref innerScrollViewRect)).get_y() + config.ScrollPos.y) - 1f;
					num2 = ((Rect)(ref rect2)).get_yMax() - ((Rect)(ref outerScrollViewRect)).get_height() + ((Rect)(ref innerScrollViewRect)).get_y() - config.ScrollPos.y;
					num -= ((Rect)(ref rect2)).get_height();
					num2 += ((Rect)(ref rect2)).get_height();
				}
				if (num < 0f)
				{
					config.ScrollPos.y += num;
				}
				if (num2 > 0f)
				{
					config.ScrollPos.y += num2;
				}
				if (frameCounter.FrameCount > 6)
				{
					scrollToWhenReady = null;
				}
				else
				{
					GUIHelper.RequestRepaint();
				}
			}
		}

		/// <summary>
		/// Enumerates the tree with a DFS.
		/// </summary>
		/// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
		public IEnumerable<OdinMenuItem> EnumerateTree(bool includeRootNode = false)
		{
			return root.GetChildMenuItemsRecursive(includeRootNode);
		}

		/// <summary>
		/// Enumerates the tree with a DFS.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
		public IEnumerable<OdinMenuItem> EnumerateTree(Func<OdinMenuItem, bool> predicate, bool includeRootNode)
		{
			return root.GetChildMenuItemsRecursive(includeRootNode).Where(predicate);
		}

		/// <summary>
		/// Enumerates the tree with a DFS.
		/// </summary>
		public void EnumerateTree(Action<OdinMenuItem> action)
		{
			root.GetChildMenuItemsRecursive(includeSelf: false).ForEach(action);
		}

		/// <summary>
		/// Draws the menu tree recursively.
		/// </summary>
		public void DrawMenuTree()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Invalid comparison between Unknown and I4
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Invalid comparison between Unknown and I4
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Invalid comparison between Unknown and I4
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_027b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0332: Unknown result type (might be due to invalid IL or missing references)
			//IL_0338: Invalid comparison between Unknown and I4
			EditorTimeHelper time = EditorTimeHelper.Time;
			EditorTimeHelper.Time = timeHelper;
			EditorTimeHelper.Time.Update();
			try
			{
				timeHelper.Update();
				frameCounter.Update();
				OdinMenuTreeDrawingConfig config = Config;
				if (requestRepaint)
				{
					GUIHelper.RequestRepaint();
					requestRepaint = false;
				}
				if (config.DrawSearchToolbar)
				{
					DrawSearchToolbar();
				}
				Rect outerRect = EditorGUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
				HandleActiveMenuTreeState(outerRect);
				if (config.DrawScrollView)
				{
					if ((int)Event.get_current().get_type() == 7)
					{
						outerScrollViewRect = outerRect;
					}
					if (hideScrollbarsWhileContentIsExpanding > 0)
					{
						config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUIStyle.get_none(), GUIStyle.get_none(), (GUILayoutOption[])GUILayoutOptions.ExpandHeight(expand: false));
					}
					else
					{
						config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, (GUILayoutOption[])GUILayoutOptions.ExpandHeight(expand: false));
					}
					Rect val = EditorGUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
					if (((Rect)(ref innerScrollViewRect)).get_height() == 0f || (int)Event.get_current().get_type() == 7)
					{
						float num = Mathf.Abs(((Rect)(ref innerScrollViewRect)).get_height() - ((Rect)(ref val)).get_height());
						float num2 = Mathf.Abs(((Rect)(ref innerScrollViewRect)).get_height() - ((Rect)(ref outerScrollViewRect)).get_height());
						if (!(((Rect)(ref innerScrollViewRect)).get_height() - 40f > ((Rect)(ref outerScrollViewRect)).get_height()) && num > 0f)
						{
							hideScrollbarsWhileContentIsExpanding = 5;
							GUIHelper.RequestRepaint();
						}
						else if (Mathf.Abs(num2) < 1f)
						{
							hideScrollbarsWhileContentIsExpanding = 5;
						}
						else
						{
							hideScrollbarsWhileContentIsExpanding--;
							if (hideScrollbarsWhileContentIsExpanding < 0)
							{
								hideScrollbarsWhileContentIsExpanding = 0;
							}
							else
							{
								GUIHelper.RequestRepaint();
							}
						}
						innerScrollViewRect = val;
					}
					GUILayout.Space(-1f);
				}
				if (isDirty && (int)Event.get_current().get_type() == 8)
				{
					UpdateMenuTree();
					isDirty = false;
				}
				VisibleRect = GUIClipInfo.VisibleRect.Expand(300f);
				CurrentEvent = Event.get_current();
				CurrentEventType = CurrentEvent.get_type();
				CurrentEditorTimeHelper = EditorTimeHelper.Time;
				CurrentEditorTimeHelperDeltaTime = CurrentEditorTimeHelper.DeltaTime;
				List<OdinMenuItem> list = (DrawInSearchMode ? FlatMenuTree : MenuItems);
				int count = list.Count;
				if (config.EXPERIMENTAL_INTERNAL_DrawFlatTreeFastNoLayout)
				{
					int height = DefaultMenuStyle.Height;
					int num3 = count * height;
					Rect rect = GUILayoutUtility.GetRect(0f, (float)num3);
					((Rect)(ref rect)).set_height((float)height);
					for (int i = 0; i < count; i++)
					{
						OdinMenuItem odinMenuItem = list[i];
						odinMenuItem.EXPERIMENTAL_DontAllocateNewRect = true;
						odinMenuItem.rect = rect;
						odinMenuItem.DrawMenuItem(0);
						((Rect)(ref rect)).set_y(((Rect)(ref rect)).get_y() + (float)height);
					}
				}
				else
				{
					for (int j = 0; j < count; j++)
					{
						list[j].DrawMenuItems(0);
					}
				}
				if (config.DrawScrollView)
				{
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
				if (config.AutoHandleKeyboardNavigation)
				{
					HandleKeyboardMenuNavigation();
				}
				if (scrollToWhenReady != null)
				{
					ScrollToMenuItem(scrollToWhenReady, scollToCenter);
				}
				if ((int)Event.get_current().get_type() == 7)
				{
					isFirstFrame = false;
				}
			}
			finally
			{
				EditorTimeHelper.Time = time;
			}
		}

		private void HandleActiveMenuTreeState(Rect outerRect)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 7)
			{
				if (currWindowHasFocus != GUIHelper.CurrentWindowHasFocus)
				{
					currWindowHasFocus = GUIHelper.CurrentWindowHasFocus;
					if (currWindowHasFocus && regainFocusWhenWindowFocus)
					{
						if (!preventAutoFocus)
						{
							ActiveMenuTree = this;
						}
						regainFocusWhenWindowFocus = false;
					}
				}
				if (!currWindowHasFocus && ActiveMenuTree == this)
				{
					ActiveMenuTree = null;
				}
				if (currWindowHasFocus)
				{
					regainFocusWhenWindowFocus = ActiveMenuTree == this;
				}
				if (currWindowHasFocus && ActiveMenuTree == null)
				{
					ActiveMenuTree = this;
				}
			}
			MenuTreeActivationZone(outerRect);
		}

		internal void MenuTreeActivationZone(Rect rect)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if (ActiveMenuTree != this && (int)Event.get_current().get_rawType() == 0 && ((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()) && GUIHelper.CurrentWindowHasFocus)
			{
				regainSearchFieldFocus = true;
				preventAutoFocus = true;
				ActiveMenuTree = this;
				UnityEditorEventUtility.EditorApplication_delayCall += delegate
				{
					preventAutoFocus = false;
				};
				GUIHelper.RequestRepaint();
			}
		}

		/// <summary>
		/// Marks the dirty. This will cause a tree.UpdateTree() in the beginning of the next Layout frame.
		/// </summary>
		public void MarkDirty()
		{
			isDirty = true;
			updateSearchResults = true;
		}

		/// <summary>
		/// Draws the search toolbar.
		/// </summary>
		public void DrawSearchToolbar(GUIStyle toolbarStyle = null)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Invalid comparison between Unknown and I4
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Invalid comparison between Unknown and I4
			OdinMenuTreeDrawingConfig config = Config;
			Rect val = GUILayoutUtility.GetRect(0f, (float)config.SearchToolbarHeight, (GUILayoutOption[])GUILayoutOptions.ExpandWidth());
			if ((int)Event.get_current().get_type() == 7)
			{
				(toolbarStyle ?? SirenixGUIStyles.ToolbarBackground).Draw(val, GUIContent.none, 0);
			}
			val = val.HorizontalPadding(5f).AlignMiddle(16f);
			((Rect)(ref val)).set_xMin(((Rect)(ref val)).get_xMin() + 3f);
			((Rect)(ref val)).set_y(((Rect)(ref val)).get_y() + 1f);
			EditorGUI.BeginChangeCheck();
			config.SearchTerm = DrawSearchField(val, config.SearchTerm, config.AutoFocusSearchBar);
			if ((EditorGUI.EndChangeCheck() || updateSearchResults) && hasRepaintedCurrentSearchResult)
			{
				updateSearchResults = false;
				hasRepaintedCurrentSearchResult = false;
				if (!string.IsNullOrEmpty(config.SearchTerm))
				{
					if (!DrawInSearchMode)
					{
						config.ScrollPos = default(Vector2);
					}
					DrawInSearchMode = true;
					if (config.SearchFunction != null)
					{
						FlatMenuTree.Clear();
						foreach (OdinMenuItem item in EnumerateTree())
						{
							if (config.SearchFunction(item))
							{
								FlatMenuTree.Add(item);
							}
						}
					}
					else
					{
						FlatMenuTree.Clear();
						FlatMenuTree.AddRange(from x in (from x in EnumerateTree()
								where x.Value != null
								select x).Select(delegate(OdinMenuItem x)
							{
								int score;
								bool include = FuzzySearch.Contains(Config.SearchTerm, x.SearchString, out score);
								return new
								{
									score = score,
									item = x,
									include = include
								};
							})
							where x.include
							orderby x.score descending
							select x.item);
					}
					root.UpdateFlatMenuItemNavigation();
				}
				else
				{
					DrawInSearchMode = false;
					FlatMenuTree.Clear();
					OdinMenuItem odinMenuItem = selection.LastOrDefault();
					UpdateMenuTree();
					Selection.SelectMany((OdinMenuItem x) => x.GetParentMenuItemsRecursive(includeSelf: false)).ForEach(delegate(OdinMenuItem x)
					{
						x.Toggled = true;
					});
					if (odinMenuItem != null)
					{
						ScrollToMenuItem(odinMenuItem);
					}
					root.UpdateFlatMenuItemNavigation();
				}
			}
			if ((int)Event.get_current().get_type() == 7)
			{
				hasRepaintedCurrentSearchResult = true;
			}
		}

		private string DrawSearchField(Rect rect, string searchTerm, bool autoFocus)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Invalid comparison between Unknown and I4
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Invalid comparison between Unknown and I4
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Invalid comparison between Unknown and I4
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Invalid comparison between Unknown and I4
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Invalid comparison between Unknown and I4
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Invalid comparison between Unknown and I4
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Invalid comparison between Unknown and I4
			bool flag = GUI.GetNameOfFocusedControl() == searchFieldControlName;
			if (hadSearchFieldFocus != flag)
			{
				if (flag)
				{
					ActiveMenuTree = this;
				}
				hadSearchFieldFocus = flag;
			}
			bool flag2 = flag && ((int)Event.get_current().get_keyCode() == 274 || (int)Event.get_current().get_keyCode() == 273 || (int)Event.get_current().get_keyCode() == 276 || (int)Event.get_current().get_keyCode() == 275 || (int)Event.get_current().get_keyCode() == 13);
			if (flag2)
			{
				GUIHelper.PushEventType(Event.get_current().get_type());
			}
			searchTerm = SirenixEditorGUI.SearchField(rect, searchTerm, autoFocus && regainSearchFieldFocus && ActiveMenuTree == this, searchFieldControlName);
			if (regainSearchFieldFocus && (int)Event.get_current().get_type() == 8)
			{
				regainSearchFieldFocus = false;
			}
			if (flag2)
			{
				GUIHelper.PopEventType();
				if (ActiveMenuTree == this)
				{
					regainSearchFieldFocus = true;
				}
			}
			if (forceRegainFocusCounter < 20)
			{
				if (autoFocus && forceRegainFocusCounter < 4 && ActiveMenuTree == this)
				{
					regainSearchFieldFocus = true;
				}
				GUIHelper.RequestRepaint();
				HandleUtility.Repaint();
				if ((int)Event.get_current().get_type() == 7)
				{
					forceRegainFocusCounter++;
				}
			}
			return searchTerm;
		}

		/// <summary>
		/// Updates the menu tree. This method is usually called automatically when needed.
		/// </summary>
		public void UpdateMenuTree()
		{
			root.UpdateMenuTreeRecursive(isRoot: true);
			root.UpdateFlatMenuItemNavigation();
		}

		/// <summary>
		/// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
		/// </summary>
		/// <returns>Returns true, if anything was changed via the keyboard.</returns>
		[Obsolete("Use HandleKeyboardMenuNavigation instead.", false)]
		public bool HandleKeybaordMenuNavigation()
		{
			return HandleKeyboardMenuNavigation();
		}

		/// <summary>
		/// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
		/// </summary>
		/// <returns>Returns true, if anything was changed via the keyboard.</returns>
		public bool HandleKeyboardMenuNavigation()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Invalid comparison between Unknown and I4
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Invalid comparison between Unknown and I4
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Invalid comparison between Unknown and I4
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Invalid comparison between Unknown and I4
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Invalid comparison between Unknown and I4
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Invalid comparison between Unknown and I4
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Invalid comparison between Unknown and I4
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			//IL_0337: Invalid comparison between Unknown and I4
			//IL_033e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0344: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Invalid comparison between Unknown and I4
			if ((int)Event.get_current().get_type() != 4)
			{
				return false;
			}
			if (ActiveMenuTree != this)
			{
				return false;
			}
			GUIHelper.RequestRepaint();
			KeyCode val = Event.get_current().get_keyCode();
			if (Selection.Count == 0 || !Selection.Any((OdinMenuItem x) => x._IsVisible()))
			{
				IEnumerable<OdinMenuItem> enumerable;
				if (!DrawInSearchMode)
				{
					enumerable = from x in EnumerateTree()
						where x._IsVisible()
						select x;
				}
				else
				{
					IEnumerable<OdinMenuItem> flatMenuTree = FlatMenuTree;
					enumerable = flatMenuTree;
				}
				IEnumerable<OdinMenuItem> source = enumerable;
				OdinMenuItem odinMenuItem = null;
				if ((int)val == 274)
				{
					odinMenuItem = source.FirstOrDefault();
				}
				else if ((int)val == 273)
				{
					odinMenuItem = source.LastOrDefault();
				}
				else if ((int)val == 308)
				{
					odinMenuItem = source.FirstOrDefault();
				}
				else if ((int)val == 307)
				{
					odinMenuItem = source.FirstOrDefault();
				}
				if (odinMenuItem != null)
				{
					odinMenuItem.Select();
					Event.get_current().Use();
					return true;
				}
			}
			else
			{
				if ((int)val == 276 && !DrawInSearchMode)
				{
					bool flag = true;
					foreach (OdinMenuItem item in Selection.ToList())
					{
						if (item.Toggled && item.ChildMenuItems.Any())
						{
							flag = false;
							item.Toggled = false;
						}
						if ((Event.get_current().get_modifiers() & 4) == 0)
						{
							continue;
						}
						flag = false;
						foreach (OdinMenuItem item2 in item.GetChildMenuItemsRecursive(includeSelf: false))
						{
							item2.Toggled = item.Toggled;
						}
					}
					if (flag)
					{
						val = (KeyCode)273;
					}
					Event.get_current().Use();
				}
				if ((int)val == 275 && !DrawInSearchMode)
				{
					bool flag2 = true;
					foreach (OdinMenuItem item3 in Selection.ToList())
					{
						if (!item3.Toggled && item3.ChildMenuItems.Any())
						{
							item3.Toggled = true;
							flag2 = false;
						}
						if ((Event.get_current().get_modifiers() & 4) == 0)
						{
							continue;
						}
						flag2 = false;
						foreach (OdinMenuItem item4 in item3.GetChildMenuItemsRecursive(includeSelf: false))
						{
							item4.Toggled = item3.Toggled;
						}
					}
					if (flag2)
					{
						val = (KeyCode)274;
					}
					Event.get_current().Use();
				}
				if ((int)val == 273)
				{
					if ((Event.get_current().get_modifiers() & 1) != 0)
					{
						OdinMenuItem odinMenuItem2 = Selection.Last();
						OdinMenuItem prevVisualMenuItem = odinMenuItem2.PrevVisualMenuItem;
						if (prevVisualMenuItem != null)
						{
							if (prevVisualMenuItem.IsSelected)
							{
								odinMenuItem2.Deselect();
							}
							else
							{
								prevVisualMenuItem.Select(addToSelection: true);
							}
							Event.get_current().Use();
							return true;
						}
					}
					else
					{
						OdinMenuItem prevVisualMenuItem2 = Selection.Last().PrevVisualMenuItem;
						if (prevVisualMenuItem2 != null)
						{
							prevVisualMenuItem2.Select();
							Event.get_current().Use();
							return true;
						}
					}
				}
				if ((int)val == 274)
				{
					if ((Event.get_current().get_modifiers() & 1) != 0)
					{
						OdinMenuItem odinMenuItem3 = Selection.Last();
						OdinMenuItem nextVisualMenuItem = odinMenuItem3.NextVisualMenuItem;
						if (nextVisualMenuItem != null)
						{
							if (nextVisualMenuItem.IsSelected)
							{
								odinMenuItem3.Deselect();
							}
							else
							{
								nextVisualMenuItem.Select(addToSelection: true);
							}
							Event.get_current().Use();
							return true;
						}
					}
					else
					{
						OdinMenuItem nextVisualMenuItem2 = Selection.Last().NextVisualMenuItem;
						if (nextVisualMenuItem2 != null)
						{
							nextVisualMenuItem2.Select();
							Event.get_current().Use();
							return true;
						}
					}
				}
				if ((int)val == 13)
				{
					Selection.ConfirmSelection();
					Event.get_current().Use();
					return true;
				}
			}
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return MenuItems.GetEnumerator();
		}
	}
}
