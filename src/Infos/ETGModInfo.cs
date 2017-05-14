using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer.ETGMod {
    public class ETGModInfo : GameModInfo {

        public readonly static Random RNG = new Random();

        public override string GameName {
            get {
                return "Enter the Gungeon";
            }
        }

        public override string ModName {
            get {
                return "Mod the Gungeon";
            }
        }

        public override string ModInstallerName {
            get {
                return "ETGMod.Installer";
            }
        }

        public override Image HeaderImage {
            get {
                return Properties.Resources.header;
            }
        }

        public override Image BackgroundImage {
            get {
                if (RNG.NextDouble() < 0.5D)
                    return Properties.Resources.background_2;
                return Properties.Resources.background;
            }
        }

        public override string ExecutableDir {
            get {
                if ((PlatformHelper.Current & Platform.MacOS) == Platform.MacOS)
                    return "EtG_OSX.app/Contents/MacOS";

                return null;
            }
        }

        public override string ExecutableName {
            get {
                string env = Environment.GetEnvironmentVariable("ETGMOD_EXE");
                if (!string.IsNullOrEmpty(env))
                    return env;

                if ((PlatformHelper.Current & Platform.Windows) == Platform.Windows)
                    // TODO: Is the 32-bit Windows executable also named YookaLaylee64.exe?
                    return "EtG.exe";

                if ((PlatformHelper.Current & Platform.MacOS) == Platform.MacOS)
                    return "EtG_OSX";

                if ((PlatformHelper.Current & Platform.Linux) == Platform.Linux)
                    return IntPtr.Size == 4 /*(32 bit)*/ ? "EtG.x86" : "EtG.x86_64";

                return null;
            }
        }

        public override string[] Assemblies {
            get {
                if ((PlatformHelper.Current & Platform.Windows) == Platform.Windows)
                    return new string[] {
                        "EtG_Data/Managed/Assembly-CSharp.dll"
                    };

                if ((PlatformHelper.Current & Platform.MacOS) == Platform.MacOS)
                    return new string[] {
                        "EtG_OSX.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll"
                    };

                if ((PlatformHelper.Current & Platform.Linux) == Platform.Linux)
                    return new string[] {
                        "EtG_Data/Managed/Assembly-CSharp.dll"
                    };

                return null;
            }
        }

        public override Dictionary<string, string> GameIDs {
            get {
                return new Dictionary<string, string>() {
                    { "steam", "Enter the Gungeon" },
                    { "gog", "1456912569" }
                };
            }
        }

        public override ModVersion[] ModVersions {
            get {
                return new ModVersion[0];
            }
        }

    }
}
