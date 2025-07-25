using Newtonsoft.Json;

using System.IO;

using static Newtonsoft.Json.JsonConvert;
using static System.IO.File;
using static System.IO.Path;

public static class DataConfigMeta
{
    public const int Version = 14;
    public const string DefaultFileName = "data_config.json";
}

public sealed class DataConfig
{
    public int Version = DataConfigMeta.Version;
    public string Root { get; set; } = Join("..", "json");

    // Constants for addon configuration
    public const string AUTO_SELL_GREY_ITEMS = "AUTO_SELL_GREY_ITEMS";
    public const string AUTO_REPAIR = "AUTO_REPAIR";
    public const string LOOT_AURA_FRAME = "LOOT_AURA_FRAME";
    public const string ParallelKey = "ParallelKey";
    public const string NPCKey = "NPCKey";

    [JsonIgnore]
    public string Class => Join(Root, "class");
    [JsonIgnore]
    public string Path => Join(Root, "path");
    [JsonIgnore]
    public string ExpDbc => Join(Root, "dbc", Exp);
    [JsonIgnore]
    public string PathInfo => Join(Root, "PathInfo");
    [JsonIgnore]
    public string MPQ => Join(Root, "MPQ");
    [JsonIgnore]
    public string ExpArea => Join(Root, "area", Exp);
    [JsonIgnore]
    public string PPather => Join(Root, "PPather");
    [JsonIgnore]
    public string Screenshot => Join(Root, "cap");
    [JsonIgnore]
    public string ExpHistory => Join(Root, "History", Exp);
    [JsonIgnore]
    public string ExpExperience => Join(Root, "experience", Exp);
    [JsonIgnore]
    public string NpcLocations => Join(Root, "npclocations", Exp);

    // at runtime - determined from the running exe file version
    [JsonIgnore]
    public string Exp { get; set; } = "wrath"; // hardcoded default

    public static DataConfig Load()
    {
        if (File.Exists(DataConfigMeta.DefaultFileName))
        {
            var loaded = DeserializeObject<DataConfig>(ReadAllText(DataConfigMeta.DefaultFileName));
            if (loaded.Version == DataConfigMeta.Version)
                return loaded;
        }

        return new DataConfig().Save();
    }

    public static DataConfig Load(string client)
    {
        if (File.Exists(DataConfigMeta.DefaultFileName))
        {
            var loaded = DeserializeObject<DataConfig>(ReadAllText(DataConfigMeta.DefaultFileName));
            if (loaded.Version == DataConfigMeta.Version)
            {
                loaded.Exp = client;
                return loaded;
            }
        }

        DataConfig newConfig = new DataConfig().Save();
        newConfig.Exp = client;
        return newConfig;
    }

    private DataConfig Save()
    {
        WriteAllText(DataConfigMeta.DefaultFileName, SerializeObject(this));

        return this;
    }

    public void DeletePPatherCache()
    {
        if (!Directory.Exists(PathInfo))
        {
            return;
        }

        var directories = Directory.GetDirectories(PathInfo);
        foreach (string directory in directories)
        {
            Directory.Delete(directory, true);
        }
    }
}