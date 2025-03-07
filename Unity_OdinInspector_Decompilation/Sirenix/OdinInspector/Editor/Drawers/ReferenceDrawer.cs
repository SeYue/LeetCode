using System;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all reference type properties, which has already been drawn elsewhere. This drawer adds an additional foldout to prevent infinite draw depth.
	/// </summary>
	[AllowGUIEnabledForReadonly]
	[DrawerPriority(90.0, 0.0, 0.0)]
	public sealed class ReferenceDrawer<T> : OdinValueDrawer<T> where T : class
	{
		private LocalPersistentContext<bool> isToggled;

		private InspectorProperty referencedProperty;

		private bool hideReferenceBox;

		private string error;

		/// <summary>
		/// Prevents the drawer from being applied to UnityEngine.Object references since they are shown as an object field, and is not drawn in-line.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (!typeof(MemberInfo).IsAssignableFrom(type))
			{
				return !typeof(Object).IsAssignableFrom(type);
			}
			return false;
		}

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (base.CanDrawValueProperty(property))
			{
				return !property.Attributes.HasAttribute<DoNotDrawAsReferenceAttribute>();
			}
			return false;
		}

		protected override void Initialize()
		{
			isToggled = this.GetPersistentValue("is_Toggled", defaultValue: false);
			hideReferenceBox = base.Property.Attributes.HasAttribute<HideDuplicateReferenceBoxAttribute>();
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if ((int)Event.get_current().get_type() == 8)
			{
				if (propertyValueEntry.ValueState == PropertyValueState.Reference)
				{
					referencedProperty = propertyValueEntry.Property.Tree.GetPropertyAtPath(propertyValueEntry.TargetReferencePath);
					if (referencedProperty == null)
					{
						error = "Reference to " + propertyValueEntry.TargetReferencePath + ". But no property was found at path, which is a problem.";
					}
					else
					{
						error = null;
					}
				}
				else
				{
					error = null;
					referencedProperty = null;
				}
			}
			if (error != null)
			{
				SirenixEditorGUI.ErrorMessageBox(error);
			}
			if (referencedProperty != null)
			{
				PropertyContext<bool> global = referencedProperty.Context.GetGlobal("is_in_reference", defaultValue: false);
				bool flag = true;
				if (!global.Value)
				{
					flag = !hideReferenceBox;
				}
				if (flag)
				{
					SirenixEditorGUI.BeginToolbarBox();
					SirenixEditorGUI.BeginToolbarBoxHeader();
					isToggled.Value = SirenixEditorGUI.Foldout(isToggled.Value, label, out var valueRect);
					GUI.Label(valueRect, "Reference to " + referencedProperty.Path, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
					SirenixEditorGUI.EndToolbarBoxHeader();
					if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(base.Property, this), isToggled.Value))
					{
						bool value = global.Value;
						global.Value = true;
						referencedProperty.Draw(label);
						global.Value = value;
					}
					SirenixEditorGUI.EndFadeGroup();
					SirenixEditorGUI.EndToolbarBox();
				}
				else
				{
					bool value2 = global.Value;
					global.Value = true;
					referencedProperty.Draw(label);
					global.Value = value2;
				}
			}
			else
			{
				CallNextDrawer(label);
			}
		}
	}
}
