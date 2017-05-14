using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        public virtual string CurrentExecutablePath { get; set; }
        public virtual string CurrentGamePath {
            get {
                string path = CurrentExecutablePath;
                if (string.IsNullOrEmpty(CurrentExecutablePath) || !File.Exists(path))
                    return null;

                if (!string.IsNullOrEmpty(ExecutableName))
                    path = Path.GetDirectoryName(path);

                string exeDir = ExecutableDir;
                while (!string.IsNullOrEmpty(exeDir)) {
                    path = Path.GetDirectoryName(path);
                    exeDir = Path.GetDirectoryName(exeDir);
                }

                return path;
            }
        }

        public class ModVersion {
            public string Name;
            public string URL;
        }

    }
}
