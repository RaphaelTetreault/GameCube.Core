﻿using Manifold;
using Manifold.IO;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCube.GX.Texture
{
    public class Texture
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public TextureColor[] Pixels { get; private set; }
        public bool IsPaletted { get; private set; }
        public int[] PaletteIndexes { get; private set; }
        public Palette Palette { get; private set; }


        public static Texture FromColors(TextureColor[] colors, int width, int height)
        {
            throw new NotImplementedException();
        }

        public static Texture FromDirectTiles(DirectTile[] directTiles, int tilesWidth, int tilesHeight)
        {
            throw new NotImplementedException();
        }

        public static Texture FromIndexTilesAndPalette(IndirectTile[] indirectTiles, int tilesWidth, int tilesHeight, Palette palette)
        {
            throw new NotImplementedException();
        }

        public static Texture FromRawColors(uint[] rawColors, int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}
