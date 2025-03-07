using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>This is a class for creating, getting and modifying a property's various states. An instance of this class always comes attached to an InspectorProperty.</para>
	/// <para>See Odin's tutorials for more information about usage of the state system.</para>
	/// </summary>
	public sealed class PropertyState
	{
		private class CustomState
		{
			public object Value;

			public object ValueLastLayout;

			public object DefaultValue;

			public Type Type;

			public ILocalPersistentContext PersistentValue;
		}

		private bool visible;

		private bool visibleLastLayout;

		private bool enabled;

		private bool enabledLastLayout;

		private LocalPersistentContext<bool> expanded;

		private bool expandedLastLayout;

		private InspectorProperty property;

		private int index;

		private Dictionary<string, CustomState> customStates;

		/// <summary>
		/// If set to true, all state changes for this property will be logged to the console.
		/// </summary>
		public bool LogChanges;

		/// <summary>
		/// Whether the property is visible in the inspector.
		/// </summary>
		public bool Visible
		{
			get
			{
				return visible;
			}
			set
			{
				if (visible != value)
				{
					if (LogChanges)
					{
						LogChange("Visible", visible, value);
					}
					visible = value;
					SendStateChangedNotifications("Visible");
				}
			}
		}

		/// <summary>
		/// Whether the Visible state was true or not during the last layout event.
		/// </summary>
		public bool VisibleLastLayout => visibleLastLayout;

		/// <summary>
		/// Whether the property is enabled in the inspector.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (enabled != value)
				{
					if (LogChanges)
					{
						LogChange("Enabled", enabled, value);
					}
					enabled = value;
					SendStateChangedNotifications("Enabled");
				}
			}
		}

		/// <summary>
		/// Whether the Enabled state was true or not during the last layout event.
		/// </summary>
		public bool EnabledLastLayout => enabledLastLayout;

		/// <summary>
		/// Whether the property is expanded in the inspector.
		/// </summary>
		public bool Expanded
		{
			get
			{
				if (expanded == null)
				{
					expanded = GetPersistentContext("expanded", expandedLastLayout);
				}
				return expanded.Value;
			}
			set
			{
				if (expanded == null)
				{
					expanded = GetPersistentContext("expanded", expandedLastLayout);
				}
				if (expanded.Value != value)
				{
					if (LogChanges)
					{
						LogChange("Expanded", expanded.Value, value);
					}
					expanded.Value = value;
					SendStateChangedNotifications("Expanded");
				}
			}
		}

		/// <summary>
		/// Whether the Expanded state was true or not during the last layout event.
		/// </summary>
		public bool ExpandedLastLayout => expandedLastLayout;

		public PropertyState(InspectorProperty property, int index)
		{
			this.property = property;
			this.index = index;
			if (this.property.ChildResolver is ICollectionResolver)
			{
				expandedLastLayout = GlobalConfig<GeneralDrawerConfig>.Instance.ExpandFoldoutByDefault;
			}
			else
			{
				expandedLastLayout = GlobalConfig<GeneralDrawerConfig>.Instance.OpenListsByDefault;
			}
			Reset();
			Update();
		}

		/// <summary>
		/// Creates a custom state with a given name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="persistent"></param>
		/// <param name="defaultValue"></param>
		public void Create<T>(string key, bool persistent, T defaultValue)
		{
			if (customStates == null)
			{
				customStates = new Dictionary<string, CustomState>();
			}
			else if (customStates.ContainsKey(key))
			{
				throw new InvalidOperationException("The state '" + key + "' already exists on the property '" + property.Path + "'; can't create a new one with the same key.");
			}
			CustomState customState = new CustomState();
			customState.Type = typeof(T);
			if (persistent)
			{
				customState.PersistentValue = GetPersistentContext(key, defaultValue);
				customState.ValueLastLayout = customState.PersistentValue.WeakValue;
			}
			else
			{
				customState.Value = defaultValue;
				customState.ValueLastLayout = customState.Value;
				customState.DefaultValue = defaultValue;
			}
			customStates.Add(key, customState);
			SendStateChangedNotifications(key);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key)
		{
			bool isPersistent;
			Type valueType;
			return Exists(key, out isPersistent, out valueType);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="isPersistent">If the state exists, this out parameter will be true if the state is persistent.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key, out bool isPersistent)
		{
			Type valueType;
			return Exists(key, out isPersistent, out valueType);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="valueType">If the state exists, this out parameter will contain the type of value that the state contains.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key, out Type valueType)
		{
			bool isPersistent;
			return Exists(key, out isPersistent, out valueType);
		}

		/// <summary>
		/// Determines whether a state with the given key exists.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <param name="isPersistent">If the state exists, this out parameter will be true if the state is persistent.</param>
		/// <param name="valueType">If the state exists, this out parameter will contain the type of value that the state contains.</param>
		/// <returns>True if the state exists, otherwise, false.</returns>
		public bool Exists(string key, out bool isPersistent, out Type valueType)
		{
			switch (key)
			{
			case "Expanded":
				isPersistent = true;
				valueType = typeof(bool);
				return true;
			case "Visible":
				isPersistent = false;
				valueType = typeof(bool);
				return true;
			case "Enabled":
				isPersistent = false;
				valueType = typeof(bool);
				return true;
			default:
			{
				if (customStates != null && customStates.TryGetValue(key, out var value))
				{
					isPersistent = value.PersistentValue != null;
					valueType = value.Type;
					return true;
				}
				isPersistent = false;
				valueType = null;
				return false;
			}
			}
		}

		/// <summary>
		/// Gets the value of a given state as an instance of type T.
		/// </summary>
		/// <typeparam name="T">The type to get the state value as. An <see cref="T:System.InvalidOperationException" /> will be thrown if the state's value type cannot be assigned to T.</typeparam>
		/// <param name="key">The key of the state to get. An <see cref="T:System.InvalidOperationException" /> will be thrown if a state with the given key does not exist.</param>
		/// <returns>The value of the state.</returns>
		public T Get<T>(string key)
		{
			if (customStates != null && customStates.TryGetValue(key, out var value))
			{
				try
				{
					return (T)((value.PersistentValue != null) ? value.PersistentValue.WeakValue : value.Value);
				}
				catch (InvalidCastException)
				{
					throw new InvalidOperationException("Cannot get property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + value.Type.GetNiceName() + "'.");
				}
			}
			throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + property.Path + "'.");
		}

		/// <summary>
		/// Gets the value that a given state contained last layout as an instance of type T.
		/// </summary>
		/// <typeparam name="T">The type to get the state value as. An <see cref="T:System.InvalidOperationException" /> will be thrown if the state's value type cannot be assigned to T.</typeparam>
		/// <param name="key">The key of the state to get. An <see cref="T:System.InvalidOperationException" /> will be thrown if a state with the given key does not exist.</param>
		/// <returns>The value of the state during the last layout event.</returns>
		public T GetLastLayout<T>(string key)
		{
			if (customStates != null && customStates.TryGetValue(key, out var value))
			{
				try
				{
					return (T)value.ValueLastLayout;
				}
				catch (InvalidCastException)
				{
					throw new InvalidOperationException("Cannot get property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + value.Type.GetNiceName() + "'.");
				}
			}
			throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + property.Path + "'.");
		}

		/// <summary>
		/// Sets the value of a given state to a given value.
		/// </summary>
		/// <typeparam name="T">The type to set the state value as. An <see cref="T:System.InvalidOperationException" /> will be thrown if T cannot be assigned to the state's value type.</typeparam>
		/// <param name="key">The key of the state to set the value of. An <see cref="T:System.InvalidOperationException" /> will be thrown if a state with the given key does not exist.</param>
		/// <param name="value">The value to set.</param>
		public void Set<T>(string key, T value)
		{
			if (customStates != null && customStates.TryGetValue(key, out var value2))
			{
				if (typeof(T) != value2.Type)
				{
					throw new InvalidOperationException("Cannot set property state '" + key + "' as a '" + typeof(T).GetNiceName() + "'; the state is of type '" + value2.Type.GetNiceName() + "'.");
				}
				T val = (T)((value2.PersistentValue != null) ? value2.PersistentValue.WeakValue : value2.Value);
				if (!PropertyValueEntry<T>.EqualityComparer(val, value))
				{
					if (LogChanges)
					{
						LogChange(key, val, value);
					}
					if (value2.PersistentValue != null)
					{
						value2.PersistentValue.WeakValue = value;
					}
					else
					{
						value2.Value = value;
					}
					SendStateChangedNotifications(key);
				}
				return;
			}
			throw new InvalidOperationException("The state '" + key + "' does not exist on the property '" + property.Path + "'.");
		}

		private LocalPersistentContext<T> GetPersistentContext<T>(string key, T defaultValue)
		{
			return PersistentContext.GetLocal(TwoWaySerializationBinder.Default.BindToName(property.Tree.TargetType).GetHashCode(), property.Path, index, key, defaultValue);
		}

		internal void Update()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			if (Event.get_current() != null && (int)Event.get_current().get_type() != 8)
			{
				return;
			}
			visibleLastLayout = visible;
			enabledLastLayout = enabled;
			if (expanded != null)
			{
				expandedLastLayout = expanded.Value;
			}
			if (customStates == null)
			{
				return;
			}
			foreach (CustomState item in customStates.GFValueIterator())
			{
				item.ValueLastLayout = ((item.PersistentValue != null) ? item.PersistentValue.WeakValue : item.Value);
			}
		}

		/// <summary>
		/// Cleans the property state and prepares it for cached reuse of its containing PropertyTree. This will also reset the state.
		/// </summary>
		public void CleanForCachedReuse()
		{
			if (customStates != null)
			{
				customStates.Clear();
			}
			Reset();
		}

		/// <summary>
		/// Resets all states to their default values. Persistent states will be updated to their persistent cached value if one exists.
		/// </summary>
		public void Reset()
		{
			enabled = true;
			visible = true;
			if (expanded != null)
			{
				expanded.UpdateLocalValue();
			}
			if (customStates == null)
			{
				return;
			}
			foreach (CustomState item in customStates.GFValueIterator())
			{
				if (item.PersistentValue != null)
				{
					item.PersistentValue.UpdateLocalValue();
				}
				else
				{
					item.Value = item.DefaultValue;
				}
			}
		}

		private void LogChange<T>(string state, T oldValue, T newValue)
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			string text = string.Concat("Property '", property.Path, "'s '", state, "' state changed from '", oldValue, "' to '", newValue, "'");
			text = ((Event.get_current() != null) ? string.Concat(text, " during IMGUI event '", Event.get_current().get_type(), "'") : (text + " while outside IMGUI context"));
			Debug.Log((object)text);
		}

		private void SendStateChangedNotifications(string state)
		{
			if (Event.get_current() != null)
			{
				OdinDrawer[] bakedDrawerArray = property.GetActiveDrawerChain().BakedDrawerArray;
				for (int i = 0; i < bakedDrawerArray.Length; i++)
				{
					IOnSelfStateChangedNotification onSelfStateChangedNotification = bakedDrawerArray[i] as IOnSelfStateChangedNotification;
					if (onSelfStateChangedNotification != null)
					{
						try
						{
							onSelfStateChangedNotification.OnSelfStateChanged(state);
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
			}
			StateUpdater[] stateUpdaters = property.StateUpdaters;
			for (int j = 0; j < stateUpdaters.Length; j++)
			{
				IOnSelfStateChangedNotification onSelfStateChangedNotification2 = stateUpdaters[j] as IOnSelfStateChangedNotification;
				if (onSelfStateChangedNotification2 != null)
				{
					try
					{
						onSelfStateChangedNotification2.OnSelfStateChanged(state);
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
					}
				}
			}
			InspectorProperty inspectorProperty = property.Parent;
			if (inspectorProperty == null && property != property.Tree.RootProperty)
			{
				inspectorProperty = property.Tree.RootProperty;
			}
			if (inspectorProperty == null)
			{
				return;
			}
			int childIndex = property.Index;
			if (Event.get_current() != null)
			{
				OdinDrawer[] bakedDrawerArray2 = inspectorProperty.GetActiveDrawerChain().BakedDrawerArray;
				for (int k = 0; k < bakedDrawerArray2.Length; k++)
				{
					IOnChildStateChangedNotification onChildStateChangedNotification = bakedDrawerArray2[k] as IOnChildStateChangedNotification;
					if (onChildStateChangedNotification != null)
					{
						try
						{
							onChildStateChangedNotification.OnChildStateChanged(childIndex, state);
						}
						catch (Exception ex3)
						{
							Debug.LogException(ex3);
						}
					}
				}
			}
			StateUpdater[] stateUpdaters2 = inspectorProperty.StateUpdaters;
			for (int l = 0; l < stateUpdaters2.Length; l++)
			{
				IOnChildStateChangedNotification onChildStateChangedNotification2 = stateUpdaters2[l] as IOnChildStateChangedNotification;
				if (onChildStateChangedNotification2 != null)
				{
					try
					{
						onChildStateChangedNotification2.OnChildStateChanged(childIndex, state);
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
					}
				}
			}
			InspectorProperty inspectorProperty2 = inspectorProperty;
			while (inspectorProperty2 != null)
			{
				if (Event.get_current() != null)
				{
					OdinDrawer[] bakedDrawerArray3 = inspectorProperty2.GetActiveDrawerChain().BakedDrawerArray;
					for (int m = 0; m < bakedDrawerArray3.Length; m++)
					{
						IRecursiveOnChildStateChangedNotification recursiveOnChildStateChangedNotification = bakedDrawerArray3[m] as IRecursiveOnChildStateChangedNotification;
						if (recursiveOnChildStateChangedNotification != null)
						{
							try
							{
								recursiveOnChildStateChangedNotification.OnChildStateChanged(property, state);
							}
							catch (Exception ex5)
							{
								Debug.LogException(ex5);
							}
						}
					}
				}
				stateUpdaters2 = inspectorProperty2.StateUpdaters;
				for (int n = 0; n < stateUpdaters2.Length; n++)
				{
					IRecursiveOnChildStateChangedNotification recursiveOnChildStateChangedNotification2 = stateUpdaters2[n] as IRecursiveOnChildStateChangedNotification;
					if (recursiveOnChildStateChangedNotification2 != null)
					{
						try
						{
							recursiveOnChildStateChangedNotification2.OnChildStateChanged(property, state);
						}
						catch (Exception ex6)
						{
							Debug.LogException(ex6);
						}
					}
				}
				InspectorProperty inspectorProperty3 = inspectorProperty2.Parent;
				if (inspectorProperty3 == null && inspectorProperty2 != inspectorProperty2.Tree.RootProperty)
				{
					inspectorProperty3 = inspectorProperty2.Tree.RootProperty;
				}
				inspectorProperty2 = inspectorProperty3;
			}
		}
	}
}
