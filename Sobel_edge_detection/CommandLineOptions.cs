using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parcs.Module.CommandLine;

namespace Sobel
{
    using CommandLine;
    class CommandLineOptions : BaseModuleOptions {        
        [Option("p", Required = true, HelpText = "Number of points.")]
        public int PointsNum { get; set; }
    }
}
