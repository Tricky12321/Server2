using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;
public static class SynchronousSocketListener
{

    public static List<object> ClassesToHandle = new List<object>();
    // Incoming data from the client.  
    public static string data = null;

    public static object StartListening()
    {
        // Data buffer for incoming data.  
        byte[] bytes = new Byte[1024];
        object output = new object();
        // Establish the local endpoint for the socket.  
        // Dns.GetHostName returns the name of the   
        // host running the application.  
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and   
        // listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.  
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();
                data = null;

                // An incoming connection needs to be processed.  
                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                    
                }
                Console.WriteLine("Text received : {0}", data);

                // Show the data on the console.  
                if (data.Substring(0, data.IndexOf("\n")) == "object")
                {

                }
                
                output = HandleCommand(data);
                // Console.WriteLine("Text sent     : {0}", output);
                // Echo the data back to the client.  
                byte[] msg = Encoding.ASCII.GetBytes("1");

                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        return output;

    }

    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }

    public static string HandleCommand(string command)
    {
        command = command.Replace("<EOF>", "");
        string CmdType = command.Substring(0, command.IndexOf("\n"));
        command = command.Substring(command.IndexOf("\n"));
        if (CmdType == "object")
        {
            int index = command.LastIndexOf("\n");
            int length = (command.Length - index);
            string type = command.Substring(index, length).Substring(3);
            type = type.Substring(0, type.Length - 1);
            HandleObjectSend(type, command);
            return null;
        }
        else
        {
            switch (command)
            {
                case "[OPEN]":
                    return "Connection Accepted!";
                default:
                    return "Unknown command";
            }
        }
    }

    public static T XmlDeserializeFromString<T>(this string objectData)
    {
        return (T)XmlDeserializeFromString(objectData, typeof(T));
    }

    public static object XmlDeserializeFromString(this string objectData, Type type)
    {
        var serializer = new XmlSerializer(type);
        object result;

        using (TextReader reader = new StringReader(objectData))
        {
            result = serializer.Deserialize(reader);
        }

        return result;
    }

    public static void HandleObjectSend(string type, string content)
    {
        switch (type)
        {
            case "Person":
                Person PersonFromNetwork = XmlDeserializeFromString<Person>(content);
                break;
            default:
                throw new NoObjectFound();
        }
    }
}


public class NoObjectFound : Exception
{

}
public class Person
{
    public string Name;
    public string Efternavn;
}
