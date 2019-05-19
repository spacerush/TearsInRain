using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueSharp;
using RogueSharp.MapCreation;
using RogueSharp.Random;

namespace TearsInRainGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        KeyboardState oldState;
        private IMap _map;
        private Texture2D _floor;
        private Texture2D _wall;
        private Player _player;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here

            IMapCreationStrategy<Map> mapCreationStrategy = new RandomRoomsMapCreationStrategy<Map>(50, 30, 100, 7, 3);
            _map = Map.Create(mapCreationStrategy);


            base.Initialize();
        }

        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            _wall = Content.Load<Texture2D>("wall");
            _floor = Content.Load<Texture2D>("floor");

            Vector2 startingCell = GetRandomEmptyCell(50, 30);
            _player = new Player((int) startingCell.X, (int) startingCell.Y, 0.5f, Content.Load<Texture2D>("player"));
        }

        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }



        private Vector2 GetRandomEmptyCell(int mapWidth, int mapHeight) {
            IRandom random = new DotNetRandom();

            while (true) {
                int x = random.Next(mapWidth - 1);
                int y = random.Next(mapHeight - 1);
                if (_map.IsWalkable(x, y)) {
                    return new Vector2(x, y);
                }
            }
        }



        protected override void Update(GameTime gameTime) {
            KeyboardState newState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            System.Console.WriteLine(_player.X + " " + _player.Y);

            if (newState.IsKeyDown(Keys.NumPad1) && oldState.IsKeyUp(Keys.NumPad1))
                _player.basicMove(-1, 1, _map);

            if (newState.IsKeyDown(Keys.NumPad2) && oldState.IsKeyUp(Keys.NumPad2))
                _player.basicMove(0, 1, _map);

            if (newState.IsKeyDown(Keys.NumPad3) && oldState.IsKeyUp(Keys.NumPad3))
                _player.basicMove(1, 1, _map);

            if (newState.IsKeyDown(Keys.NumPad4) && oldState.IsKeyUp(Keys.NumPad4))
                _player.basicMove(-1, 0, _map);


            if (newState.IsKeyDown(Keys.NumPad6) && oldState.IsKeyUp(Keys.NumPad6))
                _player.basicMove(1, 0, _map);

            if (newState.IsKeyDown(Keys.NumPad7) && oldState.IsKeyUp(Keys.NumPad7))
                _player.basicMove(-1, -1, _map);

            if (newState.IsKeyDown(Keys.NumPad8) && oldState.IsKeyUp(Keys.NumPad8))
                _player.basicMove(0, -1, _map);

            if (newState.IsKeyDown(Keys.NumPad9) && oldState.IsKeyUp(Keys.NumPad9))
                _player.basicMove(1, -1, _map);



            // TODO: Add your update logic here

            base.Update(gameTime);

            oldState = newState;
        }
        
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            _player.Draw(spriteBatch);

            int sizeOfSprites = 32;
            float scale = 0.5f;

            foreach (Cell cell in _map.GetAllCells()) {
                var position = new Vector2(cell.X * sizeOfSprites * scale, cell.Y * sizeOfSprites * scale);

                if (cell.IsWalkable) { 
                    spriteBatch.Draw(_floor, position, null, null, null, 0.0f, new Vector2(scale, scale), Color.White, SpriteEffects.None, 0.5f);
                } else {
                    spriteBatch.Draw(_wall, position, null, null, null, 0.0f, new Vector2(scale, scale), Color.White, SpriteEffects.None, 0.5f);
                }
            }

            // spriteBatch.Draw(textureBall, ballPosition*32, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
