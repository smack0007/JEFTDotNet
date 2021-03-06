﻿using System;

namespace JEFTDotNet
{
    public class FontImage
    {
        private byte[] _pixels;

        public int Width { get; }

        public int Height { get; private set; }

        public int Length => _pixels.Length;

        public byte this[int i] => _pixels[i];

        public byte this[int x, int y] => _pixels[y * Width + x];

        internal FontImage(int width, int height, byte[]? pixels = null)
        {
            Width = width;
            Height = height;

            if (pixels == null)
                pixels = new byte[width * height];
            
            _pixels = pixels;
        }

        internal void Blit(FontImage image, int destX, int destY)
        {
            for (int srcY = 0; srcY < image.Height; srcY++)
            {
                for (int srcX = 0; srcX < image.Width; srcX++)
                {
                    _pixels[(destY + srcY) * Width + destX + srcX] = image[srcX, srcY];
                }
            }
        }

        internal void CropToHeight(int height)
        {
            var newPixels = new byte[Width * height];
            Buffer.BlockCopy(_pixels, 0, newPixels, 0, newPixels.Length);

            Height = height;
            _pixels = newPixels;
        }
    }
}
