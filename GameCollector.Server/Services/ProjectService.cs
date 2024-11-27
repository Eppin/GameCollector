namespace GameCollector.Server.Services;

using System.Reflection;
using System.Text;

public class ProjectService
{
    public List<Project> Projects { get; } = [];

    public async Task Load()
    {
        foreach (var (saveType, lines) in await GetResources())
        {
            var groups = new List<Group>();

            foreach (var line in lines)
            {
                var group = line[0];
                var dex = line
                    .Skip(1)
                    .Select(s =>
                    {
                        var split = s.Split(',');

                        var nationalAndForm = split[0].Split('-');
                        var national = ushort.Parse(nationalAndForm[0]);
                        var form = byte.Parse(nationalAndForm[1]);

                        var local = ushort.Parse(split[1]);

                        return new Dex(national, local, form);
                    });

                groups.Add(new(group, dex));
            }

            Projects.Add(new Project(saveType, groups));
        }
    }

    private static async Task<IDictionary<SaveType, List<string[]>>> GetResources()
    {
        var results = new Dictionary<SaveType, List<string[]>>();

        var executingAssembly = Assembly.GetExecutingAssembly();

        var files = executingAssembly
            .GetManifestResourceNames()
            .Where(r => r.EndsWith(".csv"));

        foreach (var file in files)
        {
            await using var stream = executingAssembly.GetManifestResourceStream(file)!;

            using var streamReader = new StreamReader(stream, Encoding.UTF8);

            var lines = new List<string[]>();
            while (!streamReader.EndOfStream)
            {
                var lineStr = await streamReader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(lineStr))
                    continue;

                lines.Add(lineStr.Split(';'));
            }

            // Extract save type from resource name
            var saveTypeStr = file
                .Replace($"{executingAssembly.GetName().Name}.Resources.", string.Empty)
                .Replace(".csv", string.Empty);

            var saveType = Enum.Parse<SaveType>(saveTypeStr, true);
            results.Add(saveType, lines);
        }

        return results;
    }
}
