using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.StateUpdaters;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;

[assembly: RegisterDefaultActionResolver(typeof(MethodPropertyActionResolverCreator), 20.0)]
[assembly: RegisterDefaultActionResolver(typeof(ExpressionActionResolverCreator), -10.0)]
[assembly: RegisterDefaultActionResolver(typeof(MethodReferenceActionResolverCreator), 10.0)]
[assembly: ContainsOdinResolvers]
[assembly: RegisterStateUpdater(typeof(GroupVisibilityStateUpdater<>), 0.0)]
[assembly: RegisterStateUpdater(typeof(OnInspectorDisposeStateUpdater), -10000.0)]
[assembly: RegisterStateUpdater(typeof(OnInspectorInitStateUpdater), 10000.0)]
[assembly: RegisterStateUpdater(typeof(ShowIfAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(EnableGUIAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(OnStateUpdateAttributeStateUpdater), 0.0)]
[assembly: RegisterValidator(typeof(AssetsOnlyValidator<>))]
[assembly: RegisterValidator(typeof(ChildGameObjectsOnlyValidator<>))]
[assembly: RegisterValidator(typeof(DetailedInfoBoxValidator))]
[assembly: RegisterValidator(typeof(FilePathValidator))]
[assembly: RegisterValidator(typeof(FolderPathValidator))]
[assembly: RegisterValidator(typeof(InfoBoxValidator))]
[assembly: RegisterValidator(typeof(MaxValueValidator<>))]
[assembly: RegisterValidator(typeof(MinMaxSliderValidator<>))]
[assembly: RegisterValidator(typeof(MinValueValidator<>))]
[assembly: RegisterValidator(typeof(RangeValidator<>))]
[assembly: RegisterValidator(typeof(PropertyRangeValidator<>))]
[assembly: RegisterValidator(typeof(RequireComponentValidator<>))]
[assembly: RegisterValidator(typeof(RequiredValidator<>))]
[assembly: RegisterValidator(typeof(SceneObjectsOnlyValidator<>))]
[assembly: RegisterValidator(typeof(ValidateInputAttributeValidator<>))]
[assembly: RegisterDefaultValueResolverCreator(typeof(MethodPropertyValueResolverCreator), 20.0)]
[assembly: RegisterDefaultValueResolverCreator(typeof(ExpressionValueResolverCreator), -5.0)]
[assembly: RegisterDefaultValueResolverCreator(typeof(MemberReferenceValueResolverCreator), -10.0)]
[assembly: RegisterStateUpdater(typeof(ShowInInlineEditorsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableInInlineEditorsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableInNonPrefabsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableInPrefabAssetsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableInPrefabInstancesAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableInPrefabsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInInlineEditorsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInNonPrefabsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInPrefabAssetsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInPrefabInstancesAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInPrefabsAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableIfAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableInEditorModeAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(DisableInPlayModeAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInPlayModeAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInInspectorAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideIfAttributeStateUpdater), 0.0)]
[assembly: StaticInitializeBeforeDrawing(new Type[] { typeof(TwoDimensionalEnumArrayDrawerLocator) })]
[assembly: AtomContainer]
[assembly: RegisterStateUpdater(typeof(EnableIfAttributeStateUpdater), 0.0)]
[assembly: RegisterStateUpdater(typeof(HideInEditorModeAttributeStateUpdater), 0.0)]
[assembly: CLSCompliant(false)]
[assembly: AssemblyTitle("Sirenix.OdinInspector.Editor")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Sirenix IVS")]
[assembly: AssemblyProduct("Sirenix.OdinInspector.Editor")]
[assembly: AssemblyCopyright("Copyright © 2017")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("a4865f1a-b450-4ed8-a368-670db22f409c")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: PersistentAssembly]
[assembly: SirenixBuildName("Personal")]
[assembly: SirenixBuildVersion("3.0.9.0")]
[assembly: AssemblyVersion("1.0.0.0")]
