using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public abstract class GameModInfo {

        public abstract string GameName { get; }

        public abstract string ModName { get; }
        public abstract string ModInstallerName { get; }

        public abstract Image HeaderImage { get; }
        public abstract Image BackgroundImage { get; }

        public abstract string ExecutableDir { get; }
        public abstract string ExecutableName { get; }
        public abstract string[] Assemblies { get; }
        public abstract Dictionary<string, string> GameIDs { get; }

        public abstract ModVersion[] ModVersions { get; }

        public class ModVersion {
            public string Name;
            public string URL;
        }

    }
}
