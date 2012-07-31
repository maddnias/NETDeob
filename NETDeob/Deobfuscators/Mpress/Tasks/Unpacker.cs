using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NETDeob.Misc;

namespace NETDeob.Deobfuscators.Mpress.Tasks
{
    public class Unpacker : IDeobfuscationTask
    {
        public override void PerformTask()
        {
            byte[] test;
            bool a = lf(DeobfuscatorContext.InPath, out test);

            File.WriteAllBytes(@"C:\Users\Mattias\Documents\CodeWall\mpress\outfile.exe", test);

            return;
        }

        public override void CleanUp()
        {
           
        }

        #region Reversed Methods

        private static bool lf(string fn, out byte[] a)
        {
            FileStream input = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);
            int length = (int)input.Length;
            input.Seek(60L, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(input);
            int num2 = reader.ReadInt32();
            if ((num2 >= 2) && (num2 <= (length - 0x200)))
            {
                input.Seek((long)num2, SeekOrigin.Begin);
                if (reader.ReadUInt32() == 0x4550)
                {
                    ushort num4 = reader.ReadUInt16();
                    if (num4 == 0x8664)
                    {
                        num2 += 0x144;
                    }
                    else
                    {
                        num2 += 0x15c;
                    }
                    input.Seek((long)num2, SeekOrigin.Begin);
                    int num5 = reader.ReadInt32();
                    if (num4 == 0x8664)
                    {
                        num2 -= 12;
                        input.Seek((long)num2, SeekOrigin.Begin);
                        num5 += reader.ReadInt32();
                    }
                    else
                    {
                        num5 += 0x10;
                    }
                    if ((num5 < length) && (num5 >= 0x300))
                    {
                        length -= num5;
                        byte[] buffer = new byte[length];
                        input.Seek((long)num5, SeekOrigin.Begin);
                        input.Read(buffer, 0, length);
                        input.Close();
                        if (lz(buffer, out a, length))
                        {
                            return true;
                        }
                    }
                }
            }
            a = null;
            return false;
        }
        private static bool lz(byte[] c, out byte[] a, int l)
        {
            a = null;
            if ((c[4] == 0x4d) && (c[6] == 90))
            {
                int num = (((8 + c[0]) + (c[1] << 8)) + (c[2] << 0x10)) + (c[3] << 0x18);
                byte[] pOs = new byte[num];
                if (lzmat(pOs, c, l) != 0)
                {
                    a = pOs;
                    return true;
                }
            }
            return false;
        }
        private static unsafe int lzmat(byte[] pOs, byte[] pIs, int cI)
        {
            int num2;
            byte[] buffer;
            if (((buffer = pOs) == null) || (buffer.Length == 0))
            {

            }
            fixed (byte* numRef = buffer)
            {
            Label_001B:
                if (((buffer = pIs) == null) || (buffer.Length == 0))
                {

                }
                fixed (byte* numRef2 = buffer)
                {
                Label_0037:
                    numRef[0] = numRef2[4];
                    int index = 5;
                    num2 = 1;
                    byte num3 = 0;
                    while (index < (cI - num3))
                    {
                        byte num5 = numRef2[index++];
                        if (num3 != 0)
                        {
                            num5 = (byte)(num5 >> 4);
                            num5 = (byte)(num5 + ((byte)(numRef2[index] << 4)));
                        }
                        int num4 = 0;
                        while ((num4 < 8) && (index < (cI - num3)))
                        {
                            if ((num5 & 0x80) == 0x80)
                            {
                                int num8;
                                int num9 = numRef2[index];
                                if (num3 != 0)
                                {
                                    num9 = num9 >> 4;
                                }
                                index++;
                                num9 &= 0xfffff;
                                if (num2 < 0x881)
                                {
                                    num8 = num9 >> 1;
                                    if ((num9 & 1) == 1)
                                    {
                                        index += num3;
                                        num8 = (num8 & 0x7ff) + 0x81;
                                        num3 = (byte)(num3 ^ 1);
                                    }
                                    else
                                    {
                                        num8 = (num8 & 0x7f) + 1;
                                    }
                                }
                                else
                                {
                                    num8 = num9 >> 2;
                                    switch ((num9 & 3))
                                    {
                                        case 0:
                                            num8 = (num8 & 0x3f) + 1;
                                            break;

                                        case 1:
                                            index += num3;
                                            num8 = (num8 & 0x3ff) + 0x41;
                                            num3 = (byte)(num3 ^ 1);
                                            break;

                                        case 2:
                                            num8 = (num8 & 0x3fff) + 0x441;
                                            index++;
                                            break;

                                        case 3:
                                            index += 1 + num3;
                                            num8 = (num8 & 0x3ffff) + 0x4441;
                                            num3 = (byte)(num3 ^ 1);
                                            break;
                                    }
                                }
                                int num7 = numRef2[index];
                                if (num3 != 0)
                                {
                                    num7 = num7 >> 4;
                                    num3 = 0;
                                    index++;
                                }
                                else
                                {
                                    num7 &= 0xfff;
                                    num3 = 1;
                                }
                                if ((num7 & 15) != 15)
                                {
                                    num7 = (num7 & 15) + 3;
                                }
                                else
                                {
                                    index++;
                                    if (num7 != 0xfff)
                                    {
                                        num7 = (num7 >> 4) + 0x12;
                                    }
                                    else
                                    {
                                        num7 = numRef2[index];
                                        if (num3 != 0)
                                        {
                                            num7 = num7 >> 4;
                                        }
                                        num7 &= 0xffff;
                                        index += 2;
                                        if (num7 == 0xffff)
                                        {
                                            if (num3 != 0)
                                            {
                                                num7 = (*(((numRef2 + index) - (4))) & 0xfc) << 5;
                                                index++;
                                                num3 = 0;
                                            }
                                            else
                                            {
                                                num7 = (*(((numRef2 + index) - (5))) & 0xfc0) << 1;
                                            }
                                            num7 += (num5 & 0x7f) + 4;
                                            num7 = num7 << 1;
                                            while (num7-- != 0)
                                            {
                                                numRef[num2] = numRef2[index];
                                                index += 4;
                                                num2 += 4;
                                            }
                                            break;
                                        }
                                        num7 += 0x111;
                                    }
                                }
                                int num6 = num2 - num8;
                                while (num7-- != 0)
                                {
                                    numRef[num2++] = numRef[num6++];
                                }
                            }
                            else
                            {
                                numRef[num2] = numRef2[index];
                                if (num3 != 0)
                                {
                                    numRef[num2] = (byte)(numRef[num2] >> 4);
                                    IntPtr ptr1 = (IntPtr)(numRef + num2);
                                    ptr1 = (IntPtr)((byte)(ptr1 + ((byte)(numRef2[index + 1] << 4))));
                                }
                                num2++;
                                index++;
                            }
                            num4++;
                            num5 = (byte)(num5 << 1);
                        }
                    }
                }
            }
            return num2;
        }

 

 


        #endregion
    }
}
