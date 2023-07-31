namespace AppFrame.Interfaces;

public interface IReport<T>
{
    ITable<ReportDefinition> Table { get; }
    IPubSub PubSub { get; }

    Task<byte[]> Generate(string reportId);
    Task Define(string reportId, ReportDefinition definition);
    Task Subscribe(string reportId, Uri callbackUri, string cronSpec);
    Task Delete(string reportId);
    Task Clone(string sourceReportId, string destinationReportId);
}
public record ReportDefinition
{
    public string Title { get; init; }
    public List<string> Header { get; init; }
    public List<string> Footer { get; init; }
    public List<string> HeaderFields { get; init; }
    public List<TableDefinition> DetailTables { get; init; }
}

public record TableDefinition
{
    public List<(string Field, string Function, string FormattingExpression)> Columns { get; init; }
}
