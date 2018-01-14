using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chartreuse.Today.Core.Shared.Model;
using Chartreuse.Today.Core.Shared.Tools;

namespace Chartreuse.Today.App.Test
{
    public static class TestFactoryHelpersNotWorking
    {
        public static T CreateWithMock<T>()
        {
            Type instanciationType = typeof(T);

            // find the first constructor that we can use
            var constructor = instanciationType.GetTypeInfo().DeclaredConstructors.FirstOrDefault();
            if (constructor == null)
            {
                throw new ArgumentException("No constructor found");
            }

            // prepare the list of parameters neeeded to invoke the constructor
            List<object> args = new List<object>();
            var parameters = constructor.GetParameters();
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;

                if (!Ioc.HasType(parameterType))
                {
                    // no mocking framework for UWP so far :-(
                    // var parameterInstance = Mock.Create(parameterType);
                    // Ioc.RegisterInstance(parameterType, parameterInstance);
                }

                args.Add(Ioc.Resolve(parameterType));
            }

            var serviceInstance = constructor.Invoke(args.ToArray());

            return (T)serviceInstance;
        }

        public static void SetupSettingsNotworking()
        {
            // setup settings
            var settings = Ioc.Resolve<IWorkbook>().Settings;
            var dictionary = new Dictionary<string, object>();



            //Mock.Arrange(() => settings.SetValue(Arg.AnyString, Arg.AnyString)).DoInstead((string k, object v) =>
            //{
            //    if (!dictionary.ContainsKey(k))
            //        dictionary.Add(k, v);
            //    else
            //        dictionary[k] = v;
            //});

            //Mock.Arrange(() => settings.GetValue<string>(Arg.AnyString)).Returns((string k) =>
            //{
            //    if (dictionary.ContainsKey(k))
            //        return (string)dictionary[k];

            //    return null;
            //});
        }
    }
}