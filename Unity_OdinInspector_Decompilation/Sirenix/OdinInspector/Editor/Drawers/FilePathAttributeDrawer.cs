using System.Diagnostics;
using System.IO;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public sealed class FilePathAttributeDrawer : OdinAttributeDrawer<FilePathAttribute, string>, IDefinesGenericMenuItems
	{
		private ValueResolver<string> parentResolver;

		private ValueResolver<string> extensionsResolver;

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			parentResolver = ValueResolver.GetForString(base.Property, base.Attribute.ParentFolder);
			extensionsResolver = ValueResolver.GetForString(base.Property, base.Attribute.Extensions);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			ValueResolver.DrawErrors(parentResolver, extensionsResolver);
			base.ValueEntry.SmartValue = SirenixEditorFields.FilePathField(label, base.ValueEntry.SmartValue, parentResolver.GetValue(), extensionsResolver.GetValue(), base.Attribute.AbsolutePath, base.Attribute.UseBackslashes);
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Expected O, but got Unknown
			//IL_0137: Expected O, but got Unknown
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Expected O, but got Unknown
			InspectorProperty inspectorProperty = property.FindParent((InspectorProperty p) => p.Info.HasSingleBackingMember, includeSelf: true);
			IPropertyValueEntry<string> propertyValueEntry = (IPropertyValueEntry<string>)property.ValueEntry;
			string value = parentResolver.GetValue();
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			string path = propertyValueEntry.SmartValue;
			if (!path.IsNullOrWhitespace())
			{
				if (!Path.IsPathRooted(path))
				{
					if (!value.IsNullOrWhitespace())
					{
						path = Path.Combine(value, path);
					}
					path = Path.GetFullPath(path);
				}
			}
			else if (!value.IsNullOrWhitespace())
			{
				path = Path.GetFullPath(value);
			}
			else
			{
				path = Path.GetDirectoryName(Application.get_dataPath());
			}
			if (!path.IsNullOrWhitespace())
			{
				while (!path.IsNullOrWhitespace() && !Directory.Exists(path))
				{
					path = Path.GetDirectoryName(path);
				}
			}
			if (!path.IsNullOrWhitespace())
			{
				genericMenu.AddItem(new GUIContent("Show in explorer"), false, (MenuFunction)delegate
				{
					Process.Start(path);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Show in explorer"));
			}
		}
	}
}
