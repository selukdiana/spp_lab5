using DependencyInjectionContainer.CustomExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainer
{
    public static class Creator
    {
/*
        public static object CreateSingleton(Dependency dependency, DependenciesConfiguration dependencyConfiguration)
        {
            var ctor = getConstrZeroArgs(dependency.Type);
            dependency.Instance = ctor.Invoke(new object[0]);

            var fields = dependency.Type.GetFields();
            var values = ProvideFields(fields, dependencyConfiguration).ToArray();

            for (int i =  0; i < fields.Length; i++)
            {
                fields[i].SetValue(dependency.Instance, values[i]);
            }
            return dependency.Instance;
        }
        private static ConstructorInfo getConstrZeroArgs(Type type)
        {
            return type.GetConstructor(new Type[0]);
        }*/
        public static object CreateInstance(Type type, DependenciesConfiguration dependencyConfiguration)
        {

            var constructors = ChooseConstructors(type).ToList();
            if (constructors.Count == 0) throw new CreatorException($"{type} has no injectable constructor");
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var arguments = ProvideParameters(parameters, dependencyConfiguration);
                return constructor.Invoke(arguments.ToArray());
            }

            throw new CreatorException($"Can't create instance of {type}");
        }

        private static IEnumerable<object> ProvideParameters(IEnumerable<ParameterInfo> parameters,
            DependenciesConfiguration dependencyConfiguration)
        {
            var provider = new DependencyProvider(dependencyConfiguration);
            return parameters.Select(provider.Resolve);
        }
/*        private static IEnumerable<object> ProvideFields(IEnumerable<FieldInfo> fields, DependenciesConfiguration dependencyConfiguration)
        {

            var provider = new DependencyProvider(dependencyConfiguration);
            return fields.Select(provider.Resolve);
        }*/
            
        private static IEnumerable<ConstructorInfo> ChooseConstructors(Type type)
        {
            return type.GetConstructors()
                .Where(HasConstructedParameters);
        }

        private static bool HasConstructedParameters(ConstructorInfo constructor)
        {
            return constructor.GetParameters()
                .All(IsParameterConstructable);
        }

        private static bool IsParameterConstructable(ParameterInfo parameter)
        {
            var parameterType = parameter.GetType();
            return parameterType.IsClass;
        }
    }
}