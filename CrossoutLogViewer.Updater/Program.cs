using CrossoutLogView.Common;

using System;
using System.Threading.Tasks;

namespace CrossoutLogView.Updater
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args is null || args.Length == 0 || String.IsNullOrEmpty(args[0]))
                return;
            switch (args[0].ToUpperInvariant())
            {
                case "UPDATE_LOCAL":
                    Update();
                    break;
                case "GEN_METADATA":
                    Metadata();
                    break;
            }
        }

        private static void Update()
        {
            using var cfg = new ConfigUpdater();
            var c = Task.Run(cfg.Update);
            using var img = new ImageUpdater();
            var i = Task.Run(img.Update);
            Task.WaitAll(c, i);
        }

        private static void Metadata()
        {
            using var cfg = new ConfigUpdater();
            var c = cfg.GenerateMetadata(Strings.ConfigPath);
            using var img = new ImageUpdater();
            var i = img.GenerateMetadata(Strings.ImagePath);
            Task.WaitAll(c, i);
        }
    }
}
