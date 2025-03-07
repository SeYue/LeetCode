using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>
	/// Base class for all value drawers. Use this class to create your own custom drawers for any specific type.
	/// </para>
	///
	/// <para>
	/// Remember to provide your custom drawer with an <see cref="T:Sirenix.OdinInspector.Editor.OdinDrawerAttribute" />
	/// in order for it to be located by the <see cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" />.
	/// </para>
	///
	/// <para>
	/// Odin supports the use of GUILayout and takes care of undo for you. It also takes care of multi-selection
	/// in many simple cases. Checkout the manual for more information on handling multi-selection.
	/// </para>
	/// </summary>
	///
	/// <remarks>
	/// Checkout the manual for more information.
	/// </remarks>
	///
	/// <example>
	/// <code>
	/// public class MyCustomBaseType
	/// {
	///
	/// }
	///
	/// public class MyCustomType : MyCustomBaseType
	/// {
	///
	/// }
	///
	/// // Remember to wrap your custom attribute drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
	///
	/// public sealed class MyCustomBaseTypeDrawer&lt;T&gt; : OdinValueDrawer&lt;T&gt; where T : MyCustomBaseType
	/// {
	///     protected override void DrawPropertyLayout(IPropertyValueEntry&lt;T&gt; entry, GUIContent label)
	///     {
	///         T value = entry.SmartValue;
	///         // Draw your custom drawer here using GUILayout and EditorGUILAyout.
	///     }
	/// }
	///
	/// // Usage:
	/// // Both values will be drawn using the MyCustomBaseTypeDrawer
	/// public class MyComponent : SerializedMonoBehaviour
	/// {
	///     public MyCustomBaseType A;
	///
	///     public MyCustomType B;
	/// }
	/// </code>
	/// </example>
	///
	/// <example>
	/// <para>
	/// Odin uses multiple drawers to draw any given property, and the order in which these drawers are
	/// called are defined using the <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />.
	/// Your custom drawer injects itself into this chain of drawers based on its <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />.
	/// If no <see cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" /> is defined, a priority is generated automatically based on the type of the drawer.
	/// Each drawer can ether choose to draw the property or not, or pass on the responsibility to the
	/// next drawer by calling CallNextDrawer(). An example of this is provided in the documentation for <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />.
	/// </para>
	///
	/// <para>
	/// This means that there is no guarantee that your drawer will be called, sins other drawers
	/// could have a higher priority than yours and choose not to call CallNextDrawer().
	/// </para>
	///
	/// <para>
	/// To avoid this, you can tell Odin, that your drawer is a PrependDecorator or an AppendDecorator drawer (see <see cref="!:OdinDrawerBehaviour" />) as shown in the example shows below.
	/// Prepend and append decorators are always drawn and are also ordered by the <see cref="!:OdinDrawerBehaviour" />.
	/// </para>
	///
	/// <para>
	/// Note that Odin's <see cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" /> have full support for generic class constraints,
	/// and if that is not enough, you can also add additional type constraints by overriding CanDrawTypeFilter(Type type).
	/// </para>
	///
	/// <para>
	/// Also note that all custom property drawers needs to handle cases where the label provided by the DrawPropertyLayout is null,
	/// otherwise exceptions will be thrown when in cases where the label is hidden. For instance when [HideLabel] is used, or the property is drawn within a list where labels are also not shown.
	/// </para>
	///
	/// <code>
	/// // [OdinDrawer(OdinDrawerBehaviour.DrawProperty)] // default
	/// // [OdinDrawer(OdinDrawerBehaviour.AppendDecorator)]
	/// [OdinDrawer(OdinDrawerBehaviour.PrependDecorator)]
	/// [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
	/// public sealed class MyCustomTypeDrawer&lt;T&gt; : OdinValueDrawer&lt;T&gt; where T : MyCustomType
	/// {
	///     public override bool CanDrawTypeFilter(Type type)
	///     {
	///         return type != typeof(SomeType);
	///     }
	///
	///     protected override void DrawPropertyLayout(IPropertyValueEntry&lt;T&gt; entry, GUIContent label)
	///     {
	///         T value = entry.SmartValue;
	///         // Draw property here.
	///     }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`2" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinAttributeDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinValueDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinGroupDrawer`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinDrawer" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinDrawerAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerPriorityAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.DrawerLocator" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorUtilities" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.PropertyTree" />
	/// <seealso cref="T:Sirenix.Utilities.Editor.GUIHelper" />
	/// <seealso cref="T:Sirenix.Utilities.Editor.SirenixEditorGUI" />
	public abstract class OdinValueDrawer<T> : OdinDrawer
	{
		private IPropertyValueEntry<T> valueEntry;

		/// <summary>
		/// The value entry of the property.
		/// </summary>
		public IPropertyValueEntry<T> ValueEntry
		{
			get
			{
				if (valueEntry == null)
				{
					valueEntry = base.Property.ValueEntry as IPropertyValueEntry<T>;
					if (valueEntry == null)
					{
						base.Property.Update(forceUpdate: true);
						valueEntry = base.Property.ValueEntry as IPropertyValueEntry<T>;
					}
				}
				return valueEntry;
			}
		}

		/// <summary>
		/// Draws the property with GUILayout support.
		/// </summary>
		/// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Expected O, but got Unknown
			if (label != null)
			{
				EditorGUILayout.LabelField(label, new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label)."), (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			else
			{
				EditorGUILayout.LabelField(new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + GetType().GetNiceName() + "', or you are calling base.DrawPropertyLayout(label)."), (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
		}

		/// <summary>
		/// Gets a value indicating if the drawer can draw for the specified property.
		/// </summary>
		/// <param name="property">The property to test.</param>
		/// <returns><c>true</c> if the drawer can draw for the property. Otherwise <c>false</c>.</returns>
		public sealed override bool CanDrawProperty(InspectorProperty property)
		{
			IPropertyValueEntry propertyValueEntry = property.ValueEntry;
			if (propertyValueEntry != null && propertyValueEntry.TypeOfValue == typeof(T) && CanDrawTypeFilter(propertyValueEntry.TypeOfValue))
			{
				return CanDrawValueProperty(property);
			}
			return false;
		}

		/// <summary>
		/// Gets a value indicating if the drawer can draw for the specified property.
		/// Override this to implement a custom property filter for your drawer.
		/// </summary>
		/// <param name="property">The property to test.</param>
		/// <returns><c>true</c> if the drawer can draw for the property. Otherwise <c>false</c>.</returns>
		protected virtual bool CanDrawValueProperty(InspectorProperty property)
		{
			return true;
		}
	}
}
