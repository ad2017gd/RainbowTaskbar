using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowTaskbar.Helpers {
    public class Cache<T> {
        public T Value { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromDays(1);
        public bool NeedsUpdate { get => DateTime.Now >= LastUpdated + UpdateInterval; }
        public Cache(T value, TimeSpan interval) { Value = value; UpdateInterval = interval; }
    }
}
