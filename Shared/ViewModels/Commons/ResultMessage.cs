namespace Shared.ViewModels.Commons;

public class ResultMessage : ResultMessage<string>
{
}

public class ResultMessage<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}