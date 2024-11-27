namespace GameCollector.Server.Controllers;

using System.Net.Mime;
using Services;

[ApiController]
[Route("api/[controller]")]
public class SettingsController(ILogger<SettingsController> logger, DataService dataService) : ControllerBase
{
    [HttpPost("Upload"), Produces(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> Upload()
    {
        try
        {
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Saves");
            if (!Directory.Exists(directoryPath))
            {
                logger.LogWarning("Creating missing data directory [{Directory}]", directoryPath);
                Directory.CreateDirectory(directoryPath);
            }

            // Copy bytes, we want to SAV untouched bytes :-)
            var untouched = new byte[bytes.Length];
            bytes.CopyTo(untouched, 0);

            if (!bytes.SequenceEqual(untouched))
            {
                logger.LogError("Somehow the bytes of the sequences weren't equal after copying");
                return new BadRequestResult();
            }

            // Search for a save file
            var supportedFile = FileUtil.GetSupportedFile(bytes, null);
            if (supportedFile is not SaveFile saveFile)
            {
                logger.LogError("Uploaded file isn't a valid SAV file");
                return new BadRequestResult();
            }

            var saveType = saveFile.Version switch
            {
                GameVersion.B => SaveType.BlackWhite,
                GameVersion.W => SaveType.BlackWhite,
                GameVersion.B2 => SaveType.Black2White2,
                GameVersion.W2 => SaveType.Black2White2,
                GameVersion.X => SaveType.XY,
                GameVersion.Y => SaveType.XY,
                GameVersion.OR => SaveType.OmegaRubyAlphaSapphire,
                GameVersion.AS => SaveType.OmegaRubyAlphaSapphire,
                GameVersion.SN => SaveType.SunMoon,
                GameVersion.MN => SaveType.SunMoon,
                GameVersion.US => SaveType.UltraSunUltraMoon,
                GameVersion.UM => SaveType.UltraSunUltraMoon,
                GameVersion.SW => SaveType.SwordShield,
                GameVersion.SH => SaveType.SwordShield,
                GameVersion.PLA => SaveType.LegendsArceus,
                GameVersion.BD => SaveType.BrilliantDiamondShiningPearl,
                GameVersion.SP => SaveType.BrilliantDiamondShiningPearl,
                GameVersion.SL => SaveType.ScarletViolet,
                GameVersion.VL => SaveType.ScarletViolet,
                _ => throw new ArgumentOutOfRangeException()
            };

            var file = $"sav_{(int)saveType}";
            var filePath = Path.Combine(directoryPath, file);
            if (System.IO.File.Exists(filePath))
            {
                var oldFile = $"{file}_{DateTime.Now:yy-MM-dd_HH-mm-ss}";
                logger.LogInformation("Backup current SAV [{SaveType}] to [{Backup}]", saveType, oldFile);
                System.IO.File.Move(filePath, Path.Combine(directoryPath, oldFile));
            }

            logger.LogInformation("Creating new SAV for [{SaveType}]", saveType);
            await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

            // Refresh cache
            await dataService.SetCache(saveType);

            return new OkObjectResult(saveType);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed processing SAV");
            return new BadRequestResult();
        }
    }
}
