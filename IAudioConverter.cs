using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter
{
    interface IAudioConverter
    {
        byte[] CustomConversionToLinear(byte[] audio);
    }
}
