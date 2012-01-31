using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;

namespace SunshineSlashers
{
    public class Demo : Microsoft.Xna.Framework.Game
    {
        private static void Main()
        {
            using (Demo demo = new Demo())
            {
                demo.Run();
            }
        }
        private struct Light
        {
            public enum LightType
            {
                DirectionalLight,
                PointLight,
                SpotLight
            }
            public LightType Type;
            public Vector3 Direction;
            public Vector3 Position;
            public Color Ambient;
            public Color Diffuse;
            public Color Specular;
            public float SpotInnerConeRadians;
            public float SpotOuterConeRadians;
            public float Radius;
        }
        private struct Material
        {
            public Color Ambient;
            public Color Diffuse;
            public Color Emissive;
            public Color Specular;
            public float Shininess;
        }
        #region consts
        private const float CEILING_TILE_FACTOR = 8.0f;
        private const float FLOOR_PLANE_SIZE = 8192.0f;//16384.0f;//4096.0f;//1024.0f;
        private const float FLOOR_TILE_FACTOR = 64.0f;//128.0f;//32.0f;//16.0f;//8.0f;
        private const float FLOOR_CLIP_BOUNDS = FLOOR_PLANE_SIZE * 0.5f - 30.0f;
        private const float WALL_HEIGHT = 256.0f;//1024.0f;//256.0f;
        private const float WALL_TILE_FACTOR_X = 128.0f;//64.0f;//32.0f;//8.0f;
        private const float WALL_TILE_FACTOR_Y = 4.0f;//2.0f;


        private const float WEAPON_SCALE = 0.03f;
        private const float WEAPON_X_OFFSET = 0.45f;
        private const float WEAPON_Y_OFFSET = -0.75f;
        private const float WEAPON_Z_OFFSET = 1.65f;

        private const float CAMERA_FOVX = 85.0f;
        private const float CAMERA_ZNEAR = 1.0f;
        private const float CAMERA_ZFAR = FLOOR_PLANE_SIZE * 2.0f;
        private const float CAMERA_PLAYER_EYE_HEIGHT = 110.0f;
        private const float CAMERA_ACCELERATION_X = 800.0f;
        private const float CAMERA_ACCELERATION_Y = 800.0f;
        private const float CAMERA_ACCELERATION_Z = 800.0f;
        private const float CAMERA_VELOCITY_X = 200.0f;
        private const float CAMERA_VELOCITY_Y = 200.0f;
        private const float CAMERA_VELOCITY_Z = 200.0f;
        private const float CAMERA_RUNNING_MULTIPLIER = 2.0f;
        private const float CAMERA_RUNNING_JUMP_MULTIPLIER = 1.5f;
        private const float CAMERA_BOUNDS_PADDING = 30.0f;
        private const float CAMERA_BOUNDS_MIN_X = -FLOOR_PLANE_SIZE / 2.0f + CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MAX_X = FLOOR_PLANE_SIZE / 2.0f - CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MIN_Y = 0.0f;
        private const float CAMERA_BOUNDS_MAX_Y = WALL_HEIGHT;
        private const float CAMERA_BOUNDS_MIN_Z = -FLOOR_PLANE_SIZE / 2.0f + CAMERA_BOUNDS_PADDING;
        private const float CAMERA_BOUNDS_MAX_Z = FLOOR_PLANE_SIZE / 2.0f - CAMERA_BOUNDS_PADDING;

        #endregion
        #region menu stuff
            int CurrentMenuOption;
            public int CurrentScreenMode = 1;  //0 is main menu, 1 is ingame, 2 is joining games
            int TimeLastScreenModeChange;
            int TimeLastMenuChange;
        #endregion
        #region audio
        //AudioEngine audioEngine;
        //WaveBank waveBank;
        //SoundBank soundBank;

        //Song song1;
        //Song song2;
        //Song song3;
        Song soundtrack;
        //SongCollection mySongs;
        
        SoundEffect emptyClip;
        public SoundEffect cockGun;
        SoundEffect soundAK47;
        SoundEffect soundAK47reload;
        SoundEffect DeagleShot;
        SoundEffect ShottyShot;
        SoundEffect Headshot;
        SoundEffect StarGotten;
        SoundEffect Scream;
        SoundEffect Yippee;

        public bool RemotePlayerGotAHeadShot = false;
        public bool RemoteHeadShotThisFrame = false;

        //SoundEffect AmbienceNoise;

        
        #endregion
        #region textures
        private Texture2D nullTexture;
        private Texture2D brickColorMap;
        private Texture2D brickNormalMap;
        private Texture2D brickHeightMap;
        private Texture2D stoneColorMap;
        private Texture2D stoneNormalMap;
        private Texture2D stoneHeightMap;
        private Texture2D woodColorMap;
        private Texture2D woodNormalMap;
        private Texture2D woodHeightMap;
        private Texture2D Crosshair;
        private Texture2D SunTexture;
        private Texture2D HopeTexture;

        #endregion
        #region other vars
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private SpriteFont FontTF2Build;
        Vector2 FontPos;

        private Effect effect;
        private KeyboardState currentKeyboardState;
        private MouseState currentMouseState;
        private KeyboardState prevKeyboardState;
        private FirstPersonCamera camera;
        private NormalMappedRoom room;
        public Random Rand;
        public Model AK47;
        public Model Deagle;
        public Model Shotty;
        public Model Shroomer;
        public Model Leet;
        ArrayList Enemies;
        ShotList Shots;
        ShotList RemoteShots;
        ParticleList ParticleList;
        public Model UnitBox;
        public Model UnitSphere;
        private Light light;
        private Material material;
        public Being Player;
        private Color globalAmbient;
        private Vector2 scaleBias;
        private Vector2 fontPos;
        private int windowWidth;
        private int windowHeight;
        private int frames;
        private int framesPerSecond;
        private double accumElapsedTimeSec;
        private bool enableColorMap;
        private bool enableParallax;
        private bool displayHelp;
        public Model CarModel;
        public GameObject Car;
        public Model StarModel;
        public List<GameObject> Stars; //number of stars is 20
        public float PICKUP_DISTANCE = 80.0f;
        const int MAX_HOUSES = 10;
        public Model HouseModel;
        public GameObject[] House;
        const int MAX_WELLS = 5;
        public Model WellModel;
        public GameObject[] Well;
        const int MAX_CRATES = 10;
        public Model Crate1Model;
        public Model Crate2Model;
        public Model Crate3Model;
        public Model Crate4Model;
        public Model Crate5Model;
        public GameObject[] Crate;
        const int MAX_TREES = 10;
        public Model TreeModel;
        public GameObject[] Tree;
        const int MAX_MUSHROOMS = 10;
        public GameObject[] Mushroom;
        private Texture2D AK47HUDTexture;
        private Texture2D DeagleHUDTexture;
        private Texture2D ShotgunHUDTexture;
        private Texture2D AK47HUDSelectedTexture;
        private Texture2D DeagleHUDSelectedTexture;
        private Texture2D ShotgunHUDSelectedTexture;
        int weaponSelectionTime = 0;
        int controllerXpressed = 0;
        int controllerLBpressed = 0;
        int controllerRBpressed = 0;
        int rumbleTime = 0;
        private Texture2D BloodSplatterTexture;
        private Texture2D HealthBarOutlineTexture;
        private Texture2D HealthBarTexture;
        private Texture2D HappySunTexture;
        private Texture2D HappySunTransparentTexture;
        float sunCycle = 0;

        int HappyBarPower = 100;
        int RemoteHappyBarPower = 100;
        int HappyBarMode = 0;   // 0 is not activated (charging or full), 1 is decreasing because it has been activated
        int RemoteHappyBarMode = 0;   // 0 is not activated (charging or full), 1 is decreasing because it has been activated

        public int currentStarSelected = 0;

        public List<GameObject> AmmoCrates; //number of ammo is 10
        public int currentAmmoSelected = 0;

        public bool gotHeadshot = false; 

        int CurrentMapSelection = 0;
        int CurrentCharacterSelection = 0;
        
        #endregion
        #region Network Variables
        public List<Being> RemotePlayers;

        const int maxGamers = 2;
        const int maxLocalGamers = 1;

        NetworkSession networkSession;

        AvailableNetworkSessionCollection availableSessions;

        string hostTag;

        public bool newAmmo = false;

        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();

        string errorMessage;

        public bool currentDeathState = false;
        public bool previousDeathState = false;

        public bool looked = false;

        public enum PACKET_ID
        {
            INITIAL_DATA, ENEMY_DATA, STAR_DATA, PARTICLE_DATA, PLAYER_DATA, 
            REMOTE_DATA, CRATE_DATA
        };

        public bool networkGame = false;
        #endregion
        public Demo()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            camera = new FirstPersonCamera(this);
            Components.Add(camera);

            Components.Add(new GamerServicesComponent(this));

            Window.Title = "Sunshine Slashers";
            IsFixedTimeStep = false;
        }
        private void FireGun(GameTime gameTime)
        {
            if (Player.Reloading)
                return;
            #region Get shots
            int shots = 0;
            Gun CurrentGun = Player.GetCurrentGun();
            if (CurrentGun.RoundsInClip <= 0)
            {
                ReloadGun();
                emptyClip.Play();
                return;
            }
            switch (Player.CurrentGun)
            {
                case (int)Guns.Deagle:
                    if (Environment.TickCount - Player.TimeOfLastShot > 400)
                    {
                        Player.TimeOfLastShot = Environment.TickCount;
                        shots = 1;
                    }
                    break;
                case (int)Guns.AK47:
                    if (Environment.TickCount - Player.TimeOfLastShot > 100)
                    {
                        Player.TimeOfLastShot = Environment.TickCount;
                        shots = 1;
                    }
                    break;
                case (int)Guns.Panzerschreck:
                    if (Environment.TickCount - Player.TimeOfLastShot > 2500)
                    {
                        Player.TimeOfLastShot = Environment.TickCount;
                        shots = 1;
                    }
                    break;
                default:
                    if (Environment.TickCount - Player.TimeOfLastShot > 1000)
                    {
                        Player.TimeOfLastShot = Environment.TickCount;
                        shots = 1;
                    }
                    break;
            }
            if (shots == 0)
                return;
            #endregion
            #region Shoot gun with aforementioned number of shots
            while (shots > 0)
            {
                Vector3 Direction = camera.ViewDirection;
                foreach (Gun gun in Player.GunList)
                    if (gun.GunCode == Player.CurrentGun)
                    {
                        if(gun.RoundsInClip>0)
                            --gun.RoundsInClip;
                        else
                            try
                            {
                                ReloadGun();
                                emptyClip.Play();
                            }
                            catch (Exception e)
                            {
                                e.ToString();
                            }
                    }
                switch (Player.CurrentGun)
                {
                    case (int)Guns.AK47:
                        if (Rand.Next() < int.MaxValue / 6)
                        {
                            Direction.X += (float)Rand.Next() / 24073741824.0f;
                            Direction.Y += (float)Rand.Next() / 24073741824.0f;
                            Direction.Z += (float)Rand.Next() / 24073741824.0f;
                        }
                        Direction.Normalize();
                        Shots.AddShot(new Ray(camera.Position, Direction), 30, Player);
                        try
                        {
                            soundAK47.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        break;
                    case (int)Guns.Deagle:
                        Shots.AddShot(new Ray(camera.Position, Direction), 50, Player);
                        try
                        {
                            DeagleShot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        break;
                    case (int)Guns.Shotty:
                        try
                        {
                            ShottyShot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        for(int i=0; i<12; ++i)
                        {
                            Direction.X += (float)Rand.Next() / 25073741824.0f;
                            Direction.Y += (float)Rand.Next() / 25073741824.0f;
                            Direction.Z += (float)Rand.Next() / 25073741824.0f;
                            Direction.Normalize();
                            Shots.AddShot(new Ray(camera.Position, Direction), 22, Player);
                            Direction = camera.ViewDirection;
                        }
                        break;
                }
                --shots;
            }
            #endregion
            #region Recoil effect
            float HeadingChange = (float)Rand.Next() / 4294967296.0f; //get random num between 0-1
            float PitchChange = (float)Rand.Next() / 4294967296.0f;
            if (Rand.Next() % 2 == 0)
                HeadingChange *= -1;
            if (Rand.Next() % 2 == 0)
                PitchChange *= -1;
            switch (Player.CurrentGun)
            {
                case (int)Guns.Deagle:
                    {
                        Player.camera.Rotate(HeadingChange, PitchChange);
                        GamePad.SetVibration(PlayerIndex.One, 0.3f, 0.3f);
                        rumbleTime = 10;
                         
                        break;
                    }
                case (int)Guns.AK47:
                    {
                        GamePad.SetVibration(PlayerIndex.One, 0.5f, 0.5f);
                        Player.camera.Rotate(3f * HeadingChange, 3f * PitchChange);
                        rumbleTime = 10;
                        break;
                    }
                case (int)Guns.Shotty:
                    {
                        GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
                        Player.camera.Rotate(7f * HeadingChange, 7f * PitchChange);
                        rumbleTime = 10;
                        break;
                    }
            }
            #endregion
        }
        public BoundingBox GetModelBB(Model model)
        {
            Vector3 Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int stride = part.VertexStride;
                    int numberv = part.NumVertices;
                    VertexDeclaration test1 = part.VertexDeclaration;      // not used for now
                    byte[] data = new byte[stride * numberv];
                    System.Diagnostics.Debug.WriteLine("stride=" + stride.ToString() +
                                                                 "numv  =" + numberv.ToString());

                    mesh.VertexBuffer.GetData<byte>(data);

                    for (int ndx = 0; ndx < data.Length; ndx += stride)
                    {
                        float floatvaluex = BitConverter.ToSingle(data, ndx);
                        float floatvaluey = BitConverter.ToSingle(data, ndx + 4);
                        float floatvaluez = BitConverter.ToSingle(data, ndx + 8);
                        if (floatvaluex < Min.X) Min.X = floatvaluex;
                        if (floatvaluex > Max.X) Max.X = floatvaluex;
                        if (floatvaluey < Min.Y) Min.Y = floatvaluey;
                        if (floatvaluey > Max.Y) Max.Y = floatvaluey;
                        if (floatvaluez < Min.Z) Min.Z = floatvaluez;
                        if (floatvaluez > Max.Z) Max.Z = floatvaluez;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(" Min=" + Min.ToString());
            System.Diagnostics.Debug.WriteLine(" Max=" + Max.ToString());
            BoundingBox boundingbox = new BoundingBox(Min, Max);
            return boundingbox;
        }
        private void ReloadGun()
        {
            Player.TimeSinceBeginReloading = Environment.TickCount;
            Player.Reloading = true;
            GamePad.SetVibration(PlayerIndex.One, 0.1f, 0.2f);
            rumbleTime = 10;
            try
            {
                soundAK47reload.Play();
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }
        protected override void Initialize()
        {
            try
            {
                base.Initialize();
            }
            catch (Exception e)
            {
                Components.RemoveAt(1);
                base.Initialize();
            }
            Rand = new Random();

            TimeLastMenuChange = TimeLastScreenModeChange = Environment.TickCount;
            CurrentMenuOption = 0;// lastButtonPressed = 0;
            CurrentScreenMode = 0;

#if XBOX360
            windowWidth = 1280;
            windowHeight = 720;
#else
            windowWidth = (int)(GraphicsDevice.DisplayMode.Width / 1.2);
            windowHeight = (int)(GraphicsDevice.DisplayMode.Height / 1.2);
#endif

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            // Initially enable the diffuse color map texture.
            enableColorMap = true;

            // Initially enable parallax mapping.
            enableParallax = true;

            // Initial position for text rendering.
            fontPos = new Vector2(1.0f, 1.0f);

            // Parallax mapping height scale and bias values.
            scaleBias = new Vector2(0.04f, -0.03f);

            // Initialize point lighting for the scene.
            globalAmbient = new Color(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            light.Type = Light.LightType.PointLight;
            light.Direction = Vector3.Zero;
            light.Position = new Vector3(0.0f, (0.85f * WALL_HEIGHT), 0.0f);
            light.Ambient = Color.White;
            light.Diffuse = Color.White;
            light.Specular = Color.White;
            light.SpotInnerConeRadians = MathHelper.ToRadians(40.0f);
            light.SpotOuterConeRadians = MathHelper.ToRadians(70.0f);
            light.Radius = Math.Max(FLOOR_PLANE_SIZE, WALL_HEIGHT);

            // Initialize material settings. Just a plain lambert material.
            material.Ambient = new Color(new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            material.Diffuse = new Color(new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            material.Emissive = Color.Black;
            material.Specular = Color.Black;
            material.Shininess = 0.0f;

            // Create the room.
            room = new NormalMappedRoom(GraphicsDevice,
                    FLOOR_PLANE_SIZE, WALL_HEIGHT, FLOOR_TILE_FACTOR,
                    CEILING_TILE_FACTOR, WALL_TILE_FACTOR_X, WALL_TILE_FACTOR_Y);

            // Setup the camera.
            camera.EyeHeightStanding = CAMERA_PLAYER_EYE_HEIGHT;
            camera.Acceleration = new Vector3(
                CAMERA_ACCELERATION_X,
                CAMERA_ACCELERATION_Y,
                CAMERA_ACCELERATION_Z);
            camera.VelocityWalking = new Vector3(
                CAMERA_VELOCITY_X,
                CAMERA_VELOCITY_Y,
                CAMERA_VELOCITY_Z);
            camera.VelocityRunning = new Vector3(
                camera.VelocityWalking.X * CAMERA_RUNNING_MULTIPLIER,
                camera.VelocityWalking.Y * CAMERA_RUNNING_JUMP_MULTIPLIER,
                camera.VelocityWalking.Z * CAMERA_RUNNING_MULTIPLIER);
            camera.Perspective(
                CAMERA_FOVX,
                (float)windowWidth / (float)windowHeight,
                CAMERA_ZNEAR, CAMERA_ZFAR);

            ParticleList = new ParticleList();
            Shots  = new ShotList();
            RemoteShots = new ShotList();
            Enemies = new ArrayList();
            RemotePlayers = new List<Being>();
            for (int i = 0; i < 4; ++i)
            {
                Being NewGuy = new Being(this);
                NewGuy.IsHuman = false;
                NewGuy.camera.Position = new Vector3(-200+(i%2)*400, 115, i>1?200:-200);
                Enemies.Add(NewGuy);
            }
            for (int i = 0; i < 4; ++i)
            {
                Being NewGuy = new Being(this);
                NewGuy.IsHuman = false;
                NewGuy.camera.Position = new Vector3(Rand.Next() % (CAMERA_BOUNDS_MAX_X - CAMERA_BOUNDS_MIN_X) + CAMERA_BOUNDS_MIN_X,
                    115,
                    Rand.Next() % (CAMERA_BOUNDS_MAX_Z - CAMERA_BOUNDS_MIN_Z) + CAMERA_BOUNDS_MIN_Z
                    );
                NewGuy.camera.Rotate(Rand.Next() % 360, 0.0f);
                Enemies.Add(NewGuy);
            }

            // Get the initial keyboard state.
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
        }
        protected override void LoadContent()
        {
            //
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"Fonts\DemoFont");
            FontTF2Build = Content.Load<SpriteFont>(@"Fonts\TF2Build");
            
            effect = Content.Load<Effect>(@"Shaders\parallax_normal_mapping");
            effect.CurrentTechnique = effect.Techniques["ParallaxNormalMappingPointLighting"];

            brickColorMap = Content.Load<Texture2D>(@"Textures\brick_color_map");
            brickNormalMap = Content.Load<Texture2D>(@"Textures\brick_normal_map");
            brickHeightMap = Content.Load<Texture2D>(@"Textures\brick_height_map");
            stoneColorMap = Content.Load<Texture2D>(@"Textures\stone_color_map");
            stoneNormalMap = Content.Load<Texture2D>(@"Textures\stone_normal_map");
            stoneHeightMap = Content.Load<Texture2D>(@"Textures\stone_height_map");
            woodColorMap = Content.Load<Texture2D>(@"Textures\wood_color_map");
            woodNormalMap = Content.Load<Texture2D>(@"Textures\wood_normal_map");
            woodHeightMap = Content.Load<Texture2D>(@"Textures\wood_height_map");
            Crosshair = Content.Load<Texture2D>(@"Textures\sniperKustoms");

            Leet = Content.Load<Model>(@"Models\leet\leet");
            Shroomer = Content.Load<Model>(@"Models\shroomer\shroomer");
            AK47 = Content.Load<Model>(@"Models\ak47\ak47");
            Deagle = Content.Load<Model>(@"Models\deagle\deagle");
            Shotty = Content.Load<Model>(@"Models\benelli\benellimodel");
            UnitBox = Content.Load<Model>(@"Models\UnitBox");
            UnitSphere = Content.Load<Model>(@"Models\UnitSphere");
            CarModel = Content.Load<Model>(@"Models\car\car");
            HouseModel = Content.Load<Model>(@"Models\house\house");
            WellModel = Content.Load<Model>(@"Models\well\well");
            Crate1Model = Content.Load<Model>(@"Models\crates\Crate1");
            Crate2Model = Content.Load<Model>(@"Models\crates\Crate2");
            Crate3Model = Content.Load<Model>(@"Models\crates\Crate3");
            Crate4Model = Content.Load<Model>(@"Models\crates\Crate4");
            Crate5Model = Content.Load<Model>(@"Models\crates\Crate5");
            StarModel = Content.Load<Model>(@"Models\star\star");
            TreeModel = Content.Load<Model>(@"Models\tree\tree");

            Stars = new List<GameObject>();
            //for (int i = 0; i < 50; i++)
            //{
            
            //}
            Stars.Add(new GameObject(this, StarModel, new Vector3(150, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(-250, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(350, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(-450, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(-550, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(650, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(-750, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(850, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(-950, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(1050, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, 150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, -150), 0.25f));
            Stars.Add(new GameObject(this, StarModel, new Vector3(5000, 85.0f, 150), 0.25f));

            AmmoCrates = new List<GameObject>();
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(1000, 14.0f, 1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(5000, 14.0f, -1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(5000, 14.0f, 1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(200, 14.0f, -1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(5000, 14.0f, -1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(5000, 14.0f, 1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(5000, 14.0f, -1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(850, 14.0f, 1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(5000, 14.0f, -1150), 10.0f));
            AmmoCrates.Add(new GameObject(this, Crate2Model, new Vector3(5000, 14.0f, 1150), 10.0f));

			AK47HUDTexture = Content.Load<Texture2D>("Artwork\\ak47_icon");
            DeagleHUDTexture = Content.Load<Texture2D>("Artwork\\deagle_icon");
            ShotgunHUDTexture = Content.Load<Texture2D>("Artwork\\shotgun_icon");
            AK47HUDSelectedTexture = Content.Load<Texture2D>("Artwork\\ak47_icon_glow");
            DeagleHUDSelectedTexture = Content.Load<Texture2D>("Artwork\\deagle_icon_glow");
            ShotgunHUDSelectedTexture = Content.Load<Texture2D>("Artwork\\shotgun_icon_glow");
            BloodSplatterTexture = Content.Load<Texture2D>("Artwork\\blood_splatter2");
            HealthBarOutlineTexture = Content.Load<Texture2D>("Artwork\\healthbaroutline");
            HealthBarTexture = Content.Load<Texture2D>(@"Textures\bloodBar");
            HappySunTexture = Content.Load<Texture2D>("Artwork\\sun 2");
            HappySunTransparentTexture = Content.Load<Texture2D>("Artwork\\sun transparent");
            SunTexture = Content.Load<Texture2D>("Artwork\\sun 2");
            HopeTexture = Content.Load<Texture2D>("Artwork\\hope 3 blur");
            Player = new Being(this); Player.IsHuman = true;

            Car = new GameObject(this, CarModel, new Vector3(500, 5.0f, 500), 50);
            House = new GameObject[MAX_HOUSES];
            for (int i = 0; i < MAX_HOUSES; i++)
            {
                if (i == 0) House[0] = new GameObject(this, HouseModel, new Vector3(-3000, 10.0f, -1000), 50);
                if (i == 1) House[1] = new GameObject(this, HouseModel, new Vector3(-1500, 10.0f, -1000), 50);
                if (i == 2) House[2] = new GameObject(this, HouseModel, new Vector3(0, 10.0f, -1000), 50);
                if (i == 3) House[3] = new GameObject(this, HouseModel, new Vector3(1500, 10.0f, -1000), 50);
                if (i == 4) House[4] = new GameObject(this, HouseModel, new Vector3(3000, 10.0f, -1000), 50);

                if (i == 5) House[5] = new GameObject(this, HouseModel, new Vector3(-3000, 10.0f, 1000), 50);
                if (i == 6) House[6] = new GameObject(this, HouseModel, new Vector3(-1500, 10.0f, 1000), 50);
                if (i == 7) House[7] = new GameObject(this, HouseModel, new Vector3(0, 10.0f, 1000), 50);
                if (i == 8) House[8] = new GameObject(this, HouseModel, new Vector3(1500, 10.0f, 1000), 50);
                if (i == 9) House[9] = new GameObject(this, HouseModel, new Vector3(3000, 10.0f, 1000), 50);
            }


            Well = new GameObject[MAX_WELLS];
            for (int i = 0; i < MAX_WELLS; i++)
            {
                if (i == 0) Well[0] = new GameObject(this, WellModel, new Vector3(-3000, 5.0f, 0), 50);
                if (i == 1) Well[1] = new GameObject(this, WellModel, new Vector3(-1500, 5.0f, 0), 50);
                if (i == 2) Well[2] = new GameObject(this, WellModel, new Vector3(-0, 5.0f, 0), 50);
                if (i == 3) Well[3] = new GameObject(this, WellModel, new Vector3(1500, 5.0f, 0), 50);
                if (i == 4) Well[4] = new GameObject(this, WellModel, new Vector3(3000, 5.0f, 0), 50);

            }

            Crate = new GameObject[MAX_CRATES];
            for (int i = 0; i < MAX_CRATES; i++)
            {
                if (i == 0) Crate[0] = new GameObject(this, Crate2Model, new Vector3(-800, 62.0f, -400), 10);
                if (i == 1) Crate[1] = new GameObject(this, Crate2Model, new Vector3(-600, 62.0f, -400), 10);
                if (i == 2) Crate[2] = new GameObject(this, Crate2Model, new Vector3(-400, 62.0f, -400), 10);
                if (i == 3) Crate[3] = new GameObject(this, Crate2Model, new Vector3(-200, 62.0f, -400), 10);
                if (i == 4) Crate[4] = new GameObject(this, Crate2Model, new Vector3(0, 62.0f, -400), 10);
                if (i == 5) Crate[5] = new GameObject(this, Crate2Model, new Vector3(-800, 62.0f, -400), 10);
                if (i == 6) Crate[6] = new GameObject(this, Crate2Model, new Vector3(-600, 62.0f, -400), 10);
                if (i == 7) Crate[7] = new GameObject(this, Crate2Model, new Vector3(-400, 62.0f, -400), 10);
                if (i == 8) Crate[8] = new GameObject(this, Crate2Model, new Vector3(-200, 62.0f, -400), 10);
                if (i == 9) Crate[9] = new GameObject(this, Crate2Model, new Vector3(0, 62.0f, -400), 10);
            }

            Tree = new GameObject[MAX_TREES];
            for (int i = 0; i < MAX_TREES; i++)
            {
                if (i == 0) Tree[0] = new GameObject(this, TreeModel, new Vector3(2000, 5.0f, 2000), 50);
                if (i == 1) Tree[1] = new GameObject(this, TreeModel, new Vector3(2000, 5.0f, -2000), 50);
                if (i == 2) Tree[2] = new GameObject(this, TreeModel, new Vector3(-0, 5.0f, 2000), 50);
                if (i == 3) Tree[3] = new GameObject(this, TreeModel, new Vector3(-2000, 5.0f, 2000), 50);
                if (i == 4) Tree[4] = new GameObject(this, TreeModel, new Vector3(-2000, 5.0f, -2000), 50);
                if (i == 5) Tree[5] = new GameObject(this, TreeModel, new Vector3(3000, 5.0f, -200), 50);
                if (i == 6) Tree[6] = new GameObject(this, TreeModel, new Vector3(-3000, 5.0f, -200), 50);
                if (i == 7) Tree[7] = new GameObject(this, TreeModel, new Vector3(1000, 5.0f, 2500), 50);
                if (i == 8) Tree[8] = new GameObject(this, TreeModel, new Vector3(2500, 5.0f, 2500), 50);
                if (i == 9) Tree[9] = new GameObject(this, TreeModel, new Vector3(2600, 5.0f, 1900), 50);
                if (i == 10) Tree[10] = new GameObject(this, TreeModel, new Vector3(0, 5.0f, 2000), 50);
                if (i == 11) Tree[11] = new GameObject(this, TreeModel, new Vector3(-3000, 5.0f, 200), 50);
                if (i == 12) Tree[12] = new GameObject(this, TreeModel, new Vector3(-1000, 5.0f, -2500), 50);
                if (i == 13) Tree[13] = new GameObject(this, TreeModel, new Vector3(-2500, 5.0f, -2500), 50);
                if (i == 14) Tree[14] = new GameObject(this, TreeModel, new Vector3(-2600, 5.0f, -1900), 50);

            }


            Mushroom = new GameObject[MAX_MUSHROOMS];
            for (int i = 0; i < MAX_MUSHROOMS; i++)
            {
                if (i == 0) Mushroom[0] = new GameObject(this, Shroomer, new Vector3(-1500, 50.0f, 2000), 0.1f);
                if (i == 1) Mushroom[1] = new GameObject(this, Shroomer, new Vector3(200, 50.0f, 2000), 0.1f);
                if (i == 2) Mushroom[2] = new GameObject(this, Shroomer, new Vector3(2000, 50.0f, -350), 0.1f);
                if (i == 3) Mushroom[3] = new GameObject(this, Shroomer, new Vector3(100, 50.0f, -1000), 0.1f);
                if (i == 4) Mushroom[4] = new GameObject(this, Shroomer, new Vector3(500, 50.0f, -900), 0.1f);
                if (i == 5) Mushroom[5] = new GameObject(this, Shroomer, new Vector3(-1000, 50.0f, 1600), 0.1f);
                if (i == 6) Mushroom[6] = new GameObject(this, Shroomer, new Vector3(3000, 50.0f, 2000), 0.1f);
                if (i == 7) Mushroom[7] = new GameObject(this, Shroomer, new Vector3(100, 50.0f, 2500), 0.1f);
                if (i == 8) Mushroom[8] = new GameObject(this, Shroomer, new Vector3(250, 50.0f, -2500), 0.1f);
                if (i == 9) Mushroom[9] = new GameObject(this, Shroomer, new Vector3(1900, 50.0f, 2600), 0.1f);
            }

            // Create an empty white texture. This will be bound to the
            // colorMapTexture shader parameter when the user wants to
            // disable the color map texture. This trick will allow the
            // same shader to be used for when textures are enabled and
            // disabled.

            nullTexture = new Texture2D(GraphicsDevice, 1, 1, 0,
                            TextureUsage.None, SurfaceFormat.Color);

            Color[] pixels = { Color.White };

            nullTexture.SetData(pixels);


            //sound load content
            //song1 = Content.Load<Song>("Audio\\Music\\A Walk in the sun");
            //song2 = Content.Load<Song>("Audio\\Music\\dingdong");
            //song3 = Content.Load<Song>("Audio\\Music\\Morning Sunrise");
            soundtrack = Content.Load<Song>("Audio\\Music\\Soundtrack");
            cockGun = Content.Load<SoundEffect>("Audio\\Sounds\\cockgun");
            emptyClip = Content.Load<SoundEffect>("Audio\\Sounds\\emptyClip");
            soundAK47 = Content.Load<SoundEffect>("Audio\\Sounds\\ak47-1");
            ShottyShot = Content.Load<SoundEffect>("Audio\\Sounds\\shottyShotBIG");
            DeagleShot = Content.Load<SoundEffect>("Audio\\Sounds\\deagleShot");
            soundAK47reload = Content.Load<SoundEffect>("Audio\\Sounds\\ak47_boltpull");
            StarGotten = Content.Load<SoundEffect>("Audio\\Sounds\\sparkle");
            Scream = Content.Load<SoundEffect>("Audio\\Sounds\\screams male 03");
            Headshot = Content.Load<SoundEffect>("Audio\\Sounds\\headshot");
            Yippee = Content.Load<SoundEffect>("Audio\\Sounds\\yippee");
            //AmbienceNoise = Content.Load<SoundEffect>("Audio\\Sounds\\neighborhood");
            //MediaPlayer.Play(song1);

            try
            {
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(soundtrack);
                
                
                //MediaPlayer.Play(song1);
                //AmbienceNoise.Play();
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }
        protected override void UnloadContent()
        {
        }
        private void HappyModeStuff()
        {
            if (HappyBarMode == 1 && HappyBarPower > 0) HappyBarPower--;
            if (HappyBarMode == 1 && HappyBarPower <= 0) HappyBarMode = 0;
            if (RemoteHappyBarMode == 1 && RemoteHappyBarPower > 0) RemoteHappyBarPower--;
            if (RemoteHappyBarMode == 1 && RemoteHappyBarPower <= 0) RemoteHappyBarPower = 0;
        }
        private void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            graphics.IsFullScreen = !graphics.IsFullScreen;

            if (graphics.IsFullScreen)
            {
                newWidth = GraphicsDevice.DisplayMode.Width;
                newHeight = GraphicsDevice.DisplayMode.Height;
                windowWidth = (int)(GraphicsDevice.DisplayMode.Width);
                windowHeight = (int)(GraphicsDevice.DisplayMode.Height);
            }
            else
            {
                newWidth = windowWidth;
                newHeight = windowHeight;
                windowWidth = (int)(GraphicsDevice.DisplayMode.Width / 1.2f);
                windowHeight = (int)(GraphicsDevice.DisplayMode.Height / 1.2f);
            }

            graphics.PreferredBackBufferWidth = newWidth;
            graphics.PreferredBackBufferHeight = newHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            float aspectRatio = (float)newWidth / (float)newHeight;

            camera.Perspective(CAMERA_FOVX, aspectRatio, CAMERA_ZNEAR, CAMERA_ZFAR);
        }
        private void PerformCameraCollisionDetection(FirstPersonCamera camera)
        {
            Vector3 newPos = camera.Position;

            if (camera.Position.X > CAMERA_BOUNDS_MAX_X)
                newPos.X = CAMERA_BOUNDS_MAX_X;

            if (camera.Position.X < CAMERA_BOUNDS_MIN_X)
                newPos.X = CAMERA_BOUNDS_MIN_X;

            if (camera.Position.Y > CAMERA_BOUNDS_MAX_Y)
                newPos.Y = CAMERA_BOUNDS_MAX_Y;

            if (camera.Position.Y < CAMERA_BOUNDS_MIN_Y)
                newPos.Y = CAMERA_BOUNDS_MIN_Y;

            if (camera.Position.Z > CAMERA_BOUNDS_MAX_Z)
                newPos.Z = CAMERA_BOUNDS_MAX_Z;

            if (camera.Position.Z < CAMERA_BOUNDS_MIN_Z)
                newPos.Z = CAMERA_BOUNDS_MIN_Z;

            camera.Position = newPos;
        }
        private bool PerformCameraCollisionDetection(Ray ray)
        {
            if ((ray.Position.X > CAMERA_BOUNDS_MAX_X) ||
                (ray.Position.X < CAMERA_BOUNDS_MIN_X) ||
                (ray.Position.Y > CAMERA_BOUNDS_MAX_Y) ||
                (ray.Position.Y < CAMERA_BOUNDS_MIN_Y) ||
                (ray.Position.Z > CAMERA_BOUNDS_MAX_Z) ||
                (ray.Position.Z < CAMERA_BOUNDS_MIN_Z))
                return true;
            return false;
        }
        private bool KeyJustPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }
        public bool IsCollision(Ray ray)
        {
            const float distanceMod = 15.0f;
            for(int i=0; i<10; ++i)
                if (House[i].IsCollision(ray) < distanceMod && House[i].IsCollision(ray) != 0)
                    return true;
            for(int i=0; i<5; ++i)
                if (Crate[i].IsCollision(ray) < distanceMod && Crate[i].IsCollision(ray) != 0)
                    return true;
            for(int i=0; i<5; ++i)
                if (Well[i].IsCollision(ray) < distanceMod && Well[i].IsCollision(ray) != 0)
                    return true;
            return PerformCameraCollisionDetection(ray);
        }
        private void ProcessKeyboard(GameTime gameTime)
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
#if (WINDOWS)
            currentMouseState = Mouse.GetState();
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                FireGun(gameTime);
            }
#endif

            if (KeyJustPressed(Keys.Escape))
                if (CurrentScreenMode == 1)
                {
                    CurrentScreenMode = 3;
                    TimeLastScreenModeChange = Environment.TickCount;
                }
            //if (CurrentScreenMode == 1) CurrentScreenMode = 3;
            //if (CurrentScreenMode == 1) CurrentScreenMode = 3;
            //this.Exit();

            if (KeyJustPressed(Keys.H))
                displayHelp = !displayHelp;

            if (KeyJustPressed(Keys.M))
                camera.EnableMouseSmoothing = !camera.EnableMouseSmoothing;

            if (KeyJustPressed(Keys.P))
                enableParallax = !enableParallax;

            if (KeyJustPressed(Keys.T))
                enableColorMap = !enableColorMap;
            if (KeyJustPressed(Keys.E))
            {
                if (HappyBarPower == 100 && HappyBarMode == 0)
                {
                    HappyBarMode = 1;
                    try
                    {
                        Yippee.Play();
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                }
            }
            if (KeyJustPressed(Keys.R))
            {
                ReloadGun();
            }
            if (KeyJustPressed(Keys.D1))
            {
                weaponSelectionTime = 30;
                Player.SwitchWeapon((int)Guns.Deagle);
            }
            if (KeyJustPressed(Keys.D2))
            {
                weaponSelectionTime = 30;
                Player.SwitchWeapon((int)Guns.AK47);
            }
            if (KeyJustPressed(Keys.D3))
            {
                weaponSelectionTime = 30;
                Player.SwitchWeapon((int)Guns.Shotty);
            }

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) ||
                currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }

            if (KeyJustPressed(Keys.Add))
            {
                camera.RotationSpeed += 0.01f;

                if (camera.RotationSpeed > 1.0f)
                    camera.RotationSpeed = 1.0f;
            }

            if (KeyJustPressed(Keys.Subtract))
            {
                camera.RotationSpeed -= 0.01f;

                if (camera.RotationSpeed <= 0.0f)
                    camera.RotationSpeed = 0.01f;
            }

        }
        private void ProcessController(GameTime gameTime)
        {
            if (rumbleTime > 0)
            {
                rumbleTime--;
            }
            else
            {
                GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
            {

                switch (CurrentScreenMode)
                {
                    case 0:
                        //CurrentScreenMode = 1;
                        break;
                    case 1:
                        CurrentScreenMode = 3;
                        TimeLastScreenModeChange = Environment.TickCount;
                        break;
                    case 2:
                        break;
                    case 3:
                        CurrentScreenMode = 1;
                        TimeLastScreenModeChange = Environment.TickCount;
                        break;
                }

                //this.Exit();
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed &&
            Environment.TickCount - TimeLastScreenModeChange > 250)
            {
                this.Exit();
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed)
            {
                controllerXpressed++;
                if (controllerXpressed == 1) ReloadGun();

            }
            else
            {
                controllerXpressed = 0;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.LeftShoulder == ButtonState.Pressed || GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed)
            {
                controllerLBpressed++;
                if (controllerLBpressed == 1)
                {
                    Player.SwitchWeapon(Player.CurrentGun - 1);
                    weaponSelectionTime = 30;
                }
            }
            else
            {
                controllerLBpressed = 0;
            }
            if (GamePad.GetState(PlayerIndex.One).Buttons.RightShoulder == ButtonState.Pressed || GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed)
            {
                controllerRBpressed++;
                if (controllerRBpressed == 1)
                {
                    Player.SwitchWeapon(Player.CurrentGun + 1);
                    weaponSelectionTime = 30;
                }
            }
            else
            {
                controllerRBpressed = 0;
            }
            if (GamePad.GetState(PlayerIndex.One).Triggers.Right > 0.0f)
            {
                weaponSelectionTime = 0;
                FireGun(gameTime);
            }
            else
            {
                GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
            }

            if (GamePad.GetState(PlayerIndex.One).Triggers.Left > 0.0f)
            {
                if (HappyBarPower == 100 && HappyBarMode == 0)
                {
                    HappyBarMode = 1;
                    try
                    {
                        Yippee.Play();
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                }
            }
            
        }
        private void SpinVisibleStars(GameTime gameTime)
        {
            foreach (GameObject star in Stars)
            {
                star.Rotation += 0.01f * (float)gameTime.ElapsedGameTime.Milliseconds;
            }
        }
        public void starspawn(Vector3 spawnLocation)
        {
            currentStarSelected++;
            if(currentStarSelected >= 20) currentStarSelected = 0;
            Stars[currentStarSelected].Pos = spawnLocation;
        }
        public void ammospawn(Vector3 spawnLocation)
        {
            currentAmmoSelected++;
            if (currentAmmoSelected >= 10) currentAmmoSelected = 0;
            int chanceAmmoSpawn = Rand.Next() % 3;
            if (chanceAmmoSpawn == 0)
            {
                AmmoCrates[currentAmmoSelected].Pos = spawnLocation;
            }
            else if(chanceAmmoSpawn == 1)
            {
                AmmoCrates[currentAmmoSelected].Pos = new Vector3(Rand.Next() % 2000, 14, Rand.Next() % 2000);
            }
            else if (chanceAmmoSpawn == 2)
            {
                AmmoCrates[currentAmmoSelected].Pos = new Vector3(5000, 14, Rand.Next() % 5000);
            }
        }
        public void PlayDeathSound()
        {
                try
                {
                   Scream.Play();
                }
                catch (Exception e)
                {
                    e.ToString();
                }
        }
        private void CheckPickingUpStuff(GameTime gameTime)
        {
           //#region pickup stuff
            for(int i = 0; i < Stars.Count; i++)
            {
                if (Math.Abs(Player.camera.Position.X - Stars[i].Pos.X) < PICKUP_DISTANCE && Math.Abs(Player.camera.Position.Z - Stars[i].Pos.Z) < PICKUP_DISTANCE)
                {
                    HappyBarPower+=20;
                    Stars[i].Pos = new Vector3(5000, 85, 5000);
                    try
                    {
                        StarGotten.Play();
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                }
                if (networkGame)
                {
                    if (Math.Abs(RemotePlayers[0].camera.Position.X - Stars[i].Pos.X) < PICKUP_DISTANCE && Math.Abs(RemotePlayers[0].camera.Position.Z - Stars[i].Pos.Z) < PICKUP_DISTANCE)
                    {
                        RemoteHappyBarPower += 20;
                        Stars[i].Pos = new Vector3(5000, 85, 5000);
                    }
                }
            }
            foreach (GameObject ammo in AmmoCrates)
            {
                if (Math.Abs(Player.camera.Position.X - ammo.Pos.X) < PICKUP_DISTANCE && Math.Abs(Player.camera.Position.Z - ammo.Pos.Z) < PICKUP_DISTANCE)
                {
                    ammo.Pos = new Vector3(5000, 85, 5000);
                    foreach (Gun gun in Player.GunList)
                    {
                        switch (gun.GunCode)
                        {
                            case (int)Guns.Deagle: gun.TotalRounds += 14;
                                break;
                            case (int)Guns.AK47: gun.TotalRounds += 30;
                                break;
                            case (int)Guns.Panzerschreck: gun.TotalRounds += 12;
                                break;
                            default: gun.TotalRounds += 12;
                                break;
                        }
                        try
                        {
                            cockGun.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        
                        
                        //if (gun.GunCode == Player.CurrentGun)
                        // ammoLeft = gun.RoundsInClip.ToString() + " | " + gun.TotalRounds;
                    //ammo.
                    }
                    //star.Rotation += 0.01f * (float)gameTime.ElapsedGameTime.Milliseconds;
                }
                if (networkGame)
                {
                    newAmmo = false;
                    if (Math.Abs(RemotePlayers[0].camera.Position.X - ammo.Pos.X) < PICKUP_DISTANCE && Math.Abs(RemotePlayers[0].camera.Position.Z - ammo.Pos.Z) < PICKUP_DISTANCE)
                    {
                        ammo.Pos = new Vector3(5000, 85, 5000);
                        newAmmo = true;
                        //star.Rotation += 0.01f * (float)gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
            }
             
           //#endregion
        }
        private void UpdateAI(GameTime gameTime)
        {
            foreach (Being Enemy in Enemies)
            {
                if (!Enemy.IsHuman)
                {
                    Enemy.camera.Position += (Enemy.camera.CurrentVelocity * 150.0f * (float)gameTime.ElapsedGameTime.TotalSeconds);

                    if (IsCollision(new Ray(Enemy.camera.Position, Enemy.camera.ViewDirection)))
                    {
                        Enemy.camera.Position -= Enemy.camera.CurrentVelocity;
                        Enemy.camera.CurrentVelocity = Vector3.Zero;
                        Enemy.camera.Rotate(4.0f, 0.0f);
                    }
                    else
                    {
                        Vector3 EnemyVel = Enemy.camera.CurrentVelocity;
                        if (Enemy.camera.CurrentVelocity.Length() < 2.3f)
                            EnemyVel += Enemy.camera.ViewDirection/5.0f;
                        Enemy.camera.CurrentVelocity = EnemyVel;
                    }
                    if (Enemy.timeToChangeDirection < Environment.TickCount)
                    {
                        if (Enemy.timeToChangeDirection + 250 < Environment.TickCount)
                            Enemy.timeToChangeDirection = Environment.TickCount + 5000 + Rand.Next() % 5000;
                        else
                        {
                            Enemy.camera.Rotate(Enemy.camera.PitchDegrees + 4, 0.0f);
                            Enemy.camera.CurrentVelocity = Vector3.Zero;
                        }
                    }
                }
            }
        }
        private void UpdateEffect()
        {
            if (enableParallax)
                effect.CurrentTechnique = effect.Techniques["ParallaxNormalMappingPointLighting"];
            else
                effect.CurrentTechnique = effect.Techniques["NormalMappingPointLighting"];

            effect.Parameters["worldMatrix"].SetValue(Matrix.Identity);
            effect.Parameters["worldInverseTransposeMatrix"].SetValue(Matrix.Identity);
            effect.Parameters["worldViewProjectionMatrix"].SetValue(camera.ViewMatrix * camera.ProjectionMatrix);

            effect.Parameters["cameraPos"].SetValue(camera.Position);
            effect.Parameters["globalAmbient"].SetValue(globalAmbient.ToVector4());
            effect.Parameters["scaleBias"].SetValue(scaleBias);

            effect.Parameters["light"].StructureMembers["dir"].SetValue(light.Direction);
            effect.Parameters["light"].StructureMembers["pos"].SetValue(light.Position);
            effect.Parameters["light"].StructureMembers["ambient"].SetValue(light.Ambient.ToVector4());
            effect.Parameters["light"].StructureMembers["diffuse"].SetValue(light.Diffuse.ToVector4());
            effect.Parameters["light"].StructureMembers["specular"].SetValue(light.Specular.ToVector4());
            effect.Parameters["light"].StructureMembers["spotInnerCone"].SetValue(light.SpotInnerConeRadians);
            effect.Parameters["light"].StructureMembers["spotOuterCone"].SetValue(light.SpotOuterConeRadians);
            effect.Parameters["light"].StructureMembers["radius"].SetValue(light.Radius);

            effect.Parameters["material"].StructureMembers["ambient"].SetValue(material.Ambient.ToVector4());
            effect.Parameters["material"].StructureMembers["diffuse"].SetValue(material.Diffuse.ToVector4());
            effect.Parameters["material"].StructureMembers["emissive"].SetValue(material.Emissive.ToVector4());
            effect.Parameters["material"].StructureMembers["specular"].SetValue(material.Specular.ToVector4());
            effect.Parameters["material"].StructureMembers["shininess"].SetValue(material.Shininess);
        }
        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive)
                return;
            try
            {
                base.Update(gameTime);
            }
            catch (Exception e) { }
            sunCycle+=10/(gameTime.ElapsedGameTime.Milliseconds+1);
            if (sunCycle > 49) sunCycle = 0;

            
            ProcessKeyboard(gameTime);
            ProcessController(gameTime);
            //if (CurrentScreenMode == 1) 

            // Allows the game to exit
            switch (CurrentScreenMode)
            {
                // main menu
                #region CurrentScreenMode == 0
                case 0:
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -.4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Down))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption++;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        if (CurrentMenuOption > 3)
                            CurrentMenuOption = 0;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > .4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Up))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption--;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption < 0)
                            CurrentMenuOption = 3;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250) || KeyJustPressed(Keys.Enter))
                    {
                        try
                        {
                            ShottyShot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        TimeLastScreenModeChange = Environment.TickCount;
                        switch (CurrentMenuOption)
                        {
                            case 0:
                                //Network.CreateGame();
                                CurrentScreenMode = 2;
                                TimeLastScreenModeChange = Environment.TickCount;
                                break;
                            case 1:
                                //Network.FindGames();
                                CurrentScreenMode = 4;
                                TimeLastScreenModeChange = Environment.TickCount;
                                break;
                            case 2:
                                //StorageUnit.DoLoadGame();
                                CurrentScreenMode = 5;
                                FindAvailableSessions();
                                break;
                            case 3:
                                this.Exit();
                                break;
                        }
                        CurrentMenuOption = 0;
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
                    {
                        CurrentScreenMode = 1;
                        TimeLastScreenModeChange = Environment.TickCount;
                    }
                    break;
                #endregion

                // single player selection menu
                #region CurrentScreenMode == 2
                case 2:
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -.4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Down))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption++;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption > 3)
                            CurrentMenuOption = 0;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > .4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Up))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption--;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption < 0)
                            CurrentMenuOption = 3;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250) ||  KeyJustPressed(Keys.Enter))
                    {
                        try
                        {
                            ShottyShot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        TimeLastScreenModeChange = Environment.TickCount;
                        
                        switch (CurrentMenuOption)
                        {
                            case 0:
                                CurrentScreenMode = 0;
                                TimeLastScreenModeChange = Environment.TickCount;
                                //Network.CreateGame();
                                //CurrentScreenMode = 1;
                                //CreateSession();
                                break;
                            case 1:
                                CurrentMapSelection++;
                                if (CurrentMapSelection > 2) CurrentMapSelection = 0;
                                //Network.FindGames();
                                //CurrentScreenMode = 2;
                                //JoinSession();
                                break;
                            case 2:
                                CurrentCharacterSelection++;
                                if (CurrentCharacterSelection > 2) CurrentCharacterSelection = 0;
                                //StorageUnit.DoLoadGame();
                                //CurrentScreenMode = 5;
                                break;
                            case 3:
                                CurrentScreenMode = 1;
                                TimeLastScreenModeChange = Environment.TickCount;
                                networkGame = false;
                                //this.Exit();
                                break;
                        }
                        //CurrentMenuOption = 0;
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
                    {
                        TimeLastScreenModeChange = Environment.TickCount;

                        CurrentScreenMode = 0;

                        CurrentMenuOption = 0;
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
                    {
                        CurrentScreenMode = 1;
                        TimeLastScreenModeChange = Environment.TickCount;
                    }
                    break;
                #endregion
                
                // networked/live selection menu
                #region CurrentScreenMode == 4
                case 4:
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -.4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Down))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption++;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption > 3)
                            CurrentMenuOption = 0;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > .4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Up))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption--;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption < 0)
                            CurrentMenuOption = 3;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250) ||  KeyJustPressed(Keys.Enter))
                    {
                        try
                        {
                            ShottyShot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        TimeLastScreenModeChange = Environment.TickCount;
                        
                        switch (CurrentMenuOption)
                        {
                            case 0:
                                CurrentScreenMode = 0;
                                TimeLastScreenModeChange = Environment.TickCount;
                                //Network.CreateGame();
                                //CurrentScreenMode = 1;
                                //CreateSession();
                                break;
                            case 1:
                                CurrentMapSelection++;
                                if (CurrentMapSelection > 2) CurrentMapSelection = 0;
                                //Network.FindGames();
                                //CurrentScreenMode = 2;
                                //JoinSession();
                                break;
                            case 2:
                                CurrentCharacterSelection++;
                                if (CurrentCharacterSelection > 2) CurrentCharacterSelection = 0;
                                //StorageUnit.DoLoadGame();
                                //CurrentScreenMode = 5;
                                break;
                            case 3:
                                CurrentScreenMode = 1;
                                TimeLastScreenModeChange = Environment.TickCount;
                                CreateSession();
                                //this.Exit();
                                break;
                        }
                        //CurrentMenuOption = 0;
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
                    {
                        TimeLastScreenModeChange = Environment.TickCount;

                        CurrentScreenMode = 0;
                        CurrentMenuOption = 0;
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
                    {
                        CurrentScreenMode = 1;
                        TimeLastScreenModeChange = Environment.TickCount;
                    }
                    break;
                #endregion

                //available sessions menu
                #region CurrentScreenMode == 5
                case 5:
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -.4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Down))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption++;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption > availableSessions.Count + 1)
                            CurrentMenuOption = 0;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > .4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Up))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption--;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption < 0)
                            CurrentMenuOption = availableSessions.Count + 1;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250) || KeyJustPressed(Keys.Enter))
                    {
                        try
                        {
                            ShottyShot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        TimeLastScreenModeChange = Environment.TickCount;

                        if (availableSessions.Count != 0)
                        {
                            if (CurrentMenuOption < availableSessions.Count)
                                JoinThisSession(availableSessions[CurrentMenuOption].HostGamertag);
                            else if (CurrentMenuOption == availableSessions.Count)
                            {
                                FindAvailableSessions();
                            }
                            else if (CurrentMenuOption == availableSessions.Count + 1)
                            {
                                CurrentScreenMode = 0;
                            }
                        }
                        else
                        {
                            if (CurrentMenuOption == 0)
                            {
                                FindAvailableSessions();
                            }
                            else if (CurrentMenuOption == 1)
                            {
                                CurrentScreenMode = 0;
                            }
                        }
                        //CurrentMenuOption = 0;
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
                    {
                        TimeLastScreenModeChange = Environment.TickCount;

                        CurrentScreenMode = 0;
                        CurrentMenuOption = 0;
                    }
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250)
                    {
                        CurrentScreenMode = 1;
                        TimeLastScreenModeChange = Environment.TickCount;
                    }
                    break;
                #endregion
            }
            
            // gameplay and ingame menu shown
            #region CurrentScreenMode == 1

            bool headshotsoundPlayedAlreadyThisFrame = false;
            bool RemoteHeadShotThisFrame = false;
            if (CurrentScreenMode == 1 || CurrentScreenMode == 3)
            {
                if (networkGame)
                {
                    #region Network Game
                    if (networkSession == null)
                    {
                        UpdateMenuScreen();
                    }
                    else
                    {
                        if (networkSession.IsHost)
                        {
                            #region Host's Game
                            //Player.StarScore++;
                            PerformCameraCollisionDetection(Player.camera);
                            UpdateEffect();
                            Shots.Update(gameTime);
                            Player.camera = camera;
                            Player.Update(gameTime);
                            RemotePlayers[0].UpdateOnlyBounds(gameTime);
                            UpdateAI(gameTime);
                            
                            #region Shooting

                            foreach (Being CurrentBeing in Enemies)
                                CurrentBeing.Update(gameTime);
                            foreach (Shot CurrentShot in Shots.Shots)
                            {
                                foreach (Being CurrentBeing in Enemies)
                                    if (CurrentBeing.Intersect(CurrentShot.Tracer).HasValue)
                                    {
                                        ParticleList.AddParticleBatch(32, CurrentShot.Tracer.Position + CurrentBeing.Intersect(CurrentShot.Tracer).Value * CurrentShot.Tracer.Direction,
                                            Vector3.Zero, Crosshair);
                                        if (HappyBarMode == 1)
                                        {
                                            CurrentBeing.CurrentHealth -= 50;
                                        }
                                        else
                                        {
                                            CurrentBeing.CurrentHealth -= 25;
                                        }


                                        if (gotHeadshot)// && CurrentBeing.Dead)
                                        {
                                            gotHeadshot = false;
                                            if (!headshotsoundPlayedAlreadyThisFrame)
                                            {
                                                try
                                                {
                                                    Headshot.Play();
                                                }
                                                catch (Exception e)
                                                {
                                                    e.ToString();
                                                }
                                            }
                                            headshotsoundPlayedAlreadyThisFrame = true;
                                        }
                                    }
                                if (RemotePlayers[0].Intersect(CurrentShot.Tracer).HasValue)
                                {
                                    ParticleList.AddParticleBatch(32, CurrentShot.Tracer.Position + RemotePlayers[0].Intersect(CurrentShot.Tracer).Value * CurrentShot.Tracer.Direction,
                                            Vector3.Zero, Crosshair);
                                    if (HappyBarMode == 1)
                                    {
                                        RemotePlayers[0].CurrentHealth -= 50;
                                    }
                                    else
                                    {
                                        RemotePlayers[0].CurrentHealth -= 25;
                                    }
                                    if (gotHeadshot)// && CurrentBeing.Dead)
                                    {
                                        gotHeadshot = false;
                                        if (!headshotsoundPlayedAlreadyThisFrame)
                                        {
                                            try
                                            {
                                                Headshot.Play();
                                            }
                                            catch (Exception e)
                                            {
                                                e.ToString();
                                            }
                                        }
                                        headshotsoundPlayedAlreadyThisFrame = true;
                                    }
                                }
                            }
                            foreach (Shot CurrentShot in RemoteShots.Shots)
                            {
                                foreach (Being CurrentBeing in Enemies)
                                if (CurrentBeing.RemoteIntersect(CurrentShot.Tracer).HasValue)
                                {
                                    ParticleList.AddParticleBatch(32, CurrentShot.Tracer.Position + CurrentBeing.RemoteIntersect(CurrentShot.Tracer).Value * CurrentShot.Tracer.Direction,
                                        Vector3.Zero, Crosshair);
                                    if (RemoteHappyBarMode == 1)
                                    {
                                        CurrentBeing.CurrentHealth -= 50;
                                    }
                                    else
                                    {
                                        CurrentBeing.CurrentHealth -= 25;
                                    }


                                    if (RemotePlayerGotAHeadShot)// && CurrentBeing.Dead)
                                    {
                                        RemotePlayerGotAHeadShot = false;
                                        if (!RemoteHeadShotThisFrame)
                                        {
                                            RemoteHeadShotThisFrame = true;
                                        }
                                    }
                                }
                                if (Player.RemoteIntersect(CurrentShot.Tracer).HasValue)
                                {
                                    ParticleList.AddParticleBatch(32, CurrentShot.Tracer.Position + Player.Intersect(CurrentShot.Tracer).Value * CurrentShot.Tracer.Direction,
                                            Vector3.Zero, Crosshair);
                                    if (RemoteHappyBarMode == 1)
                                    {
                                        Player.CurrentHealth -= 50;
                                    }
                                    else
                                    {
                                        Player.CurrentHealth -= 25;
                                    }
                                    if (RemotePlayerGotAHeadShot)// && CurrentBeing.Dead)
                                    {
                                        RemotePlayerGotAHeadShot = false;
                                        if (!RemoteHeadShotThisFrame)
                                        {
                                            RemoteHeadShotThisFrame = true;
                                        }
                                    }
                                }
                            }
                            ParticleList.Update(gameTime);

                            #endregion

                            CheckPickingUpStuff(gameTime);
                            SpinVisibleStars(gameTime);
                            HappyModeStuff();
                            #endregion

                            UpdateNetworkSession(gameTime);
                        }
                        else
                        {
                            #region Remote Player's Game
                            PerformCameraCollisionDetection(Player.camera);
                            UpdateEffect();
                            Shots.Update(gameTime);
                            Player.camera = camera;
                            Player.Update(gameTime);
                            //foreach (Being CurrentBeing in Enemies)
                            //    CurrentBeing.Update(gameTime);
                            //foreach (Shot CurrentShot in Shots.Shots)
                            //    foreach (Being CurrentBeing in Enemies)
                            //        if (CurrentBeing.Intersect(CurrentShot.Tracer).HasValue)
                            //        {
                            //            ParticleList.AddParticleBatch(32, CurrentShot.Tracer.Position + CurrentBeing.Intersect(CurrentShot.Tracer).Value * CurrentShot.Tracer.Direction,
                            //                Vector3.Zero, Crosshair);
                            //            CurrentBeing.CurrentHealth -= 40;
                            //        }
                            //ParticleList.Update(gameTime);
                            #endregion

                            UpdateNetworkSession(gameTime);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Single-Player Non-Network Game
                    PerformCameraCollisionDetection(Player.camera);
                    UpdateEffect();
                    Shots.Update(gameTime);
                    Player.camera = camera;
                    Player.Update(gameTime);
                    UpdateAI(gameTime);
                    foreach (Being CurrentBeing in Enemies)
                        CurrentBeing.Update(gameTime);
                    foreach (Shot CurrentShot in Shots.Shots)
                        foreach (Being CurrentBeing in Enemies)
                            if (CurrentBeing.Intersect(CurrentShot.Tracer).HasValue)
                            {
                                ParticleList.AddParticleBatch(32, CurrentShot.Tracer.Position + CurrentBeing.Intersect(CurrentShot.Tracer).Value * CurrentShot.Tracer.Direction,
                                    Vector3.Zero, Crosshair);
                                CurrentBeing.CurrentHealth -= 40;
                                        if (CurrentBeing.CurrentHealth <= 0 && gotHeadshot)
                                        {
                                            gotHeadshot = false;
                                            if (!headshotsoundPlayedAlreadyThisFrame)
                                            {
                                                try
                                                {
                                                    Headshot.Play();
                                                }
                                                catch (Exception e)
                                                {
                                                    e.ToString();
                                                }
                                            }
                                            headshotsoundPlayedAlreadyThisFrame = true;
                                        }
                            }
                    ParticleList.Update(gameTime);
                    SpinVisibleStars(gameTime);
                    CheckPickingUpStuff(gameTime);
                    HappyModeStuff();
                    #endregion
                }
                if (Player.CurrentHealth < 0) Player.CurrentHealth = 0;
            }
            #endregion

            // in-game menu
            #region CurrentScreenMode == 3 and 3 alone
                if (CurrentScreenMode == 3)
                {
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y < -.4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Down))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption++;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        if (CurrentMenuOption > 3)
                            CurrentMenuOption = 0;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y > .4f &&
                        Environment.TickCount - TimeLastMenuChange > 250) || KeyJustPressed(Keys.Up))
                    {
                        TimeLastMenuChange = Environment.TickCount;
                        CurrentMenuOption--;
                        try
                        {
                            emptyClip.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        //try
                        //{
                        //    soundsPlaying[7] = sounds[7].Play();
                        //}
                        //catch (Exception e)
                        //{
                        //    e.ToString();
                        //}
                        if (CurrentMenuOption < 0)
                            CurrentMenuOption = 3;
                    }
                    if ((GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed &&
                        Environment.TickCount - TimeLastScreenModeChange > 250) || KeyJustPressed(Keys.Enter))
                    {
                        try
                        {
                            ShottyShot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                        TimeLastScreenModeChange = Environment.TickCount;
                        bool alreadyChangedYInverted = false;
                        switch (CurrentMenuOption)
                        {
                            case 0:
                                //Network.CreateGame();

                                CurrentScreenMode = 1;
                                TimeLastScreenModeChange = Environment.TickCount;
                                break;
                            case 1:
                                //Network.FindGames();
                                //CurrentScreenMode = 2;
                                if (camera.CONTROLLER_Y_INVERTED == false)
                                {
                                    camera.CONTROLLER_Y_INVERTED = true;
                                    //break;
                                    alreadyChangedYInverted = true;
                                }
                                else
                                {
                                    if (alreadyChangedYInverted == false)
                                    {
                                        camera.CONTROLLER_Y_INVERTED = false;
                                    }
                                }

                                break;
                            case 2:
                                //StorageUnit.DoLoadGame();
                                //CurrentScreenMode = 5;
                                CurrentScreenMode = 0;
                                if (networkGame)
                                {
                                    networkSession.Dispose();
                                    networkSession = null;
                                }
                                TimeLastScreenModeChange = Environment.TickCount;
                                break;
                            case 3:
                                this.Exit();
                                break;
                        }
                        //CurrentMenuOption = 0;
                    }
                    //if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed &&
                    //    Environment.TickCount - TimeLastScreenModeChange > 250)
                    //{
                    //    CurrentScreenMode = 1;
                    //    TimeLastScreenModeChange = Environment.TickCount;
                    //}
                }
                #endregion

            
           
        }
        void UpdateFrameRate(GameTime gameTime)
        {
            accumElapsedTimeSec += gameTime.ElapsedRealTime.TotalSeconds;

            if (accumElapsedTimeSec > 1.0)
            {
                framesPerSecond = frames;
                frames = 0;
                accumElapsedTimeSec = 0.0;
            }
            else
            {
                ++frames;
            }
        }
        private void DrawHUD()
        {
            StringBuilder buffer = new StringBuilder();
            fontPos = new Vector2(1.0f, 256.0f);
            //buffer.AppendLine("Health: " + Player.CurrentHealth);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.DrawString(FontTF2Build, buffer.ToString(), fontPos, Color.Yellow);
            Rectangle Dest = new Rectangle(windowWidth / 2 - 30, windowHeight / 2 - 20, 60, 40);
            spriteBatch.Draw(Crosshair, Dest, Color.White);
            Rectangle rect;
            string weaponName;
            #region Weapon Selection Menu, when active
            float SelectionNamePosY = (float)windowHeight - 288.0f;
            if (weaponSelectionTime > 0)
            {
                weaponSelectionTime--;
                switch (Player.CurrentGun)
                {
                    case (int)Guns.Deagle:
                        weaponName = "Desert Eagle";
                        fontPos = new Vector2(windowWidth / 3 - 128, SelectionNamePosY);
                        spriteBatch.DrawString(FontTF2Build, weaponName, fontPos, Color.White);
                        rect = new Rectangle(windowWidth / 3 - 128, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(DeagleHUDSelectedTexture, rect, Color.White);
                        rect = new Rectangle(windowWidth / 2 - 90, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(AK47HUDTexture, rect, Color.White);
                        rect = new Rectangle(2 * windowWidth / 3, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(ShotgunHUDTexture, rect, Color.White);
                        break;
                    case (int)Guns.AK47:
                        weaponName = "AK-47";
                        fontPos = new Vector2(windowWidth / 2 - 45, SelectionNamePosY);
                        spriteBatch.DrawString(FontTF2Build, weaponName, fontPos, Color.White);
                        rect = new Rectangle(windowWidth / 3 - 128, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(DeagleHUDTexture, rect, Color.White);
                        rect = new Rectangle(windowWidth / 2 - 90, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(AK47HUDSelectedTexture, rect, Color.White);
                        rect = new Rectangle(2 * windowWidth / 3, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(ShotgunHUDTexture, rect, Color.White);
                        break;
                    case (int)Guns.Shotty:
                        weaponName = "Shotgun";
                        fontPos = new Vector2(2 * windowWidth / 3, SelectionNamePosY);
                        spriteBatch.DrawString(FontTF2Build, weaponName, fontPos, Color.White);
                        rect = new Rectangle(windowWidth / 3 - 128, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(DeagleHUDTexture, rect, Color.White);
                        rect = new Rectangle(windowWidth / 2 - 90, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(AK47HUDTexture, rect, Color.White);
                        rect = new Rectangle(2 * windowWidth / 3, windowHeight - 250, 200, 100);
                        spriteBatch.Draw(ShotgunHUDSelectedTexture, rect, Color.White);
                        break;
                }
            }
            #endregion
            #region Draw currently active weapon
            rect = new Rectangle((windowWidth / 2) - 100, windowHeight - 125, 200, 100);
            switch (Player.CurrentGun)
            {
                case (int)Guns.Deagle:
                    {
                        spriteBatch.Draw(DeagleHUDTexture, rect, Color.White);
                        break;
                    }
                case (int)Guns.AK47:
                    {
                        spriteBatch.Draw(AK47HUDTexture, rect, Color.White);
                        break;
                    }
                case (int)Guns.Shotty:
                    {
                        spriteBatch.Draw(ShotgunHUDTexture, rect, Color.White);
                        break;
                    }

            }
            #endregion
            string ammoLeft = " | ";
            foreach (Gun gun in Player.GunList)
                if (gun.GunCode == Player.CurrentGun)
                    ammoLeft = gun.RoundsInClip.ToString() + " | " + gun.TotalRounds;
            fontPos = new Vector2((windowWidth / 2) - 40, windowHeight - 150);
            spriteBatch.DrawString(FontTF2Build, ammoLeft, fontPos, Color.White);

            rect = new Rectangle((int)(.03 * windowWidth), (int)(.95f * windowHeight - 54), 200 + 4, 34);
            spriteBatch.Draw(HealthBarOutlineTexture, rect, Color.Red);
            rect = new Rectangle((int)(.03 * windowWidth) + 2, (int)(.95f * windowHeight - 54) + 2, Player.CurrentHealth * 2, 30);
            if (Player.CurrentHealth > 0)
            {
                spriteBatch.Draw(HealthBarTexture, rect, Color.Red);

            }
            fontPos = new Vector2((int)(.03 * windowWidth) + 2, (int)(.95f * windowHeight - 54) - 50);
            spriteBatch.DrawString(FontTF2Build, "Health: " + Player.CurrentHealth, fontPos, Color.White);
            

            rect = new Rectangle(2, 2, 200, 200);
            if (HappyBarMode == 1)
            {
                spriteBatch.Draw(HappySunTransparentTexture, rect, Color.White);
            }
            else spriteBatch.Draw(HappySunTexture, rect, Color.White);
            

            //sunshine bar

            if (HappyBarPower > 0)
            {
                if (HappyBarPower > 100) HappyBarPower = 100;
                rect = new Rectangle(207, 62, HappyBarPower * 2, 30);
                spriteBatch.Draw(HealthBarTexture, rect, Color.Azure);
            }
            fontPos = new Vector2(205, 110);
            if (HappyBarPower >= 100)
            {
                spriteBatch.DrawString(FontTF2Build, "Press LT for HAPPY MODE", fontPos, Color.Yellow);
            }
            else
            {
                if (HappyBarMode == 0) spriteBatch.DrawString(FontTF2Build, "Happy Bar: " + HappyBarPower + "%", fontPos, Color.White);
                else
                {
                    Rectangle rect2;
                    rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 350, 798, 800);
                    spriteBatch.Draw(HappySunTransparentTexture, rect2, Color.White);
                    spriteBatch.DrawString(FontTF2Build, "HAPPY MODE ACTIVATED", fontPos, Color.White);
                }
            }
            rect = new Rectangle(205, 60, Player.StarScore * 2 + 4, 34);
            spriteBatch.Draw(HealthBarOutlineTexture, rect, Color.White);

            spriteBatch.End();
        }
        private void DrawMenu()
        {
            Vector2 RenderPos, textVector;
            string Title1, ButtonA, ButtonAText, ButtonB, ButtonBText, Singleplayer, Multiplayer, Options, Exit;
            string MenuOption1, MenuOption2, MenuOption3, MenuOption4, MenuOption5;
            int k;
            Rectangle rect1, rect2;
            // TODO: Add your drawing code here
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            switch (CurrentScreenMode)
            {
                // main menu
                #region CurrentScreenMode == 0
                case 0:
                    RenderPos = new Vector2(0.0f, 0.0f);
                    rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 400, 798, 800);
                    spriteBatch.Draw(HopeTexture, rect2, Color.White);

                    rect1 = new Rectangle(50, 50, 200, 200);
                    if (sunCycle < 25)
                    {
                        rect1 = new Rectangle((int)(50 - .4 * sunCycle), (int)(50 - .4 * sunCycle), (int)(sunCycle * .5 + 200), (int)(sunCycle * .5 + 200));
                    }
                    else
                    {
                        rect1 = new Rectangle((int)(50 - .4 * (50 - sunCycle)), (int)(50 - .4 * (50 - sunCycle)), (int)((50 - sunCycle) * .5 + 200), (int)((50 - sunCycle) * .5 + 200));
                    }
                    spriteBatch.Draw(SunTexture, rect1, Color.White);



                    Title1 = "Sunshine Slashers";
                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 4 - 16;
                    RenderPos = FontTF2Build.MeasureString(Title1) / 2;
                    spriteBatch.DrawString(FontTF2Build, Title1, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonA = "'";
                    //FontPos.X = windowWidth / 2 + 00; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonA, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonAText = "Select";
                    //FontPos.X = windowWidth / 2 + 50; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonAText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonB = ")";
                    //FontPos.X = windowWidth / 2 - 200; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = ControllerFont.MeasureString(ButtonB) / 2;
                    //spriteBatch.DrawString(ControllerFont, ButtonB, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonBText = "Back";
                    //FontPos.X = windowWidth / 2 - 125; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonBText) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonBText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    Singleplayer = "Single Player Game";
                    Multiplayer = "Create LAN/Live Game";
                    Options = "Join LAN/Live Game";
                    Exit = "Exit";

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2;
                    RenderPos = FontTF2Build.MeasureString(Singleplayer) / 2;
                    if (CurrentMenuOption == 0)
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 32;
                    RenderPos = FontTF2Build.MeasureString(Multiplayer) / 2;
                    if (CurrentMenuOption == 1)
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 64;
                    RenderPos = FontTF2Build.MeasureString(Options) / 2;
                    if (CurrentMenuOption == 2)
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 96;
                    RenderPos = FontTF2Build.MeasureString(Exit) / 2;
                    if (CurrentMenuOption == 3)
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    break;

                #endregion
                // single player game selection screen
                #region CurrentScreenMode == 2
                case 2:
                    RenderPos = new Vector2(0.0f, 0.0f);
                    rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 400, 798, 800);
                    spriteBatch.Draw(HopeTexture, rect2, Color.White);

                    //Rectangle rect1 = new Rectangle(50, 50, 200, 200);
                    if (sunCycle < 25)
                    {
                        rect1 = new Rectangle((int)(50 - .4 * sunCycle), (int)(50 - .4 * sunCycle), (int)(sunCycle * .5 + 200), (int)(sunCycle * .5 + 200));
                    }
                    else
                    {
                        rect1 = new Rectangle((int)(50 - .4 * (50 - sunCycle)), (int)(50 - .4 * (50 - sunCycle)), (int)((50 - sunCycle) * .5 + 200), (int)((50 - sunCycle) * .5 + 200));
                    }
                    spriteBatch.Draw(SunTexture, rect1, Color.White);

                    Title1 = "Single Player";
                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 4 - 16;
                    RenderPos = FontTF2Build.MeasureString(Title1) / 2;
                    spriteBatch.DrawString(FontTF2Build, Title1, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonA = "'";
                    //FontPos.X = windowWidth / 2 + 00; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = ControllerFont.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(ControllerFont, ButtonA, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonAText = "Select";
                    //FontPos.X = windowWidth / 2 + 50; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonAText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonB = ")";
                    //FontPos.X = windowWidth / 2 - 200; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = ControllerFont.MeasureString(ButtonB) / 2;
                    //spriteBatch.DrawString(ControllerFont, ButtonB, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonBText = "Back";
                    //FontPos.X = windowWidth / 2 - 125; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonBText) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonBText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    Singleplayer = "Return to Main Menu";
                    Multiplayer = "";
                    if(CurrentMapSelection == 0) Multiplayer = "Level: Sweetum's Town";
                    else if (CurrentMapSelection == 1) Multiplayer = "Level: Happy Fun-time Sing-a-long House";
                    else if (CurrentMapSelection == 2) Multiplayer = "Level: Color Wonderland";
                    //Multiplayer = "Character: Commando";
                    Options = "Character: Thanatos";
                    if (CurrentCharacterSelection == 0) Options = "Character: Thanatos";
                    else if (CurrentCharacterSelection == 1) Options = "Character: Chaotika";
                    else if (CurrentCharacterSelection == 2) Options = "Character: Uriel Silverfyre";

                    Exit = "Start Game";

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2;
                    RenderPos = FontTF2Build.MeasureString(Singleplayer) / 2;
                    if (CurrentMenuOption == 0)
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 32;
                    RenderPos = FontTF2Build.MeasureString(Multiplayer) / 2;
                    if (CurrentMenuOption == 1)
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 64;
                    RenderPos = FontTF2Build.MeasureString(Options) / 2;
                    if (CurrentMenuOption == 2)
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 96;
                    RenderPos = FontTF2Build.MeasureString(Exit) / 2;
                    if (CurrentMenuOption == 3)
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    break;
                #endregion

                //in-game menu
                #region CurrentScreenMode == 3
                case 3:
                    RenderPos = new Vector2(0.0f, 0.0f);
                    rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 400, 798, 800);
                    spriteBatch.Draw(HopeTexture, rect2, Color.White);

                    rect1 = new Rectangle(50, 50, 200, 200);
                    if (sunCycle < 25)
                    {
                        rect1 = new Rectangle((int)(50 - .4 * sunCycle), (int)(50 - .4 * sunCycle), (int)(sunCycle * .5 + 200), (int)(sunCycle * .5 + 200));
                    }
                    else
                    {
                        rect1 = new Rectangle((int)(50 - .4 * (50 - sunCycle)), (int)(50 - .4 * (50 - sunCycle)), (int)((50 - sunCycle) * .5 + 200), (int)((50 - sunCycle) * .5 + 200));
                    }
                    spriteBatch.Draw(SunTexture, rect1, Color.White);



                    Title1 = "Menu";
                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 4 - 16;
                    RenderPos = FontTF2Build.MeasureString(Title1) / 2;
                    spriteBatch.DrawString(FontTF2Build, Title1, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonA = "'";
                    //FontPos.X = windowWidth / 2 + 00; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonA, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonAText = "Select";
                    //FontPos.X = windowWidth / 2 + 50; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonAText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonB = ")";
                    //FontPos.X = windowWidth / 2 - 200; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = ControllerFont.MeasureString(ButtonB) / 2;
                    //spriteBatch.DrawString(ControllerFont, ButtonB, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonBText = "Back";
                    //FontPos.X = windowWidth / 2 - 125; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonBText) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonBText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    
                    Singleplayer = "Return to Game";
                    if (camera.CONTROLLER_Y_INVERTED == true)
                    {
                        Multiplayer = "Invert Vertical Axis: Yes";
                    }
                    else
                    {
                        Multiplayer = "Invert Vertical Axis: No";
                    }
                    Options = "Return to Main Menu";
                    Exit = "Exit Game";

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2;
                    RenderPos = FontTF2Build.MeasureString(Singleplayer) / 2;
                    if (CurrentMenuOption == 0)
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 32;
                    RenderPos = FontTF2Build.MeasureString(Multiplayer) / 2;
                    if (CurrentMenuOption == 1)
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 64;
                    RenderPos = FontTF2Build.MeasureString(Options) / 2;
                    if (CurrentMenuOption == 2)
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 96;
                    RenderPos = FontTF2Build.MeasureString(Exit) / 2;
                    if (CurrentMenuOption == 3)
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    break;

                #endregion

                // networked/live game selection screen
                #region CurrentScreenMode == 4
                case 4:
                    RenderPos = new Vector2(0.0f, 0.0f);
                    rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 400, 798, 800);
                    spriteBatch.Draw(HopeTexture, rect2, Color.White);

                    //Rectangle rect1 = new Rectangle(50, 50, 200, 200);
                    if (sunCycle < 25)
                    {
                        rect1 = new Rectangle((int)(50 - .4 * sunCycle), (int)(50 - .4 * sunCycle), (int)(sunCycle * .5 + 200), (int)(sunCycle * .5 + 200));
                    }
                    else
                    {
                        rect1 = new Rectangle((int)(50 - .4 * (50 - sunCycle)), (int)(50 - .4 * (50 - sunCycle)), (int)((50 - sunCycle) * .5 + 200), (int)((50 - sunCycle) * .5 + 200));
                    }
                    spriteBatch.Draw(SunTexture, rect1, Color.White);

                    Title1 = "LAN/Live Game";
                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 4 - 16;
                    RenderPos = FontTF2Build.MeasureString(Title1) / 2;
                    spriteBatch.DrawString(FontTF2Build, Title1, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonA = "'";
                    //FontPos.X = windowWidth / 2 + 00; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = ControllerFont.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(ControllerFont, ButtonA, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonAText = "Select";
                    //FontPos.X = windowWidth / 2 + 50; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonA) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonAText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    //ButtonB = ")";
                    //FontPos.X = windowWidth / 2 - 200; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = ControllerFont.MeasureString(ButtonB) / 2;
                    //spriteBatch.DrawString(ControllerFont, ButtonB, FontPos, Color.White, 0, RenderPos, 0.5f, SpriteEffects.None, 0.5f);

                    //ButtonBText = "Back";
                    //FontPos.X = windowWidth / 2 - 125; FontPos.Y = (float)(windowHeight / 1.25);
                    //RenderPos = FontTF2Build.MeasureString(ButtonBText) / 2;
                    //spriteBatch.DrawString(FontTF2Build, ButtonBText, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    Singleplayer = "Return to Main Menu";

                    Multiplayer = "";
                    if (CurrentMapSelection == 0) Multiplayer = "Level: Sweetum's Town";
                    else if (CurrentMapSelection == 1) Multiplayer = "Level: Happy Fun-time Sing-a-long House";
                    else if (CurrentMapSelection == 2) Multiplayer = "Level: Color Wonderland";
                    //Multiplayer = "Character: Commando";
                    Options = "Character: Thanatos";
                    if (CurrentCharacterSelection == 0) Options = "Character: Thanatos";
                    else if (CurrentCharacterSelection == 1) Options = "Character: Chaotika";
                    else if (CurrentCharacterSelection == 2) Options = "Character: Uriel Silverfyre";

                    Exit = "Start Session";

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2;
                    RenderPos = FontTF2Build.MeasureString(Singleplayer) / 2;
                    if (CurrentMenuOption == 0)
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Singleplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 32;
                    RenderPos = FontTF2Build.MeasureString(Multiplayer) / 2;
                    if (CurrentMenuOption == 1)
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Multiplayer, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 64;
                    RenderPos = FontTF2Build.MeasureString(Options) / 2;
                    if (CurrentMenuOption == 2)
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Options, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);

                    FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2 + 96;
                    RenderPos = FontTF2Build.MeasureString(Exit) / 2;
                    if (CurrentMenuOption == 3)
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(FontTF2Build, Exit, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    break;
                #endregion

                //available network sessions
                #region CurrentScreenMode == 5
                case 5:
                    if (availableSessions != null)
                    {
                        RenderPos = new Vector2(0.0f, 0.0f);
                        rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 400, 798, 800);
                        spriteBatch.Draw(HopeTexture, rect2, Color.White);
                        //Rectangle rect1 = new Rectangle(50, 50, 200, 200);
                        if (sunCycle < 25)
                        {
                            rect1 = new Rectangle((int)(50 - .4 * sunCycle), (int)(50 - .4 * sunCycle), (int)(sunCycle * .5 + 200), (int)(sunCycle * .5 + 200));
                        }
                        else
                        {
                            rect1 = new Rectangle((int)(50 - .4 * (50 - sunCycle)), (int)(50 - .4 * (50 - sunCycle)), (int)((50 - sunCycle) * .5 + 200), (int)((50 - sunCycle) * .5 + 200));
                        }
                        spriteBatch.Draw(SunTexture, rect1, Color.White);
                        Title1 = "Available Sessions";
                        FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 4 - 16;
                        RenderPos = FontTF2Build.MeasureString(Title1) / 2;
                        spriteBatch.DrawString(FontTF2Build, Title1, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                        List<string> sessionHostTags = new List<string>();
                        if (availableSessions.Count != 0)
                        {
                            for (int i = 0; i < availableSessions.Count; i++)
                            {
                                sessionHostTags.Add(availableSessions[i].HostGamertag);
                            }
                        }
                        FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 2;
                        for (int i = 0; i < sessionHostTags.Count; i++)
                        {
                            RenderPos = FontTF2Build.MeasureString(sessionHostTags[i]) / 2;
                            if (CurrentMenuOption == i)
                                spriteBatch.DrawString(FontTF2Build, sessionHostTags[i], FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                            else
                                spriteBatch.DrawString(FontTF2Build, sessionHostTags[i], FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                            FontPos.Y += 32;
                        }
                        FontPos.Y += 32;
                        RenderPos = FontTF2Build.MeasureString("Refresh") / 2;
                        if (CurrentMenuOption == availableSessions.Count)
                            spriteBatch.DrawString(FontTF2Build, "Refresh", FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                        else
                            spriteBatch.DrawString(FontTF2Build, "Refresh", FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                        FontPos.Y += 32;
                        RenderPos = FontTF2Build.MeasureString("Return to Main Menu") / 2;
                        if (CurrentMenuOption == availableSessions.Count + 1)
                            spriteBatch.DrawString(FontTF2Build, "Return to Main Menu", FontPos, Color.Red, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                        else
                            spriteBatch.DrawString(FontTF2Build, "Return to Main Menu", FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
                    }
                    else
                    {
                        CurrentScreenMode = 0;
                        TimeLastScreenModeChange = Environment.TickCount;
                    }
                    break;
                #endregion

            }
            spriteBatch.End();
        }
        private void DrawLine(Vector3 PosA, Vector3 PosB, Color color)
        {
            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                 PrimitiveType.LineList,
                 new VertexPositionColor[2] {
                    new VertexPositionColor(PosA, color),
                    new VertexPositionColor(PosB, color)},
                 0,  // vertex buffer offset to add to each element of the index buffer
                 2,  // number of vertices in pointList
                 new short[2] { 0, 1 },  // the index buffer
                 0,  // first index element to read
                 1   // number of primitives to draw
            );
        }
        public void DrawModel(Model model, Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = transforms[mesh.ParentBone.Index] * world;

                    // Use the matrices provided by the chase camera
                    effect.View = Player.camera.ViewMatrix;
                    effect.Projection = Player.camera.ProjectionMatrix;
                }
                mesh.Draw();
            }
        }
        private void DrawText()
        {
            StringBuilder buffer = new StringBuilder();
            fontPos = new Vector2(1.0f, 1.0f);
            if (displayHelp)
            {
                buffer.AppendLine("Move mouse to free look");
                buffer.AppendLine();
                buffer.AppendLine("Press W and S to move forwards and backwards");
                buffer.AppendLine("Press A and D to strafe left and right");
                buffer.AppendLine("Press SPACE to jump");
                buffer.AppendLine("Press and hold LEFT CTRL to crouch");
                buffer.AppendLine("Press and hold LEFT SHIFT to run");
                buffer.AppendLine();
                buffer.AppendLine("Press M to toggle mouse smoothing");
                buffer.AppendLine("Press P to toggle between parallax normal mapping and normal mapping");
                buffer.AppendLine("Press NUMPAD +/- to change camera rotation speed");
                buffer.AppendLine("Press ALT + ENTER to toggle full screen");
                buffer.AppendLine();
                buffer.AppendLine("Press H to hide help");
            }
#if DEBUG
            else
            {
                buffer.AppendFormat("FPS: {0}\n", framesPerSecond);
                buffer.AppendFormat("Technique: {0}\n",
                    (enableParallax ? "Parallax normal mapping" : "Normal mapping"));
                buffer.AppendFormat("Mouse smoothing: {0}\n\n",
                    (camera.EnableMouseSmoothing ? "on" : "off"));
                buffer.Append("Camera:\n");
                buffer.AppendFormat("  Position: x:{0} y:{1} z:{2}\n",
                    camera.Position.X.ToString("f2"),
                    camera.Position.Y.ToString("f2"),
                    camera.Position.Z.ToString("f2"));
                buffer.AppendFormat("  Orientation: heading:{0} pitch:{1}\n",
                    camera.HeadingDegrees.ToString("f2"),
                    camera.PitchDegrees.ToString("f2"));
                buffer.AppendFormat("  Velocity: x:{0} y:{1} z:{2}\n",
                    camera.CurrentVelocity.X.ToString("f2"),
                    camera.CurrentVelocity.Y.ToString("f2"),
                    camera.CurrentVelocity.Z.ToString("f2"));
                buffer.AppendFormat("  Rotation speed: {0}\n",
                    camera.RotationSpeed.ToString("f2"));
                buffer.Append("\nPress H to display help");
            }
#endif

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
            spriteBatch.End();
        }
        protected override void Draw(GameTime gameTime)
        {
            if (!this.IsActive)
                return;
            if (CurrentScreenMode != 1) GraphicsDevice.Clear(Color.SkyBlue);
            else
            {
                if (CurrentMapSelection == 0) GraphicsDevice.Clear(Color.SkyBlue);
                if (CurrentMapSelection != 0) GraphicsDevice.Clear(Color.HotPink);
            }
            
            #region Gameplay - ScreenMode1
            if (CurrentScreenMode == 1)
            {
                if (networkGame)
                {
                    if (networkSession == null)
                    {
                        DrawMenuScreen();
                    }
                    else
                    {
                        #region Networked Game
                        bool drawingCeiling = false;
                        if (CurrentMapSelection == 1) drawingCeiling = true;

                        #region Draw the room.
                        if (enableColorMap)
                        {
                            //room.Draw(effect, "colorMapTexture", "normalMapTexture", "heightMapTexture",
                            //    brickColorMap, brickNormalMap, brickHeightMap,
                            //    stoneColorMap, stoneNormalMap, stoneHeightMap,
                            //    woodColorMap, woodNormalMap, woodHeightMap);

                            room.Draw(effect, "colorMapTexture", "normalMapTexture", "heightMapTexture",
                            brickColorMap, brickNormalMap, brickHeightMap,
                            stoneColorMap, stoneNormalMap, stoneHeightMap,
                            woodColorMap, woodNormalMap, woodHeightMap, drawingCeiling);
                        }
                        else
                        {
                            room.Draw(effect, "colorMapTexture", "normalMapTexture", "heightMapTexture",
                                nullTexture, brickNormalMap, brickHeightMap,
                                nullTexture, stoneNormalMap, stoneHeightMap,
                                nullTexture, woodNormalMap, woodHeightMap, drawingCeiling);
                        }
                        #endregion
                        foreach (Being CurrentBeing in Enemies)
                        {
                            GraphicsDevice.RenderState.DepthBufferEnable = true;
                            CurrentBeing.Draw(gameTime);
                        }
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        Player.Draw(gameTime);
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        RemotePlayers[0].Draw(gameTime);
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        Car.Draw(gameTime);
                        foreach (GameObject star in Stars)
                        {
                            GraphicsDevice.RenderState.DepthBufferEnable = true;
                            star.Draw(gameTime);
                        }
                        for (int i = 0; i < MAX_HOUSES; i++)
                        {
                            GraphicsDevice.RenderState.DepthBufferEnable = true;
                            House[i].Draw(gameTime);
                        }
                        for (int i = 0; i < MAX_WELLS; i++)
                        {
                            GraphicsDevice.RenderState.DepthBufferEnable = true;
                            Well[i].Draw(gameTime);
                        }
                        //for (int i = 0; i < MAX_CRATES; i++)
                        //{
                        //    GraphicsDevice.RenderState.DepthBufferEnable = true;
                        //    Crate[i].Draw(gameTime);
                        //}
                        foreach (GameObject ammo in AmmoCrates)
                        {
                            ammo.Draw(gameTime);
                        }
                        for (int i = 0; i < MAX_TREES; i++)
                        {
                            GraphicsDevice.RenderState.DepthBufferEnable = true;
                            Tree[i].Draw(gameTime);
                        }
                        for (int i = 0; i < MAX_MUSHROOMS; i++)
                        {
                            GraphicsDevice.RenderState.DepthBufferEnable = true;
                            Mushroom[i].Draw(gameTime);
                        }
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        ParticleList.Draw(gameTime, this);
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        DrawHUD();
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        DrawText();
                        #endregion
                    }
                }
                else
                {
                    #region Single Player Game
                    bool drawingCeiling = false;
                    if (CurrentMapSelection == 1) drawingCeiling = true;

                    #region Draw the room.
                    if (enableColorMap)
                    {
                        //room.Draw(effect, "colorMapTexture", "normalMapTexture", "heightMapTexture",
                        //    brickColorMap, brickNormalMap, brickHeightMap,
                        //    stoneColorMap, stoneNormalMap, stoneHeightMap,
                        //    woodColorMap, woodNormalMap, woodHeightMap);

                        room.Draw(effect, "colorMapTexture", "normalMapTexture", "heightMapTexture",
                        brickColorMap, brickNormalMap, brickHeightMap,
                        stoneColorMap, stoneNormalMap, stoneHeightMap,
                        woodColorMap, woodNormalMap, woodHeightMap, drawingCeiling);
                    }
                    else
                    {
                        room.Draw(effect, "colorMapTexture", "normalMapTexture", "heightMapTexture",
                            nullTexture, brickNormalMap, brickHeightMap,
                            nullTexture, stoneNormalMap, stoneHeightMap,
                            nullTexture, woodNormalMap, woodHeightMap, drawingCeiling);
                    }
                    #endregion
                    GraphicsDevice.RenderState.DepthBufferEnable = true;
                    foreach (Being CurrentBeing in Enemies)
                    {
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        CurrentBeing.Draw(gameTime);
                    }
                    Player.Draw(gameTime);
                    Car.Draw(gameTime);
                    foreach (GameObject star in Stars)
                    {
                        star.Draw(gameTime);
                    }
                    foreach (GameObject ammo in AmmoCrates)
                    {
                        ammo.Draw(gameTime);
                    }
                    for (int i = 0; i < MAX_HOUSES; i++)
                    {
                        House[i].Draw(gameTime);
                    }
                    for (int i = 0; i < MAX_WELLS; i++)
                    {
                        Well[i].Draw(gameTime);
                    }
                    //for (int i = 0; i < MAX_CRATES; i++)
                    //{
                    //    Crate[i].Draw(gameTime);
                    //}
                    for (int i = 0; i < MAX_TREES; i++)
                    {
                        Tree[i].Draw(gameTime);
                    }
                    for (int i = 0; i < MAX_MUSHROOMS; i++)
                    {
                        Mushroom[i].Draw(gameTime);
                    }
                    ParticleList.Draw(gameTime, this);
                    DrawHUD();
                    DrawText();
                    #endregion
                }
                    
            }
            #endregion
            #region ScreenMode!=1
            if (CurrentScreenMode != 1)
            {
                DrawMenu();
            }
            #endregion

            base.Draw(gameTime);
            UpdateFrameRate(gameTime);
        }
        #region Network Functions
        private void UpdateMenuScreen()
        {
            if (IsActive)
                //if (CurrentScreenMode != 1)
                //{
                if (Gamer.SignedInGamers.Count == 0)
                {
                    // If there are no profiles signed in, we cannot proceed.
                    // Show the Guide so the user can sign in.
                    Guide.ShowSignIn(maxLocalGamers, false);
                }
                else if (IsPressed(Keys.A, Buttons.X))
                {
                    // Create a new session?
                    CreateSession();
                }
                else if (IsPressed(Keys.B, Buttons.B))
                {
                    // Join an existing session?
                    JoinSession();
                }
                else if (IsPressed(Keys.Y, Buttons.Y))
                {
                    FindAvailableSessions();
                }
            DrawMenu();
            //}
        }
        private bool IsPressed(Keys key, Buttons button)
        {
            return (KeyJustPressed(key) ||
                    GamePad.GetState(PlayerIndex.One).IsButtonDown(button));
        }
        private void CreateSession()
        {
            DrawMessage("Creating session...");

            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.SystemLink,
                                                       maxLocalGamers, maxGamers);
                networkSession.AllowJoinInProgress = true;
                networkGame = true;

                HookSessionEvents();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        private void DrawSearchingScreen(GameTime gameTime)
        {
            if (!BeginDraw())
                return;

            GraphicsDevice.Clear(Color.SkyBlue);
            Rectangle rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 400, 798, 800);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            //spriteBatch.Begin();

            spriteBatch.Draw(HopeTexture, rect2, Color.White);

            ////spriteBatch.Draw(SunTexture, rect1, Color.White);

            string Title1 = "Looking For Sessions...";
            FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 4 - 16;
            Vector2 RenderPos = FontTF2Build.MeasureString(Title1) / 2;
            spriteBatch.DrawString(FontTF2Build, Title1, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();

            EndDraw();
        }

        private void FindAvailableSessions()
        {
            DrawMessage("Looking For Sessions...");

            try
            {
                // Search for sessions.
                using (availableSessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                maxLocalGamers, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }
                    //availableSessions = sessionsFound;

                    looked = true;

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }
        private void JoinSession()
        {
            DrawMessage("Joining session...");

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection mySessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                maxLocalGamers, null))
                {
                    if (mySessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    networkGame = true;
                    // Join the first session we found.
                    networkSession = NetworkSession.Join(mySessions[0]);
                    RemotePlayers.Add(new Being(this));
                    RemotePlayers[0].camera.Position = Vector3.Zero;
                    CurrentScreenMode = 1;
                    //RemotePlayers[0]

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        private void JoinThisSession(string host)
        {
            DrawMessage("Joining session...");

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection mySessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                maxLocalGamers, null))
                {
                    if (mySessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    int i = 0;
                    foreach (AvailableNetworkSession s in mySessions)
                    {
                        if (s.HostGamertag == host)
                        {
                            networkGame = true;
                            // Join the first session we found.
                            networkSession = NetworkSession.Join(mySessions[i]);
                            RemotePlayers.Add(new Being(this));
                            RemotePlayers[0].camera.Position = Vector3.Zero;
                            CurrentScreenMode = 1;
                        }
                        ++i;
                    }

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        private void HookSessionEvents()
        {
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;
        }
        private void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = networkSession.AllGamers.IndexOf(e.Gamer);

            RemotePlayers.Add(new Being(this));
            RemotePlayers[RemotePlayers.Count - 1].camera.Position = Vector3.Zero;
            //e.Gamer.Tag = new Being(this);
        }
        private void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();

            networkSession.Dispose();
            networkSession = null;
            CurrentScreenMode = 5;
        }
        private void UpdateNetworkSession(GameTime gameTime)
        {
            // Read inputs for locally controlled tanks, and send them to the server.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                RemotePlayersSendPackets(gamer);
            }

            // If we are the server, update all the tanks and transmit
            // their latest positions back out over the network.
            if (networkSession.IsHost)
            {
                SendEnemiesUpdate();
                SendStarUpdate();
                SendParticleUpdate();
                SendPlayerUpdate();
                SendMapData();
                SendRemotePlayerData();
                SendCrateData();
                //HostUpdateServer();
            }

            // Pump the underlying session object.
            networkSession.Update();

            // Make sure the session has not ended.
            if (networkSession == null)
                return;

            // Read any incoming network packets.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                if (gamer.IsHost)
                {
                    HostReadInputFromClients(gamer);
                }
                else
                {
                    RemotesReadIDPacketsFromServer(gamer);
                }
            }

            base.Draw(gameTime);
            UpdateFrameRate(gameTime);
        }
        private void RemotePlayersSendPackets(LocalNetworkGamer gamer)
        {
            if (!networkSession.IsHost)
            {
                // Write our latest input state into a network packet.
                packetWriter.Write(Player.camera.Position);
                packetWriter.Write((double)Player.camera.HeadingDegrees);
                packetWriter.Write((double)Player.camera.PitchDegrees);
                packetWriter.Write((Int16)Player.CurrentGun);
                //packetWriter.Write((Int16)Player.CurrentHealth);

                packetWriter.Write((Int16)Shots.Shots.Count);

                foreach (Shot shot in Shots.Shots)
                {
                    packetWriter.Write((Int16)shot.Caliber);
                    packetWriter.Write((Int16)shot.TimeToDie);
                    packetWriter.Write((Vector3)shot.Tracer.Direction);
                    packetWriter.Write((Vector3)shot.Tracer.Position);
                }

                packetWriter.Write((Int16)HappyBarMode);

                packetWriter.Write((Boolean)Player.Dead);



                gamer.SendData(packetWriter, SendDataOptions.InOrder, networkSession.Host);
            }
        }

        private void SendEnemiesUpdate()
        {
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)PACKET_ID.ENEMY_DATA);
                packetWriter.Write((Int16)Enemies.Count);
                foreach (Being guy in Enemies)
                {
                    packetWriter.Write(guy.camera.Position);
                    packetWriter.Write((double)guy.camera.HeadingDegrees);
                    packetWriter.Write((double)guy.camera.PitchDegrees);
                    packetWriter.Write((Int16)guy.CurrentGun);
                    packetWriter.Write((Int16)guy.CurrentHealth);
                }
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }
        private void SendStarUpdate()
        {
            
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)PACKET_ID.STAR_DATA);
                packetWriter.Write((Int16)Stars.Count);

                foreach (GameObject star in Stars)
                {
                    packetWriter.Write((Vector3)star.Pos);
                    packetWriter.Write((Double)star.Rotation);
                }
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }
        private void SendParticleUpdate()
        {
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)PACKET_ID.PARTICLE_DATA);
                packetWriter.Write((Int16)ParticleList.list.Count);
                foreach (Particle p in ParticleList.list)
                {
                    packetWriter.Write((Vector3)p.pos);
                }
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }
        private void SendPlayerUpdate()
        {
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)PACKET_ID.PLAYER_DATA);
                packetWriter.Write(Player.camera.Position);
                packetWriter.Write((double)Player.camera.HeadingDegrees);
                packetWriter.Write((double)Player.camera.PitchDegrees);
                packetWriter.Write((Int16)Player.CurrentGun);
                //packetWriter.Write((Int16)Player.CurrentHealth);
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }
        private void SendMapData()
        {
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)PACKET_ID.INITIAL_DATA);
                packetWriter.Write((Int16)CurrentMapSelection);
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }
        private void SendRemotePlayerData()
        {
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)PACKET_ID.REMOTE_DATA);
                packetWriter.Write((Int16)RemoteHappyBarPower);
                packetWriter.Write((Boolean)newAmmo);
                packetWriter.Write((Boolean)RemotePlayers[0].Dead);
                packetWriter.Write((Int16)RemotePlayers[0].CurrentHealth);
                packetWriter.Write((Boolean)RemoteHeadShotThisFrame);
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }
        private void SendCrateData()
        {
            
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)PACKET_ID.CRATE_DATA);
                packetWriter.Write((Int16)AmmoCrates.Count);

                foreach (GameObject ammo in AmmoCrates)
                {
                    packetWriter.Write((Vector3)ammo.Pos);
                    packetWriter.Write((Double)ammo.Rotation);
                }
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }

        ///<summary>
        //This method only runs on the server. It calls Update on all the
        //tank instances, both local and remote, using inputs that have
        //been received over the network. It then sends the resulting
        //tank position data to everyone in the session.
        ///</summary>
        private void HostUpdateServer()
        {
            // First off, our packet will indicate how many tanks it has data for.
            //packetWriter.Write(networkSession.AllGamers.Count);

            // Loop over all the players in the session, not just the local ones!
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                packetWriter.Write((Int16)CurrentMapSelection);

                foreach (Being guy in Enemies)
                {
                    packetWriter.Write(guy.camera.Position);
                    packetWriter.Write((double)guy.camera.HeadingDegrees);
                    packetWriter.Write((double)guy.camera.PitchDegrees);
                    packetWriter.Write((Int16)guy.CurrentGun);
                    packetWriter.Write((Int16)guy.CurrentHealth);
                }

                packetWriter.Write((Int16)Stars.Count);

                foreach (GameObject star in Stars)
                {
                    packetWriter.Write((Vector3)star.Pos);
                    packetWriter.Write((Double)star.Rotation);
                }

                packetWriter.Write((Int16)RemoteHappyBarPower);

                packetWriter.Write((Int16)ParticleList.list.Count);

                foreach (Particle p in ParticleList.list)
                {
                    packetWriter.Write((Vector3)p.pos);
                }

                packetWriter.Write(Player.camera.Position);
                packetWriter.Write((double)Player.camera.HeadingDegrees);
                packetWriter.Write((double)Player.camera.PitchDegrees);
                packetWriter.Write((Int16)Player.CurrentGun);
                packetWriter.Write((Int16)Player.CurrentHealth);
            }

            //// Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }

        /// <summary>
        /// This method only runs on the server. It reads tank inputs that
        /// have been sent over the network by a client machine, storing
        /// them for later use by the UpdateServer method.
        /// </summary>
        private void HostReadInputFromClients(LocalNetworkGamer gamer)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                if (!sender.IsLocal)
                {
                    RemotePlayers[0].camera.Position = packetReader.ReadVector3();
                    RemotePlayers[0].camera.HeadingDegrees = (float)packetReader.ReadDouble();
                    RemotePlayers[0].camera.PitchDegrees = (float)packetReader.ReadDouble();
                    RemotePlayers[0].CurrentGun = packetReader.ReadInt16();
                    //RemotePlayers[0].CurrentHealth = packetReader.ReadInt16();

                    int numberOfShots = packetReader.ReadInt16();
                    RemoteShots.Shots.Clear();
                    for (int i = 0; i < numberOfShots; i++)
                    {
                        Shot newShot = new Shot(new Ray(), 0.0f, RemotePlayers[0]);
                        newShot.Caliber = packetReader.ReadInt16();
                        newShot.TimeToDie = packetReader.ReadInt16();
                        Vector3 dir = packetReader.ReadVector3();
                        Vector3 pos = packetReader.ReadVector3();
                        Ray tracer = new Ray(pos, dir);
                        newShot.Tracer = tracer;
                        RemoteShots.Shots.Add(newShot);
                    }
                    RemoteHappyBarMode = packetReader.ReadInt16();
                    RemotePlayers[0].Dead = packetReader.ReadBoolean();
                    previousDeathState = currentDeathState;
                    currentDeathState = RemotePlayers[0].Dead;
                    if (previousDeathState == true && currentDeathState == false)
                    {
                        RemotePlayers[0].CurrentHealth = 100;
                    }
                }
            }
        }


        /// <summary>
        /// This method only runs on client machines. It reads
        /// tank position data that has been computed by the server.
        /// </summary>
        private void RemotesReadPacketsFromServer(LocalNetworkGamer gamer)
        {
            //Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                CurrentMapSelection = packetReader.ReadInt16();

                foreach (Being guy in Enemies)
                {
                    guy.camera.Position = packetReader.ReadVector3();
                    guy.camera.HeadingDegrees = (float)packetReader.ReadDouble();
                    guy.camera.PitchDegrees = (float)packetReader.ReadDouble();
                    guy.CurrentGun = packetReader.ReadInt16();
                    guy.CurrentHealth = packetReader.ReadInt16();
                }

                int numberOfStars = packetReader.ReadInt16();

                for (int i = 0; i < numberOfStars; i++)
                {
                    Stars[0].Pos = packetReader.ReadVector3();
                    Stars[0].Rotation = (float)packetReader.ReadDouble();
                }

                HappyBarPower = packetReader.ReadInt16();

                int numberOfParticles = packetReader.ReadInt16();

                ParticleList.list.Clear();

                for (int i = 0; i < numberOfParticles; i++)
                {
                    Vector3 pos = packetReader.ReadVector3();
                    ParticleList.AddParticle(pos, Vector3.Zero, Crosshair);
                }

                RemotePlayers[0].camera.Position = packetReader.ReadVector3();
                RemotePlayers[0].camera.HeadingDegrees = (float)packetReader.ReadDouble();
                RemotePlayers[0].camera.PitchDegrees = (float)packetReader.ReadDouble();
                RemotePlayers[0].CurrentGun = packetReader.ReadInt16();
                RemotePlayers[0].CurrentHealth = packetReader.ReadInt16();

                //}
            }
        }

        private void RemotesReadIDPacketsFromServer(LocalNetworkGamer gamer)
        {
            //Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                PACKET_ID id = (PACKET_ID)packetReader.ReadInt16();

                switch(id)
                {
                    case PACKET_ID.INITIAL_DATA:

                    CurrentMapSelection = packetReader.ReadInt16();
                    break;

                    case PACKET_ID.ENEMY_DATA:
                    int numberOfEnemies = packetReader.ReadInt16();

                    foreach (Being guy in Enemies)
                    {
                        guy.camera.Position = packetReader.ReadVector3();
                        guy.camera.HeadingDegrees = (float)packetReader.ReadDouble();
                        guy.camera.PitchDegrees = (float)packetReader.ReadDouble();
                        guy.CurrentGun = packetReader.ReadInt16();
                        guy.CurrentHealth = packetReader.ReadInt16();
                    }
                    break;

                    case PACKET_ID.STAR_DATA:
                    int numberOfStars = packetReader.ReadInt16();

                    foreach(GameObject star in Stars)
                    {
                        star.Pos = packetReader.ReadVector3();
                        star.Rotation = (float)packetReader.ReadDouble();
                    }
                    break;

                    case PACKET_ID.PARTICLE_DATA:
                    int numberOfParticles = packetReader.ReadInt16();

                    ParticleList.list.Clear();

                    for (int i = 0; i < numberOfParticles; i++)
                    {
                        Vector3 pos = packetReader.ReadVector3();
                        ParticleList.AddParticle(pos, Vector3.Zero, Crosshair);
                    }
                    break;

                    case PACKET_ID.PLAYER_DATA:

                    RemotePlayers[0].camera.Position = packetReader.ReadVector3();
                    RemotePlayers[0].camera.HeadingDegrees = (float)packetReader.ReadDouble();
                    RemotePlayers[0].camera.PitchDegrees = (float)packetReader.ReadDouble();
                    RemotePlayers[0].CurrentGun = packetReader.ReadInt16();
                    //RemotePlayers[0].CurrentHealth = packetReader.ReadInt16();
                    break;

                    case PACKET_ID.REMOTE_DATA:

                    HappyBarPower = packetReader.ReadInt16();
                    if (HappyBarMode == 1 && HappyBarPower <= 0)
                    {
                        HappyBarMode = 0;
                    }
                    bool getAmmo = packetReader.ReadBoolean();

                    if (getAmmo)
                    {
                        foreach (Gun gun in Player.GunList)
                        {
                            switch (gun.GunCode)
                            {
                                case (int)Guns.Deagle: gun.TotalRounds += 14;
                                    break;
                                case (int)Guns.AK47: gun.TotalRounds += 30;
                                    break;
                                case (int)Guns.Panzerschreck: gun.TotalRounds += 12;
                                    break;
                                default: gun.TotalRounds += 12;
                                    break;
                            }
                            try
                            {
                                cockGun.Play();
                            }
                            catch (Exception e)
                            {
                                e.ToString();
                            }
                        }
                    }

                    bool doYouThinkImDead = packetReader.ReadBoolean();

                    if (doYouThinkImDead == false)
                    {
                        Player.CurrentHealth = packetReader.ReadInt16();
                    }
                    else
                    {
                        int i = packetReader.ReadInt16();
                    }

                    bool playHeadshot = packetReader.ReadBoolean();
                    if (playHeadshot)
                    {
                        try
                        {
                            Headshot.Play();
                        }
                        catch (Exception e)
                        {
                            e.ToString();
                        }
                    }
                    break;

                    case PACKET_ID.CRATE_DATA:
                    int numberOfCrates = packetReader.ReadInt16();

                    foreach (GameObject ammo in AmmoCrates)
                    {
                        ammo.Pos = packetReader.ReadVector3();
                        ammo.Rotation = (float)packetReader.ReadDouble();
                    }
                    break;
                }

                //}
            }
        }

        private void DrawMenuScreen()
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
                message += "Error:\n" + errorMessage.Replace(". ", ".\n") + "\n\n";

            message += "A = create session\n" +
                       "B = join session\n\n";
            if (looked)
            {
                for (int i = 0; i < availableSessions.Count; i++)
                {
                    message += availableSessions[i].HostGamertag;
                    message += "\n";
                }
            }

            spriteBatch.Begin();

            spriteBatch.DrawString(spriteFont, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(spriteFont, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();
        }


        /// <summary>
        /// Helper draws notification messages before calling blocking network methods.
        /// </summary>
        private void DrawMessage(string message)
        {
            if (!BeginDraw())
                return;

            GraphicsDevice.Clear(Color.SkyBlue);
            Rectangle rect2 = new Rectangle(windowWidth / 2 - 399, windowHeight / 2 - 400, 798, 800);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            //spriteBatch.Begin();

            spriteBatch.Draw(HopeTexture, rect2, Color.White);

            ////spriteBatch.Draw(SunTexture, rect1, Color.White);

            string Title1 = message;
            FontPos.X = windowWidth / 2; FontPos.Y = windowHeight / 4 - 16;
            Vector2 RenderPos = FontTF2Build.MeasureString(Title1) / 2;
            spriteBatch.DrawString(FontTF2Build, Title1, FontPos, Color.Black, 0, RenderPos, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.End();

            EndDraw();
        }
        #endregion
    }
}
