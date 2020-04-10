using System;
using System.Collections.Generic;
using System.Text;

namespace JEFTDotNet
{
    public class FontAtlas
    {
        private readonly Dictionary<char, FontAtlasCharacter> _characterData;

        public FontImage Image { get; }

        internal FontAtlas(FontImage image, Dictionary<char, FontAtlasCharacter> characterData)
        {
            Image = image;
            _characterData = characterData;
        }
    }
}
