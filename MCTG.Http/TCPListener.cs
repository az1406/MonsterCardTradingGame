using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MCTG.Http;

public class TCPListener(RequestExecutor requestExecutor, ILogger<TCPListener> logger) : IHostedService
{
    private const int PORT = 10001;
    private readonly TcpListener _listener = new(IPAddress.Parse("127.0.0.1"), PORT);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleClientAsync(client);
                }
                finally
                {
                    client.Dispose();
                }
            }, cancellationToken);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            var rawRequest = ReadRawRequest(stream);
            logger.LogInformation("Received request from {@ClientRemoteEndPoint}", client.Client.RemoteEndPoint);
            var response = await requestExecutor.ProcessAsync(rawRequest);
            var buffer = Encoding.UTF8.GetBytes(response.ToResponseString());
            await client.GetStream().WriteAsync(buffer, 0, buffer.Length);
            logger.LogInformation("Responded to {@ClientRemoteEndPoint}", client.Client.RemoteEndPoint);
        }
        catch (ObjectDisposedException)
        {
            // Ignore the exception
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the request");
        }
        finally
        {
            client.Close();
        }
    }

    private string[] ReadRawRequest(NetworkStream stream)
    {
        var buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        return request.Split("\r\n");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Stop();
        return Task.CompletedTask;
    }
}