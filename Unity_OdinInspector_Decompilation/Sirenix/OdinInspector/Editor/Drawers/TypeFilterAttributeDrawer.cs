using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.TypeFilterAttribute" />.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 2002.0)]
	public sealed class TypeFilterAttributeDrawer : OdinAttributeDrawer<TypeFilterAttribute>
	{
		private string error;

		private bool useSpecialListBehaviour;

		private Func<IEnumerable<ValueDropdownItem>> getValues;

		private Func<IEnumerable<object>> getSelection;

		private IEnumerable<object> result;

		private Dictionary<object, string> nameLookup;

		private ValueResolver<object> rawGetter;

		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			return property.ValueEntry != null;
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			rawGetter = ValueResolver.Get<object>(base.Property, base.Attribute.FilterGetter);
			error = rawGetter.ErrorMessage;
			useSpecialListBehaviour = base.Property.ChildResolver is ICollectionResolver && !base.Attribute.DrawValueNormally;
			getSelection = () => base.Property.ValueEntry.WeakValues.Cast<object>();
			getValues = delegate
			{
				object value = rawGetter.GetValue();
				return (value != null) ? (from object x in rawGetter.GetValue() as IEnumerable
					where x != null
					select x).Select(delegate(object x)
				{
					if (x is ValueDropdownItem)
					{
						return (ValueDropdownItem)x;
					}
					if (x is IValueDropdownItem)
					{
						IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
						return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
					}
					return new ValueDropdownItem(null, x);
				}) : null;
			};
			ReloadDropdownCollections();
		}

		private void ReloadDropdownCollections()
		{
			if (error != null)
			{
				return;
			}
			object obj = null;
			object value = rawGetter.GetValue();
			if (value != null)
			{
				obj = (rawGetter.GetValue() as IEnumerable).Cast<object>().FirstOrDefault();
			}
			if (obj is IValueDropdownItem)
			{
				IEnumerable<ValueDropdownItem> enumerable = getValues();
				nameLookup = new Dictionary<object, string>(new IValueDropdownEqualityComparer(isTypeLookup: true));
				foreach (ValueDropdownItem item in enumerable)
				{
					nameLookup[item] = item.Text;
				}
			}
			else
			{
				nameLookup = null;
			}
		}

		private static IEnumerable<ValueDropdownItem> ToValueDropdowns(IEnumerable<object> query)
		{
			return query.Select(delegate(object x)
			{
				if (x is ValueDropdownItem)
				{
					return (ValueDropdownItem)x;
				}
				if (x is IValueDropdownItem)
				{
					IValueDropdownItem valueDropdownItem = x as IValueDropdownItem;
					return new ValueDropdownItem(valueDropdownItem.GetText(), valueDropdownItem.GetValue());
				}
				return new ValueDropdownItem(null, x);
			});
		}

		/// <summary>
		/// Draws the property with GUILayout support. This method is called by DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.Property.ValueEntry == null)
			{
				CallNextDrawer(label);
			}
			else if (error != null)
			{
				SirenixEditorGUI.ErrorMessageBox(error);
				CallNextDrawer(label);
			}
			else if (useSpecialListBehaviour)
			{
				CollectionDrawerStaticInfo.NextCustomAddFunction = OpenSelector;
				CallNextDrawer(label);
				if (result != null)
				{
					AddResult(result);
					result = null;
				}
				CollectionDrawerStaticInfo.NextCustomAddFunction = null;
			}
			else
			{
				DrawDropdown(label);
			}
		}

		private void AddResult(IEnumerable<object> query)
		{
			if (!query.Any())
			{
				return;
			}
			if (useSpecialListBehaviour)
			{
				ICollectionResolver collectionResolver = base.Property.ChildResolver as ICollectionResolver;
				foreach (object item in query)
				{
					object[] array = new object[base.Property.ParentValues.Count];
					for (int i = 0; i < array.Length; i++)
					{
						Type type = item as Type;
						if (type != null)
						{
							array[i] = CreateInstance(type);
						}
					}
					collectionResolver.QueueAdd(array);
				}
				return;
			}
			object obj = query.FirstOrDefault();
			Type type2 = obj as Type;
			for (int j = 0; j < base.Property.ValueEntry.WeakValues.Count; j++)
			{
				if (type2 != null)
				{
					base.Property.ValueEntry.WeakValues[j] = CreateInstance(type2);
				}
			}
		}

		private object CreateInstance(Type type)
		{
			if (base.Property.ValueEntry.SerializationBackend == SerializationBackend.Unity)
			{
				object obj = UnitySerializationUtility.CreateDefaultUnityInitializedObject(type);
				if (obj != null)
				{
					return obj;
				}
			}
			if (type == typeof(string))
			{
				return "";
			}
			if (type.IsAbstract || type.IsInterface)
			{
				Debug.LogError((object)("TypeFilter was asked to instantiate a value of type '" + type.GetNiceFullName() + "', but it is abstract or an interface and cannot be instantiated."));
				return null;
			}
			if (type.IsValueType || type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null)
			{
				return Activator.CreateInstance(type);
			}
			return FormatterServices.GetUninitializedObject(type);
		}

		private void DrawDropdown(GUIContent label)
		{
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			EditorGUI.BeginChangeCheck();
			IEnumerable<object> enumerable = null;
			string currentValueName = GetCurrentValueName();
			if (base.Attribute.DrawValueNormally)
			{
				enumerable = OdinSelector<object>.DrawSelectorDropdown(label, currentValueName, ShowSelector, null);
				CallNextDrawer(label);
			}
			else if (base.Property.Children.Count > 0)
			{
				base.Property.State.Expanded = SirenixEditorGUI.Foldout(base.Property.State.Expanded, label, out var valueRect);
				enumerable = OdinSelector<object>.DrawSelectorDropdown(valueRect, currentValueName, ShowSelector);
				if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded))
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					for (int i = 0; i < base.Property.Children.Count; i++)
					{
						InspectorProperty inspectorProperty = base.Property.Children[i];
						inspectorProperty.Draw(inspectorProperty.Label);
					}
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
				SirenixEditorGUI.EndFadeGroup();
			}
			else
			{
				enumerable = OdinSelector<object>.DrawSelectorDropdown(label, currentValueName, ShowSelector, null);
			}
			if (EditorGUI.EndChangeCheck() && enumerable != null)
			{
				AddResult(enumerable);
			}
		}

		private void OpenSelector()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			ReloadDropdownCollections();
			Rect rect = default(Rect);
			((Rect)(ref rect))._002Ector(Event.get_current().get_mousePosition(), Vector2.get_zero());
			OdinSelector<object> odinSelector = ShowSelector(rect);
			odinSelector.SelectionConfirmed += delegate(IEnumerable<object> x)
			{
				result = x;
			};
		}

		private OdinSelector<object> ShowSelector(Rect rect)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			GenericSelector<object> genericSelector = CreateSelector();
			((Rect)(ref rect)).set_x((float)(int)((Rect)(ref rect)).get_x());
			((Rect)(ref rect)).set_y((float)(int)((Rect)(ref rect)).get_y());
			((Rect)(ref rect)).set_width((float)(int)((Rect)(ref rect)).get_width());
			((Rect)(ref rect)).set_height((float)(int)((Rect)(ref rect)).get_height());
			genericSelector.ShowInPopup(rect, new Vector2(0f, 0f));
			return genericSelector;
		}

		private GenericSelector<object> CreateSelector()
		{
			IEnumerable<ValueDropdownItem> enumerable = getValues();
			if (enumerable == null)
			{
				enumerable = Enumerable.Empty<ValueDropdownItem>();
			}
			bool drawSearchToolbar = enumerable.Take(10).Count() == 10;
			GenericSelector<object> genericSelector = new GenericSelector<object>(base.Attribute.DropdownTitle, supportsMultiSelect: false, enumerable.Select((ValueDropdownItem x) => new GenericSelectorItem<object>(x.Text, x.Value)));
			genericSelector.CheckboxToggle = false;
			genericSelector.EnableSingleClickToSelect();
			genericSelector.SelectionTree.Config.DrawSearchToolbar = drawSearchToolbar;
			IEnumerable<object> source = Enumerable.Empty<object>();
			if (!useSpecialListBehaviour)
			{
				source = getSelection();
			}
			source = source.Select((Func<object, object>)((object x) => x?.GetType()));
			genericSelector.SetSelection(source);
			genericSelector.SelectionTree.EnumerateTree().AddThumbnailIcons(preferAssetPreviewAsIcon: true);
			return genericSelector;
		}

		private string GetCurrentValueName()
		{
			if (!EditorGUI.get_showMixedValue())
			{
				object obj = base.Property.ValueEntry.WeakSmartValue;
				string value = null;
				if (nameLookup != null && obj != null)
				{
					nameLookup.TryGetValue(obj, out value);
				}
				if (obj != null)
				{
					obj = obj.GetType();
				}
				return new GenericSelectorItem<object>(value, obj).GetNiceName();
			}
			return "—";
		}
	}
}
