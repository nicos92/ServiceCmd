using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

class Cliente
{
    static void Main()
    {
    
        string[] IpsServidores = new string[] { "192.168.0.52", "192.168.0.144", "192.168.0.144", "192.168.0.53", "192.168.0.61", "192.168.0.91", "192.168.0.45" };
        int puerto = 62132;
        const string CLAVE = "algunaclave";
        const int INTENTOS = 3;
        int intento = 0;
        char opcion = '1';
        while (opcion != '0')
        {
            System.Console.WriteLine("1- enviar comando\n0-Salir");
            opcion = Console.ReadKey(true).KeyChar;
            if (opcion == '1')
            {
                while (intento <= INTENTOS)
                {
                    System.Console.WriteLine("Ingrese la clave:");
                    string claveIngresada = LeerClaveConAsteriscos();
                    if (claveIngresada != CLAVE)
                    {
                        intento++;
                        if (intento == INTENTOS)
                        {
                            System.Console.WriteLine("Demasiados intentos fallidos");
                            return;
                        }
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        System.Console.WriteLine("Clave incorrecta\nIntente nuevamente\n");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    else break;
                }
                string comando;
                intento = 0;
                bool comandoOk = false;
                do
                {

                    Console.Write("Comando a enviar: ");
                    comando = Console.ReadLine() ?? " ";
                    if (!Regex.IsMatch(comando, @"^(date|time)\s(\d{2}/\d{2/\d{4}|\d{2}:\d{2})"))
                    {

                        intento++;
                        if (intento == INTENTOS)
                        {
                            System.Console.WriteLine("Demasiados intentos fallidos");
                            return;
                        }
                        Console.ForegroundColor = ConsoleColor.DarkRed;

                        System.Console.WriteLine("Comando incorrecto\nIntente nuevamente\n");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    else
                    {
                        comandoOk = true;
                    }



                } while (!comandoOk);


                string mensaje = $"{CLAVE}|{comando}";
                foreach (var item in IpsServidores)
                {
                    try
                    {
                        using TcpClient cliente = new TcpClient(item, puerto);
                        using NetworkStream stream = cliente.GetStream();
                        using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                        writer.WriteLine(mensaje);

                        Console.WriteLine($"Respuesta del servidor {item}:");
                        string respuesta = reader.ReadToEnd(); // Lee todo lo que responde el servidor
                        Console.WriteLine(respuesta);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al conectar al servidor {item}: " + ex.Message);
                    }
                }
            }
        }
    }

    public static string LeerClaveConAsteriscos()
    {
        string clave = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                clave += key.KeyChar;
                Console.Write("*");

            }
            else if (key.Key == ConsoleKey.Backspace && clave.Length > 0)
            {
                clave = clave.Substring(0, (clave.Length - 1));
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return clave;
    }
}
