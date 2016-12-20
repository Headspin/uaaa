using System;

namespace Uaaa.Sql.Tools
{
    public sealed class ConsoleOutput : ITextOutput
    {
        void ITextOutput.ClearLine()
        {
            //Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
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