using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ultra.Core;

public class V8InspectorClient : IDisposable
{
    private readonly ClientWebSocket _ws = new();
    private readonly string _ip;
    private readonly int _port;
    private int _commandId = 0;

    public bool IsConnected => _ws.State == WebSocketState.Open;

    public V8InspectorClient(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }

    public static async Task<bool> IsInspectorAvailableAsync(string ip, int port, int timeoutMs = 200, CancellationToken ct = default)
    {
        var wsUrl = await TryGetWebSocketDebuggerUrlAsync(ip, port, timeoutMs, ct);
        return !string.IsNullOrEmpty(wsUrl);
    }

    private static async Task<string?> TryGetWebSocketDebuggerUrlAsync(string ip, int port, int timeoutMs, CancellationToken ct)
    {
        try
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(timeoutMs),
            };

            var url = $"http://{ip}:{port}/json";
            var jsonList = await httpClient.GetStringAsync(url, ct);

            var targets = JsonNode.Parse(jsonList)?.AsArray();
            var wsUrl = targets?
                .FirstOrDefault(t => t?["webSocketDebuggerUrl"] != null)?["webSocketDebuggerUrl"]?
                .ToString();

            return string.IsNullOrEmpty(wsUrl) ? null : wsUrl;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 连接到游戏的 V8 Inspector 端口
    /// </summary>
    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        try
        {
            var wsUrl = await TryGetWebSocketDebuggerUrlAsync(_ip, _port, timeoutMs: 2000, ct);
            if (string.IsNullOrEmpty(wsUrl))
            {
                return false;
            }

            await _ws.ConnectAsync(new Uri(wsUrl), ct);
            await SendCommandAsync("Profiler.enable", ct);
            await SendCommandAsync("Profiler.setSamplingInterval", ct, new { interval = 100 }); 

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task StartProfilingAsync(CancellationToken ct = default)
    {
        _ = await SendCommandAsync("Profiler.start", ct);
    }

    /// <summary>
    /// 停止录制并返回 .cpuprofile 的 JSON 内容
    /// </summary>
    public async Task<string?> StopProfilingAsync(CancellationToken ct = default)
    {
        // 发送停止指令，V8 会在响应中返回巨大的 Profile 数据
        var responseJson = await SendCommandAsync("Profiler.stop", ct);

        if (string.IsNullOrEmpty(responseJson)) return null;

        // 解析响应: { "id": 2, "result": { "profile": { ... } } }
        var root = JsonNode.Parse(responseJson);
        var profileNode = root?["result"]?["profile"];

        // 直接返回 profile 部分的 JSON 字符串，这就是标准的 .cpuprofile 格式
        return profileNode?.ToJsonString();
    }

    private async Task<string> SendCommandAsync(string method, CancellationToken ct, object? parameters = null)
    {
        if (_ws.State != WebSocketState.Open) return string.Empty;

        _commandId++;
        var request = new
        {
            id = _commandId,
            method = method,
            @params = parameters
        };

        var requestJson = JsonSerializer.Serialize(request);
        var requestBytes = Encoding.UTF8.GetBytes(requestJson);

        await _ws.SendAsync(requestBytes, WebSocketMessageType.Text, true, ct);

        // 接收响应（处理分片/大包）
        // V8 Profiler 的数据可能高达几 MB，必须循环接收
        using var ms = new MemoryStream();
        var buffer = new byte[81920]; // 80KB buffer
        WebSocketReceiveResult result;
        do
        {
            result = await _ws.ReceiveAsync(buffer, ct);
            ms.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);

        ms.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(ms, Encoding.UTF8);
        return await reader.ReadToEndAsync(ct);
    }

    public void Dispose()
    {
        try { _ws.Dispose(); } catch { }
    }
}
