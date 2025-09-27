namespace McpNetDll.Repository;

public class PaginationInfo
{
    public int Total { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
    public bool HasMore => Offset + Limit < Total;
}