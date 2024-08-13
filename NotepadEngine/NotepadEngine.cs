using System.Diagnostics;
using Windows.Win32.Foundation;

namespace NotepadEngine
{
    public class NotepadEngine
    {
        private IntPtr _RichTextHandle;
        public NotepadEngine(bool StartNotepad = false)
        {
            if (StartNotepad)
            {
                Process.Start("notepad.exe");
            }

            _RichTextHandle = Win32Helpers.GetNotepadRichText();

            if (_RichTextHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("No valid instance of Notepad's rich text could be found.");
            }
        }

        
    }
}