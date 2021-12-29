namespace DependencyInjectionContainer
{
    public enum LifeType
    {
        //возвращать на каждую реализацию или один объект или создавать всегда с помощью new
        Singleton,
        InstancePerDependency
    }
}
