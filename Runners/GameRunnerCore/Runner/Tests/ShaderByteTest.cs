﻿using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

namespace PixelVision8.CoreDesktop
{
    class ShaderByteTest : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Effect _quickDraw;
        private Texture2D _pixel, _screen, _colorPallete;
        private byte[] _data;
        int scale = 2;

        public ShaderByteTest()
        {
            _graphics = new GraphicsDeviceManager(this);
            // Content.RootDirectory = "Content";
            IsMouseVisible = true;
           

        }

        protected override void LoadContent()
        {
            PNGReader pngReader = null;
            var maskHex = "#ff00ff";

            // Load image
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "screenshot.png");
            var fileData = File.OpenRead(path);

            BinaryReader reader;
            using (reader = new BinaryReader(fileData))
            {
                pngReader = new PNGReader(reader.ReadBytes((int)reader.BaseStream.Length), maskHex);
            }

            _graphics.PreferredBackBufferWidth = pngReader.width * scale;
            _graphics.PreferredBackBufferHeight = pngReader.height * scale;
            _graphics.ApplyChanges();

            var imageParser = new ImageParser(pngReader);
            imageParser.CalculateSteps();

            while (imageParser.completed == false) imageParser.NextStep();

            // Create color palette texture
            var cachedColors = pngReader.colorPalette.ToArray();

            // Convert all of the pixels into color ids
            _data = pngReader.pixels.Select(Convert.ToByte).ToArray();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _colorPallete = new Texture2D(GraphicsDevice, 256, 1);

            var fullPalette = new Color[_colorPallete.Width];
            for (int i = 0; i < fullPalette.Length; i++) { fullPalette[i] = i < cachedColors.Length ? cachedColors[i] : cachedColors[0]; }

            _colorPallete.SetData(fullPalette);

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new Color[] { Color.White });

            // Create screen texture
            _screen = new Texture2D(GraphicsDevice, pngReader.width/4, pngReader.height);

            // Load shader
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Effects", "quick-draw-bytes.ogl.mgfxc");
            fileData = File.OpenRead(path);
            using (reader = new BinaryReader(fileData))
            {
                _quickDraw = new Effect(GraphicsDevice, reader.ReadBytes((int)reader.BaseStream.Length));
            }

            // Set palette total
            _quickDraw.Parameters["imageWidth"].SetValue((float)pngReader.width);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _screen.SetData(_data);

            _spriteBatch.Begin(SpriteSortMode.Immediate, SamplerState.PointClamp);
            _quickDraw.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.Textures[1] = _colorPallete;
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            GraphicsDevice.Textures[2] = _screen;
            GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

}
