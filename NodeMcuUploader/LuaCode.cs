namespace NodeMcuUploader
{
  /// <summary>
  /// This module contains all the LUA code that needs to be on the device to perform whats needed.
  /// </summary>
  public static class LuaCode
  {
    // Soft restart
    public const string SOFT_RESTART = "node.restart()";

    // UART setup: uart.setup(id, baud, databits, parity, stopbits[, echo])
    public const string UART_SETUP = "uart.setup(0,{0},8,0,1,{1})";

    // Info command (flash ID)
    public const string PRINT_INFO = "print(node.info());";

    // FS info
    public const string PRINT_FS_INFO = "print(file.fsinfo())";

    // Print chip ID
    public const string PRINT_CHIPID = "print(node.chipid())";

    // Print heap
    public const string PRINT_HEAP = "print(node.heap())";

    // Format FS
    public const string FORMAT = "file.format()";

    // List files
    public const string LIST_FILES = "for key,value in pairs(file.list()) do print(key,value) end";

    // File print
    public const string PRINT_FILE = "file.open(\"{0}\") print('---Start of file {0}---') print(file.read()) file.close() print('---End of file {0}---')";

    // File execute
    public const string DO_FILE = "dofile(\"{0}\")";

    // File compile
    public const string COMPILE_FILE = "node.compile(\"{0}\")";

    // File rename
    public const string RENAME_FILE = "file.rename(\"{0}\",\"{1}\")";

    // File remove
    public const string REMOVE_FILE = "file.remove(\"{0}\")";

    // File exists
    public const string FILE_EXISTS = "print(file.exists(\"{0}\"))";

    // Read file helper function
    public const string READ_HELPER = @"_dl=function()
  uart.write(0, '>'..' ')
  file.open(""{0}"",""r"")
  local buf
  repeat
    buf = file.read(1024)
    if buf ~= nil then
      buf='~~~'..'DATA-START~~~'..buf..'~~~'..'DATA-END~~~'
      uart.write(0,buf)
    end
    tmr.wdclr()
  until(buf == nil)
  file.close()
end";

    // Read file start
    public const string READ_START = "_dl()";

    // Read file delete helper function
    public const string READ_END = "_dl=nil";

    // Write file helper function
    public const string WRITE_HELPER = @"_up=function(n, l, ll)
  local i = 0
  uart.on('data', l, function(b)
    i = i + 1
    file.open(""{0}"",""a+"")
    file.write(b)
    file.close()
    uart.write(0, i..','..l..'>'..' ')
    if i == n then
      uart.on('data')
    end
    if i == n-1 and ll>0 then
      _up(1,ll,ll)
    end
  end,0)
end";

    // Write file start
    public const string WRITE_START = "_up({0},{1},{2})";

    // Write file delete helper function
    public const string WRITE_END = "_up=nil";
  }
}
