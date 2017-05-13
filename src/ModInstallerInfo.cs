using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public abstract class ModInstallerInfo {

        public abstract string GameName { get; }

        public abstract string ModName { get; }
        public abstract string ModInstallerName { get; }

        public abstract Image HeaderImage { get; }
        public abstract Image BackgroundImage { get; }

        public abstract string ExecutableName { get; }
        public abstract string[] Assemblies { get; }
        public abstract string GameIDSteam { get; }
        public abstract string GameIDGOG { get; }

        public abstract ModVersion[] ModVersions { get; }

        public class ModVersion {
            public string Name;
            public string URL;
        }

    }
}
