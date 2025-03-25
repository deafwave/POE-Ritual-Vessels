using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using Color = SharpDX.Color;

namespace RitualVessels;

public class RitualVesselsSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new(false);
}