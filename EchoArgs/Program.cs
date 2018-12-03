using System;
using System.Linq;

namespace EchoArgs
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Argument Count : {args.Count()}");
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine($"Argument {i} : {args[i]}");
            }
            Console.WriteLine();
        }
    }
}
