using System;

public static class SaveEventChannel
{
    public static event Action<int> SaveRequested;
    public static event Action<int, SaveData> SaveCompleted;
    public static event Action<int> LoadRequested;
    public static event Action<int, SaveData> LoadCompleted;

    public static void RaiseSaveRequested(int slot)
    {
        SaveRequested?.Invoke(slot);
    }

    public static void RaiseSaveCompleted(int slot, SaveData data)
    {
        SaveCompleted?.Invoke(slot, data);
    }

    public static void RaiseLoadRequested(int slot)
    {
        LoadRequested?.Invoke(slot);
    }

    public static void RaiseLoadCompleted(int slot, SaveData data)
    {
        LoadCompleted?.Invoke(slot, data);
    }
}
