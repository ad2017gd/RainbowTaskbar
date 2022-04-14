using FastMember;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;

namespace RainbowTaskbar.Configuration
{
    [DataContract]
    [KnownType("GetKnownInstructionTypes")]
    public abstract class Instruction : INotifyPropertyChanged
    {
        public static IEnumerable<Type> InstructionTypes { get; set; } = GetKnownInstructionTypes();
        public static IEnumerable<Type> DisplayableInstructionTypes { get => InstructionTypes.Skip(1); }

        public event PropertyChangedEventHandler PropertyChanged;


        public static IEnumerable<Type> GetKnownInstructionTypes()
        {
            if (InstructionTypes == null)
            {
                InstructionTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(Instruction).IsAssignableFrom(type)).ToList();
            }

            return InstructionTypes;
        }

        public bool Execute(Taskbar window)
        {
            return Execute(window, CancellationToken.None);
        }

        public abstract bool Execute(Taskbar window, CancellationToken token);

        public abstract JObject ToJSON();

        public static Instruction FromJSON(Type type, JObject json)
        {
            dynamic inst = type.GetConstructor(Array.Empty<Type>()).Invoke(null) as Instruction;


            foreach(JProperty prop in json.Properties())
            {
                if(prop.Name != "Name" && prop.Name != "Position")
                {
                    var wrapped = ObjectAccessor.Create(inst);
                    if(prop.Name.StartsWith("Color"))
                    {
                        wrapped[prop.Name] = System.Drawing.ColorTranslator.FromHtml(prop.Value.Value<string>());
                    } else
                        wrapped[prop.Name] = Convert.ChangeType(prop.Value, wrapped[prop.Name].GetType());
                }
            }

            return inst;
        }

        public string Name { get => Regex.Replace(GetType().Name.Replace("Instruction", ""), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0").TrimStart(); }
    }
}
