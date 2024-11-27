namespace GameCollector.CreatorApp;

using System.Globalization;
using System.Reflection;
using System.Text;
using PKHeX.Core;
using Shared;

public static class GeneratorService
{
    public static Task<IDictionary<string, IEnumerable<Dex>>> GetBW() => GetBW(false);
    public static Task<IDictionary<string, IEnumerable<Dex>>> GetB2W2() => GetBW(true);

    private static async Task<IDictionary<string, IEnumerable<Dex>>> GetBW(bool isB2W2)
    {
        var resource = isB2W2 ? "B2W2" : "BW";
        IPersonalTable personalTable = isB2W2 ? PersonalTable.B2W2 : PersonalTable.BW;

        var lines = await GetResource(resource);

        var dex = new List<Dex>();

        foreach (var line in lines)
        {
            var nat = line[0];

            var local = Parse(line[1]);

            var (species, form) = ExtractSpecies(nat);

            if (!personalTable.IsPresentInGame(species, form) || dex.Any(d => d.National == species)) continue;

            dex.Add(new(species, local, form));
        }

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Unova", dex.OrderBy(d => d.Local).ThenBy(d => d.Form) }
        };
    }

    public static async Task<IDictionary<string, IEnumerable<Dex>>> GetXY()
    {
        var lines = await GetResource("XY");

        var centralDex = new List<Dex>();
        var coastalDex = new List<Dex>();
        var mountainDex = new List<Dex>();

        foreach (var line in lines)
        {
            var nat = line[0];

            var central = Parse(line[1]);
            var coastal = Parse(line[2]);
            var mountain = Parse(line[3]);

            var (species, form) = ExtractSpecies(nat);

            if (!PersonalTable.XY.IsPresentInGame(species, form)) continue;

            if (central > 0 && centralDex.All(d => d.National != species)) centralDex.Add(new(species, central, form));
            if (coastal > 0 && coastalDex.All(d => d.National != species)) coastalDex.Add(new(species, coastal, form));
            if (mountain > 0 && mountainDex.All(d => d.National != species)) mountainDex.Add(new(species, mountain, form));
        }

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Central", centralDex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Coastal", coastalDex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Mountain", mountainDex.OrderBy(d => d.Local).ThenBy(d => d.Form) }
        };
    }

    public static async Task<IDictionary<string, IEnumerable<Dex>>> GetORAS()
    {
        var lines = await GetResource("ORAS");

        var dex = new List<Dex>();

        foreach (var line in lines)
        {
            var nat = line[0];

            var local = Parse(line[1]);

            var (species, form) = ExtractSpecies(nat);

            if (!PersonalTable.AO.IsPresentInGame(species, form) || dex.Any(d => d.National == species)) continue;

            dex.Add(new(species, local, form));
        }

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Hoenn", dex.OrderBy(d => d.Local).ThenBy(d => d.Form) }
        };
    }

    public static Task<IDictionary<string, IEnumerable<Dex>>> GetSM() => GetSM(false);
    public static Task<IDictionary<string, IEnumerable<Dex>>> GetUSUM() => GetSM(true);

    private static async Task<IDictionary<string, IEnumerable<Dex>>> GetSM(bool isUSUM)
    {
        var resource = isUSUM ? "USUM" : "SM";
        var personalTable = isUSUM ? PersonalTable.USUM : PersonalTable.SM;

        var lines = await GetResource(resource);

        var alolaDex = new List<Dex>();

        var melemeleDex = new List<Dex>();
        var akalaDex = new List<Dex>();
        var ulaulaDex = new List<Dex>();
        var poniDex = new List<Dex>();

        foreach (var line in lines)
        {
            var nat = line[0];

            var alola = Parse(line[1]);

            var melemele = Parse(line[2]);
            var akala = Parse(line[3]);
            var ulaula = Parse(line[4]);
            var poni = Parse(line[5]);

            var (species, form) = ExtractSpecies(nat);

            if (!personalTable.IsPresentInGame(species, form)) continue;

            alolaDex.Add(new(species, alola, form));

            if (melemele > 0) melemeleDex.Add(new(species, melemele, form));
            if (akala > 0) akalaDex.Add(new(species, akala, form));
            if (ulaula > 0) ulaulaDex.Add(new(species, ulaula, form));
            if (poni > 0) poniDex.Add(new(species, poni, form));
        }

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Alola", alolaDex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Melemele", melemeleDex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Akala", akalaDex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Ula'Ula", ulaulaDex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Poni", poniDex.OrderBy(d => d.Local).ThenBy(d => d.Form) }
        };
    }

    public static IDictionary<string, IEnumerable<Dex>> GetLA()
    {
        var table = PersonalTable.LA;
        var maxSpecies = table.MaxSpeciesID;

        var dex = new List<Dex>();
        var unownDex = new List<Dex>();

        for (ushort species = 0; species <= maxSpecies; species++)
        {
            var info = table[species];

            byte form = 0;
            if ((Species)species is Species.Sneasel) // Hisui Sneasel
                form = 1;

            for (; form < info.FormCount; form++)
            {
                if (table.IsPresentInGame(species, form) && dex.All(d => d.National != species))
                    dex.Add(new(species, table[species, form].DexIndexHisui, form));

                if (table.IsPresentInGame(species, form) && (Species)species == Species.Unown)
                    unownDex.Add(new(species, table[species, form].DexIndexHisui, form));
            }
        }

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Hisui", dex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Unown", unownDex.OrderBy(d => d.Local).ThenBy(d => d.Form) }
        };
    }

    public static IDictionary<string, IEnumerable<Dex>> GetBDSP()
    {
        var table = PersonalTable.BDSP;
        var maxSpecies = table.MaxSpeciesID;

        var dex = new List<Dex>();
        var unownDex = new List<Dex>();

        for (ushort species = 0; species <= maxSpecies; species++)
        {
            var info = table[species];

            for (byte form = 0; form < info.FormCount; form++)
            {
                if (!table.IsPresentInGame(species, form))
                    continue;

                info = table[species, form];

                // Allow to add Manaphy, as a special case!
                var dexIndex = species == (ushort)Species.Manaphy ? (ushort)151 : info.PokeDexIndex;

                if (dexIndex > 0 && dex.All(d => d.National != species))
                    dex.Add(new(species, dexIndex, form));

                if ((Species)species == Species.Unown)
                    unownDex.Add(new(species, dexIndex, form));
            }
        }

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Sinnoh", dex.OrderBy(d => d.Local).ThenBy(d => d.Form) },
            { "Unown", unownDex.OrderBy(d => d.Local).ThenBy(d => d.Form) }
        };
    }

    public static IDictionary<string, IEnumerable<Dex>> GetSWSH()
    {
        var table = PersonalTable.SWSH;
        var maxSpecies = table.MaxSpeciesID;

        var galarDex = new List<Dex>();
        var armorDex = new List<Dex>();
        var crownDex = new List<Dex>();

        for (ushort species = 0; species <= maxSpecies; species++)
        {
            var info = table[species];

            // Region exclusives, skip form 0...
            byte form = 0;
            if ((Species)species is Species.Stunfisk or Species.Ponyta
                or Species.Rapidash or Species.Slowpoke or Species.Slowbro
                or Species.Farfetchd or Species.Weezing or Species.MrMime
                or Species.Corsola or Species.Zigzagoon or Species.Linoone
                or Species.Darumaka or Species.Darmanitan or Species.Yamask)
            {
                form = 1;
            }

            // Skip Alola Meowth
            if ((Species)species is Species.Meowth) form = 2;

            for (; form < info.FormCount; form++)
            {
                if (!table.IsPresentInGame(species, form))
                    continue;

                info = table[species, form];

                if (galarDex.All(d => d.National != species) && info.PokeDexIndex > 0)
                    galarDex.Add(new(species, info.PokeDexIndex, form));

                if (armorDex.All(d => d.National != species) && info.ArmorDexIndex > 0)
                    armorDex.Add(new(species, info.ArmorDexIndex, form));

                if (crownDex.All(d => d.National != species) && info.CrownDexIndex > 0)
                    crownDex.Add(new(species, info.CrownDexIndex, form));
            }
        }

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Galar", galarDex.OrderBy(d => d.Local) },
            { "Isle of Armor", armorDex.OrderBy(d => d.Local) },
            { "Crown Tundra", crownDex.OrderBy(d => d.Local) }
        };
    }

    public static IDictionary<string, IEnumerable<Dex>> GetSV()
    {
        var table = PersonalTable.SV;
        var maxSpecies = table.MaxSpeciesID;

        var paldeaDex = new List<Dex>();
        var kitakamiDex = new List<Dex>();
        var blueberryDex = new List<Dex>();

        for (ushort species = 0; species <= maxSpecies; species++)
        {
            var info = table[species];

            for (byte form = 0; form < info.FormCount; form++)
            {
                if (!table.IsPresentInGame(species, form))
                    continue;

                info = table[species, form];

                if (paldeaDex.All(d => d.National != species) && info.DexPaldea > 0)
                    paldeaDex.Add(new(species, info.DexPaldea, form));

                if (kitakamiDex.All(d => d.National != species) && info.DexKitakami > 0)
                    kitakamiDex.Add(new(species, info.DexKitakami, form));

                if (blueberryDex.All(d => d.National != species) && info.DexBlueberry > 0)
                    blueberryDex.Add(new(species, info.DexBlueberry, form));
            }
        }

        paldeaDex = paldeaDex.OrderBy(d => d.Local).ToList();
        kitakamiDex = kitakamiDex.OrderBy(d => d.Local).ToList();
        blueberryDex = blueberryDex.OrderBy(d => d.Local).ToList();

        return new Dictionary<string, IEnumerable<Dex>>
        {
            { "Paldea", paldeaDex.OrderBy(d => d.Local) },
            { "Kitakami", kitakamiDex.OrderBy(d => d.Local) },
            { "Blueberry", blueberryDex.OrderBy(d => d.Local) }
        };
    }

    private static (ushort, byte) ExtractSpecies(string speciesStr)
    {
        var split = speciesStr.Split('-');

        byte form = split.Length switch
        {
            > 2 => throw new ArgumentOutOfRangeException(nameof(speciesStr), speciesStr,
                "Species value can only contain a single -"),
            2 => byte.Parse(split[1]),
            _ => 0
        };

        var species = ushort.Parse(split[0]);

        return (species, form);
    }

    private static ushort Parse(string str)
    {
        _ = ushort.TryParse(str, CultureInfo.InvariantCulture, out var result);
        return result;
    }

    private static async Task<List<string[]>> GetResource(string file)
    {
        var info = Assembly.GetExecutingAssembly().GetName();

        await using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"{info.Name}.Resources.{file}.csv")!;

        using var streamReader = new StreamReader(stream, Encoding.UTF8);

        var headerSkipped = false;

        var lines = new List<string[]>();
        while (!streamReader.EndOfStream)
        {
            var lineStr = await streamReader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(lineStr))
                continue;

            if (headerSkipped)
                lines.Add(lineStr.Split(';'));
            else
                headerSkipped = true;
        }

        return lines;
    }
}
