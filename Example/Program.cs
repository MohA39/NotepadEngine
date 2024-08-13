using NotepadEngine;
using System.Media;

public class Example : NotepadGame
{
    const int gapBetweenPipes = 35;
    const int gapBetweenUpperAndLower = 28;

    class FullPipe
    {
        public Drawable UpperPipe;
        public Drawable LowerPipe;
        public bool PassSoundPlayed = false;
    }
    Drawable background;
    Drawable bird;
    Drawable floor;

    List<FullPipe> pipes = new List<FullPipe>();
    Dictionary<string, SoundPlayer> soundEffects = new Dictionary<string, SoundPlayer>();
    Random RNG = new Random();

    int score = 0;
    int deaths = 0;
    const float DefaultGravity = 1;
    float gravity = DefaultGravity;
    bool collided = false;
    bool audioLoaded = false;
    public static int Main()
    {
        new Example().init(true);
        return 0;
    }

    public override void Setup()
    {
        background = new Drawable("assets/Background.txt");
        DrawOrder.Add(background);
        bird = new Drawable("assets/bird.txt");
        bird.Y = 32;

        for (int i = 0; i < 3; i++)
        {
            FullPipe fullpipe = new FullPipe();
            fullpipe.UpperPipe = new Drawable("assets/pipeUpper.txt");
            fullpipe.LowerPipe = new Drawable("assets/pipeLower.txt");

            int UpperPipeY = RNG.Next(-24, 0);
            int LowerPipeY = UpperPipeY + fullpipe.UpperPipe.rect.Height + gapBetweenUpperAndLower;
            fullpipe.UpperPipe.Y = UpperPipeY;
            fullpipe.LowerPipe.Y = LowerPipeY;
            fullpipe.UpperPipe.X = (i + 1) * gapBetweenPipes;
            fullpipe.LowerPipe.X = (i + 1) * gapBetweenPipes;

            DrawOrder.Add(fullpipe.UpperPipe);
            DrawOrder.Add(fullpipe.LowerPipe);

            pipes.Add(fullpipe);
        }
        DrawOrder.Add(bird);

        floor = new Drawable("assets/floor.txt");
        floor.Y = 65;
        DrawOrder.Add(floor);
        if (!audioLoaded)
        {
            AudioManager.AddEffect("assets/sfx_wing.wav", "flap");
            AudioManager.AddEffect("assets/sfx_point.wav", "point");
            AudioManager.AddEffect("assets/sfx_hit.wav", "hit");
            AudioManager.AddEffect("assets/sfx_die.wav", "die");
            audioLoaded = true;
        }
        base.Setup();
    }
    public override void Update()
    {
        base.Update();
        bird.Y = Math.Min(bird.Y + (int)Math.Round(gravity), floor.rect.Top - bird.rect.Height + 1);
        gravity += 0.4f;

        if (collided)
        {
            return;
        }

        floor.X--;

        if (floor.X <= -40)
        {
            floor.X = 0;
        }

        if (floor.Intersects(bird))
        {
            KillBird();
        }
        for (int i = 0; i < pipes.Count; i++)
        {
            pipes[i].LowerPipe.X -= 1;
            pipes[i].UpperPipe.X -= 1;

            if (pipes[i].LowerPipe.X < -pipes[i].LowerPipe.rect.Width) // out of canvas
            {
                pipes[i].LowerPipe.X = pipes.Max(x => x.LowerPipe.X) + gapBetweenPipes;
                pipes[i].UpperPipe.X = pipes[i].LowerPipe.X;

                int UpperPipeY = RNG.Next(-20, 0);
                int LowerPipeY = UpperPipeY + pipes[i].UpperPipe.rect.Height + gapBetweenUpperAndLower;
                pipes[i].UpperPipe.Y = UpperPipeY;
                pipes[i].LowerPipe.Y = LowerPipeY;

                pipes[i].PassSoundPlayed = false;
            }
            else
            {
                if (pipes[i].LowerPipe.X < bird.X + bird.rect.Width / 2 && !pipes[i].PassSoundPlayed)
                {
                    AudioManager.PlayEffect("point");
                    pipes[i].PassSoundPlayed = true;
                    score++;
                }

            }

            Rectangle BirdHitBox = new Rectangle(bird.rect.Location, new Size(bird.rect.Width + 1, bird.rect.Height));
            if (pipes[i].LowerPipe.rect.IntersectsWith(BirdHitBox) || pipes[i].UpperPipe.rect.IntersectsWith(BirdHitBox))
            {
                KillBird();
            }

        }
        Footer = "Score: " + score + " | " + "deaths: " + string.Concat(Enumerable.Repeat("☠️", deaths)) + (collided ? " | press R to retry" : "");
    }
    public override void Draw()
    {
        base.Draw();
    }


    public override void OnKeyReleased(Keys key)
    {
        if (collided)
        {
            if (key == Keys.R)
            {
                Reset();
            }
            return;
        }
        if (key == Keys.Space)
        {
            AudioManager.PlayEffect("flap");
            bird.Y -= 8;
            gravity = DefaultGravity;
        }
    }

    public void KillBird()
    {
        AudioManager.PlayEffect("hit");
        AudioManager.PlayEffectDelayed("die", 1000);
        bird.Animate = false;
        bird.X += 5;
        bird.Rotation = 90;
        gravity += 5;
        collided = true;
        deaths++;
    }

    public void Reset()
    {
        collided = false;
        DrawOrder.Clear();
        pipes.Clear();
        gravity = DefaultGravity;
        score = 0;

        Setup();

    }
}