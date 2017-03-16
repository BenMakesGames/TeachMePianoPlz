using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IMakeGames
{
    abstract public class Machine<GraphicsIDType, SoundIDType> : Game
    {
        private IGameState _current_state;
        private IGameState _next_state;
        private bool _quit = false;

        public GraphicsManager<GraphicsIDType> Graphics;

        public Machine()
        {
            Content.RootDirectory = "Content";
        }

        public abstract IGameState InitialState();
        public abstract Dictionary<SoundIDType, SoundMeta> DefineSounds();
        public abstract Dictionary<GraphicsIDType, GraphicsMeta> DefineGraphics();

        protected override void Initialize()
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            base.Initialize();

            Graphics = new GraphicsManager<GraphicsIDType>(
                new GraphicsDeviceManager(this),
                Content,
                GraphicsDevice
            );

            _current_state = InitialState();
        }

        protected override void LoadContent()
        {
            Graphics.Load(DefineGraphics());
            // load sound from DefineSounds()
        }

        protected override void Update(GameTime gameTime)
        {
            if(_quit)
            {
                Exit();
            }

            if(_next_state != null)
            {
                _current_state = _next_state;
                _next_state = null;

                _current_state.EnterState();
            }

            if (_current_state != null)
                _current_state.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_current_state != null)
                Graphics.DrawScene(Color.Black, () => { _current_state.Draw(); });
            else
                GraphicsDevice.Clear(Color.DarkSlateGray);
        }

        public void Quit()
        {
            _quit = true;
        }

        public void ChangeState(IGameState nextState)
        {
            if (_next_state != null) throw new Exception("A next state has already been readied (of type \"" + _next_state.GetType().ToString() + "\"). Cannot enqueue multiple next states.");

            _next_state = nextState;
        }
    }
}
