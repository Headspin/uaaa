namespace Uaaa.Sql.Tools
{
    public interface ITextOutput{
        void ClearLine();
        void Write(string message);
        void WriteLine(string message);
    }
}