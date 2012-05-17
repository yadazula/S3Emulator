using System.Runtime.InteropServices;

namespace S3Emulator
{
  public class ConsoleEventListener
  {
    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(ConsoleEventHandler handler, bool add);
    private delegate bool ConsoleEventHandler(CtrlType sig);
    private static ConsoleEventHandler handler;
    private enum CtrlType
    {
      CTRL_C_EVENT = 0,
      CTRL_CLOSE_EVENT = 2,
      CTRL_LOGOFF_EVENT = 5,
      CTRL_SHUTDOWN_EVENT = 6
    }

    public static void Listen()
    {
      handler += OnConsoleEvent;
      SetConsoleCtrlHandler(handler, true);
    }

    private static bool OnConsoleEvent(CtrlType signal)
    {
      switch (signal)
      {
        case CtrlType.CTRL_C_EVENT:
        case CtrlType.CTRL_LOGOFF_EVENT:
        case CtrlType.CTRL_SHUTDOWN_EVENT:
        case CtrlType.CTRL_CLOSE_EVENT:
          if (Program.S3Server != null) Program.S3Server.Dispose();
          break;
      }

      return true;
    }
  }
}