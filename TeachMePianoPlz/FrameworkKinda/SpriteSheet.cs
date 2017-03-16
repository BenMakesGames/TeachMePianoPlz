using System;
using Microsoft.Xna.Framework.Graphics;

namespace TeachMePianoPlz
{
    public class SpriteSheet
    {
        private int _sprite_width, _sprite_height;
        private int _sheet_columns, _sheet_rows;
        public Texture2D Texture { get; private set; }
        public BasicEffect Effect { get; private set; }

        public SpriteSheet(Texture2D spriteSheet, int spriteWidth, int spriteHeight, BasicEffect effect)
        {
            Texture = spriteSheet;
            Effect = effect;
            Effect.Texture = Texture;
            Effect.TextureEnabled = true;

            _sprite_width = spriteWidth;
            _sprite_height = spriteHeight;

            if (Texture.Width % _sprite_width != 0 || Texture.Height % _sprite_height != 0)
                throw new ArgumentException("Texture\'s width and height must be divisible by SpriteSheet's width and height, respectively, with no remainder");

            _sheet_columns = Texture.Width / _sprite_width;
            _sheet_rows = Texture.Height / _sprite_height;
        }

        public int SpriteWidth { get { return _sprite_width; } }
        public int SpriteHeight { get { return _sprite_height; } }

        public int Columns { get { return _sheet_columns; } }
        public int Rows { get { return _sheet_rows; } }
    }
}
