using System;

namespace Core;

public interface IAddonReader
{
    double AvgUpdateLatency { get; }

    string TargetName { get; }

    event Action? AddonDataChanged;

    void FullReset();

    void Update();
    void UpdateUI();
    void SessionReset();
}