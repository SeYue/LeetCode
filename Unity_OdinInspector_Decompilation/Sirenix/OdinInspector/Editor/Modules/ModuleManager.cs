using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Modules
{
	public class ModuleManager
	{
		public ModuleDataManager DataManager;

		public List<ModuleDefinition> Modules;

		public static ModuleManager CreateDefault()
		{
			ModuleManager moduleManager = new ModuleManager
			{
				DataManager = new ModuleDataManager
				{
					DataPath = SirenixAssetPaths.OdinPath + "Modules",
					InstallPath = SirenixAssetPaths.OdinPath + "Modules"
				},
				Modules = new List<ModuleDefinition>()
			};
			moduleManager.Modules.Add(new UnityMathematicsModuleDefinition
			{
				ModuleManager = moduleManager
			});
			moduleManager.Modules.Add(new ECSModuleDefinition
			{
				ModuleManager = moduleManager
			});
			return moduleManager;
		}

		public bool Refresh()
		{
			bool result = false;
			foreach (ModuleDefinition module in Modules)
			{
				bool flag = module.CheckIsActivated();
				bool flag2 = module.CheckSupportsCurrentEnvironment();
				if (flag && !flag2)
				{
					bool flag3 = false;
					if (GlobalConfig<OdinModuleConfig>.HasInstanceLoaded)
					{
						ModuleConfiguration config = GlobalConfig<OdinModuleConfig>.Instance.GetConfig(module);
						if (config != null && config.ActivationSettings == ActivationSettings.GlobalSettings)
						{
							flag3 = ((GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings != 0) ? (GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings == OdinModuleConfig.ModuleAutomationSettings.Automatic) : AskActivationAutomationQuestion(module, activate: false, GlobalConfig<OdinModuleConfig>.Instance));
						}
					}
					if (flag3)
					{
						Debug.Log((object)("Automatically deactivating Odin Module '" + module.NiceName + "', because its dependencies have gone missing..."));
						module.Deactivate();
						result = true;
					}
				}
				else if (!flag && flag2)
				{
					bool flag4 = false;
					if (!module.UnstableExperimental && GlobalConfig<OdinModuleConfig>.HasInstanceLoaded)
					{
						ModuleConfiguration config2 = GlobalConfig<OdinModuleConfig>.Instance.GetConfig(module);
						if (config2 != null && config2.ActivationSettings == ActivationSettings.GlobalSettings)
						{
							flag4 = GlobalConfig<OdinModuleConfig>.Instance.ModuleTogglingSettings switch
							{
								OdinModuleConfig.ModuleAutomationSettings.Ask => AskActivationAutomationQuestion(module, activate: true, GlobalConfig<OdinModuleConfig>.Instance), 
								OdinModuleConfig.ModuleAutomationSettings.Automatic => true, 
								_ => false, 
							};
						}
					}
					if (flag4)
					{
						Debug.Log((object)string.Concat("Automatically activating Odin Module '", module.NiceName, "' version '", module.LatestVersion, "', because its dependencies were detected..."));
						module.Activate();
						result = true;
					}
				}
				else
				{
					if (!flag)
					{
						continue;
					}
					ModuleManifest moduleManifest = module.LoadManifest();
					if (moduleManifest != null && moduleManifest.Version < module.LatestVersion)
					{
						bool flag5 = false;
						if (GlobalConfig<OdinModuleConfig>.Instance.ModuleUpdateSettings switch
						{
							OdinModuleConfig.ModuleAutomationSettings.Ask => AskUpdateAutomationQuestion(module, moduleManifest.Version, GlobalConfig<OdinModuleConfig>.Instance), 
							OdinModuleConfig.ModuleAutomationSettings.Automatic => true, 
							_ => false, 
						})
						{
							Debug.Log((object)string.Concat("Automatically updating Odin Module '", module.NiceName, "' from version '", moduleManifest.Version, "' to '", module.LatestVersion, "'"));
							module.Deactivate();
							module.Activate();
							result = true;
						}
					}
				}
			}
			return result;
		}

		private bool AskActivationAutomationQuestion(ModuleDefinition module, bool activate, OdinModuleConfig instance)
		{
			string text = "";
			if (!string.IsNullOrEmpty(module.DependenciesDescription))
			{
				text = " (" + module.DependenciesDescription + ")";
			}
			string text2 = (activate ? ("The Odin Module '" + module.NiceName + "' is not activated, but the conditions for its activation have been detected" + text + ". Do you want to automatically activate the module?") : ("The Odin Module '" + module.NiceName + "' is activated, but some of its dependencies" + text + " are missing and you will likely get compiler errors. Do you want to automatically deactivate the module?"));
			switch (EditorUtility.DisplayDialogComplex("Odin Module automation", text2, "Always automate modules", "Just this once", "Never do this"))
			{
			case 0:
				instance.ModuleTogglingSettings = OdinModuleConfig.ModuleAutomationSettings.Automatic;
				EditorUtility.SetDirty((Object)(object)instance);
				AssetDatabase.SaveAssets();
				return true;
			case 1:
				return true;
			default:
				instance.ModuleTogglingSettings = OdinModuleConfig.ModuleAutomationSettings.Manual;
				EditorUtility.SetDirty((Object)(object)instance);
				AssetDatabase.SaveAssets();
				return false;
			}
		}

		private bool AskUpdateAutomationQuestion(ModuleDefinition module, Version old, OdinModuleConfig instance)
		{
			string text = string.Concat("The installed Odin Module '", module.NiceName, "' is out of date (", old, "), and a new version is available (", module.LatestVersion, "). Do you want to automatically update the module?");
			switch (EditorUtility.DisplayDialogComplex("Odin Module automation", text, "Always update modules", "Just this once", "Never do this"))
			{
			case 0:
				instance.ModuleUpdateSettings = OdinModuleConfig.ModuleAutomationSettings.Automatic;
				EditorUtility.SetDirty((Object)(object)instance);
				AssetDatabase.SaveAssets();
				return true;
			case 1:
				return true;
			default:
				instance.ModuleUpdateSettings = OdinModuleConfig.ModuleAutomationSettings.Manual;
				EditorUtility.SetDirty((Object)(object)instance);
				AssetDatabase.SaveAssets();
				return false;
			}
		}

		public void PackageAllModules()
		{
			foreach (ModuleDefinition module in Modules)
			{
				ModuleData moduleDataForPackaging = module.GetModuleDataForPackaging();
				byte[] data = ModuleData.Serialize(moduleDataForPackaging);
				DataManager.SaveData(module.ID, data);
			}
			AssetDatabase.Refresh();
		}
	}
}
