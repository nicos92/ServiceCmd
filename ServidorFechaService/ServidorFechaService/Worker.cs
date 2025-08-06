using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;


namespace ServidorFechaService
{
    public class Worker : BackgroundService
    {
        const int puerto = 62132;
        const string CLAVE_SECRETA = "algunaclave";
        static readonly string[] ListaBlancaIPs = new[] { "192.168.0.62", "192.168.0.13" };

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, puerto);
            listener.Start();
            Console.WriteLine($"Servicio escuchando en el puerto {puerto}...");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!listener.Pending())
                {
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                TcpClient cliente = await listener.AcceptTcpClientAsync(stoppingToken);
                _ = Task.Run(() => ManejarCliente(cliente));
            }
        }

        private async Task ManejarCliente(TcpClient cliente)
        {
            string ipCliente = ((IPEndPoint)cliente.Client.RemoteEndPoint).Address.ToString() ;

            using var stream = cliente.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            string mensaje = await reader.ReadLineAsync();
            if (mensaje == null) return;

            if (!EsIPPermitida(ipCliente))
            {
                await writer.WriteLineAsync("ERROR: IP no autorizada.");
                Log($"IP NO AUTORIZADA: {ipCliente}");
                return;
            }

            string[] partes = mensaje.Split('|', 2);
            if (partes.Length != 2)
            {
                await writer.WriteLineAsync("ERROR: Formato inválido.");
                return;
            }

            if (partes[0] != CLAVE_SECRETA)
            {
                await writer.WriteLineAsync("ERROR: Clave incorrecta.");
                Log($"CLAVE INCORRECTA - IP: {ipCliente}");
                return;
            }

            string comando = partes[1];
            string salida = EjecutarComando(comando);
            await writer.WriteLineAsync(salida);
            Log($"IP: {ipCliente}\nCOMANDO: {comando}\nSALIDA:\n{salida}\n");
        }

        private static string EjecutarComando(string comando)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + comando,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proceso = Process.Start(psi);
                string salida = proceso.StandardOutput.ReadToEnd();
                string error = proceso.StandardError.ReadToEnd();
                proceso.WaitForExit();

                return string.IsNullOrWhiteSpace(error) ? salida : $"ERROR: {error}";
            }
            catch (Exception ex)
            {
                return $"EXCEPCIÓN: {ex.Message}";
            }
        }

        private static bool EsIPPermitida(string ip) => Array.Exists(ListaBlancaIPs, x => x == ip);

        private static void Log(string texto)
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "log.txt");
            File.AppendAllText(logPath, $"[{DateTime.Now}] {texto}\n");
        }
    }
}