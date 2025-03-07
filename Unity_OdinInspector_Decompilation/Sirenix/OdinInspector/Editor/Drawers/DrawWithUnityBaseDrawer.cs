using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Base class to derive from for value drawers that merely wish to cause a value to be drawn by Unity.
	/// </summary>
	public abstract class DrawWithUnityBaseDrawer<T> : OdinValueDrawer<T>
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
			bool flag = val.get_serializedObject().get_targetObject() is EmittedScriptableObject<T>;
			if (flag)
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
			if (label == null)
			{
				label = GUIContent.none;
			}
			if (!flag)
			{
				EditorGUI.BeginChangeCheck();
			}
			EditorGUILayout.PropertyField(val, label, true, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			if (!flag && EditorGUI.EndChangeCheck())
			{
				propertyValueEntry.Values.ForceMarkDirty();
			}
			if (flag)
			{
				val.get_serializedObject().ApplyModifiedPropertiesWithoutUndo();
				Object[] targetObjects2 = val.get_serializedObject().get_targetObjects();
				for (int j = 0; j < targetObjects2.Length; j++)
				{
					EmittedScriptableObject<T> emittedScriptableObject2 = (EmittedScriptableObject<T>)(object)targetObjects2[j];
					propertyValueEntry.Values[j] = emittedScriptableObject2.GetValue();
				}
			}
		}
	}
}
