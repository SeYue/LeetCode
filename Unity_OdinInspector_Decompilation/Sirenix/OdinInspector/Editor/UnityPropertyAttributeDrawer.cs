using System;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Unity property attribute drawer.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 999.5)]
	[OdinDontRegister]
	public sealed class UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint> : OdinAttributeDrawer<TAttribute> where TDrawer : PropertyDrawer, new()where TAttribute : TAttributeConstraint where TAttributeConstraint : PropertyAttribute
	{
		private static readonly FieldInfo InternalAttributeFieldInfo;

		private static readonly FieldInfo InternalFieldInfoFieldInfo;

		private static readonly ValueSetter<TDrawer, Attribute> SetAttribute;

		private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

		private TDrawer drawer;

		private object propertyHandler;

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		public UnityPropertyAttributeDrawer()
		{
			drawer = new TDrawer();
			if (UnityPropertyHandlerUtility.IsAvailable)
			{
				propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler((PropertyDrawer)(object)drawer);
			}
		}

		static UnityPropertyAttributeDrawer()
		{
			InternalAttributeFieldInfo = typeof(TDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalAttributeFieldInfo == null)
			{
				Debug.LogError((object)("Could not find the internal Unity field 'PropertyDrawer.m_Attribute'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled."));
			}
			else
			{
				SetAttribute = EmitUtilities.CreateInstanceFieldSetter<TDrawer, Attribute>(InternalAttributeFieldInfo);
			}
			if (InternalFieldInfoFieldInfo == null)
			{
				Debug.LogError((object)("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled."));
			}
			else
			{
				SetFieldInfo = EmitUtilities.CreateInstanceFieldSetter<TDrawer, FieldInfo>(InternalFieldInfoFieldInfo);
			}
		}

		protected override void Initialize()
		{
			if (base.Property.ChildResolver is ICollectionResolver)
			{
				base.SkipWhenDrawing = true;
			}
		}

		/// <summary>
		/// Draws the proprety.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			InspectorProperty inspectorProperty = base.Property;
			IPropertyValueEntry valueEntry = inspectorProperty.ValueEntry;
			if (SetAttribute == null || SetFieldInfo == null)
			{
				SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity fields 'PropertyDrawer.m_Attribute' or 'PropertyDrawer.m_FieldInfo'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
				return;
			}
			if (valueEntry == null)
			{
				SirenixEditorGUI.ErrorMessageBox(string.Concat("Cannot put the attribute '", typeof(TAttribute), "' on a property of type '", inspectorProperty.Info.PropertyType, "'."));
				return;
			}
			FieldInfo backingField;
			SerializedProperty val = inspectorProperty.Tree.GetUnityPropertyForPath(inspectorProperty.Path, out backingField);
			if (val == null)
			{
				if (UnityVersion.IsVersionOrGreater(2017, 1))
				{
					CallNextDrawer(label);
					return;
				}
				SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + inspectorProperty.NiceName + "' of type '" + valueEntry.TypeOfValue.GetNiceName() + "' at path '" + valueEntry.Property.Path + "'.");
				return;
			}
			SetFieldInfo(ref drawer, backingField);
			SetAttribute(ref drawer, (Attribute)(object)base.Attribute);
			label = label ?? GUIContent.none;
			if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				Object[] targetObjects = val.get_serializedObject().get_targetObjects();
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject emittedScriptableObject = (EmittedScriptableObject)(object)targetObjects[i];
					emittedScriptableObject.SetWeakValue(valueEntry.WeakValues[i]);
				}
				val.get_serializedObject().Update();
				val = val.get_serializedObject().FindProperty(val.get_propertyPath());
			}
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
			bool flag = EditorGUI.EndChangeCheck();
			if (val.get_serializedObject().get_targetObject() is EmittedScriptableObject)
			{
				if (val.get_serializedObject().ApplyModifiedPropertiesWithoutUndo() || flag)
				{
					ApplyValueWeak(valueEntry, val);
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
					for (int j = 0; j < baseValueEntry.ValueCount; j++)
					{
						baseValueEntry.TriggerOnValueChanged(j);
					}
				});
			}
		}

		private static void ApplyValueWeak(IPropertyValueEntry entry, SerializedProperty unityProperty)
		{
			Object[] targetObjects = unityProperty.get_serializedObject().get_targetObjects();
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject emittedScriptableObject = (EmittedScriptableObject)(object)targetObjects[i];
				entry.WeakValues[i] = emittedScriptableObject.GetWeakValue();
			}
			entry.WeakValues.ForceMarkDirty();
		}
	}
}
