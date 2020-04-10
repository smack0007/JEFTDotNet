using System;
using JEFTDotNet;
using ImageDotNet;

namespace HelloWorld
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var font = Font.LoadFromFile("OpenSans-Regular.ttf");
            var character = font.RenderCharacter('M', 48);

            var image = new Image<Rgba32>(character.Width, character.Height, TransformPixels(character, 0));
            image.SavePng("M.png");
            image.SaveTga("M.tga");
        }

        private static Rgba32[] TransformPixels(FontImage image, byte a)
        {
            var output = new Rgba32[image.Length];

            for (int i = 0; i < image.Length; i++)
            {
                output[i].R = image[i];
                output[i].G = image[i];
                output[i].B = image[i];
                output[i].A = image[i];
            }

            return output;
        }
    }
}
