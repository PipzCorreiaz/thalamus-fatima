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
            string clientName = "";
            string character = "";
            string version = "A";
            string robotId = "";
            if (args.Length == 3)
            {
                robotId = args[0];
                clientName = args[1];
                character = args[2];
            }
            ThalamusConnector thalamusCS = new ThalamusConnector(clientName, character);
            FAtiMAConnector  fatima = new FAtiMAConnector(thalamusCS, robotId, character,"M",character,version);
            thalamusCS.FAtiMAConnector = fatima;

            Console.ReadLine();
            thalamusCS.Dispose();
        }
    }
}
