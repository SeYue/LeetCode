using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities.Editor.Expressions;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Property drawer for anything that has a <see cref="T:Sirenix.OdinInspector.Editor.ICollectionResolver" />.
	/// </summary>
	[AllowGUIEnabledForReadonly]
	[DrawerPriority(0.0, 0.0, 0.9)]
	public class CollectionDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems, IHackyListDrawerInteractions
	{
		private class FilteredPropertyChildren
		{
			public InspectorProperty Property;

			public PropertyChildren Children;

			public PropertySearchFilter SearchFilter;

			private List<InspectorProperty> FilteredChildren;

			public bool IsCurrentlyFiltered => FilteredChildren != null;

			public int Count
			{
				get
				{
					if (FilteredChildren == null)
					{
						return Children.Count;
					}
					return FilteredChildren.Count;
				}
			}

			public InspectorProperty this[int index]
			{
				get
				{
					if (FilteredChildren == null)
					{
						return Children[index];
					}
					return FilteredChildren[index];
				}
			}

			public FilteredPropertyChildren(InspectorProperty property, PropertySearchFilter searchFilter)
			{
				Property = property;
				Children = property.Children;
				SearchFilter = searchFilter;
			}

			public void Update()
			{
				if (SearchFilter == null || string.IsNullOrEmpty(SearchFilter.SearchTerm))
				{
					FilteredChildren = null;
					return;
				}
				if (FilteredChildren != null)
				{
					FilteredChildren.Clear();
				}
				else
				{
					FilteredChildren = new List<InspectorProperty>();
				}
				for (int i = 0; i < Children.Count; i++)
				{
					InspectorProperty inspectorProperty = Children[i];
					if (SearchFilter.IsMatch(inspectorProperty, SearchFilter.SearchTerm))
					{
						FilteredChildren.Add(inspectorProperty);
					}
					else
					{
						if (!SearchFilter.Recursive)
						{
							continue;
						}
						foreach (InspectorProperty item in inspectorProperty.Children.Recurse())
						{
							if (SearchFilter.IsMatch(item, SearchFilter.SearchTerm))
							{
								FilteredChildren.Add(inspectorProperty);
								break;
							}
						}
					}
				}
			}

			public void ScheduleUpdate()
			{
				Property.Tree.DelayActionUntilRepaint(delegate
				{
					Update();
					GUIHelper.RequestRepaint();
				});
			}
		}

		private struct ListItemInfo
		{
			public float Width;

			public Rect RemoveBtnRect;

			public Rect DragHandleRect;
		}

		private class ListDrawerConfigInfo
		{
			public ICollectionResolver CollectionResolver;

			public IOrderedCollectionResolver OrderedCollectionResolver;

			public bool IsEmpty;

			public ListDrawerSettingsAttribute CustomListDrawerOptions;

			public int Count;

			public int StartIndex;

			public int EndIndex;

			public DropZoneHandle DropZone;

			public Vector2 DraggingMousePosition;

			public Vector2 DropZoneTopLeft;

			public int InsertAt;

			public int RemoveAt;

			public object[] RemoveValues;

			public bool ShowAllWhilePaging;

			public ObjectPicker ObjectPicker;

			public bool JumpToNextPageOnAdd;

			public GeneralDrawerConfig ListConfig;

			public InspectorProperty Property;

			public GUIContent Label;

			public bool IsAboutToDroppingUnityObjects;

			public bool IsDroppingUnityObjects;

			public bool HideAddButton;

			public bool HideRemoveButton;

			public FilteredPropertyChildren FilteredChildren;

			public bool BaseDraggable;

			public bool BaseIsReadOnly;

			public string SearchFieldControlName = "CollectionSearchFilter_" + Guid.NewGuid().ToString();

			public ActionResolver OnTitleBarGUI;

			public ActionResolver GetCustomAddFunctionVoid;

			public ValueResolver GetCustomAddFunction;

			public ActionResolver CustomRemoveIndexFunction;

			public ActionResolver CustomRemoveElementFunction;

			public ActionResolver OnBeginListElementGUI;

			public ActionResolver OnEndListElementGUI;

			public ValueResolver<Color> ElementColor;

			public Func<object, InspectorProperty, object> GetListElementLabelText;

			public GUIStyle ListItemStyle;

			public bool IsReadOnly
			{
				get
				{
					if (!BaseIsReadOnly)
					{
						return FilteredChildren.IsCurrentlyFiltered;
					}
					return true;
				}
			}

			public bool Draggable
			{
				get
				{
					if (BaseDraggable)
					{
						return !FilteredChildren.IsCurrentlyFiltered;
					}
					return false;
				}
			}

			public int NumberOfItemsPerPage
			{
				get
				{
					if (!CustomListDrawerOptions.NumberOfItemsPerPageHasValue)
					{
						return ListConfig.NumberOfItemsPrPage;
					}
					return CustomListDrawerOptions.NumberOfItemsPerPage;
				}
			}

			public ListDrawerConfigInfo()
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Expected O, but got Unknown
				//IL_0044: Expected O, but got Unknown
				GUIStyle val = new GUIStyle(GUIStyle.get_none());
				val.set_padding(new RectOffset(25, 20, 3, 3));
				ListItemStyle = val;
				base._002Ector();
			}
		}

		private static GUILayoutOption[] listItemOptions = GUILayoutOptions.MinHeight(25f).ExpandWidth();

		private ListDrawerConfigInfo info;

		private string errorMessage;

		private Action<object[]> onValuesCreated;

		private Action superHackyAddFunctionWeSeriouslyNeedANewListDrawer;

		bool IHackyListDrawerInteractions.CanCreateValuesToAdd => info.GetCustomAddFunctionVoid == null;

		void IHackyListDrawerInteractions.CreateValuesToAdd(Action<object[]> onCreated, Rect potentialPopupPosition)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			onValuesCreated = onCreated;
			StartCreatingValues(potentialPopupPosition);
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return property.ChildResolver is ICollectionResolver;
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Expected O, but got Unknown
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Expected O, but got Unknown
			//IL_01de: Expected O, but got Unknown
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Expected O, but got Unknown
			//IL_01ff: Expected O, but got Unknown
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Expected O, but got Unknown
			//IL_022b: Expected O, but got Unknown
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Expected O, but got Unknown
			if (property.ValueEntry.WeakSmartValue == null)
			{
				return;
			}
			ICollectionResolver collectionResolver = property.ChildResolver as ICollectionResolver;
			bool isReadOnly = collectionResolver.IsReadOnly;
			ListDrawerSettingsAttribute attribute = property.GetAttribute<ListDrawerSettingsAttribute>();
			bool flag = !isReadOnly && property.ValueEntry.IsEditable && (attribute == null || !attribute.IsReadOnlyHasValue || (attribute.IsReadOnlyHasValue && !attribute.IsReadOnly));
			bool flag2 = flag && Clipboard.CanPaste(collectionResolver.ElementType);
			bool flag3 = flag && property.Children.Count > 0;
			bool flag4 = flag && property.ChildResolver is IOrderedCollectionResolver && typeof(IList).IsAssignableFrom(typeof(T));
			int windowWidth = 300;
			Rect rect = property.LastDrawnValueRect.AlignTop(1f);
			ref Rect reference = ref rect;
			((Rect)(ref reference)).set_y(((Rect)(ref reference)).get_y() + 14f);
			ref Rect reference2 = ref rect;
			((Rect)(ref reference2)).set_xMin(((Rect)(ref reference2)).get_xMin() + (((Rect)(ref rect)).get_width() * 0.5f - (float)windowWidth * 0.5f));
			((Rect)(ref rect)).set_position(GUIUtility.GUIToScreenPoint(((Rect)(ref rect)).get_position()));
			((Rect)(ref rect)).set_width(1f);
			if (flag4)
			{
				if (info.GetCustomAddFunctionVoid != null)
				{
					genericMenu.AddDisabledItem(new GUIContent("Set Collection Size - disabled by 'void " + info.CustomListDrawerOptions.CustomAddFunction + "'"));
				}
				else
				{
					genericMenu.AddItem(new GUIContent("Set Collection Size"), false, (MenuFunction)delegate
					{
						//IL_0049: Unknown result type (might be due to invalid IL or missing references)
						EditorWindow window = null;
						Action cancel = delegate
						{
							UnityEditorEventUtility.EditorApplication_delayCall += window.Close;
						};
						Action<int> confirm = delegate(int size)
						{
							UnityEditorEventUtility.EditorApplication_delayCall += window.Close;
							SetCollectionSize(property, size);
						};
						CollectionSizeDialogue obj = new CollectionSizeDialogue(confirm, cancel, property.ChildResolver.MaxChildCountSeen);
						window = (EditorWindow)(object)OdinEditorWindow.InspectObjectInDropDown((object)obj, rect, (float)windowWidth);
						GUIHelper.RequestRepaint();
					});
				}
			}
			if (flag2)
			{
				genericMenu.AddItem(new GUIContent("Paste Element"), false, (MenuFunction)delegate
				{
					(property.ChildResolver as ICollectionResolver).QueueAdd(new object[1] { Clipboard.Paste() });
					GUIHelper.RequestRepaint();
				});
			}
			if (flag3)
			{
				genericMenu.AddSeparator("");
				genericMenu.AddItem(new GUIContent("Clear Collection"), false, (MenuFunction)delegate
				{
					(property.ChildResolver as ICollectionResolver).QueueClear();
					GUIHelper.RequestRepaint();
				});
			}
			else
			{
				genericMenu.AddSeparator("");
				genericMenu.AddDisabledItem(new GUIContent("Clear Collection"));
			}
		}

		private void SetCollectionSize(InspectorProperty p, int targetSize)
		{
			IOrderedCollectionResolver orderedCollectionResolver = p.ChildResolver as IOrderedCollectionResolver;
			for (int i = 0; i < p.ParentValues.Count; i++)
			{
				IList list = p.ValueEntry.WeakValues[i] as IList;
				int count = list.Count;
				int num = Math.Abs(targetSize - count);
				if (targetSize > count)
				{
					for (int j = 0; j < num; j++)
					{
						object valueToAdd = GetValueToAdd(i);
						orderedCollectionResolver.QueueAdd(valueToAdd, i);
					}
				}
				else
				{
					for (int k = 0; k < num; k++)
					{
						orderedCollectionResolver.QueueRemoveAt(count - (1 + k), i);
					}
				}
			}
		}

		private object GetValueToAdd(int selectionIndex)
		{
			bool wasFallback;
			return GetValueToAdd(selectionIndex, out wasFallback);
		}

		private object GetValueToAdd(int selectionIndex, out bool wasFallback)
		{
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Invalid comparison between Unknown and I4
			wasFallback = false;
			if (info.GetCustomAddFunction != null)
			{
				return info.GetCustomAddFunction.GetWeakValue(selectionIndex);
			}
			if (info.CustomListDrawerOptions.AlwaysAddDefaultValue)
			{
				if (!info.Property.ValueEntry.SerializationBackend.SupportsPolymorphism)
				{
					return UnitySerializationUtility.CreateDefaultUnityInitializedObject(info.CollectionResolver.ElementType);
				}
				if (info.CollectionResolver.ElementType.IsValueType)
				{
					return Activator.CreateInstance(info.CollectionResolver.ElementType);
				}
				return null;
			}
			if (info.CustomListDrawerOptions.AddCopiesLastElement && info.Count > 0)
			{
				object obj = null;
				IPropertyValueEntry propertyValueEntry = info.FilteredChildren[info.Count - 1].ValueEntry;
				IEnumerable enumerable = info.Property.ValueEntry.WeakValues[selectionIndex] as IEnumerable;
				if (enumerable != null)
				{
					foreach (object item in enumerable)
					{
						obj = item;
					}
				}
				else
				{
					obj = propertyValueEntry.WeakValues[selectionIndex];
				}
				return SerializationUtility.CreateCopy(obj);
			}
			if (info.CollectionResolver.ElementType.InheritsFrom<Object>() && (int)Event.get_current().get_modifiers() == 2)
			{
				return null;
			}
			wasFallback = true;
			Type elementType = (base.Property.ChildResolver as ICollectionResolver).ElementType;
			if (!base.ValueEntry.SerializationBackend.SupportsPolymorphism)
			{
				return UnitySerializationUtility.CreateDefaultUnityInitializedObject(elementType);
			}
			if (!elementType.IsValueType)
			{
				return null;
			}
			return Activator.CreateInstance(elementType);
		}

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			ICollectionResolver collectionResolver = base.Property.ChildResolver as ICollectionResolver;
			bool isReadOnly = collectionResolver.IsReadOnly;
			ListDrawerSettingsAttribute listDrawerSettingsAttribute = base.Property.GetAttribute<ListDrawerSettingsAttribute>() ?? new ListDrawerSettingsAttribute();
			isReadOnly = !base.ValueEntry.IsEditable || isReadOnly || (listDrawerSettingsAttribute.IsReadOnlyHasValue && listDrawerSettingsAttribute.IsReadOnly);
			PropertySearchFilter searchFilter = null;
			SearchableAttribute attribute = base.Property.GetAttribute<SearchableAttribute>();
			if (attribute != null)
			{
				searchFilter = new PropertySearchFilter(null, attribute);
				collectionResolver.OnAfterChange += delegate
				{
					base.Property.Children.Update();
					info.FilteredChildren.Update();
				};
			}
			if (listDrawerSettingsAttribute.ExpandedHasValue)
			{
				base.Property.State.Expanded = listDrawerSettingsAttribute.Expanded;
			}
			info = new ListDrawerConfigInfo
			{
				StartIndex = 0,
				RemoveAt = -1,
				ShowAllWhilePaging = false,
				EndIndex = 0,
				CustomListDrawerOptions = listDrawerSettingsAttribute,
				BaseIsReadOnly = isReadOnly,
				BaseDraggable = !isReadOnly,
				HideAddButton = (isReadOnly || listDrawerSettingsAttribute.HideAddButton),
				HideRemoveButton = (isReadOnly || listDrawerSettingsAttribute.HideRemoveButton),
				FilteredChildren = new FilteredPropertyChildren(base.Property, searchFilter)
			};
			info.ListConfig = GlobalConfig<GeneralDrawerConfig>.Instance;
			info.Property = base.Property;
			if (listDrawerSettingsAttribute.DraggableHasValue && !listDrawerSettingsAttribute.DraggableItems)
			{
				info.BaseDraggable = false;
			}
			if (!(base.Property.ChildResolver is IOrderedCollectionResolver))
			{
				info.BaseDraggable = false;
			}
			if (info.CustomListDrawerOptions.OnBeginListElementGUI != null)
			{
				info.OnBeginListElementGUI = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.OnBeginListElementGUI, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("index", typeof(int)));
			}
			if (info.CustomListDrawerOptions.OnEndListElementGUI != null)
			{
				info.OnEndListElementGUI = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.OnEndListElementGUI, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("index", typeof(int)));
			}
			if (info.CustomListDrawerOptions.OnTitleBarGUI != null)
			{
				info.OnTitleBarGUI = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.OnTitleBarGUI);
			}
			if (info.CustomListDrawerOptions.ElementColor != null)
			{
				info.ElementColor = ValueResolver.Get<Color>(base.Property, info.CustomListDrawerOptions.ElementColor, new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue[2]
				{
					new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("index", typeof(int)),
					new Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue("defaultColor", typeof(Color))
				});
			}
			if (info.CustomListDrawerOptions.ListElementLabelName != null)
			{
				info.GetListElementLabelText = CreateListElementLabelNameGetter(info.CustomListDrawerOptions.ListElementLabelName, collectionResolver.ElementType, ref errorMessage);
			}
			if (info.CustomListDrawerOptions.CustomAddFunction != null)
			{
				info.GetCustomAddFunction = ValueResolver.Get(collectionResolver.ElementType, base.Property, info.CustomListDrawerOptions.CustomAddFunction);
				if (info.GetCustomAddFunction.HasError)
				{
					info.GetCustomAddFunctionVoid = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.CustomAddFunction);
					if (!info.GetCustomAddFunctionVoid.HasError)
					{
						info.GetCustomAddFunction = null;
					}
				}
			}
			if (info.CustomListDrawerOptions.CustomRemoveIndexFunction != null)
			{
				if (!(base.Property.ChildResolver is IOrderedCollectionResolver))
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage += "ListDrawerSettings.CustomRemoveIndexFunction is invalid on unordered collections. Use ListDrawerSetings.CustomRemoveElementFunction instead.";
				}
				else
				{
					info.CustomRemoveIndexFunction = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.CustomRemoveIndexFunction, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("index", typeof(int)));
				}
			}
			else if (info.CustomListDrawerOptions.CustomRemoveElementFunction != null)
			{
				info.CustomRemoveElementFunction = ActionResolver.Get(base.Property, info.CustomListDrawerOptions.CustomRemoveElementFunction, new Sirenix.OdinInspector.Editor.ActionResolvers.NamedValue("removeElement", collectionResolver.ElementType));
			}
		}

		private static Func<object, InspectorProperty, object> CreateListElementLabelNameGetter(string resolvedString, Type elementType, ref string errorMessage)
		{
			if (resolvedString.Length > 1 && resolvedString[0] == '@')
			{
				string text = resolvedString.Substring(1);
				Type[] parameters = new Type[1] { typeof(InspectorProperty) };
				string[] parameterNames = new string[1] { "property" };
				string text2;
				Delegate exprDelegate = ExpressionUtility.ParseExpression(text, isStatic: false, elementType, parameters, parameterNames, out text2);
				if (text2 != null)
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage += text2;
					return null;
				}
				Type returnType = exprDelegate.Method.ReturnType;
				if (returnType == typeof(void) || returnType == null)
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage = errorMessage + "ListElementLabelName expression '" + text + "' is not allowed to evaluate to 'void'.";
					return null;
				}
				object[] exprParameters = new object[2];
				return delegate(object instance, InspectorProperty property)
				{
					exprParameters[0] = instance;
					exprParameters[1] = property;
					return exprDelegate.DynamicInvoke(exprParameters);
				};
			}
			FieldInfo fieldInfo = elementType.GetField(resolvedString, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (fieldInfo != null)
			{
				if (fieldInfo.IsStatic)
				{
					return (object instance, InspectorProperty property) => fieldInfo.GetValue(null);
				}
				return (object instance, InspectorProperty property) => fieldInfo.GetValue(instance);
			}
			PropertyInfo propertyInfo = elementType.GetProperty(resolvedString, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			if (propertyInfo != null)
			{
				if (propertyInfo.IsStatic())
				{
					return (object instance, InspectorProperty property) => propertyInfo.GetValue(null, null);
				}
				return (object instance, InspectorProperty property) => propertyInfo.GetValue(instance, null);
			}
			MethodInfo methodInfo = elementType.GetMethod(resolvedString, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, Type.EmptyTypes, null);
			if (methodInfo != null)
			{
				if (methodInfo.ReturnType == typeof(void) || methodInfo.ReturnType == null)
				{
					if (errorMessage != null)
					{
						errorMessage += "\n\n";
					}
					errorMessage = errorMessage + "ListElementLabelName method '" + resolvedString + "' on element type '" + elementType.GetNiceName() + "' is not allowed to return void.";
					return null;
				}
				if (methodInfo.IsStatic)
				{
					return (object instance, InspectorProperty prop) => methodInfo.Invoke(null, null);
				}
				return (object instance, InspectorProperty prop) => methodInfo.Invoke(instance, null);
			}
			if (errorMessage != null)
			{
				errorMessage += "\n\n";
			}
			errorMessage = errorMessage + "Couldn't find any field, property or parameterless method named '" + resolvedString + "' on element type '" + elementType.GetNiceName() + "' to use for ListElementLabelName.";
			return null;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Expected O, but got Unknown
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Invalid comparison between Unknown and I4
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Invalid comparison between Unknown and I4
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_047e: Invalid comparison between Unknown and I4
			//IL_0565: Unknown result type (might be due to invalid IL or missing references)
			//IL_056b: Invalid comparison between Unknown and I4
			ICollectionResolver collectionResolver = base.Property.ChildResolver as ICollectionResolver;
			bool isReadOnly = collectionResolver.IsReadOnly;
			if (errorMessage != null)
			{
				SirenixEditorGUI.ErrorMessageBox(errorMessage);
			}
			ActionResolver.DrawErrors(info.OnTitleBarGUI, info.GetCustomAddFunctionVoid, info.CustomRemoveIndexFunction, info.CustomRemoveElementFunction, info.OnBeginListElementGUI, info.OnEndListElementGUI);
			ValueResolver.DrawErrors(info.GetCustomAddFunction, info.ElementColor);
			if (info.Label == null || (label != null && label.get_text() != info.Label.get_text()))
			{
				info.Label = new GUIContent((label == null || string.IsNullOrEmpty(label.get_text())) ? base.Property.ValueEntry.TypeOfValue.GetNiceName() : label.get_text(), (label == null) ? string.Empty : label.get_tooltip());
			}
			info.BaseIsReadOnly = collectionResolver.IsReadOnly;
			info.ListItemStyle.get_padding().set_left(info.Draggable ? 25 : 7);
			info.ListItemStyle.get_padding().set_right((info.BaseIsReadOnly || info.HideRemoveButton) ? 4 : 20);
			if ((int)Event.get_current().get_type() == 7)
			{
				info.DropZoneTopLeft = GUIUtility.GUIToScreenPoint(new Vector2(0f, 0f));
			}
			info.CollectionResolver = base.Property.ChildResolver as ICollectionResolver;
			info.OrderedCollectionResolver = base.Property.ChildResolver as IOrderedCollectionResolver;
			info.Count = info.FilteredChildren.Count;
			info.IsEmpty = info.FilteredChildren.Count == 0;
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			BeginDropZone();
			DrawToolbar();
			if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(base.Property, this), base.Property.State.Expanded))
			{
				GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth - (float)info.ListItemStyle.get_padding().get_left());
				DrawItems();
				GUIHelper.PopLabelWidth();
			}
			SirenixEditorGUI.EndFadeGroup();
			EndDropZone();
			SirenixEditorGUI.EndIndentedVertical();
			if (info.OrderedCollectionResolver != null)
			{
				if (info.RemoveAt >= 0 && (int)Event.get_current().get_type() == 7)
				{
					try
					{
						if (info.CustomRemoveIndexFunction != null && !info.CustomRemoveIndexFunction.HasError)
						{
							base.Property.RecordForUndo("Custom List Remove (Index '" + info.RemoveAt + "')");
							info.CustomRemoveIndexFunction.Context.NamedValues.Set("index", info.RemoveAt);
							info.CustomRemoveIndexFunction.DoActionForAllSelectionIndices();
							base.Property.MarkSerializationRootDirty();
						}
						else if (info.CustomRemoveElementFunction != null && !info.CustomRemoveElementFunction.HasError)
						{
							base.Property.RecordForUndo("Custom List Remove (Element)");
							for (int i = 0; i < base.Property.ParentValues.Count; i++)
							{
								info.CustomRemoveElementFunction.Context.NamedValues.Set("removeElement", base.Property.Children[info.RemoveAt].ValueEntry.WeakValues[i]);
								info.CustomRemoveElementFunction.DoAction(i);
							}
							base.Property.MarkSerializationRootDirty();
						}
						else
						{
							info.OrderedCollectionResolver.QueueRemoveAt(info.RemoveAt);
						}
					}
					finally
					{
						info.RemoveAt = -1;
						info.FilteredChildren.ScheduleUpdate();
					}
					GUIHelper.RequestRepaint();
				}
			}
			else if (info.RemoveValues != null && (int)Event.get_current().get_type() == 7)
			{
				try
				{
					if (info.CustomRemoveElementFunction != null && !info.CustomRemoveElementFunction.HasError)
					{
						for (int j = 0; j < base.Property.ParentValues.Count; j++)
						{
							info.CustomRemoveElementFunction.Context.NamedValues.Set("removeElement", info.RemoveValues[j]);
							info.CustomRemoveElementFunction.DoAction(j);
						}
					}
					else
					{
						info.CollectionResolver.QueueRemove(info.RemoveValues);
					}
				}
				finally
				{
					info.RemoveValues = null;
					info.FilteredChildren.ScheduleUpdate();
				}
				GUIHelper.RequestRepaint();
			}
			if (info.ObjectPicker != null && info.ObjectPicker.IsReadyToClaim && (int)Event.get_current().get_type() == 7)
			{
				object obj = info.ObjectPicker.ClaimObject();
				if (info.JumpToNextPageOnAdd)
				{
					info.StartIndex = int.MaxValue;
				}
				object[] array = new object[info.Property.Tree.WeakTargets.Count];
				array[0] = obj;
				for (int k = 1; k < array.Length; k++)
				{
					array[k] = SerializationUtility.CreateCopy(obj);
				}
				if (onValuesCreated != null)
				{
					onValuesCreated(array);
					onValuesCreated = null;
				}
				else
				{
					info.CollectionResolver.QueueAdd(array);
				}
			}
		}

		private DropZoneHandle BeginDropZone()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Invalid comparison between Unknown and I4
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if (info.OrderedCollectionResolver == null)
			{
				return null;
			}
			DropZoneHandle dropZoneHandle = DragAndDropManager.BeginDropZone(info.Property.Tree.GetHashCode() + "-" + info.Property.Path, info.CollectionResolver.ElementType, canAcceptMove: true);
			if ((int)Event.get_current().get_type() == 7 && DragAndDropManager.IsDragInProgress)
			{
				Rect val = (dropZoneHandle.Rect = dropZoneHandle.Rect);
			}
			dropZoneHandle.Enabled = !info.IsReadOnly;
			info.DropZone = dropZoneHandle;
			return dropZoneHandle;
		}

		private static Object[] HandleUnityObjectsDrop(ListDrawerConfigInfo info)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Invalid comparison between Unknown and I4
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Invalid comparison between Unknown and I4
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Invalid comparison between Unknown and I4
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Invalid comparison between Unknown and I4
			if (info.IsReadOnly)
			{
				return null;
			}
			EventType type = Event.get_current().get_type();
			if ((int)type == 8)
			{
				info.IsAboutToDroppingUnityObjects = false;
			}
			if ((int)type == 9 || (int)type == 10)
			{
				Rect rect = info.DropZone.Rect;
				if (((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
				{
					Object[] array = null;
					if (DragAndDrop.get_objectReferences().Any((Object n) => n != (Object)null && info.CollectionResolver.ElementType.IsAssignableFrom(((object)n).GetType())))
					{
						array = (from x in DragAndDrop.get_objectReferences()
							where x != (Object)null && info.CollectionResolver.ElementType.IsAssignableFrom(((object)x).GetType())
							select x).Reverse().ToArray();
					}
					else if (info.CollectionResolver.ElementType.InheritsFrom(typeof(Component)))
					{
						array = (Object[])(object)(from x in DragAndDrop.get_objectReferences().OfType<GameObject>()
							select x.GetComponent(info.CollectionResolver.ElementType) into x
							where (Object)(object)x != (Object)null
							select x).Reverse().ToArray();
					}
					else if (info.CollectionResolver.ElementType.InheritsFrom(typeof(Sprite)) && DragAndDrop.get_objectReferences().Any((Object n) => n is Texture2D && AssetDatabase.Contains(n)))
					{
						array = (Object[])(object)(from x in DragAndDrop.get_objectReferences().OfType<Texture2D>().Select(delegate(Texture2D x)
							{
								string assetPath = AssetDatabase.GetAssetPath((Object)(object)x);
								return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
							})
							where (Object)(object)x != (Object)null
							select x).Reverse().ToArray();
					}
					if (array != null && array.Length != 0)
					{
						DragAndDrop.set_visualMode((DragAndDropVisualMode)1);
						Event.get_current().Use();
						info.IsAboutToDroppingUnityObjects = true;
						info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
						if ((int)type == 10)
						{
							DragAndDrop.AcceptDrag();
							return array;
						}
					}
				}
			}
			if ((int)type == 7)
			{
				info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
			}
			return null;
		}

		private void EndDropZone()
		{
			if (info.OrderedCollectionResolver == null)
			{
				return;
			}
			if (info.DropZone.IsReadyToClaim)
			{
				if (info.InsertAt == -1)
				{
					info.InsertAt = info.FilteredChildren.Count;
				}
				CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
				CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo = info.Property;
				object obj = info.DropZone.ClaimObject();
				object[] values = new object[info.Property.Tree.WeakTargets.Count];
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = obj;
				}
				if (info.DropZone.IsCrossWindowDrag)
				{
					GUIHelper.RequestRepaint();
					UnityEditorEventUtility.EditorApplication_delayCall += delegate
					{
						info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(info.InsertAt, 0, info.FilteredChildren.Count), values);
					};
				}
				else
				{
					info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(info.InsertAt, 0, info.FilteredChildren.Count), values);
				}
			}
			else if (!info.IsReadOnly)
			{
				Object[] array = HandleUnityObjectsDrop(info);
				if (array != null)
				{
					if (info.InsertAt == -1)
					{
						info.InsertAt = info.FilteredChildren.Count;
					}
					Object[] array2 = array;
					foreach (Object val in array2)
					{
						object[] array3 = new object[info.Property.Tree.WeakTargets.Count];
						for (int k = 0; k < array3.Length; k++)
						{
							array3[k] = val;
						}
						info.OrderedCollectionResolver.QueueInsertAt(Mathf.Clamp(info.InsertAt, 0, info.FilteredChildren.Count), array3);
					}
				}
			}
			DragAndDropManager.EndDropZone();
		}

		private void DrawToolbar()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ac: Unknown result type (might be due to invalid IL or missing references)
			SirenixEditorGUI.BeginHorizontalToolbar();
			if (info.DropZone != null && DragAndDropManager.IsDragInProgress && !info.DropZone.IsAccepted)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			if (info.Property.ValueEntry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PushIsBoldLabel(isBold: true);
			}
			bool flag = (!info.ListConfig.HideFoldoutWhileEmpty || !info.IsEmpty) && !info.CustomListDrawerOptions.Expanded;
			Rect rect = default(Rect);
			if (!flag)
			{
				GUILayout.Label(info.Label, (GUILayoutOption[])GUILayoutOptions.ExpandWidth(expand: false));
			}
			else
			{
				float fieldWidth = EditorGUIUtility.get_fieldWidth();
				EditorGUIUtility.set_fieldWidth(10f);
				rect = EditorGUILayout.GetControlRect(false, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				EditorGUIUtility.set_fieldWidth(fieldWidth);
			}
			if (info.Property.ValueEntry.ListLengthChangedFromPrefab)
			{
				GUIHelper.PopIsBoldLabel();
			}
			if (info.DropZone != null && DragAndDropManager.IsDragInProgress && !info.DropZone.IsAccepted)
			{
				GUIHelper.PopGUIEnabled();
			}
			GUILayout.FlexibleSpace();
			if (info.FilteredChildren.SearchFilter != null)
			{
				Rect rect2 = EditorGUILayout.GetControlRect(false, (GUILayoutOption[])(object)new GUILayoutOption[0]).AddYMin(2f);
				if (UnityVersion.IsVersionOrGreater(2019, 3))
				{
					rect2 = rect2.AddY(-2f);
				}
				string text = SirenixEditorGUI.SearchField(rect2, info.FilteredChildren.SearchFilter.SearchTerm, forceFocus: false, info.SearchFieldControlName);
				if (text != info.FilteredChildren.SearchFilter.SearchTerm)
				{
					if (!string.IsNullOrEmpty(text))
					{
						base.Property.State.Expanded = true;
					}
					info.FilteredChildren.SearchFilter.SearchTerm = text;
					info.FilteredChildren.ScheduleUpdate();
				}
			}
			if (flag)
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(rect, base.Property.State.Expanded, info.Label ?? GUIContent.none);
			}
			if (info.CustomListDrawerOptions.ShowItemCountHasValue ? info.CustomListDrawerOptions.ShowItemCount : info.ListConfig.ShowItemCount)
			{
				if (info.Property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
				{
					GUILayout.Label(info.Count + " / " + info.CollectionResolver.MaxCollectionLength + " items", SirenixGUIStyles.CenteredGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				else if (info.FilteredChildren.IsCurrentlyFiltered)
				{
					GUILayout.Label(info.Count + " / " + info.Property.Children.Count + " items", EditorStyles.get_centeredGreyMiniLabel(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
				else
				{
					GUILayout.Label(info.IsEmpty ? "Empty" : (info.Count + " items"), SirenixGUIStyles.CenteredGreyMiniLabel, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				}
			}
			bool flag2 = !info.CustomListDrawerOptions.PagingHasValue || info.CustomListDrawerOptions.ShowPaging;
			bool flag3 = (info.ListConfig.HidePagingWhileCollapsed && !base.Property.State.Expanded) || (info.ListConfig.HidePagingWhileOnlyOnePage && info.Count <= info.NumberOfItemsPerPage);
			int num = Math.Max(1, info.NumberOfItemsPerPage);
			int num2 = Mathf.CeilToInt((float)info.Count / (float)num);
			int num3 = ((info.Count != 0) ? (info.StartIndex / num % info.Count) : 0);
			if (flag2)
			{
				bool flag4 = flag2 && !flag3 && (DragAndDropManager.IsDragInProgress || info.ShowAllWhilePaging || !base.Property.State.Expanded);
				if (flag4)
				{
					GUIHelper.PushGUIEnabled(enabled: false);
				}
				if (!flag3)
				{
					if (num3 == 0)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft, ignoreGUIEnabled: true))
					{
						if (Event.get_current().get_button() == 0)
						{
							info.StartIndex -= num;
						}
						else
						{
							info.StartIndex = 0;
						}
					}
					if (num3 == 0)
					{
						GUIHelper.PopGUIEnabled();
					}
					int num4 = EditorGUILayout.IntField((num2 != 0) ? (num3 + 1) : 0, (GUILayoutOption[])GUILayoutOptions.Width(10 + num2.ToString(CultureInfo.InvariantCulture).Length * 10)) - 1;
					if (num3 != num4)
					{
						info.StartIndex = num4 * num;
					}
					GUILayout.Label("/ " + num2, (GUILayoutOption[])(object)new GUILayoutOption[0]);
					if (num3 == num2 - 1)
					{
						GUIHelper.PushGUIEnabled(enabled: false);
					}
					if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight, ignoreGUIEnabled: true))
					{
						if (Event.get_current().get_button() == 0)
						{
							info.StartIndex += num;
						}
						else
						{
							info.StartIndex = num * num2;
						}
					}
					if (num3 == num2 - 1)
					{
						GUIHelper.PopGUIEnabled();
					}
				}
				num3 = ((info.Count != 0) ? (info.StartIndex / num % info.Count) : 0);
				int num5 = Mathf.Clamp(num3 * num, 0, Mathf.Max(0, info.Count - 1));
				if (num5 != info.StartIndex)
				{
					info.StartIndex = num5;
					int num6 = ((info.Count != 0) ? (info.StartIndex / num % info.Count) : 0);
					if (num3 != num6)
					{
						num3 = num6;
						info.StartIndex = Mathf.Clamp(num3 * num, 0, Mathf.Max(0, info.Count - 1));
					}
				}
				info.EndIndex = Mathf.Min(info.StartIndex + num, info.Count);
				if (flag4)
				{
					GUIHelper.PopGUIEnabled();
				}
			}
			else
			{
				info.StartIndex = 0;
				info.EndIndex = info.Count;
			}
			if (flag2 && !flag3 && info.ListConfig.ShowExpandButton)
			{
				if (info.Count < 300)
				{
					if (SirenixEditorGUI.ToolbarButton(info.ShowAllWhilePaging ? EditorIcons.TriangleUp : EditorIcons.TriangleDown, ignoreGUIEnabled: true))
					{
						info.ShowAllWhilePaging = !info.ShowAllWhilePaging;
					}
				}
				else
				{
					info.ShowAllWhilePaging = false;
				}
			}
			if (!info.IsReadOnly && !info.HideAddButton)
			{
				info.ObjectPicker = ObjectPicker.GetObjectPicker(info, info.CollectionResolver.ElementType);
				superHackyAddFunctionWeSeriouslyNeedANewListDrawer = CollectionDrawerStaticInfo.NextCustomAddFunction;
				CollectionDrawerStaticInfo.NextCustomAddFunction = null;
				if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
				{
					StartCreatingValues(GUIHelper.GetCurrentLayoutRect());
				}
				info.JumpToNextPageOnAdd = flag2 && info.Count % num == 0 && num3 + 1 == num2;
			}
			if (info.OnTitleBarGUI != null && !info.OnTitleBarGUI.HasError)
			{
				info.OnTitleBarGUI.DoAction();
			}
			SirenixEditorGUI.EndHorizontalToolbar();
		}

		private void StartCreatingValues(Rect potentialPopupPosition)
		{
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			if (superHackyAddFunctionWeSeriouslyNeedANewListDrawer != null)
			{
				superHackyAddFunctionWeSeriouslyNeedANewListDrawer();
				return;
			}
			if (info.GetCustomAddFunctionVoid != null && !info.GetCustomAddFunctionVoid.HasError)
			{
				info.GetCustomAddFunctionVoid.DoAction();
				object obj = base.Property.SerializationRoot.ValueEntry.WeakValues[0];
				Object val = obj as Object;
				if (val != (Object)null)
				{
					InspectorUtilities.RegisterUnityObjectDirty(val);
				}
				return;
			}
			object[] array = new object[info.Property.ValueEntry.ValueCount];
			array[0] = GetValueToAdd(0, out var wasFallback);
			if (wasFallback)
			{
				info.ObjectPicker.ShowObjectPicker(null, info.Property.GetAttribute<AssetsOnlyAttribute>() == null, potentialPopupPosition, !info.Property.ValueEntry.SerializationBackend.SupportsPolymorphism);
				return;
			}
			for (int i = 1; i < array.Length; i++)
			{
				array[i] = GetValueToAdd(i);
			}
			if (onValuesCreated != null)
			{
				onValuesCreated(array);
				onValuesCreated = null;
			}
			else
			{
				info.CollectionResolver.QueueAdd(array);
			}
		}

		private void DrawItems()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Invalid comparison between Unknown and I4
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_023e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_024c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_029d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0325: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 9 || (int)Event.get_current().get_type() == 10)
			{
				info.DraggingMousePosition = Event.get_current().get_mousePosition();
			}
			info.InsertAt = -1;
			int num = 0;
			int num2 = info.Count;
			if ((!info.CustomListDrawerOptions.PagingHasValue || info.CustomListDrawerOptions.ShowPaging) && !info.ShowAllWhilePaging)
			{
				num = Mathf.Clamp(info.StartIndex, 0, info.Count);
				num2 = Mathf.Clamp(info.EndIndex, 0, info.Count);
			}
			Color listItemColorEven = SirenixGUIStyles.ListItemColorEven;
			Color listItemColorOdd = SirenixGUIStyles.ListItemColorOdd;
			bool flag = (info.DropZone != null && info.DropZone.IsBeingHovered) || info.IsDroppingUnityObjects;
			float num3;
			Rect rect;
			if (!flag)
			{
				num3 = 0f;
			}
			else if (!info.IsDroppingUnityObjects)
			{
				rect = DragAndDropManager.CurrentDraggingHandle.Rect;
				num3 = ((Rect)(ref rect)).get_height();
			}
			else
			{
				num3 = 16f;
			}
			float num4 = num3;
			Rect val = SirenixEditorGUI.BeginVerticalList(true, true);
			int num5 = 0;
			int i = num;
			int num6 = num;
			for (; i < num2; i++)
			{
				DragHandle dragHandle = BeginDragHandle(i, num5);
				if (flag)
				{
					Rect rect2 = dragHandle.Rect;
					((Rect)(ref rect2)).set_height(((Rect)(ref rect2)).get_height() / 2f);
					if (((Rect)(ref rect2)).Contains(info.DraggingMousePosition) || (((Rect)(ref rect2)).get_y() > info.DraggingMousePosition.y && num5 == 0))
					{
						GUILayout.Space(num4);
						flag = false;
						info.InsertAt = num6;
					}
				}
				if (!dragHandle.IsDragging)
				{
					num6++;
					DrawItem(info.FilteredChildren[i], dragHandle, listItemColorEven, listItemColorOdd);
				}
				else
				{
					if ((int)Event.get_current().get_type() == 7 && info.InsertAt != i)
					{
						int localJ = i;
						ListDrawerConfigInfo localInfo = info;
						rect = dragHandle.Rect;
						float x = ((Rect)(ref rect)).get_x();
						rect = dragHandle.Rect;
						Vector2 p = GUIUtility.GUIToScreenPoint(new Vector2(x, ((Rect)(ref rect)).get_y()));
						Rect r = dragHandle.Rect;
						base.Property.Tree.DelayAction(delegate
						{
							//IL_0002: Unknown result type (might be due to invalid IL or missing references)
							//IL_0007: Unknown result type (might be due to invalid IL or missing references)
							//IL_000c: Unknown result type (might be due to invalid IL or missing references)
							//IL_0051: Unknown result type (might be due to invalid IL or missing references)
							//IL_005b: Unknown result type (might be due to invalid IL or missing references)
							//IL_0065: Unknown result type (might be due to invalid IL or missing references)
							//IL_006a: Unknown result type (might be due to invalid IL or missing references)
							//IL_0076: Unknown result type (might be due to invalid IL or missing references)
							//IL_0080: Unknown result type (might be due to invalid IL or missing references)
							//IL_008a: Unknown result type (might be due to invalid IL or missing references)
							//IL_008f: Unknown result type (might be due to invalid IL or missing references)
							p = GUIUtility.ScreenToGUIPoint(p);
							((Rect)(ref r)).set_x(p.x);
							((Rect)(ref r)).set_y(p.y);
							if (localInfo.InsertAt > localJ)
							{
								EditorGUI.DrawRect(r.AlignTop(3f).AddY(-3f), SirenixGUIStyles.ListItemDragBgColor);
							}
							else
							{
								EditorGUI.DrawRect(r.AlignBottom(3f).AddY(0f), SirenixGUIStyles.ListItemDragBgColor);
							}
						});
					}
					DelayedGUIDrawer delayedGUIDrawer = CollectionDrawerStaticInfo.DelayedGUIDrawer;
					rect = dragHandle.Rect;
					float width = ((Rect)(ref rect)).get_width();
					rect = dragHandle.Rect;
					delayedGUIDrawer.Begin(width, ((Rect)(ref rect)).get_height());
					DragAndDropManager.AllowDrop = false;
					DrawItem(info.FilteredChildren[i], dragHandle, listItemColorEven, listItemColorOdd);
					DragAndDropManager.AllowDrop = true;
					CollectionDrawerStaticInfo.DelayedGUIDrawer.End();
				}
				if (flag)
				{
					Rect rect3 = dragHandle.Rect;
					((Rect)(ref rect3)).set_height(((Rect)(ref rect3)).get_height() / 2f);
					((Rect)(ref rect3)).set_y(((Rect)(ref rect3)).get_y() + ((Rect)(ref rect3)).get_height());
					if (((Rect)(ref rect3)).Contains(info.DraggingMousePosition) || (((Rect)(ref rect3)).get_yMax() < info.DraggingMousePosition.y && i + 1 == num2))
					{
						GUILayout.Space(num4);
						flag = false;
						info.InsertAt = Mathf.Min(num6, num2);
					}
				}
				EndDragHandle(num5);
				num5++;
			}
			if (flag)
			{
				GUILayout.Space(num4);
				info.InsertAt = ((info.DraggingMousePosition.y > ((Rect)(ref val)).get_center().y) ? num2 : num);
			}
			if (num2 == info.FilteredChildren.Count && info.Property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
			{
				SirenixEditorGUI.BeginListItem(false, null);
				GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.get_centeredGreyMiniLabel(), (GUILayoutOption[])(object)new GUILayoutOption[0]);
				SirenixEditorGUI.EndListItem();
			}
			SirenixEditorGUI.EndVerticalList();
		}

		private void EndDragHandle(int i)
		{
			DragHandle dragHandle = DragAndDropManager.EndDragHandle();
			if (!dragHandle.IsDragging)
			{
				return;
			}
			info.Property.Tree.DelayAction(delegate
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				if (DragAndDropManager.CurrentDraggingHandle != null)
				{
					CollectionDrawerStaticInfo.DelayedGUIDrawer.Draw(info.DraggingMousePosition - DragAndDropManager.CurrentDraggingHandle.MouseDownPostionOffset);
				}
			});
		}

		private DragHandle BeginDragHandle(int j, int i)
		{
			InspectorProperty inspectorProperty = info.FilteredChildren[j];
			DragHandle dragHandle = DragAndDropManager.BeginDragHandle(inspectorProperty, inspectorProperty.ValueEntry.WeakSmartValue, info.IsReadOnly ? DragAndDropMethods.Reference : DragAndDropMethods.Move);
			dragHandle.Enabled = info.Draggable;
			if (dragHandle.OnDragStarted)
			{
				CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo = null;
				CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = info.FilteredChildren[j];
				dragHandle.OnDragFinnished = delegate(DropEvents dropEvent)
				{
					if (dropEvent == DropEvents.Moved)
					{
						if (dragHandle.IsCrossWindowDrag || (CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo != null && CollectionDrawerStaticInfo.CurrentDroppingPropertyInfo.Tree != info.Property.Tree))
						{
							GUIHelper.RequestRepaint();
							UnityEditorEventUtility.EditorApplication_delayCall += delegate
							{
								info.OrderedCollectionResolver.QueueRemoveAt(j);
							};
						}
						else
						{
							info.OrderedCollectionResolver.QueueRemoveAt(j);
						}
					}
					CollectionDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
				};
			}
			return dragHandle;
		}

		private Rect DrawItem(InspectorProperty itemProperty, DragHandle dragHandle, Color evenColor, Color oddColor)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Invalid comparison between Unknown and I4
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Expected O, but got Unknown
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Expected O, but got Unknown
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Expected O, but got Unknown
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_045e: Unknown result type (might be due to invalid IL or missing references)
			int index = itemProperty.Index;
			PropertyContext<ListItemInfo> global = itemProperty.Context.GetGlobal<ListItemInfo>("listItemInfo");
			Color val = ((index % 2 == 0) ? evenColor : oddColor);
			if (info.ElementColor != null && !info.ElementColor.HasError)
			{
				info.ElementColor.Context.NamedValues.Set("index", index);
				info.ElementColor.Context.NamedValues.Set("defaultColor", val);
				val = info.ElementColor.GetValue();
			}
			Rect result = SirenixEditorGUI.BeginListItem(allowHover: false, info.ListItemStyle, val, val, val, val, listItemOptions);
			if ((int)Event.get_current().get_type() == 7 && !info.BaseIsReadOnly)
			{
				global.Value.Width = ((Rect)(ref result)).get_width();
				dragHandle.DragHandleRect = new Rect(((Rect)(ref result)).get_x() + 4f, ((Rect)(ref result)).get_y(), 20f, ((Rect)(ref result)).get_height());
				global.Value.DragHandleRect = new Rect(((Rect)(ref result)).get_x() + 4f, ((Rect)(ref result)).get_y() + 2f + (float)(((int)((Rect)(ref result)).get_height() - 23) / 2), 20f, 20f);
				global.Value.RemoveBtnRect = new Rect(((Rect)(ref global.Value.DragHandleRect)).get_x() + ((Rect)(ref result)).get_width() - 22f, ((Rect)(ref global.Value.DragHandleRect)).get_y() + 1f, 14f, 14f);
				_ = info.HideRemoveButton;
				if (info.Draggable)
				{
					GUI.Label(global.Value.DragHandleRect, EditorIcons.List.Inactive, GUIStyle.get_none());
				}
			}
			GUIHelper.PushHierarchyMode(hierarchyMode: false);
			GUIContent val2 = null;
			if (info.CustomListDrawerOptions.ShowIndexLabelsHasValue)
			{
				if (info.CustomListDrawerOptions.ShowIndexLabels)
				{
					val2 = new GUIContent(index.ToString());
				}
			}
			else if (info.ListConfig.ShowIndexLabels)
			{
				val2 = new GUIContent(index.ToString());
			}
			if (info.GetListElementLabelText != null)
			{
				object weakSmartValue = itemProperty.ValueEntry.WeakSmartValue;
				if (weakSmartValue == null)
				{
					if (val2 == null)
					{
						val2 = new GUIContent("Null");
					}
					else
					{
						GUIContent obj = val2;
						obj.set_text(obj.get_text() + " : Null");
					}
				}
				else
				{
					val2 = (GUIContent)(((object)val2) ?? ((object)new GUIContent("")));
					if (val2.get_text() != "")
					{
						GUIContent obj2 = val2;
						obj2.set_text(obj2.get_text() + " : ");
					}
					object obj3 = info.GetListElementLabelText(weakSmartValue, itemProperty);
					GUIContent obj4 = val2;
					obj4.set_text(obj4.get_text() + ((obj3 == null) ? "" : obj3.ToString()));
				}
			}
			if (info.OnBeginListElementGUI != null && !info.OnBeginListElementGUI.HasError)
			{
				info.OnBeginListElementGUI.Context.NamedValues.Set("index", index);
				info.OnBeginListElementGUI.DoAction();
			}
			itemProperty.Draw(val2);
			if (info.OnEndListElementGUI != null && !info.OnEndListElementGUI.HasError)
			{
				info.OnEndListElementGUI.Context.NamedValues.Set("index", index);
				info.OnEndListElementGUI.DoAction();
			}
			GUIHelper.PopHierarchyMode();
			if (!info.BaseIsReadOnly && !info.HideRemoveButton && SirenixEditorGUI.IconButton(global.Value.RemoveBtnRect, EditorIcons.X))
			{
				if (info.OrderedCollectionResolver != null)
				{
					if (index >= 0)
					{
						info.RemoveAt = index;
					}
				}
				else
				{
					object[] array = new object[itemProperty.ValueEntry.ValueCount];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = itemProperty.ValueEntry.WeakValues[i];
					}
					info.RemoveValues = array;
				}
			}
			SirenixEditorGUI.EndListItem();
			return result;
		}
	}
}
