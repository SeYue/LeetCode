using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public static class PropertyContextMenuDrawer
	{
		public static class GenericMenuUtility
		{
			private static FieldInfo GenericMenu_menuItems_Field;

			public static readonly bool Available;

			static GenericMenuUtility()
			{
				GenericMenu_menuItems_Field = typeof(GenericMenu).GetField("menuItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}

			public static ArrayList GetMenuItems(GenericMenu genericMenu)
			{
				throw new NotImplementedException();
			}
		}

		private class IndexPopupWindow
		{
			[HideInInspector]
			public string IntLabel;

			[HideInInspector]
			public int MaxCount;

			[LabelText("$IntLabel")]
			[MinValue(0.0)]
			[MaxValue("$MaxCount")]
			public int Value;

			[HideInInspector]
			public Action MoveAction;

			[HideInInspector]
			public Action CloseWindowAction;

			[HorizontalGroup("Buttons", 0f, 0, 0, 0f)]
			[Button]
			public void Move()
			{
				MoveAction();
				CloseWindowAction();
			}

			[HorizontalGroup("Buttons", 0f, 0, 0, 0f)]
			[Button]
			public void Cancel()
			{
				CloseWindowAction();
			}

			[OnInspectorGUI]
			[PropertyOrder(-1f)]
			private void DetectEnter()
			{
				if (Event.get_current().OnKeyUp((KeyCode)13))
				{
					Move();
					GUIUtility.ExitGUI();
				}
			}
		}

		private static MethodInfo EditorGUI_FillPropertyContextMenu = typeof(EditorGUI).GetMethod("FillPropertyContextMenu", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
		{
			typeof(SerializedProperty),
			typeof(SerializedProperty),
			typeof(GenericMenu)
		}, null);

		/// <summary>
		/// Adds the right click area.
		/// </summary>
		public static void AddRightClickArea(InspectorProperty property, Rect rect)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			int controlID = GUIUtility.GetControlID((FocusType)2);
			AddRightClickArea(property, rect, controlID);
		}

		/// <summary>
		/// Adds the right click area.
		/// </summary>
		public static void AddRightClickArea(InspectorProperty property, Rect rect, int id)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Invalid comparison between Unknown and I4
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Invalid comparison between Unknown and I4
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Event.get_current().get_type() == 0 && Event.get_current().get_button() == 1 && ((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()))
			{
				GUIUtility.set_hotControl(id);
				Event.get_current().Use();
				GUIHelper.RequestRepaint();
			}
			if ((int)Event.get_current().get_type() == 1 && ((Rect)(ref rect)).Contains(Event.get_current().get_mousePosition()) && id == GUIUtility.get_hotControl())
			{
				GUIHelper.RemoveFocusControl();
				Event.get_current().Use();
				GenericMenu val = new GenericMenu();
				GUIHelper.RemoveFocusControl();
				PopulateGenericMenu(property, val);
				property.PopulateGenericMenu(val);
				if (val.GetItemCount() == 0)
				{
					val = null;
				}
				else
				{
					val.ShowAsContext();
				}
			}
			if (GUIUtility.get_hotControl() == id && (int)Event.get_current().get_type() == 7)
			{
				((Rect)(ref rect)).set_width(3f);
				((Rect)(ref rect)).set_x(((Rect)(ref rect)).get_x() - 4f);
				SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.HighlightedTextColor);
			}
		}

		public static GenericMenu FillUnityContextMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			SerializedProperty unityPropertyForPath = property.Tree.GetUnityPropertyForPath(property.UnityPropertyPath);
			if (unityPropertyForPath == null)
			{
				return (GenericMenu)(((object)genericMenu) ?? ((object)new GenericMenu()));
			}
			return FillUnityContextMenu(unityPropertyForPath, genericMenu);
		}

		public static GenericMenu FillUnityContextMenu(SerializedProperty property, GenericMenu genericMenu = null)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (EditorGUI_FillPropertyContextMenu != null)
			{
				return (GenericMenu)EditorGUI_FillPropertyContextMenu.Invoke(null, new object[3] { property, null, genericMenu });
			}
			object obj = UnityPropertyHandlerUtility.ScriptAttributeUtility_GetHandler(property);
			if (obj != null)
			{
				genericMenu = (GenericMenu)(((object)genericMenu) ?? ((object)new GenericMenu()));
				UnityPropertyHandlerUtility.PropertyHandler_AddMenuItems(obj, property, genericMenu);
			}
			return (GenericMenu)(((object)genericMenu) ?? ((object)new GenericMenu()));
		}

		private static void PopulateChangedFromPrefabContext(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Expected O, but got Unknown
			//IL_014d: Expected O, but got Unknown
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Expected O, but got Unknown
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Expected O, but got Unknown
			//IL_0199: Expected O, but got Unknown
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Expected O, but got Unknown
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Expected O, but got Unknown
			//IL_01e5: Expected O, but got Unknown
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Expected O, but got Unknown
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0269: Expected O, but got Unknown
			//IL_0269: Expected O, but got Unknown
			if (!property.Tree.PrefabModificationHandler.HasPrefabs)
			{
				return;
			}
			IPropertyValueEntry entry = property.ValueEntry;
			if (entry == null)
			{
				return;
			}
			InspectorProperty prefabProperty = null;
			if (property.Tree.PrefabModificationHandler.PrefabPropertyTree != null)
			{
				prefabProperty = property.Tree.PrefabModificationHandler.PrefabPropertyTree.GetPropertyAtPath(property.Path);
			}
			bool flag = prefabProperty != null;
			int num = property.Children.Recurse().Count((InspectorProperty c) => c.ValueEntry != null && c.ValueEntry.ValueChangedFromPrefab);
			bool flag2 = false;
			if (entry.ValueChangedFromPrefab || num > 0)
			{
				if (flag)
				{
					genericMenu.AddItem(new GUIContent("Revert to prefab value" + ((num > 0) ? (" (" + num + " child modifications to revert)") : "")), false, (MenuFunction)delegate
					{
						property.RecordForUndo("Revert to prefab value");
						for (int n = 0; n < entry.ValueCount; n++)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, n, PrefabModificationType.Value);
						}
						if (property.Tree.UnitySerializedObject != null)
						{
							property.Tree.UnitySerializedObject.Update();
						}
					});
					flag2 = true;
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Revert to prefab value (Does not exist on prefab)"));
				}
			}
			if (entry.ListLengthChangedFromPrefab)
			{
				if (flag)
				{
					genericMenu.AddItem(new GUIContent("Revert to prefab list length"), false, (MenuFunction)delegate
					{
						property.RecordForUndo("Revert to prefab list length");
						for (int m = 0; m < entry.ValueCount; m++)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, m, PrefabModificationType.ListLength);
						}
						property.Children.Update();
						if (property.Tree.UnitySerializedObject != null)
						{
							property.Tree.UnitySerializedObject.Update();
						}
					});
					flag2 = true;
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Revert to prefab list length (Does not exist on prefab)"));
				}
			}
			if (entry.DictionaryChangedFromPrefab)
			{
				if (flag)
				{
					genericMenu.AddItem(new GUIContent("Revert dictionary changes to prefab value"), false, (MenuFunction)delegate
					{
						property.RecordForUndo("Revert to prefab dictionary");
						for (int l = 0; l < entry.ValueCount; l++)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, l, PrefabModificationType.Dictionary);
						}
						property.Children.Update();
						if (property.Tree.UnitySerializedObject != null)
						{
							property.Tree.UnitySerializedObject.Update();
						}
					});
					flag2 = true;
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Revert dictionary changes to prefab value (Does not exist on prefab)"));
				}
			}
			if (!flag2)
			{
				return;
			}
			string applyText = default(string);
			ref string val = ref applyText;
			object obj = prefabProperty.Tree.WeakTargets[0];
			val = "Apply value to prefab '" + (obj as Object).get_name() + "'";
			genericMenu.AddItem(new GUIContent(applyText), false, (MenuFunction)delegate
			{
				bool flag3 = false;
				Object[] array = prefabProperty.SerializationRoot.ValueEntry.WeakValues.Cast<Object>().AppendWith(property.SerializationRoot.ValueEntry.WeakValues.Cast<Object>()).ToArray();
				Undo.RecordObjects(array, applyText);
				if (OdinPrefabSerializationEditorUtility.get_HasApplyPropertyOverride() && property.ValueEntry.SerializationBackend.IsUnity && property.Tree.UnitySerializedObject != null)
				{
					SerializedProperty val2 = property.Tree.GetUnitySerializedObjectNoUpdate().FindProperty(property.UnityPropertyPath);
					if (val2 != null)
					{
						PrefabModificationHandler prefabModificationHandler = property.Tree.PrefabModificationHandler;
						for (int i = 0; i < prefabModificationHandler.TargetPrefabs.Count; i++)
						{
							string assetPath = AssetDatabase.GetAssetPath(prefabModificationHandler.TargetPrefabs[i]);
							OdinPrefabSerializationEditorUtility.ApplyPropertyOverride(val2, assetPath);
						}
						flag3 = true;
					}
				}
				if (!flag3)
				{
					IPropertyValueEntry valueEntry = prefabProperty.ValueEntry;
					for (int j = 0; j < entry.ValueCount; j++)
					{
						object obj2 = entry.WeakValues[j];
						object value = SerializationUtility.CreateCopy(obj2);
						valueEntry.WeakValues[j] = value;
					}
					valueEntry.ApplyChanges();
					for (int k = 0; k < entry.ValueCount; k++)
					{
						if (entry.ValueChangedFromPrefab)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, k, PrefabModificationType.Value);
						}
						if (entry.ListLengthChangedFromPrefab)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, k, PrefabModificationType.ListLength);
						}
						if (entry.DictionaryChangedFromPrefab)
						{
							property.Tree.PrefabModificationHandler.RemovePrefabModification(property, k, PrefabModificationType.Dictionary);
						}
					}
				}
				if (property.Tree.UnitySerializedObject != null)
				{
					property.Tree.UnitySerializedObject.Update();
				}
				Undo.FlushUndoRecordObjects();
			});
		}

		private static void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Expected O, but got Unknown
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Expected O, but got Unknown
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Expected O, but got Unknown
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Expected O, but got Unknown
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Expected O, but got Unknown
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Expected O, but got Unknown
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Expected O, but got Unknown
			//IL_0183: Expected O, but got Unknown
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Expected O, but got Unknown
			//IL_01a6: Expected O, but got Unknown
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Expected O, but got Unknown
			//IL_01c9: Expected O, but got Unknown
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Expected O, but got Unknown
			//IL_01ec: Expected O, but got Unknown
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Expected O, but got Unknown
			//IL_0227: Expected O, but got Unknown
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0239: Expected O, but got Unknown
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Expected O, but got Unknown
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02db: Expected O, but got Unknown
			//IL_02db: Expected O, but got Unknown
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Expected O, but got Unknown
			//IL_02fe: Expected O, but got Unknown
			//IL_041a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0427: Unknown result type (might be due to invalid IL or missing references)
			//IL_0431: Expected O, but got Unknown
			//IL_0431: Expected O, but got Unknown
			//IL_0439: Unknown result type (might be due to invalid IL or missing references)
			//IL_0443: Expected O, but got Unknown
			//IL_0452: Unknown result type (might be due to invalid IL or missing references)
			//IL_045f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0469: Expected O, but got Unknown
			//IL_0469: Expected O, but got Unknown
			//IL_048b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0498: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a2: Expected O, but got Unknown
			//IL_04a2: Expected O, but got Unknown
			//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c4: Expected O, but got Unknown
			//IL_04c4: Expected O, but got Unknown
			//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e1: Expected O, but got Unknown
			//IL_04e1: Expected O, but got Unknown
			//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04fe: Expected O, but got Unknown
			//IL_04fe: Expected O, but got Unknown
			//IL_0504: Unknown result type (might be due to invalid IL or missing references)
			//IL_0511: Unknown result type (might be due to invalid IL or missing references)
			//IL_051b: Expected O, but got Unknown
			//IL_051b: Expected O, but got Unknown
			//IL_0523: Unknown result type (might be due to invalid IL or missing references)
			//IL_052d: Expected O, but got Unknown
			//IL_0548: Unknown result type (might be due to invalid IL or missing references)
			//IL_0555: Unknown result type (might be due to invalid IL or missing references)
			//IL_055f: Expected O, but got Unknown
			//IL_055f: Expected O, but got Unknown
			//IL_0566: Unknown result type (might be due to invalid IL or missing references)
			//IL_0570: Expected O, but got Unknown
			if (Event.get_current().get_shift())
			{
				FillUnityContextMenu(property, genericMenu);
			}
			PopulateChangedFromPrefabContext(property, genericMenu);
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			if (property.Parent != null && property.Parent.ChildResolver is IOrderedCollectionResolver)
			{
				IOrderedCollectionResolver parentResolver = property.Parent.ChildResolver as IOrderedCollectionResolver;
				if ((property.Parent.GetAttribute<ListDrawerSettingsAttribute>()?.IsReadOnly ?? false) || parentResolver.IsReadOnly)
				{
					genericMenu.AddDisabledItem(new GUIContent("Move element to top"));
					genericMenu.AddDisabledItem(new GUIContent("Move element to bottom"));
					genericMenu.AddDisabledItem(new GUIContent("Move element to index"));
					genericMenu.AddDisabledItem(new GUIContent("Duplicate element"));
					genericMenu.AddDisabledItem(new GUIContent("Insert pasted element"));
					genericMenu.AddDisabledItem(new GUIContent("Insert new element"));
					genericMenu.AddDisabledItem(new GUIContent("Delete element"));
				}
				else
				{
					genericMenu.AddItem(new GUIContent("Move element to top"), false, (MenuFunction)delegate
					{
						object[] array6 = new object[property.ValueEntry.WeakValues.Count];
						for (int num3 = 0; num3 < array6.Length; num3++)
						{
							array6[num3] = property.ValueEntry.WeakValues[num3];
						}
						parentResolver.QueueRemoveAt(property.Index);
						parentResolver.QueueInsertAt(0, array6);
					});
					genericMenu.AddItem(new GUIContent("Move element to bottom"), false, (MenuFunction)delegate
					{
						object[] array5 = new object[property.ValueEntry.WeakValues.Count];
						for (int num2 = 0; num2 < array5.Length; num2++)
						{
							array5[num2] = property.ValueEntry.WeakValues[num2];
						}
						parentResolver.QueueRemoveAt(property.Index);
						parentResolver.QueueAdd(array5);
					});
					genericMenu.AddItem(new GUIContent("Move element to index"), false, (MenuFunction)delegate
					{
						IndexPopupWindow popup = new IndexPopupWindow();
						popup.IntLabel = "Index";
						popup.Value = property.Index;
						popup.MaxCount = property.Parent.Children.Count - 1;
						popup.MoveAction = delegate
						{
							int num = popup.Value;
							int index = property.Index;
							bool flag5 = false;
							if (num < 0)
							{
								num = 0;
							}
							if (num > property.Parent.Children.Count)
							{
								flag5 = true;
							}
							object[] array4 = new object[property.ValueEntry.WeakValues.Count];
							for (int n = 0; n < array4.Length; n++)
							{
								array4[n] = property.ValueEntry.WeakValues[n];
							}
							parentResolver.QueueRemoveAt(property.Index);
							if (flag5)
							{
								parentResolver.QueueAdd(array4);
							}
							else
							{
								parentResolver.QueueInsertAt(num, array4);
							}
						};
						property.Tree.DelayActionUntilRepaint(delegate
						{
							OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(popup, 120f);
							popup.CloseWindowAction = delegate
							{
								//IL_0011: Unknown result type (might be due to invalid IL or missing references)
								//IL_001b: Expected O, but got Unknown
								//IL_001b: Unknown result type (might be due to invalid IL or missing references)
								//IL_0025: Expected O, but got Unknown
								EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)new CallbackFunction(((EditorWindow)window).Close));
							};
						});
					});
					genericMenu.AddItem(new GUIContent("Duplicate element"), false, (MenuFunction)delegate
					{
						object[] array3 = new object[property.ValueEntry.WeakValues.Count];
						for (int m = 0; m < array3.Length; m++)
						{
							array3[m] = SerializationUtility.CreateCopy(property.ValueEntry.WeakValues[m]);
						}
						if (property.Index + 1 >= property.Parent.Children.Count)
						{
							parentResolver.QueueAdd(array3);
						}
						else
						{
							parentResolver.QueueInsertAt(property.Index + 1, array3);
						}
					});
					if (Clipboard.CanPaste(parentResolver.ElementType))
					{
						genericMenu.AddItem(new GUIContent("Insert pasted element"), false, (MenuFunction)delegate
						{
							object obj = Clipboard.Paste();
							object[] array2 = new object[property.ValueEntry.WeakValues.Count];
							array2[0] = obj;
							for (int l = 1; l < array2.Length; l++)
							{
								array2[l] = SerializationUtility.CreateCopy(property.ValueEntry.WeakValues[l]);
							}
							parentResolver.QueueInsertAt(property.Index, array2);
						});
					}
					else
					{
						genericMenu.AddDisabledItem(new GUIContent("Insert pasted element"));
					}
					IHackyListDrawerInteractions parentListDrawer = null;
					OdinDrawer[] bakedDrawerArray = property.Parent.GetActiveDrawerChain().BakedDrawerArray;
					foreach (OdinDrawer odinDrawer in bakedDrawerArray)
					{
						parentListDrawer = odinDrawer as IHackyListDrawerInteractions;
						if (parentListDrawer != null)
						{
							break;
						}
					}
					if (parentListDrawer == null || !parentListDrawer.CanCreateValuesToAdd)
					{
						genericMenu.AddDisabledItem(new GUIContent("Insert new element"));
					}
					else
					{
						genericMenu.AddItem(new GUIContent("Insert new element"), false, (MenuFunction)delegate
						{
							property.Tree.DelayActionUntilRepaint(delegate
							{
								//IL_0039: Unknown result type (might be due to invalid IL or missing references)
								//IL_003e: Unknown result type (might be due to invalid IL or missing references)
								//IL_0043: Unknown result type (might be due to invalid IL or missing references)
								parentListDrawer.CreateValuesToAdd(delegate(object[] values)
								{
									parentResolver.QueueInsertAt(property.Index, values);
								}, new Rect(Event.get_current().get_mousePosition(), Vector2.get_one()));
							});
						});
					}
					genericMenu.AddItem(new GUIContent("Delete element"), false, (MenuFunction)delegate
					{
						property.Tree.DelayActionUntilRepaint(delegate
						{
							parentResolver.QueueRemoveAt(property.Index);
						});
					});
				}
			}
			object[] array = (from x in property.ValueEntry.WeakValues.FilterCast<object>()
				where x != null
				select x).ToArray();
			object valueToCopy = ((array == null || array.Length == 0) ? null : ((array.Length == 1) ? array[0] : array));
			bool flag = property.ValueEntry.BaseValueType.InheritsFrom(typeof(Object));
			bool flag2 = valueToCopy != null;
			bool flag3 = Clipboard.CanPaste(property.ValueEntry.BaseValueType);
			bool isEditable = property.ValueEntry.IsEditable;
			bool flag4 = (property.ValueEntry.BaseValueType.IsClass || property.ValueEntry.BaseValueType.IsInterface) && !property.Info.TypeOfValue.IsValueType && (property.ValueEntry.SerializationBackend.SupportsPolymorphism || flag);
			if (flag3 && isEditable)
			{
				genericMenu.AddItem(new GUIContent("Paste"), false, (MenuFunction)delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						for (int k = 0; k < property.ValueEntry.ValueCount; k++)
						{
							property.ValueEntry.WeakValues[k] = Clipboard.Paste();
						}
						GUIHelper.RequestRepaint();
					});
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Paste"));
			}
			if (flag2)
			{
				if (flag)
				{
					genericMenu.AddItem(new GUIContent("Copy"), false, (MenuFunction)delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.CopyReference);
					});
				}
				else if (!property.ValueEntry.SerializationBackend.SupportsCyclicReferences)
				{
					genericMenu.AddItem(new GUIContent("Copy"), false, (MenuFunction)delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.DeepCopy);
					});
				}
				else
				{
					genericMenu.AddItem(new GUIContent("Copy"), false, (MenuFunction)delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.DeepCopy);
					});
					genericMenu.AddItem(new GUIContent("Copy Special/Deep Copy (default)"), false, (MenuFunction)delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.DeepCopy);
					});
					genericMenu.AddItem(new GUIContent("Copy Special/Shallow Copy"), false, (MenuFunction)delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.ShallowCopy);
					});
					genericMenu.AddItem(new GUIContent("Copy Special/Copy Reference"), false, (MenuFunction)delegate
					{
						Clipboard.Copy(valueToCopy, CopyModes.CopyReference);
					});
				}
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Copy"));
			}
			if (!flag4)
			{
				return;
			}
			genericMenu.AddSeparator("");
			if (flag2 && isEditable)
			{
				genericMenu.AddItem(new GUIContent("Set To Null"), false, (MenuFunction)delegate
				{
					property.Tree.DelayActionUntilRepaint(delegate
					{
						for (int j = 0; j < property.ValueEntry.ValueCount; j++)
						{
							property.ValueEntry.WeakValues[j] = null;
						}
						GUIHelper.RequestRepaint();
					});
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Set To Null"));
			}
		}
	}
	/// <summary>
	/// Opens a context menu for any given property on right click. The context menu is populated by all relevant drawers that implements <see cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" />
	[DrawerPriority(95.0, 0.0, 0.0)]
	public sealed class PropertyContextMenuDrawer<T> : OdinValueDrawer<T>
	{
		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return !property.IsTreeRoot;
		}

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			DisableContextMenuAttribute attribute = base.Property.GetAttribute<DisableContextMenuAttribute>();
			if (attribute != null && attribute.DisableForMember)
			{
				base.SkipWhenDrawing = true;
			}
			else if (base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver)
			{
				base.SkipWhenDrawing = base.Property.Parent.GetAttribute<DisableContextMenuAttribute>()?.DisableForCollectionElements ?? false;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			CallNextDrawer(label);
			int controlID = GUIUtility.GetControlID((FocusType)2);
			if ((int)Event.get_current().get_type() != 8)
			{
				Rect rect = ((base.Property.Parent == null || !(base.Property.Parent.ChildResolver is ICollectionResolver)) ? base.Property.LastDrawnValueRect : GUIHelper.GetCurrentLayoutRect());
				GUIHelper.PushGUIEnabled(enabled: true);
				PropertyContextMenuDrawer.AddRightClickArea(base.Property, rect, controlID);
				GUIHelper.PopGUIEnabled();
			}
		}
	}
}
