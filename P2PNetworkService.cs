using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HW2_BlockChain
{
    public class P2PNetworkService
    {
        private readonly Blockchain _blockChainService;
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _serverCancellation;

        private readonly ConcurrentDictionary<string, int> _peerStrikes = new();
        private const int MaxStrikes = 3;

        public P2PNetworkService(Blockchain blockChainService, int port)
        {
            _blockChainService = blockChainService;
            _port = port;
        }

        public async Task StartAsync()
        {
            if (_listener != null)
            {
                Console.WriteLine("P2P node is already running.");
                return;
            }

            _serverCancellation = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            Console.WriteLine($"P2P node started on port {_port}.");

            while (!_serverCancellation.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync(_serverCancellation.Token);
                    _ = Task.Run(() => HandleClientAsync(client));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"P2P listener error: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            _serverCancellation?.Cancel();
            _listener?.Stop();
            _listener = null;
            Console.WriteLine("P2P node stopped.");
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            string peerIp = GetPeerIp(client);

            if (IsPeerBanned(peerIp))
            {
                Console.WriteLine($"[Firewall] Заблоковано пакет від шкідливого піра: {peerIp}");
                client.Close();
                return;
            }

            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string? rawMessage = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(rawMessage))
                    {
                        AddStrike(peerIp, 1, "empty message");
                        return;
                    }

                    P2PMessage? message;

                    try
                    {
                        message = JsonSerializer.Deserialize<P2PMessage>(rawMessage);
                    }
                    catch (JsonException)
                    {
                        AddStrike(peerIp, 1, "invalid JSON");
                        return;
                    }

                    if (message == null || string.IsNullOrWhiteSpace(message.Type))
                    {
                        AddStrike(peerIp, 1, "invalid P2P message format");
                        return;
                    }

                    if (message.Type.Equals("BLOCK", StringComparison.OrdinalIgnoreCase))
                    {
                        Block? block;

                        try
                        {
                            block = message.Payload.Deserialize<Block>();
                        }
                        catch (JsonException)
                        {
                            AddStrike(peerIp, 1, "invalid block JSON");
                            return;
                        }

                        if (block == null || !_blockChainService.TryAddBlockFromPeer(block))
                        {
                            AddStrike(peerIp, 2, "fake or invalid block");
                            return;
                        }

                        Console.WriteLine($"[P2P] Valid block #{block.Index} accepted from {peerIp}.");
                    }
                    else
                    {
                        AddStrike(peerIp, 1, $"unknown message type: {message.Type}");
                    }
                }
            }
            catch (Exception ex)
            {
                AddStrike(peerIp, 1, $"network error: {ex.Message}");
            }
        }

        private string GetPeerIp(TcpClient client)
        {
            if (client.Client.RemoteEndPoint is IPEndPoint endpoint)
            {
                return endpoint.Address.MapToIPv4().ToString();
            }

            return "unknown";
        }

        private bool IsPeerBanned(string peerIp)
        {
            return _peerStrikes.TryGetValue(peerIp, out int strikes) && strikes >= MaxStrikes;
        }

        private void AddStrike(string peerIp, int points, string reason)
        {
            int newValue = _peerStrikes.AddOrUpdate(
                peerIp,
                points,
                (_, current) => current + points
            );

            string status = newValue >= MaxStrikes ? "Забанений" : "Активний";
            Console.WriteLine($"[Firewall] {peerIp} +{points} strike(s). Reason: {reason}. Total: {newValue}/{MaxStrikes}. Status: {status}");
        }

        public List<PeerFirewallStatus> GetFirewallBlacklist()
        {
            return _peerStrikes
                .OrderByDescending(peer => peer.Value)
                .Select(peer => new PeerFirewallStatus
                {
                    IpAddress = peer.Key,
                    Strikes = peer.Value,
                    Status = peer.Value >= MaxStrikes ? "Забанений" : "Активний"
                })
                .ToList();
        }

        public void ShowFirewallBlacklist()
        {
            List<PeerFirewallStatus> peers = GetFirewallBlacklist();

            Console.WriteLine("===== FIREWALL BLACKLIST =====");

            if (peers.Count == 0)
            {
                Console.WriteLine("No peers with strikes yet.");
                return;
            }

            foreach (PeerFirewallStatus peer in peers)
            {
                Console.WriteLine($"IP: {peer.IpAddress} | Strikes: {peer.Strikes}/{MaxStrikes} | Status: {peer.Status}");
            }
        }
    }
}
