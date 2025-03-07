using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// Base class for all Odin drawers. In order to create your own custom drawers you need to derive from one of the following drawers:
	/// </para>
	/// <list type="bullet">
	/// <item><see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`1" /></item>
	/// <item><see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" /></item>
	/// <item><see cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" /></item>
	/// <item><see cref="T:Sirenix.OdinInspector.Editor.OdinGroupDrawer`1" /></item>
	/// </list>
	/// <para>Remember to provide your custom drawer with an <see cref="T:Sirenix.OdinInspector.Editor.OdinDrawerAttribute" /> in order for it to be located by the <see cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" />.</para>
	/// <para>Drawers require a <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> context, and are instantiated automatically by the <see cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" />.</para>
	/// <para>Odin supports the use of GUILayout and takes care of undo for you. It also takes care of multi-selection in many simple cases. Checkout the manual for more information.</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinGroupDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinDrawerAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorUtilities" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.PropertyTree" />
	/// <seealso cref="T:Sirenix.Utilities.Editor.GUIHelper" />
	/// <seealso cref="T:Sirenix.Utilities.Editor.SirenixEditorGUI" />
	public abstract class OdinDrawer
	{
		private bool initialized;

		private InspectorProperty property;

		/// <summary>
		/// If <c>true</c> then this drawer will be skipped in the draw chain. Otherwise the drawer will be called as normal in the draw chain.
		/// </summary>
		public bool SkipWhenDrawing { get; set; }

		/// <summary>
		/// Gets a value indicating if the drawer has been initialized yet.
		/// </summary>
		public bool Initialized => initialized;

		/// <summary>
		/// Gets the property this drawer draws for.
		/// </summary>
		public InspectorProperty Property => property;

		/// <summary>
		/// <para>Override this method in order to define custom type constraints to specify whether or not a type should be drawn by the drawer.</para>
		/// <para>Note that Odin's <see cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" /> has full support for generic class constraints, so most often you can get away with not overriding CanDrawTypeFilter.</para>
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		/// Returns true by default, unless overridden.
		/// </returns>
		public virtual bool CanDrawTypeFilter(Type type)
		{
			return true;
		}

		/// <summary>
		/// Initializes the drawer instance.
		/// </summary>
		/// <param name="property"></param>
		public void Initialize(InspectorProperty property)
		{
			if (!initialized)
			{
				this.property = property;
				try
				{
					Initialize();
				}
				finally
				{
					initialized = true;
				}
			}
		}

		/// <summary>
		/// Initializes the drawer instance. Override this to implement your own initialization logic.
		/// </summary>
		protected virtual void Initialize()
		{
		}

		/// <summary>
		/// Draws the property with a custom label.
		/// </summary>
		/// <param name="label">The label. Null is allow if you wish no label should be drawn.</param>
		public void DrawProperty(GUIContent label)
		{
			if (!initialized)
			{
				throw new InvalidOperationException("Cannot call DrawProperty on a drawer before it has been initialized!");
			}
			DrawPropertyLayout(label);
		}

		/// <summary>
		/// Draws the property with GUILayout support.
		/// </summary>
		/// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
		protected virtual void DrawPropertyLayout(GUIContent label)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Expected O, but got Unknown
			if (label != null)
			{
				EditorGUILayout.LabelField(label, new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label), which you shouldn't."), (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			else
			{
				EditorGUILayout.LabelField(new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label), which you shouldn't."), (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
		}

		/// <summary>
		/// Calls the next drawer in the draw chain.
		/// </summary>
		/// <param name="label">The label to pass on to the next drawer.</param>
		protected bool CallNextDrawer(GUIContent label)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			OdinDrawer odinDrawer = null;
			BakedDrawerChain activeDrawerChain = property.GetActiveDrawerChain();
			if (activeDrawerChain.MoveNext())
			{
				odinDrawer = activeDrawerChain.Current;
			}
			if (odinDrawer != null)
			{
				odinDrawer.DrawPropertyLayout(label);
				return true;
			}
			if (property.ValueEntry != null)
			{
				Rect controlRect = EditorGUILayout.GetControlRect((GUILayoutOption[])(object)new GUILayoutOption[0]);
				if (label == null)
				{
					GUI.Label(controlRect, Property.NiceName);
				}
				else
				{
					GUI.Label(controlRect, label);
				}
			}
			else
			{
				GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
				if (label != null)
				{
					EditorGUILayout.PrefixLabel(label);
				}
				SirenixEditorGUI.WarningMessageBox(string.Concat("There is no drawer defined for property ", property.NiceName, " of type ", property.Info.PropertyType, "."));
				GUILayout.EndHorizontal();
			}
			return false;
		}

		/// <summary>
		/// Gets a value indicating if the drawer can draw for the specified property.
		/// Override this to implement a custom property filter for your drawer.
		/// </summary>
		/// <param name="property">The property to test.</param>
		/// <returns><c>true</c> if the drawer can draw for the property. Otherwise <c>false</c>.</returns>
		public virtual bool CanDrawProperty(InspectorProperty property)
		{
			return true;
		}
	}
}
