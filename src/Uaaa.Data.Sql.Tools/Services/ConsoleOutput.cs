using System;

namespace Uaaa.Sql.Tools
{
    public sealed class ConsoleOutput : ITextOutput
    {
        void ITextOutput.ClearLine()
        {
            // not yet supported
            //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");

            // just WriteLine instead
            Console.WriteLine();
        }

        void ITextOutput.Write(string message)
        {
            Console.Write(message);
        }

        void ITextOutput.WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}