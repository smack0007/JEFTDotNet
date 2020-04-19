using System;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

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
                generateFontAtlasCmd.Description = "Generates a font atlas.";

                var input = generateFontAtlasCmd.Argument<string>("input", "The input TTF file.").IsRequired();
                var outputSize = generateFontAtlasCmd.Argument<int>("outputSize", "The size of the rendered font.").IsRequired();
                var outputImage = generateFontAtlasCmd.Argument<string>("outputImage", "The output image file.").IsRequired();
                var outputJson = generateFontAtlasCmd.Argument<string>("outputJson", "The output json file.").IsRequired();

                var startChar = generateFontAtlasCmd.Option<int>("--startChar", "The start character.", CommandOptionType.SingleValue);
                var endChar = generateFontAtlasCmd.Option<int>("--endChar", "The start character.", CommandOptionType.SingleValue);

                generateFontAtlasCmd.OnExecute(() =>
                {
                    if (input.Value == null)
                    {
                        Console.Error.WriteLine("Please specify an input file.");
                        return 1;
                    }

                    if (outputSize.Value == null)
                    {
                        Console.Error.WriteLine("Please specify the size for the rendered font.");
                        return 1;
                    }

                    if (outputImage.Value == null)
                    {
                        Console.Error.WriteLine("Please specify an output png file.");
                        return 1;
                    }

                    if (outputJson.Value == null)
                    {
                        Console.Error.WriteLine("Please specify an output json file.");
                        return 1;
                    }

                    return GenerateFontAtlas(
                        input.ParsedValue,
                        outputSize.ParsedValue,
                        outputImage.ParsedValue,
                        outputJson.ParsedValue,
                        startChar.HasValue() ? (int?)startChar.ParsedValue : null,
                        endChar.HasValue() ? (int?)endChar.ParsedValue : null);
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

        private static int GenerateFontAtlas(
            string input,
            int outputSize,
            string outputImage,
            string outputJsonFile,
            int? startChar,
            int? endChar)
        {
            if (startChar == null)
                startChar = 32;

            if (endChar == null)
                endChar = 126;

            using var font = Font.LoadFromFile(input);
            var atlas = font.RenderAtlas(outputSize, Enumerable.Range(startChar.Value, endChar.Value - startChar.Value).Select(x => (char)x));
            
            var image = ImageHelper.Convert(atlas);
            image.SavePng(outputImage);

            var json = JsonConvert.SerializeObject(atlas, Formatting.Indented);
            File.WriteAllText(outputJsonFile, json);

            return 0;
        }
    }
}
