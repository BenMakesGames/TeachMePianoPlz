using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace IMakeGames
{
    public class GraphicsManager<GraphicsIDType>
    {
        public int Width { get; private set; } = 640;
        public int Height { get; private set; } = 360;
        public int Zoom { get; private set; } = 2;

        public void Resize(int width, int height, int zoom)
        {
            Width = width;
            Height = height;
            Zoom = zoom;

            _graphics.PreferredBackBufferWidth = Width * Zoom;
            _graphics.PreferredBackBufferHeight = Height * Zoom;
            _graphics.ApplyChanges();

            _projection_matrix = Matrix.CreateOrthographicOffCenter(0, Width, -Height, 0, 0, 1);

            _visibility_effect = new BasicEffect(_graphics_device)
            {
                Alpha = 1f,
                VertexColorEnabled = true,
                TextureEnabled = false,
                World = Matrix.Identity,
                View = _view_matrix,
                Projection = _projection_matrix,
            };

            _render_target = new RenderTarget2D(_graphics_device, Width, Height);
            _visibility_mask = new RenderTarget2D(_graphics_device, Width, Height);
        }

        public GraphicsManager(GraphicsDeviceManager graphicsDeviceManager, ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _graphics = graphicsDeviceManager;
            _content_manager = contentManager;
            _graphics_device = graphicsDevice;

            _view_matrix = new Matrix(
                1f, 0, 0, 0,
                0, -1f, 0, 0,
                0, 0, -1f, 0,
                0, 0, 0, 1f
            );

            _sprite_batch = new SpriteBatch(_graphics_device);

            _visibility_blender = new BlendState()
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Min,
            };

            Resize(Width, Height, Zoom);
        }

        public volatile static int LoadProgress = 0;

        public int LoadTotal()
        {
            return Enum.GetValues(typeof(GraphicsIDType)).Length;
        }
        
        public void Load(Dictionary<GraphicsIDType, GraphicsMeta> loadList)
        {
            foreach (GraphicsIDType spriteSheet in loadList.Keys)
            {
                LoadSprite(spriteSheet, loadList[spriteSheet]);
            }
        }

        private void LoadSprite(GraphicsIDType spriteSheetID, GraphicsMeta meta)
        {
            _sprite_sheets.Add(
                spriteSheetID,
                new SpriteSheet(
                    _content_manager.Load<Texture2D>("Graphics/" + spriteSheetID.ToString().Replace('_', '/')),
                    meta.SpriteWidth, meta.SpriteHeight,
                    new BasicEffect(_graphics_device)
                    {
                        World = Matrix.Identity,
                        View = _view_matrix,
                        Projection = _projection_matrix,
                    }
                )
            );

            LoadProgress++;
        }

        public void DrawSprite(Point center, Point size, GraphicsIDType spriteSheet, int spriteIndex)
        {
            DrawSprite(
                (center.X - size.X / 2),
                (center.Y - size.Y / 2),
                size.X,
                size.Y,
                spriteSheet,
                spriteIndex
            );
        }

        public void DrawSprite(Point center, Point size, Point camera, GraphicsIDType spriteSheet, int spriteIndex)
        {
            DrawSprite(
                (center.X - size.X / 2) - (int)camera.X,
                (center.Y - size.Y / 2) - (int)camera.Y,
                size.X,
                size.Y,
                spriteSheet,
                spriteIndex
            );
        }

        public void DrawSprite(double centerX, double centerY, Point size, Point camera, GraphicsIDType spriteSheet, int spriteIndex)
        {
            DrawSprite(
                (int)(centerX - size.X / 2) - camera.X,
                (int)(centerY - size.Y / 2) - camera.Y,
                size.X,
                size.Y,
                spriteSheet,
                spriteIndex
            );
        }

        public void DrawSprite(int x, int y, GraphicsIDType spriteSheet, int spriteIndex)
        {
            DrawSprite(
                x, y,
                _sprite_sheets[spriteSheet].SpriteWidth, _sprite_sheets[spriteSheet].SpriteHeight,
                spriteSheet,
                spriteIndex
            );
        }

        public void DrawSprite(int x, int y, int width, int height, GraphicsIDType spriteSheet, int spriteIndex)
        {
            int spriteX = spriteIndex % _sprite_sheets[spriteSheet].Columns;
            int spriteY = spriteIndex / _sprite_sheets[spriteSheet].Columns;

            float spriteXWidth = 1 / (float)_sprite_sheets[spriteSheet].Columns;
            float spriteYHeight = 1 / (float)_sprite_sheets[spriteSheet].Rows;

            float spriteXOffset = spriteX * spriteXWidth;
            float spriteYOffset = spriteY * spriteYHeight;

            foreach (EffectPass pass in _sprite_sheets[spriteSheet].Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphics_device.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionTexture[]
                {
                    new VertexPositionTexture(new Vector3(x, y, 0), new Vector2(spriteXOffset, spriteYOffset)),
                    new VertexPositionTexture(new Vector3(x + width, y, 0), new Vector2(spriteXOffset + spriteXWidth, spriteYOffset)),
                    new VertexPositionTexture(new Vector3(x, y + height, 0), new Vector2(spriteXOffset, spriteYOffset + spriteYHeight)),
                    new VertexPositionTexture(new Vector3(x + width, y + height, 0), new Vector2(spriteXOffset + spriteXWidth, spriteYOffset + spriteYHeight)),
                }, 0, 2, VertexPositionTexture.VertexDeclaration);
            }
        }

        public void DrawTiledSprite(int x, int y, int width, int height, GraphicsIDType spriteSheet, float offsetX, float offsetY)
        {
            int spriteWidth = _sprite_sheets[spriteSheet].SpriteWidth;
            int spriteHeight = _sprite_sheets[spriteSheet].SpriteHeight;

            foreach (EffectPass pass in _sprite_sheets[spriteSheet].Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphics_device.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionTexture[]
                {
                    new VertexPositionTexture(new Vector3(x, y, 0), new Vector2(offsetX, offsetY)),
                    new VertexPositionTexture(new Vector3(x + width, y, 0), new Vector2(offsetX + (float)width / spriteWidth, offsetY)),
                    new VertexPositionTexture(new Vector3(x, y + height, 0), new Vector2(offsetX, offsetY + (float)height / spriteHeight)),
                    new VertexPositionTexture(new Vector3(x + width, y + height, 0), new Vector2(offsetX + (float)width / spriteWidth, offsetY + (float)height / spriteHeight)),
                }, 0, 2, VertexPositionTexture.VertexDeclaration);
            }
        }
        
        public void DrawLine(int x1, int y1, int x2, int y2, Color color)
        {
            _graphics_device.DrawUserPrimitives(PrimitiveType.LineList, new VertexPositionColor[]
            {
                new VertexPositionColor(new Vector3(x1, y1, 0), color),
                new VertexPositionColor(new Vector3(x2, y2, 0), color),
            }, 0, 1);
        }

        public Point SpriteSize(GraphicsIDType spriteSheet)
        {
            return new Point(_sprite_sheets[spriteSheet].SpriteWidth, _sprite_sheets[spriteSheet].SpriteHeight);
        }

        public int SpriteCount(GraphicsIDType spriteSheet)
        {
            return _sprite_sheets[spriteSheet].Columns * _sprite_sheets[spriteSheet].Rows;
        }

        public void DrawScene(GraphicsIDType backgroundTile, Point backgroundOffset, Action drawScene)
        {
            // render scene

            _graphics_device.SetRenderTarget(_render_target);
            _graphics_device.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None
            };

            DrawTiledSprite(0, 0, Width, Height, backgroundTile, backgroundOffset.X, backgroundOffset.Y);

            drawScene();

            DrawBufferToScreen();
        }

        private void DrawBufferToScreen()
        { 
            _graphics_device.SetRenderTarget(null);

            _sprite_batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap);
            _sprite_batch.Draw(_render_target, new Rectangle(0, 0, Width * Zoom, Height * Zoom), Color.White);
            _sprite_batch.End();
        }

        public void DrawScene(Color backgroundColor, Action drawScene)
        {
            // render scene

            _graphics_device.SetRenderTarget(_render_target);
            _graphics_device.Clear(backgroundColor);
            _graphics_device.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None
            };

            drawScene();

            DrawBufferToScreen();
        }
        
        private Matrix _projection_matrix;
        private Matrix _view_matrix;

        private GraphicsDeviceManager _graphics;
        private BlendState _visibility_blender;
        private SpriteBatch _sprite_batch;
        private ContentManager _content_manager;
        private GraphicsDevice _graphics_device;
        private RenderTarget2D _render_target;
        private RenderTarget2D _visibility_mask;
        private BasicEffect _visibility_effect;

        private Dictionary<GraphicsIDType, SpriteSheet> _sprite_sheets = new Dictionary<GraphicsIDType, SpriteSheet>();
    }
}
