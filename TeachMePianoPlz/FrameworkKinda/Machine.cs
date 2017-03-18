using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TeachMePianoPlz
{
    /// <summary>
    /// Pardon the mess. This is a version of a class that I've sort of evolved and copy/pasted from project to project...
    /// I tried to make it something a little more generic, since I knew I'd be releasing this code open-source, but that
    /// seemed to require all kinds of ugly templating that just wasn't worth. I'm sure I'll take another stab at it
    /// later, but for now... just... don't even worry about this stuff.
    /// </summary>
    abstract public class Machine: Game
    {
        private IGameState _current_state;
        private IGameState _next_state;
        private bool _quit = false;

        private GraphicsDeviceManager _graphics_device_manager;

        public GraphicsManager Graphics;

        public Machine()
        {
            Content.RootDirectory = "Content";

            // for reasons beyond mortal ken, new GraphicsDeviceManager MUST be called in the Game's constructor...
            _graphics_device_manager = new GraphicsDeviceManager(this);
        }

        public abstract IGameState InitialState();
        public abstract Dictionary<SoundID, SoundMeta> DefineSounds();
        public abstract Dictionary<GraphicsID, GraphicsMeta> DefineGraphics();

        protected override void Initialize()
        {
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            base.Initialize();

            _current_state = InitialState();
        }

        protected override void LoadContent()
        {
            Graphics = new GraphicsManager(
                _graphics_device_manager, // ... we'd rather just instantiate a new GraphicsDeviceManager here, but noOOOoooOO: MonoGame won't let you.
                Content,
                GraphicsDevice,
                640, 360, 2
            );

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
                Graphics.DrawScene(Color.DarkSlateGray, () => { _current_state.Draw(); });
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
