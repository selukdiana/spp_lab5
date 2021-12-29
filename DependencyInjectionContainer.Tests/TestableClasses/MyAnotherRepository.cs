using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class MyAnotherRepository : IRepository
    {
        public MyAnotherRepository() {}
        public override string ToString()
        {
            return "MY_ReeeeeeeeeP";
        }
    }
}
