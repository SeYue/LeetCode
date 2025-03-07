using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Represents a property in the inspector, and provides the hub for all functionality related to that property.
	/// </summary>
	public sealed class InspectorProperty : IDisposable
	{
		private int maxDrawCount;

		private Stack<int> drawCountStack = new Stack<int>();

		private int lastUpdatedTreeID = -1;

		private string unityPropertyPath;

		private string prefabModificationPath;

		private List<int> drawerChainIndices = new List<int>();

		private List<BakedDrawerChain> drawerChains;

		private readonly List<Attribute> processedAttributes = new List<Attribute>();

		private ImmutableList<Attribute> processedAttributesImmutable;

		private bool? supportsPrefabModifications;

		private List<PropertyComponent> components = new List<PropertyComponent>();

		private ImmutableList<PropertyComponent> componentsImmutable;

		private List<PropertyState> states;

		private List<Rect> lastDrawnValueRects = new List<Rect>();

		private int lastUpdatedStateUpdatersID = -1;

		private StateUpdater[] stateUpdaters;

		private string cachedUnityPropertyListLengthModificationPath;

		public bool AnimateVisibility = true;

		public bool IsTreeRoot => this == Tree.RootProperty;

		/// <summary>
		/// Gets the property which is the ultimate root of this property's serialization.
		/// </summary>
		public InspectorProperty SerializationRoot { get; private set; }

		/// <summary>
		/// The name of the property.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The nice name of the property, usually as converted by <see cref="M:UnityEditor.ObjectNames.NicifyVariableName(System.String)" />.
		/// </summary>
		public string NiceName { get; private set; }

		/// <summary>
		/// The cached label of the property, usually containing <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.NiceName" />.
		/// </summary>
		public GUIContent Label { get; set; }

		/// <summary>
		/// The full Odin path of the property. To get the Unity property path, see <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.UnityPropertyPath" />.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// The child index of this property.
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// Gets the resolver for this property's children.
		/// </summary>
		public OdinPropertyResolver ChildResolver { get; private set; }

		/// <summary>
		/// <para>The current recursive draw depth, incremented for each time that the property has caused itself to be drawn recursively.</para>
		/// <para>Note that this is the <i>current</i> recursion level, not the total amount of recursions so far this frame.</para>
		/// </summary>
		public int RecursiveDrawDepth => drawCountStack.Count;

		/// <summary>
		/// The amount of times that the property has been drawn so far this frame.
		/// </summary>
		public int DrawCount
		{
			get
			{
				if (drawCountStack.Count == 0)
				{
					return maxDrawCount;
				}
				return drawCountStack.Peek();
			}
		}

		/// <summary>
		/// How deep in the drawer chain the property currently is, in the current drawing session as determined by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawCount" />.
		/// </summary>
		public int DrawerChainIndex
		{
			get
			{
				while (drawerChainIndices.Count <= DrawCount)
				{
					drawerChainIndices.Add(0);
				}
				return drawerChainIndices[DrawCount];
			}
		}

		/// <summary>
		/// Whether this property supports having prefab modifications applied or not.
		/// </summary>
		public bool SupportsPrefabModifications
		{
			get
			{
				if (!supportsPrefabModifications.HasValue)
				{
					if (!Tree.PrefabModificationHandler.HasPrefabs)
					{
						supportsPrefabModifications = false;
					}
					else if (Tree.PrefabModificationHandler.HasNestedOdinPrefabData)
					{
						supportsPrefabModifications = false;
					}
					else if (this == Tree.RootProperty)
					{
						supportsPrefabModifications = false;
					}
					else if (ValueEntry == null || (ParentValueProperty != null && !ParentValueProperty.IsTreeRoot && !ParentValueProperty.SupportsPrefabModifications))
					{
						supportsPrefabModifications = false;
					}
					else if (ValueEntry.SerializationBackend == SerializationBackend.None)
					{
						supportsPrefabModifications = false;
					}
					else if (GetAttribute<DoesNotSupportPrefabModificationsAttribute>() != null || Info.GetAttribute<DoesNotSupportPrefabModificationsAttribute>() != null)
					{
						supportsPrefabModifications = false;
					}
					else if (ChildResolver is IMaySupportPrefabModifications)
					{
						supportsPrefabModifications = (ChildResolver as IMaySupportPrefabModifications).MaySupportPrefabModifications;
					}
					else
					{
						supportsPrefabModifications = false;
					}
				}
				return supportsPrefabModifications.Value;
			}
		}

		/// <summary>
		/// Gets an immutable list of the components attached to the property.
		/// </summary>
		public ImmutableList<PropertyComponent> Components
		{
			get
			{
				if (componentsImmutable == null)
				{
					if (components == null)
					{
						CreateComponents();
					}
					componentsImmutable = new ImmutableList<PropertyComponent>(components);
				}
				return componentsImmutable;
			}
		}

		/// <summary>
		/// Gets an immutable list of processed attributes for the property.
		/// </summary>
		public ImmutableList<Attribute> Attributes
		{
			get
			{
				if (processedAttributesImmutable == null)
				{
					processedAttributesImmutable = new ImmutableList<Attribute>(processedAttributes);
				}
				return processedAttributesImmutable;
			}
		}

		/// <summary>
		/// Gets an array of the state updaters of the property. Don't change the contents of this array!
		/// </summary>
		public StateUpdater[] StateUpdaters
		{
			get
			{
				if (stateUpdaters == null)
				{
					GetNewStateUpdaters();
					UpdateStates(Tree.UpdateID);
				}
				return stateUpdaters;
			}
		}

		/// <summary>
		/// The value entry that represents the base value of this property.
		/// </summary>
		public PropertyValueEntry BaseValueEntry { get; private set; }

		/// <summary>
		/// The value entry that represents the strongly typed value of the property; this is possibly an alias entry in case of polymorphism.
		/// </summary>
		public IPropertyValueEntry ValueEntry { get; private set; }

		/// <summary>
		/// The parent of the property. If null, this property is a root-level property in the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" />.
		/// </summary>
		public InspectorProperty Parent { get; private set; }

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.InspectorPropertyInfo" /> of this property.
		/// </summary>
		public InspectorPropertyInfo Info { get; private set; }

		/// <summary>
		/// The <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> that this property exists in.
		/// </summary>
		public PropertyTree Tree { get; private set; }

		/// <summary>
		/// The children of this property.
		/// </summary>
		public PropertyChildren Children { get; private set; }

		/// <summary>
		/// The context container of this property.
		/// </summary>
		public PropertyContextContainer Context { get; private set; }

		/// <summary>
		/// The last rect that this property was drawn within.
		/// </summary>
		public Rect LastDrawnValueRect
		{
			get
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				if (DrawCount <= 0)
				{
					return default(Rect);
				}
				if (DrawCount > lastDrawnValueRects.Count)
				{
					lastDrawnValueRects.SetLength(DrawCount);
				}
				return lastDrawnValueRects[DrawCount - 1];
			}
		}

		/// <summary>
		/// The type on which this property is declared. This is the same as <see cref="P:Sirenix.OdinInspector.Editor.InspectorPropertyInfo.TypeOfOwner" />.
		/// </summary>
		public Type ParentType { get; private set; }

		/// <summary>
		/// The parent values of this property, by selection index; this represents the values that 'own' this property, on which it is declared.
		/// </summary>
		public ImmutableList ParentValues { get; private set; }

		public InspectorProperty ParentValueProperty { get; private set; }

		/// <summary>
		/// <para>The full Unity property path of this property; note that this is merely a converted version of <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, and not necessarily a path to an actual Unity property.</para>
		/// <para>In the case of Odin-serialized data, for example, no Unity properties will exist at this path.</para>
		/// </summary>
		public string UnityPropertyPath
		{
			get
			{
				if (unityPropertyPath == null)
				{
					unityPropertyPath = InspectorUtilities.ConvertToUnityPropertyPath(Path);
				}
				return unityPropertyPath;
			}
		}

		/// <summary>
		/// <para>The full path of this property as used by deep reflection, containing all the necessary information to find this property through reflection only. This is used as the path for prefab modifications.</para>
		/// </summary>
		[Obsolete("Use PrefabModificationPath instead, which serves the exact same function.", false)]
		public string DeepReflectionPath => PrefabModificationPath;

		/// <summary>
		/// <para>The full path of this property as used by prefab modifications and the deep reflection system, containing all the necessary information to find this property through reflection only.</para>
		/// </summary>
		public string PrefabModificationPath
		{
			get
			{
				if (prefabModificationPath == null)
				{
					prefabModificationPath = InspectorUtilities.ConvertToDeepReflectionPath(Path);
				}
				return prefabModificationPath;
			}
		}

		/// <summary>
		/// The PropertyState of the property at the current draw count index.
		/// </summary>
		public PropertyState State
		{
			get
			{
				int num = DrawCount - 1;
				if (num < 0)
				{
					num = 0;
				}
				if (states == null)
				{
					states = new List<PropertyState>();
				}
				while (states.Count <= num)
				{
					states.Add(null);
				}
				PropertyState propertyState = states[num];
				if (propertyState == null)
				{
					propertyState = new PropertyState(this, num);
					states[num] = propertyState;
				}
				return propertyState;
			}
		}

		private InspectorProperty()
		{
		}

		/// <summary>
		/// Gets the component of a given type on the property, or null if the property does not have a component of the given type.
		/// </summary>
		public T GetComponent<T>() where T : PropertyComponent
		{
			if (components == null || components.Count != Tree.ComponentProviders.Count)
			{
				CreateComponents();
			}
			for (int i = 0; i < components.Count; i++)
			{
				T val = components[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		/// <summary>
		/// Marks the property's serialization root values dirty if they are derived from UnityEngine.Object.
		/// </summary>
		public void MarkSerializationRootDirty()
		{
			if (SerializationRoot == null)
			{
				return;
			}
			foreach (object weakValue in SerializationRoot.ValueEntry.WeakValues)
			{
				Object val = weakValue as Object;
				if (val != (Object)null)
				{
					InspectorUtilities.RegisterUnityObjectDirty(val);
				}
			}
		}

		/// <summary>
		/// Records the property's serialization root for undo to prepare for undoable changes, with a custom string that includes the property path and Unity object name. If a message is specified, it is included in the custom undo string.
		/// </summary>
		public void RecordForUndo(string message = null, bool forceCompleteObjectUndo = false)
		{
			if (!Tree.WillUndo)
			{
				return;
			}
			InspectorProperty serializationRoot = SerializationRoot;
			if (serializationRoot == null)
			{
				return;
			}
			if (!forceCompleteObjectUndo && ValueEntry != null && UnityPolymorphicSerializationBackend.SerializeReferenceAttribute != null)
			{
				ImmutableList<Attribute> attributes = Info.Attributes;
				for (int i = 0; i < attributes.Count; i++)
				{
					if (attributes[i].GetType() == UnityPolymorphicSerializationBackend.SerializeReferenceAttribute)
					{
						forceCompleteObjectUndo = true;
						break;
					}
				}
			}
			for (int j = 0; j < serializationRoot.ValueEntry.ValueCount; j++)
			{
				object obj = serializationRoot.ValueEntry.WeakValues[j];
				Object val = obj as Object;
				if (val != (Object)null)
				{
					string text = ((this != Tree.RootProperty) ? ((message == null) ? ("Change " + PrefabModificationPath + " on " + val.get_name()) : ("Change " + PrefabModificationPath + " on " + val.get_name() + ": " + message)) : ((message == null) ? ("Change " + val.get_name()) : ("Change " + val.get_name() + ": " + message)));
					if (forceCompleteObjectUndo)
					{
						Undo.RegisterCompleteObjectUndo(val, text);
					}
					else
					{
						Undo.RecordObject(val, text);
					}
				}
			}
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property.
		/// </summary>
		public T GetAttribute<T>() where T : Attribute
		{
			for (int i = 0; i < processedAttributes.Count; i++)
			{
				T val = processedAttributes[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the first attribute of a given type on this property, which is not contained in a given hashset.
		/// </summary>
		/// <param name="exclude">The attributes to exclude.</param>
		public T GetAttribute<T>(HashSet<Attribute> exclude) where T : Attribute
		{
			for (int i = 0; i < processedAttributes.Count; i++)
			{
				T val = processedAttributes[i] as T;
				if (val != null && (exclude == null || !exclude.Contains(val)))
				{
					return val;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets all attributes of a given type on the property.
		/// </summary>
		public IEnumerable<T> GetAttributes<T>() where T : Attribute
		{
			for (int i = 0; i < processedAttributes.Count; i++)
			{
				T val = processedAttributes[i] as T;
				if (val != null)
				{
					yield return val;
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "InspectorProperty (" + Path + ")";
		}

		public BakedDrawerChain GetActiveDrawerChain()
		{
			bool isNewlyCreated;
			return GetActiveDrawerChain(out isNewlyCreated);
		}

		private BakedDrawerChain GetActiveDrawerChain(out bool isNewlyCreated)
		{
			if (drawerChains == null)
			{
				drawerChains = new List<BakedDrawerChain>();
			}
			int num = DrawCount - 1;
			if (num < 0)
			{
				num = 0;
			}
			BakedDrawerChain bakedDrawerChain;
			if (drawerChains.Count <= num)
			{
				bakedDrawerChain = Tree.DrawerChainResolver.GetDrawerChain(this).Bake();
				drawerChains.Add(bakedDrawerChain);
				for (int i = 0; i < bakedDrawerChain.BakedDrawerArray.Length; i++)
				{
					bakedDrawerChain.BakedDrawerArray[i].Initialize(this);
				}
				isNewlyCreated = true;
			}
			else
			{
				isNewlyCreated = false;
				bakedDrawerChain = drawerChains[num];
			}
			return bakedDrawerChain;
		}

		public void RefreshSetup()
		{
			RefreshSetup(disposeOld: true);
		}

		private void RefreshSetup(bool disposeOld)
		{
			if (disposeOld)
			{
				DisposeExistingSetup();
			}
			if (stateUpdaters != null)
			{
				stateUpdaters = null;
			}
			if (drawerChains != null)
			{
				drawerChains.Clear();
			}
			if (states != null)
			{
				for (int i = 0; i < states.Count; i++)
				{
					if (states[i] != null)
					{
						states[i].Reset();
					}
				}
			}
			if (components == null || components.Count != Tree.ComponentProviders.Count)
			{
				CreateComponents();
			}
			else
			{
				for (int j = 0; j < components.Count; j++)
				{
					components[j].Reset();
				}
			}
			RefreshProcessedAttributes();
			ChildResolver = Tree.PropertyResolverLocator.GetResolver(this);
			Children = new PropertyChildren(this);
			Children.Update();
			GetNewStateUpdaters();
			UpdateStates(Tree.UpdateID);
		}

		private void CreateComponents()
		{
			if (components == null)
			{
				components = new List<PropertyComponent>(Tree.ComponentProviders.Count);
			}
			else
			{
				for (int i = 0; i < components.Count; i++)
				{
					IDisposable disposable = components[i] as IDisposable;
					if (disposable != null)
					{
						try
						{
							disposable.Dispose();
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
				components.Clear();
			}
			for (int j = 0; j < Tree.ComponentProviders.Count; j++)
			{
				components.Add(Tree.ComponentProviders[j].CreateComponent(this));
			}
		}

		private void RefreshProcessedAttributes()
		{
			processedAttributes.Clear();
			for (int i = 0; i < Info.Attributes.Count; i++)
			{
				processedAttributes.Add(Info.Attributes[i]);
			}
			List<OdinAttributeProcessor> selfProcessors = Tree.AttributeProcessorLocator.GetSelfProcessors(this);
			for (int j = 0; j < selfProcessors.Count; j++)
			{
				try
				{
					selfProcessors[j].ProcessSelfAttributes(this, processedAttributes);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		internal void OnStateUpdate(int treeID)
		{
			Update();
			UpdateStates(treeID);
			PropertyChildren.ExistingChildEnumerator enumerator = Children.GetExistingChildren().GetEnumerator();
			while (enumerator.MoveNext())
			{
				InspectorProperty current = enumerator.Current;
				current.OnStateUpdate(treeID);
			}
		}

		/// <summary>
		/// Draws this property in the inspector.
		/// </summary>
		public void Draw()
		{
			Draw(Label);
		}

		/// <summary>
		/// Draws this property in the inspector with a given default label. This default label may be overridden by attributes on the drawn property.
		/// </summary>
		public void Draw(GUIContent defaultLabel)
		{
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Invalid comparison between Unknown and I4
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Invalid comparison between Unknown and I4
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Invalid comparison between Unknown and I4
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Invalid comparison between Unknown and I4
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0335: Unknown result type (might be due to invalid IL or missing references)
			//IL_033a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0394: Unknown result type (might be due to invalid IL or missing references)
			//IL_0396: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0559: Unknown result type (might be due to invalid IL or missing references)
			Update();
			bool flag = false;
			bool flag2 = true;
			try
			{
				PushDraw();
				BakedDrawerChain activeDrawerChain = GetActiveDrawerChain();
				PropertyState state = State;
				bool animateVisibility = AnimateVisibility;
				if (!(animateVisibility ? SirenixEditorGUI.BeginFadeGroup(state, state.VisibleLastLayout) : state.VisibleLastLayout))
				{
					goto IL_0563;
				}
				if (RecursiveDrawDepth + InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth > GlobalConfig<GeneralDrawerConfig>.Instance.MaxRecursiveDrawDepth)
				{
					SirenixEditorGUI.ErrorMessageBox("The property '" + NiceName + "' has exceeded the maximum recursive draw depth limit of " + GlobalConfig<GeneralDrawerConfig>.Instance.MaxRecursiveDrawDepth + ".");
					return;
				}
				if (!IsTreeRoot && ValueEntry != null && ValueEntry.SerializationBackend == SerializationBackend.Odin && !SupportsPrefabModifications && Tree.PrefabModificationHandler.HasPrefabs && !GUIHelper.IsDrawingDictionaryKey && Info.PropertyType == PropertyType.Value)
				{
					if (ParentValueProperty != null && (ParentValueProperty.IsTreeRoot || ParentValueProperty.SupportsPrefabModifications) && GlobalConfig<GeneralDrawerConfig>.Instance.ShowPrefabModificationsDisabledMessage)
					{
						string text = (Tree.PrefabModificationHandler.HasNestedOdinPrefabData ? "this instance" : "prefab instances");
						SirenixEditorGUI.InfoMessageBox("The property '" + NiceName + "' does not support being modified on " + text + ". (You can disable this message in the general drawer config.)");
					}
					GUIHelper.PushGUIEnabled(enabled: false);
					flag = true;
				}
				activeDrawerChain.Reset();
				EventType type = Event.get_current().get_type();
				int drawCount = DrawCount;
				Rect val;
				int num;
				if ((int)type != 7)
				{
					if ((int)type != 8)
					{
						val = LastDrawnValueRect;
						num = ((((Rect)(ref val)).get_height() == 0f) ? 1 : 0);
					}
					else
					{
						num = 0;
					}
				}
				else
				{
					num = 1;
				}
				bool flag3 = (byte)num != 0;
				if (flag3)
				{
					GUIHelper.BeginLayoutMeasuring();
				}
				try
				{
					if (stateUpdaters != null)
					{
						for (int i = 0; i < stateUpdaters.Length; i++)
						{
							StateUpdater stateUpdater = stateUpdaters[i];
							if (stateUpdater.ErrorMessage != null)
							{
								SirenixEditorGUI.ErrorMessageBox("Error in state updater '" + stateUpdater.GetType().GetNiceName() + "':\n\n" + stateUpdater.ErrorMessage);
							}
						}
					}
					if (activeDrawerChain.MoveNext())
					{
						bool flag4 = ValueEntry != null && (int)type == 7;
						bool flag5 = false;
						if (flag4)
						{
							flag5 = ValueEntry.ValueChangedFromPrefab;
							bool flag6 = flag5;
							if (GUIHelper.IsDrawingDictionaryKey)
							{
								flag6 |= GUIHelper.IsBoldLabel;
							}
							GUIHelper.PushIsBoldLabel(flag6);
						}
						bool flag7 = ValueEntry != null && !ValueEntry.IsEditable;
						flag7 |= !state.EnabledLastLayout;
						if (flag7)
						{
							GUIHelper.PushGUIEnabled(enabled: false);
						}
						activeDrawerChain.Current.DrawProperty(defaultLabel);
						if (flag7)
						{
							GUIHelper.PopGUIEnabled();
						}
						if (flag4)
						{
							GUIHelper.PopIsBoldLabel();
						}
						if (flag5 && (int)type == 7 && GlobalConfig<GeneralDrawerConfig>.Instance.ShowPrefabModifiedValueBar)
						{
							Rect lastDrawnValueRect = LastDrawnValueRect;
							val = default(Rect);
							if (lastDrawnValueRect != val)
							{
								Color color = default(Color);
								((Color)(ref color))._002Ector(0.003921569f, 0.6f, 0.9215686f, 0.75f);
								Rect lastDrawnValueRect2 = LastDrawnValueRect;
								((Rect)(ref lastDrawnValueRect2)).set_width(2f);
								((Rect)(ref lastDrawnValueRect2)).set_x(((Rect)(ref lastDrawnValueRect2)).get_x() - 2.5f);
								((Rect)(ref lastDrawnValueRect2)).set_x(((Rect)(ref lastDrawnValueRect2)).get_x() + GUIHelper.CurrentIndentAmount);
								if (ChildResolver is ICollectionResolver)
								{
									((Rect)(ref lastDrawnValueRect2)).set_height(((Rect)(ref lastDrawnValueRect2)).get_height() - 3.5f);
								}
								GUIHelper.PushGUIEnabled(enabled: true);
								SirenixEditorGUI.DrawSolidRect(lastDrawnValueRect2, color);
								GUIHelper.PopGUIEnabled();
							}
						}
					}
					else if (Info.PropertyType == PropertyType.Method)
					{
						EditorGUILayout.LabelField(NiceName, "No drawers could be found for the method property '" + Name + "'.", (GUILayoutOption[])(object)new GUILayoutOption[0]);
					}
					else if (Info.PropertyType == PropertyType.Group)
					{
						PropertyGroupAttribute propertyGroupAttribute = GetAttribute<PropertyGroupAttribute>() ?? Info.GetAttribute<PropertyGroupAttribute>();
						if (propertyGroupAttribute != null)
						{
							EditorGUILayout.LabelField(NiceName, "No drawers could be found for the property group '" + Name + "' with property group attribute type '" + propertyGroupAttribute.GetType().GetNiceName() + "'.", (GUILayoutOption[])(object)new GUILayoutOption[0]);
						}
						else
						{
							EditorGUILayout.LabelField(NiceName, "No drawers could be found for the property group '" + Name + "'.", (GUILayoutOption[])(object)new GUILayoutOption[0]);
						}
					}
				}
				catch (Exception ex)
				{
					if (ex.IsExitGUIException())
					{
						flag2 = false;
						throw ex.AsExitGUIException();
					}
					string message = string.Concat("This error occurred while being drawn by Odin. \nCurrent IMGUI event: ", Event.get_current().get_type(), "\nOdin Property Path: ", Path, "\nOdin Drawer Chain:\n", string.Join("\n", activeDrawerChain.BakedDrawerArray.Select((OdinDrawer n) => " > " + n.GetType().GetNiceName()).ToArray()), ".");
					Debug.LogException((Exception)new OdinPropertyException(message, ex));
				}
				if (flag3)
				{
					if (drawCount > lastDrawnValueRects.Count)
					{
						lastDrawnValueRects.SetLength(drawCount);
					}
					lastDrawnValueRects[drawCount - 1] = GUIHelper.EndLayoutMeasuring();
				}
				goto IL_0563;
				IL_0563:
				if (animateVisibility)
				{
					SirenixEditorGUI.EndFadeGroup();
				}
			}
			finally
			{
				if (flag2)
				{
					PopDraw();
				}
				if (flag)
				{
					GUIHelper.PopGUIEnabled();
				}
			}
		}

		/// <summary>
		/// Push a draw session. This is used by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawCount" /> and <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.RecursiveDrawDepth" />.
		/// </summary>
		public void PushDraw()
		{
			maxDrawCount++;
			drawCountStack.Push(maxDrawCount);
		}

		/// <summary>
		/// Increments the current drawer chain index. This is used by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawerChainIndex" />.
		/// </summary>
		public void IncrementDrawerChainIndex()
		{
			while (drawerChainIndices.Count <= DrawCount)
			{
				drawerChainIndices.Add(0);
			}
			drawerChainIndices[DrawCount]++;
		}

		/// <summary>
		/// Pop a draw session. This is used by <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.DrawCount" /> and <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.RecursiveDrawDepth" />.
		/// </summary>
		public void PopDraw()
		{
			drawCountStack.Pop();
		}

		public bool IsReachableFromRoot()
		{
			bool flag = false;
			try
			{
				if (Parent == null)
				{
					InspectorProperty rootProperty = Tree.RootProperty;
					flag = this == rootProperty || rootProperty.Children[Name] == this;
					return flag;
				}
				if (!Parent.IsReachableFromRoot())
				{
					return false;
				}
				flag = Parent.Children[Name] == this;
				return flag;
			}
			finally
			{
				if (flag)
				{
					Update();
				}
			}
		}

		/// <summary>
		/// Gets the next property in the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" />, or null if none is found.
		/// </summary>
		/// <param name="includeChildren">Whether to include children or not.</param>
		/// <param name="visibleOnly">Whether to only include visible properties.</param>
		public InspectorProperty NextProperty(bool includeChildren = true, bool visibleOnly = false)
		{
			if (includeChildren)
			{
				if (visibleOnly)
				{
					for (int i = 0; i < Children.Count; i++)
					{
						InspectorProperty inspectorProperty = Children[i];
						if (inspectorProperty.State.Visible)
						{
							return inspectorProperty;
						}
					}
				}
				else if (Children.Count > 0)
				{
					return Children.Get(0);
				}
			}
			InspectorProperty inspectorProperty2 = null;
			InspectorProperty inspectorProperty3 = this;
			InspectorProperty rootProperty = Tree.RootProperty;
			while (true)
			{
				inspectorProperty2 = inspectorProperty3;
				inspectorProperty3 = inspectorProperty3.Parent;
				if (inspectorProperty3 != null && inspectorProperty3 != rootProperty && inspectorProperty2.Index + 1 >= inspectorProperty2.Parent.Children.Count)
				{
					continue;
				}
				if (inspectorProperty3 == null)
				{
					break;
				}
				if (visibleOnly)
				{
					for (int j = inspectorProperty2.Index + 1; j < inspectorProperty3.Children.Count; j++)
					{
						InspectorProperty inspectorProperty4 = inspectorProperty3.Children[j];
						if (inspectorProperty4.State.Visible)
						{
							return inspectorProperty4;
						}
					}
				}
				else if (inspectorProperty2.Index + 1 < inspectorProperty3.Children.Count)
				{
					return inspectorProperty3.Children[inspectorProperty2.Index + 1];
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the first parent property that matches a given predicate.
		/// </summary>
		public InspectorProperty FindParent(Func<InspectorProperty, bool> predicate, bool includeSelf)
		{
			for (InspectorProperty inspectorProperty = (includeSelf ? this : Parent); inspectorProperty != null; inspectorProperty = inspectorProperty.Parent)
			{
				if (predicate(inspectorProperty))
				{
					return inspectorProperty;
				}
			}
			return null;
		}

		/// <summary>
		/// Finds the first child recursively, that matches a given predicate.
		/// </summary>
		public InspectorProperty FindChild(Func<InspectorProperty, bool> predicate, bool includeSelf)
		{
			if (includeSelf && predicate(this))
			{
				return this;
			}
			return Children.Recurse().FirstOrDefault(predicate);
		}

		internal void ClearDrawCount()
		{
			maxDrawCount = 0;
			drawCountStack.Clear();
			for (int i = 0; i < drawerChainIndices.Count; i++)
			{
				drawerChainIndices[i] = 0;
			}
		}

		/// <summary>
		/// Updates the property. This method resets the temporary context, and updates the value entry and the property children.
		/// </summary>
		/// <param name="forceUpdate">If true, the property will update regardless of whether it has already updated for the current <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.UpdateID" />.</param>
		public bool Update(bool forceUpdate = false)
		{
			bool flag = Tree.UpdateID != lastUpdatedTreeID;
			if (!forceUpdate && !flag)
			{
				return false;
			}
			if (flag)
			{
				ClearDrawCount();
			}
			lastUpdatedTreeID = Tree.UpdateID;
			UpdateValueEntry();
			if (stateUpdaters == null || Children == null || ChildResolver == null)
			{
				RefreshSetup(disposeOld: false);
			}
			else if (ValueEntry != null && ChildResolver.ResolverForType != null && ValueEntry.TypeOfValue != ChildResolver.ResolverForType)
			{
				RefreshSetup(disposeOld: true);
			}
			else
			{
				Children.Update();
			}
			if (ValueEntry != null)
			{
				if (ValueEntry.SerializationBackend == SerializationBackend.Odin)
				{
					PrefabModificationType? prefabModificationType = Tree.PrefabModificationHandler.GetPrefabModificationType(this);
					BaseValueEntry.ValueChangedFromPrefab = prefabModificationType == PrefabModificationType.Value;
					BaseValueEntry.ListLengthChangedFromPrefab = prefabModificationType == PrefabModificationType.ListLength;
					BaseValueEntry.DictionaryChangedFromPrefab = prefabModificationType == PrefabModificationType.Dictionary;
				}
				else if (ValueEntry.SerializationBackend.IsUnity)
				{
					int count = ParentValues.Count;
					BaseValueEntry.ValueChangedFromPrefab = false;
					BaseValueEntry.ListLengthChangedFromPrefab = false;
					bool childrenHaveModifications;
					if (ChildResolver.IsCollection)
					{
						if (cachedUnityPropertyListLengthModificationPath == null)
						{
							cachedUnityPropertyListLengthModificationPath = UnityPropertyPath + ".Array.size";
						}
						for (int i = 0; i < count; i++)
						{
							PropertyModification unityPropertyModification = Tree.PrefabModificationHandler.GetUnityPropertyModification(cachedUnityPropertyListLengthModificationPath, i, out childrenHaveModifications);
							if (unityPropertyModification != null)
							{
								BaseValueEntry.ListLengthChangedFromPrefab = true;
								break;
							}
						}
					}
					for (int j = 0; j < count; j++)
					{
						PropertyModification unityPropertyModification = Tree.PrefabModificationHandler.GetUnityPropertyModification(UnityPropertyPath, j, out childrenHaveModifications);
						if (unityPropertyModification != null || (ValueEntry.IsMarkedAtomic && childrenHaveModifications))
						{
							BaseValueEntry.ValueChangedFromPrefab = true;
							break;
						}
					}
				}
			}
			UpdateStates(lastUpdatedTreeID);
			return true;
		}

		private void UpdateStates(int treeID)
		{
			if (stateUpdaters == null)
			{
				GetNewStateUpdaters();
			}
			if (lastUpdatedStateUpdatersID == treeID)
			{
				return;
			}
			lastUpdatedStateUpdatersID = treeID;
			for (int i = 0; i < stateUpdaters.Length; i++)
			{
				try
				{
					stateUpdaters[i].OnStateUpdate();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			if (states == null)
			{
				return;
			}
			for (int j = 0; j < states.Count; j++)
			{
				if (states[j] != null)
				{
					states[j].Update();
				}
			}
		}

		private void GetNewStateUpdaters()
		{
			stateUpdaters = Tree.StateUpdaterLocator.GetStateUpdaters(this);
			for (int i = 0; i < stateUpdaters.Length; i++)
			{
				try
				{
					stateUpdaters[i].Initialize(this);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		/// <summary>
		/// Populates a generic menu with items from all drawers for this property that implement <see cref="T:Sirenix.OdinInspector.Editor.IDefinesGenericMenuItems" />.
		/// </summary>
		public void PopulateGenericMenu(GenericMenu genericMenu)
		{
			if (genericMenu == null)
			{
				throw new ArgumentNullException("genericMenu");
			}
			OdinDrawer[] bakedDrawerArray = GetActiveDrawerChain().BakedDrawerArray;
			int drawCount = DrawCount;
			int drawerChainIndex = DrawerChainIndex;
			try
			{
				for (int i = 0; i < bakedDrawerArray.Length; i++)
				{
					IDefinesGenericMenuItems definesGenericMenuItems = bakedDrawerArray[i] as IDefinesGenericMenuItems;
					if (definesGenericMenuItems != null)
					{
						drawerChainIndices[drawCount] = i + 1;
						definesGenericMenuItems.PopulateGenericMenu(this, genericMenu);
					}
				}
			}
			finally
			{
				drawerChainIndices[drawCount] = drawerChainIndex;
			}
		}

		/// <summary>
		/// Determines whether this property is the child of another property in the hierarchy.
		/// </summary>
		/// <param name="other">The property to check whether this property is the child of.</param>
		/// <exception cref="T:System.ArgumentNullException">other is null</exception>
		public bool IsChildOf(InspectorProperty other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			for (InspectorProperty parent = Parent; parent != null; parent = parent.Parent)
			{
				if (parent == other)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Determines whether this property is a parent of another property in the hierarchy.
		/// </summary>
		/// <param name="other">The property to check whether this property is the parent of.</param>
		/// <exception cref="T:System.ArgumentNullException">other is null</exception>
		public bool IsParentOf(InspectorProperty other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			for (InspectorProperty parent = other.Parent; parent != null; parent = parent.Parent)
			{
				if (parent == this)
				{
					return true;
				}
			}
			return false;
		}

		internal static InspectorProperty Create(PropertyTree tree, InspectorProperty parent, InspectorPropertyInfo info, int index, bool isRoot)
		{
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Expected O, but got Unknown
			if (tree == null)
			{
				throw new ArgumentNullException("tree");
			}
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			if (parent != null)
			{
				if (tree != parent.Tree)
				{
					throw new ArgumentException("The given tree and the given parent's tree are not the same tree.");
				}
				if (index < 0 || index >= parent.Children.Count)
				{
					throw new IndexOutOfRangeException("The given index for the property to create is out of bounds.");
				}
			}
			else if (!isRoot)
			{
				throw new ArgumentException("The property to be created has been given no parent, and is not the tree root.");
			}
			InspectorProperty inspectorProperty = new InspectorProperty();
			inspectorProperty.Tree = tree;
			inspectorProperty.Info = info;
			inspectorProperty.Parent = parent;
			inspectorProperty.Index = index;
			inspectorProperty.Context = new PropertyContextContainer(inspectorProperty);
			if (parent != null)
			{
				inspectorProperty.Path = parent.Children.GetPath(index);
			}
			else
			{
				inspectorProperty.Path = info.PropertyName;
			}
			if (inspectorProperty.Path == null)
			{
				Debug.Log((object)("Property path is null for property " + ObjectNames.NicifyVariableName(info.PropertyName.TrimStart('#', '$')) + "!"));
			}
			if (parent != null)
			{
				InspectorProperty inspectorProperty2 = inspectorProperty;
				do
				{
					inspectorProperty2 = inspectorProperty2.Parent;
				}
				while (inspectorProperty2 != null && inspectorProperty2.BaseValueEntry == null);
				inspectorProperty.ParentValueProperty = inspectorProperty2;
			}
			if (inspectorProperty.ParentValueProperty != null)
			{
				inspectorProperty.ParentType = inspectorProperty.ParentValueProperty.ValueEntry.TypeOfValue;
				inspectorProperty.ParentValues = new ImmutableList(inspectorProperty.ParentValueProperty.ValueEntry.WeakValues);
			}
			else
			{
				inspectorProperty.ParentType = tree.TargetType;
				inspectorProperty.ParentValues = new ImmutableList(tree.WeakTargets);
			}
			InspectorProperty parentValueProperty = inspectorProperty.ParentValueProperty;
			while (parentValueProperty != null && !parentValueProperty.ValueEntry.TypeOfValue.InheritsFrom(typeof(Object)))
			{
				parentValueProperty = parentValueProperty.ParentValueProperty;
			}
			if (parentValueProperty != null)
			{
				inspectorProperty.SerializationRoot = parentValueProperty;
			}
			else
			{
				inspectorProperty.SerializationRoot = (isRoot ? inspectorProperty : tree.RootProperty);
			}
			inspectorProperty.Name = info.PropertyName;
			MethodInfo methodInfo = inspectorProperty.Info.GetMemberInfo() as MethodInfo;
			if (methodInfo != null)
			{
				string text = inspectorProperty.Name;
				int num = text.IndexOf('(');
				if (num >= 0)
				{
					text = text.Substring(0, num);
				}
				inspectorProperty.NiceName = text.TrimStart('#', '$').SplitPascalCase();
			}
			else
			{
				inspectorProperty.NiceName = ObjectNames.NicifyVariableName(inspectorProperty.Name.TrimStart('#', '$'));
			}
			inspectorProperty.Label = new GUIContent(inspectorProperty.NiceName);
			if (inspectorProperty.Info.PropertyType == PropertyType.Value)
			{
				inspectorProperty.BaseValueEntry = PropertyValueEntry.Create(inspectorProperty, info.TypeOfValue, isRoot);
				inspectorProperty.ValueEntry = inspectorProperty.BaseValueEntry;
			}
			inspectorProperty.CreateComponents();
			if (!isRoot)
			{
				inspectorProperty.RefreshProcessedAttributes();
				inspectorProperty.ChildResolver = tree.PropertyResolverLocator.GetResolver(inspectorProperty);
				inspectorProperty.Children = new PropertyChildren(inspectorProperty);
			}
			return inspectorProperty;
		}

		private void UpdateValueEntry()
		{
			if (Info.PropertyType != 0)
			{
				if (ValueEntry != null || BaseValueEntry != null)
				{
					ValueEntry = null;
					BaseValueEntry = null;
					RefreshSetup();
				}
				return;
			}
			BaseValueEntry.Update();
			if (!Info.TypeOfValue.IsValueType)
			{
				Type typeOfValue = BaseValueEntry.TypeOfValue;
				if (typeOfValue != BaseValueEntry.BaseValueType)
				{
					if (ValueEntry == null || (ValueEntry.IsAlias && ValueEntry.TypeOfValue != typeOfValue) || (!ValueEntry.IsAlias && ValueEntry.TypeOfValue != ValueEntry.BaseValueType))
					{
						DisposeExistingSetup();
						ValueEntry = PropertyValueEntry.CreateAlias(BaseValueEntry, typeOfValue);
						RefreshSetup(disposeOld: false);
					}
				}
				else if (ValueEntry != BaseValueEntry)
				{
					DisposeExistingSetup();
					ValueEntry = BaseValueEntry;
					RefreshSetup(disposeOld: false);
				}
			}
			else if (ValueEntry == null)
			{
				DisposeExistingSetup();
				ValueEntry = BaseValueEntry;
				RefreshSetup(disposeOld: false);
			}
			if (ValueEntry != BaseValueEntry)
			{
				ValueEntry.Update();
			}
		}

		public void Dispose()
		{
			DisposeExistingSetup();
		}

		public void CleanForCachedReuse()
		{
			PropertyChildren.ExistingChildEnumerator enumerator = Children.GetExistingChildren().GetEnumerator();
			while (enumerator.MoveNext())
			{
				InspectorProperty current = enumerator.Current;
				current.CleanForCachedReuse();
			}
			if (drawerChains != null)
			{
				foreach (BakedDrawerChain drawerChain in drawerChains)
				{
					OdinDrawer[] bakedDrawerArray = drawerChain.BakedDrawerArray;
					foreach (OdinDrawer odinDrawer in bakedDrawerArray)
					{
						IDisposable disposable = odinDrawer as IDisposable;
						if (disposable != null)
						{
							try
							{
								disposable.Dispose();
							}
							catch (Exception ex)
							{
								Debug.LogException(ex);
							}
						}
					}
				}
				drawerChains.Clear();
			}
			if (stateUpdaters != null)
			{
				for (int j = 0; j < stateUpdaters.Length; j++)
				{
					IDisposable disposable2 = stateUpdaters[j] as IDisposable;
					if (disposable2 != null)
					{
						try
						{
							disposable2.Dispose();
						}
						catch (Exception ex2)
						{
							Debug.LogException(ex2);
						}
					}
				}
				stateUpdaters = null;
			}
			if (components != null)
			{
				for (int k = 0; k < components.Count; k++)
				{
					IDisposable disposable3 = components[k] as IDisposable;
					if (disposable3 != null)
					{
						try
						{
							disposable3.Dispose();
						}
						catch (Exception ex3)
						{
							Debug.LogException(ex3);
						}
					}
				}
			}
			if (states != null)
			{
				for (int l = 0; l < states.Count; l++)
				{
					states[l]?.CleanForCachedReuse();
				}
			}
			components = null;
			componentsImmutable = null;
		}

		private void DisposeExistingSetup()
		{
			if (drawerChains != null)
			{
				foreach (BakedDrawerChain drawerChain in drawerChains)
				{
					OdinDrawer[] bakedDrawerArray = drawerChain.BakedDrawerArray;
					foreach (OdinDrawer odinDrawer in bakedDrawerArray)
					{
						IDisposable disposable = odinDrawer as IDisposable;
						if (disposable != null)
						{
							try
							{
								disposable.Dispose();
							}
							catch (Exception ex)
							{
								Debug.LogException(ex);
							}
						}
					}
				}
				drawerChains.Clear();
			}
			if (stateUpdaters != null)
			{
				for (int j = 0; j < stateUpdaters.Length; j++)
				{
					IDisposable disposable2 = stateUpdaters[j] as IDisposable;
					if (disposable2 != null)
					{
						try
						{
							disposable2.Dispose();
						}
						catch (Exception ex2)
						{
							Debug.LogException(ex2);
						}
					}
				}
				stateUpdaters = null;
			}
			if (components != null)
			{
				for (int k = 0; k < components.Count; k++)
				{
					IDisposable disposable3 = components[k] as IDisposable;
					if (disposable3 != null)
					{
						try
						{
							disposable3.Dispose();
						}
						catch (Exception ex3)
						{
							Debug.LogException(ex3);
						}
					}
				}
				components.Clear();
			}
			if (ChildResolver is IDisposable)
			{
				try
				{
					(ChildResolver as IDisposable).Dispose();
				}
				catch (Exception ex4)
				{
					Debug.LogException(ex4);
				}
			}
			if (ValueEntry != null)
			{
				try
				{
					ValueEntry.Dispose();
				}
				catch (Exception ex5)
				{
					Debug.LogException(ex5);
				}
			}
			if (Children != null)
			{
				PropertyChildren.ExistingChildEnumerator enumerator2 = Children.GetExistingChildren().GetEnumerator();
				while (enumerator2.MoveNext())
				{
					InspectorProperty current2 = enumerator2.Current;
					try
					{
						current2.Dispose();
					}
					catch (Exception ex6)
					{
						Debug.LogException(ex6);
					}
				}
			}
			Tree.NotifyPropertyDisposed(this);
		}

		private static InspectorProperty PropertyQueryLookup(InspectorProperty context, string path)
		{
			InspectorProperty inspectorProperty = context.ParentValueProperty;
			while (inspectorProperty != null && !inspectorProperty.Info.HasBackingMembers)
			{
				inspectorProperty = inspectorProperty.ParentValueProperty;
			}
			if (inspectorProperty == null)
			{
				inspectorProperty = context.Tree.RootProperty;
			}
			InspectorProperty inspectorProperty2 = inspectorProperty.Children[path];
			if (inspectorProperty2 == null)
			{
				inspectorProperty2 = ((inspectorProperty != context.Tree.RootProperty) ? context.Tree.GetPropertyAtPath(inspectorProperty.Path + "." + path) : context.Tree.GetPropertyAtPath(path));
			}
			if (inspectorProperty2 == null)
			{
				throw new Exception("Property query could not find the property '" + path + "' in the context of the property '" + context.NiceName + "'.");
			}
			if (Event.get_current() != null)
			{
				inspectorProperty2.GetActiveDrawerChain();
			}
			return inspectorProperty2;
		}
	}
}
