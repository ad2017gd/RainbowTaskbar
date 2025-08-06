using RainbowTaskbar.Configuration.Instruction.Instructions;
using RainbowTaskbar.Configuration.Instruction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RainbowTaskbar.Languages {
    public class Localization
    {
        public static List<string> languages = new List<string>() { "SystemDefault", "en_US", "zh_CN", "fr_FR", "ro_RO" };
        public ResourceDictionary dictionary;
        public Localization() {
            Switch(CultureInfo.InstalledUICulture.Name);
        }

        public void Switch(string lang) {
            var language = lang is not null ? lang.Replace("-", "_") : null;
            switch (language) {
                case "zh":
                case "zh_CN":
                case "zh_SG":
                    dictionary = new zh_CN();
                    (dictionary as zh_CN).InitializeComponent();
                    break;
                case "fr":
                case "fr_FR":
                    dictionary = new fr_FR();
                    (dictionary as fr_FR).InitializeComponent();
                    break;
                case "ro":
                case "ro_RO":
                case "ro_MD":
                    dictionary = new ro_RO();
                    (dictionary as ro_RO).InitializeComponent();
                    break;
                default:
                    dictionary = new en_US();
                    (dictionary as en_US).InitializeComponent();
                    break;
            }
        }

        public string Get(string key) {
            return dictionary[key] as string;
        }

        public string this[string key] {
            get {
                return dictionary[key] as string;
            }
        }

        public string UseSystemDefaultString { get {
                return this["systemdefault"];
            } }


        public string InstructionFormat(Instruction instruction, params object[] args) {
            return string.Format(Get($"{instruction.GetType().Name.ToLower()}format"), args);
        }
        public string Name(string str) {
            return str is not null ? Get(str.ToLower()) : null;
        }
        public string InstructionFormatSuffix(Instruction instruction, string suffix, params object[] args) {
            return string.Format(Get($"{instruction.GetType().Name.ToLower()}{suffix}format"), args);
        }

        public string Enum(Enum en) {
            return en is not null ? Get($"enum_{en.ToString().ToLower()}") : null;
        }


        public void Enable(Collection<ResourceDictionary> mergedDicts) {
            var idx = mergedDicts.ToList().FindIndex((x) => x.Contains("systemdefault"));
            if (idx > -1) mergedDicts.RemoveAt(idx);
            mergedDicts.Add(dictionary);

        }

    }

    public static class EnumLocalization {
        public static string ToStringLocalized(this Enum shape) {
            return App.localization.Enum(shape);
        }

    }
    
}
