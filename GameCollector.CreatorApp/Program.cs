namespace GameCollector.CreatorApp;

using System.Text;
using Shared;
using static GeneratorService;

internal static class Program
{
    private static readonly string ResourcesPath = DeterminePath();

    private static async Task Main()
    {
        Console.WriteLine("Hello, World!");

        Write(SaveType.BlackWhite, await GetBW());
        Write(SaveType.Black2White2, await GetB2W2());
        Write(SaveType.XY, await GetXY());
        Write(SaveType.OmegaRubyAlphaSapphire, await GetORAS());
        Write(SaveType.SunMoon, await GetSM());
        Write(SaveType.UltraSunUltraMoon, await GetUSUM());
        Write(SaveType.SwordShield, GetSWSH());
        Write(SaveType.LegendsArceus, GetLA());
        Write(SaveType.BrilliantDiamondShiningPearl, GetBDSP());
        Write(SaveType.ScarletViolet, GetSV());

        Console.WriteLine("Done creating files");
    }

    private static string DeterminePath()
    {
        var found = false;
        var maxAttempts = 10;
        var path = Directory.GetCurrentDirectory();
        do
        {
            path = Directory.GetParent(path)!.FullName;
            var resourcesPath = Path.Combine(path, "GameCollector.Server", "Resources");

            maxAttempts--;
            if (maxAttempts <= 0) throw new DirectoryNotFoundException("Directory GameCollector.Server/Resources not found");
            if (!Directory.Exists(resourcesPath)) continue;

            path = resourcesPath;
            found = true;
        } while (!found);

        return path;
    }

    private static void Write(SaveType saveType, IDictionary<string, IEnumerable<Dex>> data)
    {
        var sb = new StringBuilder();

        foreach (var (group, dex) in data)
            sb.AppendLine(FormatString(group, dex));

        var content = sb.ToString();
        var fileName = $"{saveType}.csv";


        File.WriteAllText(Path.Combine(ResourcesPath, fileName), content);
        Console.WriteLine($"Created {fileName} file");
    }

    private static string FormatString(string group, IEnumerable<Dex> dex)
    {
        return $"{group};{string.Join(';', dex.Select(d => $"{d.National}-{d.Form},{d.Local}"))}";
    }
}
