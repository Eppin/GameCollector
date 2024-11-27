namespace GameCollector.Shared;

public record struct Group(string Name, IEnumerable<Dex> Dex);
