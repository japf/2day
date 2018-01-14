using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
#pragma warning disable 1570

namespace Chartreuse.Today.Core.Shared.Tests
{
    /// <summary>
    /// An helper class that is able to create an instance of a type by dynamically injecting mock-based object for all
    /// parameters of its constructor. Very helpful in unit-test to create in a single line an instance of a type (typically
    /// the type we want to write test against).
    /// </summary>
    /// <example>
    /// MockBasedObject<MyServiceImplementation> mockBased = MockBasedFactory.Build<MyServiceImplementation>
    /// MyServiceImplementation serviceImplementation = mockBasedObject.Object;
    /// // retrieve parameter
    /// IServiceParameter parameter = mockBasedObject.Get<IServiceParameter>();
    /// </example>
    public static class MockBasedFactory
    {
        /// <summary>
        /// Create an instance of object given as type parameter by using mocks for every parameters of its constructor
        /// </summary>
        /// <typeparam name="T">Type of the object to create</typeparam>
        /// <returns></returns>
        public static MockBasedObject<T> Build<T>(params object[] existingTypes)
        {
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructors()[0];

            List<object> parameterInstances = new List<object>();
            List<object> parameterMocks = new List<object>();
            
            foreach (var parameterInfo in constructor.GetParameters())
            {
                Type parameterType = parameterInfo.ParameterType;
                if (!parameterType.IsInterface)
                {
                    throw new NotSupportedException("Only interface-based parameter can be injected in the constructor");
                }

                object mock = null;
                object mockedObject = existingTypes.FirstOrDefault(o => parameterType.IsAssignableFrom(o.GetType()));
                if (mockedObject == null)
                {
                    // Moq library is usually used with 'var mock = new Mock<T>' here we need the same thing, but we cannot use this API
                    // since T is given as parameter of this method - so we dynamically invoke the appropriate generic constructor
                    var constructorInfo = typeof(Mock<>).MakeGenericType(parameterType).GetConstructor(Type.EmptyTypes);
                    mock = constructorInfo.Invoke(new object[] { });
                    mock.GetType().GetProperties().Single(f => f.Name == "DefaultValue").SetValue(mock, DefaultValue.Mock);

                    mockedObject = mock.GetType().GetProperties().Single(f => f.Name == "Object" && f.PropertyType == parameterType).GetValue(mock, new object[] { });
                }

                if (mock != null)
                    parameterMocks.Add(mock);

                parameterInstances.Add(mockedObject);
            }

            T instance = (T)constructor.Invoke(parameterInstances.ToArray());

            return new MockBasedObject<T>(instance, parameterMocks);
        }
    }
}
