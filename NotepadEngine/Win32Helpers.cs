using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;



namespace NotepadEngine
{
    internal class Win32Helpers
    {
        const uint WM_USER = 0x0400;
        const int EM_SETTEXTEX = (int)WM_USER + 97;
        const int WM_SETTEXT = 0X000C;
        const int EM_SETZOOM = ((int)WM_USER + 225);
        unsafe public static void* vPointer;
        public static IntPtr[] GetProcessWindows(int process)
        {
            unsafe
            {
                IntPtr[] apRet = (new IntPtr[256]);
                int iCount = 0;
                HWND pLast = HWND.Null;
                do
                {
                    pLast = FindWindowEx(HWND.Null, pLast, (string)null, (string)null);
                    uint iProcess_;
                    GetWindowThreadProcessId(pLast, &iProcess_);
                    if (iProcess_ == process) apRet[iCount++] = pLast;
                } while (pLast != IntPtr.Zero);
                Array.Resize(ref apRet, iCount);
                return apRet;

            }

        }

        public static IntPtr GetNotepadRichText()
        {
            foreach (Process p in Process.GetProcessesByName("notepad"))
            {
                foreach (IntPtr possibleWindows in GetProcessWindows(p.Id))
                {
                    IntPtr notepadTextbox = FindWindowEx(new HWND(possibleWindows), HWND.Null, "NotepadTextBox", null);
                    IntPtr RichText = FindWindowEx(new HWND(notepadTextbox), HWND.Null, "RichEditD2DPT", null);

                    if (RichText != IntPtr.Zero)
                    {
                        return RichText;
                    }
                }

            }

            return IntPtr.Zero;
        }

        public static bool IsNotepadActive()
        {
            nint ActiveWindow = GetForegroundWindow().Value;
            foreach (Process p in Process.GetProcessesByName("notepad"))
            {
                foreach (IntPtr possibleWindows in GetProcessWindows(p.Id))
                {
                    if (ActiveWindow ==  possibleWindows) return true;
                }

            }
            return false;
        }
        public static void SetRichTextBoxText(HWND hwnd, string text)
        {
            unsafe
            {
                
                uint pid;

                HANDLE process;

                GetWindowThreadProcessId(hwnd, &pid);

                process = OpenProcess(Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_VM_OPERATION | Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ |
                                Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_VM_WRITE | Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION, false, pid);
                nuint bufferSize = 16384*2;
                if (vPointer == null)
                {
                    vPointer = VirtualAllocEx(process, null, bufferSize, Windows.Win32.System.Memory.VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT, Windows.Win32.System.Memory.PAGE_PROTECTION_FLAGS.PAGE_READWRITE);
                }

                SETTEXTEX vGetTextLengthEx = new SETTEXTEX();
                vGetTextLengthEx.flags = (uint)RTBW_FLAGS.ST_DEFAULT;
                vGetTextLengthEx.codepage = 1200;

                nuint SetTextEXSize = (nuint)sizeof(SETTEXTEX);
                WriteProcessMemory(process, vPointer, &vGetTextLengthEx, SetTextEXSize, null);

                fixed (char* p = text)
                {
                    WriteProcessMemory(process, (char*)(vPointer) + SetTextEXSize, p, (nuint)bufferSize - SetTextEXSize, null);
                }

                LRESULT L = SendMessage(hwnd, EM_SETTEXTEX, new WPARAM((nuint)vPointer), new LPARAM((IntPtr)((char*)(vPointer) + SetTextEXSize)));
               // VirtualFreeEx(process, vPointer, 0, Windows.Win32.System.Memory.VIRTUAL_FREE_TYPE.MEM_RELEASE);
                CloseHandle(process);

            }

        }
        
        public static void SetRichZoom(HWND hwnd, int numerator, int denominator)
        {
            SendMessage(hwnd, EM_SETZOOM, new WPARAM((nuint)numerator), new LPARAM(denominator));
        }
        public static uint MciSendString(string s)
        {
            return mciSendString(s, null, 0, HWND.Null);
        }
        public static Rectangle GetWindowRectangle(HWND hWnd)
        {
            RECT rect;
            GetWindowRect(hWnd, out rect);

            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }
        [StructLayout(LayoutKind.Sequential)]
        struct SETTEXTEX
        {
            public uint flags;
            public uint codepage;
        }
        enum RTBW_FLAGS
        {
            ST_DEFAULT = 0x00,
            ST_KEEPUNDO = 0x01,
            ST_SELECTION = 0x02,
            ST_NEWCHARS = 0x04,
            ST_UNICODE = 0x8,
            ST_PLACEHOLDERTEXT = 0x10,
            ST_PLAINTEXTONLY = 0x20
        }


    }
}
