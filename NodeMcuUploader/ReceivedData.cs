using System;

namespace NodeMcuUploader
{
  public class ReceivedData
  {
    public string Echo { get; set; }
    public string Response { get; set; }

    public ReceivedData()
    {
      this.Echo     = String.Empty;
      this.Response = String.Empty;
    }
  }
}
