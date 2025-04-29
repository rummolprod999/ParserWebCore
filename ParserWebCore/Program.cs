#region

using System;
using ParserWebCore.BuilderApp;
using ParserWebCore.ParserExecutor;

#endregion

namespace ParserWebCore
{
    internal class Program
    {
        private static Arguments Arg { get; set; }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    $"Недостаточно аргументов для запуска, используйте {AppBuilder.ReqArguments} в качестве аргумента");
                return;
            }

            Init(args[0]);
            Parser();
        }

        private static void Init(string a)
        {
            AppBuilder.GetBuilder(a);
            Arg = AppBuilder.Arg;
        }

        private static void Parser()
        {
            var executor = new Executor(Arg);
            executor.ExecuteParser();
        }
    }
}