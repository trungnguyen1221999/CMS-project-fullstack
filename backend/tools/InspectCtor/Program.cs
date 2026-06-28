using System.Reflection;

var type = typeof(Microsoft.AspNetCore.Identity.SignInManager<>);
foreach (var ctor in type.GetConstructors())
{
    foreach (var p in ctor.GetParameters())
    {
        Console.WriteLine($"{p.Position}: {p.ParameterType.Name} {p.Name}");
    }
    Console.WriteLine("---");
}
