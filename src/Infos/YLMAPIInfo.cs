using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
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

        public override string GameIDSteam {
            get {
                return "Yooka-Laylee";
            }
        }

        public override string GameIDGOG {
            get {
                // HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\GOG.com\Games\1445853962
                // exe or EXE
                return "1445853962";
            }
        }

        public override ModVersion[] ModVersions {
            get {
                throw new NotImplementedException();
            }
        }

    }
}
