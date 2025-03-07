using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.DrawWithUnityAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RequireComponent" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />
	/// <seealso cref="T:UnityEngine.HideInInspector" />
	[DrawerPriority(0.0, 0.0, 6000.0)]
	public class DrawWithUnityAttributeDrawer<T> : OdinAttributeDrawer<DrawWithUnityAttribute, T>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			FieldInfo backingField;
			SerializedProperty val = propertyValueEntry.Property.Tree.GetUnityPropertyForPath(propertyValueEntry.Property.Path, out backingField);
			if (val == null)
			{
				SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + propertyValueEntry.Property.NiceName + "' of type '" + propertyValueEntry.TypeOfValue.GetNiceName() + "' at path '" + propertyValueEntry.Property.Path + "'.");
				return;
			}
			if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject<T>)
			{
				Object[] targetObjects = val.get_serializedObject().get_targetObjects();
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject<T> emittedScriptableObject = (EmittedScriptableObject<T>)(object)targetObjects[i];
					emittedScriptableObject.SetValue(propertyValueEntry.Values[i]);
				}
				val.get_serializedObject().Update();
				val = val.get_serializedObject().FindProperty(val.get_propertyPath());
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(val, label ?? GUIContent.none, true, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			bool flag = EditorGUI.EndChangeCheck();
			if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject<T>)
			{
				val.get_serializedObject().ApplyModifiedPropertiesWithoutUndo();
				Object[] targetObjects2 = val.get_serializedObject().get_targetObjects();
				for (int j = 0; j < targetObjects2.Length; j++)
				{
					EmittedScriptableObject<T> emittedScriptableObject2 = (EmittedScriptableObject<T>)(object)targetObjects2[j];
					propertyValueEntry.Values[j] = emittedScriptableObject2.GetValue();
				}
				if (flag)
				{
					propertyValueEntry.Values.ForceMarkDirty();
				}
			}
			else
			{
				if (!flag)
				{
					return;
				}
				base.Property.Tree.DelayActionUntilRepaint(delegate
				{
					PropertyValueEntry baseValueEntry = base.Property.BaseValueEntry;
					for (int k = 0; k < baseValueEntry.ValueCount; k++)
					{
						baseValueEntry.TriggerOnValueChanged(k);
					}
				});
			}
		}
	}
}
