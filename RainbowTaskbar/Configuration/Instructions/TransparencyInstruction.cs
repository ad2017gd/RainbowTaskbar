﻿using System;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using RainbowTaskbar.Helpers;
using RainbowTaskbar.Languages;

namespace RainbowTaskbar.Configuration.Instructions;

[DataContract]
public class TransparencyInstruction : Instruction {
    public enum TransparencyInstructionStyle {
        Default,
        Blur,
        Transparent
    }

    public enum TransparencyInstructionType {
        Taskbar,
        RainbowTaskbar,
        All,
        Style,
        Layer
    }

    [field: DataMember] public TransparencyInstructionType Type { get; set; }

    [field: DataMember] public double Opacity { get; set; } = 1;

    [field: DataMember] public TransparencyInstructionStyle Style { get; set; }

    [field: DataMember] public int Layer { get; set; }

    public override string Description {
        get {
            /*return Type == TransparencyInstructionType.Style ? $"Taskbar style - {Style.ToString()}" :
                (Type == TransparencyInstructionType.Layer ? $"Layer {Layer} - {Math.Round(Opacity * 100)}% opacity" :
                $"{Type.ToString()} - {Math.Round(Opacity * 100)}% opacity");*/
            if(Type == TransparencyInstructionType.Style) {
                return App.localization.FormatSuffix(this, "style", Style.ToStringLocalized());
            } else if (Type == TransparencyInstructionType.Layer) {
                return App.localization.FormatSuffix(this, "layer", Layer, Math.Round(Opacity * 100));
            } else {
                return App.localization.FormatSuffix(this, "opacity", Type.ToStringLocalized(), Math.Round(Opacity * 100));
            }
                
        }
    }

    public override JObject ToJSON() {
        dynamic data = new ExpandoObject();
        data.Name = GetType().Name;
        data.Opacity = Opacity;
        data.Layer = Layer;
        data.Style = Style;
        data.Type = Type;

        return JObject.FromObject(data);
    }

    public override bool Execute(Taskbar window, CancellationToken token) {
        switch (Type) {
            case TransparencyInstructionType.Taskbar:

                window.taskbarHelper.SetAlpha(Opacity);
                break;

            case TransparencyInstructionType.RainbowTaskbar:
                window.Dispatcher.Invoke(() => {
                    if (window.Opacity != Opacity) window.Opacity = Opacity;
                }, System.Windows.Threading.DispatcherPriority.Normal, token);
                break;

            case TransparencyInstructionType.All:
                window.Dispatcher.Invoke(() => {
                    if (window.Opacity != Opacity) window.Opacity = Opacity;
                }, System.Windows.Threading.DispatcherPriority.Normal, token);
                window.taskbarHelper.SetAlpha(Opacity);
                break;

            case TransparencyInstructionType.Style:

                switch (Style) {
                    case TransparencyInstructionStyle.Default:
                        if (window.taskbarHelper.Style != TaskbarHelper.TaskbarStyle.Default)
                            window.taskbarHelper.Style = TaskbarHelper.TaskbarStyle.ForceDefault;
                        break;
                    case TransparencyInstructionStyle.Blur:
                        window.taskbarHelper.Style = TaskbarHelper.TaskbarStyle.Blur;
                        break;
                    case TransparencyInstructionStyle.Transparent:
                        window.taskbarHelper.Style = TaskbarHelper.TaskbarStyle.Transparent;
                        break;
                }

                break;

            case TransparencyInstructionType.Layer:
                window.Dispatcher.Invoke(() =>
                    window.canvasManager.canvases[Layer].Opacity = Opacity
                    , System.Windows.Threading.DispatcherPriority.Normal, token);
                break;
        }

        return false;
    }
}