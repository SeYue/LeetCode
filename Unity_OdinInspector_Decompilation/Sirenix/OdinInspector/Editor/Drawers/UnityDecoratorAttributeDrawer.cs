using System;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all Unity DecoratorDrawers within prepend attribute drawers within Odin.
	/// </summary>
	[DrawerPriority(0.0, 1.0, 0.0)]
	[OdinDontRegister]
	public sealed class UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint> : OdinAttributeDrawer<TAttribute> where TDrawer : DecoratorDrawer, new()where TAttribute : TAttributeConstraint where TAttributeConstraint : PropertyAttribute
	{
		private static readonly FieldInfo InternalAttributeFieldInfo;

		private static readonly ValueSetter<TDrawer, Attribute> SetAttribute;

		private TDrawer drawer = new TDrawer();

		/// <summary>
		/// Initializes the <see cref="!:UnityDecoratorAttributeDrawer&lt;TDrawer, TAttribute&gt;" /> class.
		/// </summary>
		static UnityDecoratorAttributeDrawer()
		{
			InternalAttributeFieldInfo = typeof(TDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalAttributeFieldInfo == null)
			{
				Debug.LogError((object)("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled."));
			}
			else
			{
				SetAttribute = EmitUtilities.CreateInstanceFieldSetter<TDrawer, Attribute>(InternalAttributeFieldInfo);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			if (base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver)
			{
				CallNextDrawer(label);
				return;
			}
			if (SetAttribute == null)
			{
				SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
				return;
			}
			SetAttribute(ref drawer, (Attribute)(object)base.Attribute);
			float height = ((DecoratorDrawer)drawer).GetHeight();
			Rect controlRect = EditorGUILayout.GetControlRect(false, height, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			((DecoratorDrawer)drawer).OnGUI(controlRect);
			CallNextDrawer(label);
		}
	}
}
