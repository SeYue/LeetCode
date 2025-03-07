using System;
using System.Text;
using Sirenix.Serialization.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Provides a variety of miscellaneous utilities widely used in the inspector.
	/// </summary>
	public static class InspectorUtilities
	{
		/// <summary>
		/// Converts an Odin property path to a deep reflection path.
		/// </summary>
		public static string ConvertToDeepReflectionPath(string odinPropertyPath)
		{
			return ConvertOdinPath(odinPropertyPath, isUnity: false);
		}

		/// <summary>
		/// Converts an Odin property path (without groups included) into a Unity property path.
		/// </summary>
		public static string ConvertToUnityPropertyPath(string odinPropertyPath)
		{
			return ConvertOdinPath(odinPropertyPath, isUnity: true);
		}

		private static string ConvertOdinPath(string odinPropertyPath, bool isUnity)
		{
			bool flag = false;
			for (int i = 0; i < odinPropertyPath.Length; i++)
			{
				if (odinPropertyPath[i] == '$' || odinPropertyPath[i] == '#')
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				using (Cache<StringBuilder> cache = Cache<StringBuilder>.Claim())
				{
					StringBuilder value = cache.Value;
					value.Length = 0;
					bool flag2 = false;
					for (int j = 0; j < odinPropertyPath.Length; j++)
					{
						char c = odinPropertyPath[j];
						if (c == '.')
						{
							flag2 = false;
						}
						else if (flag2)
						{
							continue;
						}
						switch (c)
						{
						case '$':
							value.Append(isUnity ? "Array.data[" : "[");
							for (j++; j < odinPropertyPath.Length && char.IsNumber(odinPropertyPath[j]); j++)
							{
								value.Append(odinPropertyPath[j]);
							}
							value.Append(']');
							j--;
							break;
						case '#':
							flag2 = true;
							break;
						case '.':
							if (value.Length > 0 && value[value.Length - 1] != '.')
							{
								value.Append('.');
							}
							break;
						default:
							value.Append(c);
							break;
						}
					}
					while (value.Length > 0 && value[0] == '.')
					{
						value.Remove(0, 1);
					}
					while (value.Length > 0 && value[value.Length - 1] == '.')
					{
						value.Remove(value.Length - 1, 1);
					}
					return value.ToString();
				}
			}
			return odinPropertyPath;
		}

		/// <summary>
		/// Prepares a property tree for drawing, and handles management of undo, as well as marking scenes and drawn assets dirty.
		/// </summary>
		/// <param name="tree">The tree to be drawn.</param>
		/// <param name="withUndo">Whether to register undo commands for the changes made to the tree. This can only be set to true if the tree has a <see cref="T:UnityEditor.SerializedObject" /> to represent.</param>
		/// <exception cref="T:System.ArgumentNullException">tree is null</exception>
		[Obsolete("Use PropertyTree.BeginDraw instead.", false)]
		public static void BeginDrawPropertyTree(PropertyTree tree, bool withUndo)
		{
			tree.BeginDraw(withUndo);
		}

		/// <summary>
		/// Ends drawing a property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.
		/// </summary>
		/// <param name="tree">The tree.</param>
		[Obsolete("Use PropertyTree.EndDraw instead.", false)]
		public static void EndDrawPropertyTree(PropertyTree tree)
		{
			tree.EndDraw();
		}

		public static void RegisterUnityObjectDirty(Object unityObj)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if (AssetDatabase.Contains(unityObj))
			{
				EditorUtility.SetDirty(unityObj);
			}
			else if (!Application.get_isPlaying())
			{
				if (unityObj is Component)
				{
					Component val = (Component)unityObj;
					EditorUtility.SetDirty((Object)(object)val);
					EditorUtility.SetDirty((Object)(object)val.get_gameObject());
					EditorSceneManager.MarkSceneDirty(val.get_gameObject().get_scene());
				}
				else if (unityObj is EditorWindow || unityObj is ScriptableObject)
				{
					EditorUtility.SetDirty(unityObj);
				}
				else
				{
					EditorUtility.SetDirty(unityObj);
					EditorSceneManager.MarkAllScenesDirty();
				}
			}
		}

		/// <summary>
		/// Draws all properties in a given property tree; must be wrapped by a <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.BeginDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree,System.Boolean)" /> and <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.EndDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree)" />.
		/// </summary>
		/// <param name="tree">The tree to be drawn.</param>
		public static void DrawPropertiesInTree(PropertyTree tree)
		{
			tree.DrawProperties();
		}

		/// <summary>
		/// Draws a property in the inspector using a given label.
		/// </summary>
		[Obsolete("Use InspectorProperty.Draw(label) instead.", false)]
		public static void DrawProperty(InspectorProperty property, GUIContent label)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			property.Draw(label);
		}
	}
}
