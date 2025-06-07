using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace amaz
{
    public abstract class BaseService : IDisposable
    {
        public abstract void Dispose();
    }
}

