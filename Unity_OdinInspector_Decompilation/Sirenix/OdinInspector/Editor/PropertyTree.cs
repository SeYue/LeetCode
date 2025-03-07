using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Represents a set of values of the same type as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
	/// </summary>
	public abstract class PropertyTree : IDisposable
	{
		/// <summary>
		/// Delegate for on property value changed callback.
		/// </summary>
		public delegate void OnPropertyValueChangedDelegate(InspectorProperty property, int selectionIndex);

		private static GUIFrameCounter frameCounter;

		private static int drawnInspectorDepthCount;

		private static ValueGetter<SerializedObject, IntPtr> SerializedObject_nativeObjectPtrGetter;

		private MethodInfo onValidateMethod;

		private OdinAttributeProcessorLocator attributeProcessorLocator;

		private OdinPropertyResolverLocator propertyResolverLocator;

		private DrawerChainResolver drawerChainResolver;

		private StateUpdaterLocator stateUpdaterLocator;

		internal float ContextWidth;

		internal bool WillUndo;

		internal EditorTimeHelper timeHelper = new EditorTimeHelper();

		internal EditorTimeHelper prevTimeHelper;

		protected SerializedProperty monoScriptProperty;

		protected bool monoScriptPropertyHasBeenGotten;

		private PropertySearchFilter searchFilter;

		public bool AllowSearchFiltering = true;

		/// <summary>
		/// The component providers that create components for each property in the tree. If you change this list after the tree has been used, you should call tree.RootProperty.RefreshSetup() to make the changes update properly throughout the tree.
		/// </summary>
		public readonly List<ComponentProvider> ComponentProviders = new List<ComponentProvider>();

		private volatile bool disposedValue;

		/// <summary>
		/// The <see cref="T:UnityEditor.SerializedObject" /> that this tree represents, if the tree was created for a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		public abstract SerializedObject UnitySerializedObject { get; }

		/// <summary>
		/// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="M:Sirenix.OdinInspector.Editor.InspectorProperty.Update(System.Boolean)" /> to avoid updating multiple times in the same update round.
		/// </summary>
		public abstract int UpdateID { get; }

		/// <summary>
		/// The type of the values that the property tree represents.
		/// </summary>
		public abstract Type TargetType { get; }

		/// <summary>
		/// The actual values that the property tree represents.
		/// </summary>
		public abstract ImmutableList<object> WeakTargets { get; }

		/// <summary>
		/// The number of root properties in the tree.
		/// </summary>
		public abstract int RootPropertyCount { get; }

		/// <summary>
		/// The prefab modification handler of the tree.
		/// </summary>
		public abstract PrefabModificationHandler PrefabModificationHandler { get; }

		/// <summary>
		/// Whether this property tree also represents members that are specially serialized by Odin.
		/// </summary>
		[Obsolete("This value is no longer guaranteed to be correct, as it may have different answers for different properties in the tree. Instead look at InspectorProperty.SerializationRoot to determine whether specially serialized members might be included.", true)]
		public abstract bool IncludesSpeciallySerializedMembers { get; }

		/// <summary>
		/// Gets a value indicating whether or not to draw the mono script object field at the top of the property tree.
		/// </summary>
		public bool DrawMonoScriptObjectField { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not the PropertyTree is inspecting a static type.
		/// </summary>
		public bool IsStatic { get; protected set; }

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.OdinAttributeProcessorLocator" /> for the PropertyTree.
		/// </summary>
		public OdinAttributeProcessorLocator AttributeProcessorLocator
		{
			get
			{
				if (attributeProcessorLocator == null)
				{
					attributeProcessorLocator = DefaultOdinAttributeProcessorLocator.Instance;
				}
				return attributeProcessorLocator;
			}
			set
			{
				if (attributeProcessorLocator != value)
				{
					attributeProcessorLocator = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.OdinPropertyResolverLocator" /> for the PropertyTree.
		/// </summary>
		public OdinPropertyResolverLocator PropertyResolverLocator
		{
			get
			{
				if (propertyResolverLocator == null)
				{
					propertyResolverLocator = DefaultOdinPropertyResolverLocator.Instance;
				}
				return propertyResolverLocator;
			}
			set
			{
				if (propertyResolverLocator != value)
				{
					propertyResolverLocator = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.DrawerChainResolver" /> for the PropertyTree.
		/// </summary>
		public DrawerChainResolver DrawerChainResolver
		{
			get
			{
				if (drawerChainResolver == null)
				{
					drawerChainResolver = DefaultDrawerChainResolver.Instance;
				}
				return drawerChainResolver;
			}
			set
			{
				if (drawerChainResolver != value)
				{
					drawerChainResolver = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="T:Sirenix.OdinInspector.Editor.StateUpdaterLocator" /> for the PropertyTree.
		/// </summary>
		public StateUpdaterLocator StateUpdaterLocator
		{
			get
			{
				if (stateUpdaterLocator == null)
				{
					stateUpdaterLocator = DefaultStateUpdaterLocator.Instance;
				}
				return stateUpdaterLocator;
			}
			set
			{
				if (stateUpdaterLocator != value)
				{
					stateUpdaterLocator = value;
					RootProperty.RefreshSetup();
				}
			}
		}

		/// <summary>
		/// Gets the root property of the tree.
		/// </summary>
		public abstract InspectorProperty RootProperty { get; }

		/// <summary>
		/// Gets the secret root property of the tree, which hosts the property resolver used to resolve the "actual" root properties of the tree.
		/// </summary>
		[Obsolete("Use RootProperty instead; the root is no longer considered 'secret'.", false)]
		public abstract InspectorProperty SecretRootProperty { get; }

		/// <summary>
		/// An event that is invoked whenever an undo or a redo is performed in the inspector.
		/// The advantage of using this event on a property tree instance instead of
		/// <see cref="F:UnityEditor.Undo.undoRedoPerformed" /> is that this event will be desubscribed from
		/// <see cref="F:UnityEditor.Undo.undoRedoPerformed" /> when the selection changes and the property
		/// tree is no longer being used, allowing the GC to collect the property tree.
		/// </summary>
		public event Action OnUndoRedoPerformed;

		/// <summary>
		/// This event is invoked whenever the value of any property in the entire property tree is changed through the property system.
		/// </summary>
		public event OnPropertyValueChangedDelegate OnPropertyValueChanged;

		static PropertyTree()
		{
			frameCounter = new GUIFrameCounter();
			drawnInspectorDepthCount = 0;
			string name = (UnityVersion.IsVersionOrGreater(2018, 3) ? "m_NativeObjectPtr" : "m_Property");
			FieldInfo field = typeof(SerializedObject).GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				SerializedObject_nativeObjectPtrGetter = EmitUtilities.CreateInstanceFieldGetter<SerializedObject, IntPtr>(field);
			}
			else
			{
				Debug.LogWarning((object)"The internal Unity field SerializedObject.m_Property (< 2018.3)/SerializedObject.m_NativeObjectPtr (>= 2018.3) has been renamed in this version of Unity!");
			}
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for all target values of a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		public PropertyTree()
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			if (typeof(Object).IsAssignableFrom(TargetType))
			{
				onValidateMethod = GetOnValidateMethod(TargetType);
				Undo.undoRedoPerformed = (UndoRedoCallback)Delegate.Combine((Delegate)(object)Undo.undoRedoPerformed, (Delegate)new UndoRedoCallback(InvokeOnUndoRedoPerformed));
			}
		}

		private static MethodInfo GetOnValidateMethod(Type type)
		{
			MethodInfo method = type.GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method == null)
			{
				type = type.BaseType;
				while (method == null && type != null)
				{
					method = type.GetMethod("OnValidate", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					type = type.BaseType;
				}
			}
			return method;
		}

		internal void InvokeOnPropertyValueChanged(InspectorProperty property, int selectionIndex)
		{
			//IL_0018: Expected O, but got Unknown
			if (this.OnPropertyValueChanged == null)
			{
				return;
			}
			try
			{
				this.OnPropertyValueChanged(property, selectionIndex);
			}
			catch (ExitGUIException val)
			{
				ExitGUIException val2 = val;
				throw val2;
			}
			catch (Exception ex)
			{
				if (ex.IsExitGUIException())
				{
					throw ex.AsExitGUIException();
				}
				Debug.LogException(ex);
			}
		}

		internal abstract void NotifyPropertyCreated(InspectorProperty property);

		internal abstract void NotifyPropertyDisposed(InspectorProperty property);

		internal abstract void ClearPathCaches();

		public abstract void CleanForCachedReuse();

		public abstract void SetTargets(params object[] newTargets);

		public abstract void SetSerializedObject(SerializedObject serializedObject);

		/// <summary>
		/// Registers that a given property is dirty and needs its changes to be applied at the end of the current frame.
		/// </summary>
		public abstract void RegisterPropertyDirty(InspectorProperty property);

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the current GUI frame.
		/// </summary>
		/// <param name="action">The action delegate to be delayed.</param>
		public abstract void DelayAction(Action action);

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
		/// </summary>
		/// <param name="action">The action to be delayed.</param>
		public abstract void DelayActionUntilRepaint(Action action);

		/// <summary>
		/// Enumerates over the properties of the tree.
		/// </summary>
		/// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
		/// <param name="onlyVisible">Whether to only include visible properties. Properties whose parents are invisible are considered invisible.</param>
		public abstract IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren = true, bool onlyVisible = false);

		/// <summary>
		/// Gets the property at the given path. Note that this is the path found in <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, not the Unity path.
		/// </summary>
		/// <param name="path">The path of the property to get.</param>
		public abstract InspectorProperty GetPropertyAtPath(string path);

		/// <summary>
		/// Gets the property at the given path. Note that this is the path found in <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, not the Unity path.
		/// </summary>
		/// <param name="path">The path of the property to get.</param>
		/// <param name="closestProperty"></param>
		public abstract InspectorProperty GetPropertyAtPath(string path, out InspectorProperty closestProperty);

		/// <summary>
		/// Gets the property at the given Unity path.
		/// </summary>
		/// <param name="path">The Unity path of the property to get.</param>
		public abstract InspectorProperty GetPropertyAtUnityPath(string path);

		/// <summary>
		/// Gets the property at the given Unity path.
		/// </summary>
		/// <param name="path">The Unity path of the property to get.</param>
		/// <param name="closestProperty"></param>
		public abstract InspectorProperty GetPropertyAtUnityPath(string path, out InspectorProperty closestProperty);

		/// <summary>
		/// Gets the property at the given deep reflection path.
		/// </summary>
		/// <param name="path">The deep reflection path of the property to get.</param>
		[Obsolete("Use GetPropertyAtPrefabModificationPath instead.", false)]
		public InspectorProperty GetPropertyAtDeepReflectionPath(string path)
		{
			return GetPropertyAtPrefabModificationPath(path);
		}

		/// <summary>
		/// Gets the property at the given Odin prefab modification path.
		/// </summary>
		/// <param name="path">The prefab modification path of the property to get.</param>
		public abstract InspectorProperty GetPropertyAtPrefabModificationPath(string path);

		/// <summary>
		/// Gets the property at the given Odin prefab modification path.
		/// </summary>
		/// <param name="path">The prefab modification path of the property to get.</param>
		/// <param name="closestProperty"></param>
		public abstract InspectorProperty GetPropertyAtPrefabModificationPath(string path, out InspectorProperty closestProperty);

		/// <summary>
		/// <para>Draw the property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.</para>
		/// <para>
		/// This is a shorthand for calling
		/// <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.BeginDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree,System.Boolean)" />,
		/// <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.DrawPropertiesInTree(Sirenix.OdinInspector.Editor.PropertyTree)" /> and .
		/// <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.EndDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree)" />.
		/// </para>
		/// </summary>
		public void Draw(bool applyUndo = true)
		{
			BeginDraw(applyUndo);
			DrawProperties();
			EndDraw();
		}

		public void BeginDraw(bool withUndo)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Invalid comparison between Unknown and I4
			prevTimeHelper = EditorTimeHelper.Time;
			EditorTimeHelper.Time = timeHelper;
			EditorTimeHelper.Time.Update();
			if ((int)Event.get_current().get_type() == 7)
			{
				ContextWidth = GUIHelper.ContextWidth;
			}
			GUIHelper.BetterContextWidth = ContextWidth;
			if (frameCounter.Update().IsNewFrame)
			{
				drawnInspectorDepthCount = 0;
			}
			drawnInspectorDepthCount++;
			if (this == null)
			{
				throw new ArgumentNullException("tree");
			}
			if (!IsStatic)
			{
				for (int i = 0; i < WeakTargets.Count; i++)
				{
					if (WeakTargets[i] == null)
					{
						GUILayout.Label("An inspected object has been destroyed; please refresh the inspector.", (GUILayoutOption[])(object)new GUILayoutOption[0]);
						return;
					}
				}
			}
			UpdateTree();
			WillUndo = false;
			if (withUndo)
			{
				if (!TargetType.ImplementsOrInherits(typeof(Object)))
				{
					Debug.LogError((object)("Automatic inspector undo only works when you're inspecting a type derived from UnityEngine.Object, and you are inspecting '" + TargetType.GetNiceName() + "'."));
				}
				else
				{
					WillUndo = true;
				}
			}
			RootProperty.OnStateUpdate(UpdateID);
			if (PrefabModificationHandler.HasNestedOdinPrefabData)
			{
				SirenixEditorGUI.ErrorMessageBox("A selected object is serialized by Odin, is a prefab, and contains nested prefab data (IE, more than one possible layer of prefab modifications). This is NOT CURRENTLY SUPPORTED by Odin - therefore, modification of all Odin-serialized values has been disabled for this object.\n\nThere is a strong likelihood that Odin-serialized values will be corrupt and/or wrong in other ways, as well as a very real risk that your computer may spontaneously combust and turn into a flaming wheel of cheese.");
			}
			if (!DrawMonoScriptObjectField)
			{
				return;
			}
			if (!monoScriptPropertyHasBeenGotten)
			{
				if (UnitySerializedObject != null)
				{
					monoScriptProperty = GetUnitySerializedObjectNoUpdate().FindProperty("m_Script");
				}
				monoScriptPropertyHasBeenGotten = true;
			}
			if (monoScriptProperty != null)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
				EditorGUILayout.PropertyField(monoScriptProperty, (GUILayoutOption[])(object)new GUILayoutOption[0]);
				GUIHelper.PopGUIEnabled();
			}
		}

		public void DrawProperties()
		{
			if (!AllowSearchFiltering || searchFilter == null || !DrawSearch())
			{
				RootProperty.Draw(null);
			}
		}

		/// <summary>
		/// <para>Draws a search bar for the property tree, and draws the search results if the search bar is used.</para>
		/// <para>If this method returns true, the property tree should generally not be drawn normally afterwards.</para>
		/// <para>Note that this method will throw exceptions if the property tree is not set up to be searchable; for that, see <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree.SetSearchable(System.Boolean,Sirenix.OdinInspector.SearchableAttribute)" />.</para>
		/// </summary>
		/// <returns>True if the property tree is being searched and is currently drawing its search results, otherwise false.</returns>
		public bool DrawSearch()
		{
			if (AllowSearchFiltering && searchFilter != null)
			{
				searchFilter.DrawDefaultSearchFieldLayout(null);
				if (searchFilter.HasSearchResults)
				{
					searchFilter.DrawSearchResults();
					return true;
				}
				return false;
			}
			throw new InvalidOperationException("Search is not currently enabled on this PropertyTree. Call SetSearchable(true) first.");
		}

		public void EndDraw()
		{
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Expected O, but got Unknown
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				InvokeDelayedActions();
				SerializedObject instance = GetUnitySerializedObjectNoUpdate();
				if (instance == null)
				{
					goto IL_004e;
				}
				if (SerializedObject_nativeObjectPtrGetter == null)
				{
					goto IL_0036;
				}
				IntPtr intPtr = SerializedObject_nativeObjectPtrGetter(ref instance);
				if (!(intPtr == IntPtr.Zero))
				{
					goto IL_0036;
				}
				goto end_IL_0000;
				IL_0036:
				if (WillUndo)
				{
					instance.ApplyModifiedProperties();
				}
				else
				{
					instance.ApplyModifiedPropertiesWithoutUndo();
				}
				goto IL_004e;
				IL_004e:
				bool flag = false;
				if (ApplyChanges())
				{
					flag = true;
					GUIHelper.RequestRepaint();
				}
				InvokeDelayedActions();
				if (flag)
				{
					InvokeOnValidate();
					if (PrefabModificationHandler.HasPrefabs)
					{
						ImmutableList<object> weakTargets = WeakTargets;
						for (int i = 0; i < weakTargets.Count; i++)
						{
							if (!(PrefabModificationHandler.TargetPrefabs[i] == (Object)null))
							{
								Object val = (Object)weakTargets[i];
								PrefabUtility.RecordPrefabInstancePropertyModifications(val);
							}
						}
					}
				}
				if (WillUndo)
				{
					if (flag && (int)Application.get_platform() == 0)
					{
						Undo.IncrementCurrentGroup();
						foreach (object weakTarget in WeakTargets)
						{
							if (weakTarget is Object)
							{
								Object val2 = weakTarget as Object;
								Undo.RecordObject(val2, "Odin change to " + val2.get_name());
							}
						}
					}
					Undo.FlushUndoRecordObjects();
				}
				drawnInspectorDepthCount--;
				end_IL_0000:;
			}
			finally
			{
				EditorTimeHelper.Time = prevTimeHelper;
			}
		}

		/// <summary>
		/// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="T:UnityEditor.SerializedObject" /> for this property tree, or no such property is found in the <see cref="T:UnityEditor.SerializedObject" />, a property will be emitted using <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter" />.
		/// </summary>
		/// <param name="path">The Odin or Unity path to the property to get.</param>
		public SerializedProperty GetUnityPropertyForPath(string path)
		{
			FieldInfo backingField;
			return GetUnityPropertyForPath(path, out backingField);
		}

		/// <summary>
		/// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="T:UnityEditor.SerializedObject" /> for this property tree, or no such property is found in the <see cref="T:UnityEditor.SerializedObject" />, a property will be emitted using <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter" />.
		/// </summary>
		/// <param name="path">The Odin or Unity path to the property to get.</param>
		/// <param name="backingField">The backing field of the Unity property.</param>
		public abstract SerializedProperty GetUnityPropertyForPath(string path, out FieldInfo backingField);

		/// <summary>
		/// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
		/// </summary>
		/// <param name="value">The reference value to check.</param>
		/// <param name="referencePath">The first found path of the object.</param>
		public abstract bool ObjectIsReferenced(object value, out string referencePath);

		/// <summary>
		/// Gets the number of references to a given object instance in this tree.
		/// </summary>
		public abstract int GetReferenceCount(object reference);

		/// <summary>
		/// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
		/// </summary>
		public abstract void UpdateTree();

		/// <summary>
		/// Replaces all occurrences of a value with another value, in the entire tree.
		/// </summary>
		/// <param name="from">The value to find all instances of.</param>
		/// <param name="to">The value to replace the found values with.</param>
		public abstract void ReplaceAllReferences(object from, object to);

		/// <summary>
		/// Gets the root tree property at a given index.
		/// </summary>
		/// <param name="index">The index of the property to get.</param>
		public abstract InspectorProperty GetRootProperty(int index);

		/// <summary>
		/// Invokes the actions that have been delayed using <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree.DelayAction(System.Action)" /> and <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree.DelayActionUntilRepaint(System.Action)" />.
		/// </summary>
		public abstract void InvokeDelayedActions();

		/// <summary>
		/// Applies all changes made with properties to the inspected target tree values, and marks all changed Unity objects dirty.
		/// </summary>
		/// <returns>true if any values were changed, otherwise false</returns>
		public abstract bool ApplyChanges();

		internal abstract SerializedObject GetUnitySerializedObjectNoUpdate();

		/// <summary>
		/// Invokes the OnValidate method on the property tree's targets if they are derived from <see cref="T:UnityEngine.Object" /> and have the method defined.
		/// </summary>
		public void InvokeOnValidate()
		{
			if (onValidateMethod == null)
			{
				return;
			}
			for (int i = 0; i < WeakTargets.Count; i++)
			{
				try
				{
					onValidateMethod.Invoke(WeakTargets[i], null);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		/// <summary>
		/// Registers an object reference to a given path; this is used to ensure that objects are always registered after having been encountered once.
		/// </summary>
		/// <param name="reference">The referenced object.</param>
		/// <param name="property">The property that contains the reference.</param>
		internal abstract void ForceRegisterObjectReference(object reference, InspectorProperty property);

		/// <summary>
		/// Creates a PropertyTree to inspect the static values of the given type.
		/// </summary>
		/// <param name="type">The type to inspect.</param>
		/// <returns>A PropertyTree instance for inspecting the type.</returns>
		public static PropertyTree CreateStatic(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return ((PropertyTree)Activator.CreateInstance(typeof(PropertyTree<>).MakeGenericType(type))).SetUpForIMGUIDrawing();
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a given target value.
		/// </summary>
		/// <param name="target">The target to create a tree for.</param>
		/// <exception cref="T:System.ArgumentNullException">target is null</exception>
		public static PropertyTree Create(object target)
		{
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			return Create(new object[1] { target }, null);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		/// <exception cref="T:System.ArgumentNullException">targets is null</exception>
		public static PropertyTree Create(params object[] targets)
		{
			if (targets == null)
			{
				throw new ArgumentNullException("targets");
			}
			return Create((IList)targets);
		}

		/// <summary>
		/// Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for all target values of a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		/// <param name="serializedObject">The serialized object to create a tree for.</param>
		/// <exception cref="T:System.ArgumentNullException">serializedObject is null</exception>
		public static PropertyTree Create(SerializedObject serializedObject)
		{
			if (serializedObject == null)
			{
				throw new ArgumentNullException("serializedObject");
			}
			return Create(serializedObject.get_targetObjects(), serializedObject);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		public static PropertyTree Create(IList targets)
		{
			return Create(targets, null);
		}

		/// <summary>
		/// <para>Creates a new <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree" /> for a set of given target values, represented by a given <see cref="T:UnityEditor.SerializedObject" />.</para>
		/// <para>Note that the targets all need to be of the same type.</para>
		/// </summary>
		/// <param name="targets">The targets to create a tree for.</param>
		/// <param name="serializedObject">The serialized object to create a tree for. Note that the target values of the given <see cref="T:UnityEditor.SerializedObject" /> must be the same values given in the targets parameter.</param>
		public static PropertyTree Create(IList targets, SerializedObject serializedObject)
		{
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Expected O, but got Unknown
			if (targets == null)
			{
				throw new ArgumentNullException("targets");
			}
			if (targets.Count == 0)
			{
				throw new ArgumentException("There must be at least one target.");
			}
			if (serializedObject != null)
			{
				bool flag = true;
				Object[] targetObjects = serializedObject.get_targetObjects();
				if (targets.Count != targetObjects.Length)
				{
					flag = false;
				}
				else
				{
					for (int i = 0; i < targets.Count; i++)
					{
						if (targets[i] != targetObjects[i])
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					throw new ArgumentException("Given target array must be identical in length and content to the target objects array in the given serializedObject.");
				}
			}
			Type type = null;
			for (int j = 0; j < targets.Count; j++)
			{
				object obj = targets[j];
				if (obj == null)
				{
					throw new ArgumentException("Target at index " + j + " was null.");
				}
				Type type2;
				if (j == 0)
				{
					type = obj.GetType();
				}
				else if (type != (type2 = obj.GetType()) && !type.IsAssignableFrom(type2))
				{
					if (!type2.IsAssignableFrom(type))
					{
						throw new ArgumentException("Expected targets of type " + type.Name + ", but got an incompatible target of type " + type2.Name + " at index " + j + ".");
					}
					type = type2;
				}
			}
			Type type3 = typeof(PropertyTree<>).MakeGenericType(type);
			Array array;
			if (targets.GetType().IsArray && targets.GetType().GetElementType() == type)
			{
				array = (Array)targets;
			}
			else
			{
				array = Array.CreateInstance(type, targets.Count);
				targets.CopyTo(array, 0);
			}
			if (serializedObject == null && type.IsAssignableFrom(typeof(Object)))
			{
				Object[] array2 = (Object[])(object)new Object[targets.Count];
				targets.CopyTo(array2, 0);
				serializedObject = new SerializedObject(array2);
			}
			return ((PropertyTree)Activator.CreateInstance(type3, array, serializedObject)).SetUpForIMGUIDrawing();
		}

		private void InvokeOnUndoRedoPerformed()
		{
			if (this.OnUndoRedoPerformed != null)
			{
				this.OnUndoRedoPerformed();
			}
		}

		protected void InitSearchFilter()
		{
			SearchableAttribute attribute = RootProperty.GetAttribute<SearchableAttribute>();
			if (attribute != null)
			{
				searchFilter = new PropertySearchFilter(RootProperty, attribute);
			}
		}

		/// <summary>
		/// <para>Sets whether the property tree should be searchable or not, and allows the passing in of a custom SearchableAttribute instance to configure the search.</para>
		/// </summary>
		/// <param name="searchable">Whether the tree should be set to be searchable or not.</param>
		/// <param name="config">If the tree is set to be searchable, then if this parameter is not null, it will be used to configure the property tree search. If the parameter is null, the SearchableAttribute on the tree's <see cref="P:Sirenix.OdinInspector.Editor.PropertyTree.RootProperty" /> will be used. If that property has no such attribute, then default search settings will be applied.</param>
		public void SetSearchable(bool searchable, SearchableAttribute config = null)
		{
			AllowSearchFiltering = searchable;
			if (searchable)
			{
				searchFilter = new PropertySearchFilter(RootProperty, config ?? RootProperty.GetAttribute<SearchableAttribute>() ?? new SearchableAttribute());
			}
			else
			{
				searchFilter = null;
			}
		}

		protected virtual void Dispose(bool finalizer)
		{
			if (!disposedValue)
			{
				if (finalizer)
				{
					UnityEditorEventUtility.DelayActionThreadSafe(ActuallyDispose);
				}
				else
				{
					ActuallyDispose();
				}
			}
		}

		~PropertyTree()
		{
			Dispose(finalizer: true);
		}

		public void Dispose()
		{
			Dispose(finalizer: false);
		}

		private void ActuallyDispose()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			RootProperty.Dispose();
			Undo.undoRedoPerformed = (UndoRedoCallback)Delegate.Remove((Delegate)(object)Undo.undoRedoPerformed, (Delegate)new UndoRedoCallback(InvokeOnUndoRedoPerformed));
			if (drawerChainResolver is IDisposable)
			{
				(drawerChainResolver as IDisposable).Dispose();
			}
			if (attributeProcessorLocator is IDisposable)
			{
				(attributeProcessorLocator as IDisposable).Dispose();
			}
			if (propertyResolverLocator is IDisposable)
			{
				(propertyResolverLocator as IDisposable).Dispose();
			}
			this.OnUndoRedoPerformed = null;
			this.OnPropertyValueChanged = null;
			disposedValue = true;
		}

		public PropertyTree SetUpForIMGUIDrawing()
		{
			ComponentProviders.Clear();
			ComponentProviders.Add(new ValidationComponentProvider(new DefaultValidatorLocator
			{
				CustomValidatorFilter = (Type type) => (!type.IsDefined<NoValidationInInspectorAttribute>(inherit: true)) ? true : false
			}));
			RootProperty.RefreshSetup();
			return this;
		}

		public PropertyTree SetUpForValidation()
		{
			ComponentProviders.Clear();
			ComponentProviders.Add(new ValidationComponentProvider());
			RootProperty.RefreshSetup();
			return this;
		}
	}
	/// <summary>
	/// <para>Represents a set of strongly typed values as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
	/// <para>This class also handles management of prefab modifications.</para>
	/// </summary>
	public sealed class PropertyTree<T> : PropertyTree
	{
		private struct PropertyPathResult
		{
			public InspectorProperty Property;

			public InspectorProperty ClosestProperty;
		}

		private static readonly bool TargetIsValueType = typeof(T).IsValueType;

		private static readonly bool TargetIsUnityObject = typeof(Object).IsAssignableFrom(typeof(T));

		private Dictionary<object, int> objectReferenceCounts = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Default);

		private Dictionary<object, string> objectReferences = new Dictionary<object, string>(ReferenceEqualityComparer<object>.Default);

		private Dictionary<string, Dictionary<Type, SerializedProperty>> emittedUnityPropertyCache = new Dictionary<string, Dictionary<Type, SerializedProperty>>();

		private List<Action> delayedActions = new List<Action>();

		private List<Action> delayedRepaintActions = new List<Action>();

		private List<InspectorProperty> dirtyProperties = new List<InspectorProperty>();

		private T[] targets;

		private InspectorProperty rootProperty;

		private SerializedObject serializedObject;

		private int serializedObjectUpdateID;

		private int updateID = 1;

		private object[] weakTargets;

		private ImmutableList<T> immutableTargets;

		private ImmutableList<object> immutableWeakTargets;

		private bool includesSpeciallySerializedMembers;

		private PrefabModificationHandler prefabModificationHandler;

		private int prefabModificationHandler_lastUpdateID;

		private static readonly bool includesSpeciallySerializedMembers_StaticCache = InspectorPropertyInfoUtility.TypeDefinesShowOdinSerializedPropertiesInInspectorAttribute_Cached(typeof(T));

		/// <summary>
		/// Gets the root property of the tree.
		/// </summary>
		public override InspectorProperty RootProperty
		{
			get
			{
				if (rootProperty == null)
				{
					rootProperty = InspectorProperty.Create(this, null, InspectorPropertyInfo.CreateValue("$ROOT", 0f, includesSpeciallySerializedMembers ? SerializationBackend.Odin : SerializationBackend.Unity, (IValueGetterSetter)new GetterSetter<int, T>(delegate(ref int index)
					{
						return targets[index];
					}, delegate(ref int index, T value)
					{
						targets[index] = value;
					}), (Attribute[])null), 0, isRoot: true);
					rootProperty.Update(forceUpdate: true);
				}
				return rootProperty;
			}
		}

		/// <summary>
		/// Gets the secret root property of the PropertyTree.
		/// </summary>
		[Obsolete("Use RootProperty instead; the root is no longer considered 'secret'.", false)]
		public override InspectorProperty SecretRootProperty => RootProperty;

		/// <summary>
		/// Gets the <see cref="F:Sirenix.OdinInspector.Editor.PropertyTree`1.prefabModificationHandler" /> for the PropertyTree.
		/// </summary>
		public override PrefabModificationHandler PrefabModificationHandler
		{
			get
			{
				if (prefabModificationHandler == null)
				{
					prefabModificationHandler = new PrefabModificationHandler(this);
				}
				if (TargetIsUnityObject && prefabModificationHandler_lastUpdateID != updateID)
				{
					prefabModificationHandler.Update();
					prefabModificationHandler_lastUpdateID = updateID;
				}
				return prefabModificationHandler;
			}
		}

		/// <summary>
		/// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="M:Sirenix.OdinInspector.Editor.InspectorProperty.Update(System.Boolean)" /> to avoid updating multiple times in the same update round.
		/// </summary>
		public override int UpdateID => updateID;

		/// <summary>
		/// The <see cref="T:UnityEditor.SerializedObject" /> that this tree represents, if the tree was created for a <see cref="T:UnityEditor.SerializedObject" />.
		/// </summary>
		public override SerializedObject UnitySerializedObject
		{
			get
			{
				if (serializedObject != null && serializedObjectUpdateID != updateID)
				{
					serializedObjectUpdateID = updateID;
					serializedObject.Update();
				}
				return serializedObject;
			}
		}

		/// <summary>
		/// The type of the values that the property tree represents.
		/// </summary>
		public override Type TargetType => typeof(T);

		/// <summary>
		/// The strongly types actual values that the property tree represents.
		/// </summary>
		public ImmutableList<T> Targets
		{
			get
			{
				if (immutableTargets == null)
				{
					immutableTargets = new ImmutableList<T>(targets);
				}
				return immutableTargets;
			}
		}

		/// <summary>
		/// The weakly types actual values that the property tree represents.
		/// </summary>
		public override ImmutableList<object> WeakTargets
		{
			get
			{
				if (immutableWeakTargets == null)
				{
					if (weakTargets == null)
					{
						weakTargets = new object[targets.Length];
						targets.CopyTo(weakTargets, 0);
					}
					immutableWeakTargets = new ImmutableList<object>(weakTargets);
				}
				else if (TargetIsValueType)
				{
					targets.CopyTo(weakTargets, 0);
				}
				return immutableWeakTargets;
			}
		}

		/// <summary>
		/// The number of root properties in the tree.
		/// </summary>
		public override int RootPropertyCount => RootProperty.Children.Count;

		/// <summary>
		/// Whether this property tree also represents members that are specially serialized by Odin.
		/// </summary>
		[Obsolete("This value is no longer guaranteed to be correct, as it may have different answers for different properties in the tree. Instead look at InspectorProperty.SerializationRoot to determine whether specially serialized members might be included.", true)]
		public override bool IncludesSpeciallySerializedMembers
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		internal override SerializedObject GetUnitySerializedObjectNoUpdate()
		{
			return serializedObject;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class, inspecting only the target (<see cref="!:T" />) type's static members.
		/// </summary>
		public PropertyTree()
		{
			base.IsStatic = true;
			targets = new T[1];
			InitSearchFilter();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class.
		/// </summary>
		/// <param name="serializedObject">The serialized object to represent.</param>
		public PropertyTree(SerializedObject serializedObject)
			: this(serializedObject.get_targetObjects().Cast<T>().ToArray(), serializedObject)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class.
		/// </summary>
		/// <param name="targets">The targets to represent.</param>
		public PropertyTree(T[] targets)
			: this(targets, (SerializedObject)null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.PropertyTree`1" /> class.
		/// </summary>
		/// <param name="targets">The targets to represent.</param>
		/// <param name="serializedObject">The serialized object to represent. Note that the target values of the given <see cref="T:UnityEditor.SerializedObject" /> must be the same values given in the targets parameter.</param>
		/// <exception cref="T:System.ArgumentNullException">targets is null</exception>
		/// <exception cref="T:System.ArgumentException">
		/// There must be at least one target.
		/// or
		/// A given target is a null value.
		/// </exception>
		public PropertyTree(T[] targets, SerializedObject serializedObject)
		{
			if (targets == null)
			{
				throw new ArgumentNullException("targets");
			}
			if (targets.Length == 0)
			{
				throw new ArgumentException("There must be at least one target.");
			}
			for (int i = 0; i < targets.Length; i++)
			{
				if (targets[i] == null)
				{
					throw new ArgumentException("A target at index '" + i + "' is a null value.");
				}
			}
			includesSpeciallySerializedMembers = includesSpeciallySerializedMembers_StaticCache;
			this.serializedObject = serializedObject;
			this.targets = targets;
			InitSearchFilter();
		}

		/// <summary>
		/// Applies all changes made with properties to the inspected target tree values.
		/// </summary>
		/// <returns>
		/// true if any values were changed, otherwise false
		/// </returns>
		public override bool ApplyChanges()
		{
			bool flag = false;
			for (int i = 0; i < dirtyProperties.Count; i++)
			{
				InspectorProperty inspectorProperty = dirtyProperties[i];
				IApplyableResolver applyableResolver = inspectorProperty.ChildResolver as IApplyableResolver;
				if (applyableResolver != null && applyableResolver.ApplyChanges())
				{
					flag = true;
					if (inspectorProperty.BaseValueEntry != null)
					{
						for (int j = 0; j < inspectorProperty.BaseValueEntry.ValueCount; j++)
						{
							inspectorProperty.BaseValueEntry.TriggerOnValueChanged(j);
						}
						if (inspectorProperty.BaseValueEntry.ValueChangedFromPrefab)
						{
							for (int k = 0; k < Targets.Count; k++)
							{
								PrefabModificationHandler.RegisterPrefabValueModification(inspectorProperty, k);
							}
						}
					}
				}
				if (inspectorProperty.ValueEntry != null && inspectorProperty.ValueEntry.ApplyChanges())
				{
					flag = true;
				}
				if (!flag)
				{
					continue;
				}
				InspectorProperty serializationRoot = inspectorProperty.SerializationRoot;
				for (int l = 0; l < serializationRoot.ValueEntry.ValueCount; l++)
				{
					object obj = serializationRoot.ValueEntry.WeakValues[l];
					Object val = obj as Object;
					if (val != (Object)null)
					{
						InspectorUtilities.RegisterUnityObjectDirty(val);
					}
				}
			}
			dirtyProperties.Clear();
			if (flag && PrefabModificationHandler != null && PrefabModificationHandler.HasPrefabs && UnitySerializedObject != null)
			{
				DelayActionUntilRepaint(delegate
				{
					DelayActionUntilRepaint(delegate
					{
						//IL_002b: Unknown result type (might be due to invalid IL or missing references)
						//IL_0035: Expected O, but got Unknown
						for (int m = 0; m < WeakTargets.Count; m++)
						{
							object obj2 = WeakTargets[m];
							ISerializationCallbackReceiver val2 = obj2 as ISerializationCallbackReceiver;
							if (val2 != null)
							{
								val2.OnBeforeSerialize();
							}
							PrefabUtility.RecordPrefabInstancePropertyModifications((Object)WeakTargets[m]);
						}
					});
				});
			}
			return flag;
		}

		/// <summary>
		/// Registers that a given property is dirty and needs its changes to be applied at the end of the current frame.
		/// </summary>
		/// <param name="property"></param>
		/// <exception cref="T:System.NotImplementedException"></exception>
		public override void RegisterPropertyDirty(InspectorProperty property)
		{
			dirtyProperties.Add(property);
		}

		/// <summary>
		/// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
		/// </summary>
		public override void UpdateTree()
		{
			ApplyChanges();
			updateID++;
			objectReferences.Clear();
			objectReferenceCounts.Clear();
			RootProperty.Update();
		}

		internal override void NotifyPropertyCreated(InspectorProperty property)
		{
		}

		internal override void NotifyPropertyDisposed(InspectorProperty property)
		{
		}

		internal override void ClearPathCaches()
		{
		}

		/// <summary>
		/// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
		/// </summary>
		/// <param name="value">The reference value to check.</param>
		/// <param name="referencePath">The first found path of the object.</param>
		public override bool ObjectIsReferenced(object value, out string referencePath)
		{
			return objectReferences.TryGetValue(value, out referencePath);
		}

		/// <summary>
		/// Gets the number of references to a given object instance in this tree.
		/// </summary>
		/// <param name="reference"></param>
		public override int GetReferenceCount(object reference)
		{
			objectReferenceCounts.TryGetValue(reference, out var value);
			return value;
		}

		/// <summary>
		/// Gets the property at the given path. Note that this is the path found in <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, not the Unity path.
		/// </summary>
		/// <param name="path">The path of the property to get.</param>
		public override InspectorProperty GetPropertyAtPath(string path)
		{
			InspectorProperty closestProperty;
			return GetPropertyAtPath(path, out closestProperty);
		}

		/// <summary>
		/// Gets the property at the given path. Note that this is the path found in <see cref="P:Sirenix.OdinInspector.Editor.InspectorProperty.Path" />, not the Unity path.
		/// </summary>
		/// <param name="path">The path of the property to get.</param>
		/// <param name="closestProperty"></param>
		public override InspectorProperty GetPropertyAtPath(string path, out InspectorProperty closestProperty)
		{
			if (path == "$ROOT")
			{
				closestProperty = RootProperty;
				return RootProperty;
			}
			closestProperty = null;
			int num = 0;
			int num2 = StringIndexOf(path, '.', num);
			StringSlice name = ((num2 == -1) ? new StringSlice(path) : path.Slice(num, num2 - num));
			InspectorProperty inspectorProperty = RootProperty;
			PropertyPathResult propertyPathResult = default(PropertyPathResult);
			while (true)
			{
				propertyPathResult.ClosestProperty = inspectorProperty;
				inspectorProperty = inspectorProperty.Children.Get(ref name);
				if (inspectorProperty == null || num2 == -1)
				{
					break;
				}
				num = num2 + 1;
				num2 = StringIndexOf(path, '.', num);
				name = ((num2 == -1) ? path.Slice(num) : path.Slice(num, num2 - num));
			}
			propertyPathResult.Property = inspectorProperty;
			if (propertyPathResult.Property == null && propertyPathResult.ClosestProperty != null)
			{
				int num3 = path.LastIndexOf('.');
				if (num3 > 0)
				{
					StringSlice name2 = path.Slice(num3 + 1);
					propertyPathResult.Property = propertyPathResult.ClosestProperty.Children.Get(ref name2);
				}
			}
			closestProperty = propertyPathResult.ClosestProperty;
			return propertyPathResult.Property;
		}

		[MethodImpl((MethodImplOptions)256)]
		private static int StringIndexOf(string str, char c, int start)
		{
			for (int i = start; i < str.Length; i++)
			{
				if (str[i] == c)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Finds the property at the specified unity path.
		/// </summary>
		/// <param name="path">The unity path for the property.</param>
		/// <returns>The property found at the path.</returns>
		public override InspectorProperty GetPropertyAtUnityPath(string path)
		{
			InspectorProperty closestProperty;
			return GetPropertyAtUnityPath(path, out closestProperty);
		}

		/// <summary>
		/// Finds the property at the specified unity path.
		/// </summary>
		/// <param name="path">The unity path for the property.</param>
		/// <param name="closestProperty"></param>
		/// <returns>The property found at the path.</returns>
		public override InspectorProperty GetPropertyAtUnityPath(string path, out InspectorProperty closestProperty)
		{
			closestProperty = null;
			PropertyPathResult propertyPathResult = default(PropertyPathResult);
			propertyPathResult.ClosestProperty = null;
			int num = 0;
			int num2 = StringIndexOf(path, '.', num);
			StringSlice name = ((num2 == -1) ? new StringSlice(path) : path.Slice(num, num2 - num));
			InspectorProperty inspectorProperty = RootProperty;
			while (true)
			{
				InspectorProperty inspectorProperty2 = inspectorProperty.Children.Get(ref name);
				if (inspectorProperty2 == null && name == "Array" && num2 != -1)
				{
					int num3 = num2 + 1;
					int num4 = StringIndexOf(path, '.', num3);
					StringSlice stringSlice = ((num4 == -1) ? path.Slice(num3) : path.Slice(num3, num4 - num3));
					if (stringSlice.StartsWith("data[") && int.TryParse(stringSlice.Slice(5, stringSlice.Length - 6).ToString(), out var result))
					{
						string name2 = CollectionResolverUtilities.DefaultIndexToChildName(result);
						inspectorProperty2 = inspectorProperty.Children.Get(name2);
						if (inspectorProperty2 != null)
						{
							num = num3;
							num2 = num4;
							name = stringSlice;
						}
					}
				}
				if (inspectorProperty2 == null && !(inspectorProperty.ChildResolver is ICollectionResolver))
				{
					inspectorProperty2 = TryFindChildMemberPropertyWithNameFromGroups(name, inspectorProperty);
				}
				inspectorProperty = inspectorProperty2;
				if (inspectorProperty2 == null || num2 == -1)
				{
					break;
				}
				propertyPathResult.ClosestProperty = inspectorProperty;
				num = num2 + 1;
				num2 = StringIndexOf(path, '.', num);
				name = ((num2 == -1) ? path.Slice(num) : path.Slice(num, num2 - num));
			}
			propertyPathResult.Property = inspectorProperty;
			closestProperty = propertyPathResult.ClosestProperty;
			return propertyPathResult.Property;
		}

		/// <summary>
		/// Finds the property at the specified modification path.
		/// </summary>
		/// <param name="path">The prefab modification path for the property.</param>
		/// <returns>The property found at the path.</returns>
		public override InspectorProperty GetPropertyAtPrefabModificationPath(string path)
		{
			InspectorProperty closestProperty;
			return GetPropertyAtPrefabModificationPath(path, out closestProperty);
		}

		/// <summary>
		/// Finds the property at the specified modification path.
		/// </summary>
		/// <param name="path">The prefab modification path for the property.</param>
		/// <param name="closestProperty"></param>
		/// <returns>The property found at the path.</returns>
		public override InspectorProperty GetPropertyAtPrefabModificationPath(string path, out InspectorProperty closestProperty)
		{
			closestProperty = null;
			PropertyPathResult propertyPathResult = default(PropertyPathResult);
			propertyPathResult.ClosestProperty = null;
			int num = 0;
			int num2 = StringIndexOf(path, '.', num);
			StringSlice name = ((num2 == -1) ? new StringSlice(path) : path.Slice(num, num2 - num));
			InspectorProperty inspectorProperty = RootProperty;
			while (true)
			{
				InspectorProperty inspectorProperty2 = inspectorProperty.Children.Get(ref name);
				if (inspectorProperty2 == null && !(inspectorProperty.ChildResolver is ICollectionResolver))
				{
					inspectorProperty2 = TryFindChildMemberPropertyWithNameFromGroups(name, inspectorProperty);
				}
				inspectorProperty = inspectorProperty2;
				if (inspectorProperty2 == null || num2 == -1)
				{
					break;
				}
				propertyPathResult.ClosestProperty = inspectorProperty;
				num = num2 + 1;
				num2 = StringIndexOf(path, '.', num);
				name = ((num2 == -1) ? path.Slice(num) : path.Slice(num, num2 - num));
			}
			propertyPathResult.Property = inspectorProperty;
			closestProperty = propertyPathResult.ClosestProperty;
			return propertyPathResult.Property;
		}

		private InspectorProperty TryFindChildMemberPropertyWithNameFromGroups(StringSlice name, InspectorProperty property)
		{
			if (property.ChildResolver is ICollectionResolver)
			{
				return null;
			}
			for (int i = 0; i < property.Children.Count; i++)
			{
				InspectorProperty inspectorProperty = property.Children.Get(i);
				switch (inspectorProperty.Info.PropertyType)
				{
				case PropertyType.Value:
					if (inspectorProperty.Info.HasSingleBackingMember && inspectorProperty.Name == name)
					{
						return inspectorProperty;
					}
					break;
				case PropertyType.Group:
				{
					InspectorProperty inspectorProperty2 = TryFindChildMemberPropertyWithNameFromGroups(name, inspectorProperty);
					if (inspectorProperty2 != null)
					{
						return inspectorProperty2;
					}
					break;
				}
				default:
					throw new NotImplementedException(inspectorProperty.Info.PropertyType.ToString());
				case PropertyType.Method:
					break;
				}
			}
			return null;
		}

		/// <summary>
		/// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="T:UnityEditor.SerializedObject" /> for this property tree, or no such property is found in the <see cref="T:UnityEditor.SerializedObject" />, a property will be emitted using <see cref="T:Sirenix.OdinInspector.Editor.UnityPropertyEmitter" />.
		/// </summary>
		/// <param name="path">The Odin or Unity path to the property to get.</param>
		/// <param name="backingField">The backing field of the Unity property.</param>
		public override SerializedProperty GetUnityPropertyForPath(string path, out FieldInfo backingField)
		{
			backingField = null;
			InspectorProperty propertyAtPath = GetPropertyAtPath(path);
			string text = ((propertyAtPath != null) ? propertyAtPath.UnityPropertyPath : InspectorUtilities.ConvertToUnityPropertyPath(path));
			SerializedProperty value = null;
			SerializedObject unitySerializedObject = UnitySerializedObject;
			if (unitySerializedObject != null)
			{
				value = unitySerializedObject.FindProperty(text);
				if (propertyAtPath != null)
				{
					backingField = propertyAtPath.Info.GetMemberInfo() as FieldInfo;
					if (backingField == null && propertyAtPath.Parent != null && propertyAtPath.Parent.ChildResolver is ICollectionResolver)
					{
						backingField = propertyAtPath.Parent.Info.GetMemberInfo() as FieldInfo;
					}
				}
			}
			if (value == null && propertyAtPath != null && propertyAtPath.Info.PropertyType == PropertyType.Value)
			{
				if (!emittedUnityPropertyCache.TryGetValue(path, out var value2))
				{
					value2 = new Dictionary<Type, SerializedProperty>(FastTypeComparer.Instance);
					emittedUnityPropertyCache.Add(path, value2);
				}
				if (!value2.TryGetValue(propertyAtPath.ValueEntry.TypeOfValue, out value))
				{
					value = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(propertyAtPath.Info.PropertyName, propertyAtPath.ValueEntry.TypeOfValue, targets.Length);
					value2.Add(propertyAtPath.ValueEntry.TypeOfValue, value);
				}
				else if (value != null && value.get_serializedObject().get_targetObject() == (Object)null)
				{
					value = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(propertyAtPath.Info.PropertyName, propertyAtPath.ValueEntry.TypeOfValue, targets.Length);
					value2[propertyAtPath.ValueEntry.TypeOfValue] = value;
				}
				if (value != null)
				{
					value.get_serializedObject().Update();
				}
			}
			return value;
		}

		/// <summary>
		/// Enumerates over the properties of the tree. WARNING: For tree that have large targets with lots of data, this may involve massive amounts of work as the full tree structure is resolved. USE THIS METHOD SPARINGLY AND ONLY WHEN ABSOLUTELY NECESSARY!
		/// </summary>
		/// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
		/// /// <param name="onlyVisible">Whether to only include visible properties. Properties whose parents are invisible are considered invisible.</param>
		public override IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren = true, bool onlyVisible = false)
		{
			if (includeChildren)
			{
				if (RootProperty.Children.Count == 0)
				{
					yield break;
				}
				for (InspectorProperty current = RootProperty.Children.Get(0); current != null; current = current.NextProperty(includeChildren: true, onlyVisible))
				{
					if (!onlyVisible || current.State.Visible)
					{
						yield return current;
					}
				}
				yield break;
			}
			for (int i = 0; i < RootProperty.Children.Count; i++)
			{
				InspectorProperty inspectorProperty = RootProperty.Children.Get(i);
				if (!onlyVisible || inspectorProperty.State.Visible)
				{
					yield return RootProperty.Children.Get(i);
				}
			}
		}

		/// <summary>
		/// Replaces all occurrences of a value with another value, in the entire tree.
		/// </summary>
		/// <param name="from">The value to find all instances of.</param>
		/// <param name="to">The value to replace the found values with.</param>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		/// <exception cref="T:System.ArgumentException">The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").</exception>
		public override void ReplaceAllReferences(object from, object to)
		{
			if (from == null)
			{
				throw new ArgumentNullException();
			}
			if (to != null && from.GetType() != to.GetType())
			{
				throw new ArgumentException("The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").");
			}
			foreach (InspectorProperty item in EnumerateTree())
			{
				if (item.Info.PropertyType != 0 || item.Info.TypeOfValue.IsValueType)
				{
					continue;
				}
				IPropertyValueEntry valueEntry = item.ValueEntry;
				for (int i = 0; i < valueEntry.ValueCount; i++)
				{
					object obj = valueEntry.WeakValues[i];
					if (from == obj)
					{
						valueEntry.WeakValues[i] = to;
					}
				}
			}
		}

		internal override void ForceRegisterObjectReference(object reference, InspectorProperty property)
		{
			objectReferences[reference] = property.Path;
		}

		/// <summary>
		/// Gets the root tree property at a given index.
		/// </summary>
		/// <param name="index">The index of the property to get.</param>
		public override InspectorProperty GetRootProperty(int index)
		{
			return RootProperty.Children.Get(index);
		}

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the current GUI frame.
		/// </summary>
		/// <param name="action">The action delegate to be delayed.</param>
		/// <exception cref="T:System.ArgumentNullException">action</exception>
		public override void DelayAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			delayedActions.Add(action);
		}

		/// <summary>
		/// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
		/// </summary>
		/// <param name="action">The action to be delayed.</param>
		/// <exception cref="T:System.ArgumentNullException">action</exception>
		public override void DelayActionUntilRepaint(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			delayedRepaintActions.Add(action);
		}

		/// <summary>
		/// Invokes the actions that have been delayed using <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree`1.DelayAction(System.Action)" /> and <see cref="M:Sirenix.OdinInspector.Editor.PropertyTree`1.DelayActionUntilRepaint(System.Action)" />.
		/// </summary>
		public override void InvokeDelayedActions()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Invalid comparison between Unknown and I4
			for (int i = 0; i < delayedActions.Count; i++)
			{
				try
				{
					delayedActions[i]();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			delayedActions.Clear();
			if ((int)Event.get_current().get_type() != 7)
			{
				return;
			}
			for (int j = 0; j < delayedRepaintActions.Count; j++)
			{
				try
				{
					delayedRepaintActions[j]();
				}
				catch (Exception ex2)
				{
					Debug.LogException(ex2);
				}
			}
			delayedRepaintActions.Clear();
		}

		public override void CleanForCachedReuse()
		{
			PropertyChildren children = RootProperty.Children;
			PropertyChildren.ExistingChildEnumerator enumerator = children.GetExistingChildren().GetEnumerator();
			while (enumerator.MoveNext())
			{
				InspectorProperty current = enumerator.Current;
				current.CleanForCachedReuse();
			}
			delayedActions.Clear();
			delayedRepaintActions.Clear();
			updateID++;
		}

		public override void SetTargets(params object[] newTargets)
		{
			serializedObject = null;
			prefabModificationHandler = null;
			monoScriptProperty = null;
			monoScriptPropertyHasBeenGotten = false;
			if (targets.Length != newTargets.Length)
			{
				throw new ArgumentException("Target count of tree cannot be changed");
			}
			for (int i = 0; i < targets.Length; i++)
			{
				T val = (T)newTargets[i];
				if (val == null)
				{
					throw new NullReferenceException("Tree target cannot be null");
				}
				targets[i] = val;
			}
			targets.CopyTo(weakTargets, 0);
			UpdateTree();
		}

		public override void SetSerializedObject(SerializedObject serializedObject)
		{
			this.serializedObject = serializedObject;
			prefabModificationHandler = null;
			monoScriptProperty = null;
			monoScriptPropertyHasBeenGotten = false;
			Object[] targetObjects = serializedObject.get_targetObjects();
			if (targets.Length != targetObjects.Length)
			{
				throw new ArgumentException("Target count of tree cannot be changed");
			}
			for (int i = 0; i < targets.Length; i++)
			{
				T val = (T)(object)targetObjects[i];
				if (val == null)
				{
					throw new NullReferenceException("Tree target cannot be null");
				}
				targets[i] = val;
			}
			targets.CopyTo(weakTargets, 0);
			UpdateTree();
		}
	}
}
