namespace JukeboxLibrary.Domain
{
    public class Shell : IController
    {
        public string InputLine { get; set; }

        public Shell() : this("") { }

#pragma warning disable CS8618
        public Shell(string argument)
#pragma warning restore CS8618
        {
            if (string.IsNullOrEmpty(argument))
            {
                Console.Write("Enter song pattern: ");
#pragma warning disable CS8601
                InputLine = Console.ReadLine();
#pragma warning restore CS8601
            }
            else
            {
                InputLine = argument;
            }

        }        

        public void Start()
        {
            if (!String.IsNullOrEmpty(InputLine))
                Next();
        }

        public void Next()
        {
            new Parser(InputLine).Start();           
        }
    }
}
