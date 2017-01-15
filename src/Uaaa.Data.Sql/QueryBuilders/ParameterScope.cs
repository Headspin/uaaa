namespace Uaaa.Data.Sql.QueryBuilders
{
    /// <summary>
    /// Defines scope for sqlparameter name generation
    /// </summary>
    public sealed class ParameterScope
    {
        private int parameterIndex = 0;
        public int GetParameterIndex() => ++parameterIndex;
    }
}
