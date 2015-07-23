using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThalamusFAtiMA
{
    class Program
    {
        static void Main(string[] args)
        {

            string character = "";
            string version = "A";
            if (args.Length > 0)
            {
                if (args[0] == "help")
                {
                    Console.WriteLine("Usage: " + Environment.GetCommandLineArgs()[0] + " <CharacterName> <Version>");
                    return;
                }
                character = args[0];
            }
            if(args.Length > 1)
            {
                version = args[1];
            }
            ThalamusConnector thalamusCS = new ThalamusConnector("FAtiMA Mind", character);
            FAtiMAConnector  fatima = new FAtiMAConnector(thalamusCS, character,"M",character,version);
            thalamusCS.FAtiMAConnector = fatima;

            Console.ReadLine();
            thalamusCS.Dispose();
        }
    }
}
