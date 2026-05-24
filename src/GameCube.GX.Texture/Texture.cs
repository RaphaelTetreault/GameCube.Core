// Invaluable resource:
// https://wiki.tockdom.com/wiki/Image_Formats
// TODO: implement own quantization
// https://en.wikipedia.org/wiki/Median_cut
// https://en.wikipedia.org/wiki/K-means_clustering
// And consider where dithering fits in?
// https://en.wikipedia.org/wiki/Dither
// https://en.wikipedia.org/wiki/Floyd%E2%80%93Steinberg_dithering
// https://tannerhelland.com/2012/12/28/dithering-eleven-algorithms-source-code.html

using Manifold.IO;
using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using System.Collections.Immutable;

namespace GameCube.GX.Texture;

/// <summary>
///     A GameCube GX texture.
/// </summary>
public class Texture
{
    private readonly record struct BlockOrigin(int X, int Y);

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
    public TexturePixel[] Pixels { get; private set; } = [];

    /// <summary>
    ///     Indexer to get get/set a pixel within this texture.
    /// </summary>
    /// <param name="i">The pixel index in this texture.</param>
    /// <returns>
    ///     Pixel at the specified index within this block.
    /// </returns>
    public TexturePixel this[int i]
    {
        get => Pixels[i];
        set => Pixels[i] = value;
    }

    /// <summary>
    ///     Indexer to get get/set a pixel within this texture.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel in this texture.</param>
    /// <param name="y">The vertical coordinate of the pixel in this texture.</param>
    /// <returns>
    ///     Pixel at the specified coordinate within this block.
    /// </returns>
    public TexturePixel this[int x, int y]
    {
        get => Pixels[x + y * Width];
        set => Pixels[x + y * Width] = value;
    }


    /// <summary>
    ///     Create a new empty texture.
    /// </summary>
    public Texture() { }

    /// <summary>
    ///     Create a new texture of <paramref name="width"/> by <paramref name="height"/> size.
    /// </summary>
    /// <param name="width">The texture's pixel width.</param>
    /// <param name="height">The texture's pixel height.</param>
    public Texture(int width, int height)
    {
        Width = width;
        Height = height;
        Pixels = new TexturePixel[Width * Height];
    }

    /// <summary>
    ///     Create a new texture of <paramref name="width"/> by <paramref name="height"/> size
    ///     whose pixel contents are all <paramref name="color"/>.
    /// </summary>
    /// <param name="width">The texture's pixel width.</param>
    /// <param name="height">The texture's pixel height.</param>
    /// <param name="color">The default color of all pixels for the texture.</param>
    public Texture(int width, int height, TexturePixel color)
    {
        Width = width;
        Height = height;
        Pixels = new TexturePixel[Width * Height];

        // Set all colors as default
        for (int i = 0; i < Pixels.Length; i++)
            Pixels[i] = color;
    }


    /// <summary>
    ///     Map of <see cref="TextureFormat"/> to <see cref="IBlockEncoding"/>.
    /// </summary>
    public static readonly ImmutableDictionary<TextureFormat, IBlockEncoding> MapTextureFormatToBlockEncoding =
    [
        // DIRECT ENCODINGS
        new(TextureFormat.I4, DirectEncoding.I4),
        new(TextureFormat.I8, DirectEncoding.I8),
        new(TextureFormat.IA4, DirectEncoding.IA4),
        new(TextureFormat.IA8, DirectEncoding.IA8),
        new(TextureFormat.RGB565, DirectEncoding.RGB565),
        new(TextureFormat.RGB5A3, DirectEncoding.RGB5A3),
        new(TextureFormat.RGBA8, DirectEncoding.RGBA8),
        new(TextureFormat.CMPR, DirectEncoding.CMPR),
        // INDIRECT ENCODINGS
        new(TextureFormat.CI4, IndirectEncoding.CI4),
        new(TextureFormat.CI8, IndirectEncoding.CI8),
        new(TextureFormat.CI14X2, IndirectEncoding.CI14X2),
    ];


    /// <summary>
    ///     Copy area from source array as if it were 2 dimensional. Copy is bounded 
    ///     meaning a copy over edge returns default value for copy type <typeparamref name="T"/>.
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
    ///     Array of length <paramref name="width"/>*<paramref name="height"/> from 
    ///     (<paramref name="srcOriginX"/>, <paramref name="srcOriginY"/>) which is
    ///     a copy of contents <paramref name="src"/>. If copy exceeds bounds 
    ///     <paramref name="srcWidth"/> or <paramref name="srcHeight"/>, then the
    ///     default value for <typeparamref name="T"/> will be used.
    /// </returns>
    private static T[] CopyArea<T>(ReadOnlySpan<T> src, int srcWidth, int srcHeight, int srcOriginX, int srcOriginY, int width, int height)
    {
        // Create array "slice"
        int size = width * height;
        T[] copy = new T[size];

        // Figure out how many elements to copy over. eg. Pixels. On smaller textures (eg: 4x2), outside region is black.
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

    /// <summary>
    ///     Get origin points for each block in a texture given <paramref name="blocksInfo"/>.
    /// </summary>
    /// <param name="blocksInfo"></param>
    /// <returns>
    ///     Array of origins for each block.
    /// </returns>
    private static BlockOrigin[] GetBlockOrigins(TextureBlocksInfo blocksInfo)
    {
        // Create origin for each block within texture
        BlockOrigin[] origins = new BlockOrigin[blocksInfo.BlockCount];

        // Iterate over each block
        int originIndex = 0;
        for (int y = 0; y < blocksInfo.BlockCountY; y++)
        {
            int originY = y * blocksInfo.BlockPixelHeight;
            for (int x = 0; x < blocksInfo.BlockCountX; x++)
            {
                int originX = x * blocksInfo.BlockPixelWidth;
                //int originIndex = y * blocksInfo.BlockCountX + x;
                origins[originIndex] = new(originX, originY);
                originIndex++;
            }
        }

        return origins;
    }

    public static Texture ReadDirectColorTexture(EndianBinaryReader reader, DirectTextureFormat directFormat, int pxWidth, int pxHeight)
    {
        directFormat.Validate();
        DirectEncoding directEncoding = DirectEncoding.MapDirectFormatToEncoding[directFormat];
        TextureBlocksInfo blocks = TextureBlocksInfo.FromPixelDimensions(pxWidth, pxHeight, directEncoding);
        DirectBlock[] directBlocks = directEncoding.ReadBlocks(reader, blocks.BlockCount);
        Texture texture = FromDirectBlocks(directBlocks, blocks);
        return texture;
    }

    public static Texture ReadIndirectColorTexture(EndianBinaryReader reader, IndirectTextureFormat indexFormat, Palette palette, int pxWidth, int pxHeight)
    {
        indexFormat.Validate();
        IndirectEncoding indirectEncoding = IndirectEncoding.MapIndirectFormatToEncoding[indexFormat];
        TextureBlocksInfo blocksInfo = TextureBlocksInfo.FromPixelDimensions(pxWidth, pxHeight, indirectEncoding);
        IndirectBlock[] indirectBlocks = indirectEncoding.ReadBlocks(reader, blocksInfo.BlockCount);
        Texture texture = FromIndirectBlocksAndPalette(indirectBlocks, palette, blocksInfo);
        return texture;
    }

    public static void WriteDirectColorTexture(EndianBinaryWriter writer, Texture texture, DirectTextureFormat directFormat)
    {
        directFormat.Validate();
        DirectEncoding directEncoding = DirectEncoding.MapDirectFormatToEncoding[directFormat];
        DirectBlock[] directBlocks = CreateDirectColorBlocksFromTexture(texture, directEncoding);
        foreach (DirectBlock directBlock in directBlocks)
            directEncoding.WriteDirectBlock(writer, directBlock);
    }

    public static void WriteIndirectColorTexture(EndianBinaryWriter writer, Texture texture, IndirectTextureFormat indirectFormat, PaletteColorFormat paletteFormat)
    {
        indirectFormat.Validate();
        paletteFormat.Validate();
        IndirectEncoding indirectEncoding = IndirectEncoding.MapIndirectFormatToEncoding[indirectFormat];
        (IndirectBlock[] indirectBlocks, Palette palette) = CreateIndirectColorBlocksAndPaletteFromTexture(texture, indirectEncoding, paletteFormat);
        Palette.Write(writer, palette);
        foreach (IndirectBlock indirectBlock in indirectBlocks)
            indirectEncoding.WriteIndirectBlock(writer, indirectBlock);
    }

    public static DirectBlock[] CreateDirectColorBlocksFromTexture(Texture texture, DirectEncoding directEncoding)
    {
        // 
        TextureBlocksInfo blocksInfo = TextureBlocksInfo.FromPixelDimensions(texture.Width, texture.Height, directEncoding);
        BlockOrigin[] blockOrigins = GetBlockOrigins(blocksInfo);
        DirectBlock[] blocks = new DirectBlock[blocksInfo.BlockCount];

        // 
        for (int i = 0; i < blockOrigins.Length; i++)
        {
            BlockOrigin blockOrigin = blockOrigins[i];
            TexturePixel[] pixels = CopyArea(texture.Pixels, texture.Width, texture.Height, blockOrigin.X, blockOrigin.Y, directEncoding.BlockPixelHeight, directEncoding.BlockPixelHeight);
            blocks[i] = new(directEncoding, pixels);
        }

        return blocks;
    }

    public static (IndirectBlock[] blocks, Palette palette) CreateIndirectColorBlocksAndPaletteFromTexture(Texture texture, IndirectEncoding indirectEncoding, PaletteColorFormat paletteFormat)
    {
        paletteFormat.Validate();
        // limitations of image sharp
        if (indirectEncoding.MaxPaletteSize > 256)
        {
            throw new NotImplementedException();
        }

        // 
        TextureBlocksInfo blocksInfo = TextureBlocksInfo.FromPixelDimensions(texture.Width, texture.Height, indirectEncoding);
        BlockOrigin[] blockOrigins = GetBlockOrigins(blocksInfo);
        IndirectBlock[] blocks = new IndirectBlock[blocksInfo.BlockCount];

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
        TexturePixel[] paletteColors = new TexturePixel[paletteColorsRGBA32.Length];
        for (int i = 0; i < paletteColorsRGBA32.Length; i++)
            paletteColors[i] = new TexturePixel(paletteColorsRGBA32[i].PackedValue);
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
            ushort[] blockIndexes = CopyArea(indexes, texture.Width, texture.Height, blockOrigin.X, blockOrigin.Y, indirectEncoding.BlockPixelHeight, indirectEncoding.BlockPixelHeight);
            blocks[i] = new(indirectEncoding, blockIndexes);
        }

        return (blocks, palette);
    }

    /// <summary>
    ///     Convert texture to image.
    /// </summary>
    /// <param name="sourceTexture"></param>
    /// <returns>
    ///     
    /// </returns>
    public static Image<Rgba32> ToImage(Texture sourceTexture)
    {
        Image<Rgba32> image = new(sourceTexture.Width, sourceTexture.Height);

        for (int y = 0; y < sourceTexture.Height; y++)
        {
            for (int x = 0; x < sourceTexture.Width; x++)
            {
                TexturePixel pixel = sourceTexture[x, y];
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
    public static Texture FromColors(TexturePixel[] colors, int width, int height)
    {
        int numPixels = width * height;
        if (numPixels != colors.Length)
        {
            string msg = "Number of raw colors does not match length of width*height.";
            throw new ArgumentException(msg);
        }

        var texture = new Texture
        {
            Width = width,
            Height = height,
            Pixels = new TexturePixel[numPixels]
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
            Width = width,
            Height = height,
            Pixels = new TexturePixel[numPixels]
        };

        // Copy in pixels
        for (int i = 0; i < rawColors.Length; i++)
            texture.Pixels[i] = new TexturePixel(rawColors[i]);

        return texture;
    }

    /// <summary>
    ///     Create a texture from an array of <paramref name="directBlocks"/> where <paramref name="blocksInfo"/>
    ///     defines the configuration of blocks for the texture.
    /// </summary>
    /// <param name="directBlocks">The direct blocks to construct the texture with.</param>
    /// <param name="blocksInfo">Information about the configuration of blocks.</param>
    /// <returns>
    ///     A new texture created from the source <paramref name="directBlocks"/>.
    /// </returns>
    public static Texture FromDirectBlocks(DirectBlock[] directBlocks, TextureBlocksInfo blocksInfo)
    {
        // Sanity check
        if (blocksInfo.BlockCount != directBlocks.Length)
        {
            string msg = $"Number of blocks does not match length info.";
            throw new ArgumentException(msg);
        }

        // Copy references of blocks from direct blocks
        ImmutableArray<TexturePixel>[] blocks = new ImmutableArray<TexturePixel>[blocksInfo.BlockCount];
        for (int i = 0; i < blocks.Length; i++)
            blocks[i] = directBlocks[i].Pixels;

        // Construct texture from deswizzled blocks
        ImmutableArray<TexturePixel> texturePixels = DeswizzleBlocks(blocks, blocksInfo);
        Texture texture = new()
        {
            Width = blocksInfo.TexturePixelWidth,
            Height = blocksInfo.TexturePixelHeight,
            Pixels = [.. texturePixels],
        };
        return texture;
    }

    /// <summary>
    ///     Create a texture from an array of <paramref name="indirectBlocks"/> 
    ///     and <paramref name="palette"/> where <paramref name="blocksInfo"/>
    ///     defines the configuration of blocks for the texture.
    /// </summary>
    /// <param name="indirectBlocks">The indirect blocks to construct the texture with.</param>
    /// <param name="palette">The palette for the indirect blocks.</param>
    /// <param name="blocksInfo">Information about the configuration of blocks.</param>
    /// <returns>
    ///     A new texture created from the source <paramref name="indirectBlocks"/>
    ///     and <paramref name="palette"/>.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public static Texture FromIndirectBlocksAndPalette(IndirectBlock[] indirectBlocks, Palette palette, TextureBlocksInfo blocksInfo)
    {
        // Sanity check
        if (blocksInfo.BlockCount != indirectBlocks.Length)
        {
            string msg = $"Number of blocks does not match length info.";
            throw new ArgumentException(msg);
        }

        // Construct pixels from indexes and pallete
        ImmutableArray<TexturePixel>[] blocks = new ImmutableArray<TexturePixel>[blocksInfo.BlockCount];
        for (int i = 0; i < blocks.Length; i++)
            blocks[i] = IndirectBlocksAndPaletteToTextureColors(indirectBlocks[i], palette);

        // Construct texture from deswizzled blocks
        ImmutableArray<TexturePixel> texturePixels = DeswizzleBlocks(blocks, blocksInfo);
        Texture texture = new()
        {
            Width = blocksInfo.TexturePixelWidth,
            Height = blocksInfo.TexturePixelHeight,
            Pixels = [.. texturePixels],
        };
        return texture;

        // Index into palette and construct pixel colors.
        static ImmutableArray<TexturePixel> IndirectBlocksAndPaletteToTextureColors(IndirectBlock indirectBlocks, Palette palette)
        {
            TexturePixel[] colors = new TexturePixel[indirectBlocks.ColorIndexes.Length];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = palette.Colors[indirectBlocks.ColorIndexes[i]];
            ImmutableArray<TexturePixel> value = ImmutableArray.Create(colors);
            return value;
        }
    }

    /// <summary>
    ///     Convert from block-order to pixel-order.
    /// </summary>
    /// <param name="blocks">The texture blocks.</param>
    /// <param name="blocksInfo">Information about the blocks.</param>
    /// <returns>
    ///     
    /// </returns>
    private static ImmutableArray<TexturePixel> DeswizzleBlocks(ReadOnlySpan<ImmutableArray<TexturePixel>> blocks, TextureBlocksInfo blocksInfo)
    {
        // GOAL: Linearize texture pixels.
        // HOW: We will step through in this over to copy the top line of pixels from each block into the destination.
        //      Example: 4x4 blocks, each block 8x8 pixels.
        //      Loop over each block row (Y) of blocks, eg. loop through 4 blocks per Y row inside texture.
        //      Loop over each pixel row (Y) in block,  eg. loop through 8 pixels per Y row inside block.
        //      Loop over each block col (X) in blocks, eg. loop through 4 blocks per X column inside texture.
        //      Loop over each pixel col (X) in block,  eg. loop through 8 pixels per X column inside block.

        // Our pixels and which index we are currently copying into array.
        TexturePixel[] texture = new TexturePixel[blocksInfo.BlockPixelCount];
        int texturePixelIndex = 0;

        // Iterate over each block row on Y axis, top to bottom
        for (int blockY = 0; blockY < blocksInfo.BlockCountY; blockY++)
        {
            // Iterate over each pixel row on Y, top to bottom
            for (int pixelY = 0; pixelY < blocksInfo.BlockPixelHeight; pixelY++)
            {
                // Convert Y 2D position to 1D stride inside blocks
                int blockIndexY = blockY * blocksInfo.BlockCountX;
                // Iterate over each block along X axis, left to right
                for (int blockX = 0; blockX < blocksInfo.BlockCountX; blockX++)
                {
                    // Which pixel we are sampling
                    int pixelIndexY = pixelY * blocksInfo.BlockPixelWidth;

                    // Which block we are sampling
                    int blockIndex = blockX + blockIndexY;
                    // Iterate over each pixel in row, left to right
                    for (int pixelX = 0; pixelX < blocksInfo.BlockPixelWidth; pixelX++)
                    {
                        // Which sub-block we are sampling
                        int pixelIndex = pixelX + pixelIndexY;

                        // Get block, get pixel from block, assign to texture pixels
                        var block = blocks[blockIndex];
                        var pixel = block[pixelIndex];
                        texture[texturePixelIndex] = pixel;
                        texturePixelIndex++;
                    }
                }
            }
        }

        ImmutableArray<TexturePixel> texturePixels = ImmutableArray.Create(texture);
        return texturePixels;
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
        var cropped = new Texture(pixelWidth, pixelHeight);
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

    // TODO: collapse both Copy functions. eg. This one could be called inside the other or vice-versa.

    /// <summary>
    ///     
    /// </summary>
    /// <param name="sourceTexture"></param>
    /// <param name="destinationTexture"></param>
    /// <param name="destinationOriginX"></param>
    /// <param name="destinationOriginY"></param>
    /// <exception cref="ArgumentException"></exception>
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
    ///     Get this texture serialized as bytes.
    /// </summary>
    /// <param name="directTextureFormat">The color format to encode the texture in.</param>
    /// <returns>
    ///     New array of bytes of this texture encoded with <paramref name="directTextureFormat"/>.
    /// </returns>
    public byte[] GetRawBytes(DirectTextureFormat directTextureFormat)
    {
        directTextureFormat.Validate();
        var memoryStream = new System.IO.MemoryStream();
        using var writer = new EndianBinaryWriter(memoryStream, Endianness.BigEndian);
        WriteDirectColorTexture(writer, this, directTextureFormat);
        writer.Flush();
        byte[] rawData = memoryStream.ToArray();
        writer.Close();
        return rawData;
    }

}
