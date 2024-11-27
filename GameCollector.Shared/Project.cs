namespace GameCollector.Shared;

public class Project(SaveType saveType, IEnumerable<Group> groups)
{
    public SaveType SaveType { get; set; } = saveType;
    public IEnumerable<Group> Groups { get; set; } = groups;
}
