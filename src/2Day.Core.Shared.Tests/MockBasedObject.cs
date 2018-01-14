using System;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace Chartreuse.Today.Core.Shared.Tests
{
    public struct MockBasedObject<T>
    {
        private readonly List<object> parameters;

        public T Object { get; private set; }

        public MockBasedObject(T instance, List<object> parameters)
            : this()
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            this.Object = instance;
            this.parameters = parameters;
        }

        public Mock<TParam> GetParameter<TParam>() where TParam : class
        {
            return (Mock<TParam>)this.parameters.First(p => (p as Mock<TParam>) != null);
        }
    }
}