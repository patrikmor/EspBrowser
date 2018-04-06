using System;

namespace NodeMcuUploader
{
  public class CommunicationException : TimeoutException
  {
    public string Response { get; set; }

    public CommunicationException(string message, string response)
      : base(message)
    {
      this.Response = response;
    }
  }
}
