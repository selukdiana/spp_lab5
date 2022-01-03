using DependencyInjectionContainer;
using TestDemo.ClassesUnderTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using DependencyInjectionContainer.CustomExceptions;
using System;

namespace TestProject1
{
    [TestClass]
    public class DependencyConfigTest
    {
        [TestMethod]
        public void CircularTest2()
        {
            var config = new DependenciesConfiguration();
            config.Register<IM, M>(LifeCycle.Singleton);
            config.Register<IW, W>(LifeCycle.Singleton);
            config.Register<IL, L>(LifeCycle.Singleton);


            var dp = new DependencyProvider(config);

            IM m = dp.Resolve<IM>();
            IL l = dp.Resolve<IL>();

            Assert.AreSame(m, ((W)((M)m).w).m);
            Assert.AreSame(((M)m).w,((M)((W)((M)m).w).m).w);
            Assert.AreSame(((W)((M)m).w).l, l);

        }
        [TestMethod]
        public void SimpleGenericTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IService<IRepository>, ServiceImpl1<IRepository>>();

            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>();
            var service = container.Resolve<IService<IRepository>>();

            Assert.AreEqual(typeof(MyRepository), repository.GetType(), "MyRepository type was expected!");
            Assert.AreEqual(typeof(ServiceImpl1<IRepository>), service.GetType(), "ServiceImpl1<IRepository> type was expected!");
        }
        [TestMethod]
        public void SingletonTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeCycle.Singleton, "1");
            configuration.Register(typeof(IService<>), typeof(ServiceImpl1<>), LifeCycle.InstancePerDependency, "2");

            var container = new DependencyProvider(configuration);
            var serviceImpl1 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl2 = container.Resolve<IService<IRepository>>("2");

            var serviceImpl11 = container.Resolve<IService<IRepository>>("1");
            var serviceImpl22 = container.Resolve<IService<IRepository>>("2");

            Assert.AreSame(serviceImpl1, serviceImpl11, "The very same object was expected! (Singleton)");
            Assert.AreNotSame(serviceImpl2, serviceImpl22, "A newly created object was expected! (InstancePerDependency)");
        }
        [TestMethod]
        public void BaseInterfaceTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register<IBaseService, ServiceImpl1<IRepository>>();

            var container = new DependencyProvider(configuration);
            var service = container.Resolve<IBaseService>();

            Assert.AreEqual(typeof(ServiceImpl1<IRepository>), service.GetType(), "ServiceImpl1<IRepository> was expected!");
        }
        [TestMethod]
        public void ListOfDependenciesWithOpenGenericsTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register(typeof(IRepository), typeof(MyRepository));
            configuration.Register(typeof(IRepository), typeof(SomeRepository));

            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            Assert.AreEqual(2, services.Count, "IRepository is supposed to be wired up against 2 implementations! (open generics)");
            Assert.AreEqual(typeof(MyRepository), services[0].GetType(), "First implementation has to be MyRepository!");
            Assert.AreEqual(typeof(SomeRepository), services[1].GetType(), "Second implementation has to be SomeRepository!");
        }
        [TestMethod]
        public void ListOfDependenciesTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>();
            configuration.Register<IRepository, SomeRepository>();

            var container = new DependencyProvider(configuration);
            var services = container.Resolve<IEnumerable<IRepository>>().ToList();

            Assert.AreEqual(2, services.Count, "IRepository is supposed to be wired up against 2 implementations!");
            Assert.AreEqual(typeof(MyRepository), services[0].GetType(), "First implementation has to be MyRepository!");
            Assert.AreEqual(typeof(SomeRepository), services[1].GetType(), "Second implementation has to be SomeRepository");
        }
        [TestMethod]
        public void CycleTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<SelfDependent, SelfDependent>();
            var container = new DependencyProvider(configuration);

              var result =   container.Resolve<SelfDependent>();
            Assert.IsNull(result.sd);
        }
        [TestMethod]
        public void CyclicDependencyTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<Class1, Class1>();
            configuration.Register<Class2, Class2>();
            var container = new DependencyProvider(configuration);

            try
            {
                container.Resolve<Class1>();
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Cyclic dependency has been evaded! (Class1)");
            }

            try
            {
                container.Resolve<Class2>();
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Cyclic dependency has been evaded! (Class2)");
            }
        }
        [TestMethod]
        public void WrongIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("SuchAUniqueAndTotallyUnexpectedNameWhoWhouldveThought");
            var container = new DependencyProvider(configuration);

            try
            {
                container.Resolve<IRepository>("wrongName");
            }
            catch (DependencyException e)
            {
                Console.WriteLine("Unexpected name has been intercepted!");
            }

        }
        [TestMethod]
        public void ProvideByIdTest()
        {
            var configuration = new DependenciesConfiguration();
            configuration.Register<IRepository, MyRepository>("my");

            var container = new DependencyProvider(configuration);
            var repository = container.Resolve<IRepository>("my");

            Assert.AreEqual(typeof(MyRepository), repository.GetType(), "Resolution by name has not been successful!");
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

            Assert.AreEqual(typeof(ServiceImpl3<IRepository>), service.GetType(), "ServiceImpl3<IRepository>");
            Assert.AreEqual(typeof(MyAnotherRepository), ((ServiceImpl3<IRepository>)service).Repository.GetType(), "Named dependency failed!");
        }

    }
}
