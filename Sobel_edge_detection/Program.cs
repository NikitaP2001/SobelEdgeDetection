using Parcs;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sobel
{
    using log4net;
   
    public class Program : MainModule
    {
        public Program()
        {
            if (!Directory.Exists(m_skResDir))
                Directory.CreateDirectory(m_skResDir);

            if (!Directory.Exists(m_skInDir))
                throw new DirectoryNotFoundException("Input dir not found");

            inFiles = Directory.GetFiles(m_skInDir);
        }        

        public Bitmap process(Bitmap source, ModuleInfo info, int parts)
        {            
            RawBitmap pixelBuffer = new RawBitmap(source);

            RawBitmap[] slices = new RawBitmap[parts];
            var points = new IPoint[parts];
            var channels = new IChannel[parts];

            for (int i = 0; i < parts; i++) {
                slices[i] = new RawBitmap(pixelBuffer, parts, i);
                points[i] = info.CreatePoint();
                channels[i] = points[i].CreateChannel();
                points[i].ExecuteClass("Sobel.Module");
                channels[i].WriteObject(slices[i]);
            }
            DateTime time = DateTime.Now;
            _log.Info("Waiting for a result...");

            for (int i = 0; i < parts; i++) {
                RawBitmap slice = (RawBitmap)channels[i].ReadObject(typeof(RawBitmap));
                pixelBuffer.writePart(slice, parts, i);
            }
            _log.InfoFormat("Result found: time = {0}s",
                Math.Round((DateTime.Now - time).TotalSeconds, 3));
            return pixelBuffer.ToBitmap();
        }

        public override void Run(ModuleInfo info, CancellationToken token = default(CancellationToken))
        {            
            try {
                int pointsNum = options.PointsNum;
                _log.InfoFormat("Number points: {0}", pointsNum);                
                DateTime time = DateTime.Now;                
                foreach (string imgFile in inFiles)
                {                    
                    Bitmap sourceImage = new Bitmap(imgFile);
                    _log.InfoFormat("Processing image {0}", sourceImage);
                    Bitmap resImg = process(sourceImage, info, pointsNum);
                    string resPath = m_skResDir + "/";
                    resPath += Path.GetFileNameWithoutExtension(imgFile);
                    resPath += ".jpg";
                    resImg.Save(resPath, ImageFormat.Jpeg);
                    _log.InfoFormat("Result saved to {0}", resPath);
                }
                _log.InfoFormat("All data processed in: {0}s",
                Math.Round((DateTime.Now - time).TotalSeconds, 3));
            }
            catch (ArgumentException ex)
            {
                _log.Error("Could not open bitmap", ex);
            }
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            options = new CommandLineOptions();

            if (args != null)
            {
                if (!CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    throw new ArgumentException($@"Cannot parse the arguments. Possible usages:{options.GetUsage()}");
                }
            }
            (new Program()).RunModule(options);
        }

        static readonly string m_skResDir = "result";
        static readonly string m_skInDir = "input";
        string[] inFiles;
        private static CommandLineOptions options;
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));
    }
}
