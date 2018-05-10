using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter
{
    public class AudioType
    {
        public static AudioTypeEnum CheckAudioType(byte[] audio)
        {
            //30 26 B2 75 8E 66 CF 11 A6 D9 00 AA 00 62 CE 6C
            if (audio[0] == 0x30 && audio[1] == 0x26 && audio[2] == 0xB2 && audio[3] == 0x75 && audio[4] == 0x8E && audio[5] == 0x66 && audio[6] == 0xCF && audio[7] == 0x11 && audio[8] == 0xA6 && audio[9] == 0xD9 &&
                audio[10] == 0x00 && audio[11] == 0xAA && audio[12] == 0x00 && audio[13] == 0x62 && audio[14] == 0xCE && audio[15] == 0x6C)

            {
                return AudioTypeEnum.WMA;
            }
            else if ((audio[0] == 0xFF) || //255 - Exception for Vivo project 24/04/2018
                (audio[0] == 0x49 && audio[1] == 0x44 && audio[2] == 0x33) ||  //or 73 68 51 
                (audio[0] == 0xFF && audio[1] == 0xFB))  //or 255 251
            {
                return AudioTypeEnum.MP3;
            }
            else if (audio[0] == 0x52 && audio[1] == 0x49 && audio[2] == 0x46 && audio[3] == 0x46) //WAV FILE - 52 49 46 46
            {
                return AudioTypeEnum.WAV;
            }
            else
            {
                return AudioTypeEnum.Unknown;
            }
        }

        
    }

    public enum AudioTypeEnum
    {
        Unknown,
        WAV,
        MP3,
        WMA
    }
}
