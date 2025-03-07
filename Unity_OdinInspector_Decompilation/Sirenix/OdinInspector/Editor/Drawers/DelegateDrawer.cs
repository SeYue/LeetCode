using System;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Delegate property drawer. This drawer is rather simplistic for now, and will receive significant upgrades in the future.
	/// </summary>
	[DrawerPriority(0.51, 0.0, 0.0)]
	public class DelegateDrawer<T> : OdinValueDrawer<T> where T : class
	{
		private static MethodInfo invokeMethodField;

		private static bool gotInvokeMethod;

		private Object contextObj;

		private static MethodInfo InvokeMethod
		{
			get
			{
				if (!gotInvokeMethod)
				{
					invokeMethodField = typeof(T).GetMethod("Invoke");
					gotInvokeMethod = true;
				}
				return invokeMethodField;
			}
		}

		/// <summary>
		/// See <see cref="M:Sirenix.OdinInspector.Editor.OdinDrawer.CanDrawTypeFilter(System.Type)" />.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (!type.IsAbstract && typeof(Delegate).IsAssignableFrom(type))
			{
				return InvokeMethod != null;
			}
			return false;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Invalid comparison between Unknown and I4
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Expected O, but got Unknown
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Expected O, but got Unknown
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			IPropertyValueEntry<T> propertyValueEntry = base.ValueEntry;
			Delegate @delegate = (Delegate)(object)propertyValueEntry.SmartValue;
			GUIContent val = GUIHelper.TempContent((string)null);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			for (int i = 0; i < propertyValueEntry.ValueCount; i++)
			{
				Delegate delegate2 = (Delegate)(object)propertyValueEntry.Values[i];
				if ((object)delegate2 != null && delegate2.Method == null)
				{
					flag4 = true;
				}
				else
				{
					flag5 = true;
				}
			}
			if (propertyValueEntry.ValueState == PropertyValueState.NullReference || (flag4 && !flag5))
			{
				flag = true;
				flag3 = true;
				val.set_text("Null");
			}
			else if (propertyValueEntry.ValueState == PropertyValueState.ReferenceValueConflict || flag4)
			{
				flag = true;
				val.set_text("Multiselection Value Conflict");
			}
			else
			{
				MethodInfo method = @delegate.Method;
				object target = @delegate.Target;
				for (int j = 1; j < propertyValueEntry.ValueCount; j++)
				{
					Delegate delegate3 = (Delegate)(object)propertyValueEntry.Values[j];
					if (delegate3.Method != method)
					{
						flag = true;
					}
					if (delegate3.Target != target)
					{
						flag2 = true;
					}
				}
				if (flag)
				{
					val.set_text("Multiselection Method Conflict");
				}
				else
				{
					val.set_text(method.GetFullName());
					if (method.IsStatic)
					{
						val.set_text("static " + val.get_text());
					}
				}
			}
			if (flag3)
			{
				val.set_text(typeof(T).GetNiceName());
			}
			Rect val2 = EditorGUILayout.GetControlRect(label != null, (GUILayoutOption[])(object)new GUILayoutOption[0]);
			val2 = ((label == null) ? EditorGUI.IndentedRect(val2) : EditorGUI.PrefixLabel(val2, label));
			Object val3 = (((object)@delegate == null) ? ((Object)null) : (@delegate.Target as Object));
			if (val3 == (Object)null)
			{
				val3 = contextObj;
			}
			((Rect)(ref val2)).set_width(((Rect)(ref val2)).get_width() * 0.5f);
			if (GUI.Button(val2, val, EditorStyles.get_popup()))
			{
				Popup(propertyValueEntry, val2, val3);
			}
			bool showMixedValue = EditorGUI.get_showMixedValue();
			if (flag2)
			{
				EditorGUI.set_showMixedValue(true);
			}
			((Rect)(ref val2)).set_x(((Rect)(ref val2)).get_x() + ((Rect)(ref val2)).get_width());
			EditorGUI.BeginChangeCheck();
			Object val4 = EditorGUI.ObjectField(val2, val3, typeof(Object), true);
			bool flag6 = EditorGUI.EndChangeCheck();
			if ((object)@delegate != null && (int)Event.get_current().get_type() == 7 && val3 != (Object)null)
			{
				MethodInfo method2 = @delegate.Method;
				string text = (flag2 ? "Target conflict" : ((!(val3 is Component)) ? ((object)val3).GetType().GetNiceName() : ((Object)(val3 as Component).get_gameObject()).get_name()));
				GUIContent val5 = new GUIContent(text, (Texture)(object)AssetPreview.GetMiniThumbnail(val3));
				GUI.Label(val2, val5, EditorStyles.get_objectField());
			}
			else if (val3 == (Object)null)
			{
				GUIContent val6 = new GUIContent("None", (Texture)(object)AssetPreview.GetMiniThumbnail(val3));
				GUI.Label(val2, val6, EditorStyles.get_objectField());
			}
			if (val4 != val3 && flag6)
			{
				for (int k = 0; k < propertyValueEntry.ValueCount; k++)
				{
					propertyValueEntry.Values[k] = null;
				}
				contextObj = val4;
			}
			if (flag2)
			{
				EditorGUI.set_showMixedValue(showMixedValue);
			}
		}

		private void Popup(IPropertyValueEntry<T> entry, Rect rect, Object target)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Expected O, but got Unknown
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			Type returnType = InvokeMethod.ReturnType;
			Type[] parameters = (from n in InvokeMethod.GetParameters()
				select n.ParameterType).ToArray();
			GenericMenu val = new GenericMenu();
			if (target == (Object)null)
			{
				val.AddDisabledItem(new GUIContent("No target selected"));
			}
			else
			{
				GameObject val2 = target as GameObject;
				Component val3 = target as Component;
				if ((Object)(object)val2 == (Object)null && (Object)(object)val3 != (Object)null)
				{
					val2 = val3.get_gameObject();
				}
				if ((Object)(object)val2 != (Object)null)
				{
					RegisterGameObject(val, entry, "", val2, returnType, parameters);
				}
				else
				{
					RegisterUnityObject(val, entry, "", target, returnType, parameters);
				}
				if (val.GetItemCount() == 0)
				{
					val.AddDisabledItem(new GUIContent("No suitable method found on target"));
				}
			}
			val.DropDown(rect);
		}

		private void RegisterGameObject(GenericMenu menu, IPropertyValueEntry<T> entry, string path, GameObject go, Type returnType, Type[] parameters)
		{
			RegisterUnityObject(menu, entry, path + "/GameObject", (Object)(object)go, returnType, parameters);
			Component[] components = go.GetComponents<Component>();
			foreach (Component val in components)
			{
				RegisterUnityObject(menu, entry, path + "/" + ((object)val).GetType().GetNiceName(), (Object)(object)val, returnType, parameters);
			}
		}

		private void RegisterUnityObject(GenericMenu menu, IPropertyValueEntry<T> entry, string path, Object obj, Type returnType, Type[] parameters)
		{
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Expected O, but got Unknown
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Expected O, but got Unknown
			MethodInfo[] array = ((object)obj).GetType().GetAllMembers<MethodInfo>(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate(MethodInfo n)
			{
				if (n.ReturnType != returnType)
				{
					return false;
				}
				ParameterInfo[] parameters2 = n.GetParameters();
				if (parameters2.Length != parameters.Length)
				{
					return false;
				}
				for (int k = 0; k < parameters2.Length; k++)
				{
					if (parameters2[k].ParameterType != parameters[k])
					{
						return false;
					}
				}
				return true;
			})
				.ToArray();
			MethodInfo[] array2 = array;
			foreach (MethodInfo methodInfo in array2)
			{
				string text = methodInfo.GetFullName();
				MethodInfo closureMethod = methodInfo;
				if (methodInfo.DeclaringType != ((object)obj).GetType())
				{
					text = methodInfo.DeclaringType.GetNiceName() + "/" + text;
				}
				if (methodInfo.IsStatic)
				{
					text += " (static)";
				}
				MenuFunction val = (MenuFunction)delegate
				{
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						Delegate @delegate = ((!closureMethod.IsStatic) ? Delegate.CreateDelegate(typeof(T), obj, closureMethod) : Delegate.CreateDelegate(typeof(T), null, closureMethod));
						for (int j = 0; j < entry.ValueCount; j++)
						{
							entry.Values[j] = (T)(object)@delegate;
						}
						contextObj = null;
					});
				};
				menu.AddItem(new GUIContent((path + "/" + text).TrimStart('/')), false, val);
			}
		}
	}
}
