using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Castle.Core.Internal;
using DependencyInjectionContainer.Attribute;
using DependencyInjectionContainer.Exception;
using Moq;

namespace DependencyInjectionContainer
{
    public class DependencyProvider
    {
        private readonly DependenciesConfiguration _dependencyConfiguration;
    //лок определенного блока кода
        private readonly object _syncObject = new object();
        //когда нужно заменять null на другие значения 
        public static Queue<Dependency> Dependencies = new Queue<Dependency>();
        //пересоздать все без null
        public static List<Dependency> resolvedDependencies = new List<Dependency>();
        //если true можно перестраивать корень
        public static bool isCircularShouldBeRecreate;

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
            if (_dependencyConfiguration.IsExcluded(dependency.Type))
            {
                if (dependency.LifeType == LifeType.InstancePerDependency)
                {
                    throw new DependencyException("Circular dependency!");
                }
                 
                return dependency.Instance;
            }

            _dependencyConfiguration.ExcludeType(dependency.Type);
            object result = null;
            if (dependency.LifeType == LifeType.InstancePerDependency)
            {
                result = Creator.GetInstance(dependency.Type, _dependencyConfiguration);
            }
            else if (dependency.LifeType == LifeType.Singleton)
            {
                lock (_syncObject)
                {
                    if (isCircularShouldBeRecreate || dependency.Instance == null)
                    {
                        isCircularShouldBeRecreate = false;
                        result = Creator.GetInstance(dependency.Type, _dependencyConfiguration);
                        dependency.Instance = result;
                    }
                    else
                    {
                        result = dependency.Instance;
                    }
                }
            }
            _dependencyConfiguration.RemoveFromExcluded(dependency.Type);

            resolvedDependencies.Add(dependency);
            return result;
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
                    genericDependency.Instance = Creator.GetInstance(genericType, _dependencyConfiguration);
                }
                
                var tempGenericDependency = new Dependency(genericType, genericDependency.LifeType, genericDependency.Key) { Instance = genericDependency.Instance };
                return tempGenericDependency;
            }
            if (key != null) return GetNamedDependency(@interface, key);
            if (_dependencyConfiguration.TryGet(@interface, out var dependency))
            {
                return dependency;
            }

            if (@interface.GetInterfaces().Length == 1)
            {
                _dependencyConfiguration.TryGet(@interface.GetInterfaces()[0], out var requestedDependency);
                return requestedDependency;
            }

            throw new DependencyException($"Dependency for type {@interface} is not registered");
        }

        public object Resolve(Type @interface, object key = null)
        {
            if (typeof(IEnumerable).IsAssignableFrom(@interface))
            {
                return ResolveAll(@interface.GetGenericArguments()[0]);
            }

            var dependency = GetDependency(@interface, key);
            var result = ResolveDependency(dependency);
            if (result == null)
            {
                //если пустая очередь или крайний элемент очереди не такой же
                if (Dependencies.IsNullOrEmpty() || Dependencies.Peek().Type != dependency.Type)
                    Dependencies.Enqueue(dependency);
                //циркулярка и на это время она null
                return null;
            }

            //очищаем , чтобы мы могли использовать циркулярные типы
            _dependencyConfiguration.ClearExcluded();
            //если нет циркурной зависимости
            if (Dependencies.IsNullOrEmpty())
            {
                return result;
            }

            //перестройка дерева с первой циркулярки
            if (dependency.Type != Dependencies.Peek().Type)
            {
                return result;
            }

            var nullCircular = Dependencies.Dequeue();
            //очистка все не циркулярные зависим
            foreach (var resolvedDependency in resolvedDependencies)
            {
                if (resolvedDependency.Type != nullCircular.Type)
                {
                    resolvedDependency.Instance = null;
                }
            }

            isCircularShouldBeRecreate = true;

            return Resolve(@interface);
        }
    }
}