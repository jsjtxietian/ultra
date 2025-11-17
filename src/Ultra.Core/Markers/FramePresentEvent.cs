// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Text.Json;

namespace Ultra.Core.Markers;

/// <summary>
/// 为 Firefox Profiler 定义一个 DXGI Present 事件 (用于标记帧)
/// </summary>
public class FramePresentEvent : FirefoxProfiler.MarkerPayload
{
    /// <summary>
    /// 事件的类型 ID.
    /// </summary>
    public const string TypeId = "dxgi.present.start"; // 自定义名称

    /// <summary>
    /// Initializes a new instance of the <see cref="FramePresentEvent"/> class.
    /// </summary>
    public FramePresentEvent()
    {
        Type = TypeId;
    }

    /// <summary>
    /// 获取或设置交换链 (SwapChain) 地址.
    /// </summary>
    public ulong SwapChainAddress { get; set; }

    /// <summary>
    /// 获取或设置 Present 标志.
    /// </summary>
    public uint Flags { get; set; }

    /// <inheritdoc />
    protected internal override void WriteJson(Utf8JsonWriter writer, FirefoxProfiler.MarkerPayload payload, JsonSerializerOptions options)
    {
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
            ChartLabel = "Frame Present (SwapChain: {marker.data.swapChainAddress})",
            TableLabel = "Frame Present",

            Display =
            {
                FirefoxProfiler.MarkerDisplayLocation.TimelineOverview, 
                FirefoxProfiler.MarkerDisplayLocation.MarkerChart,
                FirefoxProfiler.MarkerDisplayLocation.MarkerTable,
            },

            Data =
            {
            },
        };
}