using System;
using ParserWebCore.BuilderApp;

namespace ParserWebCore
{
    class Program
    {
        public static Arguments Arg { get; set; }
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    $"Недостаточно аргументов для запуска, используйте {Builder.reqArguments} в качестве аргумента");
                return;
            }
            Init(args[0]);
        }

        private static void Init(string a)
        {
            Builder.GetBuilder(a);
            Arg = Builder.Arg;
        }
    }
}