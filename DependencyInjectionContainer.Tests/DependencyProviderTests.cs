using DependencyInjectionContainer.Exception;
using DependencyInjectionContainer.Tests.TestableClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionContainer.Tests
{
    [TestClass]
    public class DependencyProviderTests
    {
        [TestMethod]
        public void GenericResolveSimpleTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IService<IRepository>, ServiceImpl1<MyRepository>>();
            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>();
            var service = container.Resolve<IService<IRepository>>();
            Assert.IsInstanceOfType(repository, typeof(MyRepository));
            Assert.IsInstanceOfType(service, typeof(ServiceImpl1<MyRepository>));
        }

        [TestMethod]
        public void SingletonTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeType.Singleton, "1");
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeType.InstancePerDependency, "2");
            var container = new DependencyProvider(configuration);
            var serviceImpl1 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl2 = container.Resolve<IService<IRepository>>("2");

            var serviceImpl11 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl22 = container.Resolve<IService<IRepository>>("2");

            Assert.IsTrue(serviceImpl1 == serviceImpl11);
            Assert.IsFalse(serviceImpl2 == serviceImpl22);
        }

        [TestMethod]
        public void BaseInterfaceTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register<IBaseService, ServiceImpl1<IRepository>>();
            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IBaseService>();

            Assert.IsInstanceOfType(service, typeof(ServiceImpl1<IRepository>));
        }

        [TestMethod]
        public void ListOfDependenciesWithOpenGenericsTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IRepository), typeof(SomeRepository));
            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();
            Assert.IsTrue(services.Count == 2);

            Assert.IsInstanceOfType(services[0], typeof(MyRepository));
            Assert.IsInstanceOfType(services[1], typeof(SomeRepository));
        }

        [TestMethod]
        public void ListOfDependenciesTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, SomeRepository>();
            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            Assert.IsTrue(services.Count == 2);

            Assert.IsInstanceOfType(services[0], typeof(MyRepository));
            Assert.IsInstanceOfType(services[1], typeof(SomeRepository));
        }

        [TestMethod]
        public void RecursionTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<SelfDependent, SelfDependent>();
            var container = new DependencyProvider(configuration);

            Assert.ThrowsException<DependencyException>(() => container.Resolve<SelfDependent>());
        }

        [TestMethod]
        public void CyclicDependencyTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<Class1, Class1>();
            configuration.Register<Class2, Class2>();
            var container = new DependencyProvider(configuration);
            
            Assert.ThrowsException<DependencyException>(() => container.Resolve<Class1>());
            Assert.ThrowsException<DependencyException>(() => container.Resolve<Class2>());
        }

        [TestMethod]
        public void WrongIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("kek");
            var container = new DependencyProvider(configuration);

            Assert.ThrowsException<DependencyException>(() => container.Resolve<IRepository>("wrong"));
        }

        [TestMethod]
        public void ProvideByIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("my");
            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>("my");

            Assert.IsInstanceOfType(repository, typeof(MyRepository));
        }

        [TestMethod]
        public void ProvidedAttributeTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, MyAnotherRepository>("yes");
            configuration.Register<IService<IRepository>, ServiceImpl3<IRepository>>();
            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IService<IRepository>>();

            Assert.IsInstanceOfType(service, typeof(ServiceImpl3<IRepository>));
            Assert.IsInstanceOfType(((ServiceImpl3<IRepository>)service).Repository, typeof(MyAnotherRepository));
        }

        [TestMethod]
        public void ResolveCircularSingletnDependency()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(Class1), typeof(Class1), LifeType.Singleton);
            configuration.Register(typeof(Class2), typeof(Class2), LifeType.Singleton); 
            var container = new DependencyProvider(configuration);

            var classFirst = container.Resolve<Class1>();
            var classSecond = container.Resolve<Class2>();
            var singltoneTestFirst = container.Resolve<Class1>();
            var singltoneTestSecond = container.Resolve<Class2>();
            
            Assert.IsNotNull(classFirst);
            Assert.IsNotNull(classFirst.class2);
            Assert.IsNotNull(classFirst.class2.class1);

            Assert.IsNotNull(classSecond);
            Assert.IsNotNull(classSecond.class1);
            Assert.IsNotNull(classSecond.class1.class2);
            
            Assert.AreEqual(classFirst, singltoneTestFirst);
            Assert.AreEqual(classSecond, singltoneTestSecond);
        }
        
        [TestMethod]
        public void ResolveCircularThreeLevelSingletnDependency()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(FirstClass), typeof(FirstClass), LifeType.Singleton);
            configuration.Register(typeof(SecondClass), typeof(SecondClass), LifeType.Singleton); 
            configuration.Register(typeof(ThirdClass), typeof(ThirdClass), LifeType.Singleton); 
            var container = new DependencyProvider(configuration);

            var classFirst = container.Resolve<FirstClass>();
            var classSecond = container.Resolve<SecondClass>();
            var classThird = container.Resolve<ThirdClass>();
            
            Assert.IsNotNull(classFirst);
            Assert.IsNotNull(classFirst.SecondClass);
            Assert.IsNotNull(classFirst.SecondClass.ThirdClass);
            Assert.IsNotNull(classFirst.SecondClass.ThirdClass.FirstClass);
            
            Assert.IsNotNull(classSecond);
            Assert.IsNotNull(classSecond.ThirdClass);
            Assert.IsNotNull(classSecond.ThirdClass.FirstClass);
            Assert.IsNotNull(classSecond.ThirdClass.FirstClass.SecondClass);
            
            Assert.IsNotNull(classThird);
            Assert.IsNotNull(classThird.FirstClass);
            Assert.IsNotNull(classThird.FirstClass.SecondClass);
            Assert.IsNotNull(classThird.FirstClass.SecondClass.ThirdClass);
            
            var classFirstSingleton = container.Resolve<FirstClass>();
            var classSecondSingleton = container.Resolve<SecondClass>();
            var classThirdSingleton = container.Resolve<ThirdClass>();
            
            Assert.AreSame(classFirst, classFirstSingleton);
            Assert.AreSame(classSecond, classSecondSingleton);
            Assert.AreSame(classThird, classThirdSingleton);
        }
        
        [TestMethod]
        public void ResolveCircularThreeLevelMultipleSingletnDependency()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(HardFirst), typeof(HardFirst), LifeType.Singleton);
            configuration.Register(typeof(HardSecond), typeof(HardSecond), LifeType.Singleton); 
            configuration.Register(typeof(HardThird), typeof(HardThird), LifeType.Singleton); 
            var container = new DependencyProvider(configuration);

            var classFirst = container.Resolve<HardFirst>();
            var classSecond = container.Resolve<HardSecond>();
            var classThird = container.Resolve<HardThird>();
            
            Assert.IsNotNull(classFirst);
            Assert.IsNotNull(classFirst.HardSecond);
            Assert.IsNotNull(classFirst.HardSecond.HardFirst); 
            Assert.IsNotNull(classFirst.HardSecond.HardThird);
            Assert.IsNotNull(classFirst.HardSecond.HardThird.HardFirst);
            
            Assert.IsNotNull(classSecond);
            Assert.IsNotNull(classSecond.HardFirst);
            Assert.IsNotNull(classSecond.HardFirst.HardSecond);
            Assert.IsNotNull(classSecond.HardThird);
            Assert.IsNotNull(classSecond.HardThird.HardFirst);
            Assert.IsNotNull(classSecond.HardThird.HardFirst.HardSecond);
            
            Assert.IsNotNull(classThird);
            Assert.IsNotNull(classThird.HardFirst);
            Assert.IsNotNull(classThird.HardFirst.HardSecond);
            Assert.IsNotNull(classThird.HardFirst.HardSecond.HardThird);
        }
    }
}
