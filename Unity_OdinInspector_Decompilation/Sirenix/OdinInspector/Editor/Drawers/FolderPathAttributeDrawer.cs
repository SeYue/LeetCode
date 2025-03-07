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
	public sealed class FolderPathAttributeDrawer : OdinAttributeDrawer<FolderPathAttribute, string>, IDefinesGenericMenuItems
	{
		private ValueResolver<string> parentResolver;

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			parentResolver = ValueResolver.GetForString(base.Property, base.Attribute.ParentFolder);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (parentResolver.HasError)
			{
				SirenixEditorGUI.ErrorMessageBox(parentResolver.ErrorMessage);
			}
			EditorGUI.BeginChangeCheck();
			base.ValueEntry.SmartValue = SirenixEditorFields.FolderPathField(label, base.ValueEntry.SmartValue, parentResolver.GetValue(), base.Attribute.AbsolutePath, base.Attribute.UseBackslashes);
		}

		/// <summary>
		/// Adds customs generic menu options.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Expected O, but got Unknown
			//IL_0151: Expected O, but got Unknown
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Expected O, but got Unknown
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Expected O, but got Unknown
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Expected O, but got Unknown
			//IL_01a2: Expected O, but got Unknown
			InspectorProperty inspectorProperty = property.FindParent((InspectorProperty p) => p.Info.HasSingleBackingMember, includeSelf: true);
			IPropertyValueEntry<string> propertyValueEntry = (IPropertyValueEntry<string>)property.ValueEntry;
			string value = parentResolver.GetValue();
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			bool flag = false;
			string createDirectoryPath = propertyValueEntry.SmartValue;
			if (!createDirectoryPath.IsNullOrWhitespace())
			{
				if (!Path.IsPathRooted(createDirectoryPath))
				{
					if (!value.IsNullOrWhitespace())
					{
						createDirectoryPath = Path.Combine(value, createDirectoryPath);
					}
					createDirectoryPath = Path.GetFullPath(createDirectoryPath);
				}
				flag = Directory.Exists(createDirectoryPath);
			}
			string showInExplorerPath = createDirectoryPath;
			if (showInExplorerPath.IsNullOrWhitespace())
			{
				if (!value.IsNullOrWhitespace())
				{
					showInExplorerPath = Path.GetFullPath(value);
				}
				else
				{
					showInExplorerPath = Path.GetDirectoryName(Application.get_dataPath());
				}
			}
			while (!showInExplorerPath.IsNullOrWhitespace() && !Directory.Exists(showInExplorerPath))
			{
				showInExplorerPath = Path.GetDirectoryName(showInExplorerPath);
			}
			if (!showInExplorerPath.IsNullOrWhitespace())
			{
				genericMenu.AddItem(new GUIContent("Show in explorer"), false, (MenuFunction)delegate
				{
					Application.OpenURL(showInExplorerPath);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Show in explorer"));
			}
			if (flag || createDirectoryPath.IsNullOrWhitespace())
			{
				genericMenu.AddDisabledItem(new GUIContent("Create directory"));
				return;
			}
			genericMenu.AddItem(new GUIContent("Create directory"), false, (MenuFunction)delegate
			{
				Directory.CreateDirectory(createDirectoryPath);
				AssetDatabase.Refresh();
			});
		}
	}
}
