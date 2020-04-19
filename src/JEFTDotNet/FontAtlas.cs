using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JEFTDotNet
{
    public class FontAtlas
    {
        public FontImage Image { get; }

        public Dictionary<char, FontAtlasCharacter> Characters { get; }

        public int LineHeight { get; }

        internal FontAtlas(FontImage image, Dictionary<char, FontAtlasCharacter> characters, int lineHeight)
        {
            Image = image;
            Characters = characters;
            LineHeight = lineHeight;
        }
    }
}
