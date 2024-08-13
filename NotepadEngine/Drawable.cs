using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NotepadEngine
{
    public class Drawable
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int Rotation { get; set; }

        public bool Animate = true;
        public bool IsVisible { get; set; }
        private List<string> _textAnimation;

        private int _currentFrame = 0;

        private List<Drawable> _DrawnDrawables = new List<Drawable>();
        public Rectangle rect { 
           get => new Rectangle(X, Y, new StringInfo(GetCurrentFrame().Split(Environment.NewLine)[0]).LengthInTextElements, GetCurrentFrame().Split(Environment.NewLine).Length);
        }

        public Drawable(string file)
        {
            _textAnimation = Encoding.UTF8.GetString(File.ReadAllBytes(file)).Split(Environment.NewLine + "{FrameEnd}" + Environment.NewLine).ToList();
        }
        public Drawable(List<string> text)
        {
            _textAnimation = text;
        }


        public string GetCurrentFrame()
        {
            return TextHelpers.RotateString(_textAnimation[_currentFrame], Rotation);
        }

        public void IncrementFrame()
        {
            _currentFrame++;

            if (_currentFrame >= _textAnimation.Count)
            {
                _currentFrame = 0;
            }
        }

        public bool Intersects(Drawable drawable)
        {
            return rect.IntersectsWith(drawable.rect);
        }
    }
}
