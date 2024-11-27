namespace GameCollector.Server.Utils;

public static class ImageFile
{
    public static string Get(Ball ball)
    {
        return ball switch
        {
            <= Ball.Cherish => BallStr(ball),
            <= Ball.Sport => BallStr(ball, 475),
            <= Ball.Beast => BallStr(ball, 825),
            Ball.Strange => BallStr(ball, 1758),
            <= Ball.LAFeather => BallStr(ball, 1682),
            <= Ball.LAGigaton => BallStr(ball, 1714),
            Ball.LAOrigin => BallStr(ball, 1734),
            _ => throw new ArgumentOutOfRangeException(nameof(ball), ball, null)
        };
    }

    private static string BallStr(Ball ball, int adjust = 0)
    {
        const string stringFormat = "item_{0}^o.png";

        var ballStr = ((int)ball + adjust).ToString().PadLeft(4, '0');
        return string.Format(stringFormat, ballStr);
    }

    public static string Get(PKM pkm)
    {
        const string baseStr = "poke_capture_{0}_{1}_{2}_{3}_{4}_f_{5}.png";

        var species = pkm.Species.ToString().PadLeft(4, '0');
        var form = pkm.Form.ToString().PadLeft(3, '0');

        var genderStr = "mf";

        if (GenderDependents.Species.Any(g => (ushort)g.Species == pkm.Species && g.Form == pkm.Form))
        {
            genderStr = pkm.Gender == 0 ? "md" : "fd";
        }
        else
        {
            genderStr = (GenderRatio)pkm.PersonalInfo.Gender switch
            {
                GenderRatio.Male => "mo",
                GenderRatio.Female => "fo",
                GenderRatio.Genderless => "uk",
                _ => genderStr
            };
        }

        var gigantamax = pkm is PK8 { CanGigantamax: true } ? "g" : "n";

        var extraValue = (Species)pkm.Species is Species.Alcremie ? pkm.Data[0xE4] : 0;
        var extra = extraValue.ToString().PadLeft(8, '0');

        var shiny = pkm.IsShiny ? "r" : "n";

        return string.Format(baseStr, species, form, genderStr, gigantamax, extra, shiny);
    }

    private enum GenderRatio
    {
        Male = 0,
        Male88 = 31,
        Male75 = 63,
        Even = 127,
        Female75 = 191,
        Female88 = 225,
        Female = 254,
        Genderless = 255,
    }
}