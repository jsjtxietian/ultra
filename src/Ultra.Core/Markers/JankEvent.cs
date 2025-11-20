// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Text.Json;

namespace Ultra.Core.Markers;

/// <summary>
/// 为 Firefox Profiler 定义一个 "Jank" (卡顿) 区间事件
/// </summary>
public class JankEvent : FirefoxProfiler.MarkerPayload
{
    /// <summary>
    /// 事件的类型 ID.
    /// </summary>
    public const string TypeId = "CC";  // hack

    /// <summary>
    /// Initializes a new instance of the <see cref="JankEvent"/> class.
    /// </summary>
    public JankEvent()
    {
        Type = TypeId;
    }

    /// <summary>
    /// 获取或设置这一帧的实际耗时 (毫秒).
    /// </summary>
    public double FrameTimeMs { get; set; }

    /// <inheritdoc />
    protected internal override void WriteJson(Utf8JsonWriter writer, FirefoxProfiler.MarkerPayload payload, JsonSerializerOptions options)
    {
        // "duration" 格式会自动处理毫秒的显示
        writer.WriteNumber("frameTimeMs", FrameTimeMs);
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
            // {marker.data.frameTimeMs;duration} 会自动将毫秒格式化为 e.g. "50.2ms"
            ChartLabel = "Jank! {marker.data.frameTimeMs}",
            TableLabel = "Jank Event",

            Display =
            {
                // 让它在时间线概览和 Marker 图表中都可见
                FirefoxProfiler.MarkerDisplayLocation.TimelineOverview,
                FirefoxProfiler.MarkerDisplayLocation.MarkerChart,
                FirefoxProfiler.MarkerDisplayLocation.MarkerTable
            },

            Data =
            {
                new FirefoxProfiler.MarkerDataItem()
                {
                    Format = FirefoxProfiler.MarkerFormatType.Duration, // 使用 "duration" 格式
                    Key = "frameTimeMs",
                    Label = "Frame Time",
                },
            },
        };
}