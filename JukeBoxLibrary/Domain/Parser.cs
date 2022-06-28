using System.IO;

namespace JukeboxLibrary.Domain
{
    internal class Parser : IController
    {
        public string ParseLine { get; set; }
        public string FileName { get; set; }

#pragma warning disable CS8618
        internal Parser(string argument)
#pragma warning restore CS8618
        {
            ParseLine = argument;
        }

        public void Start()
        {
            const string root = @"e:\";

            if (string.IsNullOrEmpty(ParseLine)) return;

            PreparePattern();

            foreach (var ddir in Directory.GetDirectories(root))
            {
                try
                {
                    foreach (var dfile in Directory.GetFiles(ddir, ParseLine, SearchOption.AllDirectories))
                    {
                        if (ExtensionsOk(dfile) && VersionRequired(dfile))
                        {
                            FileName = dfile;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(FileName))
                    {
                        Next();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Standard...
                }
            }

            NothingDoing();
        }

        public void Next()
        {
            new Launcher(FileName).Start();
        }

        private static void NothingDoing()
        {
            //var c = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("Not found");
            //Console.WriteLine();
            //Console.ForegroundColor = c;
            new Shell().Start();
        }

        private static bool ExtensionsOk(string candidate)
        {
            var extensions = new List<string> { ".mp3", ".m4a" };

            foreach (var s in extensions)
            {
                if (candidate.Contains(s)) return true;
            }

            return false;
        }

        private static bool VersionRequired(string candidate)
        {
            Console.Write("Found: ");
            WriteWhite(candidate);
            Console.Write("Play this y/n? ");
            var cki = Console.ReadKey();
            Console.WriteLine();
            return (cki.Key == ConsoleKey.Y);
        }

        private static void WriteWhite(string data)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(data);
            Console.ForegroundColor = c;
        }

        private void PreparePattern()
        {
            ParseLine = "*" + ParseLine.Replace(' ', '*') + "*";
        }
    }
}
