using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace NodeMcuUploader
{
  public class Uploader : IDisposable
  {
    protected static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private const string EOL               = "> ";
    private const string CRLF              = "\r\n";
    private static string[] CRLF_SEPARATOR = { CRLF };
    private const string DATA_START        = "~~~DATA-START~~~";
    private const string DATA_END          = "~~~DATA-END~~~";
    private const int RECEIVE_BUFFER_SIZE  = 1024;
    private const int DOWNLOAD_BUFFER_SIZE = 1100;
    private const int UPLOAD_PACKET_SIZE   = 250;

    public int Timeout { get; protected set; }
    public int BaudRate { get; protected set; }
    public bool EchoEnabled { get; protected set; }

    protected SerialPort port;

    public Uploader(string port, int baud_rate, int timeout, bool echo_enabled)
    {
      this.BaudRate    = baud_rate;
      this.Timeout     = timeout;
      this.EchoEnabled = echo_enabled;

      log.Info($"Opening port {port} with {baud_rate} baud");
      this.port              = new SerialPort(port, baud_rate);
      this.port.ReadTimeout  = timeout * 1000;
      this.port.WriteTimeout = timeout * 1000;
      this.port.RtsEnable    = false;
      this.port.DtrEnable    = false;
      this.port.Open();
      ClearBuffers();
    }

    /// <summary>
    /// Attach SerialDataReceived and SerialErrorReceived event handlers
    /// </summary>
    /// <param name="data_received">SerialDataReceivedEventHandler</param>
    /// <param name="error_recieved">SerialErrorReceivedEventHandler</param>
    public void AttachEvents(SerialDataReceivedEventHandler data_received, SerialErrorReceivedEventHandler error_recieved)
    {
      this.port.DataReceived  += data_received;
      this.port.ErrorReceived += error_recieved;
    }

    /// <summary>
    /// Dettach SerialDataReceived and SerialErrorReceived event handlers
    /// </summary>
    /// <param name="data_received">SerialDataReceivedEventHandler</param>
    /// <param name="error_recieved">SerialErrorReceivedEventHandler</param>
    public void DettachEvents(SerialDataReceivedEventHandler data_received, SerialErrorReceivedEventHandler error_recieved)
    {
      this.port.DataReceived  -= data_received;
      this.port.ErrorReceived -= error_recieved;
    }

    /// <summary>
    /// Set Read/Write timeout on serial port
    /// </summary>
    /// <param name="timeout"></param>
    public void SetTimeout(int timeout)
    {
      log.Info($"Changing timeout to {timeout}");
      this.Timeout           = timeout;
      this.port.ReadTimeout  = timeout * 1000;
      this.port.WriteTimeout = timeout * 1000;
    }

    /// <summary>
    /// Setting baudrate if supported
    /// </summary>
    /// <param name=""></param>
    public void SetBaudrate(int baud_rate)
    {
      log.Info($"Changing communication to {baud_rate} baud");
      WriteLine(String.Format(LuaCode.UART_SETUP, baud_rate, this.EchoEnabled ? "1" : "0"));
      // Wait for the string to be sent before switching baud
      Thread.Sleep(100);
      this.port.BaudRate = baud_rate;
      this.BaudRate = baud_rate;
    }

    /// <summary>
    /// Setting Echo
    /// </summary>
    /// <param name="echo_enabled"></param>
    public void SetEcho(bool echo_enabled)
    {
      log.Info($"Changing echo to {echo_enabled}");
      WriteLine(String.Format(LuaCode.UART_SETUP, this.BaudRate, echo_enabled ? "1" : "0"));
      Thread.Sleep(100);
      this.EchoEnabled = echo_enabled;
    }

    /// <summary>
    /// reset NodeMCU devkit - compatible with devices using rts/dtr reset/programming circuit
    /// @see https://github.com/nodemcu/nodemcu-devkit-v1.0/blob/master/NODEMCU_DEVKIT_V1.0.PDF Page #3
    /// Reset => DTR=1, RTS=0
    /// </summary>
    public void HardReset()
    {
      // Pull down RST pin using the reset circuit
      this.port.DtrEnable = false;
      this.port.RtsEnable = true;

      // 100ms delay
      Thread.Sleep(100);

      // Restore previous state after 100ms
      this.port.DtrEnable = false;
      this.port.RtsEnable = false;
      ClearBuffers();
    }

    /// <summary>
    /// Software Reset using node.restart() command
    /// </summary>
    public void SoftReset()
    {
      WriteLine(LuaCode.SOFT_RESTART);
    }

    /// <summary>
    /// Clears the input and output buffers
    /// </summary>
    public void ClearBuffers()
    {
      try
      {
        this.port.DiscardInBuffer();
        this.port.DiscardOutBuffer();
      }
      catch
      {
      }
    }

    /// <summary>
    /// Will wait for exp to be returned from nodemcu or timeout
    /// </summary>
    /// <param name="exp">Expected string</param>
    /// <param name="timeout">Read timeout</param>
    /// <returns></returns>
    protected ReceivedData Expect(string exp = EOL, int? timeout = null)
    {
      int read_timeout = (timeout ?? this.Timeout);
      this.port.ReadTimeout = read_timeout * 1000;

      try
      {
        int b;
        StringBuilder sb = new StringBuilder(RECEIVE_BUFFER_SIZE);
        while(!StringBuilderEndsWith(sb, exp))
        {
          b = this.port.ReadByte();
          if(b == -1)
            break;

          sb.Append((char)b);
        }

        var data = sb.ToString();
        log.Debug($"Expect returned: '{data}'");

        if(!data.EndsWith(exp) && exp.Length > 0)
          throw new BadResponseException("Bad response.", exp, data);

        ReceivedData result = new ReceivedData();

        var lines = SplitLines(data);
        if(lines.Length > 0 && this.EchoEnabled)
          result.Echo = lines[0];

        // Build response by skipping first line (echo) and last line (exp)
        StringBuilder sb2 = new StringBuilder(data.Length);
        int start = (this.EchoEnabled ? 1 : 0);
        for(int i = start; i < lines.Length - 1; i++)
        {
          if(i > start)
            sb2.Append(CRLF);

          sb2.Append(lines[i]);
        }

        result.Response = sb2.ToString();
        return result;
      }
      finally
      {
        // Restore read timeout
        if(read_timeout != this.Timeout)
          this.port.ReadTimeout = this.Timeout * 1000;
      }
    }

    protected bool StringBuilderEndsWith(StringBuilder sb, string text)
    {
      var sb_length   = sb.Length;
      var text_length = text.Length;
      if(sb_length < text_length)
        return false;

      for(int i = 1; i <= text_length; i++)
      {
        if(text[text_length - i] != sb[sb_length - i])
          return false;
      }

      return true;
    }

    /// <summary>
    /// Write data on the nodemcu port
    /// </summary>
    /// <param name="output"></param>
    public void Write(string output)
    {
      log.Debug("Write: " + output);
      this.port.Write(output);
      this.port.BaseStream.Flush();
    }

    /// <summary>
    /// Write, with linefeed
    /// </summary>
    /// <param name="output"></param>
    public void WriteLine(string output)
    {
      Write(output + '\n');
    }

    /// <summary>
    /// Write output to the port and wait for response
    /// </summary>
    /// <param name="output"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public ReceivedData Exchange(string output, int? timeout = null)
    {
      WriteLine(output);
      return Expect(timeout: timeout);
    }

    /// <summary>
    /// Restores the nodemcu to default baudrate and then closes the port
    /// </summary>
    public void Close()
    {
      try
      {
        this.port.BaseStream.Flush();
        ClearBuffers();
      }
      catch
      {
      }

      log.Debug("Closing port");
      this.port.Close();
    }

    /// <summary>
    /// Format FS
    /// </summary>
    public string Format()
    {
      ClearBuffers();
      log.Info("Formatting FS");

      var data = Exchange(LuaCode.FORMAT, 30);
      log.Debug(data.Response);

      return data.Response;
    }

    /// <summary>
    /// Check for existence of file on device
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public bool ExistsFile(string filename)
    {
      ClearBuffers();
      log.Info($"Checking file exists {filename}");

      var data = Exchange(String.Format(LuaCode.FILE_EXISTS, filename));
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error checking file exists: {data.Response}");
        throw new CommunicationException("Error checking file exists", data.Response);
      }

      return Convert.ToBoolean(data.Response);
    }

    /// <summary>
    /// Compile file
    /// </summary>
    /// <param name="filename"></param>
    public void Compile(string filename)
    {
      ClearBuffers();
      log.Info($"Compiling file {filename}");

      var data = Exchange(String.Format(LuaCode.COMPILE_FILE, filename));
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error compiling file: {data.Response}");
        throw new CommunicationException("Error compiling file", data.Response);
      }
    }

    /// <summary>
    /// Rename file
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="new_filename"></param>
    public void Rename(string filename, string new_filename)
    {
      ClearBuffers();
      log.Info($"Renaning file {filename} to {new_filename}");

      var data = Exchange(String.Format(LuaCode.RENAME_FILE, filename, new_filename));
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error renaming file: {data.Response}");
        throw new CommunicationException("Error renaming file", data.Response);
      }
    }

    /// <summary>
    /// Remove file
    /// </summary>
    /// <param name="filename"></param>
    public void Remove(string filename)
    {
      ClearBuffers();
      log.Info($"Removing file {filename}");

      var data = Exchange(String.Format(LuaCode.REMOVE_FILE, filename));
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error removing file: {data.Response}");
        throw new CommunicationException("Error removing file", data.Response);
      }
    }

    /// <summary>
    /// List files on the device
    /// </summary>
    /// <returns></returns>
    public FsInfo ReadFileList()
    {
      ClearBuffers();
      log.Info("Listing files");
      string[] parts;
      FsInfo result = new FsInfo();

      var data = Exchange(LuaCode.LIST_FILES);
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error reading file list: {data.Response}");
        throw new CommunicationException("Error reading file list", data.Response);
      }

      log.Debug(data.Response);
      if(!String.IsNullOrWhiteSpace(data.Response))
      {
        var lines = SplitLines(data.Response);
        foreach(var line in lines)
        {
          parts = line.Split('\t');
          result.Files.Add(new FsFileInfo() { Name = parts[0].Trim(), Size = Convert.ToInt32(parts[1]) });
        }
      }

      data = Exchange(LuaCode.PRINT_FS_INFO);
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error reading FS Info: {data.Response}");
        throw new CommunicationException("Error reading FS Info", data.Response);
      }

      parts = data.Response.Split('\t');
      if(parts.Length == 3)
      {
        int value;
        if(Int32.TryParse(parts[0].Trim(), out value))
          result.FreeBytes = value;

        if(Int32.TryParse(parts[1].Trim(), out value))
          result.UsedBytes = value;

        if(Int32.TryParse(parts[2].Trim(), out value))
          result.TotalBytes = value;
      }

      return result;
    }

    /// <summary>
    /// Read NodeMCU info
    /// </summary>
    /// <returns></returns>
    public NodeInfo ReadInfo()
    {
      ClearBuffers();
      log.Info("Reading info");

      var data = Exchange(LuaCode.PRINT_INFO);
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error reading info: {data.Response}");
        throw new CommunicationException("Error reading info", data.Response);
      }

      log.Debug(data.Response);
      if(String.IsNullOrWhiteSpace(data.Response))
      {
        log.Error($"Info response is empty: {data.Response}");
        throw new CommunicationException("Info response is empty", data.Response);
      }

      var parts = data.Response.Split('\t');
      if(parts.Length != 8)
      {
        log.Error($"Error parsing info: {data.Response}");
        throw new CommunicationException("Error parsing info", data.Response);
      }

      return new NodeInfo()
      {
        MajorVer   = parts[0].Trim(),
        MinorVer   = parts[1].Trim(),
        DevVer     = parts[2].Trim(),
        ChipId     = parts[3].Trim(),
        FlashId    = parts[4].Trim(),
        FlashSize  = parts[5].Trim(),
        FlashMode  = parts[6].Trim(),
        FlashSpeed = parts[7].Trim()
      };
    }

    /// <summary>
    /// Read file from device
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="stream"></param>
    /// <param name="report_progress"></param>
    /// <param name="token"></param>
    public void ReadFile(string filename, int size, Stream stream, IProgress<int> report_progress, CancellationToken token)
    {
      ClearBuffers();
      log.Info($"Receiving file {filename}");

      // Create read helper function
      ReceivedData data;
      var cmd = String.Format(LuaCode.READ_HELPER, filename);
      foreach(var line in SplitLines(cmd))
      {
        data = Exchange(line);
        if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
        {
          log.Error($"Error creating read helper function: {data.Response}");
          throw new CommunicationException("Error creating read helper function", data.Response);
        }
      }

      // Call helper function and start reading
      data = Exchange(LuaCode.READ_START);
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error start reading: {data.Response}");
        throw new CommunicationException("Error start reading", data.Response);
      }

      int b, buffer_length = 0, data_start_index;
      int data_start_length = DATA_START.Length, data_start_end_length = DATA_START.Length + DATA_END.Length;
      byte[] buffer = new byte[DOWNLOAD_BUFFER_SIZE]; // Packet size is 1024 bytes

      while(!BufferEndsWith(buffer, buffer_length, EOL))
      {
        if(token.IsCancellationRequested)
        {
          log.Warn($"Cancel request reading file {filename}");
          break;
        }

        b = this.port.ReadByte();
        if(b == -1)
          break;

        buffer[buffer_length++] = (byte)b;

        // Check for DATA_END pattern on the end of readed bytes
        if(BufferEndsWith(buffer, buffer_length, DATA_END))
        {
          // Find position of DATA_START pattern in readed bytes
          data_start_index = BufferIndexOf(buffer, buffer_length, DATA_START);
          if(data_start_index > -1)
          {
            // Full packet was readed, write it to output stream
            stream.Write(buffer, data_start_index + data_start_length, buffer_length - data_start_index - data_start_end_length);
            log.Debug($"{buffer_length} bytes readed");
            buffer_length = 0;
          }

          // Report progress
          report_progress?.Report((int)((double)stream.Length / size * 100.0));
        }
      }

      // Delete helper function
      data = Exchange(LuaCode.READ_END);
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error deleting read helper function: {data.Response}");
        throw new CommunicationException("Error deleting read helper function", data.Response);
      }
    }

    /// <summary>
    /// Write file to device
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="filename"></param>
    /// <param name="report_progress"></param>
    /// <param name="token"></param>
    public void WriteFile(Stream stream, string filename, IProgress<int> report_progress, CancellationToken token)
    {
      ClearBuffers();
      log.Info($"Transferring file {filename}");

      // Size of file
      int size = (int)stream.Length;

      // Create write helper function
      ReceivedData data;
      var cmd = String.Format(LuaCode.WRITE_HELPER, filename);
      foreach(var line in SplitLines(cmd))
      {
        data = Exchange(line);
        if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
        {
          log.Error($"Error creating write helper function: {data.Response}");
          throw new CommunicationException("Error creating write helper function", data.Response);
        }
      }

      int buffer_length, stream_length = (int)stream.Length;
      byte[] buffer = new byte[UPLOAD_PACKET_SIZE];

      // Compute count of packets
      int packets_count = 1;
      int last_packet_size = 0;
      if(stream_length > UPLOAD_PACKET_SIZE)
      {
        packets_count = stream_length / UPLOAD_PACKET_SIZE;
        last_packet_size = stream_length % UPLOAD_PACKET_SIZE;
        if(last_packet_size > 0)
          packets_count++;
      }
      else
      {
        last_packet_size = stream_length;
      }

      int start_packets = (packets_count == 1 ? last_packet_size : UPLOAD_PACKET_SIZE);

      // Delete remote file
      data = Exchange(String.Format(LuaCode.REMOVE_FILE, filename));
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error removing file: {data.Response}");
        throw new CommunicationException("Error removing file", data.Response);
      }

      // Call helper function and start writing
      data = Exchange(String.Format(LuaCode.WRITE_START, packets_count, start_packets, last_packet_size));
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error start writing file: {data.Response}");
        throw new CommunicationException("Error start writing file", data.Response);
      }

      // Read stream and write file to device in packets
      int writed_bytes = 0;
      stream.Position  = 0;
      while((buffer_length = stream.Read(buffer, 0, UPLOAD_PACKET_SIZE)) > 0)
      {
        if(token.IsCancellationRequested)
        {
          log.Warn($"Cancel request writing file {filename}");
          break;
        }

        this.port.Write(buffer, 0, buffer_length);
        string res = this.port.ReadTo(EOL);
        log.Debug(res);

        // Report progress
        writed_bytes += buffer_length;
        report_progress?.Report((int)((double)writed_bytes / size * 100.0));
      }

      // Delete helper function
      data = Exchange(LuaCode.WRITE_END);
      if(data.Response.IndexOf("unexpected") > -1 || data.Response.IndexOf("stdin") > -1)
      {
        log.Error($"Error deleting write helper function: {data.Response}");
        throw new CommunicationException("Error deleting write helper function", data.Response);
      }
    }

    protected int BufferIndexOf(byte[] buffer, int buffer_length, string text)
    {
      int text_length = text.Length;
      if(buffer_length < text_length)
        return -1;

      bool found;
      for(int i = 0; i <= buffer_length - text_length; i++)
      {
        found = true;
        for(int j = 0; j < text_length; j++)
        {
          if(text[j] != buffer[i + j])
          {
            found = false;
            break;
          }
        }

        if(found)
          return i;
      }

      return -1;
    }

    protected bool BufferEndsWith(byte[] buffer, int buffer_length, string text)
    {
      int text_length = text.Length;
      if(buffer_length < text_length)
        return false;

      for(int i = 1; i <= text_length; i++)
      {
        if(text[text_length - i] != buffer[buffer_length - i])
          return false;
      }

      return true;
    }

    protected string[] SplitLines(string text)
    {
      return text.Split(CRLF_SEPARATOR, StringSplitOptions.None);
    }

    private string TraceBuffer(byte[] buffer)
    {
      StringBuilder sb = new StringBuilder(buffer.Length);
      foreach(byte b in buffer)
      {
        sb.Append((char)b);
      }

      return sb.ToString();
    }

    public void Dispose()
    {
      Close();
    }
  }
}
