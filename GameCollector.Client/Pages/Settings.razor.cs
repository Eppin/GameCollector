namespace GameCollector.Client.Pages;

using System.Globalization;
using Extensions;
using GameCollector.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

public partial class Settings : ComponentBase
{
    [Inject] public ILogger<Settings> Logger { get; set; } = null!;
    [Inject] public HttpClient HttpClient { get; set; } = null!;
    [Inject] public ISnackbar Snackbar { get; set; } = null!;

    private MudFileUpload<IBrowserFile>? _fileUpload;

    public async Task Upload(IBrowserFile? obj)
    {
        try
        {
            if (obj == null)
                return;

            var response = await HttpClient.PostAsync("api/Settings/Upload", new StreamContent(obj.OpenReadStream(2 * 1024 * 1024)));
            response.EnsureSuccessStatusCode();

            var responseStr = await response.Content.ReadAsStringAsync();

            if (int.TryParse(responseStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var saveType) && Enum.IsDefined(typeof(SaveType), saveType))
                Snackbar.Add($"Processed SAV {((SaveType)saveType).ToName()}!", Severity.Success);
            else
                Snackbar.Add($"Processed SAV {responseStr}!", Severity.Success);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Snackbar.Add("Failed processing SAV!", Severity.Error);
        }
        finally
        {
            _fileUpload?.ClearAsync();
        }
    }
}
