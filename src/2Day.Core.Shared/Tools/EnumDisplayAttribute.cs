using System;

namespace Chartreuse.Today.Core.Shared.Tools
{
    public class EnumDisplayAttribute : Attribute
    {
        private readonly string resourceKey;

        public string ResourceKey
        {
            get { return this.resourceKey; }
        }

        public EnumDisplayAttribute(string resourceKey)
        {
            this.resourceKey = resourceKey;
        }
    }
}