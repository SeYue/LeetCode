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
	/// A menu item that represents one or more objects.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public class OdinMenuItem
	{
		private static Color mouseOverColor = (EditorGUIUtility.get_isProSkin() ? new Color(1f, 1f, 1f, 0.028f) : new Color(1f, 1f, 1f, 0.3f));

		private static OdinMenuItem handleClickEventOnMouseUp;

		private static bool previousMenuItemWasSelected = false;

		private List<OdinMenuItem> childMenuItems;

		private int flatTreeIndex;

		private Texture iconSelected;

		private Texture icon;

		private Func<Texture> iconGetter;

		private bool isInitialized;

		private LocalPersistentContext<bool> isToggledContext;

		private OdinMenuTree menuTree;

		private string prevName;

		private string name;

		private bool isVisible = true;

		private OdinMenuItem nextMenuItem;

		private OdinMenuItem nextMenuItemFlat;

		private OdinMenuItem parentMenuItem;

		private OdinMenuItem previousMenuItem;

		private OdinMenuItem previousMenuItemFlat;

		private OdinMenuStyle style;

		private Rect triangleRect;

		private Rect labelRect;

		private bool? nonCachedToggledState;

		private object value;

		internal Rect rect;

		internal bool EXPERIMENTAL_DontAllocateNewRect;

		public bool MenuItemIsBeingRendered;

		/// <summary>
		/// The default toggled state
		/// </summary>
		public bool DefaultToggledState;

		/// <summary>
		/// Occurs right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
		/// </summary>
		public Action<OdinMenuItem> OnDrawItem;

		/// <summary>
		/// Occurs when the user has right-clicked the menu item.
		/// </summary>
		public Action<OdinMenuItem> OnRightClick;

		private float t = -1f;

		private bool wasMouseDownEvent;

		/// <summary>
		/// Gets the child menu items.
		/// </summary>
		/// <value>
		/// The child menu items.
		/// </value>
		public virtual List<OdinMenuItem> ChildMenuItems => childMenuItems;

		/// <summary>
		/// Gets the index location of the menu item.
		/// </summary>
		public int FlatTreeIndex => flatTreeIndex;

		/// <summary>
		/// Gets or sets a value indicating whether the menu item is visible.
		/// Not that setting this to false will not hide its children as well. For that see use Toggled.
		/// </summary>
		[Obsolete("OdinMenuItems no longer has this concept which was previously used for filtering search results. Instead search results are cached to seperate list in order to support sorting.")]
		public virtual bool IsVisible
		{
			get
			{
				return isVisible;
			}
			set
			{
				isVisible = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon that is used when the menu item is not selected.
		/// </summary>
		public Texture Icon
		{
			get
			{
				return icon;
			}
			set
			{
				icon = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon that is used when the menu item is selected.
		/// </summary>
		public Texture IconSelected
		{
			get
			{
				return iconSelected;
			}
			set
			{
				iconSelected = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is selected.
		/// </summary>
		public bool IsSelected => menuTree.Selection.Contains(this);

		/// <summary>
		/// Gets the menu tree instance.
		/// </summary>
		public OdinMenuTree MenuTree => menuTree;

		/// <summary>
		/// Gets or sets the raw menu item name.
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		/// <summary>
		/// Gets or sets the search string used when searching for menu items.
		/// </summary>
		public string SearchString { get; set; }

		/// <summary>
		/// Gets the next visual menu item.
		/// </summary>
		public OdinMenuItem NextVisualMenuItem
		{
			get
			{
				EnsureInitialized();
				if (MenuTree.DrawInSearchMode)
				{
					return nextMenuItemFlat;
				}
				if (ChildMenuItems.Count > 0 && nextMenuItem != null && !Toggled && _IsVisible())
				{
					return nextMenuItem;
				}
				return GetAllNextMenuItems().FirstOrDefault((OdinMenuItem x) => x._IsVisible());
			}
		}

		/// <summary>
		/// Gets the parent menu item.
		/// </summary>
		public OdinMenuItem Parent
		{
			get
			{
				EnsureInitialized();
				return parentMenuItem;
			}
		}

		/// <summary>
		/// Gets the previous visual menu item.
		/// </summary>
		public OdinMenuItem PrevVisualMenuItem
		{
			get
			{
				EnsureInitialized();
				if (MenuTree.DrawInSearchMode)
				{
					return previousMenuItemFlat;
				}
				if (ChildMenuItems.Count > 0 && !Toggled && _IsVisible())
				{
					if (previousMenuItem != null)
					{
						if (previousMenuItem.ChildMenuItems.Count == 0 || !previousMenuItem.Toggled)
						{
							return previousMenuItem;
						}
					}
					else if (parentMenuItem != null)
					{
						return parentMenuItem;
					}
				}
				return GetAllPreviousMenuItems().FirstOrDefault((OdinMenuItem x) => x._IsVisible());
			}
		}

		/// <summary>
		/// Gets the drawn rect.
		/// </summary>
		public Rect Rect => rect;

		/// <summary>
		/// Gets the drawn label rect.
		/// </summary>
		public Rect LabelRect => labelRect;

		/// <summary>
		/// Gets or sets the style. If null is specified, then the menu trees DefaultMenuStyle is used.
		/// </summary>
		public OdinMenuStyle Style
		{
			get
			{
				if (style == null)
				{
					style = menuTree.DefaultMenuStyle;
				}
				return style;
			}
			set
			{
				style = value;
			}
		}

		/// <summary>
		/// Gets the first object of the <see cref="P:Sirenix.OdinInspector.Editor.OdinMenuItem.ObjectInstances" />
		/// </summary>
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value instead.", false)]
		public virtual object ObjectInstance
		{
			get
			{
				if (Value == null)
				{
					return null;
				}
				IList list = Value as IList;
				object obj = ((list != null && list.Count != 0) ? list[0] : Value);
				Func<object> func = obj as Func<object>;
				if (func != null)
				{
					return func();
				}
				return obj;
			}
		}

		/// <summary>
		/// Gets the object instances the menu item represents
		/// </summary>
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value as IEnumerable instead.", false)]
		public virtual IEnumerable<object> ObjectInstances
		{
			get
			{
				IList list = Value as IList;
				if (list == null || list.Count == 0)
				{
					yield break;
				}
				foreach (object item in list)
				{
					if (item == null)
					{
						yield return null;
					}
					object instance = item;
					Func<object> func = instance as Func<object>;
					if (func != null)
					{
						yield return func();
					}
					else
					{
						yield return instance;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the value the menu item represents.
		/// </summary>
		public object Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		/// <summary>
		/// Gets a nice menu item name. If the raw name value is null or a dollar sign, then the name is retrieved from the object itself via ToString().
		/// </summary>
		public virtual string SmartName
		{
			get
			{
				object obj = value;
				Func<object> func = Value as Func<object>;
				if (func != null)
				{
					obj = func();
				}
				if (name == null || name == "$")
				{
					if (obj == null)
					{
						return "";
					}
					Object val = obj as Object;
					if (Object.op_Implicit(val))
					{
						return val.get_name().SplitPascalCase();
					}
					return obj.ToString();
				}
				return name;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" /> is toggled / expanded. This value tries it best to be persistent.
		/// </summary>
		public virtual bool Toggled
		{
			get
			{
				if (childMenuItems.Count == 0)
				{
					return false;
				}
				if (menuTree.Config.UseCachedExpandedStates)
				{
					if (isToggledContext == null)
					{
						isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + GetFullPath(), DefaultToggledState));
					}
					return isToggledContext.Value;
				}
				if (!nonCachedToggledState.HasValue)
				{
					nonCachedToggledState = DefaultToggledState;
				}
				return nonCachedToggledState.Value;
			}
			set
			{
				if (menuTree.Config.UseCachedExpandedStates)
				{
					if (isToggledContext == null)
					{
						isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + GetFullPath(), DefaultToggledState));
					}
					isToggledContext.Value = value;
				}
				else
				{
					nonCachedToggledState = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the icon getter.
		/// </summary>
		public Func<Texture> IconGetter
		{
			get
			{
				if (iconGetter == null)
				{
					iconGetter = delegate
					{
						if (!IsSelected)
						{
							return Icon;
						}
						return (!Object.op_Implicit((Object)(object)IconSelected)) ? Icon : IconSelected;
					};
				}
				return iconGetter;
			}
			set
			{
				iconGetter = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" /> class.
		/// </summary>
		/// <param name="tree">The Odin menu tree instance the menu item belongs to.</param>
		/// <param name="name">The name of the menu item.</param>
		/// <param name="value">The instance the value item represents.</param>
		public OdinMenuItem(OdinMenuTree tree, string name, object value)
		{
			if (tree == null)
			{
				throw new ArgumentNullException("tree");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			menuTree = tree;
			this.name = name;
			SearchString = name;
			Value = value;
			childMenuItems = new List<OdinMenuItem>();
		}

		/// <summary>
		/// Deselects this instance.
		/// </summary>
		public bool Deselect()
		{
			return menuTree.Selection.Remove(this);
		}

		/// <summary>
		/// Selects the specified add to selection.
		/// </summary>
		public void Select(bool addToSelection = false)
		{
			if (!addToSelection)
			{
				menuTree.Selection.Clear();
			}
			menuTree.Selection.Add(this);
		}

		/// <summary>
		/// Gets the child menu items recursive in a DFS.
		/// </summary>
		/// <param name="includeSelf">Whether to include it self in the collection.</param>
		public IEnumerable<OdinMenuItem> GetChildMenuItemsRecursive(bool includeSelf)
		{
			if (includeSelf)
			{
				yield return this;
			}
			foreach (OdinMenuItem item in ChildMenuItems.SelectMany((OdinMenuItem x) => x.GetChildMenuItemsRecursive(includeSelf: true)))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Gets the child menu items recursive in a DFS.
		/// </summary>
		/// <param name="includeSelf">Whether to include it self in the collection.</param>
		/// <param name="includeRoot">Whether to include the root.</param>
		public IEnumerable<OdinMenuItem> GetParentMenuItemsRecursive(bool includeSelf, bool includeRoot = false)
		{
			if (includeSelf || (Parent == null && includeRoot))
			{
				yield return this;
			}
			if (Parent == null)
			{
				yield break;
			}
			foreach (OdinMenuItem item in Parent.GetParentMenuItemsRecursive(includeSelf: true, includeRoot))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Gets the full menu item path.
		/// </summary>
		public string GetFullPath()
		{
			EnsureInitialized();
			OdinMenuItem parent = Parent;
			if (parent == null)
			{
				return SmartName;
			}
			return parent.GetFullPath() + "/" + SmartName;
		}

		/// <summary>
		/// Sets the object instance
		/// </summary>
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value = obj instead.", false)]
		public void SetObjectInstance(object obj)
		{
			Value = obj;
		}

		/// <summary>
		/// Sets the object instances
		/// </summary>
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value = obj instead.", false)]
		public void SetObjectInstances(IList objects)
		{
			Value = objects;
		}

		/// <summary>
		/// Draws this menu item followed by all of its child menu items
		/// </summary>
		/// <param name="indentLevel">The indent level.</param>
		public virtual void DrawMenuItems(int indentLevel)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Invalid comparison between Unknown and I4
			DrawMenuItem(indentLevel);
			List<OdinMenuItem> list = ChildMenuItems;
			int count = list.Count;
			if (count == 0)
			{
				return;
			}
			bool toggled = Toggled;
			if (t < 0f)
			{
				t = (toggled ? 1 : 0);
			}
			if ((int)OdinMenuTree.CurrentEventType == 8)
			{
				t = Mathf.MoveTowards(t, (float)(toggled ? 1 : 0), OdinMenuTree.CurrentEditorTimeHelperDeltaTime * (1f / SirenixEditorGUI.DefaultFadeGroupDuration));
			}
			if (SirenixEditorGUI.BeginFadeGroup(t))
			{
				for (int i = 0; i < count; i++)
				{
					list[i].DrawMenuItems(indentLevel + 1);
				}
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		/// <summary>
		/// Draws the menu item with the specified indent level.
		/// </summary>
		public virtual void DrawMenuItem(int indentLevel)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Invalid comparison between Unknown and I4
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Invalid comparison between Unknown and I4
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Invalid comparison between Unknown and I4
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Invalid comparison between Unknown and I4
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_025d: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Invalid comparison between Unknown and I4
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_032e: Unknown result type (might be due to invalid IL or missing references)
			//IL_035e: Unknown result type (might be due to invalid IL or missing references)
			//IL_036e: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0383: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0423: Unknown result type (might be due to invalid IL or missing references)
			//IL_042d: Unknown result type (might be due to invalid IL or missing references)
			//IL_043d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0442: Unknown result type (might be due to invalid IL or missing references)
			//IL_0448: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0521: Unknown result type (might be due to invalid IL or missing references)
			//IL_0527: Unknown result type (might be due to invalid IL or missing references)
			//IL_0547: Unknown result type (might be due to invalid IL or missing references)
			//IL_054d: Unknown result type (might be due to invalid IL or missing references)
			Rect val = (EXPERIMENTAL_DontAllocateNewRect ? rect : GUILayoutUtility.GetRect(0f, (float)Style.Height));
			Event currentEvent = OdinMenuTree.CurrentEvent;
			EventType currentEventType = OdinMenuTree.CurrentEventType;
			if ((int)currentEventType == 8)
			{
				return;
			}
			if ((int)currentEventType == 7 || ((int)currentEventType != 8 && ((Rect)(ref rect)).get_width() == 0f))
			{
				rect = val;
			}
			float y = ((Rect)(ref rect)).get_y();
			if (y > 1000f)
			{
				float y2 = ((Rect)(ref OdinMenuTree.VisibleRect)).get_y();
				if (y + ((Rect)(ref rect)).get_height() < y2 || y > y2 + ((Rect)(ref OdinMenuTree.VisibleRect)).get_height())
				{
					MenuItemIsBeingRendered = false;
					return;
				}
			}
			MenuItemIsBeingRendered = true;
			if ((int)currentEventType == 7)
			{
				float num = Style.Offset + (float)indentLevel * Style.IndentAmount;
				labelRect = rect.AddXMin(num);
				bool isSelected = IsSelected;
				if (isSelected)
				{
					if (OdinMenuTree.ActiveMenuTree == menuTree)
					{
						if (EditorGUIUtility.get_isProSkin())
						{
							EditorGUI.DrawRect(rect, Style.SelectedColorDarkSkin);
						}
						else
						{
							EditorGUI.DrawRect(rect, Style.SelectedColorLightSkin);
						}
					}
					else if (EditorGUIUtility.get_isProSkin())
					{
						EditorGUI.DrawRect(rect, Style.SelectedInactiveColorDarkSkin);
					}
					else
					{
						EditorGUI.DrawRect(rect, Style.SelectedInactiveColorLightSkin);
					}
				}
				if (!isSelected && ((Rect)(ref rect)).Contains(currentEvent.get_mousePosition()))
				{
					EditorGUI.DrawRect(rect, mouseOverColor);
				}
				if (ChildMenuItems.Count > 0 && !MenuTree.DrawInSearchMode && Style.DrawFoldoutTriangle)
				{
					EditorIcon editorIcon = (Toggled ? EditorIcons.TriangleDown : EditorIcons.TriangleRight);
					if (Style.AlignTriangleLeft)
					{
						triangleRect = labelRect.AlignLeft(Style.TriangleSize).AlignMiddle(Style.TriangleSize);
						ref Rect reference = ref triangleRect;
						((Rect)(ref reference)).set_x(((Rect)(ref reference)).get_x() - (Style.TriangleSize - Style.TrianglePadding));
					}
					else
					{
						triangleRect = rect.AlignRight(Style.TriangleSize).AlignMiddle(Style.TriangleSize);
						ref Rect reference2 = ref triangleRect;
						((Rect)(ref reference2)).set_x(((Rect)(ref reference2)).get_x() - Style.TrianglePadding);
					}
					if ((int)currentEventType == 7)
					{
						if (EditorGUIUtility.get_isProSkin())
						{
							if (isSelected || ((Rect)(ref triangleRect)).Contains(currentEvent.get_mousePosition()))
							{
								GUI.DrawTexture(triangleRect, editorIcon.Highlighted);
							}
							else
							{
								GUI.DrawTexture(triangleRect, editorIcon.Active);
							}
						}
						else if (isSelected)
						{
							GUI.DrawTexture(triangleRect, (Texture)(object)editorIcon.Raw);
						}
						else if (((Rect)(ref triangleRect)).Contains(currentEvent.get_mousePosition()))
						{
							GUI.DrawTexture(triangleRect, editorIcon.Active);
						}
						else
						{
							GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.7f));
							GUI.DrawTexture(triangleRect, editorIcon.Active);
							GUIHelper.PopColor();
						}
					}
				}
				Texture val2 = IconGetter();
				if (Object.op_Implicit((Object)(object)val2))
				{
					Rect val3 = labelRect.AlignLeft(Style.IconSize).AlignMiddle(Style.IconSize);
					((Rect)(ref val3)).set_x(((Rect)(ref val3)).get_x() + Style.IconOffset);
					if (!isSelected)
					{
						GUIHelper.PushColor(new Color(1f, 1f, 1f, Style.NotSelectedIconAlpha));
					}
					GUI.DrawTexture(val3, val2, (ScaleMode)2);
					ref Rect reference3 = ref labelRect;
					((Rect)(ref reference3)).set_xMin(((Rect)(ref reference3)).get_xMin() + (Style.IconSize + Style.IconPadding));
					if (!isSelected)
					{
						GUIHelper.PopColor();
					}
				}
				GUIStyle val4 = (isSelected ? Style.SelectedLabelStyle : Style.DefaultLabelStyle);
				labelRect = labelRect.AlignMiddle(16f).AddY(Style.LabelVerticalOffset);
				GUI.Label(labelRect, SmartName, val4);
				if (Style.Borders)
				{
					float num2 = Style.BorderPadding;
					bool flag = true;
					if (isSelected || previousMenuItemWasSelected)
					{
						num2 = 0f;
						if (!EditorGUIUtility.get_isProSkin())
						{
							flag = false;
						}
					}
					previousMenuItemWasSelected = isSelected;
					if (flag)
					{
						Rect val5 = rect;
						((Rect)(ref val5)).set_x(((Rect)(ref val5)).get_x() + num2);
						((Rect)(ref val5)).set_width(((Rect)(ref val5)).get_width() - num2 * 2f);
						SirenixEditorGUI.DrawHorizontalLineSeperator(((Rect)(ref val5)).get_x(), ((Rect)(ref val5)).get_y(), ((Rect)(ref val5)).get_width(), Style.BorderAlpha);
					}
				}
			}
			wasMouseDownEvent = (int)currentEventType == 0 && ((Rect)(ref rect)).Contains(currentEvent.get_mousePosition());
			if (wasMouseDownEvent)
			{
				handleClickEventOnMouseUp = this;
			}
			OnDrawMenuItem(rect, labelRect);
			if (OnDrawItem != null)
			{
				OnDrawItem(this);
			}
			HandleMouseEvents(rect, triangleRect);
		}

		/// <summary>
		/// Override this to add custom GUI to the menu items.
		/// This is called right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
		/// </summary>
		protected virtual void OnDrawMenuItem(Rect rect, Rect labelRect)
		{
		}

		internal void UpdateMenuTreeRecursive(bool isRoot = false)
		{
			isInitialized = true;
			OdinMenuItem odinMenuItem = null;
			foreach (OdinMenuItem childMenuItem in ChildMenuItems)
			{
				childMenuItem.parentMenuItem = null;
				childMenuItem.nextMenuItem = null;
				childMenuItem.previousMenuItemFlat = null;
				childMenuItem.nextMenuItemFlat = null;
				childMenuItem.previousMenuItem = null;
				if (!isRoot)
				{
					childMenuItem.parentMenuItem = this;
				}
				if (odinMenuItem != null)
				{
					odinMenuItem.nextMenuItem = childMenuItem;
					childMenuItem.previousMenuItem = odinMenuItem;
				}
				odinMenuItem = childMenuItem;
				childMenuItem.UpdateMenuTreeRecursive();
			}
		}

		internal void UpdateFlatMenuItemNavigation()
		{
			int num = 0;
			OdinMenuItem odinMenuItem = null;
			IEnumerable<OdinMenuItem> enumerable;
			if (!menuTree.DrawInSearchMode)
			{
				enumerable = menuTree.EnumerateTree();
			}
			else
			{
				IEnumerable<OdinMenuItem> flatMenuTree = menuTree.FlatMenuTree;
				enumerable = flatMenuTree;
			}
			IEnumerable<OdinMenuItem> enumerable2 = enumerable;
			foreach (OdinMenuItem item in enumerable2)
			{
				item.flatTreeIndex = num++;
				item.nextMenuItemFlat = null;
				item.previousMenuItemFlat = null;
				if (odinMenuItem != null)
				{
					item.previousMenuItemFlat = odinMenuItem;
					odinMenuItem.nextMenuItemFlat = item;
				}
				odinMenuItem = item;
			}
		}

		/// <summary>
		/// Handles the mouse events.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <param name="triangleRect">The triangle rect.</param>
		protected void HandleMouseEvents(Rect rect, Rect triangleRect)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Invalid comparison between Unknown and I4
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Invalid comparison between Unknown and I4
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Invalid comparison between Unknown and I4
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Invalid comparison between Unknown and I4
			EventType type = Event.get_current().get_type();
			if ((int)type == 12 && wasMouseDownEvent)
			{
				wasMouseDownEvent = false;
				handleClickEventOnMouseUp = this;
			}
			if ((int)type != 0 && ((int)type != 1 || handleClickEventOnMouseUp != this))
			{
				return;
			}
			handleClickEventOnMouseUp = null;
			wasMouseDownEvent = false;
			if (!((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				return;
			}
			bool flag = ChildMenuItems.Any();
			bool isSelected = IsSelected;
			if (Event.get_current().get_button() == 1 && OnRightClick != null)
			{
				OnRightClick(this);
			}
			if (Event.get_current().get_button() == 0)
			{
				bool flag2 = false;
				if (flag)
				{
					if (isSelected && (int)Event.get_current().get_modifiers() == 0)
					{
						flag2 = true;
					}
					else if (((Rect)(ref triangleRect)).Contains(Event.get_current().get_mousePosition()))
					{
						flag2 = true;
					}
				}
				if (flag2 && ((Rect)(ref triangleRect)).Contains(Event.get_current().get_mousePosition()))
				{
					bool toggled = !Toggled;
					if ((int)Event.get_current().get_modifiers() == 4)
					{
						foreach (OdinMenuItem item in GetChildMenuItemsRecursive(includeSelf: true))
						{
							item.Toggled = toggled;
						}
					}
					else
					{
						Toggled = toggled;
					}
				}
				else if (menuTree.Selection.SupportsMultiSelect && (int)Event.get_current().get_modifiers() == 1 && menuTree.Selection.Count > 0)
				{
					OdinMenuItem odinMenuItem = menuTree.Selection.First();
					int num = Mathf.Abs(odinMenuItem.FlatTreeIndex - FlatTreeIndex) + 1;
					bool flag3 = odinMenuItem.FlatTreeIndex < FlatTreeIndex;
					menuTree.Selection.Clear();
					for (int i = 0; i < num; i++)
					{
						if (odinMenuItem == null)
						{
							break;
						}
						odinMenuItem.Select(addToSelection: true);
						if (odinMenuItem == this)
						{
							break;
						}
						odinMenuItem = (flag3 ? odinMenuItem.NextVisualMenuItem : odinMenuItem.PrevVisualMenuItem);
					}
				}
				else
				{
					bool flag4 = (int)Event.get_current().get_modifiers() == 2;
					if (flag4 && isSelected && MenuTree.Selection.SupportsMultiSelect)
					{
						Deselect();
					}
					else
					{
						Select(flag4);
					}
					if (MenuTree.Config.ConfirmSelectionOnDoubleClick && Event.get_current().get_clickCount() == 2)
					{
						MenuTree.Selection.ConfirmSelection();
					}
				}
			}
			GUIHelper.RemoveFocusControl();
			Event.get_current().Use();
		}

		internal bool _IsVisible()
		{
			if (menuTree.DrawInSearchMode)
			{
				return menuTree.FlatMenuTree.Contains(this);
			}
			return !ParentMenuItemsBottomUp(includeSelf: false).Any((OdinMenuItem x) => !x.Toggled);
		}

		internal void SetChildMenuItems(List<OdinMenuItem> newChildMenuItems)
		{
			childMenuItems = newChildMenuItems ?? new List<OdinMenuItem>();
		}

		private IEnumerable<OdinMenuItem> GetAllNextMenuItems()
		{
			if (nextMenuItemFlat == null)
			{
				yield break;
			}
			yield return nextMenuItemFlat;
			foreach (OdinMenuItem allNextMenuItem in nextMenuItemFlat.GetAllNextMenuItems())
			{
				yield return allNextMenuItem;
			}
		}

		private IEnumerable<OdinMenuItem> GetAllPreviousMenuItems()
		{
			if (previousMenuItemFlat == null)
			{
				yield break;
			}
			yield return previousMenuItemFlat;
			foreach (OdinMenuItem allPreviousMenuItem in previousMenuItemFlat.GetAllPreviousMenuItems())
			{
				yield return allPreviousMenuItem;
			}
		}

		private IEnumerable<OdinMenuItem> ParentMenuItemsBottomUp(bool includeSelf = true)
		{
			if (parentMenuItem != null)
			{
				foreach (OdinMenuItem item in parentMenuItem.ParentMenuItemsBottomUp())
				{
					yield return item;
				}
			}
			if (includeSelf)
			{
				yield return this;
			}
		}

		private void EnsureInitialized()
		{
			if (!isInitialized)
			{
				menuTree.UpdateMenuTree();
				if (!isInitialized)
				{
					Debug.LogWarning((object)"Could not initialize menu item. Is the menu item not part of a menu tree?");
				}
			}
		}
	}
}
