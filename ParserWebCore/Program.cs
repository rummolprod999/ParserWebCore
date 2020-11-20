using System;
using ParserWebCore.BuilderApp;
using ParserWebCore.ParserExecutor;

namespace ParserWebCore
{
    class Program
    {
        private static Arguments Arg { get; set; }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    $"Недостаточно аргументов для запуска, используйте {Builder.ReqArguments} в качестве аргумента");
                return;
            }

            Init(args[0]);
            Parser();
        }

        private static void Init(string a)
        {
            Builder.GetBuilder(a);
            Arg = Builder.Arg;
        }

        private static void Parser()
        {
            var executor = new Executor(Arg);
            executor.ExecuteParser();
        }
    }
}