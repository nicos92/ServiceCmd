using ServidorFechaService;

using  Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.WindowsServices;


IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // Esta extensión aplica aquí
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
