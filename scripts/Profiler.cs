using System.Collections.Generic;
using System.Diagnostics;
public class Profiler {
    private static Dictionary<string, Stopwatch> watches = new Dictionary<string, Stopwatch>();
	private static bool enabled = true;
    
    public static void Begin(string section) {
        if (!watches.ContainsKey(section) && enabled) {
            watches[section] = new Stopwatch();
        }
        watches[section].Restart();
    }
    
    public static void End(string section) {
        if (watches.ContainsKey(section) && enabled) {
            watches[section].Stop();
            DeveloperConsole.Log($"{section}: {watches[section].ElapsedMilliseconds}ms");
        }
    }
}
