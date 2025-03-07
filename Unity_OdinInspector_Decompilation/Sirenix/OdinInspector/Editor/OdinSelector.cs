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
	/// OdinSelectors is an abstract base class that combines OdinMenuTrees and OdinEditorWindows to help making feature-rich selectors and popup selectors.
	/// </summary>
	/// <example>
	/// <code>
	/// public class MySelector : OdinSelector&lt;SomeType&gt;
	/// {
	///     private readonly List&lt;SomeType&gt; source;
	///     private readonly bool supportsMultiSelect;
	///
	///     public MySelector(List&lt;SomeType&gt; source, bool supportsMultiSelect)
	///     {
	///         this.source = source;
	///         this.supportsMultiSelect = supportsMultiSelect;
	///     }
	///
	///     protected override void BuildSelectionTree(OdinMenuTree tree)
	///     {
	///         tree.Config.DrawSearchToolbar = true;
	///         tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
	///
	///         tree.Add("Defaults/None", null);
	///         tree.Add("Defaults/A", new SomeType());
	///         tree.Add("Defaults/B", new SomeType());
	///
	///         tree.AddRange(this.source, x =&gt; x.Path, x =&gt; x.SomeTexture);
	///     }
	///
	///     [OnInspectorGUI]
	///     private void DrawInfoAboutSelectedItem()
	///     {
	///         SomeType selected = this.GetCurrentSelection().FirstOrDefault();
	///
	///         if (selected != null)
	///         {
	///             GUILayout.Label("Name: " + selected.Name);
	///             GUILayout.Label("Data: " + selected.Data);
	///         }
	///     }
	/// }
	/// </code>
	/// Usage:
	/// <code>
	/// void OnGUI()
	/// {
	///     if (GUILayout.Button("Open My Selector"))
	///     {
	///         List&lt;SomeType&gt; source = this.GetListOfThingsToSelectFrom();
	///         MySelector selector = new MySelector(source, false);
	///
	///         selector.SetSelection(this.someValue);
	///
	///         selector.SelectionCancelled += () =&gt; { };  // Occurs when the popup window is closed, and no slection was confirmed.
	///         selector.SelectionChanged += col =&gt; { };
	///         selector.SelectionConfirmed += col =&gt; this.someValue = col.FirstOrDefault();
	///
	///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
	///     }
	/// }
	///
	/// // All Odin Selectors can be rendered anywhere with Odin.
	/// [ShowInInspector]
	/// MySelector inlineSelector;
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.EnumSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.TypeSelector" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public abstract class OdinSelector<T>
	{
		private static EditorWindow selectorFieldWindow = null;

		private static IEnumerable<T> selectedValues = null;

		private static bool selectionWasConfirmed = false;

		private static bool selectionWasChanged = false;

		private static int confirmedPopupControlId = -1;

		private static int focusedControlId = -1;

		private static GUIStyle titleStyle = null;

		private OdinEditorWindow popupWindowInstance;

		private OdinMenuTree selectionTree;

		/// <summary>
		/// If true, a confirm selection button will be drawn in the title-bar.
		/// </summary>
		[HideInInspector]
		public bool DrawConfirmSelectionButton;

		[SerializeField]
		[HideInInspector]
		private OdinMenuTreeDrawingConfig config = new OdinMenuTreeDrawingConfig
		{
			SearchToolbarHeight = 22,
			AutoScrollOnSelectionChanged = true,
			DefaultMenuStyle = new OdinMenuStyle
			{
				Height = 22
			}
		};

		private static bool wasKeyboard;

		private static int prevKeyboardId;

		private static GUIContent tmpValueLabel;

		/// <summary>
		/// Gets the selection menu tree.
		/// </summary>
		public OdinMenuTree SelectionTree
		{
			get
			{
				if (selectionTree == null)
				{
					selectionTree = new OdinMenuTree(supportsMultiSelect: true);
					selectionTree.Config = config;
					OdinMenuTree.ActiveMenuTree = selectionTree;
					BuildSelectionTree(selectionTree);
					selectionTree.Selection.SelectionConfirmed += delegate
					{
						if (this.SelectionConfirmed != null)
						{
							IEnumerable<T> currentSelection = GetCurrentSelection();
							if (IsValidSelection(currentSelection))
							{
								this.SelectionConfirmed(currentSelection);
							}
						}
					};
					selectionTree.Selection.SelectionChanged += delegate
					{
						TriggerSelectionChanged();
					};
				}
				return selectionTree;
			}
		}

		/// <summary>
		/// Gets the title. No title will be drawn if the string is null or empty.
		/// </summary>
		public virtual string Title => null;

		/// <summary>
		/// Occurs when the window is closed, and no slection was confirmed.
		/// </summary>
		public event Action SelectionCancelled;

		/// <summary>
		/// Occurs when the menuTrees selection is changed and IsValidSelection returns true.
		/// </summary>
		public event Action<IEnumerable<T>> SelectionChanged;

		/// <summary>
		/// Occurs when the menuTrees selection is confirmed and IsValidSelection returns true.
		/// </summary>
		public event Action<IEnumerable<T>> SelectionConfirmed;

		/// <summary>
		/// Enables the single click to select.
		/// </summary>
		public void EnableSingleClickToSelect()
		{
			SelectionTree.EnumerateTree(delegate(OdinMenuItem x)
			{
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Remove(x.OnDrawItem, new Action<OdinMenuItem>(EnableSingleClickToSelect));
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Remove(x.OnDrawItem, new Action<OdinMenuItem>(EnableSingleClickToSelect));
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(EnableSingleClickToSelect));
			});
		}

		private void EnableSingleClickToSelect(OdinMenuItem obj)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Invalid comparison between Unknown and I4
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Invalid comparison between Unknown and I4
			EventType type = Event.get_current().get_type();
			if ((int)type == 8)
			{
				return;
			}
			Rect rect = obj.Rect;
			if (((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				GUIHelper.RequestRepaint();
				if ((int)Event.get_current().get_type() == 3 && obj is T && IsValidSelection(Enumerable.Repeat((T)obj.Value, 1)))
				{
					obj.Select();
				}
				if ((int)type == 1 && obj.ChildMenuItems.Count == 0)
				{
					obj.MenuTree.Selection.ConfirmSelection();
					Event.get_current().Use();
				}
			}
		}

		/// <summary>
		/// Gets the current selection from the menu tree whether it's valid or not.
		/// </summary>
		public virtual IEnumerable<T> GetCurrentSelection()
		{
			return SelectionTree.Selection.Select((OdinMenuItem x) => x.Value).OfType<T>();
		}

		/// <summary>
		/// Determines whether the specified collection is a valid collection.
		/// If false, the SlectionChanged and SelectionConfirm events will not be called.
		/// By default, this returns true if the collection contains one or more items.
		/// </summary>
		public virtual bool IsValidSelection(IEnumerable<T> collection)
		{
			return true;
		}

		/// <summary>
		/// Sets the selection.
		/// </summary>
		public virtual void SetSelection(IEnumerable<T> selection)
		{
			SelectionTree.Selection.Clear();
			if (selection == null)
			{
				return;
			}
			foreach (T item in selection)
			{
				SetSelection(item);
			}
		}

		/// <summary>
		/// Sets the selection.
		/// </summary>
		public virtual void SetSelection(T selected)
		{
			if (selected != null)
			{
				List<OdinMenuItem> list = (from x in SelectionTree.EnumerateTree()
					where x.Value is T
					where EqualityComparer<T>.Default.Equals((T)x.Value, selected)
					select x).ToList();
				list.ForEach(delegate(OdinMenuItem x)
				{
					x.Select(addToSelection: true);
				});
			}
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
		/// </summary>
		public OdinEditorWindow ShowInPopup()
		{
			EditorWindow focusedWindow = EditorWindow.get_focusedWindow();
			float num = DefaultWindowWidth();
			OdinEditorWindow odinEditorWindow = ((num != 0f) ? OdinEditorWindow.InspectObjectInDropDown(this, num) : OdinEditorWindow.InspectObjectInDropDown(this));
			SetupWindow(odinEditorWindow, focusedWindow);
			return odinEditorWindow;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Rect btnRect)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ShowInPopup(btnRect, ((Rect)(ref btnRect)).get_width());
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Rect btnRect, float windowWidth)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			EditorWindow focusedWindow = EditorWindow.get_focusedWindow();
			OdinEditorWindow odinEditorWindow = OdinEditorWindow.InspectObjectInDropDown(this, btnRect, windowWidth);
			SetupWindow(odinEditorWindow, focusedWindow);
			return odinEditorWindow;
		}

		/// <summary>
		/// The mouse position is used as the position for the window.
		/// Opens up the selector instance in a popup at the specified position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(float windowWidth)
		{
			EditorWindow focusedWindow = EditorWindow.get_focusedWindow();
			OdinEditorWindow odinEditorWindow = OdinEditorWindow.InspectObjectInDropDown(this, windowWidth);
			SetupWindow(odinEditorWindow, focusedWindow);
			return odinEditorWindow;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Vector2 position, float windowWidth)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			EditorWindow focusedWindow = EditorWindow.get_focusedWindow();
			OdinEditorWindow odinEditorWindow = OdinEditorWindow.InspectObjectInDropDown(this, position, windowWidth);
			SetupWindow(odinEditorWindow, focusedWindow);
			return odinEditorWindow;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Rect btnRect, Vector2 windowSize)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			EditorWindow focusedWindow = EditorWindow.get_focusedWindow();
			OdinEditorWindow odinEditorWindow = OdinEditorWindow.InspectObjectInDropDown(this, btnRect, windowSize);
			SetupWindow(odinEditorWindow, focusedWindow);
			return odinEditorWindow;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified position.
		/// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Vector2 position)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			EditorWindow focusedWindow = EditorWindow.get_focusedWindow();
			float num = DefaultWindowWidth();
			OdinEditorWindow odinEditorWindow = ((num != 0f) ? OdinEditorWindow.InspectObjectInDropDown(this, position, num) : OdinEditorWindow.InspectObjectInDropDown(this, position));
			SetupWindow(odinEditorWindow, focusedWindow);
			return odinEditorWindow;
		}

		/// <summary>
		/// Opens up the selector instance in a popup with the specified width and height.
		/// The mouse position is used as the position for the window.
		/// </summary>
		public OdinEditorWindow ShowInPopup(float width, float height)
		{
			EditorWindow focusedWindow = EditorWindow.get_focusedWindow();
			OdinEditorWindow odinEditorWindow = OdinEditorWindow.InspectObjectInDropDown(this, width, height);
			SetupWindow(odinEditorWindow, focusedWindow);
			return odinEditorWindow;
		}

		/// <summary>
		/// Builds the selection tree.
		/// </summary>
		protected abstract void BuildSelectionTree(OdinMenuTree tree);

		/// <summary>
		/// When ShowInPopup is called, without a specifed window width, this methods gets called.
		/// Here you can calculate and give a good default width for the popup. 
		/// The default implementation returns 0, which will let the popup window determain the width itself. This is usually a fixed value.
		/// </summary>
		protected virtual float DefaultWindowWidth()
		{
			return 0f;
		}

		/// <summary>
		/// Triggers the selection changed event, but only if the current selection is valid.
		/// </summary>
		protected void TriggerSelectionChanged()
		{
			if (this.SelectionChanged != null)
			{
				IEnumerable<T> currentSelection = GetCurrentSelection();
				if (IsValidSelection(currentSelection))
				{
					this.SelectionChanged(currentSelection);
				}
			}
		}

		/// <summary>
		/// Draw the selecotr manually.
		/// </summary>
		public void OnInspectorGUI()
		{
			DrawSelectionTree();
		}

		/// <summary>
		/// Draws the selection tree. This gets drawn using the OnInspectorGUI attribute.
		/// </summary>
		[OnInspectorGUI]
		[PropertyOrder(-1f)]
		protected virtual void DrawSelectionTree()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_0097: Expected O, but got Unknown
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Invalid comparison between Unknown and I4
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Expected O, but got Unknown
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			Rect val = EditorGUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			EditorGUI.DrawRect(val, SirenixGUIStyles.DarkEditorBackground);
			GUILayout.Space(1f);
			bool flag = !string.IsNullOrEmpty(Title);
			bool drawSearchToolbar = SelectionTree.Config.DrawSearchToolbar;
			bool drawConfirmSelectionButton = DrawConfirmSelectionButton;
			if (flag || drawSearchToolbar || drawConfirmSelectionButton)
			{
				SirenixEditorGUI.BeginHorizontalToolbar(SelectionTree.Config.SearchToolbarHeight, 4);
				if (flag)
				{
					if (titleStyle == null)
					{
						GUIStyle val2 = new GUIStyle(SirenixGUIStyles.LeftAlignedCenteredLabel);
						val2.set_padding(new RectOffset(10, 10, 0, 0));
						titleStyle = val2;
					}
					Rect rect = GUILayoutUtility.GetRect(new GUIContent(Title), titleStyle, (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false).Height(SelectionTree.Config.SearchToolbarHeight));
					if ((int)Event.get_current().get_type() == 7)
					{
						((Rect)(ref rect)).set_y(((Rect)(ref rect)).get_y() - 2f);
						GUI.Label(rect.AlignCenterY(16f), Title, titleStyle);
					}
				}
				if (drawSearchToolbar)
				{
					SelectionTree.DrawSearchToolbar(GUIStyle.get_none());
				}
				else
				{
					GUILayout.FlexibleSpace();
				}
				EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1f), SirenixGUIStyles.BorderColor);
				if (drawConfirmSelectionButton && SirenixEditorGUI.ToolbarButton(new GUIContent((Texture)(object)EditorIcons.TestPassed)))
				{
					SelectionTree.Selection.ConfirmSelection();
				}
				SirenixEditorGUI.EndHorizontalToolbar();
			}
			bool drawSearchToolbar2 = SelectionTree.Config.DrawSearchToolbar;
			SelectionTree.Config.DrawSearchToolbar = false;
			if (SelectionTree.MenuItems.Count == 0)
			{
				GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				SirenixEditorGUI.InfoMessageBox("There are no possible values to select.");
				GUILayout.EndVertical();
			}
			SelectionTree.DrawMenuTree();
			SelectionTree.Config.DrawSearchToolbar = drawSearchToolbar2;
			SirenixEditorGUI.DrawBorders(val, 1);
			EditorGUILayout.EndVertical();
		}

		private void SetupWindow(OdinEditorWindow window, EditorWindow prevSelectedWindow)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			int prevFocusId = GUIUtility.get_hotControl();
			int prevKeybaorFocus = GUIUtility.get_keyboardControl();
			popupWindowInstance = window;
			window.WindowPadding = default(Vector4);
			bool wasConfirmed = false;
			SelectionTree.Selection.SelectionConfirmed += delegate
			{
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Invalid comparison between Unknown and I4
				bool ctrl = Event.get_current() != null && (int)Event.get_current().get_modifiers() != 2;
				UnityEditorEventUtility.DelayAction(delegate
				{
					if (IsValidSelection(GetCurrentSelection()))
					{
						wasConfirmed = true;
						if (ctrl)
						{
							((EditorWindow)window).Close();
							if (Object.op_Implicit((Object)(object)prevSelectedWindow))
							{
								prevSelectedWindow.Focus();
							}
						}
					}
				});
			};
			window.OnBeginGUI += delegate
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Invalid comparison between Unknown and I4
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Invalid comparison between Unknown and I4
				if ((int)Event.get_current().get_type() == 4 && (int)Event.get_current().get_keyCode() == 27)
				{
					UnityEditorEventUtility.DelayAction(delegate
					{
						((EditorWindow)window).Close();
					});
					if (Object.op_Implicit((Object)(object)prevSelectedWindow))
					{
						prevSelectedWindow.Focus();
					}
					Event.get_current().Use();
				}
			};
			window.OnClose += delegate
			{
				if (!wasConfirmed && this.SelectionCancelled != null)
				{
					this.SelectionCancelled();
				}
				GUIUtility.set_hotControl(prevFocusId);
				GUIUtility.set_keyboardControl(prevKeybaorFocus);
			};
		}

		internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, string label, GUIStyle style, int id, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter) where TSelector : OdinSelector<T>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			return DrawSelectorButton(buttonRect, new GUIContent(label), style, id, out bindSelector, out resultGetter);
		}

		internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, GUIContent label, GUIStyle style, int id, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter) where TSelector : OdinSelector<T>
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Invalid comparison between Unknown and I4
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Invalid comparison between Unknown and I4
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Invalid comparison between Unknown and I4
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Invalid comparison between Unknown and I4
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Invalid comparison between Unknown and I4
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Invalid comparison between Unknown and I4
			bool result = false;
			bindSelector = null;
			resultGetter = null;
			if ((int)Event.get_current().get_type() == 7)
			{
				bool flag = GUIUtility.get_hotControl() == id || focusedControlId == id;
				style = style ?? EditorStyles.get_popup();
				style.Draw(buttonRect, label, flag, flag, false, GUIUtility.get_keyboardControl() == id);
			}
			bool flag2 = false;
			if ((int)Event.get_current().get_keyCode() == 13 && (int)Event.get_current().get_type() == 4 && GUIUtility.get_keyboardControl() == id)
			{
				GUIUtility.set_hotControl(id);
				wasKeyboard = true;
			}
			else if (GUIUtility.get_hotControl() == id && (int)Event.get_current().get_keyCode() == 13 && (int)Event.get_current().get_type() == 5 && GUIUtility.get_keyboardControl() == id)
			{
				flag2 = true;
				wasKeyboard = true;
			}
			else if ((int)Event.get_current().get_type() == 0 && Event.get_current().get_button() == 0 && ((Rect)(ref buttonRect)).Contains(Event.get_current().get_mousePosition()))
			{
				GUIUtility.set_hotControl(id);
				wasKeyboard = false;
			}
			else if (GUIUtility.get_hotControl() == id && (int)Event.get_current().get_type() == 1 && Event.get_current().get_button() == 0 && ((Rect)(ref buttonRect)).Contains(Event.get_current().get_mousePosition()))
			{
				flag2 = true;
				wasKeyboard = false;
			}
			if (flag2)
			{
				prevKeyboardId = GUIUtility.get_keyboardControl();
				selectedValues = null;
				selectionWasConfirmed = false;
				selectionWasChanged = false;
				focusedControlId = id;
				selectorFieldWindow = EditorWindow.get_focusedWindow();
				GUIUtility.set_hotControl(id);
				if (wasKeyboard)
				{
					GUIUtility.set_keyboardControl(id);
				}
				bindSelector = delegate(TSelector selector)
				{
					selector.SelectionChanged += delegate(IEnumerable<T> x)
					{
						selectedValues = x;
						selectionWasChanged = true;
						confirmedPopupControlId = id;
					};
					selector.SelectionConfirmed += delegate(IEnumerable<T> x)
					{
						selectionWasConfirmed = true;
						selectedValues = x;
						confirmedPopupControlId = id;
					};
					OdinEditorWindow odinEditorWindow = selector.popupWindowInstance;
					if ((Object)(object)odinEditorWindow != (Object)null)
					{
						odinEditorWindow.OnClose += delegate
						{
							focusedControlId = -1;
						};
						odinEditorWindow.OnClose += delegate
						{
							confirmedPopupControlId = id;
						};
					}
				};
				result = true;
				Event.get_current().Use();
			}
			if ((int)Event.get_current().get_type() == 7 && (Object)(object)selectorFieldWindow == (Object)(object)GUIHelper.CurrentWindow && id == confirmedPopupControlId)
			{
				if (wasKeyboard)
				{
					GUIUtility.set_keyboardControl(prevKeyboardId);
				}
				else
				{
					GUIUtility.set_keyboardControl(0);
				}
				if (focusedControlId == -1)
				{
					confirmedPopupControlId = 0;
				}
				if (selectionWasConfirmed)
				{
					confirmedPopupControlId = 0;
					focusedControlId = -1;
					GUI.set_changed(true);
					selectionWasConfirmed = false;
					GUIHelper.RequestRepaint();
					resultGetter = () => selectedValues ?? Enumerable.Empty<T>();
				}
				else if (selectionWasChanged)
				{
					selectionWasChanged = false;
					GUIHelper.RequestRepaint();
					resultGetter = () => selectedValues ?? Enumerable.Empty<T>();
				}
			}
			return result;
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(Rect rect, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			return DrawSelectorDropdown(rect, new GUIContent(btnLabel), createSelector, style);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(Rect rect, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Invalid comparison between Unknown and I4
			tmpValueLabel = (GUIContent)(((object)tmpValueLabel) ?? ((object)new GUIContent()));
			int controlID = GUIUtility.GetControlID((FocusType)1);
			tmpValueLabel.set_image(btnLabel.get_image());
			tmpValueLabel.set_text(EditorGUI.get_showMixedValue() ? "—" : btnLabel.get_text());
			tmpValueLabel.set_tooltip(btnLabel.get_tooltip());
			if ((int)Event.get_current().get_type() == 0 && Event.get_current().get_button() == 0 && ((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				GUIUtility.set_keyboardControl(controlID);
			}
			style = style ?? EditorStyles.get_popup();
			if (DrawSelectorButton(rect, tmpValueLabel, style, controlID, out Action<OdinSelector<T>> bindSelector, out Func<IEnumerable<T>> resultGetter))
			{
				OdinSelector<T> obj = createSelector(rect);
				bindSelector(obj);
				if ((int)Application.get_platform() == 16)
				{
					GUIUtility.ExitGUI();
				}
			}
			return resultGetter?.Invoke();
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null, params GUILayoutOption[] options)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			return DrawSelectorDropdown(label, new GUIContent(btnLabel), createSelector, style, options);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null, params GUILayoutOption[] options)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Invalid comparison between Unknown and I4
			tmpValueLabel = (GUIContent)(((object)tmpValueLabel) ?? ((object)new GUIContent()));
			tmpValueLabel.set_image(btnLabel.get_image());
			tmpValueLabel.set_text(EditorGUI.get_showMixedValue() ? "—" : btnLabel.get_text());
			tmpValueLabel.set_tooltip(btnLabel.get_tooltip());
			SirenixEditorGUI.GetFeatureRichControlRect(label, out var controlId, out var _, out var valueRect, options);
			style = style ?? EditorStyles.get_popup();
			if (DrawSelectorButton(valueRect, tmpValueLabel, style, controlId, out Action<OdinSelector<T>> bindSelector, out Func<IEnumerable<T>> resultGetter))
			{
				OdinSelector<T> obj = createSelector(valueRect);
				bindSelector(obj);
				if ((int)Application.get_platform() == 16)
				{
					GUIUtility.ExitGUI();
				}
			}
			return resultGetter?.Invoke();
		}
	}
}
