using ImageDotNet;

namespace JEFTDotNet.CLI
{
    public static class ImageHelper
    {
        public static Image<Rgba32> Convert(FontAtlas atlas) =>
            new Image<Rgba32>(atlas.Image.Width, atlas.Image.Height, TransformPixels(atlas.Image, 0));

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
