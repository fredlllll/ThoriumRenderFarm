﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Thorium.Reflection;
using static Thorium.Shared.JobAndTaskProperties;

namespace Thorium.Shared
{
    public class LightweightTask
    {
        public string JobID { get; set; }
        public JObject JobInformation { get; set; }
        public string ID { get; set; }
        public JObject Information { get; set; }

        public LightweightTask(Task t, Job job)
        {
            JobID = job.ID;
            JobInformation = job.Information;
            ID = t.ID;
            Information = t.Information;
        }

        public LightweightTask() { }

        public AExecutioner GetExecutioner()
        {
            string typeName = GetInfo<string>(ExecutionerType);
            Type t = ReflectionHelper.GetType(typeName);
            var ci = t.GetConstructor(new Type[] { typeof(LightweightTask) });
            return (AExecutioner)ci.Invoke(new object[] { this });
        }

        public T GetInfo<T>(string key)
        {
            var val = Information[key];
            if(val != null && !val.IsNull())
            {
                return val.Value<T>();
            }
            val = JobInformation[key];
            if(val != null && !val.IsNull())
            {
                return val.Value<T>();
            }
            throw new KeyNotFoundException("could not find key " + key + " in task or job info");
        }

        public T GetInfo<T>(string key, T def)
        {
            var val = Information[key];
            if(val != null && !val.IsNull())
            {
                return val.Value<T>();
            }
            val = JobInformation[key];
            if(val != null && !val.IsNull())
            {
                return val.Value<T>();
            }
            return def;
        }
    }
}
