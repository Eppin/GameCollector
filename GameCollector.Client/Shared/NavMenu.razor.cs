namespace GameCollector.Client.Shared;

using System.Net.Http.Json;
using GameCollector.Shared;
using Microsoft.AspNetCore.Components;

public partial class NavMenu : ComponentBase
{
    [Inject] public HttpClient HttpClient { get; set; } = null!;

    private SaveType[] Projects { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        Projects = await HttpClient.GetFromJsonAsync<SaveType[]>("api/Projects") ?? [];
    }
}
