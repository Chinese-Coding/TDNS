using System.Net;
using System.Net.Sockets;
using log4net;
using log4net.Config;
using TDNS.DNSMessages;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace TDNS;

class Program
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

    public static void Main()
    {
        Logger.Info("Hello and welcome to tdns, the teaching authoritative nameserver");
        var a = new Program();
        var tasks = a.LaunchDNSServer([IPAddress.Any, IPAddress.Parse("10.0.0.1")]);
        Task.WaitAll(tasks);
    }

    private List<Task> LaunchDNSServer(List<IPAddress> locals)
    {
        var tasks = new List<Task>();
        foreach (var local in locals)
        {
            var udpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // 设置 SO_REUSEADDR 选项
            udpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpListener.Bind(new IPEndPoint(local, 53));
            Logger.Info($"Listening on UDP on {local}: 53");
            // 创建分离的任务
            var task = Task.Run(() => UDPTask(udpListener)).ContinueWith(t =>
                {
                    if (t.Exception == null) return;
                    foreach (var ex in t.Exception.InnerExceptions)
                        Console.WriteLine($"Exception: {ex.Message}");
                    
                }, TaskContinuationOptions.OnlyOnFaulted
            );
            tasks.Add(task);
        }

        Logger.Info("所有监听器创建完成");
        return tasks;
    }

    private void UDPTask(Socket socket)
    {
        var buffer = new Byte[512];
        EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            var msgLen = socket.ReceiveFrom(buffer, ref remoteEndPoint);
            Logger.Info($"Received from {remoteEndPoint}, {msgLen} bytes");
            var msgReader = new DNSMessageReader(buffer.Take(msgLen).ToArray());
            var dnsHeader = msgReader.dnsHeader;
            var qname = msgReader.qname;
            var recordList = msgReader.recordList;
            Logger.Info(
                $"{dnsHeader.qucount} questions, {dnsHeader.ancount} answers, {dnsHeader.aucount} authority, {dnsHeader.adcount} additional");
            Logger.Info($"{recordList.Count}");
            Logger.Info($"收到的 qname: {qname.ToReadableString()}");
            var ednsOpts = msgReader.GetEDNSOpts();
            if (ednsOpts == null) continue;
            foreach (var option in ednsOpts.options)
            {
                Logger.Info($"EDNS option: {option.Item1}, {BitConverter.ToString(option.Item2)}");
            }
        }
    }
}
