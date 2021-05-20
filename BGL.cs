using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OTTER
{
    /// <summary>
    /// -
    /// </summary>
    public partial class BGL : Form
    {
        /* ------------------- */
        #region Environment Variables

        List<Func<int>> GreenFlagScripts = new List<Func<int>>();

        /// <summary>
        /// Uvjet izvršavanja igre. Ako je <c>START == true</c> igra će se izvršavati.
        /// </summary>
        /// <example><c>START</c> se često koristi za beskonačnu petlju. Primjer metode/skripte:
        /// <code>
        /// private int MojaMetoda()
        /// {
        ///     while(START)
        ///     {
        ///       //ovdje ide kod
        ///     }
        ///     return 0;
        /// }</code>
        /// </example>
        public static bool START = true;

        //sprites
        /// <summary>
        /// Broj likova.
        /// </summary>
        public static int spriteCount = 0, soundCount = 0;

        /// <summary>
        /// Lista svih likova.
        /// </summary>
        //public static List<Sprite> allSprites = new List<Sprite>();
        public static SpriteList<Sprite> allSprites = new SpriteList<Sprite>();

        //sensing
        int mouseX, mouseY;
        Sensing sensing = new Sensing();

        //background
        List<string> backgroundImages = new List<string>();
        int backgroundImageIndex = 0;
        string ISPIS = "";

        SoundPlayer[] sounds = new SoundPlayer[1000];
        TextReader[] readFiles = new StreamReader[1000];
        TextWriter[] writeFiles = new StreamWriter[1000];
        bool showSync = false;
        int loopcount;
        DateTime dt = new DateTime();
        String time;
        double lastTime, thisTime, diff;

        #endregion
        /* ------------------- */
        #region Events

        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {                
                foreach (Sprite sprite in allSprites)
                {                    
                    if (sprite != null)
                        if (sprite.Show == true)
                        {
                            g.DrawImage(sprite.CurrentCostume, new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Heigth));
                        }
                    if (allSprites.Change)
                        break;
                }
                if (allSprites.Change)
                    allSprites.Change = false;
            }
            catch
            {
                //ako se doda sprite dok crta onda se mijenja allSprites
                MessageBox.Show("Greška!");
            }
        }

        private void startTimer(object sender, EventArgs e)
        {
            timer1.Start();
            timer2.Start();
            Init();
        }

        private void updateFrameRate(object sender, EventArgs e)
        {
            updateSyncRate();
        }

        /// <summary>
        /// Crta tekst po pozornici.
        /// </summary>
        /// <param name="sender">-</param>
        /// <param name="e">-</param>
        public void DrawTextOnScreen(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var brush = new SolidBrush(Color.WhiteSmoke);
            string text = ISPIS;

            SizeF stringSize = new SizeF();
            Font stringFont = new Font("Arial", 14);
            stringSize = e.Graphics.MeasureString(text, stringFont);

            using (Font font1 = stringFont)
            {
                RectangleF rectF1 = new RectangleF(0, 0, stringSize.Width, stringSize.Height);
                e.Graphics.FillRectangle(brush, Rectangle.Round(rectF1));
                e.Graphics.DrawString(text, font1, Brushes.Black, rectF1);
            }
        }

        private void mouseClicked(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;            
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = false;
            sensing.MouseDown = false;
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;

            //sensing.MouseX = e.X;
            //sensing.MouseY = e.Y;
            //Sensing.Mouse.x = e.X;
            //Sensing.Mouse.y = e.Y;
            sensing.Mouse.X = e.X;
            sensing.Mouse.Y = e.Y;

        }

        delegate void LevUp();
        event LevUp _levup;

        private void keyDown(object sender, KeyEventArgs e)
        {
            sensing.Key = e.KeyCode.ToString();
            sensing.KeyPressedTest = true;
            if(e.KeyCode==Keys.Left)
            {
                goleft = true;
            }
            if (e.KeyCode == Keys.Right)
            {
                goright= true;
            }
            if (e.KeyCode == Keys.Space && jumping == false) //nema visestrukog skakanja!
            {
                jumping = true;
                jumpSpeed = 70;
            }
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            sensing.Key = "";
            sensing.KeyPressedTest = false;
            if (e.KeyCode == Keys.Left)
            {
                goleft = false;
            }
            if (e.KeyCode == Keys.Right)
            {
                goright = false;
            }
            if (jumping == true)
            {
                jumping = false;
            }
        }

        private void Update(object sender, EventArgs e)
        {
            if (sensing.KeyPressed(Keys.Escape))
            {
                START = false;
            }

            if (START)
            {
                this.Refresh();
            }
        }

        #endregion
        /* ------------------- */
        #region Start of Game Methods

        //my
        #region my

        //private void StartScriptAndWait(Func<int> scriptName)
        //{
        //    Task t = Task.Factory.StartNew(scriptName);
        //    t.Wait();
        //}

        //private void StartScript(Func<int> scriptName)
        //{
        //    Task t;
        //    t = Task.Factory.StartNew(scriptName);
        //}

        private int AnimateBackground(int intervalMS)
        {
            while (START)
            {
                setBackgroundPicture(backgroundImages[backgroundImageIndex]);
                Game.WaitMS(intervalMS);
                backgroundImageIndex++;
                if (backgroundImageIndex == 3)
                    backgroundImageIndex = 0;
            }
            return 0;
        }

        private void KlikNaZastavicu()
        {
            foreach (Func<int> f in GreenFlagScripts)
            {
                Task.Factory.StartNew(f);
            }
        }

        #endregion

        /// <summary>
        /// BGL
        /// </summary>
        public BGL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pričekaj (pauza) u sekundama.
        /// </summary>
        /// <example>Pričekaj pola sekunde: <code>Wait(0.5);</code></example>
        /// <param name="sekunde">Realan broj.</param>
        public void Wait(double sekunde)
        {
            int ms = (int)(sekunde * 1000);
            Thread.Sleep(ms);
        }

        //private int SlucajanBroj(int min, int max)
        //{
        //    Random r = new Random();
        //    int br = r.Next(min, max + 1);
        //    return br;
        //}

        /// <summary>
        /// -
        /// </summary>
        public void Init()
        {
            if (dt == null) time = dt.TimeOfDay.ToString();
            loopcount++;
            //Load resources and level here
            this.Paint += new PaintEventHandler(DrawTextOnScreen);
            SetupGame();
         
        }

        /// <summary>
        /// -
        /// </summary>
        /// <param name="val">-</param>
        public void showSyncRate(bool val)
        {
            showSync = val;
            if (val == true) syncRate.Show();
            if (val == false) syncRate.Hide();
        }

        /// <summary>
        /// -
        /// </summary>
        public void updateSyncRate()
        {
            if (showSync == true)
            {
                thisTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                diff = thisTime - lastTime;
                lastTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                double fr = (1000 / diff) / 1000;

                int fr2 = Convert.ToInt32(fr);

                syncRate.Text = fr2.ToString();
            }

        }

        //stage
        #region Stage

        /// <summary>
        /// Postavi naslov pozornice.
        /// </summary>
        /// <param name="title">tekst koji će se ispisati na vrhu (naslovnoj traci).</param>
        public void SetStageTitle(string title)
        {
            this.Text = title;
        }

        /// <summary>
        /// Postavi boju pozadine.
        /// </summary>
        /// <param name="r">r</param>
        /// <param name="g">g</param>
        /// <param name="b">b</param>
        public void setBackgroundColor(int r, int g, int b)
        {
            this.BackColor = Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Postavi boju pozornice. <c>Color</c> je ugrađeni tip.
        /// </summary>
        /// <param name="color"></param>
        public void setBackgroundColor(Color color)
        {
            this.BackColor = color;
        }

        /// <summary>
        /// Postavi sliku pozornice.
        /// </summary>
        /// <param name="backgroundImage">Naziv (putanja) slike.</param>
        public void setBackgroundPicture(string backgroundImage)
        {
            this.BackgroundImage = new Bitmap(backgroundImage);
        }

        /// <summary>
        /// Izgled slike.
        /// </summary>
        /// <param name="layout">none, tile, stretch, center, zoom</param>
        public void setPictureLayout(string layout)
        {
            if (layout.ToLower() == "none") this.BackgroundImageLayout = ImageLayout.None;
            if (layout.ToLower() == "tile") this.BackgroundImageLayout = ImageLayout.Tile;
            if (layout.ToLower() == "stretch") this.BackgroundImageLayout = ImageLayout.Stretch;
            if (layout.ToLower() == "center") this.BackgroundImageLayout = ImageLayout.Center;
            if (layout.ToLower() == "zoom") this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        #endregion

        //sound
        #region sound methods

        /// <summary>
        /// Učitaj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        /// <param name="file">-</param>
        public void loadSound(int soundNum, string file)
        {
            soundCount++;
            sounds[soundNum] = new SoundPlayer(file);
        }

        /// <summary>
        /// Sviraj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        public void playSound(int soundNum)
        {
            sounds[soundNum].Play();
        }

        /// <summary>
        /// loopSound
        /// </summary>
        /// <param name="soundNum">-</param>
        public void loopSound(int soundNum)
        {
            sounds[soundNum].PlayLooping();
        }

        /// <summary>
        /// Zaustavi zvuk.
        /// </summary>
        /// <param name="soundNum">broj</param>
        public void stopSound(int soundNum)
        {
            sounds[soundNum].Stop();
        }

        #endregion

        //file
        #region file methods

        /// <summary>
        /// Otvori datoteku za čitanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToRead(string fileName, int fileNum)
        {
            readFiles[fileNum] = new StreamReader(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToRead(int fileNum)
        {
            readFiles[fileNum].Close();
        }

        /// <summary>
        /// Otvori datoteku za pisanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToWrite(string fileName, int fileNum)
        {
            writeFiles[fileNum] = new StreamWriter(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToWrite(int fileNum)
        {
            writeFiles[fileNum].Close();
        }

        /// <summary>
        /// Zapiši liniju u datoteku.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <param name="line">linija</param>
        public void writeLine(int fileNum, string line)
        {
            writeFiles[fileNum].WriteLine(line);
        }

        /// <summary>
        /// Pročitaj liniju iz datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća pročitanu liniju</returns>
        public string readLine(int fileNum)
        {
            return readFiles[fileNum].ReadLine();
        }

        /// <summary>
        /// Čita sadržaj datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća sadržaj</returns>
        public string readFile(int fileNum)
        {
            return readFiles[fileNum].ReadToEnd();
        }

        #endregion

        //mouse & keys
        #region mouse methods

        /// <summary>
        /// Sakrij strelicu miša.
        /// </summary>
        public void hideMouse()
        {
            Cursor.Hide();
        }

        /// <summary>
        /// Pokaži strelicu miša.
        /// </summary>
        public void showMouse()
        {
            Cursor.Show();
        }

        /// <summary>
        /// Provjerava je li miš pritisnut.
        /// </summary>
        /// <returns>true/false</returns>
        public bool isMousePressed()
        {
            //return sensing.MouseDown;
            return sensing.MouseDown;
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">naziv tipke</param>
        /// <returns></returns>
        public bool isKeyPressed(string key)
        {
            if (sensing.Key == key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">tipka</param>
        /// <returns>true/false</returns>
        public bool isKeyPressed(Keys key)
        {
            if (sensing.Key == key.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
        /* ------------------- */

        /* ------------ GAME CODE START ------------ */

        /* Game variables */
        Sprite kula;
        Sprite kula2;
        Sprite zmaj;
        Sprite drvo1;
        Sprite drvo2;
        Sprite drvo3;
        Sprite drvo4;
        Sprite diamond1;
        Sprite diamond2;
        Sprite diamond3;
        Sprite diamond4;
        Sprite witch;
        Sprite star;
        Sprite star1;
        Sprite star2;
        Sprite star3;
        Sprite ball;

        int zmajBrzina = 20;
        int force;
        int G = 30;
        int jumpSpeed;
        int branchSpeed = 10;
        int witchSpeed = 25;
        int starSpeed = 15;
        int ballSpeed = 30;
        public int bodovi = 0;
        bool goright, goleft,jumping,gameover,loser,levelup;

        /* Initialization */
        
        
        private void SetupGame()
        {
            //1. setup stage
            SetStageTitle("PMF");
             
            setBackgroundPicture("backgrounds\\back.jpg");
            //none, tile, stretch, center, zoom
            setPictureLayout("stretch");


            //2. add sprites
            //    zmaj = new Sprite("sprites//zmaj.png", 0, 550);
           


                kula = new Sprite("sprites//kula2.png", 0, 0);
                kula.SetSize(50);
                kula.SetTransparentColor(Color.White);
                kula.SetVisible(false);
                Game.AddSprite(kula);

                drvo1 = new Sprite("sprites//tree.png", 230, 150);

                drvo1.SetSize(50);

                Game.AddSprite(drvo1);

                drvo2 = new Sprite("sprites//tree.png", 70, 300);

                drvo2.SetSize(50);

                Game.AddSprite(drvo2);

                drvo3 = new Sprite("sprites//tree.png", 200, 570);

                drvo3.SetSize(50);

                Game.AddSprite(drvo3);

                drvo4 = new Sprite("sprites//tree.png", 280, 430);

                drvo4.SetSize(50);

                Game.AddSprite(drvo4);

                diamond1 = new Sprite("sprites//diamond.png", 320, 400);
                diamond1.SetSize(5);
                Game.AddSprite(diamond1);

                diamond2 = new Sprite("sprites//diamond.png", 240, 540);
                diamond2.SetSize(5);
                Game.AddSprite(diamond2);

                diamond3 = new Sprite("sprites//diamond.png", 110, 270);
                diamond3.SetSize(5);
                Game.AddSprite(diamond3);

                diamond4 = new Sprite("sprites//diamond.png", 270, 110);
                diamond4.SetSize(5);
                Game.AddSprite(diamond4);


                witch = new Sprite("sprites//witch.png", 320, 465);
                witch.SetSize(15);
                Game.AddSprite(witch);



                star1 = new Sprite("sprites//star1.png", 100, 100);
                star1.SetSize(50);
                star1.SetVisible(false);
                Game.AddSprite(star1);

                star2 = new Sprite("sprites//star2.png", 200, 200);
                star2.SetSize(50);
                star2.SetVisible(false);
                Game.AddSprite(star2);

                star3 = new Sprite("sprites//star3.png", 70, 70);
                star3.SetSize(50);
                star3.SetVisible(false);
                Game.AddSprite(star3);

                zmaj = new Zmaj("sprites//zmaj.png", 0, 550);
                zmaj.SetSize(20);

                Game.AddSprite(zmaj);

                //3. scripts that start

                Game.StartScript(Zmaj);
                Game.StartScript(PlatformeZmaj);
                Game.StartScript(Dijamanti);
                Game.StartScript(Ispisi);
                Game.StartScript(branchMove);
                Game.StartScript(Kula);
                Game.StartScript(witchMove);
                Game.StartScript(zmajWitch);
                Game.StartScript(Star);

            _levup += BGL__levup;

        }



        private void BGL__levup()
        {
            START = false;
            allSprites.Clear();
            Wait(0.2);
            START = true;
            SetupGame2();
        }


        private void SetupGame2()
        {

                levelup = false;
                
                kula2 = new Sprite("sprites//kula3.png", GameOptions.RightEdge-50, 0);
                kula2.SetSize(70);
                kula2.SetTransparentColor(Color.White);
                kula2.SetVisible(false);
                Game.AddSprite(kula2);

                drvo1 = new Sprite("sprites//tree.png", 350, 350);

                drvo1.SetSize(50);

                Game.AddSprite(drvo1);

                drvo2 = new Sprite("sprites//tree.png", 270, 150);

                drvo2.SetSize(50);

                Game.AddSprite(drvo2);

                drvo3 = new Sprite("sprites//tree.png", 80, 250);

                drvo3.SetSize(50);

                Game.AddSprite(drvo3);

                drvo4 = new Sprite("sprites//tree.png", 50, 500);

                drvo4.SetSize(50);

                Game.AddSprite(drvo4);

                diamond1 = new Sprite("sprites//diamond.png", 390,310 );  
                diamond1.SetSize(5);
                Game.AddSprite(diamond1);

                diamond2 = new Sprite("sprites//diamond.png", 310, 110);
                diamond2.SetSize(5);
                Game.AddSprite(diamond2);

                diamond3 = new Sprite("sprites//diamond.png", 120, 210);
                diamond3.SetSize(5);
                Game.AddSprite(diamond3);

                diamond4 = new Sprite("sprites//diamond.png", 90,460);
                diamond4.SetSize(5);
                Game.AddSprite(diamond4);

                ball= new Sprite("sprites//ball.png", 80, 50);
                ball.SetSize(40);
                Game.AddSprite(ball);

                witch = new Sprite("sprites//witch.png", 50, 30);
                witch.SetSize(15);
                Game.AddSprite(witch);



                star1 = new Sprite("sprites//star1.png", 100, 100);
                star1.SetSize(50);
                star1.SetVisible(false);
                Game.AddSprite(star1);

                star2 = new Sprite("sprites//star2.png", 200, 200);
                star2.SetSize(50);
                star2.SetVisible(false);
                Game.AddSprite(star2);

                star3 = new Sprite("sprites//star3.png", 70, 70);
                star3.SetSize(50);
                star3.SetVisible(false);
                Game.AddSprite(star3);

                zmaj = new Zmaj("sprites//zmaj.png", 0, 550);
                zmaj.SetSize(20);

                Game.AddSprite(zmaj);

                //3. scripts that start

                Game.StartScript(Zmaj);
                Game.StartScript(PlatformeZmaj);
                Game.StartScript(Dijamanti);
                Game.StartScript(Ispisi);
                Game.StartScript(branchMove2);
                Game.StartScript(Kula2);
                Game.StartScript(Ball);
                Game.StartScript(zmajWitch);
                Game.StartScript(Star);
                Game.StartScript(zmajBall);
            
        }

        /* Scripts */

        private int Zmaj()
        {
            while (START && gameover==false) //ili neki drugi uvjet
            { 
                zmaj.Y += 25;
                if (goright == true) zmaj.X += zmajBrzina;
                if (goleft == true) zmaj.X -= zmajBrzina;
                if (zmaj.TouchingSprite(drvo2) && goleft == false && goright == false && bodovi<4)
                {
                  
                    zmaj.X -= branchSpeed;
                }
                if (zmaj.TouchingSprite(drvo1) && goleft == false && goright == false && bodovi>=4)
                {

                    zmaj.X -= branchSpeed;
                }

                if (jumping == true) //&& force < 0
                {
                    zmaj.Y -= jumpSpeed;
                    //jumping = false;
                }

                if (jumping == true)
                {

                    jumpSpeed = -7;  
                 
                }
                else
                {
                    jumpSpeed = 70 ;
                }



                Wait(0.1);
            }
            return 0;
        }

        private int PlatformeZmaj()
        {
            while (START)
            {
                if (zmaj.TouchingSprite(drvo1)) //|| zmaj.TouchingSprite(drvo2) || zmaj.TouchingSprite(drvo3) || zmaj.TouchingSprite(drvo4)
                {

              
                    zmaj.Y = drvo1.Y - zmaj.Heigth;
                }
                if (zmaj.TouchingSprite(drvo2))
                {

                  
                    zmaj.Y = drvo2.Y - zmaj.Heigth;
                }
                if (zmaj.TouchingSprite(drvo3))
                {

                 
                    zmaj.Y = drvo3.Y - zmaj.Heigth;
                }
                if (zmaj.TouchingSprite(drvo4))
                {

                   // jumpSpeed = 150;
                    zmaj.Y = drvo4.Y - zmaj.Heigth;

                }
                Wait(0.1);
            }
            return 0;
        }


        private int Dijamanti()
        {
            while (START) //ili neki drugi uvjet
            {
                if(zmaj.TouchingSprite(diamond1))
                {
                    bodovi++;
                    diamond1.SetX(0);
                    diamond1.SetY(0);
                    diamond1.SetVisible(false);
                }
                if (zmaj.TouchingSprite(diamond2))
                {
                    bodovi++;
                    diamond2.SetX(0);
                    diamond2.SetY(0);
                    diamond2.SetVisible(false);
                }
                if (zmaj.TouchingSprite(diamond3))
                {
                    bodovi++;
                    diamond3.SetX(0);
                    diamond3.SetY(0);
                    diamond3.SetVisible(false);
                }
                if (zmaj.TouchingSprite(diamond4))
                {
                    bodovi++;
                    diamond4.SetX(0);
                    diamond4.SetY(0);
                    diamond4.SetVisible(false);
                }


                Wait(0.1);
            }
            return 0;
        }

        private int Ispisi()
        {
            while (START) //ili neki drugi uvjet
            {
                if(gameover==true && loser==false && levelup==false)
                {
                    ISPIS = "CONGRATS!!! YOU WON!";
                }
                else if(gameover==true && loser==true)
                {
                    ISPIS = "Game over! You lost";
                }
                else if(gameover==false && loser==false && levelup==true)
                {
                    ISPIS = "Novi Level !!!";
                }
                else
                {
                    ISPIS = "Bodovi : " + bodovi;
                }
               
                Wait(0.1);
            }
            return 0;
        }

        private int branchMove() //  drvo2(70, 300)
        {
            while (START) //ili neki drugi uvjet
            {
                if (bodovi < 4)
                {

                    drvo2.X -= branchSpeed;
                    if (drvo2.X < 0 || (drvo2.X + drvo2.Width) > GameOptions.RightEdge)
                    {
                        branchSpeed = -branchSpeed;
                    }
                }
                else
                {
                    drvo2.SetX(0);
                    drvo2.Y += branchSpeed;
                    if((drvo2.Y+drvo2.Heigth)>GameOptions.DownEdge-zmaj.Heigth || drvo2.Y<kula.Heigth)
                    {
                        branchSpeed = -branchSpeed;
                    }
                }
                Wait(0.1);
            }
            return 0;
        }


        private int branchMove2() //  drvo1(350, 350)
        {
            while (START) //ili neki drugi uvjet
            {
                

                    drvo1.X -= branchSpeed;
                    if (drvo1.X < 0 || (drvo1.X + drvo1.Width) > GameOptions.RightEdge)
                    {
                        branchSpeed = -branchSpeed;
                    }
             
                Wait(0.1);
            }
            return 0;
        }

        private int Kula()
        {
            while (START) //ili neki drugi uvjet
            {
                if(bodovi>=4)
                {
                    kula.SetVisible(true);
                    if(zmaj.TouchingSprite(kula))
                    {

                        //gameover = true;
                        
                        zmaj.SetX(0);
                        zmaj.SetY(kula.Heigth-zmaj.Heigth);
                        levelup = true;
                        _levup.Invoke();
                        Wait(2);
                      
                        //Application.Exit();
                    }
                }
                Wait(0.1);
            }
            return 0;
        }


        private int Kula2()
        {
            while (START) //ili neki drugi uvjet
            {
                if (bodovi >= 8)
                {
                    kula2.SetVisible(true);
                    if (zmaj.TouchingSprite(kula2))
                    {

                        gameover = true;

                        zmaj.SetX(kula2.X);
                        zmaj.SetY(kula2.Heigth - zmaj.Heigth);
                       
                        Wait(2);

                        //Application.Exit();
                    }
                }
                Wait(0.1);
            }
            return 0;
        }

        private int witchMove()
        {
            while (START) //ili neki drugi uvjet
            {
                witch.X -= witchSpeed;
                if (witch.X < 0 || (witch.X + witch.Width) > GameOptions.RightEdge)
                {
                    witchSpeed = -witchSpeed;
                }
                Wait(0.1);
            }
            return 0;
        }

        private int witchMove2()
        {
            while (START) //ili neki drugi uvjet
            {
                witch.Y -= witchSpeed;
                if (witch.Y < 0 || (witch.Y + witch.Heigth) > GameOptions.DownEdge)
                {
                    witchSpeed = -witchSpeed;
                }
                Wait(0.1);
            }
            return 0;
        }

        private int Ball()
        {
            while (START) //ili neki drugi uvjet
            {
                if(ball.Y<GameOptions.DownEdge)
                {
                    ball.Y += ballSpeed;
                }
                else
                {
                    ball.SetY(50);
                }
                Wait(0.1);
            }
            return 0;
        }

        private int zmajWitch()
        {
            while (START) //ili neki drugi uvjet
            {
                if(zmaj.TouchingSprite(witch))
                {
                    gameover = true;
                    loser = true;
                    Wait(2);
                    Application.Exit();
                }
                Wait(0.1);
            }
            return 0;
        }

        private int zmajBall()
        {
            while (START) //ili neki drugi uvjet
            {
                if (zmaj.TouchingSprite(ball))
                {
                    gameover = true;
                    loser = true;
                    Wait(2);
                    Application.Exit();
                }
                Wait(0.1);
            }
            return 0;
        }

        private int Star()
        {
            while (START) //ili neki drugi uvjet
            {

                star1.Y += starSpeed;
                if (jumping == true)
                {
                    star1.SetX(zmaj.X +zmaj.Heigth);
                    star1.SetY(zmaj.Y);
                    star1.SetVisible(true);

                }

                star2.Y +=( starSpeed+5);
                if (jumping == true)
                {
                    star2.SetX(zmaj.X+zmaj.Heigth +15);
                    star2.SetY(zmaj.Y+10);
                    star2.SetVisible(true);

                }

                star3.Y +=( starSpeed+10);
                if (jumping == true)
                {
                    star3.SetX(zmaj.X+zmaj.Heigth+  30);
                    star3.SetY(zmaj.Y+20);
                    star3.SetVisible(true);

                }
                Wait(0.1);
            }
            return 0;
        }
       



        private int Metoda()
        {
            while (START) //ili neki drugi uvjet
            {

                Wait(0.1);
            }
            return 0;
        }



        /* ------------ GAME CODE END ------------ */


    }
}
