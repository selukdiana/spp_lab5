namespace DependencyInjectionContainer.Tests.TestableClasses
{
    public class HardFirst
    {
        public HardSecond HardSecond { get; }

        public HardFirst(HardSecond hardSecond)
        {
            HardSecond = hardSecond;
        }
    }
    
    public class HardSecond
    {
        public HardThird HardThird { get; }
        public HardFirst HardFirst { get; }

        public HardSecond(HardFirst hardFirst, HardThird hardThird)
        {
            HardThird = hardThird;
            HardFirst = hardFirst;
        }
    }
    
    public class HardThird
    {
        public HardFirst HardFirst { get; }

        public HardThird(HardFirst hardFirst)
        {
            HardFirst = hardFirst;
        }
    }
}