using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjectionContainer
{
    class Program
    {
        public static void Main()
        {
            var config = new DependenciesConfiguration();
            config.Register<IM, M>(LifeCycle.Singleton);
            config.Register<IW, W>(LifeCycle.Singleton);

            var dp = new DependencyProvider(config);

            IM m = dp.Resolve<IM>();
        }
    }

    public interface IM
    {

    }
    public interface IW
    {

    }
    public interface IL
    {

    }

    public class M : IM
    {
        public IW w;
        public M(IW w)
        {
            this.w = w;
        }      
    }

    public class W :IW
    {
        public IM m;
        public IL l;

        public W(IM m, IL l)
        {
            this.m = m;
            this.l = l;
        }
    }

    public class L : IL
    {
        public IM m;
        public L(IM m)
        {
            this.m = m;
        }
    }
}
