using System;

namespace NodeMcuUploader
{
  public class BadResponseException : ApplicationException
  {
    public string Expected { get; set; }
    public string Actual { get; set; }

    public BadResponseException(string message, string expected, string actual)
      : base(message)
    {
      this.Expected = expected;
      this.Actual   = actual;
    }
  }
}
