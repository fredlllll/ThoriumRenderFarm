﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Codolith.Config;

namespace Thorium_Shared
{
    [DataContract]
    public class TaskInformation
    {
        [DataMember]
        public string JobID { get; set; }
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public Config Config { get; } = new Config();

        public Type TaskType
        {
            get
            {
                return Type.GetType(Config.Get(nameof(TaskType)));
            }
            set
            {
                if(!value.IsSubclassOf(typeof(ATask)))
                {
                    throw new ArgumentException("the type has to be a subclass of " + nameof(ATask));
                }
                Config.Set(nameof(TaskType), value.AssemblyQualifiedName);
            }
        }
    }
}