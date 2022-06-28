namespace JukeBoxLibrary.Helpers
{
    public static class Display
    {
        public static void FlowerBox()
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;

            WriteText();

            Console.ForegroundColor = c;
        }

        private static void WriteText()
        {
            var lines = new List<string> {
                "************************************",
                "*                                  *",
                "*   Robbie Dee's Console Jukebox   *",
                "*  © 2014-2022 Rogedo Consultants  *",
                "*                                  *",
                "************************************",
                ""
            };

            WriteLines(lines);
        }

        private static void WriteLines(IEnumerable<string> data)
        {
            foreach (var s in data)
            {
                Write(s);
            }
        }

        private static void Write(string s)
        {
            Console.WriteLine(s);
        }
    }
}
