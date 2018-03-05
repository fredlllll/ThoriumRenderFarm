﻿namespace Thorium_Shared
{
    public abstract class AExecutioner
    {
        public LightweightTask Task { get; protected set; }

        public AExecutioner(LightweightTask t) {
            Task = t;
        }

        public abstract void Execute();
    }
}
