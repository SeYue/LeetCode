using System;
using System.Collections;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all nullable reference types, with an object field.
	/// </summary>
	[AllowGUIEnabledForReadonly]
	[DrawerPriority(0.0, 0.0, 2000.0)]
	public sealed class NullableReferenceDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
	{
		private bool shouldDrawReferencePicker;

		private bool drawUnityObject;

		private bool allowSceneObjects;

		private OdinDrawer[] bakedDrawerArray;

		private InlinePropertyAttribute inlinePropertyAttr;

		private bool drawChildren;

		private PropertySearchFilter searchFilter;

		private string searchFieldControlName;

		protected override void Initialize()
		{
			SearchableAttribute attribute = base.Property.GetAttribute<SearchableAttribute>();
			if (attribute != null)
			{
				searchFilter = new PropertySearchFilter(base.Property, attribute);
				searchFieldControlName = "PropertyTreeSearchField_" + Guid.NewGuid().ToString();
			}
			drawUnityObject = typeof(Object).IsAssignableFrom(base.ValueEntry.TypeOfValue);
			allowSceneObjects = base.Property.GetAttribute<AssetsOnlyAttribute>() == null;
			bakedDrawerArray = base.Property.GetActiveDrawerChain().BakedDrawerArray;
			inlinePropertyAttr = base.Property.Attributes.GetAttribute<InlinePropertyAttribute>();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Invalid comparison between Unknown and I4
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if ((int)Event.get_current().get_type() == 8)
			{
				shouldDrawReferencePicker = ShouldDrawReferenceObjectPicker(base.ValueEntry);
				if (base.Property.Children.Count > 0)
				{
					drawChildren = true;
				}
				else if (base.ValueEntry.ValueState != 0)
				{
					drawChildren = false;
				}
				else
				{
					drawChildren = bakedDrawerArray[bakedDrawerArray.Length - 2] != this;
				}
			}
			if (entry.ValueState == PropertyValueState.NullReference)
			{
				if (drawUnityObject)
				{
					CallNextDrawer(label);
				}
				else
				{
					if (!entry.SerializationBackend.SupportsPolymorphism && entry.IsEditable)
					{
						SirenixEditorGUI.ErrorMessageBox("Unity-backed value is null. This should already be fixed by the FixUnityNullDrawer! It is likely that this type has been incorrectly guessed by Odin to be serialized by Unity when it is actually not. Please create an issue on Odin's issue tracker stating how to reproduce this error message.");
					}
					DrawField(label);
				}
			}
			else if (shouldDrawReferencePicker)
			{
				DrawField(label);
			}
			else
			{
				CallNextDrawer(label);
			}
			ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
			if (!objectPicker.IsReadyToClaim)
			{
				return;
			}
			object obj = objectPicker.ClaimObject();
			entry.Property.Tree.DelayActionUntilRepaint(delegate
			{
				entry.WeakValues[0] = obj;
				for (int i = 1; i < entry.ValueCount; i++)
				{
					entry.WeakValues[i] = SerializationUtility.CreateCopy(obj);
				}
			});
		}

		private void DrawField(GUIContent label)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			if (inlinePropertyAttr != null)
			{
				bool flag = inlinePropertyAttr.LabelWidth > 0;
				if (label == null)
				{
					if (flag)
					{
						GUIHelper.PushLabelWidth(inlinePropertyAttr.LabelWidth);
					}
					DrawInlinePropertyReferencePicker();
					if (drawChildren)
					{
						CallNextDrawer(null);
					}
					if (flag)
					{
						GUIHelper.PopLabelWidth();
					}
					return;
				}
				SirenixEditorGUI.BeginVerticalPropertyLayout(label);
				DrawInlinePropertyReferencePicker();
				if (flag)
				{
					GUIHelper.PushLabelWidth(inlinePropertyAttr.LabelWidth);
				}
				if (drawChildren)
				{
					CallNextDrawer(null);
				}
				if (flag)
				{
					GUIHelper.PopLabelWidth();
				}
				if (UnityVersion.IsVersionOrGreater(2019, 3))
				{
					GUILayout.Space(5f);
				}
				else
				{
					GUILayout.Space(4f);
				}
				SirenixEditorGUI.EndVerticalPropertyLayout();
				return;
			}
			int controlId;
			bool hasKeyboardFocus;
			Rect valueRect;
			Rect val = SirenixEditorGUI.GetFeatureRichControlRect(null, out controlId, out hasKeyboardFocus, out valueRect);
			if (label != null)
			{
				((Rect)(ref val)).set_width(GUIHelper.BetterLabelWidth);
				((Rect)(ref valueRect)).set_xMin(((Rect)(ref val)).get_xMax());
				DrawSearchFilter(val);
				if (drawChildren)
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(val, base.Property.State.Expanded, label);
				}
				else
				{
					val = EditorGUI.IndentedRect(val);
					GUI.Label(val, label);
				}
			}
			else if (drawChildren)
			{
				if (EditorGUIUtility.get_hierarchyMode())
				{
					((Rect)(ref val)).set_width(18f);
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(val, base.Property.State.Expanded, GUIContent.none);
				}
				else
				{
					((Rect)(ref val)).set_width(18f);
					((Rect)(ref valueRect)).set_xMin(((Rect)(ref val)).get_xMax());
					bool hierarchyMode = EditorGUIUtility.get_hierarchyMode();
					EditorGUIUtility.set_hierarchyMode(false);
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(val, base.Property.State.Expanded, GUIContent.none);
					EditorGUIUtility.set_hierarchyMode(hierarchyMode);
				}
			}
			EditorGUI.BeginChangeCheck();
			bool showMixedValue = EditorGUI.get_showMixedValue();
			if (base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				EditorGUI.set_showMixedValue(true);
			}
			object newValue = SirenixEditorFields.PolymorphicObjectField(valueRect, base.ValueEntry.WeakSmartValue, base.ValueEntry.BaseValueType, allowSceneObjects, hasKeyboardFocus, controlId);
			EditorGUI.set_showMixedValue(showMixedValue);
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
				{
					base.ValueEntry.WeakValues[0] = newValue;
					for (int i = 1; i < base.ValueEntry.ValueCount; i++)
					{
						base.ValueEntry.WeakValues[i] = SerializationUtility.CreateCopy(newValue);
					}
				});
			}
			if (!drawChildren)
			{
				return;
			}
			bool isVisible = base.ValueEntry.ValueState != PropertyValueState.NullReference && base.Property.State.Expanded;
			if (SirenixEditorGUI.BeginFadeGroup(this, isVisible))
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				if (searchFilter != null && searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
				}
				else
				{
					CallNextDrawer(null);
				}
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		private void DrawSearchFilter(Rect labelRect)
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			if (searchFilter == null)
			{
				return;
			}
			Rect rect = default(Rect);
			((Rect)(ref rect))._002Ector(((Rect)(ref labelRect)).get_xMin() + ((Rect)(ref labelRect)).get_width() * 0.5f, ((Rect)(ref labelRect)).get_y() + 1f, ((Rect)(ref labelRect)).get_width() * 0.5f - 5f, ((Rect)(ref labelRect)).get_height());
			string newTerm = SirenixEditorGUI.SearchField(rect, searchFilter.SearchTerm, forceFocus: false, searchFieldControlName);
			if (!(newTerm != searchFilter.SearchTerm))
			{
				return;
			}
			searchFilter.SearchTerm = newTerm;
			base.Property.Tree.DelayActionUntilRepaint(delegate
			{
				if (!string.IsNullOrEmpty(newTerm))
				{
					base.Property.State.Expanded = true;
				}
				searchFilter.UpdateSearch();
				GUIHelper.RequestRepaint();
			});
		}

		private void DrawInlinePropertyReferencePicker()
		{
			EditorGUI.BeginChangeCheck();
			bool showMixedValue = EditorGUI.get_showMixedValue();
			if (base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				EditorGUI.set_showMixedValue(true);
			}
			object newValue = SirenixEditorFields.PolymorphicObjectField(base.ValueEntry.WeakSmartValue, base.ValueEntry.BaseValueType, allowSceneObjects);
			EditorGUI.set_showMixedValue(showMixedValue);
			if (!EditorGUI.EndChangeCheck())
			{
				return;
			}
			base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
			{
				base.ValueEntry.WeakValues[0] = newValue;
				for (int i = 1; i < base.ValueEntry.ValueCount; i++)
				{
					base.ValueEntry.WeakValues[i] = SerializationUtility.CreateCopy(newValue);
				}
			});
		}

		private static bool ShouldDrawReferenceObjectPicker(IPropertyValueEntry<T> entry)
		{
			if (entry.SerializationBackend.SupportsPolymorphism && !entry.BaseValueType.IsValueType && entry.BaseValueType != typeof(string) && !(entry.Property.ChildResolver is ICollectionResolver) && !entry.BaseValueType.IsArray && entry.IsEditable && (!typeof(Object).IsAssignableFrom(entry.TypeOfValue) || entry.BaseValueType.IsInterface) && !entry.BaseValueType.InheritsFrom(typeof(IDictionary)) && !Object.op_Implicit(entry.WeakSmartValue as Object))
			{
				return entry.Property.GetAttribute<HideReferenceObjectPickerAttribute>() == null;
			}
			return false;
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Expected O, but got Unknown
			//IL_0105: Expected O, but got Unknown
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Expected O, but got Unknown
			IPropertyValueEntry<T> entry = property.ValueEntry as IPropertyValueEntry<T>;
			if (!property.ValueEntry.SerializationBackend.SupportsPolymorphism || entry.BaseValueType.IsValueType || entry.BaseValueType == typeof(string))
			{
				return;
			}
			if (entry.IsEditable)
			{
				ObjectPicker objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
				Rect rect = entry.Property.LastDrawnValueRect;
				((Rect)(ref rect)).set_position(GUIUtility.GUIToScreenPoint(((Rect)(ref rect)).get_position()));
				((Rect)(ref rect)).set_height(20f);
				genericMenu.AddItem(new GUIContent("Change Type"), false, (MenuFunction)delegate
				{
					//IL_0018: Unknown result type (might be due to invalid IL or missing references)
					objectPicker.ShowObjectPicker(entry.WeakSmartValue, allowSceneObjects: false, rect);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Change Type"));
			}
		}

		/// <summary>
		/// Returns a value that indicates if this drawer can be used for the given property.
		/// </summary>
		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (property.IsTreeRoot)
			{
				return false;
			}
			Type baseValueType = property.ValueEntry.BaseValueType;
			if ((baseValueType.IsClass || baseValueType.IsInterface) && baseValueType != typeof(string))
			{
				return !typeof(Object).IsAssignableFrom(baseValueType);
			}
			return false;
		}
	}
}
