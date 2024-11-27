namespace GameCollector.Server.Tests.Utils;

using PKHeX.Core;
using Server.Utils;

public class ImageFileTests
{
    [Theory]
    [InlineData(Species.Enamorus, 0, 0, false)]
    [InlineData(Species.Enamorus, 1, 0, "poke_capture_0905_000_fd_n_00000000_f_n.png")]
    [InlineData(Species.Enamorus, 2, 0, false)]
    [InlineData(Species.Sneasel, 0, 0, "poke_capture_0215_000_md_n_00000000_f_n.png")]
    [InlineData(Species.Sneasel, 1, 0, "poke_capture_0215_000_fd_n_00000000_f_n.png")]
    [InlineData(Species.Sneasel, 2, 0, false)]
    [InlineData(Species.Sneasel, 0, 1, "poke_capture_0215_001_md_n_00000000_f_n.png")]
    [InlineData(Species.Sneasel, 1, 1, "poke_capture_0215_001_fd_n_00000000_f_n.png")]
    [InlineData(Species.Sneasel, 2, 1, false)]
    public void Test_LA(Species species, byte gender, byte form, object result)
    {
        // Act
        Test<PA8>(species, gender, form, result);
    }
    
    [Theory]
    [InlineData(Species.Torchic, 0, 0, "poke_capture_0255_000_md_n_00000000_f_n.png")]
    [InlineData(Species.Torchic, 1, 0, "poke_capture_0255_000_fd_n_00000000_f_n.png")]
    [InlineData(Species.Torchic, 2, 0, false)]
    public void Test_SV(Species species, byte gender, byte form, object result)
    {
        // Act
        Test<PK9>(species, gender, form, result);
    }

    private static void Test<T>(Species species, byte gender, byte form, object result) where T : PKM, new()
    {
        // Arrange
        var pk = new T { Species = (ushort)species, Form = form };
        pk.SetIsShiny(false);
        pk.SetSaneGender(gender);
        
        // Act
        var img = ImageFile.Get(pk);

        // Assert
        switch (result)
        {
            case bool b when b != pk.IsGenderValid():
                Assert.Fail("No valid gender");
                break;

            case string str:
                Assert.Equal(result, img);
                break;
        }
    }
}
