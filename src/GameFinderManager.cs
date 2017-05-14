using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MonoMod.Installer {
    public static class GameFinderManager {

        private readonly static Type[] _EmptyTypeArray = new Type[0];
        private readonly static object[] _EmptyObjectArray = new object[0];

        public readonly static List<GameFinder> Finders = new List<GameFinder>();

        static GameFinderManager() {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < types.Length; i++) {
                Type type = types[i];
                if (!typeof(GameFinder).IsAssignableFrom(type) || type.IsAbstract)
                    continue;
                Finders.Add((GameFinder) type.GetConstructor(_EmptyTypeArray).Invoke(_EmptyObjectArray));
            }
        }

        public static string Find(Dictionary<string, string> idmap) {
            for (int i = 0; i < Finders.Count; i++) {
                GameFinder finder = Finders[i];
                string s;
                if (!idmap.TryGetValue(finder.ID, out s))
                    continue;
                if ((s = finder.Find(s)) != null)
                    return s;
            }

            return null;
        }

    }
}
