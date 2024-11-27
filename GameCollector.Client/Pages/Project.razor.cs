namespace GameCollector.Client.Pages;

using System.Net.Http.Json;
using Extensions;
using GameCollector.Shared;
using Microsoft.AspNetCore.Components;

public partial class Project : ComponentBase
{
    [Parameter] public string ProjectId { get; set; } = null!;

    [Inject] public HttpClient HttpClient { get; set; } = null!;

    private Group[] Groups { get; set; } = [];
    private Data[] Data { get; set; } = [];
    private Game Game { get; set; } = new("Loading...", 0, "00ː00ː00");

    private (double Caught, double Dex) Progress { get; set; } = (0, 0);

    protected override void OnParametersSet()
    {
        Game = new("Loading...", 0, "00ː00ː00");
        Groups = [];
        Data = [];
        Progress = (0, 0);

        _ = Task
            .WhenAll([LoadGame(), LoadGroups(), LoadData()])
            .ContinueWith(_ =>
            {
                Progress = GetProgress();
                StateHasChanged();
            });
    }

    private async Task LoadGame()
    {
        try
        {
            Game = await HttpClient.GetFromJsonAsync<Game>($"api/Projects/Single/{ProjectId}") ?? new Game("Unknown", 0, "00ː00ː00");
        }
        catch (Exception)
        {
            Game = new("Not yet started", 0, "00ː00ː00");
            Console.WriteLine("Wasn't able to load game data");
        }
    }

    private async Task LoadGroups()
    {
        try
        {
            Groups = await HttpClient.GetFromJsonAsync<Group[]>($"api/Projects/Items/{ProjectId}") ?? [];
        }
        catch (Exception)
        {
            Console.WriteLine("Wasn't able to load group data");
            Groups = [];
            Data = [];
        }
    }

    private async Task LoadData()
    {
        try
        {
            Data = await HttpClient.GetFromJsonAsync<Data[]>($"api/Projects/Content/{ProjectId}") ?? [];
        }
        catch (Exception)
        {
            Console.WriteLine("Wasn't able to load all data");
            Data = [];
        }
    }

    private IEnumerable<string> GetHeaderUri()
    {
        const string baseUri = "./images";
        const string baseLogoUri = $"{baseUri}/logos";


        if (!int.TryParse(ProjectId, out var saveType) || !Enum.IsDefined(typeof(SaveType), saveType))
            return [$"{baseUri}/none-250.png"];

        return (SaveType)saveType switch
        {
            SaveType.BlackWhite => [$"{baseLogoUri}/logo_black.png", $"{baseLogoUri}/logo_white.png"],
            SaveType.Black2White2 => [$"{baseLogoUri}/logo_black2.png", $"{baseLogoUri}/logo_white2.png"],
            SaveType.XY => [$"{baseLogoUri}/logo_x^q.png", $"{baseLogoUri}/logo_y^q.png"],
            SaveType.OmegaRubyAlphaSapphire => [$"{baseLogoUri}/logo_omegaruby^q.png", $"{baseLogoUri}/logo_alphasapphire^q.png"],
            SaveType.SunMoon => [$"{baseLogoUri}/logo_sun^q.png", $"{baseLogoUri}/logo_moon^q.png"],
            SaveType.UltraSunUltraMoon => [$"{baseLogoUri}/logo_usun^q.png", $"{baseLogoUri}/logo_umoon^q.png"],
            SaveType.LetsGoPikachuEevee => [$"{baseLogoUri}/logo_letsgopikachu^q.png", $"{baseLogoUri}/logo_letsgoeevee^q.png"],
            SaveType.SwordShield => [$"{baseLogoUri}/logo_sword^q.png", $"{baseLogoUri}/logo_shield^q.png"],
            SaveType.LegendsArceus => [$"{baseLogoUri}/logo_larceus^q.png"],
            SaveType.BrilliantDiamondShiningPearl => [$"{baseLogoUri}/logo_bdiamond^q.png", $"{baseLogoUri}/logo_sperl^q.png"],
            SaveType.ScarletViolet => [$"{baseLogoUri}/logo_scarlet^q.png", $"{baseLogoUri}/logo_violet^q.png"],
            _ => [$"{baseLogoUri}/logo_dummy^t.png"]
        };
    }

    private string GetTitle()
    {
        const string baseStr = "Pokémon";

        if (!int.TryParse(ProjectId, out var saveType) || !Enum.IsDefined(typeof(SaveType), saveType))
            return baseStr;

        return $"{baseStr} {((SaveType)saveType).ToName()}";
    }

    private static string GetLocalIndex(Dex dex)
    {
        return $"#{dex.Local.ToString().PadLeft(3, '0')}";
    }

    private string GetImageUri(Dex dex, string type)
    {
        const string baseUri = "./images";

        var data = Data.FirstOrDefault(p => p.National == dex.National && p.Form == dex.Form);

        var noneSize = type == "pokemon" ? "512" : "64";
        var image = type == "pokemon" ? data?.Sprite : data?.Ball;

        var imagePath = data == null
            ? $"{baseUri}/none-{noneSize}.png"
            : $"{baseUri}/{type}/{image}";

        return imagePath;
    }

    private string GetMetLocation(Dex dex)
    {
        return Data
            .FirstOrDefault(p => p.National == dex.National && p.Form == dex.Form)
            ?.MetLocation ?? "<i>Not yet caught<i>";
    }

    private string GetProgressStr()
    {
        var (caught, dex) = Progress;
        return $"{caught}/{dex}";
    }

    private string GetProgressPercent()
    {
        var (caught, dex) = Progress;
        return dex == 0 ? $"{dex:P2}" : $"{caught / dex:P2}";
    }

    private (double Caught, double Dex) GetProgress()
    {
        // Dex entries
        var dex = Groups
            .SelectMany(g => g.Dex)
            .DistinctBy(d => new { d.National, d.Form })
            .ToList();

        // Caught
        var caught = Data
            .DistinctBy(d => new { d.National, d.Form })
            .Count(p => dex.Any(d => d.National == p.National && d.Form == p.Form));

        return (caught, dex.Count);
    }
}
