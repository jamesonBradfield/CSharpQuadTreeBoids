using System;

public static class MemoryProfiler
{
    private static MemoryProfileModule _instance;

    public static void Initialize(MemoryProfileModule instance)
    {
        _instance = instance;
    }

    public static void CreateProfile(string name)
    {
        if (_instance == null) return;
        _instance.CreateProfile(new[] { name });
    }

    public static void StartProfile(string name)
    {
        if (_instance == null) return;
        
        _instance.CreateProfile(new[] { name });
        _instance.SwitchProfile(new[] { name });
        _instance.StartRecording(Array.Empty<string>());
    }

    public static void StopProfile(string name)
    {
        if (_instance == null) return;
        
        _instance.SwitchProfile(new[] { name });
        _instance.StopRecording(Array.Empty<string>());
    }

    public static void Snapshot(string name)
    {
        if (_instance == null) return;
        
        _instance.SwitchProfile(new[] { name });
        _instance.TakeSnapshot(Array.Empty<string>());
    }

    public static void AddMetric(string profileName, string metricName, double value)
    {
        if (_instance == null) return;
       	if(_instance._currentProfile != profileName) {
        	_instance.SwitchProfile(new[] { profileName });
		}
        _instance.AddCustomMetric(metricName, value);
    }

    public static void CompareProfiles(string profile1, string profile2)
    {
        _instance?.CompareProfiles(new[] { profile1, profile2 });
    }
}
