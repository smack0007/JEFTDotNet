namespace JEFTDotNet
{
    public class FontImage
    {
        private readonly byte[] _pixels;

        public int Width { get; }

        public int Height { get; }

        public int Length => _pixels.Length;

        public byte this[int i] => _pixels[i];

        public byte this[int x, int y] => _pixels[y * Width + x];

        internal FontImage(int width, int height, byte[] pixels)
        {
            Width = width;
            Height = height;
            _pixels = pixels;
        }
    }
}
