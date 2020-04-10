using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static JEFTDotNet.FreeType;

namespace JEFTDotNet
{
    public class Font : IDisposable
    {
        private IntPtr _library;
        private FreeTypeStreamWrapper _streamWrapper;
        private FT_Face _face;

        private Font(IntPtr library, FreeTypeStreamWrapper streamWrapper, FT_Face face)
        {
            _library = library;
            _streamWrapper = streamWrapper;
            _face = face;
        }

        public static Font LoadFromFile(string filePath) => LoadFromStream(File.OpenRead(filePath));

        public static Font LoadFromStream(Stream stream)
        {
            FT_Init_FreeType(out IntPtr library);

            var streamWrapper = new FreeTypeStreamWrapper(stream);

            if (FT_Open_Face(library, streamWrapper, 0, out var face) != 0)
                throw new JEFTDotNetException("Could not open font.");

            return new Font(library, streamWrapper, face);
        }

        public void Dispose()
        {
            FT_Done_FreeType(_library);
            _streamWrapper.Dispose();
        }

        public FontImage RenderCharacter(int size, char ch)
        {
            FT_Set_Pixel_Sizes(_face, 0, (uint)size);

            if (FT_Load_Char(_face, ch, FT_LOAD_RENDER) != 0)
                throw new JEFTDotNetException($"Could not load character '{ch}'.");

            var glyph = _face.Glyph();
            var bitmap = glyph.Bitmap();

            var bitmapData = ConvertBitmap(bitmap);

            return new FontImage(bitmapData.width, bitmapData.height, bitmapData.pixels);
        }

        public FontAtlas RenderAtlas(int size, IEnumerable<char> characters)
        {
            int fontWidth = 0;
            int fontHeight = 0;
            int fontLineHeight = 0;
            var characterImages = new Dictionary<char, FontImage>();
            var characterData = new Dictionary<char, FontAtlasCharacter>();

            FT_Set_Pixel_Sizes(_face, 0, (uint)size);

            if (FT_Load_Char(_face, 'M', FT_LOAD_RENDER) != 0)
                throw new JEFTDotNetException($"Could not load character 'M'.");

            var glyph = _face.Glyph();
            var metrics = glyph.Metrics();
            var bitmap = glyph.Bitmap();
            fontWidth = (int)bitmap.width;
            fontHeight = (int)bitmap.rows;
            fontLineHeight = (int)(metrics.height / 64);

            foreach (var ch in characters)
            {
                if (FT_Load_Char(_face, ch, FT_LOAD_RENDER) != 0)
                    throw new JEFTDotNetException($"Could not load character '{ch}'.");

                glyph = _face.Glyph();
                metrics = glyph.Metrics();
                bitmap = glyph.Bitmap();
                var width = (int)(metrics.width / 64);
                var height = (int)(metrics.height / 64);
                var offsetX = (int)(metrics.horiBearingX / 64);
                var offsetY = fontLineHeight - (int)(metrics.horiBearingY / 64);
                var advance = glyph.Advance();

                var bitmapData = ConvertBitmap(bitmap);
                characterImages[ch] = new FontImage(bitmapData.width, bitmapData.height, bitmapData.pixels);

                var data = new FontAtlasCharacter()
                {
                    Width = width,
                    Height = height,
                    OffsetX = offsetX,
                    OffsetY = offsetY,
                    AdvanceX = (int)(advance.x / 64),
                    AdvanceY = (int)(advance.y / 64),
                };

                characterData[ch] = data;
            }

            var image = MergeImages(characterImages, characterData, fontWidth, fontHeight);
            
            return new FontAtlas(image, characterData);
        }

        private static (int width, int height, byte[] pixels) ConvertBitmap(FT_Bitmap bitmap)
        {
            if (bitmap.rows == 0 || bitmap.pitch == 0)
                return (0, 0, Array.Empty<byte>());

            var pixels = new byte[bitmap.rows * bitmap.pitch];
            Marshal.Copy(bitmap.buffer, pixels, 0, pixels.Length);

            return ((int)bitmap.pitch, (int)bitmap.rows, pixels);
        }

        private static FontImage MergeImages(
           Dictionary<char, FontImage> characterImages,
           Dictionary<char, FontAtlasCharacter> characterData,
           int fontWidth,
           int fontHeight)
        {
            var halfCharacterCount = characterData.Values.Count / 4;
            var imageWidth = (int)MathHelper.RoundClosestPowerOf2((uint)(halfCharacterCount * fontWidth));

            var charactersPerLine = imageWidth / fontWidth;
            var neededLines = (int)Math.Ceiling(characterData.Values.Count / (float)charactersPerLine);
            var imageHeight = (int)MathHelper.RoundNextPowerOf2((uint)(fontHeight * neededLines));

            var result = new FontImage(imageWidth, imageHeight);

            var x = 0;
            var y = 0;
            foreach (var character in characterImages.Keys)
            {
                var image = characterImages[character];
                var data = characterData[character];

                if (x + image.Width > result.Width)
                {
                    y += fontHeight;
                    x = 0;
                }

                result.Blit(image, x, y);
                data.X = x;
                data.Y = y;

                x += image.Width;
            }

            return result;
        }
    }
}
