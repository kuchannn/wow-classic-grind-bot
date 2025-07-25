using Core.Extensions;

using Game;

using Microsoft.Extensions.Logging;

using SharedLib;

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core;

public sealed partial class AddonConfigurator
{
    private readonly ILogger<AddonConfigurator> logger;
    private readonly WowProcess process;

    public AddonConfig Config { get; init; }

    private const string DefaultAddonName = "DataToColor";
    private const string AddonSourcePath = @".\Addons\";

    private string AddonBasePath => Path.Join(process.Path, "Interface", "AddOns");

    private string DefaultAddonPath => Path.Join(AddonBasePath, DefaultAddonName);
    public string FinalAddonPath => Path.Join(AddonBasePath, Config.Title);

    public event Action? OnChange;

    public AddonConfigurator(ILogger<AddonConfigurator> logger, WowProcess process)
    {
        this.logger = logger;
        this.process = process;

        Config = AddonConfig.Load();
    }

    public bool Installed()
    {
        return GetInstallVersion() != null;
    }

    public bool IsDefault()
    {
        return Config.IsDefault();
    }

    public bool Validate()
    {
        if (string.IsNullOrEmpty(Config.Author))
        {
            logger.LogError($"{nameof(Config)}.{nameof(Config.Author)} - error - cannot be empty: '{Config.Author}'");
            return false;
        }

        if (!string.IsNullOrEmpty(Config.Title))
        {
            // this will appear in the lua code so
            // special character not allowed
            // also numbers not allowed
            Config.Title = RegexTitle().Replace(Config.Title, string.Empty);
            Config.Title = new string(Config.Title.Where(char.IsLetter).ToArray());
            Config.Title =
                Config.Title.Trim()
                .Replace(" ", "");

            if (Config.Title.Length == 0)
            {
                logger.LogError($"{nameof(Config)}.{nameof(Config.Title)} - error - use letters only: '{Config.Title}'");
                return false;
            }

            Config.Command = Config.Title.Trim().ToLower();
        }
        else
        {
            logger.LogError($"{nameof(Config)}.{nameof(Config.Title)} - error - cannot be empty: '{Config.Title}'");
            return false;
        }

        if (!int.TryParse(Config.CellSize, out int size))
        {
            logger.LogError($"{nameof(Config)}.{nameof(Config.CellSize)} - error - be a number: '{Config.CellSize}'");
            return false;
        }
        else if (size < 1 || size > 9)
        {
            logger.LogError($"{nameof(Config)}.{nameof(Config.CellSize)} - error - must be, including between 1 and 9: '{Config.CellSize}'");
            return false;
        }

        return true;
    }

    public void Install()
    {
        try
        {
            DeleteAddon();
            CopyAddonFiles();
            RenameAddon();
            MakeUnique();

            logger.LogInformation($"{nameof(Install)} - Success");
        }
        catch (Exception e)
        {
            logger.LogInformation($"{nameof(Install)} - Failed\n{e.Message}");
        }
    }

    private void DeleteAddon()
    {
        if (Directory.Exists(DefaultAddonPath))
        {
            logger.LogInformation($"{nameof(DeleteAddon)} -> Default Addon Exists");
            Directory.Delete(DefaultAddonPath, true);
        }

        if (!string.IsNullOrEmpty(Config.Title) && Directory.Exists(FinalAddonPath))
        {
            logger.LogInformation($"{nameof(DeleteAddon)} -> Unique Addon Exists");
            Directory.Delete(FinalAddonPath, true);
        }
    }

    private void CopyAddonFiles()
    {
        try
        {
            CopyFolder("");
            logger.LogInformation($"{nameof(CopyAddonFiles)} - Success");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);

            // This only should be happen when running from IDE
            CopyFolder(".");
            logger.LogInformation($"{nameof(CopyAddonFiles)} - Success");
        }
    }

    private void CopyFolder(string parentFolder)
    {
        DirectoryCopy(Path.Join(parentFolder + AddonSourcePath), AddonBasePath, true);
    }

    private void RenameAddon()
    {
        string src = Path.Join(AddonBasePath, DefaultAddonName);
        if (src != FinalAddonPath)
            Directory.Move(src, FinalAddonPath);
    }

    private void MakeUnique()
    {
        BulkRename(FinalAddonPath, DefaultAddonName, Config.Title);
        EditToc();
        EditMainLua();
        EditModulesLua();
    }

    private static void BulkRename(string fPath, string match, string fNewName)
    {
        foreach (FileInfo f in new DirectoryInfo(fPath).GetFiles())
        {
            string fromName = Path.GetFileNameWithoutExtension(f.Name);

            if (!fromName.Contains(match))
                continue;

            string ext = Path.GetExtension(f.Name);

            fromName = Path.Join(fPath, f.Name);
            string toName = Path.Join(fPath, fNewName) + ext;

            File.Move(fromName, toName);
        }
    }

    private void EditToc()
    {
        string tocPath = Path.Join(FinalAddonPath, Config.Title + ".toc");
        string text =
            File.ReadAllText(tocPath)
            .Replace(DefaultAddonName, Config.Title)
            .Replace("## Author: FreeHongKongMMO", "## Author: " + Config.Author);

        File.WriteAllText(tocPath, text);
    }

    private void EditMainLua()
    {
        string mainLuaPath = Path.Join(FinalAddonPath, Config.Title + ".lua");
        string text =
            File.ReadAllText(mainLuaPath)
            .Replace(DefaultAddonName, Config.Title)
            .Replace("dc", Config.Command)
            .Replace("DC", Config.Command);

        Regex cellSizeRegex = RegexCellSize();
        text = text.Replace(cellSizeRegex, "SIZE", Config.CellSize);

        File.WriteAllText(mainLuaPath, text);
    }

    private void EditModulesLua()
    {
        FileInfo[] files = new DirectoryInfo(FinalAddonPath).GetFiles();
        foreach (var f in files)
        {
            if (f.Extension.Contains("lua"))
            {
                string path = f.FullName;
                string text = File.ReadAllText(path);
                text = text.Replace(DefaultAddonName, Config.Title);

                File.WriteAllText(path, text);
            }
        }
    }

    public void Delete()
    {
        DeleteAddon();
        AddonConfig.Delete();

        OnChange?.Invoke();
    }

    public void Save()
    {
        Config.Save();

        OnChange?.Invoke();
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }

    public void Update(ClassConfiguration classConfig, ExecGameCommand execGameCommand)
    {
        UpdateDataConfig(nameof(DataConfig.ParallelKey), classConfig.Parallel.Sequence.Length, execGameCommand);
        UpdateDataConfig(nameof(DataConfig.NPCKey), classConfig.NPC.Sequence.Length, execGameCommand);

        UpdateDataConfig(nameof(DataConfig.AUTO_SELL_GREY_ITEMS), classConfig.SellGrey, execGameCommand);
        UpdateDataConfig(nameof(DataConfig.AUTO_REPAIR), true, execGameCommand);

        logger.LogInformation($"Updated DataConfig values");
    }

    private void UpdateDataConfig(string key, object value, ExecGameCommand execGameCommand)
    {
        execGameCommand.Run($"/run {Config.Command}:UpdateDataConfig(\"{key}\", {value.ToString()!.ToLower()})");
    }

    public bool UpdateAvailable()
    {
        if (Config.IsDefault())
            return false;

        Version? repo = GetRepoVerion();
        Version? installed = GetInstallVersion();

        return installed != null && repo != null && repo > installed;
    }

    public Version? GetRepoVerion()
    {
        Version? repo = null;
        try
        {
            repo = GetVersion(Path.Join(AddonSourcePath, DefaultAddonName), DefaultAddonName);

            if (repo == null)
            {
                repo = GetVersion(Path.Join("." + AddonSourcePath, DefaultAddonName), DefaultAddonName);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
        return repo;
    }

    public Version? GetInstallVersion()
    {
        return GetVersion(FinalAddonPath, Config.Title);
    }

    private static Version? GetVersion(string path, string fileName)
    {
        string tocPath = Path.Join(path, fileName + ".toc");

        if (!File.Exists(tocPath))
            return null;

        string begin = "## Version: ";
        string? line = File
            .ReadLines(tocPath)
            .SkipWhile(line => !line.StartsWith(begin))
            .FirstOrDefault();

        string? versionStr = line?.Split(begin)[1];
        return Version.TryParse(versionStr, out Version? version) ? version : null;
    }

    [GeneratedRegex(@"[^\u0000-\u007F]+")]
    private static partial Regex RegexTitle();

    [GeneratedRegex(@"^local CELL_SIZE = (?<SIZE>[0-9]+)", RegexOptions.Multiline)]
    private static partial Regex RegexCellSize();
}