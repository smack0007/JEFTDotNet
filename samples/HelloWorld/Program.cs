using System;
using JEFTDotNet;
using ImageDotNet;
using System.Linq;

namespace HelloWorld
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var font = Font.LoadFromFile("OpenSans-Regular.ttf");
            
            var character = font.RenderCharacter(48, 'M');
            var image = new Image<Rgba32>(character.Width, character.Height, TransformPixels(character, 0));
            image.SavePng("M.png");

            var atlas = font.RenderAtlas(48, Enumerable.Range(32, 126 - 32).Select(x => (char)x));
            image = new Image<Rgba32>(atlas.Image.Width, atlas.Image.Height, TransformPixels(atlas.Image, 0));
            image.SavePng("Font.png");
        }

        private static Rgba32[] TransformPixels(FontImage image, byte alpha)
        {
            var output = new Rgba32[image.Length];

            for (int i = 0; i < image.Length; i++)
            {
                output[i].R = image[i];
                output[i].G = image[i];
                output[i].B = image[i];
                output[i].A = image[i] != 0 ? (byte)255 : alpha;
            }

            return output;
        }
    }
}
