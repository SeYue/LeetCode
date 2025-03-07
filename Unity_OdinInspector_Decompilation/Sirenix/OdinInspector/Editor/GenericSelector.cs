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
	/// In simple one-off use cases, making a custom OdinSelector might not be needed, as the GenericSelecor 
	/// can be populated with anything and customized a great deal.
	/// </summary>
	/// <example>
	/// <code>
	/// SomeType someValue;
	///
	/// [OnInspectorGUI]
	/// void OnInspectorGUI()
	/// {
	///     if (GUILayout.Button("Open Generic Selector Popup"))
	///     {
	///         List&lt;SomeType&gt; source = ...;
	///         GenericSelector&lt;SomeType&gt; selector = new GenericSelector&lt;SomeType&gt;("Title", false, x =&gt; x.Path, source);
	///         selector.SetSelection(this.someValue);
	///         selector.SelectionTree.Config.DrawSearchToolbar = false;
	///         selector.SelectionTree.DefaultMenuStyle.Height = 22;
	///         selector.SelectionConfirmed += selection =&gt; this.someValue = selection.FirstOrDefault()
	///         var window = selector.ShowInPopup();
	///         window.OnEndGUI += () =&gt; { EditorGUILayout.HelpBox("A quick way of injecting custom GUI to the editor window popup instance.", MessageType.Info); };
	///         window.OnClose += selector.SelectionTree.Selection.ConfirmSelection; // Confirm selection when window clses.
	///     }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.EnumSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.TypeSelector" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public class GenericSelector<T> : OdinSelector<T>
	{
		private int checkboxUpdateId;

		private readonly string title;

		private readonly IEnumerable<GenericSelectorItem<T>> genericSelectorCollection;

		private readonly bool supportsMultiSelect;

		private readonly IEnumerable<T> collection;

		private Func<T, string> getMenuItemName;

		private HashSet<T> selection = new HashSet<T>();

		private bool requestCheckboxUpdate;

		/// <summary>
		/// Gets or sets a value indicating whether [flattened tree].
		/// </summary>
		public bool FlattenedTree { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [checkbox toggle].
		/// </summary>
		public bool CheckboxToggle { get; set; }

		/// <summary>
		/// Gets the title. No title will be drawn if the string is null or empty.
		/// </summary>
		public override string Title => title;

		public GenericSelector(string title, bool supportsMultiSelect, IEnumerable<GenericSelectorItem<T>> collection)
		{
			this.supportsMultiSelect = supportsMultiSelect;
			this.title = title;
			genericSelectorCollection = collection;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(string title, IEnumerable<T> collection, bool supportsMultiSelect, Func<T, string> getMenuItemName = null)
		{
			this.title = title;
			this.supportsMultiSelect = supportsMultiSelect;
			this.getMenuItemName = getMenuItemName;
			this.collection = collection;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(string title, bool supportsMultiSelect, Func<T, string> getMenuItemName, params T[] collection)
		{
			this.title = title;
			this.supportsMultiSelect = supportsMultiSelect;
			this.getMenuItemName = getMenuItemName ?? ((Func<T, string>)((T x) => x.ToString()));
			this.collection = collection;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(string title, bool supportsMultiSelect, params T[] collection)
			: this(title, supportsMultiSelect, (Func<T, string>)null, collection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(string title, params T[] collection)
			: this(title, supportsMultiSelect: false, (Func<T, string>)null, collection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(params T[] collection)
			: this((string)null, supportsMultiSelect: false, (Func<T, string>)null, collection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(string title, bool supportsMultiSelect, Func<T, string> getMenuItemName, IEnumerable<T> collection)
		{
			this.title = title;
			this.supportsMultiSelect = supportsMultiSelect;
			this.getMenuItemName = getMenuItemName ?? ((Func<T, string>)((T x) => x.ToString()));
			this.collection = collection;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(string title, bool supportsMultiSelect, IEnumerable<T> collection)
			: this(title, supportsMultiSelect, (Func<T, string>)null, collection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(string title, IEnumerable<T> collection)
			: this(title, supportsMultiSelect: false, (Func<T, string>)null, collection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> class.
		/// </summary>
		public GenericSelector(IEnumerable<T> collection)
			: this((string)null, supportsMultiSelect: false, (Func<T, string>)null, collection)
		{
		}

		/// <summary>
		/// Draws the selection tree. This gets drawn using the OnInspectorGUI attribute.
		/// </summary>
		protected override void DrawSelectionTree()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Invalid comparison between Unknown and I4
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Invalid comparison between Unknown and I4
			if (CheckboxToggle && (int)Event.get_current().get_type() == 4 && (int)Event.get_current().get_keyCode() == 32 && base.SelectionTree == OdinMenuTree.ActiveMenuTree)
			{
				IEnumerable<T> source = (from x in base.SelectionTree.Selection.SelectMany((OdinMenuItem x) => x.GetChildMenuItemsRecursive(includeSelf: true))
					select x.Value).OfType<T>();
				if (source.Any())
				{
					bool remove = selection.Contains(source.FirstOrDefault());
					source.ForEach(delegate(T x)
					{
						if (remove)
						{
							selection.Remove(x);
						}
						else
						{
							selection.Add(x);
						}
					});
				}
				Event.get_current().Use();
				checkboxUpdateId++;
			}
			if (requestCheckboxUpdate && (int)Event.get_current().get_type() == 7)
			{
				requestCheckboxUpdate = false;
				checkboxUpdateId++;
			}
			base.DrawSelectionTree();
		}

		private void DrawCheckboxMenuItems(OdinMenuItem xx)
		{
			List<T> allChilds = (from x in xx.GetChildMenuItemsRecursive(includeSelf: true)
				select x.Value).OfType<T>().ToList();
			bool isEmpty = allChilds.Count == 0;
			bool isSelected = false;
			bool isMixed = false;
			int prevUpdateId = -1;
			Action validate = delegate
			{
				if (!isEmpty)
				{
					isSelected = selection.Contains(allChilds[0]);
					T val2 = allChilds[0];
					isMixed = false;
					for (int j = 1; j < allChilds.Count; j++)
					{
						bool flag2 = selection.Contains(allChilds[j]);
						if (flag2 != isSelected)
						{
							isMixed = true;
							break;
						}
					}
				}
			};
			OdinMenuItem odinMenuItem = xx;
			odinMenuItem.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(odinMenuItem.OnDrawItem, (Action<OdinMenuItem>)delegate
			{
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_006d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0073: Invalid comparison between Unknown and I4
				//IL_008d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
				if (!isEmpty)
				{
					Rect val = xx.LabelRect;
					val = val.AlignMiddle(18f).AlignLeft(16f);
					((Rect)(ref val)).set_x(((Rect)(ref val)).get_x() - 16f);
					if (Object.op_Implicit((Object)(object)xx.IconGetter()))
					{
						((Rect)(ref val)).set_x(((Rect)(ref val)).get_x() - 16f);
					}
					if ((int)Event.get_current().get_type() != 7 && xx.ChildMenuItems.Count == 0)
					{
						val = xx.Rect;
					}
					if (prevUpdateId != checkboxUpdateId)
					{
						validate();
						prevUpdateId = checkboxUpdateId;
					}
					EditorGUI.set_showMixedValue(isMixed);
					EditorGUI.BeginChangeCheck();
					bool flag = EditorGUI.Toggle(val, isSelected);
					if (EditorGUI.EndChangeCheck())
					{
						for (int i = 0; i < allChilds.Count; i++)
						{
							if (flag)
							{
								selection.Add(allChilds[i]);
							}
							else
							{
								selection.Remove(allChilds[i]);
							}
						}
						xx.Select();
						validate();
						requestCheckboxUpdate = true;
						GUIHelper.RemoveFocusControl();
					}
					EditorGUI.set_showMixedValue(false);
				}
			});
		}

		public override void SetSelection(T selected)
		{
			if (CheckboxToggle)
			{
				selection.Clear();
				selection.Add(selected);
			}
			else
			{
				base.SetSelection(selected);
			}
		}

		public override void SetSelection(IEnumerable<T> selection)
		{
			if (CheckboxToggle)
			{
				this.selection.Clear();
				foreach (T item in selection)
				{
					this.selection.Add(item);
				}
			}
			else
			{
				base.SetSelection(selection);
			}
		}

		public override bool IsValidSelection(IEnumerable<T> collection)
		{
			if (base.SelectionTree.Selection.SupportsMultiSelect)
			{
				return true;
			}
			return base.IsValidSelection(collection);
		}

		public override IEnumerable<T> GetCurrentSelection()
		{
			if (CheckboxToggle)
			{
				return selection;
			}
			return base.GetCurrentSelection();
		}

		/// <summary>
		/// Builds the selection tree.
		/// </summary>
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			tree.Selection.SupportsMultiSelect = supportsMultiSelect;
			tree.DefaultMenuStyle = OdinMenuStyle.TreeViewStyle;
			getMenuItemName = getMenuItemName ?? ((Func<T, string>)((T x) => (x != null) ? x.ToString() : ""));
			if (FlattenedTree)
			{
				if (genericSelectorCollection != null)
				{
					foreach (GenericSelectorItem<T> item in genericSelectorCollection)
					{
						tree.MenuItems.Add(new OdinMenuItem(tree, item.GetNiceName(), item.Value));
					}
				}
				else
				{
					foreach (T item2 in collection)
					{
						tree.MenuItems.Add(new OdinMenuItem(tree, getMenuItemName(item2), item2));
					}
				}
			}
			else if (genericSelectorCollection != null)
			{
				foreach (GenericSelectorItem<T> item3 in genericSelectorCollection)
				{
					tree.AddObjectAtPath(item3.GetNiceName(), item3.Value);
				}
			}
			else
			{
				tree.AddRange(collection, getMenuItemName);
			}
			if (CheckboxToggle)
			{
				tree.EnumerateTree().ForEach(DrawCheckboxMenuItems);
				tree.DefaultMenuStyle.TrianglePadding -= 17f;
				tree.DefaultMenuStyle.Offset += 18f;
				tree.DefaultMenuStyle.SelectedColorDarkSkin = new Color(1f, 1f, 1f, 0.05f);
				tree.DefaultMenuStyle.SelectedColorLightSkin = new Color(1f, 1f, 1f, 0.05f);
				tree.DefaultMenuStyle.SelectedLabelStyle = tree.DefaultMenuStyle.DefaultLabelStyle;
				tree.Config.ConfirmSelectionOnDoubleClick = false;
			}
		}
	}
}
