namespace GameCube.GX.Texture;

public enum GciPalette : byte
{
    Invalid,
    Shared,
    Unique,
}

public static class GciPaletteExtensions
{
    extension(GciPalette gciPalette)
    {
        public byte Byte => (byte)gciPalette;

        public void Validate()
        {
            if (!System.Enum.IsDefined(gciPalette) && gciPalette != GciPalette.Invalid)
            {
                string msg = $"{nameof(GciPalette)} is invalid ({gciPalette}).";
                throw new System.Exception(msg);
            }
        }
    }
}