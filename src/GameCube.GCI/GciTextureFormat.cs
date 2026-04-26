namespace GameCube.GX.Texture;

public enum GciTextureFormat : byte
{
    Invalid,
    DirectColor,
    IndirectColor,
}

public static class GciTextureFormatExtensions
{
    extension(GciTextureFormat gciTextureFormat)
    {
        public byte Byte => (byte)gciTextureFormat;

        public void Validate()
        {
            if (!System.Enum.IsDefined(gciTextureFormat) && gciTextureFormat != GciTextureFormat.Invalid)
            {
                string msg = $"{nameof(GciTextureFormat)} is invalid ({gciTextureFormat}).";
                throw new System.Exception(msg);
            }
        }
    }
}