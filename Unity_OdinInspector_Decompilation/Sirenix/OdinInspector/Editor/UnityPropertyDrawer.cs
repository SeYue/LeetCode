using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Unity property drawer.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.5)]
	[OdinDontRegister]
	public class UnityPropertyDrawer<TDrawer, TDrawnType> : OdinValueDrawer<TDrawnType> where TDrawer : PropertyDrawer, new()
	{
		private static readonly FieldInfo InternalFieldInfoFieldInfo;

		private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

		protected TDrawer drawer;

		protected object propertyHandler;

		protected bool delayApplyValueUntilRepaint;

		/// <summary>
		/// Initializes the property drawer.
		/// </summary>
		public UnityPropertyDrawer()
		{
			drawer = new TDrawer();
			if (UnityPropertyHandlerUtility.IsAvailable)
			{
				propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler((PropertyDrawer)(object)drawer);
			}
		}

		static UnityPropertyDrawer()
		{
			InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalFieldInfoFieldInfo == null)
			{
				Debug.LogError((object)("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(UnityPropertyDrawer<TDrawer, TDrawnType>).GetNiceName() + "' has been disabled."));
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
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<TDrawnType> entry = base.ValueEntry;
			if (SetFieldInfo == null)
			{
				SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(UnityPropertyDrawer<TDrawer, TDrawnType>).GetNiceName() + "' has been disabled.");
				return;
			}
			FieldInfo backingField;
			SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out backingField);
			if (unityProperty == null)
			{
				if (UnityVersion.IsVersionOrGreater(2017, 1))
				{
					CallNextDrawer(label);
					return;
				}
				SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
				return;
			}
			label = label ?? GUIContent.none;
			SetFieldInfo(ref drawer, backingField);
			if (unityProperty.get_serializedObject().get_targetObject() is EmittedScriptableObject<TDrawnType>)
			{
				Object[] targetObjects = unityProperty.get_serializedObject().get_targetObjects();
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject<TDrawnType> emittedScriptableObject = (EmittedScriptableObject<TDrawnType>)(object)targetObjects[i];
					emittedScriptableObject.SetValue(entry.Values[i]);
				}
				unityProperty.get_serializedObject().Update();
			}
			else if (unityProperty.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				Object[] targetObjects2 = unityProperty.get_serializedObject().get_targetObjects();
				for (int j = 0; j < targetObjects2.Length; j++)
				{
					EmittedScriptableObject emittedScriptableObject2 = (EmittedScriptableObject)(object)targetObjects2[j];
					emittedScriptableObject2.SetWeakValue(entry.Values[j]);
				}
				unityProperty.get_serializedObject().Update();
				unityProperty = unityProperty.get_serializedObject().FindProperty(unityProperty.get_propertyPath());
			}
			float num = ((propertyHandler == null) ? ((PropertyDrawer)drawer).GetPropertyHeight(unityProperty.Copy(), label) : UnityPropertyHandlerUtility.PropertyHandlerGetHeight(propertyHandler, unityProperty.Copy(), label, includeChildren: false));
			Rect controlRect = EditorGUILayout.GetControlRect(false, num, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			EditorGUI.BeginChangeCheck();
			if (propertyHandler != null)
			{
				UnityPropertyHandlerUtility.PropertyHandlerOnGUI(propertyHandler, controlRect, unityProperty, label, includeChildren: false);
			}
			else
			{
				((PropertyDrawer)drawer).OnGUI(controlRect, unityProperty, label);
			}
			bool flag = EditorGUI.EndChangeCheck();
			if (unityProperty.get_serializedObject().get_targetObject() is EmittedScriptableObject<TDrawnType>)
			{
				if (!(unityProperty.get_serializedObject().ApplyModifiedPropertiesWithoutUndo() || flag))
				{
					return;
				}
				if (delayApplyValueUntilRepaint)
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						ApplyValueStrong(entry, unityProperty);
					});
				}
				else
				{
					ApplyValueStrong(entry, unityProperty);
				}
			}
			else if (unityProperty.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				if (!(unityProperty.get_serializedObject().ApplyModifiedPropertiesWithoutUndo() || flag))
				{
					return;
				}
				if (delayApplyValueUntilRepaint)
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						ApplyValueWeak(entry, unityProperty);
					});
				}
				else
				{
					ApplyValueWeak(entry, unityProperty);
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

		private static void ApplyValueWeak(IPropertyValueEntry<TDrawnType> entry, SerializedProperty unityProperty)
		{
			Object[] targetObjects = unityProperty.get_serializedObject().get_targetObjects();
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject emittedScriptableObject = (EmittedScriptableObject)(object)targetObjects[i];
				entry.Values[i] = (TDrawnType)emittedScriptableObject.GetWeakValue();
			}
			entry.Values.ForceMarkDirty();
		}

		private static void ApplyValueStrong(IPropertyValueEntry<TDrawnType> entry, SerializedProperty unityProperty)
		{
			Object[] targetObjects = unityProperty.get_serializedObject().get_targetObjects();
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject<TDrawnType> emittedScriptableObject = (EmittedScriptableObject<TDrawnType>)(object)targetObjects[i];
				entry.Values[i] = emittedScriptableObject.GetValue();
			}
			entry.Values.ForceMarkDirty();
		}
	}
}
