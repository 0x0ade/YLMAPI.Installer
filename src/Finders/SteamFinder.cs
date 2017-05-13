using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoMod.Installer {
    public class SteamFinder : GameFinder {

        public override string ID => "steam";

        public override string Find(string gameid) {
            return null;
        }

    }
}
