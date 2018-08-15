using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Memory
{
    // Credits to Dot. for this!!
    internal static unsafe class RainPuddleEditor
    {
        private static float* pPuddleLevel;

        static RainPuddleEditor()
        {
            byte* address = FindPattern("\x75\x08\xF3\x0F\x10\x35\x00\x00\x00\x00\xF3\x0F\x10\x05\x00\x00\x00\x00", "xxxxxx????xxxx????") + 2;
            pPuddleLevel = (float*)(*(int*)(address + 4) + address + 8);

        }

        public static float Level
        {
            set
            {
                *pPuddleLevel = value;
            }
            get
            {
                return *pPuddleLevel;
            }
        }

        public unsafe static byte* FindPattern(string pattern, string mask)
        {
            ProcessModule module = Process.GetCurrentProcess().MainModule;

            ulong address = (ulong)module.BaseAddress.ToInt64();
            ulong endAddress = address + (ulong)module.ModuleMemorySize;

            for (; address < endAddress; address++)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (mask[i] != '?' && ((byte*)address)[i] != pattern[i])
                    {
                        break;
                    }
                    else if (i + 1 == pattern.Length)
                    {
                        return (byte*)address;
                    }
                }
            }

            return null;
        }
    }
}
