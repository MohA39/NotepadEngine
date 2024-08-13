using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharpHook;
using System.Globalization;
using SharpHook.Providers;
using SharpHook.Native;

namespace NotepadEngine
{
    public abstract class NotepadGame
    {
        const int targetFPS = 10;
        const float notepadZoom = 1f;

        const string fontFamily = "Cosolas";
        const float fontSize = 11;
        const FontStyle fontStyle = FontStyle.Regular;
        public string NotepadText { get; private set; }
        public string Footer { get; set; } = "";
        private Process _notepadProcess;
        private IntPtr _notepadRichText = IntPtr.Zero;

        CancellationTokenSource wtoken = new CancellationTokenSource();
        TaskPoolGlobalHook GlobalInputHook = new TaskPoolGlobalHook();

        public event EventHandler<(int id, string message, object LoginObject)> LoginCompleted;
        public static List<Drawable> DrawOrder { get; set; } = new List<Drawable>();
        Stopwatch sw = new Stopwatch();

        public enum ButtonClicked
        {
            Left = 1,
            Right = 2
        }
        public void init(bool StartNotepad = false)
        {
            if (StartNotepad)
            {
                _notepadProcess = Process.Start("notepad.exe");
                Thread.Sleep(500);
            }

            _notepadRichText = Win32Helpers.GetNotepadRichText();

            if (_notepadRichText == IntPtr.Zero)
            {
                throw new InvalidOperationException("No valid instance of Notepad's rich text could be found.");
            }
            Win32Helpers.SetRichZoom(new Windows.Win32.Foundation.HWND(_notepadRichText), 2, 5);

            GlobalInputHook.KeyPressed += KeyPressed;
            GlobalInputHook.KeyReleased += KeyReleased;
            GlobalInputHook.MouseClicked += MouseClicked;
            GlobalInputHook.RunAsync();
            Setup();

            if (_notepadRichText == IntPtr.Zero)
            {
                throw new InvalidOperationException("NotepadGame is uninitialized. Please use init.");
            }

            while (true)
            {
                Update();
                Draw();

                NotepadText = "";
                Thread.Sleep(1000 / targetFPS);
            }

        }

        private void MouseClicked(object? sender, MouseHookEventArgs e)
        {
            if (Win32Helpers.IsNotepadActive())
            {
                OnMouseClicked((ButtonClicked)e.Data.Button);
            }
        }

        private void KeyReleased(object? sender, KeyboardHookEventArgs e)
        {

            if (Win32Helpers.IsNotepadActive())
            {
                OnKeyReleased((Keys)e.Data.KeyCode);
            }

        }
        public void KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (Win32Helpers.IsNotepadActive())
            {
                OnKeyPressed((Keys)e.Data.KeyCode);
            }
        }


        public virtual void OnKeyPressed(Keys key) { }
        public virtual void OnKeyReleased(Keys key) { }
        public virtual void OnMouseClicked(ButtonClicked key) { }
        public virtual void Setup()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void Draw()
        {
            try
            {
                foreach (Drawable d in DrawOrder)
                {
                    DrawDrawable(d);
                }
            }
            catch (InvalidOperationException)
            {

            }

            Win32Helpers.SetRichTextBoxText(new Windows.Win32.Foundation.HWND(_notepadRichText), NotepadText + Environment.NewLine + Footer);
            NotepadText = null;
        }

        public void DrawDrawable(Drawable drawable)
        {
            if (NotepadText == null || NotepadText == "")
            {
                NotepadText = drawable.GetCurrentFrame(); // background
            }
            else
            {

                string[] LinesInCanvas = NotepadText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                string[] LinesInDrawable = drawable.GetCurrentFrame().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                int CanvasHeight = NotepadText.Split(Environment.NewLine).Length;
                int CanvasWidth = new StringInfo(NotepadText.Split(Environment.NewLine)[0]).LengthInTextElements;

                int DrawableWidth = new StringInfo(LinesInDrawable[0]).LengthInTextElements;
                int DrawableHeight = LinesInDrawable.Length;
                int rightMostpos = drawable.X + DrawableWidth;

                if (drawable.Y < DrawableHeight * -1 ||
                    drawable.Y > CanvasHeight ||
                    drawable.X < DrawableWidth * -1 ||
                    drawable.X > CanvasWidth) // outside canvas
                {
                    return;
                }

                if (DrawOrder.Contains(drawable))
                {
                    if (DrawOrder.Skip(DrawOrder.IndexOf(drawable) + 1).Any(x => x.rect.Contains(drawable.rect)))
                    {
                        return;
                    }
                }

                // How many characters each line of the canvas can hold
                int SquareCount = new StringInfo(NotepadText.Split(Environment.NewLine)[0]).LengthInTextElements;

                int startI = drawable.Y < 0 ? Math.Abs(drawable.Y) : 0; // In case Y is negative;


                for (int i = startI; i < LinesInDrawable.Length; i++)
                {
                    int charactersSkipped = 0;

                    int startJ = drawable.X < 0 ? Math.Abs(drawable.X) : 0; // In case X is negative;

                    int endJ = rightMostpos <= CanvasWidth ? DrawableWidth : DrawableWidth - ((rightMostpos) - CanvasWidth);

                    
                    if (DrawOrder.Contains(drawable))
                    {
                        if (DrawOrder.Skip(DrawOrder.IndexOf(drawable) + 1).Any(x => x.rect.Contains(new Rectangle(drawable.X, drawable.Y + i, DrawableWidth, 1))))
                        {
                            continue;
                        }
                    }
                    TextElementEnumerator charEnum = StringInfo.GetTextElementEnumerator(LinesInDrawable[i]);
                    for (int j = 0; j < endJ; j++)
                    {
                        if(!charEnum.MoveNext())
                        {
                            break;
                        }

                        // Anti-intersection (for performance reasons)
                        if (j < startJ)
                        {
                            continue;
                        }
                        string currentCharacter = charEnum.GetTextElement();

                        if (currentCharacter == "🔲")
                        {
                            charactersSkipped++;
                            continue;
                        }

                        int indexOfReplacement = ((drawable.X + j) + ((drawable.Y + i) * (SquareCount + 1)));

                        try
                        {
                            LinesInCanvas[drawable.Y + i] = ReplaceCharacter(LinesInCanvas[drawable.Y + i], currentCharacter, (drawable.X + j));
                        }
                        catch (IndexOutOfRangeException)
                        {

                        }
                    }

                }
                
                NotepadText = string.Join(Environment.NewLine, LinesInCanvas);

            }

            if (drawable.Animate)
            {
                drawable.IncrementFrame();
            }
        }
        private string ReplaceCharacter(string original, string character, int index)
        {
            // Ugly hack to allow last character to be replaced
            original += "%";
            int[] indexes = StringInfo.ParseCombiningCharacters(original);

            if (index >= indexes.Length)
            {
                return original;
            }

            return ReplaceAt(original, indexes[index], indexes[index + 1] - indexes[index], character) .TrimEnd('%').ToString();
            

        }
        public static string ReplaceAt(string str, int index, int length, string replace)
        {
            return string.Create(str.Length - length + replace.Length, (str, index, length, replace),
                (span, state) =>
                {
                    state.str.AsSpan().Slice(0, state.index).CopyTo(span);
                    state.replace.AsSpan().CopyTo(span.Slice(state.index));
                    state.str.AsSpan().Slice(state.index + state.length).CopyTo(span.Slice(state.index + state.replace.Length));
                });
        }

        public static SizeF MeasureString(string s, System.Drawing.Font font)
        {
            SizeF result;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                result = g.MeasureString(s, font, int.MaxValue, StringFormat.GenericTypographic);
            }

            return result;
        }
    }
}
