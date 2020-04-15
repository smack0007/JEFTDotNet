using System;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace JEFTDotNet.CLI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "jeft",
                Description = "Just Enough FreeType for DotNet",
            };

            app.HelpOption(inherited: true);

            app.Command("generateFontAtlas", generateFontAtlasCmd =>
            {
                var input = generateFontAtlasCmd.Argument<string>("input", "The input TTF file.");
                var outputImage = generateFontAtlasCmd.Argument<string>("outputImage", "The output image file.");

                generateFontAtlasCmd.OnExecute(() =>
                {
                    if (input.Value == null)
                    {
                        Console.Error.WriteLine("Please specify an input file.");
                        return 1;
                    }

                    if (outputImage.Value == null)
                    {
                        Console.Error.WriteLine("Please specify an output png file.");
                        return 1;
                    }

                    return GenerateFontAtlas(input.Value, outputImage.Value);
                });
            });

            app.OnExecute(() =>
            {
                Console.Error.WriteLine("Please specify a command");
                app.ShowHelp();
                return 1;
            });

            return app.Execute(args);
        }

        private static int GenerateFontAtlas(string input, string outputImage)
        {
            using var font = Font.LoadFromFile(input);
            var atlas = font.RenderAtlas(48, Enumerable.Range(32, 126 - 32).Select(x => (char)x));
            var image = ImageHelper.Convert(atlas);
            image.SavePng(outputImage);
            return 0;
        }
    }
}
