using System;
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

            //FT_Set_Pixel_Sizes(face, 0, fontSize);

            //if (FT_Load_Char(face, 'M', FT_LOAD_RENDER) != 0)
            //    throw new JEFTDotNetException($"Could not load character 'M'.");

            //var glyph = face.Glyph();
            //var metrics = glyph.Metrics();
            //var bitmap = glyph.Bitmap();
            //fontWidth = (int)bitmap.width;
            //fontHeight = (int)bitmap.rows;
            //fontLineHeight = (int)(metrics.height / 64);

            //foreach (var ch in characters)
            //{
            //    if (FT_Load_Char(face, ch, FT_LOAD_RENDER) != 0)
            //        throw new PixelCannonException($"Could not load character '{ch}'.");

            //    glyph = face.Glyph();
            //    metrics = glyph.Metrics();
            //    bitmap = glyph.Bitmap();
            //    var width = (int)(metrics.width / 64);
            //    var height = (int)(metrics.height / 64);
            //    var offsetX = (int)(metrics.horiBearingX / 64);
            //    var offsetY = fontLineHeight - (int)(metrics.horiBearingY / 64);
            //    var advance = glyph.Advance();

            //    characterSurfaces[ch] = RenderGlyph(bitmap, alpha);

            //    var data = new Character()
            //    {
            //        Width = width,
            //        Height = height,
            //        OffsetX = offsetX,
            //        OffsetY = offsetY,
            //        AdvanceX = (int)(advance.x / 64),
            //        AdvanceY = (int)(advance.y / 64),
            //    };

            //    characterData[ch] = data;


        }

        public void Dispose()
        {
            FT_Done_FreeType(_library);
            _streamWrapper.Dispose();
        }

        public FontImage RenderCharacter(char ch, int size)
        {
            FT_Set_Pixel_Sizes(_face, 0, (uint)size);

            if (FT_Load_Char(_face, ch, FT_LOAD_RENDER) != 0)
                throw new JEFTDotNetException($"Could not load character '{ch}'.");

            var glyph = _face.Glyph();
            var metrics = glyph.Metrics();
            var bitmap = glyph.Bitmap();
            var width = (int)(metrics.width / 64);
            var height = (int)(metrics.height / 64);

            var bitmapData = ConvertBitmap(bitmap);

            return new FontImage(bitmapData.width, bitmapData.height, bitmapData.pixels);
        }

        private static (int width, int height, byte[] pixels) ConvertBitmap(FT_Bitmap bitmap)
        {
            if (bitmap.rows == 0 || bitmap.pitch == 0)
                return (0, 0, Array.Empty<byte>());

            var pixels = new byte[bitmap.rows * bitmap.pitch];
            Marshal.Copy(bitmap.buffer, pixels, 0, pixels.Length);

            return ((int)bitmap.pitch, (int)bitmap.rows, pixels);
        }
    }
}
