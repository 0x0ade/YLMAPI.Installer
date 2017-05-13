using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    internal sealed class CachedInfo : ModInstallerInfo {

        internal ModInstallerInfo _From;

        internal string _GameName;
        public override string GameName => _GameName;

        internal string _ModName;
        public override string ModName => _ModName;

        internal string _ModInstallerName;
        public override string ModInstallerName => _ModInstallerName;

        internal Image _HeaderImage;
        public override Image HeaderImage => _HeaderImage;

        internal Image _BackgroundImage;
        public override Image BackgroundImage => _BackgroundImage;

        internal string _ExecutableName;
        public override string ExecutableName => _ExecutableName;

        internal string[] _Assemblies;
        public override string[] Assemblies => _Assemblies;

        internal Dictionary<string, string> _GameIDs;
        public override Dictionary<string, string> GameIDs => _GameIDs;

        internal ModVersion[] _ModVersions;
        public override ModVersion[] ModVersions {
            get {
                if (_ModVersions != null)
                    return _ModVersions;
                return _ModVersions = _From.ModVersions;
            }
        }

        public CachedInfo(ModInstallerInfo from) {
            _From = from;
            _GameName = from.GameName;
            _ModName = from.ModName;
            _ModInstallerName = from.ModInstallerName;
            _HeaderImage = from.HeaderImage;
            _BackgroundImage = from.BackgroundImage;
            _ExecutableName = from.ExecutableName;
            _Assemblies = from.Assemblies;
            _GameIDs = from.GameIDs;
        }

    }
}
