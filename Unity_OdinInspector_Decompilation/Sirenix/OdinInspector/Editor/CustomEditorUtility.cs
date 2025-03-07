using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class CustomEditorUtility
	{
		private static Type CustomEditorAttributesType;

		private static Type MonoEditorType;

		private static readonly FieldInfo CustomEditorAttributesType_CachedEditorForType;

		private static readonly FieldInfo CustomEditorAttributesType_CachedMultiEditorForType;

		private static readonly FieldInfo CustomEditorAttributesType_CustomEditors;

		private static readonly FieldInfo CustomEditorAttributesType_CustomMultiEditors;

		private static readonly FieldInfo CustomEditorAttributesType_Initialized;

		private static readonly FieldInfo CustomEditor_EditorForChildClassesField;

		private static readonly FieldInfo MonoEditorType_InspectedType;

		private static readonly FieldInfo MonoEditorType_InspectorType;

		private static readonly FieldInfo MonoEditorType_EditorForChildClasses;

		private static readonly FieldInfo MonoEditorType_IsFallback;

		private static readonly MethodInfo CustomEditorAttributesType_Rebuild;

		private static readonly bool IsBackedByADictionary;

		private static bool IsValid;

		static CustomEditorUtility()
		{
			try
			{
				CustomEditorAttributesType = typeof(Editor).Assembly.GetType("UnityEditor.CustomEditorAttributes");
				MonoEditorType = CustomEditorAttributesType.GetNestedType("MonoEditorType", BindingFlags.Public | BindingFlags.NonPublic);
				CustomEditorAttributesType_Initialized = CustomEditorAttributesType.GetField("s_Initialized", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				CustomEditorAttributesType_CachedEditorForType = CustomEditorAttributesType.GetField("kCachedEditorForType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				CustomEditorAttributesType_CachedMultiEditorForType = CustomEditorAttributesType.GetField("kCachedMultiEditorForType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				CustomEditorAttributesType_CustomEditors = CustomEditorAttributesType.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				CustomEditorAttributesType_CustomMultiEditors = CustomEditorAttributesType.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				CustomEditorAttributesType_Rebuild = CustomEditorAttributesType.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				MonoEditorType_InspectedType = MonoEditorType.GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				MonoEditorType_InspectorType = MonoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				MonoEditorType_EditorForChildClasses = MonoEditorType.GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				MonoEditorType_IsFallback = MonoEditorType.GetField("m_IsFallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				CustomEditor_EditorForChildClassesField = typeof(CustomEditor).GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (CustomEditorAttributesType_Initialized == null || CustomEditorAttributesType_CustomEditors == null || CustomEditorAttributesType_CustomMultiEditors == null || MonoEditorType_InspectedType == null || MonoEditorType_InspectorType == null || MonoEditorType_EditorForChildClasses == null || MonoEditorType_IsFallback == null || CustomEditor_EditorForChildClassesField == null)
				{
					throw new NullReferenceException();
				}
				IsBackedByADictionary = typeof(IDictionary).IsAssignableFrom(CustomEditorAttributesType_CustomEditors.FieldType);
				IsValid = true;
			}
			catch (NullReferenceException)
			{
				IsValid = false;
				Debug.LogError((object)("Unity's internal custom editor management classes have changed in this version of Unity (" + Application.get_unityVersion() + "). Odin may not be able to draw editors."));
			}
		}

		public static void ResetCustomEditors()
		{
			if (!IsValid)
			{
				return;
			}
			if (IsBackedByADictionary)
			{
				((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
				((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
			}
			else
			{
				if (CustomEditorAttributesType_CachedEditorForType != null)
				{
					((Dictionary<Type, Type>)CustomEditorAttributesType_CachedEditorForType.GetValue(null)).Clear();
				}
				if (CustomEditorAttributesType_CachedMultiEditorForType != null)
				{
					((Dictionary<Type, Type>)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null)).Clear();
				}
				((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Clear();
				((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Clear();
			}
			if (UnityVersion.IsVersionOrGreater(2019, 1))
			{
				CustomEditorAttributesType_Rebuild.Invoke(null, null);
				CustomEditorAttributesType_Initialized.SetValue(null, true);
			}
			else
			{
				CustomEditorAttributesType_Initialized.SetValue(null, false);
			}
		}

		public static void SetCustomEditor(Type inspectedType, Type editorType)
		{
			if (IsValid)
			{
				CustomEditor customAttribute = editorType.GetCustomAttribute<CustomEditor>();
				if (customAttribute == null)
				{
					throw new ArgumentException("Editor type to set '" + editorType.GetNiceName() + "' has no CustomEditor attribute applied! Use a SetCustomEditor overload that takes isFallbackEditor and isEditorForChildClasses parameters.");
				}
				SetCustomEditor(inspectedType, editorType, customAttribute.get_isFallback(), (bool)CustomEditor_EditorForChildClassesField.GetValue(customAttribute));
			}
		}

		public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses)
		{
			if (IsValid)
			{
				SetCustomEditor(inspectedType, editorType, isFallbackEditor, isEditorForChildClasses, editorType.IsDefined<CanEditMultipleObjects>());
			}
		}

		public static void SetCustomEditor(Type inspectedType, Type editorType, bool isFallbackEditor, bool isEditorForChildClasses, bool isMultiEditor)
		{
			if (!IsValid)
			{
				return;
			}
			object obj = Activator.CreateInstance(MonoEditorType);
			MonoEditorType_InspectedType.SetValue(obj, inspectedType);
			MonoEditorType_InspectorType.SetValue(obj, editorType);
			MonoEditorType_IsFallback.SetValue(obj, isFallbackEditor);
			MonoEditorType_EditorForChildClasses.SetValue(obj, isEditorForChildClasses);
			if (IsBackedByADictionary)
			{
				AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomEditors.GetValue(null), obj, inspectedType);
				if (isMultiEditor)
				{
					AddEntryToDictList((IDictionary)CustomEditorAttributesType_CustomMultiEditors.GetValue(null), obj, inspectedType);
				}
				return;
			}
			if (CustomEditorAttributesType_CachedEditorForType != null && CustomEditorAttributesType_CachedMultiEditorForType != null)
			{
				((IDictionary)CustomEditorAttributesType_CachedEditorForType.GetValue(null))[inspectedType] = editorType;
				if (isMultiEditor)
				{
					((IDictionary)CustomEditorAttributesType_CachedMultiEditorForType.GetValue(null))[inspectedType] = editorType;
				}
			}
			((IList)CustomEditorAttributesType_CustomEditors.GetValue(null)).Insert(0, obj);
			if (isMultiEditor)
			{
				((IList)CustomEditorAttributesType_CustomMultiEditors.GetValue(null)).Insert(0, obj);
			}
		}

		private static void AddEntryToDictList(IDictionary dict, object entry, Type inspectedType)
		{
			IList list = (IList)(dict.Contains(inspectedType) ? ((IList)dict[inspectedType]) : (dict[inspectedType] = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(MonoEditorType))));
			list.Insert(0, entry);
		}
	}
}
