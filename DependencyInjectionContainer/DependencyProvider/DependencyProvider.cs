using DependencyInjectionContainer.CustomAttributes;
using DependencyInjectionContainer.CustomExceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public class DependencyProvider
    {
        static object locker = new object();
        private readonly DependencyConfig _configuration;
        public readonly Dictionary<Type, List<SingletonContainer>> _singletons;
        private readonly Stack<Type> _recursionStack = new Stack<Type>();
        private Dictionary<Type, Type> nullDictionary = new Dictionary<Type, Type>();

        private static List<object> toFill = new List<object>();

        private void fillWithSingleton(object singleton)
        {
            foreach (object o in toFill)
            {
                var fields = o.GetType().GetFields();
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType.IsAssignableFrom(singleton.GetType()))
                    {
                        field.SetValue(o, singleton);
                    }
                }
                var properties = o.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType.IsAssignableFrom(singleton.GetType()) && property.CanWrite)
                    {
                        property.SetValue(o, singleton);
                    }
                }

            }
        }
        public DependencyProvider(DependenciesConfiguration dependencyConfiguration)
        {
            _dependencyConfiguration = dependencyConfiguration;
        }

        internal object Resolve(ParameterInfo parameter)
        {
            var name = parameter.GetCustomAttribute<DependencyKeyAttribute>()?.Key;
            return Resolve(parameter.ParameterType, name);
        }
        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        public TInterface Resolve<TInterface>(object name)
        {
            return (TInterface)Resolve(typeof(TInterface), name);
        }

        public object Resolve(Type @interface, object key = null)
        {
            if (typeof(IEnumerable).IsAssignableFrom(@interface))
            {
                return ResolveAll(@interface.GetGenericArguments()[0]);
            }
            var dependency = GetDependency(@interface, key);

            return ResolveDependency(dependency);
        }

        public IEnumerable<T> ResolveAll<T>()
            where T : class
        {
            return (IEnumerable<T>)ResolveAll(typeof(T));
        }

        public IEnumerable<object> ResolveAll(Type @interface)
        {
            if (_dependencyConfiguration.TryGetAll(@interface, out var dependencies))
            {
                var collection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(@interface));

                foreach (var dependency in dependencies)
                {
                    collection.Add(ResolveDependency(dependency));
                }

                return (IEnumerable<object>)collection;
            }

            return null;
        }

        private object ResolveDependency(Dependency dependency)
        {
            foreach (KeyValuePair<Type, Type> keyValuePair in nullDictionary)
            {
                if (replaceType.Equals(keyValuePair.Value))
                {
                    object objectWithNull = Resolve(keyValuePair.Key, ImplNumber.Any);
                    PropertyInfo[] propertyInfos = objectWithNull.GetType().GetProperties();
                    for (int i = 0; i < propertyInfos.Length; i++)
                    {
                        if (propertyInfos[i].PropertyType.Equals(keyValuePair.Value))
                        {
                            _recursionStack.Pop();
                            object replaceObject = Resolve(replaceType, ImplNumber.Any);
                            Console.WriteLine(replaceObject == null);
                            objectWithNull.GetType().GetProperty(propertyInfos[i].Name).SetValue(objectWithNull, replaceObject);
                            break;
                        }
                    }
                }
            }
        }


        private Dependency GetNamedDependency(Type @interface, object key)
        {
            if (_dependencyConfiguration.TryGetAll(@interface, out var namedDependencies))
            {
                foreach (var dependency in namedDependencies)
                {
                    if (key.Equals(dependency.Key)) return dependency;
                }
            }

            throw new DependencyException($"Dependency with [{key}] key for type {@interface} is not registered");
        }

        private Dependency GetDependency(Type @interface, object key = null)
        {
            if (@interface.IsGenericType &&
                _dependencyConfiguration.TryGet(@interface.GetGenericTypeDefinition(), out var genericDependency))
            {
                if (key != null)
                {
                    genericDependency = GetNamedDependency(@interface.GetGenericTypeDefinition(), key);
                }

                var genericType = genericDependency.Type.MakeGenericType(@interface.GenericTypeArguments);
                if (genericDependency.Instance == null)
                {
                    genericDependency.Instance = Creator.CreateInstance(genericType, _dependencyConfiguration);
                }
                
                var tempGenericDependency = new Dependency(genericType, genericDependency.LifeCycle, genericDependency.Key)
                { 
                    Instance = genericDependency.Instance 
                };

                return tempGenericDependency;
            }

            if (key != null) return GetNamedDependency(@interface, key);

            if (_dependencyConfiguration.TryGet(@interface, out var dependency))
            {
                return dependency;
            }

            throw new DependencyException($"Dependency for type {@interface} is not registered");
        }
    }
}
