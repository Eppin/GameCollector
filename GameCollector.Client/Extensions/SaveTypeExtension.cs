namespace GameCollector.Client.Extensions;

using GameCollector.Shared;

public static class SaveTypeExtension
{
    public static string ToName(this SaveType saveType)
    {
        return saveType switch
        {
            SaveType.RedBlue => "Red & Blue",
            SaveType.Yellow => "Yellow",
            SaveType.GoldSilver => "Gold & Silver",
            SaveType.Crystal => "Crystal",
            SaveType.RubySapphire => "Ruby & Sapphire",
            SaveType.Emerald => "Emerald",
            SaveType.FireRedLeafGreen => "Fire Red & Leaf Green",
            SaveType.DiamondPearl => "Diamond & Pearl",
            SaveType.Platinum => "Platinum",
            SaveType.HeartGoldSoulSilver => "Heart Gold & Soul Silver",
            SaveType.BlackWhite => "Black & White",
            SaveType.Black2White2 => "Black 2 & White 2",
            SaveType.XY => "X & Y",
            SaveType.OmegaRubyAlphaSapphire => "Omega Ruby & Alpha Sapphire",
            SaveType.SunMoon => "Sun & Moon",
            SaveType.UltraSunUltraMoon => "Ultra Sun & Ultra Moon",
            SaveType.LetsGoPikachuEevee => "Let's Go: Pikachu & Eevee",
            SaveType.SwordShield => "Sword & Shield",
            SaveType.LegendsArceus => "Legends: Arceus",
            SaveType.BrilliantDiamondShiningPearl => "Brilliant Diamond & Shining Pearl",
            SaveType.ScarletViolet => "Scarlet & Violet",
            _ => throw new ArgumentOutOfRangeException(nameof(saveType), saveType, null)
        };
    }
}
