using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TeachMePianoPlz
{
    public class Teacher: Machine
    {
        public override Dictionary<GraphicsID, GraphicsMeta> DefineGraphics()
        {
            return new Dictionary<GraphicsID, GraphicsMeta>()
            {
                { GraphicsID.Notes, new GraphicsMeta(160, 180) },
                { GraphicsID.Digits, new GraphicsMeta(28, 38) }
            };
        }

        public override Dictionary<SoundID, SoundMeta> DefineSounds()
        {
            return new Dictionary<SoundID, SoundMeta>();
        }

        public override IGameState InitialState()
        {
            return new Training();
        }

        public static Teacher Instance;

        protected override void Initialize()
        {
            Instance = this;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Graphics.Resize(1600, 900, 1);
        }
    }
}
