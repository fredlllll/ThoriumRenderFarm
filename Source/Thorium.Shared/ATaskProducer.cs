﻿using System.Collections.Generic;

namespace Thorium_Shared
{
    public abstract class ATaskProducer
    {
        public Job Job { get; protected set; }

        public ATaskProducer(Job job)
        {
            Job = job;
        }

        public abstract IEnumerator<Task> GetTasks();
    }
}