using System.Reflection;

namespace MyOwnDependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        /*var service = new HelloService();
        var consumer = new ServiceConsumer(service);*/
        var container = new DependencyContainer();
        container.AddSingleTon(typeof(HelloService));
       // container.AddDependency<ServiceConsumer>();
        //container.AddDependency<MessageService>();
        var resolver = new DependencyResolver(container);

        var service = resolver.GetService<HelloService>();
        
        //(HelloService)Activator.CreateInstance(typeof(HelloService));
       // var consumer=(ServiceConsumer)Activator.CreateInstance(typeof(ServiceConsumer),service);
        service.Print();
      //  consumer.Print();
    }
    
}

public class DependencyResolver
{
    private DependencyContainer container;
    public DependencyResolver(DependencyContainer container)
    {
        this.container = container;
    }

    public T GetService<T>()
    {
        return (T) GetService(typeof(T));
    }

    public object GetService(Type type)
    {
        var dependency = this.container.GetDependency(type);
        var constructor = dependency.Type.GetConstructors().Single();
        ParameterInfo[] parameters = constructor.GetParameters();
        var parameterImplementations = new object[parameters.Length];

        if (parameters.Length > 0)
        {
            for (int indexI = 0; indexI < parameters.Length; indexI++)
            {
                parameterImplementations[indexI] = GetService(parameters[indexI].ParameterType);
            }
            return CreateImplimentation(dependency,t=>Activator.CreateInstance(type,parameterImplementations));
        }

        return CreateImplimentation(dependency,t=>Activator.CreateInstance(type));
    }

    public object CreateImplimentation(Dependency dependency,Func<Type,object> factory)
    {
        if (dependency.Implimented)
            return dependency.Implimentation;

        var implimentation = factory(dependency.Type);
        if (dependency.DependencyLifeTime == DependencyLifeTime.SingleTon)
        {
            dependency.AddImplimentation(implimentation);
        }
        else if (dependency.DependencyLifeTime == DependencyLifeTime.Transient)
        {
            dependency.AddImplimentation(implimentation);
        }
        return implimentation;
    }
}

public class DependencyContainer
{
    private List<Dependency> dependencies;

    public DependencyContainer()
    {
        this.dependencies = new List<Dependency>();
    }

    public void AddSingleTon(Type type)=> this.dependencies.Add(new Dependency(type,DependencyLifeTime.SingleTon));
    public void AddTransient(Type type)=> this.dependencies.Add(new Dependency(type,DependencyLifeTime.Transient));
   // public void AddDependency<T>() => this.dependencies.Add(typeof(T));
    public Dependency GetDependency(Type type) => this.dependencies.First(x => x.Type.Name == type.Name);
    
}

public class Dependency
{
    public Dependency(Type type,DependencyLifeTime lifeTime)
    {
        Type = type;
        DependencyLifeTime = lifeTime;
    }
    public Type Type { get; set; }
    public DependencyLifeTime DependencyLifeTime { get; set; }
    public object Implimentation{ get; set; }
    public bool Implimented{ get; set; }
    
    public void AddImplimentation(object implimented)
    {
        this.Implimentation = implimented;
        this.Implimented = true;
    }
}

public enum DependencyLifeTime
{
    SingleTon=0,
    Transient=1,
}
public class HelloService
{
    private MessageService messageService;
    public HelloService(MessageService messageService)
    {
        this.messageService = messageService;
    }
    public void Print()
    {
        Console.WriteLine($"Hello World '{messageService.Message()}'");
    }
}
public class ServiceConsumer
{
    private HelloService helloService;
    public ServiceConsumer(HelloService helloService)
    {
        this.helloService = helloService;
    }
    public void Print()
    {
        this.helloService.Print();
    }
}
public class MessageService
{
    int random;
    public MessageService()
    {
        this.random = new Random().Next();
    }
    public string Message() => $"Wow {random}";
}