using System;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	internal class OdinGettingStartedWindowData
	{
		[HideReferenceObjectPicker]
		public class Page
		{
			public string Title;

			public Section[] Sections = new Section[0];
		}

		[HideReferenceObjectPicker]
		public class Section
		{
			[FoldoutGroup("$Title", 0f)]
			public string Title;

			[FoldoutGroup("$Title", 0f)]
			public Card[] Cards = new Card[0];

			public int ColCount = 2;

			public Action<OdinGettingStartedWindow> OnInspectorGUI = delegate
			{
			};
		}

		[HideReferenceObjectPicker]
		public class Card
		{
			[FoldoutGroup("$Title", 0f)]
			public string Title;

			[Multiline]
			[FoldoutGroup("$Title", 0f)]
			public string Description;

			[FoldoutGroup("$Title", 0f)]
			public string Package;

			[FoldoutGroup("$Title", 0f)]
			public string AssetPathFromPackage;

			[FoldoutGroup("$Title", 0f)]
			public Page SubPage;

			[FoldoutGroup("$Title", 0f)]
			public string SubPageTitle;

			[FoldoutGroup("$Title", 0f)]
			public GUIStyle Style;

			[FoldoutGroup("$Title", 0f)]
			public BtnAction[] CustomActions = new BtnAction[0];
		}

		[HideReferenceObjectPicker]
		public class BtnAction
		{
			public string Name;

			public Action Action;

			public BtnAction(string name, Action action)
			{
				Name = name;
				Action = action;
			}
		}

		private const string EDITOR_WINDOW_DEMO_FOLDER = "Editor Windows";

		private const string BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_CS = "/Scripts/Editor/BasicOdinEditorExampleWindow.cs";

		private const string OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_CS = "/Scripts/Editor/OverrideGetTargetsExampleWindow.cs";

		private const string QUICKLY_INSPECT_OBJECTS_CS = "/Scripts/Editor/QuicklyInspectObjects.cs";

		private const string ODIN_MENU_EDITOR_WINDOW_EXAMPLE_CS = "/Scripts/Editor/OdinMenuEditorWindowExample.cs";

		private const string QUICKLY_INSPECT_OBJECTS_TYPE = "Sirenix.OdinInspector.Demos.QuicklyInspectObjects";

		private const string BASIC_ODIN_EDITOR_EXAMPLE_WINDOWS_TYPE = "Sirenix.OdinInspector.Demos.BasicOdinEditorExampleWindow";

		private const string ODIN_MENU_EDITOR_WINDOW_EXAMPLE_TYPE = "Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample";

		private const string OVERRIDE_GET_TARGETS_EXAMPLE_WINDOW_TYPE = "Sirenix.OdinInspector.Demos.OverrideGetTargetsExampleWindow";

		private const string SAMPLE_RPG_EDITOR_FOLDER = "Sample - RPG Editor";

		private const string RPG_EDITOR_WINDOW_CS = "/Scripts/Editor/RPGEditorWindow.cs";

		private const string RPG_EDITOR_WINDOW_TYPE = "Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow";

		private const string CUSTOM_ATTRIBUTE_PROCESSORS_FOLDER = "Custom Attribute Processors";

		private const string CUSTOM_ATTRIBUTE_PROCESSORS_SCENE = "/Custom Attribute Processors.unity";

		private const string CUSTOM_DRAWERS_FOLDER = "Custom Drawers";

		private const string CUSTOM_DRAWERS_SCENE = "/Custom Drawers.unity";

		private const string ATTRIBUTES_OVERVIEW_FOLDER = "Attributes Overview";

		private const string ATTRIBUTES_OVERVIEW_SCENE = "/Attributes Overview.unity";

		private static string DemoFolder => SirenixAssetPaths.SirenixPluginPath + "Demos/";

		public static Page OdinEditorWindowsIntroduction
		{
			get
			{
				Page page = new Page();
				page.Title = "Odin Editor Windows";
				page.Sections = new Section[1]
				{
					new Section
					{
						Title = "Odin Editor Window Examples",
						Cards = new Card[4]
						{
							new Card
							{
								Title = "Basic Odin Editor Window",
								Description = "Inherit from OdinEditorWindow instead of EditorWindow. This will enable you to render fields, properties and methods and make editor windows using attributes, without writing any custom editor code.",
								Package = DemoFolder + "Editor Windows.unitypackage",
								AssetPathFromPackage = DemoFolder + "Editor Windows/Scripts/Editor/BasicOdinEditorExampleWindow.cs",
								CustomActions = new BtnAction[2]
								{
									new BtnAction("Open Window", delegate
									{
										AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.BasicOdinEditorExampleWindow").GetMethod("OpenWindow", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
									}),
									new BtnAction("Open Script", delegate
									{
										AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(DemoFolder + "Editor Windows/Scripts/Editor/BasicOdinEditorExampleWindow.cs"));
									})
								}
							},
							new Card
							{
								Title = "Override GetTargets()",
								Description = "Odin Editor Windows are not limited to drawing themselves; you can override GetTarget() or GetTargets() to make them display scriptable objects, components or any arbitrary types (except value types like structs).",
								Package = DemoFolder + "Editor Windows.unitypackage",
								AssetPathFromPackage = DemoFolder + "Editor Windows/Scripts/Editor/OverrideGetTargetsExampleWindow.cs",
								CustomActions = new BtnAction[2]
								{
									new BtnAction("Open Window", delegate
									{
										AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.OverrideGetTargetsExampleWindow").GetMethod("OpenWindow", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
									}),
									new BtnAction("Open Script", delegate
									{
										AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(DemoFolder + "Editor Windows/Scripts/Editor/OverrideGetTargetsExampleWindow.cs"));
									})
								}
							},
							new Card
							{
								Title = "Quickly inspect objects",
								Description = "Call OdinEditorWindow.InspectObject(myObj) to quickly pop up an editor window for any given object. This is a great way to quickly debug objects or make custom editor windows on the spot!",
								Package = DemoFolder + "Editor Windows.unitypackage",
								AssetPathFromPackage = DemoFolder + "Editor Windows/Scripts/Editor/QuicklyInspectObjects.cs",
								CustomActions = new BtnAction[2]
								{
									new BtnAction("Open Window", delegate
									{
										OdinEditorWindow.InspectObject(Activator.CreateInstance(AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.QuicklyInspectObjects")));
									}),
									new BtnAction("Open Script", delegate
									{
										AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(DemoFolder + "Editor Windows/Scripts/Editor/QuicklyInspectObjects.cs"));
									})
								}
							},
							new Card
							{
								Title = "Odin Menu Editor Windows",
								Description = "Derive from OdinMenuEditorWindow to create windows that inspect a custom tree of target objects. These are great for organizing your project, and managing Scriptable Objects etc. Odin itself uses this to draw its preferences window.",
								Package = DemoFolder + "Editor Windows.unitypackage",
								AssetPathFromPackage = DemoFolder + "Editor Windows/Scripts/Editor/OverrideGetTargetsExampleWindow.cs",
								CustomActions = new BtnAction[2]
								{
									new BtnAction("Open Window", delegate
									{
										AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.OdinMenuEditorWindowExample").GetMethod("OpenWindow", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
									}),
									new BtnAction("Open Script", delegate
									{
										AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(DemoFolder + "Editor Windows/Scripts/Editor/OdinMenuEditorWindowExample.cs"));
									})
								}
							}
						}
					}
				};
				return page;
			}
		}

		public static Page MainPage
		{
			get
			{
				Page page = new Page();
				page.Title = "Odin Inspector";
				page.Sections = new Section[4]
				{
					new Section
					{
						Title = "Getting Started",
						Cards = new Card[4]
						{
							new Card
							{
								Title = "Odin Attributes Overview",
								Description = "The best way to get started using Odin is to open the Attributes Overview window found at Tools > Odin Inspector > Attribute Overview.",
								CustomActions = new BtnAction[1]
								{
									new BtnAction("Open Attributes Overview", delegate
									{
										AttributesExampleWindow.OpenWindow();
									})
								}
							},
							new Card
							{
								Title = "Odin Editor Windows",
								Description = "You can use Odin to rapidly create custom Editor Windows to help organize your project data. This is where Odin can really help boost your workflow.",
								SubPage = OdinEditorWindowsIntroduction,
								SubPageTitle = "Learn More"
							},
							new Card
							{
								Title = "The Static Inspector",
								Description = "If you're a programmer, then you're likely going find the static inspector helpful during debugging and testing. Just open up the window, and start using it! You can find the utility under 'Tools > Odin Inspector > Static Inspector'.",
								CustomActions = new BtnAction[1]
								{
									new BtnAction("Open the Static Inspector", delegate
									{
										StaticInspectorWindow.InspectType(typeof(Time), StaticInspectorWindow.AccessModifierFlags.All, StaticInspectorWindow.MemberTypeFlags.AllButObsolete);
									})
								}
							},
							new Card
							{
								Title = "The Serialization Debugger",
								Description = "If you are utilizing Odin's serialization, the Serialization Debugger can show you which members of any given type are being serialized, and whether they are serialized by Unity, Odin or both. You can find the utility under 'Tools > Odin Inspector > Serialization Debugger' or from the context menu in the inspector.",
								CustomActions = new BtnAction[1]
								{
									new BtnAction("Open the Serialization Debugger", delegate
									{
										SerializationDebuggerWindow.ShowWindow();
									})
								}
							}
						}
					},
					new Section
					{
						Title = "Advanced Topics",
						Cards = new Card[2]
						{
							new Card
							{
								Title = "Custom Drawers",
								Description = "Making custom drawers in Odin is 10x faster and 10x more powerful than in vanilla Unity. Drawers are strongly typed, with generic resolution, and have full support for the layout system - no need to calculate any property heights.",
								AssetPathFromPackage = DemoFolder + "Custom Drawers/Custom Drawers.unity",
								Package = DemoFolder + "Custom Drawers.unitypackage",
								CustomActions = new BtnAction[1]
								{
									new BtnAction("Open Example Scene", delegate
									{
										OpenScene(DemoFolder + "Custom Drawers/Custom Drawers.unity");
									})
								}
							},
							new Card
							{
								Title = "Attribute Processors",
								Description = "You can take complete control over how Odin finds its members to display and which attributes to put on those members. This can be extremely useful for automation and providing support and editor customizations for third-party libraries you don't own the code for.",
								AssetPathFromPackage = DemoFolder + "Custom Attribute Processors/Custom Attribute Processors.unity",
								Package = DemoFolder + "Custom Attribute Processors.unitypackage",
								CustomActions = new BtnAction[1]
								{
									new BtnAction("Open Example Scene", delegate
									{
										OpenScene(DemoFolder + "Custom Attribute Processors/Custom Attribute Processors.unity");
									})
								}
							}
						}
					},
					new Section
					{
						Title = "Sample Projects",
						Cards = new Card[1]
						{
							new Card
							{
								Title = "RPG Editor",
								Description = "This project showcases Odin Editor Windows, Odin Selectors, various attribute combinations, and custom drawers to build a feature-rich editor window for managing scriptable objects.",
								AssetPathFromPackage = DemoFolder + "Sample - RPG Editor/Scripts/Editor/RPGEditorWindow.cs",
								Package = DemoFolder + "Sample - RPG Editor.unitypackage",
								CustomActions = new BtnAction[2]
								{
									new BtnAction("Open Window", delegate
									{
										AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Demos.RPGEditor.RPGEditorWindow").GetMethod("Open", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(null, null);
									}),
									new BtnAction("Open Script", delegate
									{
										AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(DemoFolder + "Sample - RPG Editor/Scripts/Editor/RPGEditorWindow.cs"));
									})
								}
							}
						}
					},
					new Section
					{
						Title = "Online Resources",
						OnInspectorGUI = delegate(OdinGettingStartedWindow window)
						{
							window.DrawFooter();
						}
					}
				};
				return page;
			}
		}

		private static void OpenScene(string scenePath)
		{
			UnityEditorEventUtility.DelayAction(delegate
			{
				SceneAsset val = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
				AssetDatabase.OpenAsset((Object)(object)val);
				if (Object.op_Implicit((Object)(object)val))
				{
					UnityEditorEventUtility.DelayAction(delegate
					{
						(from x in Object.FindObjectsOfType<Transform>()
							where (Object)(object)x.get_parent() == (Object)null && x.get_childCount() > 0
							orderby x.GetSiblingIndex() descending
							select ((Component)((Component)x).get_transform().GetChild(0)).get_gameObject()).ForEach(delegate(GameObject x)
						{
							EditorGUIUtility.PingObject((Object)(object)x);
						});
					});
				}
			});
		}
	}
}
