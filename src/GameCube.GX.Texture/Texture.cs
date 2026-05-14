using Manifold.IO;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace GameCube.GX.Texture;

/// <summary>
///     A GameCube GX texture.
/// </summary>
/// <remarks>
///     Invaluable resource: <see href="https://wiki.tockdom.com/wiki/Image_Formats"></see>
/// </remarks>
public class Texture
{
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
    ///     The texture's colour pallete, if texture uses a colour-indexed texture format.
    /// </summary>
    public Palette? Palette { get; private set; } = null;

    /// <summary>
    ///     The texture's blocks.
    /// </summary>
    public Block[] Blocks { get; private set; } = [];

    /// <summary>
    ///     True if the texture's palette is not null.
    /// </summary>
    public bool IsPaletted => Palette is not null;


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



    public static DirectBlock[] CreateDirectColorBlocksFromTexture(Texture texture, DirectEncoding directEncoding, out int blocksCountHorizontal, out int blocksCountVertical)
    {
        // Create blocks array. Each block will contain pixel data for texture.
        blocksCountHorizontal = directEncoding.GetHorizontalBlocksCountToEncode(texture);
        blocksCountVertical = directEncoding.GetVerticalBlocksCountToEncode(texture);
        int blocksCount = blocksCountHorizontal * blocksCountVertical;
        DirectBlock[] blocks = new DirectBlock[blocksCount];

        // Copy colors from array texture into blocks
        TextureIndexToBlockIndex colorsToBlock = (int textureOriginX, int textureOriginY, int blockIndex) =>
        {
            blocks[blockIndex] = RegionToDirectBlock(texture, directEncoding, textureOriginX, textureOriginY);
        };
        LoopTextureIndexToBlockIndex(blocksCountHorizontal, blocksCountVertical, directEncoding.Width, directEncoding.Height, colorsToBlock);

        return blocks;
    }
    public static DirectBlock[] CreateDirectColorBlocksFromTexture(Texture texture, DirectEncoding directEncoding)
        => CreateDirectColorBlocksFromTexture(texture, directEncoding, out _, out _);

    public static IndirectBlock[] CreateIndirectColorBlocksAndPaletteFromTexture(Texture texture, IndirectEncoding indirectEncoding, TextureFormat paletteFormat, out Palette palette, out int blocksCountHorizontal, out int blocksCountVertical)
    {
        // Create blocks array. Each block will contain indexes for image.
        blocksCountHorizontal = indirectEncoding.GetHorizontalBlocksCountToEncode(texture);
        blocksCountVertical = indirectEncoding.GetVerticalBlocksCountToEncode(texture);
        int blocksCount = blocksCountHorizontal * blocksCountVertical;
        IndirectBlock[] blocks = new IndirectBlock[blocksCount];

        // Create palette
        ToIndexesAndPalette256(texture, indirectEncoding, paletteFormat, out ushort[] indexes, out palette);

        // Copy indexes from array (indexes generated from texture) into blocks
        TextureIndexToBlockIndex indexesToBlock = (int textureOriginX, int textureOriginY, int blockIndex) =>
        {
            blocks[blockIndex] = RegionToIndirectBlock(indexes, texture.Width, texture.Height, indirectEncoding, textureOriginX, textureOriginY);
        };
        LoopTextureIndexToBlockIndex(blocksCountHorizontal, blocksCountVertical, indirectEncoding.BlockWidth, indirectEncoding.BlockHeight, indexesToBlock);

        return blocks;
    }
    public static void CreateIndirectColorBlocksAndPaletteFromTexture(Texture texture, TextureFormat indirectFormat, out IndirectBlock[] blocks, TextureFormat paletteFormat, out Palette palette)
    {
        IndirectEncoding encoding = IndirectEncoding.GetEncoding(indirectFormat);
        blocks = CreateIndirectColorBlocksAndPaletteFromTexture(texture, encoding, paletteFormat, out palette, out _, out _);
    }


    // NEW IDEA: pass in delegate which gives the proper indexes for doing: tex[x,y] => block[x,y]
    private delegate void TextureIndexToBlockIndex(int originX, int originY, int blockIndex);
    private static void LoopTextureIndexToBlockIndex(int blockCountHorizontal, int blockCountVertical, int blockWidth, int blockHeight, TextureIndexToBlockIndex action)
    {
        for (int v = 0; v < blockCountVertical; v++)
        {
            int originY = v * blockHeight;
            for (int h = 0; h < blockCountHorizontal; h++)
            {
                int originX = h * blockWidth;
                int blockIndex = h + v * blockCountHorizontal;
                action(originX, originY, blockIndex);
            }
        }
    }



    /// <summary>
    ///     Cretae a new texture block from a <paramref name="sourceTexture"/>.
    /// </summary>
    /// <param name="sourceTexture">The source texture to copy a region from.</param>
    /// <param name="directEncoding">The encoding to use use for the output block.</param>
    /// <param name="originX">The left edge of the copied region from <paramref name="sourceTexture"/>.</param>
    /// <param name="originY">The top edge of the copied region from <paramref name="sourceTexture"/>.</param>
    /// <returns>
    ///     A new block which contains the region sampled from <paramref name="sourceTexture"/>. The width
    ///     and height of the block depends on the <paramref name="directEncoding"/> used.
    /// </returns>
    private static DirectBlock RegionToDirectBlock(Texture sourceTexture, DirectEncoding directEncoding, int originX, int originY)
    {
        var block = new DirectBlock(directEncoding);

        // Figure out how many pixels to copy over. On smaller textures (eg: 4x2), outside region is black.
        int nPixelsX = Math.Min(directEncoding.Width, sourceTexture.Width - originX);
        int nPixelsY = Math.Min(directEncoding.Height, sourceTexture.Height - originY);

        // Copy texture region into block
        for (int y = 0; y < nPixelsY; y++)
        {
            int sourceY = originY + y;
            for (int x = 0; x < nPixelsX; x++)
            {
                int sourceX = originX + x;
                block[x, y] = sourceTexture[sourceX, sourceY];
            }
        }

        return block;
    }
    private static IndirectBlock RegionToIndirectBlock(ushort[] indexes, int width, int height, IndirectEncoding indirectEncoding, int originX, int originY)
    {
        var block = new IndirectBlock(indirectEncoding);

        // Figure out how many pixels to copy over. On smaller textures (eg: 4x2), outside region is black.
        int nPixelsX = Math.Min(indirectEncoding.BlockWidth, width - originX);
        int nPixelsY = Math.Min(indirectEncoding.BlockHeight, height - originY);

        // Copy texture region into block
        for (int y = 0; y < nPixelsY; y++)
        {
            int sourceY = originY * width + y;
            for (int x = 0; x < nPixelsX; x++)
            {
                int sourceX = originX + x;
                int source = sourceX + sourceY;
                block[x, y] = indexes[source];
            }
        }

        return block;
    }

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
    private static void ToIndexesAndPalette256(Texture sourceTexture, IndirectEncoding indirectEncoding, TextureFormat paletteFormat, out ushort[] indexes, out Palette palette)
    {
        // Validate encoder
        switch (indirectEncoding.Format)
        {
            // Valid formats
            case TextureFormat.CI4:
            case TextureFormat.CI8:
                break;

            // Not yet supported
            case TextureFormat.CI14X2:
            {
                string msg = $"CI14X2 format is not yet supported. Use CI8 if possible.";
                throw new NotImplementedException(msg);
            }

            // Everything else
            default:
            {
                string msg = $"Type {indirectEncoding.Format} is not a valid indexed format.";
                throw new ArgumentException(msg);
            }
        };

        // init some settings
        var configuration = new Configuration() { };
        var quantizerOptions = new QuantizerOptions()
        {
            MaxColors = indirectEncoding.MaxPaletteSize,
        }; // See also: dithers
        var quantizer = new WuQuantizer(quantizerOptions);
        var rgba32Quantizer = quantizer.CreatePixelSpecificQuantizer<Rgba32>(configuration);

        // build palette and indices
        var image = ToImage(sourceTexture);
        var frame = image.Frames.RootFrame;
        var indexedImageFrame = rgba32Quantizer.BuildPaletteAndQuantizeFrame(frame, frame.Bounds());

        // PALETTE
        Rgba32[] paletteColors = indexedImageFrame.Palette.ToArray();
        palette = Palette.CreatePalette(paletteFormat);
        palette.SetColors(paletteColors);

        // INDEXES
        int stride = image.Width;
        int capacity = image.Width * image.Height;
        indexes = new ushort[capacity];
        for (int y = 0; y < image.Height; y++)
        {
            int originY = y * stride;
            var row = indexedImageFrame.GetWritablePixelRowSpanUnsafe(y);
            for (int x = 0; x < image.Width; x++)
            {
                int index = x + originY;
                indexes[index] = row[x];
            }
        }
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

        int subBlockWidth = directBlocks[0].Width;
        int subBlockHeight = directBlocks[0].Height;
        var format = directBlocks[0].Format;

        int pixelsWidth = blocksCountHorizontal * subBlockWidth;
        int pixelsHeight = blocksCountVertical * subBlockHeight;
        var texture = new Texture(pixelsWidth, pixelsHeight, format);
        texture.Blocks = directBlocks;

        int pixelIndex = 0;
        // Linearize texture pixels
        for (int v = 0; v < blocksCountVertical; v++)
        {
            for (int y = 0; y < subBlockHeight; y++)
            {
                for (int h = 0; h < blocksCountHorizontal; h++)
                {
                    // Which block we are sampling
                    int blockIndex = h + v * blocksCountHorizontal;
                    for (int x = 0; x < subBlockWidth; x++)
                    {
                        // Which sub-block we are sampling
                        int colorIndex = x + y * subBlockWidth;
                        var block = directBlocks[blockIndex];
                        var color = block.Colors[colorIndex];
                        texture.Pixels[pixelIndex++] = color;
                    }
                }
            }
        }
        return texture;
    }

    /// <summary>
    ///     Create a texture from an array of <paramref name="indirectBlocks"/> where <paramref name="blocksCountHorizontal"/>
    ///     defines the number of blocks across the texture width and <paramref name="blocksCountVertical"/> defines the number
    ///     of blocks across the texture height.
    /// </summary>
    /// <param name="indirectBlocks">The source texture blocks to construct the texture with.</param>
    /// <param name="blocksCountHorizontal">The number of blocks along the horizontal axis.</param>
    /// <param name="blocksCountVertical">The number of blocks along the vertical axis.</param>
    /// <param name="palette">The colour palette for the <paramref name="indirectBlocks"/> indexes to sample from.</param>
    /// <returns>
    ///     A new texture created from the source <paramref name="indirectBlocks"/> and <paramref name="palette"/>.
    /// </returns>
    public static Texture FromIndirectBlocksAndPalette(IndirectBlock[] indirectBlocks, int blocksCountHorizontal, int blocksCountVertical, Palette palette)
    {
        int numBlocks = blocksCountHorizontal * blocksCountVertical;
        if (numBlocks != indirectBlocks.Length)
        {
            string msg =
                $"Number of {nameof(IndirectBlock)} does not match length " +
                $"{nameof(blocksCountHorizontal)}*{nameof(blocksCountVertical)}.";
            throw new ArgumentException(msg);
        }

        int blockWidth = indirectBlocks[0].Width;
        int blockHeight = indirectBlocks[0].Height;
        int pixelsCount = blocksCountHorizontal * blocksCountVertical * blockWidth * blockHeight;
        var texture = new Texture
        {
            Format = indirectBlocks[0].Format,
            Width = blocksCountHorizontal * blockWidth,
            Height = blocksCountVertical * blockHeight,
            Pixels = new TextureColor[pixelsCount],
            Palette = palette,
            Blocks = indirectBlocks,
        };

        int pixelIndex = 0;
        // Linearize texture pixels
        for (int v = 0; v < blocksCountVertical; v++)
        {
            for (int y = 0; y < blockHeight; y++)
            {
                for (int h = 0; h < blocksCountHorizontal; h++)
                {
                    int blockIndex = v * blocksCountHorizontal + h;
                    for (int x = 0; x < blockWidth; x++)
                    {
                        int subBlockIndex = y * blockWidth + x;
                        var indirectBlock = indirectBlocks[blockIndex];
                        var indirectIndex = indirectBlock.ColorIndexes[subBlockIndex];
                        var color = palette.Colors[indirectIndex];
                        texture.Pixels[pixelIndex++] = color;
                    }
                }
            }
        }
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
    public static Texture FromRawColors(uint[] rawColors, int width, int height)
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
        cropped.Blocks = sourceTexture.Blocks;

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
                int dx = destinationOriginX + x; ;
                destinationTexture[dx, dy] = sourceTexture[x, y];
            }
        }
    }

    // 2023/07/08: make simple functions for read/write
    public static Texture ReadDirectColorTexture(EndianBinaryReader reader, TextureFormat directFormat, int pxWidth, int pxHeight)
    {
        DirectEncoding encoding = DirectEncoding.GetEncoding(directFormat);
        int blocksWidth = (int)MathF.Ceiling(pxWidth / encoding.Width);
        int blocksHeight = (int)MathF.Ceiling(pxHeight / encoding.Height);
        int blocksCount = blocksWidth * blocksHeight;
        DirectBlock[] directBlocks = encoding.ReadBlocks<DirectBlock>(reader, encoding, blocksCount);
        Texture texture = FromDirectBlocks(directBlocks, blocksWidth, blocksHeight);
        return texture;
    }
    public static Texture ReadIndirectColorTexture(EndianBinaryReader reader, Palette palette, TextureFormat colorIndexFormat, int pxWidth, int pxHeight)
    {
        IndirectEncoding encoding = IndirectEncoding.GetEncoding(colorIndexFormat);
        int blocksWidth = (int)MathF.Ceiling(pxWidth / encoding.BlockWidth);
        int blocksHeight = (int)MathF.Ceiling(pxHeight / encoding.BlockHeight);
        int blocksCount = blocksWidth * blocksHeight;
        IndirectBlock[] blocks = encoding.ReadBlocks<IndirectBlock>(reader, encoding, blocksCount);
        Texture texture = FromIndirectBlocksAndPalette(blocks, blocksWidth, blocksHeight, palette);
        return texture;
    }
    public static void WriteDirectColorTexture(EndianBinaryWriter writer, Texture texture, TextureFormat directFormat)
    {
        DirectEncoding encoding = DirectEncoding.GetEncoding(directFormat);
        DirectBlock[] blocks = CreateDirectColorBlocksFromTexture(texture, encoding);
        encoding.WriteBlocks(writer, blocks);
    }
    public static void WriteIndirectColorTexture(EndianBinaryWriter writer, Texture texture, TextureFormat indirectFormat, TextureFormat paletteFormat)
    {
        IndirectEncoding encoding = IndirectEncoding.GetEncoding(indirectFormat);
        IndirectBlock[] blocks = CreateIndirectColorBlocksAndPaletteFromTexture(texture, encoding, paletteFormat, out Palette palette, out _, out _);
        palette.WritePalette(writer, encoding);
        encoding.WriteBlocks(writer, blocks);
    }


    /// <summary>
    ///     Compute the max number of mipmaps this a texture of size
    ///     <paramref name="width"/> and <paramref name="height"/>
    ///     would have.
    /// </summary>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <returns>
    ///     Max number of valid mipmaps for the specified size.
    /// </returns>
    public static ushort GetMaxMipmapCount(int width, int height)
    {
        ushort mipmapCount = 0;
        while (width > 1 && height > 1)
        {
            width >>= 1;
            height >>= 1;
            mipmapCount++;
        }
        return mipmapCount;
    }

    public byte[] GetRawBytes(TextureFormat textureFormat)
    {
        var memoryStream = new System.IO.MemoryStream();
        using var writer = new EndianBinaryWriter(memoryStream, Endianness.BigEndian);
        Texture.WriteDirectColorTexture(writer, this, textureFormat);
        byte[] rawData = memoryStream.ToArray();
        return rawData;
    }











    // NEW STUFF

    public static IndirectBlock[] QuantizeTexture(Texture texture, PaletteColorFormat colorFormat, IndirectEncoding indirectEncoding)
    {
        // TODO: quantize texture 
        TextureColor[] paletteColors = [];
        ushort[] textureIndexes = [];

        // Create palettes from quantized data
        Palette palette = new(colorFormat, indirectEncoding, paletteColors);

        // TODO: loop over texture, extracting the appropriate strides
        // for x,y
        IndirectBlock indirectBlock = new(indirectEncoding, palette)
        {
            ColorIndexes = [],
        };

        // then return array of indirect blocks
        throw new NotImplementedException();
    }
}
