using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Base class for two-dimensional array drawers.
	/// </summary>
	public abstract class TwoDimensionalArrayDrawer<TArray, TElement> : OdinValueDrawer<TArray> where TArray : IList
	{
		protected internal class Context
		{
			public int RowCount;

			public int ColCount;

			public GUITable Table;

			public TElement[,] Value;

			public int DraggingRow = -1;

			public int DraggingCol = -1;

			public TableMatrixAttribute Attribute;

			public ValueResolver<TElement> DrawElement;

			public ValueResolver<string> HorizontalTitleGetter;

			public ValueResolver<string> VerticalTitleGetter;

			public Vector2 dragStartPos;

			public bool IsDraggingColumn;

			public int ColumnDragFrom;

			public int ColumnDragTo;

			public bool IsDraggingRow;

			public int RowDragFrom;

			public int RowDragTo;

			public string ExtraErrorMessage;
		}

		private static readonly NamedValue[] DrawElementNamedArgs = new NamedValue[6]
		{
			new NamedValue("rect", typeof(Rect)),
			new NamedValue("element", typeof(TElement)),
			new NamedValue("value", typeof(TElement)),
			new NamedValue("array", typeof(TArray)),
			new NamedValue("x", typeof(int)),
			new NamedValue("y", typeof(int))
		};

		private Context context;

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected TableMatrixAttribute TableMatrixAttribute { get; private set; }

		/// <summary>
		/// <para>Override this method in order to define custom type constraints to specify whether or not a type should be drawn by the drawer.</para>
		/// <para>Note that Odin's <see cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" /> has full support for generic class constraints, so most often you can get away with not overriding CanDrawTypeFilter.</para>
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (type.IsArray && type.GetArrayRank() == 2)
			{
				return type.GetElementType() == typeof(TElement);
			}
			return false;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected virtual TableMatrixAttribute GetDefaultTableMatrixAttributeSettings()
		{
			return new TableMatrixAttribute();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_04a6: Expected O, but got Unknown
			TElement[,] array = base.ValueEntry.Values[0] as TElement[,];
			bool flag = false;
			bool flag2 = false;
			TableMatrixAttribute tableMatrixAttribute = base.ValueEntry.Property.GetAttribute<TableMatrixAttribute>() ?? GetDefaultTableMatrixAttributeSettings();
			int num = (tableMatrixAttribute.Transpose ? 1 : 0);
			int dimension = 1 - num;
			int num2 = array.GetLength(num);
			int num3 = array.GetLength(dimension);
			for (int i = 1; i < base.ValueEntry.Values.Count; i++)
			{
				TElement[,] array2 = base.ValueEntry.Values[i] as TElement[,];
				flag2 = flag2 || array2.GetLength(num) != num2;
				flag = flag || array2.GetLength(dimension) != num3;
				num2 = Mathf.Min(num2, array2.GetLength(num));
				num3 = Mathf.Min(num3, array2.GetLength(dimension));
			}
			if (context == null || num2 != context.ColCount || num3 != context.RowCount)
			{
				context = new Context();
				context.Value = array;
				context.ColCount = num2;
				context.RowCount = num3;
				context.Attribute = tableMatrixAttribute;
				if (context.Attribute.DrawElementMethod != null)
				{
					context.DrawElement = ValueResolver.Get<TElement>(base.Property, context.Attribute.DrawElementMethod, DrawElementNamedArgs);
				}
				context.HorizontalTitleGetter = ValueResolver.GetForString(base.Property, context.Attribute.HorizontalTitle);
				context.VerticalTitleGetter = ValueResolver.GetForString(base.Property, context.Attribute.VerticalTitle);
				context.Table = GUITable.Create(Mathf.Max(num2, 1) + (flag2 ? 1 : 0), Mathf.Max(num3, 1) + (flag ? 1 : 0), delegate(Rect rect, int x, int y)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					DrawElement(rect, base.ValueEntry, context, x, y);
				}, context.HorizontalTitleGetter.GetValue(), context.Attribute.HideColumnIndices ? null : ((Action<Rect, int>)delegate(Rect rect, int x)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					DrawColumn(rect, base.ValueEntry, context, x);
				}), context.VerticalTitleGetter.GetValue(), context.Attribute.HideRowIndices ? null : ((Action<Rect, int>)delegate(Rect rect, int y)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					DrawRows(rect, base.ValueEntry, context, y);
				}), context.Attribute.ResizableColumns);
				context.Table.RespectIndentLevel = context.Attribute.RespectIndentLevel;
				if (context.Attribute.RowHeight != 0)
				{
					for (int j = 0; j < context.RowCount; j++)
					{
						int y2 = context.Table.RowCount - 1 - j;
						for (int k = 0; k < context.Table.ColumnCount; k++)
						{
							GUITableCell gUITableCell = context.Table[k, y2];
							if (gUITableCell != null)
							{
								gUITableCell.Height = context.Attribute.RowHeight;
							}
						}
					}
				}
				if (flag2)
				{
					context.Table[context.Table.ColumnCount - 1, 1].Width = 15f;
				}
				if (flag2)
				{
					for (int l = 0; l < context.Table.ColumnCount; l++)
					{
						context.Table[l, context.Table.RowCount - 1].Height = 15f;
					}
				}
			}
			if (context.Attribute.SquareCells)
			{
				SetSquareRowHeights(context);
			}
			TableMatrixAttribute = context.Attribute;
			context.Value = array;
			bool showMixedValue = EditorGUI.get_showMixedValue();
			OnBeforeDrawTable(base.ValueEntry, context, label);
			ValueResolver.DrawErrors(context.DrawElement, context.HorizontalTitleGetter, context.VerticalTitleGetter);
			if (context.ExtraErrorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(context.ExtraErrorMessage);
			}
			if (context.DrawElement == null || !context.DrawElement.HasError)
			{
				try
				{
					context.Table.DrawTable();
					GUILayout.Space(3f);
				}
				catch (ExitGUIException val)
				{
					ExitGUIException val2 = val;
					throw val2;
				}
				catch (Exception ex)
				{
					if (ex.IsExitGUIException())
					{
						throw ex.AsExitGUIException();
					}
					Debug.LogException(ex);
				}
			}
			EditorGUI.set_showMixedValue(showMixedValue);
		}

		private static void SetSquareRowHeights(Context context)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			if (context.ColCount <= 0 || context.RowCount <= 0)
			{
				return;
			}
			GUITableCell gUITableCell = context.Table[context.ColCount - 1, context.RowCount - 1];
			if (gUITableCell == null)
			{
				return;
			}
			Rect rect = gUITableCell.Rect;
			float height = ((Rect)(ref rect)).get_height();
			rect = gUITableCell.Rect;
			if (!(Mathf.Abs(height - ((Rect)(ref rect)).get_width()) > 0f))
			{
				return;
			}
			for (int i = 0; i < context.RowCount; i++)
			{
				int y = context.Table.RowCount - 1 - i;
				for (int j = 0; j < context.Table.ColumnCount; j++)
				{
					GUITableCell gUITableCell2 = context.Table[j, y];
					if (gUITableCell2 != null)
					{
						rect = gUITableCell.Rect;
						gUITableCell2.Height = ((Rect)(ref rect)).get_width();
					}
				}
			}
			context.Table.ReCalculateSizes();
			GUIHelper.RequestRepaint();
		}

		/// <summary>
		/// This method gets called from DrawPropertyLayout right before the table and error message is drawn.
		/// </summary>
		protected internal virtual void OnBeforeDrawTable(IPropertyValueEntry<TArray> entry, Context value, GUIContent label)
		{
		}

		private void DrawRows(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int rowIndex)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Invalid comparison between Unknown and I4
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Invalid comparison between Unknown and I4
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Invalid comparison between Unknown and I4
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0401: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_043f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0446: Expected O, but got Unknown
			//IL_044d: Unknown result type (might be due to invalid IL or missing references)
			//IL_045a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0464: Expected O, but got Unknown
			//IL_0464: Expected O, but got Unknown
			//IL_046b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_0482: Expected O, but got Unknown
			//IL_0482: Expected O, but got Unknown
			//IL_0489: Unknown result type (might be due to invalid IL or missing references)
			//IL_0496: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a0: Expected O, but got Unknown
			//IL_04a0: Expected O, but got Unknown
			//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ca: Expected O, but got Unknown
			//IL_04ca: Expected O, but got Unknown
			if (rowIndex < context.RowCount)
			{
				GUI.Label(rect, rowIndex.ToString(), SirenixGUIStyles.LabelCentered);
				if (!context.Attribute.IsReadOnly)
				{
					int controlID = GUIUtility.GetControlID((FocusType)2);
					if (GUI.get_enabled() && (int)Event.get_current().get_type() == 0 && Event.get_current().get_button() == 0 && ((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
					{
						GUIHelper.RemoveFocusControl();
						GUIUtility.set_hotControl(controlID);
						EditorGUIUtility.SetWantsMouseJumping(1);
						Event.get_current().Use();
						context.RowDragFrom = rowIndex;
						context.RowDragTo = rowIndex;
						context.dragStartPos = Event.get_current().get_mousePosition();
					}
					else if (GUIUtility.get_hotControl() == controlID)
					{
						Vector2 val = context.dragStartPos - Event.get_current().get_mousePosition();
						if (((Vector2)(ref val)).get_sqrMagnitude() > 25f)
						{
							context.IsDraggingRow = true;
						}
						if ((int)Event.get_current().get_type() == 3)
						{
							Event.get_current().Use();
						}
						else if ((int)Event.get_current().get_type() == 1)
						{
							GUIUtility.set_hotControl(0);
							EditorGUIUtility.SetWantsMouseJumping(0);
							Event.get_current().Use();
							context.IsDraggingRow = false;
							if (context.Attribute.Transpose)
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveColumn(arr, context.RowDragFrom, context.RowDragTo));
							}
							else
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveRow(arr, context.RowDragFrom, context.RowDragTo));
							}
						}
					}
					if (context.IsDraggingRow && (int)Event.get_current().get_type() == 7)
					{
						float y = Event.get_current().get_mousePosition().y;
						if (y > ((Rect)(ref rect)).get_y() - 1f && y < ((Rect)(ref rect)).get_y() + ((Rect)(ref rect)).get_height() + 1f)
						{
							Rect arrowRect;
							if (y > ((Rect)(ref rect)).get_y() + ((Rect)(ref rect)).get_height() * 0.5f)
							{
								arrowRect = rect.AlignBottom(16f);
								((Rect)(ref arrowRect)).set_width(16f);
								ref Rect reference = ref arrowRect;
								((Rect)(ref reference)).set_y(((Rect)(ref reference)).get_y() + 8f);
								ref Rect reference2 = ref arrowRect;
								((Rect)(ref reference2)).set_x(((Rect)(ref reference2)).get_x() - 13f);
								context.RowDragTo = rowIndex;
							}
							else
							{
								arrowRect = rect.AlignTop(16f);
								((Rect)(ref arrowRect)).set_width(16f);
								ref Rect reference3 = ref arrowRect;
								((Rect)(ref reference3)).set_y(((Rect)(ref reference3)).get_y() - 8f);
								ref Rect reference4 = ref arrowRect;
								((Rect)(ref reference4)).set_x(((Rect)(ref reference4)).get_x() - 13f);
								context.RowDragTo = rowIndex - 1;
							}
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								//IL_0001: Unknown result type (might be due to invalid IL or missing references)
								//IL_0016: Unknown result type (might be due to invalid IL or missing references)
								//IL_001b: Unknown result type (might be due to invalid IL or missing references)
								//IL_0020: Unknown result type (might be due to invalid IL or missing references)
								//IL_006c: Unknown result type (might be due to invalid IL or missing references)
								//IL_0071: Unknown result type (might be due to invalid IL or missing references)
								//IL_007e: Unknown result type (might be due to invalid IL or missing references)
								//IL_0093: Unknown result type (might be due to invalid IL or missing references)
								GUI.DrawTexture(arrowRect, EditorIcons.ArrowRight.Active);
								Rect val3 = arrowRect;
								((Rect)(ref val3)).set_y(((Rect)(ref val3)).get_center().y - 2f + 1f);
								((Rect)(ref val3)).set_height(3f);
								((Rect)(ref val3)).set_x(((Rect)(ref val3)).get_x() + 14f);
								Rect tableRect = context.Table.TableRect;
								((Rect)(ref val3)).set_xMax(((Rect)(ref tableRect)).get_xMax());
								EditorGUI.DrawRect(val3, new Color(0f, 0f, 0f, 0.6f));
							});
						}
						if (rowIndex == context.RowCount - 1)
						{
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								//IL_004c: Unknown result type (might be due to invalid IL or missing references)
								//IL_0051: Unknown result type (might be due to invalid IL or missing references)
								//IL_0064: Unknown result type (might be due to invalid IL or missing references)
								//IL_0079: Unknown result type (might be due to invalid IL or missing references)
								GUITableCell gUITableCell = context.Table[context.Table.ColumnCount - 1, context.Table.RowCount - context.RowCount + context.RowDragFrom];
								Rect rect2 = gUITableCell.Rect;
								((Rect)(ref rect2)).set_xMin(((Rect)(ref rect)).get_xMin());
								SirenixEditorGUI.DrawSolidRect(rect2, new Color(0f, 0f, 0f, 0.2f));
							});
						}
					}
				}
			}
			else
			{
				GUI.Label(rect, "...", EditorStyles.get_centeredGreyMiniLabel());
			}
			if (context.Attribute.IsReadOnly || (int)Event.get_current().get_type() != 0 || Event.get_current().get_button() != 1 || !((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				return;
			}
			Event.get_current().Use();
			GenericMenu val2 = new GenericMenu();
			val2.AddItem(new GUIContent("Insert 1 above"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneRowAbove(arr, rowIndex) : MultiDimArrayUtilities.InsertOneColumnLeft(arr, rowIndex));
			});
			val2.AddItem(new GUIContent("Insert 1 below"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneRowBelow(arr, rowIndex) : MultiDimArrayUtilities.InsertOneColumnRight(arr, rowIndex));
			});
			val2.AddItem(new GUIContent("Duplicate"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DuplicateRow(arr, rowIndex) : MultiDimArrayUtilities.DuplicateColumn(arr, rowIndex));
			});
			val2.AddSeparator("");
			val2.AddItem(new GUIContent("Delete"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DeleteRow(arr, rowIndex) : MultiDimArrayUtilities.DeleteColumn(arr, rowIndex));
			});
			val2.ShowAsContext();
		}

		private void DrawColumn(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int columnIndex)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Invalid comparison between Unknown and I4
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Invalid comparison between Unknown and I4
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Invalid comparison between Unknown and I4
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0401: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_043f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0446: Expected O, but got Unknown
			//IL_044d: Unknown result type (might be due to invalid IL or missing references)
			//IL_045a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0464: Expected O, but got Unknown
			//IL_0464: Expected O, but got Unknown
			//IL_046b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_0482: Expected O, but got Unknown
			//IL_0482: Expected O, but got Unknown
			//IL_0489: Unknown result type (might be due to invalid IL or missing references)
			//IL_0496: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a0: Expected O, but got Unknown
			//IL_04a0: Expected O, but got Unknown
			//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ca: Expected O, but got Unknown
			//IL_04ca: Expected O, but got Unknown
			if (columnIndex < context.ColCount)
			{
				GUI.Label(rect, columnIndex.ToString(), SirenixGUIStyles.LabelCentered);
				if (!context.Attribute.IsReadOnly)
				{
					int controlID = GUIUtility.GetControlID((FocusType)2);
					if (GUI.get_enabled() && (int)Event.get_current().get_type() == 0 && Event.get_current().get_button() == 0 && ((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
					{
						GUIHelper.RemoveFocusControl();
						GUIUtility.set_hotControl(controlID);
						EditorGUIUtility.SetWantsMouseJumping(1);
						Event.get_current().Use();
						context.ColumnDragFrom = columnIndex;
						context.ColumnDragTo = columnIndex;
						context.dragStartPos = Event.get_current().get_mousePosition();
					}
					else if (GUIUtility.get_hotControl() == controlID)
					{
						Vector2 val = context.dragStartPos - Event.get_current().get_mousePosition();
						if (((Vector2)(ref val)).get_sqrMagnitude() > 25f)
						{
							context.IsDraggingColumn = true;
						}
						if ((int)Event.get_current().get_type() == 3)
						{
							Event.get_current().Use();
						}
						else if ((int)Event.get_current().get_type() == 1)
						{
							GUIUtility.set_hotControl(0);
							EditorGUIUtility.SetWantsMouseJumping(0);
							Event.get_current().Use();
							context.IsDraggingColumn = false;
							if (context.Attribute.Transpose)
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveRow(arr, context.ColumnDragFrom, context.ColumnDragTo));
							}
							else
							{
								ApplyArrayModifications(entry, (TElement[,] arr) => MultiDimArrayUtilities.MoveColumn(arr, context.ColumnDragFrom, context.ColumnDragTo));
							}
						}
					}
					if (context.IsDraggingColumn && (int)Event.get_current().get_type() == 7)
					{
						float x = Event.get_current().get_mousePosition().x;
						if (x > ((Rect)(ref rect)).get_x() - 1f && x < ((Rect)(ref rect)).get_x() + ((Rect)(ref rect)).get_width() + 1f)
						{
							Rect arrowRect;
							if (x > ((Rect)(ref rect)).get_x() + ((Rect)(ref rect)).get_width() * 0.5f)
							{
								arrowRect = rect.AlignRight(16f);
								((Rect)(ref arrowRect)).set_height(16f);
								ref Rect reference = ref arrowRect;
								((Rect)(ref reference)).set_y(((Rect)(ref reference)).get_y() - 13f);
								ref Rect reference2 = ref arrowRect;
								((Rect)(ref reference2)).set_x(((Rect)(ref reference2)).get_x() + 8f);
								context.ColumnDragTo = columnIndex;
							}
							else
							{
								arrowRect = rect.AlignLeft(16f);
								((Rect)(ref arrowRect)).set_height(16f);
								ref Rect reference3 = ref arrowRect;
								((Rect)(ref reference3)).set_y(((Rect)(ref reference3)).get_y() - 13f);
								ref Rect reference4 = ref arrowRect;
								((Rect)(ref reference4)).set_x(((Rect)(ref reference4)).get_x() - 8f);
								context.ColumnDragTo = columnIndex - 1;
							}
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								//IL_0001: Unknown result type (might be due to invalid IL or missing references)
								//IL_0016: Unknown result type (might be due to invalid IL or missing references)
								//IL_001b: Unknown result type (might be due to invalid IL or missing references)
								//IL_0020: Unknown result type (might be due to invalid IL or missing references)
								//IL_006c: Unknown result type (might be due to invalid IL or missing references)
								//IL_0071: Unknown result type (might be due to invalid IL or missing references)
								//IL_007e: Unknown result type (might be due to invalid IL or missing references)
								//IL_0093: Unknown result type (might be due to invalid IL or missing references)
								GUI.DrawTexture(arrowRect, EditorIcons.ArrowDown.Active);
								Rect val3 = arrowRect;
								((Rect)(ref val3)).set_x(((Rect)(ref val3)).get_center().x - 2f + 1f);
								((Rect)(ref val3)).set_width(3f);
								((Rect)(ref val3)).set_y(((Rect)(ref val3)).get_y() + 14f);
								Rect tableRect = context.Table.TableRect;
								((Rect)(ref val3)).set_yMax(((Rect)(ref tableRect)).get_yMax());
								EditorGUI.DrawRect(val3, new Color(0f, 0f, 0f, 0.6f));
							});
						}
						if (columnIndex == context.ColCount - 1)
						{
							entry.Property.Tree.DelayActionUntilRepaint(delegate
							{
								//IL_004c: Unknown result type (might be due to invalid IL or missing references)
								//IL_0051: Unknown result type (might be due to invalid IL or missing references)
								//IL_0064: Unknown result type (might be due to invalid IL or missing references)
								//IL_0079: Unknown result type (might be due to invalid IL or missing references)
								GUITableCell gUITableCell = context.Table[context.Table.ColumnCount - context.ColCount + context.ColumnDragFrom, context.Table.RowCount - 1];
								Rect rect2 = gUITableCell.Rect;
								((Rect)(ref rect2)).set_yMin(((Rect)(ref rect)).get_yMin());
								SirenixEditorGUI.DrawSolidRect(rect2, new Color(0f, 0f, 0f, 0.2f));
							});
						}
					}
				}
			}
			else
			{
				GUI.Label(rect, "-", EditorStyles.get_centeredGreyMiniLabel());
			}
			if (context.Attribute.IsReadOnly || (int)Event.get_current().get_type() != 0 || Event.get_current().get_button() != 1 || !((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				return;
			}
			Event.get_current().Use();
			GenericMenu val2 = new GenericMenu();
			val2.AddItem(new GUIContent("Insert 1 left"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneColumnLeft(arr, columnIndex) : MultiDimArrayUtilities.InsertOneRowAbove(arr, columnIndex));
			});
			val2.AddItem(new GUIContent("Insert 1 right"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.InsertOneColumnRight(arr, columnIndex) : MultiDimArrayUtilities.InsertOneRowBelow(arr, columnIndex));
			});
			val2.AddItem(new GUIContent("Duplicate"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DuplicateColumn(arr, columnIndex) : MultiDimArrayUtilities.DuplicateRow(arr, columnIndex));
			});
			val2.AddSeparator("");
			val2.AddItem(new GUIContent("Delete"), false, (MenuFunction)delegate
			{
				ApplyArrayModifications(entry, (TElement[,] arr) => (!TableMatrixAttribute.Transpose) ? MultiDimArrayUtilities.DeleteColumn(arr, columnIndex) : MultiDimArrayUtilities.DeleteRow(arr, columnIndex));
			});
			val2.ShowAsContext();
		}

		private void ApplyArrayModifications(IPropertyValueEntry<TArray> entry, Func<TElement[,], TElement[,]> modification)
		{
			for (int i = 0; i < entry.Values.Count; i++)
			{
				int localI = i;
				TElement[,] newArr = modification(entry.Values[localI] as TElement[,]);
				entry.Property.Tree.DelayActionUntilRepaint(delegate
				{
					entry.Values[localI] = (TArray)(object)newArr;
				});
			}
		}

		private void DrawElement(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int x, int y)
		{
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			if (x >= context.ColCount || y >= context.RowCount)
			{
				return;
			}
			int num = (context.Attribute.Transpose ? x : y);
			int num2 = (context.Attribute.Transpose ? y : x);
			bool showMixedValue = false;
			if (entry.Values.Count != 1)
			{
				for (int i = 1; i < entry.Values.Count; i++)
				{
					TElement a = (entry.Values[i] as TElement[,])[num2, num];
					TElement b = (entry.Values[i - 1] as TElement[,])[num2, num];
					if (!CompareElement(a, b))
					{
						showMixedValue = true;
						break;
					}
				}
			}
			EditorGUI.set_showMixedValue(showMixedValue);
			EditorGUI.BeginChangeCheck();
			TElement val = context.Value[num2, num];
			TElement val2;
			if (context.DrawElement != null)
			{
				context.DrawElement.Context.NamedValues.Set("rect", rect);
				context.DrawElement.Context.NamedValues.Set("element", val);
				context.DrawElement.Context.NamedValues.Set("value", val);
				context.DrawElement.Context.NamedValues.Set("array", context.Value);
				context.DrawElement.Context.NamedValues.Set("x", x);
				context.DrawElement.Context.NamedValues.Set("y", y);
				val2 = context.DrawElement.GetValue();
			}
			else
			{
				val2 = DrawElement(rect, val);
			}
			if (EditorGUI.EndChangeCheck())
			{
				for (int j = 0; j < entry.Values.Count; j++)
				{
					(entry.Values[j] as TElement[,])[num2, num] = val2;
				}
				entry.Values.ForceMarkDirty();
			}
		}

		/// <summary>
		/// Compares the element.
		/// </summary>
		protected virtual bool CompareElement(TElement a, TElement b)
		{
			return EqualityComparer<TElement>.Default.Equals(a, b);
		}

		/// <summary>
		/// Draws a table cell element.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <param name="value">The input value.</param>
		/// <returns>The output value.</returns>
		protected abstract TElement DrawElement(Rect rect, TElement value);
	}
}
