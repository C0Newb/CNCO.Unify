namespace CNCO.Unify.Communications.Http.Routing;

internal class ListenerOnWebRequestException : Exception
{
  public Exception ListenerException { get; private set; }

  public ListenerOnWebRequestException(Exception innerException)
    : base("Listener handler threw an exception.", innerException) =>
    ListenerException = innerException;
}
