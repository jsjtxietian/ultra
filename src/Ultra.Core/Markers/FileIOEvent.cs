using System.Text.Json;
namespace Ultra.Core.Markers;

/// <summary>
/// 为 Firefox Profiler 定义一个自定义的 File IO 事件
/// </summary>
public class FileIOEvent : FirefoxProfiler.MarkerPayload
{
    /// <summary>
    /// 事件的类型 ID.
    /// </summary>
    public const string TypeId = "dotnet.io.file"; // 你可以自定义这个名字

    /// <summary>
    /// Initializes a new instance of the <see cref="FileIOEvent"/> class.
    /// </summary>
    public FileIOEvent()
    {
        Type = TypeId;
    }

    /// <summary>
    /// 获取或设置文件名.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 获取或设置操作类型 (e.g., Open, Read, Write, Close).
    /// </summary>
    public string? OperationType { get; set; }

    /// <inheritdoc />
    protected internal override void WriteJson(Utf8JsonWriter writer, FirefoxProfiler.MarkerPayload payload, JsonSerializerOptions options)
    {
        writer.WriteString("fileName", FileName);
        writer.WriteString("operationType", OperationType);
    }

    /// <summary>
    /// 返回此 Marker 的 schema.
    /// </summary>
    /// <returns>The marker schema.</returns>
    public static FirefoxProfiler.MarkerSchema Schema()
        => new()
        {
            Name = TypeId,

            // 定义在 UI 上的显示格式
            ChartLabel = "File IO: {marker.data.operationType} - {marker.data.fileName}",
            TableLabel = "File IO: {marker.data.operationType} - {marker.data.fileName}",

            Display =
            {
                FirefoxProfiler.MarkerDisplayLocation.MarkerChart,
                FirefoxProfiler.MarkerDisplayLocation.MarkerTable,
                FirefoxProfiler.MarkerDisplayLocation.TimelineFileio, // TODO: check UI
            },

            Data =
            {
                new FirefoxProfiler.MarkerDataItem()
                {
                    Format = FirefoxProfiler.MarkerFormatType.String,
                    Key = "fileName",
                    Label = "File Name",
                },
                new FirefoxProfiler.MarkerDataItem()
                {
                    Format = FirefoxProfiler.MarkerFormatType.String,
                    Key = "operationType",
                    Label = "Operation",
                },
            },
        };
}