using System;

namespace SharedLib;

public sealed class StartupClientVersion
{
    public ClientVersion Version { get; }

    public string Path { get; }

    public StartupClientVersion(Version version)
    {
        switch (version.Major)
        {
            case 1:
                Version = ClientVersion.SoM;
                Path = "som";
                break;
            case 2:
                Version = ClientVersion.TBC;
                Path = "tbc";
                break;
            case 3:
                Version = ClientVersion.Wrath;
                Path = "wrath";
                break;
            case 4:
                Version = ClientVersion.Cata;
                Path = "cata";
                break;
            default:
                Version = ClientVersion.Retail;
                Path = "retail";
                break;
        }
    }

}
