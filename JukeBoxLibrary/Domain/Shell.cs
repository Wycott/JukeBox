namespace JukeBoxLibrary.Domain
{
    public class Shell : IController
    {
        public string InputLine { get; set; }

        public Shell() : this("") { }

        public Shell(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                Console.Write("Enter song pattern: ");
                InputLine = Console.ReadLine();
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
