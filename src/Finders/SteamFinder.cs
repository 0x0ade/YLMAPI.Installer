using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoMod.Installer {
    public class SteamFinder : GameFinder {

        // "" -> "
        public readonly static Regex BaseInstallFolderRegex = new Regex(@"BaseInstallFolder[^""]*""\s*""([^""]*)""", RegexOptions.Compiled);

        public override string ID => "steam";

        public string SteamDir {
            get {
                if ((PlatformHelper.Current & Platform.Windows) == Platform.Windows) {
                    string regKey =
                        (PlatformHelper.Current & Platform.X64) == Platform.X64 ?
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam" :
                        @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
                    // Win32 is case-insensitive.
                    return (string) Registry.GetValue(regKey, "InstallPath", null);
                }

                if ((PlatformHelper.Current & Platform.MacOS) == Platform.MacOS) {
                    return Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library/Application Support/Steam");
                }

                if ((PlatformHelper.Current & Platform.Linux) == Platform.Linux) {
                    return Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share/Steam");
                }

                return null;
            }
        }


        public List<string> LibraryDirs {
            get {
                List<string> dirs = new List<string>();

                string steam = SteamDir;
                if (steam == null || !Directory.Exists(steam))
                    return dirs;

                string steamapps = Path.Combine(steam, "SteamApps");
                if (!Directory.Exists(steamapps))
                    // Unix is case-sensitive.
                    steamapps = Path.Combine(steam, "steamapps");
                if (Directory.Exists(steamapps)) {
                    steamapps = Path.Combine(steamapps, "common");
                    if (Directory.Exists(steamapps))
                        dirs.Add(steamapps);
                }


                string config = Path.Combine(steam, "config");
                config = Path.Combine(config, "config.vdf");
                if (!File.Exists(config))
                    return dirs;

                using (StreamReader reader = new StreamReader(config))
                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine().Trim();
                        Match match = BaseInstallFolderRegex.Match(line);
                        if (match.Success)
                            dirs.Add(Regex.Unescape(match.Groups[1].Value));
                    }

                return dirs;
            }
        }

        public override string FindGameDir(string gameid) {
            List<string> dirs = LibraryDirs;
            for (int i = 0; i < dirs.Count; i++) {
                string path = Path.Combine(dirs[0], gameid);
                if (Directory.Exists(path))
                    return path;
            }
            return null;
        }

    }
}
