﻿using Manifold.IO;
using System;

namespace GameCube.GX.Texture
{
    /// <summary>
    /// The base representation of a GameCube texture colour format encoding.
    /// </summary>
    public abstract class Encoding
    {
        // Encodings for textures
        public static readonly EncodingI4 EncodingI4 = new();
        public static readonly EncodingI8 EncodingI8 = new();
        public static readonly EncodingIA4 EncodingIA4 = new();
        public static readonly EncodingIA8 EncodingIA8 = new();
        public static readonly EncodingRGB565 EncodingRGB565 = new();
        public static readonly EncodingRGB5A3 EncodingRGB5A3 = new();
        public static readonly EncodingRGBA8 EncodingRGBA8 = new();
        public static readonly EncodingCI4 EncodingCI4 = new();
        public static readonly EncodingCI8 EncodingCI8 = new();
        public static readonly EncodingCI14X2 EncodingCI14X2 = new();
        public static readonly EncodingCMPR EncodingCMPR = new();

        /// <summary>
        ///     The pixel width of a block for this encoding.
        /// </summary>
        public abstract byte BlockWidth { get; }

        /// <summary>
        ///     The pixel height of a block for this encoding.
        /// </summary>
        public abstract byte BlockHeight { get; }

        /// <summary>
        ///     True if this block uses direct colour encoding.
        /// </summary>
        public abstract bool IsDirect { get; }

        /// <summary>
        ///     True if this block uses indirect colour encoding.
        /// </summary>
        public abstract bool IsIndirect { get; }

        /// <summary>
        ///     The texture format used by this encoding.
        /// </summary>
        public abstract TextureFormat Format { get; }

        /// <summary>
        ///     The number of bytes used by this encoding per block.
        /// </summary>
        public abstract int BytesPerBlock { get; }

        /// <summary>
        ///     Read a block using this encoding.
        /// </summary>
        /// <param name="reader">The reader stream to read a block from.</param>
        /// <returns>
        ///     A texture block in the format specified by this encoding.
        /// </returns>
        public abstract Block ReadBlock(EndianBinaryReader reader);

        /// <summary>
        ///     Read a number of texture blocks from a binary stream.
        /// </summary>
        /// <typeparam name="TBlock">The type of block to encode.</typeparam>
        /// <param name="reader">The reader stream to read blocks from.</param>
        /// <param name="blocksWidthCount">The number of horizontal blocks to read from the stream.</param>
        /// <param name="blocksHeightCount">The number of vertical blocks to read from the stream.</param>
        /// <param name="encoding">The encoding used to deserialize the target texture block.</param>
        /// <returns>
        ///     
        /// </returns>
        public TBlock[] ReadBlocks<TBlock>(EndianBinaryReader reader, Encoding encoding, int blocksWidthCount, int blocksHeightCount)
            where TBlock : Block
        {
            int blocksCount = blocksWidthCount * blocksHeightCount;
            var blocks = ReadBlocks<TBlock>(reader, encoding, blocksCount);
            return blocks;
        }

        /// <summary>
        ///     Read a number of texture blocks from a binary stream.
        /// </summary>
        /// <typeparam name="TBlock">The type of block to encode.</typeparam>
        /// <param name="reader">The reader stream to read blocks from.</param>
        /// <param name="encoding">The encoding used to deserialize the target texture block.</param>
        /// <param name="blocksCount">The total number of blocks to read from the stream.</param>
        /// <returns>
        ///     
        /// </returns>
        public TBlock[] ReadBlocks<TBlock>(EndianBinaryReader reader, Encoding encoding, int blocksCount)
            where TBlock : Block
        {
            var blocks = new TBlock[blocksCount];
            for (int i = 0; i < blocksCount; i++)
            {
                var block = encoding.ReadBlock(reader);
                blocks[i] = block as TBlock;
            }
            return blocks;
        }


        /// <summary>
        ///     Write a texture block using this encoding.
        /// </summary>
        /// <param name="writer">The stream to write a texture block to.</param>
        /// <param name="block">The texture block to write to the stream.</param>
        public abstract void WriteBlock(EndianBinaryWriter writer, Block block);

        /// <summary>
        ///     Write a block using this encoding.
        /// </summary>
        /// <param name="writer">The stream to write a texture block to.</param>
        /// <param name="blocks">The texture blocks to write to the stream.</param>
        public void WriteBlocks(EndianBinaryWriter writer, Block[] blocks)
        {
            foreach (var block in blocks)
                WriteBlock(writer, block);
        }

        public void WriteTexture(EndianBinaryWriter writer, Block[] blocks) => WriteBlocks(writer, blocks);

        public void WriteTexture(EndianBinaryWriter writer, Texture texture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Fetch a shared encoding instance for the provided <paramref name="textureFormat"/> format.
        /// </summary>
        /// <param name="textureFormat">The texture format encoding you want.</param>
        /// <returns>
        ///     
        /// </returns>
        /// <exception cref="System.Exception">
        ///     Thrown if the <paramref name="textureFormat"/> is not defined.
        /// </exception>
        public static Encoding GetEncoding(TextureFormat textureFormat)
        {
            switch (textureFormat)
            {
                case TextureFormat.I4: return EncodingI4;
                case TextureFormat.I8: return EncodingI8;
                case TextureFormat.IA4: return EncodingIA4;
                case TextureFormat.IA8: return EncodingIA8;
                case TextureFormat.RGB565: return EncodingRGB565;
                case TextureFormat.RGB5A3: return EncodingRGB5A3;
                case TextureFormat.RGBA8: return EncodingRGBA8;
                case TextureFormat.CI4: return EncodingCI4;
                case TextureFormat.CI8: return EncodingCI8;
                case TextureFormat.CI14X2: return EncodingCI14X2;
                case TextureFormat.CMPR: return EncodingCMPR;
                default: throw new System.Exception($"Unhandled texture format {textureFormat}.");
            }
        }

        /// <summary>
        ///     Returns the number of blocks required to encode a <paramref name="widthPixels"/> by
        ///     <paramref name="heightPixels"/> sized texture using this encoding.
        /// </summary>
        /// <param name="widthPixels">The width of the texture to encode.</param>
        /// <param name="heightPixels">The height of the texture to encode.</param>
        /// <returns>
        ///     The total number of blocks required to encode.
        /// </returns>
        public int GetTotalBlocksToEncode(int widthPixels, int heightPixels)
            => GetTotalBlocksToEncode(widthPixels, heightPixels, BlockWidth, BlockHeight);

        /// <summary>
        ///     Returns the number of blocks required to encode a <paramref name="widthPixels"/> by
        ///     <paramref name="heightPixels"/> sized texture using a <paramref name="blockWidth"/>
        ///     by <paramref name="blockHeight"/> sized encoding.
        /// </summary>
        /// <param name="widthPixels">The width of the texture to encode.</param>
        /// <param name="heightPixels">The height of the texture to encode.</param>
        /// <param name="blockWidth">The block width size of an encoding.</param>
        /// <param name="blockHeight">The block height size of an encoding.</param>
        /// <returns>
        ///     The total number of blocks required to encode.
        /// </returns>
        public static int GetTotalBlocksToEncode(int widthPixels, int heightPixels, int blockWidth, int blockHeight)
        {
            int nBlocksWidth = (int)Math.Ceiling(widthPixels / (float)blockWidth);
            int nBlocksHeight = (int)Math.Ceiling(heightPixels / (float)blockHeight);
            int totalBlocks = nBlocksWidth * nBlocksHeight;
            return totalBlocks;
        }

    }
}
