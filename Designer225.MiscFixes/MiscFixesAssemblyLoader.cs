using System.IO;
using ConditionalAssemblyLoader;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace Designer225.MiscFixes
{
    public sealed class MiscFixesAssemblyLoader : AssemblyLoader<MiscFixesEntryPoint>
    {
        public MiscFixesAssemblyLoader()
        {
            var nativeModule = ModuleHelper.GetModuleInfo("Native");
            var version = nativeModule.Version;

            var binaryPath = Path.Combine(Path.GetFullPath(ModuleHelper.GetModuleFullPath("FixedBanditSpawning")),
                "bin", "Win64_Shipping_Client");
            References.AddRange(new[]
            {
                new ConditionalAssemblyReference(
                    () => version.Major == 1 && version.Minor >= 3 && version.Revision >= 13,
                    "Designer225.MiscFixes.1.2.0", Path.Combine(binaryPath, "Designer225.MiscFixes.1.2.0.dll")),
                new ConditionalAssemblyReference(
                    () => true,
                    "Designer225.MiscFixes.1.1.21.160", Path.Combine(binaryPath, "Designer225.MiscFixes.1.1.21.160.dll"))
            });
            Out = str => Debug.Print(str);
            Error = str => Debug.Print(str);
        }

        protected override void OnAssemblyLoaded(MiscFixesEntryPoint value)
        {
            
        }
    }
}