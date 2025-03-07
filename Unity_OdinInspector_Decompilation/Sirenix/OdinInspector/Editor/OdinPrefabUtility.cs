using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class OdinPrefabUtility
	{
		public static void UpdatePrefabInstancePropertyModifications(Object prefabInstance, bool withUndo)
		{
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Expected O, but got Unknown
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected O, but got Unknown
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Expected O, but got Unknown
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Expected O, but got Unknown
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Expected O, but got Unknown
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Expected O, but got Unknown
			if (prefabInstance == (Object)null)
			{
				throw new ArgumentNullException("prefabInstance");
			}
			if (!(prefabInstance is ISupportsPrefabSerialization))
			{
				throw new ArgumentException("Type must implement ISupportsPrefabSerialization");
			}
			if (!(prefabInstance is ISerializationCallbackReceiver))
			{
				throw new ArgumentException("Type must implement ISerializationCallbackReceiver");
			}
			if (!OdinPrefabSerializationEditorUtility.ObjectIsPrefabInstance(prefabInstance))
			{
				throw new ArgumentException("Value must be a prefab instance");
			}
			Action action = null;
			HierarchyWindowItemCallback hierarchyCallback = (HierarchyWindowItemCallback)delegate
			{
				action();
			};
			ProjectWindowItemCallback projectCallback = (ProjectWindowItemCallback)delegate
			{
				action();
			};
			OnSceneFunc sceneCallback = (OnSceneFunc)delegate
			{
				action();
			};
			EditorApplication.hierarchyWindowItemOnGUI = (HierarchyWindowItemCallback)Delegate.Combine((Delegate)(object)EditorApplication.hierarchyWindowItemOnGUI, (Delegate)(object)hierarchyCallback);
			EditorApplication.projectWindowItemOnGUI = (ProjectWindowItemCallback)Delegate.Combine((Delegate)(object)EditorApplication.projectWindowItemOnGUI, (Delegate)(object)projectCallback);
			SceneView.onSceneGUIDelegate = (OnSceneFunc)Delegate.Combine((Delegate)(object)SceneView.onSceneGUIDelegate, (Delegate)(object)sceneCallback);
			action = delegate
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Expected O, but got Unknown
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Expected O, but got Unknown
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_004e: Expected O, but got Unknown
				//IL_0104: Unknown result type (might be due to invalid IL or missing references)
				//IL_010a: Invalid comparison between Unknown and I4
				//IL_0205: Unknown result type (might be due to invalid IL or missing references)
				//IL_020b: Expected O, but got Unknown
				EditorApplication.hierarchyWindowItemOnGUI = (HierarchyWindowItemCallback)Delegate.Remove((Delegate)(object)EditorApplication.hierarchyWindowItemOnGUI, (Delegate)(object)hierarchyCallback);
				EditorApplication.projectWindowItemOnGUI = (ProjectWindowItemCallback)Delegate.Remove((Delegate)(object)EditorApplication.projectWindowItemOnGUI, (Delegate)(object)projectCallback);
				SceneView.onSceneGUIDelegate = (OnSceneFunc)Delegate.Remove((Delegate)(object)SceneView.onSceneGUIDelegate, (Delegate)(object)sceneCallback);
				ISupportsPrefabSerialization supportsPrefabSerialization = (ISupportsPrefabSerialization)prefabInstance;
				if (supportsPrefabSerialization.SerializationData.PrefabModifications != null)
				{
					supportsPrefabSerialization.SerializationData.PrefabModifications.Clear();
				}
				if (supportsPrefabSerialization.SerializationData.PrefabModificationsReferencedUnityObjects != null)
				{
					supportsPrefabSerialization.SerializationData.PrefabModificationsReferencedUnityObjects.Clear();
				}
				PrefabModificationCache.CachePrefabModifications(prefabInstance, new List<PrefabModification>());
				try
				{
					if (!(prefabInstance == (Object)null))
					{
						if (Event.get_current() == null)
						{
							throw new InvalidOperationException("Delayed property modification delegate can only be called during the GUI event loop; Event.current must be accessible.");
						}
						try
						{
							PrefabUtility.RecordPrefabInstancePropertyModifications(prefabInstance);
						}
						catch (Exception ex)
						{
							Debug.LogError((object)"Exception occurred while calling Unity's PrefabUtility.RecordPrefabInstancePropertyModifications:");
							Debug.LogException(ex);
						}
						PropertyTree propertyTree = PropertyTree.Create((object)prefabInstance);
						propertyTree.DrawMonoScriptObjectField = false;
						bool flag = (int)Event.get_current().get_type() == 7;
						if (!flag)
						{
							GUIHelper.PushEventType((EventType)7);
						}
						propertyTree.BeginDraw(withUndo);
						foreach (InspectorProperty item in propertyTree.EnumerateTree())
						{
							if (item.ValueEntry != null && item.SupportsPrefabModifications)
							{
								item.Update(forceUpdate: true);
								if (item.ChildResolver is IKeyValueMapResolver)
								{
									if (item.ValueEntry.DictionaryChangedFromPrefab)
									{
										propertyTree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(item, 0);
									}
									else
									{
										InspectorProperty propertyAtPath = propertyTree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(item.Path);
										if (propertyAtPath != null && propertyAtPath.ValueEntry != null && item.SupportsPrefabModifications && item.ChildResolver is IKeyValueMapResolver)
										{
											propertyTree.PrefabModificationHandler.RegisterPrefabDictionaryDeltaModification(item, 0);
										}
									}
								}
							}
						}
						propertyTree.EndDraw();
						if (!flag)
						{
							GUIHelper.PopEventType();
						}
						ISerializationCallbackReceiver val2 = (ISerializationCallbackReceiver)prefabInstance;
						val2.OnBeforeSerialize();
						val2.OnAfterDeserialize();
					}
				}
				catch (Exception ex2)
				{
					Debug.LogException(ex2);
				}
			};
			foreach (SceneView sceneView in SceneView.get_sceneViews())
			{
				SceneView val = sceneView;
				((EditorWindow)val).Repaint();
			}
		}
	}
}
