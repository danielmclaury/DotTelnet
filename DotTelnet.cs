using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class DotTelnet
{

  private static void usage()
  {
    Console.WriteLine("Usage: DotTelnet [server] [port]");
  }



  public static async Task Main(string[] args)
  {
    string server = args[0];
    int port;

    try
    {
      int.TryParse(args[1], out port);
    }
    catch(Exception e)
    {
      Console.WriteLine("Error: " + e.ToString());
      usage();
      return;
    }

    const int CONSOLE_INDEX = 0;
    const int SERVER_INDEX = 1;

    using(TcpClient client = new TcpClient(server, port))
    {
      Console.WriteLine("Connected to " + server + ":" + port.ToString());

      using(StreamReader netStreamReader = new StreamReader(client.GetStream()))
      {
        using(StreamWriter netStreamWriter = new StreamWriter(client.GetStream()))
        {
	  List<Task<string>> readTasks = new List<Task<string>>() { null, null };
	  readTasks[CONSOLE_INDEX] = Console.In.ReadLineAsync();
	  readTasks[SERVER_INDEX] = netStreamReader.ReadLineAsync();

	  while(true)
	  {
	    Task<string> finishedTask = await Task.WhenAny(readTasks);

	    if(finishedTask == readTasks[CONSOLE_INDEX])
	    {
	      netStreamWriter.WriteLine(finishedTask.Result);
	      netStreamWriter.Flush();
	      readTasks[CONSOLE_INDEX] = Console.In.ReadLineAsync();
	    }
	    else if(finishedTask == readTasks[SERVER_INDEX])
	    {
	      string result = finishedTask.Result;

	      if(result == null)
	      {
		Console.WriteLine("Disconnected from server");
		client.Close();
		return;
	      }

	      Console.WriteLine(result);
	      readTasks[SERVER_INDEX] = netStreamReader.ReadLineAsync();
	    }
	  }
        }
      }
    }
  }
}
