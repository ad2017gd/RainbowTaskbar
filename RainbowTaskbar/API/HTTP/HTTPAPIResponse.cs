using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace RainbowTaskbar.API.HTTP;

public abstract class HTTPAPIResponse {
    protected HTTPAPIResponse(bool success) {
        Success = success;
    }

     public bool Success { get; }

     public abstract object Data { get; }

    public string Name {
        get => Regex.Replace(GetType().Name.Replace("Response", ""), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))",
            " $0").TrimStart();
    }

    public static IEnumerable<Type> GetKnownResponseTypes() {
        return Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(HTTPAPIResponse).IsAssignableFrom(type)).ToList();
    }
}