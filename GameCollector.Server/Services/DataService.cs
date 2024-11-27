namespace GameCollector.Server.Services;

using Microsoft.Extensions.Caching.Memory;
using Models;
using Utils;

public class DataService(ILogger<DataService> logger, ProjectService projectService, IMemoryCache cache)
{
    public async Task Load()
    {
        foreach (var saveType in Enum.GetValues<SaveType>())
        {
            try
            {
                await SetCache(saveType);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while loading SAV file");
            }
        }
    }

    public async Task SetCache(SaveType saveType)
    {
        var game = await GetGame(saveType);
        var data = await GetProject(saveType);

        if (game == null || data == null)
        {
            logger.LogWarning("Unable to load data for [{SaveType}]", saveType);
            return;
        }

        cache.Set($"{saveType}_game", game);
        cache.Set($"{saveType}_data", data);

        logger.LogInformation("Successfully set cache for [{SaveType}]", saveType);
    }

    private async Task<Game?> GetGame(SaveType saveType)
    {
        var saveData = await GetSAV(saveType);
        if (saveData == null)
            return null;

        var sav = saveData.SaveFile;
        return new Game(sav.OT, sav.DisplayTID, sav.PlayTimeString);
    }

    private async Task<List<Data>?> GetProject(SaveType saveType)
    {
        try
        {
            var sav = await GetSAV(saveType);
            if (sav == null)
                return null;

            var project = projectService.Projects.FirstOrDefault(p => p.SaveType == (SaveType)saveType);

            var data = new List<Data>();
            foreach (var group in project?.Groups ?? [])
            {
                var box = group.Dex.Count() / 30;
                box += 1; // Get box count
                box *= 30; // Get box size

                var values = group.Name
                    .Split(' ')
                    .ToList();

                var found = sav.Boxes.FirstOrDefault(b => values.Any(v => b.Value.Contains(v)));

                if (string.IsNullOrWhiteSpace(found.Value))
                    continue;

                foreach (var pkm in sav.SaveFile.BoxData.Skip(found.Key * 30).Take(box))
                {
                    if (!pkm.Valid || pkm.Species == 0)
                        continue;

                    data.Add(new Data(pkm.Species, pkm.Form, ImageFile.Get(pkm), ImageFile.Get((Ball)pkm.Ball), GetLocation(pkm)));
                }
            }

            return data;

            string GetLocation(PKM? pkm)
            {
                return pkm == null
                    ? "Unknown"
                    : pkm.GetLocationString(false);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to retrieve content for project {ProjectId}", saveType);
            return null;
        }
    }

    private async Task<SaveData?> GetSAV(SaveType saveType)
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Saves");
        if (!Directory.Exists(directoryPath))
        {
            logger.LogWarning("Creating missing data directory [{Directory}]", directoryPath);
            Directory.CreateDirectory(directoryPath);
        }

        // Search for a save file
        var filePath = Path.Combine(directoryPath, $"sav_{(int)saveType}");
        if (!File.Exists(filePath))
        {
            logger.LogWarning("No matching save file [{File}] found", filePath);
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(filePath);
        return ReadSAV(saveType, bytes);
    }

    private static SaveData? ReadSAV(SaveType saveType, byte[] bytes)
    {
        var boxes = new Dictionary<int, string>();

        // Helper method to process each save type
        Func<byte[], Dictionary<int, string>, SaveFile>? createSAV = saveType switch
        {
            SaveType.BlackWhite => (data, _) => new SAV5BW(data),
            SaveType.Black2White2 => (data, _) => new SAV5B2W2(data),
            SaveType.XY => (data, _) => new SAV6XY(data),
            SaveType.OmegaRubyAlphaSapphire => (data, _) => new SAV6AO(data),
            SaveType.SunMoon => (data, _) => new SAV7SM(data),
            SaveType.UltraSunUltraMoon => (data, _) => new SAV7USUM(data),
            SaveType.SwordShield => (data, _) => new SAV8SWSH(data),
            SaveType.LegendsArceus => (data, _) => new SAV8LA(data),
            SaveType.BrilliantDiamondShiningPearl => (data, _) => new SAV8BS(data),
            SaveType.ScarletViolet => (data, _) => new SAV9SV(data),
            _ => null
        };

        if (createSAV == null) return null;

        // Use reflection to get the `GetBoxName` method
        var sav = createSAV(bytes, boxes);
        var getBoxNameMethod = sav.GetType().GetMethod("GetBoxName", [typeof(int)]);

        for (var i = 0; i < sav.BoxCount; i++)
        {
            var boxName = (string)getBoxNameMethod?.Invoke(sav, [i])!;
            boxes.TryAdd(i, boxName);
        }

        return new SaveData(sav, boxes);
    }
}
