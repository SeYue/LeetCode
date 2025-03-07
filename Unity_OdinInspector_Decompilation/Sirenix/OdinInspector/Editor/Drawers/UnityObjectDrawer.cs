using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Unity object drawer.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.25)]
	public sealed class UnityObjectDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : Object
	{
		private bool drawAsPreview;

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return !property.IsTreeRoot;
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			drawAsPreview = false;
			GeneralDrawerConfig.UnityObjectType squareUnityObjectEnableFor = GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectEnableFor;
			drawAsPreview = squareUnityObjectEnableFor != 0 && (((squareUnityObjectEnableFor & GeneralDrawerConfig.UnityObjectType.Components) != 0 && typeof(Component).IsAssignableFrom(typeof(T))) || ((squareUnityObjectEnableFor & GeneralDrawerConfig.UnityObjectType.GameObjects) != 0 && typeof(GameObject).IsAssignableFrom(typeof(T))) || ((squareUnityObjectEnableFor & GeneralDrawerConfig.UnityObjectType.Materials) != 0 && typeof(Material).IsAssignableFrom(typeof(T))) || ((squareUnityObjectEnableFor & GeneralDrawerConfig.UnityObjectType.Sprites) != 0 && typeof(Sprite).IsAssignableFrom(typeof(T))) || ((squareUnityObjectEnableFor & GeneralDrawerConfig.UnityObjectType.Textures) != 0 && typeof(Texture).IsAssignableFrom(typeof(T))));
			if (!drawAsPreview && (squareUnityObjectEnableFor & GeneralDrawerConfig.UnityObjectType.Others) != 0 && !typeof(Component).IsAssignableFrom(typeof(T)) && !typeof(GameObject).IsAssignableFrom(typeof(T)) && !typeof(Material).IsAssignableFrom(typeof(T)) && !typeof(Sprite).IsAssignableFrom(typeof(T)) && !typeof(Texture).IsAssignableFrom(typeof(T)))
			{
				drawAsPreview = true;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			if (!drawAsPreview)
			{
				if (propertyValueEntry.BaseValueType.IsInterface)
				{
					propertyValueEntry.WeakSmartValue = SirenixEditorFields.PolymorphicObjectField(label, propertyValueEntry.WeakSmartValue, propertyValueEntry.BaseValueType, propertyValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null);
					return;
				}
				object weakSmartValue = propertyValueEntry.WeakSmartValue;
				propertyValueEntry.WeakSmartValue = SirenixEditorFields.UnityObjectField(label, weakSmartValue as Object, propertyValueEntry.BaseValueType, propertyValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null);
			}
			else
			{
				object weakSmartValue2 = propertyValueEntry.WeakSmartValue;
				propertyValueEntry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(label, weakSmartValue2 as Object, propertyValueEntry.BaseValueType, propertyValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null, GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectFieldHeight, GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectAlignment);
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			//IL_0046: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected O, but got Unknown
			Object unityObj = default(Object);
			ref Object val = ref unityObj;
			object weakSmartValue = property.ValueEntry.WeakSmartValue;
			val = weakSmartValue as Object;
			if (Object.op_Implicit(unityObj))
			{
				genericMenu.AddItem(new GUIContent("Open in new inspector"), false, (MenuFunction)delegate
				{
					GUIHelper.OpenInspectorWindow(unityObj);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Open in new inspector"));
			}
		}
	}
}
