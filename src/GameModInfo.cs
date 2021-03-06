﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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

        public virtual string CacheDir {
            get {
                return "ModInstallerCache";
            }
        }
        public virtual ModBackup[] AdditionalBackups {
            get {
                return new ModBackup[0];
            }
        }

        public abstract ModVersion[] ModVersions { get; }

        private string _CurrentExecutablePath;
        public virtual string CurrentExecutablePath {
            get {
                return _CurrentExecutablePath ?? "";
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    _CurrentExecutablePath = null;
                    OnChangeCurrentExecutablePath?.Invoke(this, null);
                    return;
                }

                if (File.Exists(value) &&
                    value.ToLowerInvariant().EndsWith(ExecutableName.ToLowerInvariant())) {
                    _CurrentExecutablePath = value;
                    OnChangeCurrentExecutablePath?.Invoke(this, value);
                }
            }
        }
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

        public virtual ModVersion CurrentModVersion { get; set; }

        public event Action<GameModInfo, string> OnChangeCurrentExecutablePath;

        public class ModVersion {
            public string Name;
            public string URL;
            public override string ToString() {
                return Name;
            }
        }

        public class ModBackup {
            public string From;
            public string To;
        }

        public ModVersion[] GetAndParseVersions(string url) {
            string data = null;
            using (WebClient wc = new WebClient())
                data = wc.DownloadString(url);

            string[] lines = data.Split('\n');

            List<ModVersion> versions = new List<ModVersion>();
            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                string[] split = line.Split('|');
                versions.Add(new ModVersion { Name = split[0], URL = split[1] });
            }

            return versions.ToArray();
        }

    }
}
