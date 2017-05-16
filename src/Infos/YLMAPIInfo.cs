using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer.YLMAPI {
    public class YLMAPIInfo : GameModInfo {

        public readonly static Random RNG = new Random();

        public override string GameName {
            get {
                return "Yooka-Laylee";
            }
        }

        public override string ModName {
            get {
                return "YLMAPI";
            }
        }

        public override string ModInstallerName {
            get {
                return "YLMAPI.Installer";
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
                    // From ETG:
                    // /Users/$USER/Library/Application Support/Steam/SteamApps/common/Enter the Gungeon/EtG_OSX.app/Contents/MacOS/EtG_OSX
                    return "YookaLaylee_OSX.app/Contents/MacOS".NormalizeFilepath();

                return null;
            }
        }

        public override string ExecutableName {
            get {
                string env = Environment.GetEnvironmentVariable("YLMAPI_EXE");
                if (!string.IsNullOrEmpty(env))
                    return env;

                if ((PlatformHelper.Current & Platform.Windows) == Platform.Windows)
                    // TODO: Is the 32-bit Windows executable also named YookaLaylee64.exe?
                    return "YookaLaylee64.exe";

                // TODO: What are the macOS and Linux executable names?
                if ((PlatformHelper.Current & Platform.MacOS) == Platform.MacOS)
                    // From ETG:
                    // /Users/$USER/Library/Application Support/Steam/SteamApps/common/Enter the Gungeon/EtG_OSX.app/Contents/MacOS/EtG_OSX
                    return "YookaLaylee_OSX";

                if ((PlatformHelper.Current & Platform.Linux) == Platform.Linux)
                    return IntPtr.Size == 4 /*(32 bit)*/ ? "YookaLaylee.x86" : "YookaLaylee.x86_64";

                return null;
            }
        }

        public override string[] Assemblies {
            get {
                if ((PlatformHelper.Current & Platform.Windows) == Platform.Windows)
                    return new string[] {
                        "YookaLaylee64_Data/Managed/Assembly-CSharp.dll".NormalizeFilepath()
                    };

                if ((PlatformHelper.Current & Platform.MacOS) == Platform.MacOS)
                    return new string[] {
                        "YookaLaylee_OSX.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll".NormalizeFilepath()
                    };

                if ((PlatformHelper.Current & Platform.Linux) == Platform.Linux)
                    return new string[] {
                        "YookaLaylee_Data/Managed/Assembly-CSharp.dll".NormalizeFilepath()
                    };

                return null;
            }
        }

        public override Dictionary<string, string> GameIDs {
            get {
                return new Dictionary<string, string>() {
                    { "steam", "YookaLaylee" },
                    { "gog", "1445853962" }
                };
            }
        }

        public override ModVersion[] ModVersions {
            get {
                return GetAndParseVersions("http://ylmapi.github.io/versions.txt");
            }
        }

    }
}
