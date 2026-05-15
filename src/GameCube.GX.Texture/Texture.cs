using Manifold.IO;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Dithering;

namespace GameCube.GX.Texture;

/// <summary>
///     A GameCube GX texture.
/// </summary>
/// <remarks>
///     Invaluable resource: <see href="https://wiki.tockdom.com/wiki/Image_Formats"></see>
/// </remarks>
public class Texture
{
    public readonly record struct BlockOrigin(int X, int Y);

    /// <summary>
    ///     The texture's format.
    /// </summary>
    public TextureFormat Format { get; set; } = TextureFormat.RGBA8;

    /// <summary>
    ///     The texture's pixel width.
    /// </summary>
    public int Width { get; private set; } = 0;

    /// <summary>
    ///     The texture's pixel height.
    /// </summary>
    public int Height { get; private set; } = 0;

    /// <summary>
    ///     The texture's pixels.
    /// </summary>
    /// <remarks>
    ///     Organized horizontally left-to-right with subsequent rows stacked vertically.
    /// </remarks>
    public TextureColor[] Pixels { get; private set; } = [];

    /// <summary>
    ///     Indexer to get get/set a direct colour (pixel) within this texture.
    /// </summary>
    /// <param name="i">The direct colour (pixel) index in this texture.</param>
    /// <returns>
    ///     Direct colour (pixel) at the specified index within this block.
    /// </returns>
    public TextureColor this[int i]
    {
        get => Pixels[i];
        set => Pixels[i] = value;
    }

    /// <summary>
    ///     Indexer to get get/set a direct colour pixel within this texture.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel in this texture.</param>
    /// <param name="y">The vertical coordinate of the pixel in this texture.</param>
    /// <returns>
    ///     Direct colour (pixel) at the specified coordinate within this block.
    /// </returns>
    public TextureColor this[int x, int y]
    {
        get => Pixels[x + y * Width];
        set => Pixels[x + y * Width] = value;
    }


    /// <summary>
    ///     Create a new empty texture.
    /// </summary>
    public Texture() { }

    /// <summary>
    ///     Create a new texture of <paramref name="width"/> by <paramref name="height"/> size
    ///     of the specified <paramref name="format"/>.
    /// </summary>
    /// <param name="width">The texture's pixel width.</param>
    /// <param name="height">The texture's pixel height.</param>
    /// <param name="format">The texture's format.</param>
    public Texture(int width, int height, TextureFormat format = TextureFormat.RGBA8)
    {
        Format = format;
        Width = width;
        Height = height;
        Pixels = new TextureColor[Width * Height];
    }

    /// <summary>
    ///     Create a new texture of <paramref name="width"/> by <paramref name="height"/> size
    ///     of the specified <paramref name="format"/> whose pixel contents are all <paramref name="color"/>.
    /// </summary>
    /// <param name="width">The texture's pixel width.</param>
    /// <param name="height">The texture's pixel height.</param>
    /// <param name="color">The default colour of all pixels for the texture.</param>
    /// <param name="format">The texture's format.</param>
    public Texture(int width, int height, TextureColor color, TextureFormat format = TextureFormat.RGBA8)
    {
        Format = format;
        Width = width;
        Height = height;
        Pixels = new TextureColor[Width * Height];

        // Set all colors as default
        for (int i = 0; i < Pixels.Length; i++)
            Pixels[i] = color;
    }


    // TODO: put IEncoding.MapFormatToEncoding inside texture...?
    //public IEncoding Encoding => IEncoding.MapFormatToEncoding[Format];

    // TODO
    // DirectTextureFormat = DirectColorFormat
    // IndirectTextureFormat = IndirectIndexFormat
    // PaletteColorFormat = PaletteColorFormat (ok)



    /// <summary>
    ///     Bounded meaning a copy over edge returns default value for copy type.
    /// </summary>
    /// <typeparam name="T">Type to copy.</typeparam>
    /// <param name="src">Source array.</param>
    /// <param name="srcWidth">Source width.</param>
    /// <param name="srcHeight">Source height.</param>
    /// <param name="srcOriginX">X origin of copy within source.</param>
    /// <param name="srcOriginY">Y origin of copy within source.</param>
    /// <param name="width">Width of copy.</param>
    /// <param name="height">Height of copy.</param>
    /// <returns>
    ///     
    /// </returns>
    internal static T[] CopyArea<T>(ReadOnlySpan<T> src, int srcWidth, int srcHeight, int srcOriginX, int srcOriginY, int width, int height)
    {
        // Create array "slice"
        int size = width * height;
        T[] copy = new T[size];

        // Figure out how many pixels to copy over. On smaller textures (eg: 4x2), outside region is black.
        // In other words, this "clamps" the copy and leaves out-of-bounds indices unset (black pixels).
        int nPixelsX = Math.Min(width, srcWidth - srcOriginX);
        int nPixelsY = Math.Min(height, srcHeight - srcOriginY);

        // Copy texture region into block
        for (int y = 0; y < nPixelsY; y++)
        {
            int srcY = (srcOriginY + y) * srcWidth;
            int dstY = y * width;
            for (int x = 0; x < nPixelsX; x++)
            {
                int srcX = srcOriginX + x;
                int dstX = x;
                int srcIndex = srcY + srcX;
                int dstIndex = dstY + dstX;
                //
                copy[dstIndex] = src[srcIndex];
            }
        }

        return copy;
    }

    public static TextureColor[] CopyArea(Texture texture, int srcOriginX, int srcOriginY, int dstWidth, int dstHeight)
    {
        IEncoding encoding = IEncoding.MapFormatToEncoding[texture.Format];
        TextureColor[] copy = CopyArea(texture.Pixels, texture.Width, texture.Height, srcOriginX, srcOriginY, encoding.BlockWidth, encoding.BlockHeight);
        return copy;
    }

    public static BlockOrigin[] GetBlockOrigins(Texture texture, IEncoding encoding)
    {
        // Get info
        BlocksInfo blocksInfo = BlocksInfo.FromTexture(texture);
        // Create origin for each block within texture
        BlockOrigin[] origins = new BlockOrigin[blocksInfo.Count];

        // Iterate over each block
        for (int y = 0; y < blocksInfo.CountY; y++)
        {
            int originY = y * encoding.BlockHeight;
            for (int x = 0; x < blocksInfo.CountX; x++)
            {
                int originX = x * encoding.BlockWidth;
                int originIndex = y * blocksInfo.CountX + x;
                origins[originIndex] = new(originX, originY);
            }
        }

        return origins;
    }

    public static Texture ReadDirectColorTexture(EndianBinaryReader reader, DirectTextureFormat directFormat, int pxWidth, int pxHeight)
    {
        directFormat.Validate();
        DirectEncoding directEncoding = DirectEncoding.MapFormatToEncoding[directFormat];
        BlocksInfo blocks = BlocksInfo.FromPixelDimensions(pxWidth, pxHeight, directEncoding);
        DirectBlock[] directBlocks = directEncoding.ReadBlocks(reader, blocks.Count);
        Texture texture = FromDirectBlocks(directBlocks, blocks.CountX, blocks.CountY);
        return texture;
    }

    public static Texture ReadIndirectColorTexture(EndianBinaryReader reader, IndirectTextureFormat indexFormat, Palette palette, int pxWidth, int pxHeight)
    {
        indexFormat.Validate();
        IndirectEncoding indirectEncoding = IndirectEncoding.MapFormatToEncoding[indexFormat];
        BlocksInfo blocks = BlocksInfo.FromPixelDimensions(pxWidth, pxHeight, indirectEncoding);
        IndirectBlock[] indirectBlocks = indirectEncoding.ReadBlocks(reader, blocks.Count);
        Texture texture = FromIndirectBlocksAndPalette(indirectBlocks, palette, blocks.CountX, blocks.CountY);
        return texture;
    }

    public static void WriteDirectColorTexture(EndianBinaryWriter writer, Texture texture, DirectTextureFormat directFormat)
    {
        DirectEncoding directEncoding = DirectEncoding.MapFormatToEncoding[directFormat];
        DirectBlock[] directBlocks = CreateDirectColorBlocksFromTexture(texture, directEncoding);
        foreach (DirectBlock directBlock in directBlocks)
            directEncoding.WriteBlock(writer, directBlock);
    }

    public static void WriteIndirectColorTexture(EndianBinaryWriter writer, Texture texture, IndirectTextureFormat indirectFormat, PaletteColorFormat paletteFormat)
    {
        IndirectEncoding indirectEncoding = IndirectEncoding.MapFormatToEncoding[indirectFormat];
        (IndirectBlock[] indirectBlocks, Palette palette) =
            CreateIndirectColorBlocksAndPaletteFromTexture(texture, indirectEncoding, paletteFormat);
        foreach (IndirectBlock indirectBlock in indirectBlocks)
            indirectEncoding.WriteBlock(writer, indirectBlock);
    }

    public static DirectBlock[] CreateDirectColorBlocksFromTexture(Texture texture, DirectEncoding directEncoding, out BlocksInfo blocksInfo)
    {
        // 
        blocksInfo = BlocksInfo.FromPixelDimensions(texture.Width, texture.Height, directEncoding);
        BlockOrigin[] blockOrigins = GetBlockOrigins(texture, directEncoding);
        DirectBlock[] blocks = new DirectBlock[blocksInfo.Count];
        //
        Assert.IsTrue(blocks.Length == blockOrigins.Length);

        // 
        for (int i = 0; i < blockOrigins.Length; i++)
        {
            BlockOrigin blockOrigin = blockOrigins[i];
            TextureColor[] pixels = CopyArea(texture, blockOrigin.X, blockOrigin.Y, directEncoding.BlockHeight, directEncoding.BlockHeight);
            blocks[i] = new(directEncoding, pixels);
        }

        return blocks;
    }
    public static DirectBlock[] CreateDirectColorBlocksFromTexture(Texture texture, DirectEncoding directEncoding)
        => CreateDirectColorBlocksFromTexture(texture, directEncoding, out _);

    public static (IndirectBlock[] blocks, Palette palette) CreateIndirectColorBlocksAndPaletteFromTexture(Texture texture, IndirectEncoding indirectEncoding, PaletteColorFormat paletteFormat, out BlocksInfo blocksInfo)
    {
        // limitations of image sharp
        if (indirectEncoding.MaxPaletteSize > 256)
        {
            throw new NotImplementedException();
        }


        // 
        blocksInfo = BlocksInfo.FromPixelDimensions(texture.Width, texture.Height, indirectEncoding);
        BlockOrigin[] blockOrigins = GetBlockOrigins(texture, indirectEncoding);
        IndirectBlock[] blocks = new IndirectBlock[blocksInfo.Count];
        //
        Assert.IsTrue(blocks.Length == blockOrigins.Length);


        // init some settings
        var configuration = new Configuration() { };
        var quantizerOptions = new QuantizerOptions()
        {
            //Dither = ,
            //DitherScale = ,
            MaxColors = indirectEncoding.MaxPaletteSize,
        };
        var quantizer = new WuQuantizer(quantizerOptions);
        var rgba32Quantizer = quantizer.CreatePixelSpecificQuantizer<Rgba32>(configuration);

        // build palette and indices
        var image = ToImage(texture);
        var frame = image.Frames.RootFrame;
        var indexedImageFrame = rgba32Quantizer.BuildPaletteAndQuantizeFrame(frame, frame.Bounds());

        // PALETTE
        ReadOnlySpan<Rgba32> paletteColorsRGBA32 = indexedImageFrame.Palette.Span;
        TextureColor[] paletteColors = new TextureColor[paletteColorsRGBA32.Length];
        for (int i = 0; i < paletteColorsRGBA32.Length; i++)
            paletteColors[i] = new TextureColor(paletteColorsRGBA32[i].PackedValue);
        Palette palette = new(paletteFormat, indirectEncoding, paletteColors);

        // INDEXES
        ushort[] indexes = new ushort[image.Width * image.Height];
        for (int y = 0; y < image.Height; y++)
        {
            int originY = y * image.Width;
            var row = indexedImageFrame.GetWritablePixelRowSpanUnsafe(y);
            for (int x = 0; x < image.Width; x++)
            {
                int index = x + originY;
                indexes[index] = row[x];
            }
        }

        // Swizzle indexes
        for (int i = 0; i < blockOrigins.Length; i++)
        {
            BlockOrigin blockOrigin = blockOrigins[i];
            ushort[] blockIndexes = CopyArea(indexes, texture.Width, texture.Height, blockOrigin.X, blockOrigin.Y, indirectEncoding.BlockHeight, indirectEncoding.BlockHeight);
            blocks[i] = new(indirectEncoding, blockIndexes);
        }

        return (blocks, palette);
    }
    public static (IndirectBlock[] blocks, Palette palette) CreateIndirectColorBlocksAndPaletteFromTexture(Texture texture, IndirectEncoding indirectEncoding, PaletteColorFormat paletteFormat)
        => CreateIndirectColorBlocksAndPaletteFromTexture(texture, indirectEncoding, paletteFormat, out _);

    private static Image<Rgba32> ToImage(Texture sourceTexture)
    {
        Image<Rgba32> image = new(sourceTexture.Width, sourceTexture.Height);

        for (int y = 0; y < sourceTexture.Height; y++)
        {
            for (int x = 0; x < sourceTexture.Width; x++)
            {
                TextureColor pixel = sourceTexture[x, y];
                image[x, y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
            }
        }

        return image;
    }

    /// <summary>
    ///     Create a new texture of <paramref name="width"/> by <paramref name="height"/> size.
    ///     The texture's pixels are <paramref name="colors"/>.
    /// </summary>
    /// <param name="colors">The source pixels to construct the texture with.</param>
    /// <param name="width">The texture's pixel width.</param>
    /// <param name="height">The texture's pixel height.</param>
    /// <returns>
    ///     A new texture created from the source <paramref name="colors"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the size of the texture and number of <paramref name="colors"/> are not equal.
    /// </exception>
    public static Texture FromColors(TextureColor[] colors, int width, int height)
    {
        int numPixels = width * height;
        if (numPixels != colors.Length)
        {
            string msg = "Number of raw colors does not match length of width*height.";
            throw new ArgumentException(msg);
        }

        var texture = new Texture
        {
            Format = TextureFormat.RGBA8,
            Width = width,
            Height = height,
            Pixels = new TextureColor[numPixels]
        };

        // Copy pixels
        Array.Copy(colors, texture.Pixels, numPixels);

        return texture;
    }

    /// <summary>
    ///     Create a new texture of <paramref name="width"/> by <paramref name="height"/> size.
    ///     The texture's pixels are <paramref name="rawColors"/> in RGBA format.
    /// </summary>
    /// <param name="rawColors">The raw pixels source to construct the texture with.</param>
    /// <param name="width">The texture's pixel width.</param>
    /// <param name="height">The texture's pixel height.</param>
    /// <returns>
    ///     A new texture created from the source <paramref name="rawColors"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the size of the texture and number of <paramref name="rawColors"/> are not equal.
    /// </exception>
    public static Texture FromColorsRaw(uint[] rawColors, int width, int height)
    {
        int numPixels = width * height;
        if (numPixels != rawColors.Length)
            throw new ArgumentException("Number of raw colors does not match length of width*height.");

        var texture = new Texture
        {
            Format = TextureFormat.RGBA8,
            Width = width,
            Height = height,
            Pixels = new TextureColor[numPixels]
        };

        // Copy in pixels
        for (int i = 0; i < rawColors.Length; i++)
            texture.Pixels[i] = new TextureColor(rawColors[i]);

        return texture;
    }

    /// <summary>
    ///     Create a texture from an array of <paramref name="directBlocks"/> where <paramref name="blocksCountHorizontal"/>
    ///     defines the number of blocks across the texture width and <paramref name="blocksCountVertical"/> defines the number
    ///     of blocks across the texture height.
    /// </summary>
    /// <param name="directBlocks">The source texture blocks to construct the texture with.</param>
    /// <param name="blocksCountHorizontal">The number of blocks along the horizontal axis.</param>
    /// <param name="blocksCountVertical">The number of blocks along the vertical axis.</param>
    /// <returns>
    ///     A new texture created from the source <paramref name="directBlocks"/>.
    /// </returns>
    public static Texture FromDirectBlocks(DirectBlock[] directBlocks, int blocksCountHorizontal, int blocksCountVertical)
    {
        int numBlocks = blocksCountHorizontal * blocksCountVertical;
        if (numBlocks != directBlocks.Length)
        {
            string msg =
                $"Number of {nameof(DirectBlock)} does not match length " +
                $"{nameof(blocksCountHorizontal)}*{nameof(blocksCountVertical)}.";
            throw new ArgumentException(msg);
        }

        // Deswizzle
        throw new NotImplementedException();
    }

    public static Texture FromIndirectBlocksAndPalette(IndirectBlock[] directBlocks, Palette palette, int blocksCountHorizontal, int blocksCountVertical)
    {
        int numBlocks = blocksCountHorizontal * blocksCountVertical;
        if (numBlocks != directBlocks.Length)
        {
            string msg =
                $"Number of {nameof(DirectBlock)} does not match length " +
                $"{nameof(blocksCountHorizontal)}*{nameof(blocksCountVertical)}.";
            throw new ArgumentException(msg);
        }

        // Deswizzle
        throw new NotImplementedException();
    }


    /// <summary>
    ///     Create a new texture cropped from the a region of <paramref name="sourceTexture"/>.
    /// </summary>
    /// <param name="sourceTexture">The source texture to crop from.</param>
    /// <param name="pixelWidth">The pixel width of the cropped region.</param>
    /// <param name="pixelHeight">The pixel height of the cropped region.</param>
    /// <param name="originX">The horizontal origin point of the cropping region.</param>
    /// <param name="originY">The vertical origin point of the cropping region.</param>
    /// <returns>
    ///     A new texture instance with pixel contents cropped from the <paramref name="sourceTexture"/>.
    /// </returns>
    /// <remarks>
    ///     The new texture's format is the same as the source texture.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="originX"/> or <paramref name="originY"/> are negative.
    ///     Thrown if desired crop region does not fit within the bounds of <paramref name="sourceTexture"/>.
    /// </exception>
    public static Texture Crop(Texture sourceTexture, int pixelWidth, int pixelHeight, int originX = 0, int originY = 0)
    {
        // Make sure origin indexes are not negative
        bool isNegative = originX < 0 || originY < 0;
        if (isNegative)
        {
            string msg = $"Neither argument {nameof(originX)} or {nameof(originY)} can be negative.";
            throw new ArgumentException(msg);
        }

        // Make sure desired crop region is within bounds of texture.
        bool isTooWide = (originX + pixelWidth) > sourceTexture.Width;
        bool isTooTall = (originY + pixelHeight) > sourceTexture.Height;
        bool isInvalidCropRegion = isTooWide || isTooTall;
        if (isInvalidCropRegion)
        {
            string msg =
                $"Crop region is either too wide ({isTooWide}) " +
                $"or too tall ({isTooTall}) for the {nameof(sourceTexture)}.";
            throw new ArgumentException(msg);
        }

        // Begin crop
        var cropped = new Texture(pixelWidth, pixelHeight, sourceTexture.Format);
        for (int y = 0; y < cropped.Height; y++)
        {
            int sourceY = originY + y;
            for (int x = 0; x < cropped.Width; x++)
            {
                int sourceX = originX + x;
                cropped[x, y] = sourceTexture[sourceX, sourceY];
            }
        }

        return cropped;
    }

    public static void Copy(Texture sourceTexture, Texture destinationTexture, int destinationOriginX = 0, int destinationOriginY = 0)
    {
        bool canFitX = destinationOriginX + sourceTexture.Width <= destinationTexture.Width;
        bool canFitY = destinationOriginY + sourceTexture.Height <= destinationTexture.Height;
        bool cannotFitCopy = !canFitX || !canFitY;
        if (cannotFitCopy)
        {
            string msg =
                $"Cannot copy texture contents. Destination texture ({destinationTexture.Width},{destinationTexture.Height}) " +
                $"not large enough to fit source texture ({sourceTexture.Width},{sourceTexture.Height}) at origin point " +
                $"({destinationOriginX},{destinationOriginY})";
            throw new ArgumentException(msg);
        }

        // Copy over data
        for (int y = 0; y < sourceTexture.Height; y++)
        {
            int dy = destinationOriginY + y;
            for (int x = 0; x < sourceTexture.Width; x++)
            {
                int dx = destinationOriginX + x;
                destinationTexture[dx, dy] = sourceTexture[x, y];
            }
        }
    }



    /// <summary>
    ///     Compute the max number of mipmaps this a texture of size
    ///     <paramref name="width"/> and <paramref name="height"/>
    ///     would have.
    /// </summary>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="minMipmapPixelSize">Minimum dimension in pixels (x or y) where mipmap generation ends.</param>
    /// <returns>
    ///     Max number of valid mipmaps for the specified size.
    /// </returns>
    public static ushort GetMaxMipmapCount(int width, int height, int minMipmapPixelSize = 1)
    {
        ushort mipmapCount = 0;
        while (width > minMipmapPixelSize && height > minMipmapPixelSize)
        {
            width >>= 1;
            height >>= 1;
            mipmapCount++;
        }
        return mipmapCount;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="directTextureFormat"></param>
    /// <returns>
    ///     
    /// </returns>
    public byte[] GetRawBytes(DirectTextureFormat directTextureFormat)
    {
        var memoryStream = new System.IO.MemoryStream();
        using var writer = new EndianBinaryWriter(memoryStream, Endianness.BigEndian);
        WriteDirectColorTexture(writer, this, directTextureFormat);
        writer.Flush();
        byte[] rawData = memoryStream.ToArray();
        writer.Close();
        return rawData;
    }


}
