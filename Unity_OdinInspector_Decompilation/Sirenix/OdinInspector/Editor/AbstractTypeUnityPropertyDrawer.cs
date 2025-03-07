using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Unity property drawer for abstract types.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.5)]
	[OdinDontRegister]
	public sealed class AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T> : OdinValueDrawer<T> where TDrawer : PropertyDrawer, new()where T : TDrawnType
	{
		private static readonly FieldInfo InternalFieldInfoFieldInfo;

		private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

		private TDrawer drawer;

		private object propertyHandler;

		/// <summary>
		/// Initializes the property drawer.
		/// </summary>
		public AbstractTypeUnityPropertyDrawer()
		{
			drawer = new TDrawer();
			if (UnityPropertyHandlerUtility.IsAvailable)
			{
				propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler((PropertyDrawer)(object)drawer);
			}
		}

		static AbstractTypeUnityPropertyDrawer()
		{
			InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalFieldInfoFieldInfo == null)
			{
				Debug.LogError((object)("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled."));
			}
			else
			{
				SetFieldInfo = EmitUtilities.CreateInstanceFieldSetter<TDrawer, FieldInfo>(InternalFieldInfoFieldInfo);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_021b: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if (SetFieldInfo == null)
			{
				SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled.");
				return;
			}
			FieldInfo backingField;
			SerializedProperty val = propertyValueEntry.Property.Tree.GetUnityPropertyForPath(propertyValueEntry.Property.Path, out backingField);
			if (val == null)
			{
				SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + propertyValueEntry.Property.NiceName + "' of type '" + propertyValueEntry.TypeOfValue.GetNiceName() + "' at path '" + propertyValueEntry.Property.Path + "'. Legacy Unity drawing compatibility is broken for this property; falling back to normal Odin drawing. Please report an issue on Odin's issue tracker with details.");
				CallNextDrawer(label);
				return;
			}
			label = label ?? GUIContent.none;
			SetFieldInfo(ref drawer, backingField);
			if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject<T>)
			{
				Object[] targetObjects = val.get_serializedObject().get_targetObjects();
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject<T> emittedScriptableObject = (EmittedScriptableObject<T>)(object)targetObjects[i];
					emittedScriptableObject.SetValue(propertyValueEntry.Values[i]);
				}
				val.get_serializedObject().Update();
			}
			else if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				Object[] targetObjects2 = val.get_serializedObject().get_targetObjects();
				for (int j = 0; j < targetObjects2.Length; j++)
				{
					EmittedScriptableObject emittedScriptableObject2 = (EmittedScriptableObject)(object)targetObjects2[j];
					emittedScriptableObject2.SetWeakValue(propertyValueEntry.Values[j]);
				}
				val.get_serializedObject().Update();
				val = val.get_serializedObject().FindProperty(val.get_propertyPath());
			}
			try
			{
				float num = ((propertyHandler == null) ? ((PropertyDrawer)drawer).GetPropertyHeight(val.Copy(), label) : UnityPropertyHandlerUtility.PropertyHandlerGetHeight(propertyHandler, val.Copy(), label, includeChildren: false));
				Rect controlRect = EditorGUILayout.GetControlRect(false, num, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				EditorGUI.BeginChangeCheck();
				if (propertyHandler != null)
				{
					UnityPropertyHandlerUtility.PropertyHandlerOnGUI(propertyHandler, controlRect, val, label, includeChildren: false);
				}
				else
				{
					((PropertyDrawer)drawer).OnGUI(controlRect, val, label);
				}
			}
			finally
			{
				if (label == GUIContent.none && label.get_text() != "")
				{
					label.set_text("");
				}
			}
			if (label == GUIContent.none && label.get_text() != "")
			{
				label.set_text("");
			}
			bool flag = EditorGUI.EndChangeCheck();
			if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject<T>)
			{
				if (val.get_serializedObject().ApplyModifiedPropertiesWithoutUndo() || flag)
				{
					ApplyValueStrong(propertyValueEntry, val);
				}
			}
			else if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				if (val.get_serializedObject().ApplyModifiedPropertiesWithoutUndo() || flag)
				{
					ApplyValueWeak(propertyValueEntry, val);
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

		private static void ApplyValueWeak(IPropertyValueEntry<T> entry, SerializedProperty unityProperty)
		{
			Object[] targetObjects = unityProperty.get_serializedObject().get_targetObjects();
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject emittedScriptableObject = (EmittedScriptableObject)(object)targetObjects[i];
				entry.Values[i] = (T)emittedScriptableObject.GetWeakValue();
			}
			entry.Values.ForceMarkDirty();
		}

		private static void ApplyValueStrong(IPropertyValueEntry<T> entry, SerializedProperty unityProperty)
		{
			Object[] targetObjects = unityProperty.get_serializedObject().get_targetObjects();
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject<T> emittedScriptableObject = (EmittedScriptableObject<T>)(object)targetObjects[i];
				entry.Values[i] = emittedScriptableObject.GetValue();
			}
			entry.Values.ForceMarkDirty();
		}
	}
}
