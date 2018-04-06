using System.Collections.Generic;

namespace NodeMcuUploader
{
  public class FsInfo
  {
    public List<FsFileInfo> Files { get; set; }
    public int FreeBytes { get; set; }
    public int UsedBytes { get; set; }
    public int TotalBytes { get; set; }

    public FsInfo()
    {
      this.Files = new List<FsFileInfo>();
    }
  }
}
