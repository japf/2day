using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Chartreuse.Today.Core.Shared.Annotations;
using Chartreuse.Today.Core.Shared.Tools.Tracking;

namespace Chartreuse.Today.Core.Shared.Tools
{
    // original code from: http://msmvps.com/blogs/vcsjones/archive/2010/11/25/a-really-super-light-and-simple-ioc-container-for-windows-phone-7.aspx
    public static class Ioc
    {
        private static readonly Dictionary<Type, object> Rot = new Dictionary<Type, object>();
        private static readonly object SyncLock = new object();

        public static void RemoveInstance<T>()
        {
            if (Rot.ContainsKey(typeof(T)))
                Rot.Remove(typeof(T));
        }

        public static bool HasType<T>()
        {
            return Rot.ContainsKey(typeof (T));
        }

        public static bool HasType(Type type)
        {
            return Rot.ContainsKey(type);
        }

        [DebuggerStepThrough]
        private static object Resolve([NotNull] Type type, string memberName = null, string filePath = null, int lineNumber = 0)
        {
            if (type == null) 
                throw new ArgumentNullException("type");

            lock (SyncLock)
            {
                if (!Rot.ContainsKey(type))
                {
                    throw new Exception(string.Format("Type: {0} not registered (types count: {1}, member name: {2}, file path: {3}, line number: {4})", type.Name, Rot.Count, memberName, filePath, lineNumber));
                }
                return Rot[type];
            }
        }

        public static object Resolve(Type type)
        {
            return Resolve(type, null, null, 0);
        }

        [DebuggerStepThrough]
        public static T Resolve<T>([CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return (T)Resolve(typeof(T), memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Creates and returns a new instance of the given type and resolve constructor parameters automatically
        /// </summary>
        /// <typeparam name="T">Type of the object to create</typeparam>
        /// <returns>A newly created instance of T</returns>
        public static T Build<T>()
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
                if (!Rot.ContainsKey(parameterType))
                {
                    throw new InvalidOperationException("Cannot resolve dependency");
                }
                var parameterInstance = Rot[parameterType];

                args.Add(parameterInstance);
            }

            var serviceInstance = constructor.Invoke(args.ToArray());

            return (T)serviceInstance;
        }
        
        public static C RegisterInstance<I, C>(C instance) where C : class, I
        {
            lock (SyncLock)
            {
                if (!Rot.ContainsKey(typeof (I)))
                {
                    Rot.Add(typeof (I), instance);
                }
                else
                {
                    TrackingManagerHelper.Exception(new ArgumentException("Duplicate key"), string.Format("Type {0} was already registered", typeof(I).Name));
                    return (C) Rot[typeof (I)];
                }

                return instance;
            }
        }       
    }
}
