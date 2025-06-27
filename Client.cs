using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static UdpClient udpClient = new UdpClient();
    static IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 5000);

    static void Main()
    {
        Console.Write("Digite seu nome: ");
        string nome = Console.ReadLine();
        Enviar($"ENTRAR:{nome}");

        Console.WriteLine("Digite 'p' para pedir carta ou 'x' para parar.");

        new Thread(() =>
        {
            while (true)
            {
                byte[] data = udpClient.Receive(ref serverEP);
                string resposta = Encoding.UTF8.GetString(data);
                Console.WriteLine($"
> {resposta}");
            }
        }).Start();

        while (true)
        {
            string cmd = Console.ReadLine();
            if (cmd == "p")
                Enviar("PEDIR_CARTA");
            else if (cmd == "x")
                Enviar("PARAR");
        }
    }

    static void Enviar(string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        udpClient.Send(data, data.Length, serverEP);
    }
}