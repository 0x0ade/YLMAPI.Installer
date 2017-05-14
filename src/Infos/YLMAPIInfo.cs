using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer.YLMAPI {
    public class YLMAPIInfo : ModInstallerInfo {

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

        public override string ExecutableName {
            get {
                return "YookaLaylee64.exe";
            }
        }

        public override string[] Assemblies {
            get {
                return new string[] {
                    "YookaLaylee64_Data/Managed/Assembly-CSharp.dll"
                };
            }
        }

        public override Dictionary<string, string> GameIDs {
            get {
                return new Dictionary<string, string>() {
                    { "steam", "Yooka-Laylee" },
                    { "gog", "1445853962" }
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
