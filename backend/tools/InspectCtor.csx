using System.Reflection;

var dllPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
    "dotnet", "shared", "Microsoft.AspNetCore.App", "10.0.9",
    "Microsoft.AspNetCore.Identity.dll");

var asm = Assembly.LoadFrom(dllPath);
var type = asm.GetType("Microsoft.AspNetCore.Identity.SignInManager`1");

if (type == null)
{
    Console.WriteLine("SignInManager type not found");
    return;
}

foreach (var ctor in type.GetConstructors())
{
    foreach (var p in ctor.GetParameters())
    {
        Console.WriteLine($"{p.Position}: {p.ParameterType.Name} {p.Name}");
    }
    Console.WriteLine("---");
}
