using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Parcs;

namespace Sobel
{
    public class Module : IModule
    {
        public void Run(ModuleInfo info, CancellationToken token = default(CancellationToken))
        {
            RawBitmap slice = (RawBitmap)info.Parent.ReadObject(typeof(RawBitmap));
            slice.grayScale();
            slice.edgeDetect();
            info.Parent.WriteObject(slice);
        }
    }
}
