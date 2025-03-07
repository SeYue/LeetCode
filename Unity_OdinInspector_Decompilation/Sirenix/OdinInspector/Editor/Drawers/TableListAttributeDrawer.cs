using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// The TableList attirbute drawer.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.TableListAttribute" />
	public class TableListAttributeDrawer : OdinAttributeDrawer<TableListAttribute>
	{
		private enum ColumnType
		{
			Property,
			Index,
			DeleteButton
		}

		private class Column : IResizableColumn
		{
			public string Name;

			public float ColWidth;

			public float MinWidth;

			public bool Preserve;

			public bool Resizable;

			public string NiceName;

			public int NiceNameLabelWidth;

			public ColumnType ColumnType;

			public bool PreferWide;

			float IResizableColumn.ColWidth
			{
				get
				{
					return ColWidth;
				}
				set
				{
					ColWidth = value;
				}
			}

			float IResizableColumn.MinWidth => MinWidth;

			bool IResizableColumn.PreserveWidth => Preserve;

			bool IResizableColumn.Resizable => Resizable;

			public Column(int minWidth, bool preserveWidth, bool resizable, string name, ColumnType colType)
			{
				MinWidth = minWidth;
				ColWidth = minWidth;
				Preserve = preserveWidth;
				Name = name;
				ColumnType = colType;
				Resizable = resizable;
			}
		}

		private IOrderedCollectionResolver resolver;

		private LocalPersistentContext<bool> isPagingExpanded;

		private LocalPersistentContext<Vector2> scrollPos;

		private LocalPersistentContext<int> currPage;

		private GUITableRowLayoutGroup table;

		private HashSet<string> seenColumnNames;

		private List<Column> columns;

		private ObjectPicker picker;

		private int colOffset;

		private GUIContent indexLabel;

		private bool isReadOnly;

		private int indexLabelWidth;

		private Rect columnHeaderRect;

		private GUIPagingHelper paging;

		private bool drawAsList;

		private bool isFirstFrame = true;

		/// <summary>
		/// Determines whether this instance [can draw attribute property] the specified property.
		/// </summary>
		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			return property.ChildResolver is IOrderedCollectionResolver;
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Expected O, but got Unknown
			//IL_01f9: Expected O, but got Unknown
			drawAsList = false;
			isReadOnly = base.Attribute.IsReadOnly || !base.Property.ValueEntry.IsEditable;
			indexLabelWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent("100")).x + 15;
			indexLabel = new GUIContent();
			colOffset = 0;
			seenColumnNames = new HashSet<string>();
			table = new GUITableRowLayoutGroup();
			table.MinScrollViewHeight = base.Attribute.MinScrollViewHeight;
			table.MaxScrollViewHeight = base.Attribute.MaxScrollViewHeight;
			resolver = base.Property.ChildResolver as IOrderedCollectionResolver;
			scrollPos = this.GetPersistentValue<Vector2>("scrollPos", Vector2.get_zero());
			currPage = this.GetPersistentValue("currPage", 0);
			isPagingExpanded = this.GetPersistentValue("expanded", defaultValue: false);
			columns = new List<Column>(10);
			paging = new GUIPagingHelper();
			paging.NumberOfItemsPerPage = ((base.Attribute.NumberOfItemsPerPage > 0) ? base.Attribute.NumberOfItemsPerPage : GlobalConfig<GeneralDrawerConfig>.Instance.NumberOfItemsPrPage);
			paging.IsExpanded = isPagingExpanded.Value;
			paging.IsEnabled = GlobalConfig<GeneralDrawerConfig>.Instance.ShowPagingInTables || base.Attribute.ShowPaging;
			paging.CurrentPage = currPage.Value;
			base.Property.ValueEntry.OnChildValueChanged += OnChildValueChanged;
			if (base.Attribute.AlwaysExpanded)
			{
				base.Property.State.Expanded = true;
			}
			int cellPadding = base.Attribute.CellPadding;
			if (cellPadding > 0)
			{
				GUITableRowLayoutGroup gUITableRowLayoutGroup = table;
				GUIStyle val = new GUIStyle();
				val.set_padding(new RectOffset(cellPadding, cellPadding, cellPadding, cellPadding));
				gUITableRowLayoutGroup.CellStyle = val;
			}
			GUIHelper.RequestRepaint(3);
			if (base.Attribute.ShowIndexLabels)
			{
				colOffset++;
				columns.Add(new Column(indexLabelWidth, preserveWidth: true, resizable: false, null, ColumnType.Index));
			}
			if (!isReadOnly)
			{
				columns.Add(new Column(22, preserveWidth: true, resizable: false, null, ColumnType.DeleteButton));
			}
		}

		/// <summary>
		/// Draws the property layout.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Invalid comparison between Unknown and I4
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Invalid comparison between Unknown and I4
			if (drawAsList)
			{
				if (GUILayout.Button("Draw as table", (GUILayoutOption[])(object)new GUILayoutOption[0]))
				{
					drawAsList = false;
				}
				CallNextDrawer(label);
				return;
			}
			picker = ObjectPicker.GetObjectPicker(this, resolver.ElementType);
			paging.Update(resolver.MaxCollectionLength);
			currPage.Value = paging.CurrentPage;
			isPagingExpanded.Value = paging.IsExpanded;
			Rect rect = SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			if (!base.Attribute.HideToolbar)
			{
				DrawToolbar(label);
			}
			if (base.Attribute.AlwaysExpanded)
			{
				base.Property.State.Expanded = true;
				DrawColumnHeaders();
				DrawTable();
			}
			else
			{
				if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded) && base.Property.Children.Count > 0)
				{
					DrawColumnHeaders();
					DrawTable();
				}
				SirenixEditorGUI.EndFadeGroup();
			}
			SirenixEditorGUI.EndIndentedVertical();
			if ((int)Event.get_current().get_type() == 7)
			{
				((Rect)(ref rect)).set_yMin(((Rect)(ref rect)).get_yMin() - 1f);
				((Rect)(ref rect)).set_height(((Rect)(ref rect)).get_height() - 3f);
				SirenixEditorGUI.DrawBorders(rect, 1);
			}
			DropZone(rect);
			HandleObjectPickerEvents();
			if ((int)Event.get_current().get_type() == 7)
			{
				isFirstFrame = false;
			}
		}

		private void OnChildValueChanged(int index)
		{
			IPropertyValueEntry valueEntry = base.Property.Children[index].ValueEntry;
			if (valueEntry == null || !typeof(ScriptableObject).IsAssignableFrom(valueEntry.TypeOfValue))
			{
				return;
			}
			for (int i = 0; i < valueEntry.ValueCount; i++)
			{
				object obj = valueEntry.WeakValues[i];
				Object val = obj as Object;
				if (Object.op_Implicit(val))
				{
					EditorUtility.SetDirty(val);
				}
			}
		}

		private void DropZone(Rect rect)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Invalid comparison between Unknown and I4
			if (isReadOnly)
			{
				return;
			}
			EventType type = Event.get_current().get_type();
			if (((int)type != 9 && (int)type != 10) || !((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				return;
			}
			Object[] array = null;
			if (DragAndDrop.get_objectReferences().Any((Object n) => n != (Object)null && resolver.ElementType.IsAssignableFrom(((object)n).GetType())))
			{
				array = (from x in DragAndDrop.get_objectReferences()
					where x != (Object)null && resolver.ElementType.IsAssignableFrom(((object)x).GetType())
					select x).Reverse().ToArray();
			}
			else if (resolver.ElementType.InheritsFrom(typeof(Component)))
			{
				array = (Object[])(object)(from x in DragAndDrop.get_objectReferences().OfType<GameObject>()
					select x.GetComponent(resolver.ElementType) into x
					where (Object)(object)x != (Object)null
					select x).Reverse().ToArray();
			}
			else if (resolver.ElementType.InheritsFrom(typeof(Sprite)) && DragAndDrop.get_objectReferences().Any((Object n) => n is Texture2D && AssetDatabase.Contains(n)))
			{
				array = (Object[])(object)(from x in DragAndDrop.get_objectReferences().OfType<Texture2D>().Select(delegate(Texture2D x)
					{
						string assetPath = AssetDatabase.GetAssetPath((Object)(object)x);
						return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
					})
					where (Object)(object)x != (Object)null
					select x).Reverse().ToArray();
			}
			if (array == null || array.Length == 0)
			{
				return;
			}
			DragAndDrop.set_visualMode((DragAndDropVisualMode)1);
			Event.get_current().Use();
			if ((int)type != 10)
			{
				return;
			}
			DragAndDrop.AcceptDrag();
			Object[] array2 = array;
			foreach (Object val in array2)
			{
				object[] array3 = new object[base.Property.ParentValues.Count];
				for (int j = 0; j < array3.Length; j++)
				{
					array3[j] = val;
				}
				resolver.QueueAdd(array3);
			}
		}

		private void AddColumns(int rowIndexFrom, int rowIndexTo)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() != 8)
			{
				return;
			}
			for (int i = rowIndexFrom; i < rowIndexTo; i++)
			{
				int num = 0;
				InspectorProperty inspectorProperty = base.Property.Children[i];
				for (int j = 0; j < inspectorProperty.Children.Count; j++)
				{
					InspectorProperty inspectorProperty2 = inspectorProperty.Children[j];
					if (!seenColumnNames.Add(inspectorProperty2.Name))
					{
						continue;
					}
					HideInTablesAttribute columnAttribute = GetColumnAttribute<HideInTablesAttribute>(inspectorProperty2);
					if (columnAttribute != null)
					{
						num++;
						continue;
					}
					bool preserveWidth = false;
					bool resizable = true;
					bool preferWide = true;
					int minWidth = base.Attribute.DefaultMinColumnWidth;
					TableColumnWidthAttribute columnAttribute2 = GetColumnAttribute<TableColumnWidthAttribute>(inspectorProperty2);
					if (columnAttribute2 != null)
					{
						preserveWidth = !columnAttribute2.Resizable;
						resizable = columnAttribute2.Resizable;
						minWidth = columnAttribute2.Width;
						preferWide = false;
					}
					Column column = new Column(minWidth, preserveWidth, resizable, inspectorProperty2.Name, ColumnType.Property);
					column.NiceName = inspectorProperty2.NiceName;
					column.NiceNameLabelWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent(column.NiceName)).x;
					column.PreferWide = preferWide;
					int val = j + colOffset - num;
					columns.Insert(Math.Min(val, columns.Count), column);
					GUIHelper.RequestRepaint(3);
				}
			}
		}

		private void DrawToolbar(GUIContent label)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			Rect toolbarRect = GUILayoutUtility.GetRect(0f, 22f);
			bool flag = (int)Event.get_current().get_type() == 7;
			if (flag)
			{
				SirenixGUIStyles.ToolbarBackground.Draw(toolbarRect, GUIContent.none, 0);
			}
			if (!isReadOnly)
			{
				Rect val = toolbarRect.AlignRight(23f);
				((Rect)(ref val)).set_width(((Rect)(ref val)).get_width() - 1f);
				((Rect)(ref toolbarRect)).set_xMax(((Rect)(ref val)).get_xMin());
				if (GUI.Button(val, GUIContent.none, SirenixGUIStyles.ToolbarButton))
				{
					picker.ShowObjectPicker(null, base.Property.GetAttribute<AssetsOnlyAttribute>() == null && !typeof(ScriptableObject).IsAssignableFrom(resolver.ElementType), toolbarRect, !base.Property.ValueEntry.SerializationBackend.SupportsPolymorphism);
				}
				EditorIcons.Plus.Draw(val, 16f);
			}
			if (!isReadOnly)
			{
				Rect val2 = toolbarRect.AlignRight(23f);
				((Rect)(ref toolbarRect)).set_xMax(((Rect)(ref val2)).get_xMin());
				if (GUI.Button(val2, GUIContent.none, SirenixGUIStyles.ToolbarButton))
				{
					drawAsList = !drawAsList;
				}
				EditorIcons.HamburgerMenu.Draw(val2, 13f);
			}
			paging.DrawToolbarPagingButtons(ref toolbarRect, base.Property.State.Expanded, showItemCount: true);
			if (label == null)
			{
				label = GUIHelper.TempContent("");
			}
			Rect val3 = toolbarRect;
			((Rect)(ref val3)).set_x(((Rect)(ref val3)).get_x() + 5f);
			((Rect)(ref val3)).set_y(((Rect)(ref val3)).get_y() + 3f);
			((Rect)(ref val3)).set_height(16f);
			if (base.Property.Children.Count > 0)
			{
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				if (base.Attribute.AlwaysExpanded)
				{
					GUI.Label(val3, label);
				}
				else
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(val3, base.Property.State.Expanded, label);
				}
				GUIHelper.PushHierarchyMode(hierarchyMode: true);
			}
			else if (flag)
			{
				GUI.Label(val3, label);
			}
		}

		private void DrawColumnHeaders()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Invalid comparison between Unknown and I4
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Invalid comparison between Unknown and I4
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			if (base.Property.Children.Count == 0)
			{
				return;
			}
			columnHeaderRect = GUILayoutUtility.GetRect(0f, 21f);
			ref Rect reference = ref columnHeaderRect;
			((Rect)(ref reference)).set_height(((Rect)(ref reference)).get_height() + 1f);
			ref Rect reference2 = ref columnHeaderRect;
			((Rect)(ref reference2)).set_y(((Rect)(ref reference2)).get_y() - 1f);
			if ((int)Event.get_current().get_type() == 7)
			{
				SirenixEditorGUI.DrawBorders(columnHeaderRect, 1);
				EditorGUI.DrawRect(columnHeaderRect, SirenixGUIStyles.ColumnTitleBg);
			}
			float width = ((Rect)(ref columnHeaderRect)).get_width();
			Rect contentRect = table.ContentRect;
			float num = width - ((Rect)(ref contentRect)).get_width();
			ref Rect reference3 = ref columnHeaderRect;
			((Rect)(ref reference3)).set_width(((Rect)(ref reference3)).get_width() - num);
			GUITableUtilities.ResizeColumns(columnHeaderRect, columns);
			if ((int)Event.get_current().get_type() != 7)
			{
				return;
			}
			GUITableUtilities.DrawColumnHeaderSeperators(columnHeaderRect, columns, SirenixGUIStyles.BorderColor);
			Rect val = columnHeaderRect;
			for (int i = 0; i < columns.Count; i++)
			{
				Column column = columns[i];
				if (!(((Rect)(ref val)).get_x() > ((Rect)(ref columnHeaderRect)).get_xMax()))
				{
					((Rect)(ref val)).set_width(column.ColWidth);
					((Rect)(ref val)).set_xMax(Mathf.Min(((Rect)(ref columnHeaderRect)).get_xMax(), ((Rect)(ref val)).get_xMax()));
					if (column.NiceName != null)
					{
						Rect val2 = val;
						GUI.Label(val2, column.NiceName, SirenixGUIStyles.LabelCentered);
					}
					((Rect)(ref val)).set_x(((Rect)(ref val)).get_x() + column.ColWidth);
					continue;
				}
				break;
			}
		}

		private void DrawTable()
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			GUIHelper.PushHierarchyMode(hierarchyMode: false);
			table.DrawScrollView = base.Attribute.DrawScrollView && (paging.IsExpanded || !paging.IsEnabled);
			table.ScrollPos = scrollPos.Value;
			table.BeginTable(paging.EndIndex - paging.StartIndex);
			AddColumns(table.RowIndexFrom, table.RowIndexTo);
			DrawListItemBackGrounds();
			float num = 0f;
			for (int i = 0; i < columns.Count; i++)
			{
				Column column = columns[i];
				int num2 = (int)column.ColWidth;
				if (isFirstFrame && column.PreferWide)
				{
					num2 = 200;
				}
				table.BeginColumn((int)num, num2);
				GUIHelper.PushLabelWidth((float)num2 * 0.3f);
				num += column.ColWidth;
				for (int j = table.RowIndexFrom; j < table.RowIndexTo; j++)
				{
					table.BeginCell(j);
					DrawCell(column, j);
					table.EndCell(j);
				}
				GUIHelper.PopLabelWidth();
				table.EndColumn();
			}
			DrawRightClickContextMenuAreas();
			table.EndTable();
			scrollPos.Value = table.ScrollPos;
			DrawColumnSeperators();
			GUIHelper.PopHierarchyMode();
			if (columns.Count > 0 && columns[0].ColumnType == ColumnType.Index)
			{
				columns[0].ColWidth = indexLabelWidth;
				columns[0].MinWidth = indexLabelWidth;
			}
		}

		private void DrawColumnSeperators()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 7)
			{
				Color borderColor = SirenixGUIStyles.BorderColor;
				borderColor.a *= 0.4f;
				Rect outerRect = table.OuterRect;
				GUITableUtilities.DrawColumnHeaderSeperators(outerRect, columns, borderColor);
			}
		}

		private void DrawListItemBackGrounds()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 7)
			{
				for (int i = table.RowIndexFrom; i < table.RowIndexTo; i++)
				{
					Color val = default(Color);
					Rect rowRect = table.GetRowRect(i);
					val = ((i % 2 == 0) ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd);
					EditorGUI.DrawRect(rowRect, val);
				}
			}
		}

		private void DrawRightClickContextMenuAreas()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			for (int i = table.RowIndexFrom; i < table.RowIndexTo; i++)
			{
				Rect rowRect = table.GetRowRect(i);
				base.Property.Children[i].Update();
				PropertyContextMenuDrawer.AddRightClickArea(base.Property.Children[i], rowRect);
			}
		}

		private void DrawCell(Column col, int rowIndex)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Invalid comparison between Unknown and I4
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			rowIndex += paging.StartIndex;
			if (col.ColumnType == ColumnType.Index)
			{
				Rect rect = GUILayoutUtility.GetRect(0f, 16f);
				((Rect)(ref rect)).set_xMin(((Rect)(ref rect)).get_xMin() + 5f);
				((Rect)(ref rect)).set_width(((Rect)(ref rect)).get_width() - 2f);
				if ((int)Event.get_current().get_type() == 7)
				{
					indexLabel.set_text(rowIndex.ToString());
					GUI.Label(rect, indexLabel, SirenixGUIStyles.Label);
					int num = (int)SirenixGUIStyles.Label.CalcSize(indexLabel).x;
					indexLabelWidth = Mathf.Max(indexLabelWidth, num + 15);
				}
			}
			else if (col.ColumnType == ColumnType.DeleteButton)
			{
				Rect rect2 = GUILayoutUtility.GetRect(20f, 20f).AlignCenter(16f);
				if (SirenixEditorGUI.IconButton(rect2, EditorIcons.X))
				{
					resolver.QueueRemoveAt(rowIndex);
				}
			}
			else
			{
				if (col.ColumnType != 0)
				{
					throw new NotImplementedException(col.ColumnType.ToString());
				}
				base.Property.Children[rowIndex].Children[col.Name]?.Draw(null);
			}
		}

		private void HandleObjectPickerEvents()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			if (picker.IsReadyToClaim && (int)Event.get_current().get_type() == 7)
			{
				object obj = picker.ClaimObject();
				object[] array = new object[base.Property.Tree.WeakTargets.Count];
				array[0] = obj;
				for (int i = 1; i < array.Length; i++)
				{
					array[i] = SerializationUtility.CreateCopy(obj);
				}
				resolver.QueueAdd(array);
			}
		}

		private IEnumerable<InspectorProperty> EnumerateGroupMembers(InspectorProperty groupProperty)
		{
			for (int i = 0; i < groupProperty.Children.Count; i++)
			{
				InspectorPropertyInfo info = groupProperty.Children[i].Info;
				if (info.PropertyType != PropertyType.Group)
				{
					yield return groupProperty.Children[i];
					continue;
				}
				foreach (InspectorProperty item in EnumerateGroupMembers(groupProperty.Children[i]))
				{
					yield return item;
				}
			}
		}

		private T GetColumnAttribute<T>(InspectorProperty col) where T : Attribute
		{
			if (col.Info.PropertyType == PropertyType.Group)
			{
				return (from c in EnumerateGroupMembers(col)
					select c.GetAttribute<T>()).FirstOrDefault((T c) => c != null);
			}
			return col.GetAttribute<T>();
		}
	}
}
