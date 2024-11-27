namespace GameCollector.Server.Models;

public class SaveData(SaveFile saveFile, Dictionary<int, string> boxes)
{
    public SaveFile SaveFile { get; set; } = saveFile;
    public Dictionary<int, string> Boxes { get; set; } = boxes;
}
