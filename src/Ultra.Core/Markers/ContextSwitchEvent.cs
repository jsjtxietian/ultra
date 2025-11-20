using System.Text.Json;

namespace Ultra.Core.Markers;

public class ContextSwitchEvent : FirefoxProfiler.MarkerPayload
{
    public const string TypeId = "CC";

    public ContextSwitchEvent()
    {
        Type = TypeId;
    }

    // 等待原因 (例如: Executive, UserRequest, EventPairHigh, ...)
    public string? WaitReason { get; set; }

    // 等待模式 (例如: KernelMode, UserMode)
    public string? WaitMode { get; set; }

    // 之前的状态 (例如: Running, Standby, ...)
    public string? OldState { get; set; }

    protected internal override void WriteJson(Utf8JsonWriter writer, FirefoxProfiler.MarkerPayload payload, JsonSerializerOptions options)
    {
        writer.WriteString("waitReason", WaitReason);
        writer.WriteString("waitMode", WaitMode);
        writer.WriteString("oldState", OldState);
    }

    public static FirefoxProfiler.MarkerSchema Schema()
        => new()
        {
            Name = TypeId,
            ChartLabel = "Context Switch: {marker.data.waitReason}",
            TableLabel = "Context Switch: {marker.data.waitReason}",
            Display =
            {
                FirefoxProfiler.MarkerDisplayLocation.TimelineOverview,
                FirefoxProfiler.MarkerDisplayLocation.MarkerChart,
                FirefoxProfiler.MarkerDisplayLocation.MarkerTable
            },
            Data =
            {
                new FirefoxProfiler.MarkerDataItem()
                {
                    Key = "waitReason",
                    Label = "Wait Reason",
                    Format = FirefoxProfiler.MarkerFormatType.String,
                },
                new FirefoxProfiler.MarkerDataItem()
                {
                    Key = "waitMode",
                    Label = "Wait Mode",
                    Format = FirefoxProfiler.MarkerFormatType.String,
                },
                 new FirefoxProfiler.MarkerDataItem()
                {
                    Key = "oldState",
                    Label = "Old State",
                    Format = FirefoxProfiler.MarkerFormatType.String,
                }
            }
        };
}