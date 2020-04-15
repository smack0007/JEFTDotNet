using System;
using System.Collections.Generic;
using System.Text;

namespace JEFTDotNet
{
    public class FontAtlas
    {
        private readonly Dictionary<char, FontAtlasCharacter> _characters;

        public FontImage Image { get; }

        public int LineHeight { get; }

        public FontAtlasCharacter this[char ch] => _characters[ch];

        internal FontAtlas(FontImage image, Dictionary<char, FontAtlasCharacter> characters, int lineHeight)
        {
            Image = image;
            _characters = characters;
            LineHeight = lineHeight;
        }
    }
}
