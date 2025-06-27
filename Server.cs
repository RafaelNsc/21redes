using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static UdpClient udpServer = new UdpClient(5000);
    static Dictionary<IPEndPoint, (string nome, int pontos, bool ativo)> jogadores = new();

    static Random random = new();

    static void Main()
    {
        Console.WriteLine("Servidor iniciado na porta 5000...");

        while (true)
        {
            IPEndPoint clienteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpServer.Receive(ref clienteEndPoint);
            string mensagem = Encoding.UTF8.GetString(data);

            if (mensagem.StartsWith("ENTRAR:"))
            {
                string nome = mensagem.Split(':')[1];
                jogadores[clienteEndPoint] = (nome, 0, true);
                Enviar(clienteEndPoint, $"MENSAGEM:Bem-vindo, {nome}!");
                Console.WriteLine($"{nome} entrou no jogo.");
            }
            else if (mensagem == "PEDIR_CARTA")
            {
                if (!jogadores.ContainsKey(clienteEndPoint)) continue;
                var jogador = jogadores[clienteEndPoint];
                int carta = random.Next(1, 12);
                int novaPontuacao = jogador.pontos + carta;

                if (novaPontuacao > 21)
                {
                    Enviar(clienteEndPoint, $"CARTA:{carta}");
                    Enviar(clienteEndPoint, "RESULTADO:perdeu");
                    jogadores[clienteEndPoint] = (jogador.nome, novaPontuacao, false);
                }
                else
                {
                    Enviar(clienteEndPoint, $"CARTA:{carta}");
                    jogadores[clienteEndPoint] = (jogador.nome, novaPontuacao, true);
                }
            }
            else if (mensagem == "PARAR")
            {
                var jogador = jogadores[clienteEndPoint];
                jogadores[clienteEndPoint] = (jogador.nome, jogador.pontos, false);
                Enviar(clienteEndPoint, "MENSAGEM:Você parou.");
            }

            if (jogadores.Count >= 2 && TodosInativos())
            {
                EnviarResultados();
                jogadores.Clear();
                Console.WriteLine("Rodada encerrada.
");
            }
        }
    }

    static void Enviar(IPEndPoint cliente, string mensagem)
    {
        byte[] data = Encoding.UTF8.GetBytes(mensagem);
        udpServer.Send(data, data.Length, cliente);
    }

    static bool TodosInativos()
    {
        foreach (var jogador in jogadores.Values)
        {
            if (jogador.ativo) return false;
        }
        return true;
    }

    static void EnviarResultados()
    {
        int maior = -1;
        foreach (var j in jogadores.Values)
            if (j.pontos <= 21 && j.pontos > maior)
                maior = j.pontos;

        foreach (var (cliente, jogador) in jogadores)
        {
            string resultado = jogador.pontos == maior && jogador.pontos <= 21 ? "ganhou" : "perdeu";
            Enviar(cliente, $"RESULTADO:{resultado}");
        }
    }
}