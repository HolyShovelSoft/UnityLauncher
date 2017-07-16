using UnityLauncher.Interfaces;

namespace UnityLauncher.Core
{
    public abstract class BaseModel
    {
        protected IContext Context { get; private set; }

        public virtual void Init(IContext context)
        {
            Context = context;
        }

        protected BaseModel() { }
    }
}