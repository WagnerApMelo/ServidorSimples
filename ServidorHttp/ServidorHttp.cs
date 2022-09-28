using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServidorHttp
{
    public class ServidorHttp
    {
        private TcpListener Controlador { get; set; }
        public int Porta { get; set; }
        public int QtdRequests { get; set; }

        public ServidorHttp(int porta = 8080)
        {
            Porta = porta;
            try
            {
                this.Controlador = new TcpListener(IPAddress.Parse("127.0.0.1"), this.Porta);
                this.Controlador.Start();
                Console.WriteLine($"Servidor HTTP está rodando na porta {this.Porta}.");
                Console.WriteLine($"Para acessar, digite no navegador: http://localhost:{this.Porta}.");

                Task ServidorHttpTask = Task.Run(() => AguardarRequests());
                ServidorHttpTask.GetAwaiter().GetResult();

            }
            catch(Exception e)
            {
                Console.WriteLine($"Erro ao acessar servidor na porta {this.Porta}: \n{e.Message}");

            }
        }

        private async Task AguardarRequests()
        {
            while(true)
            {
                Socket conexao = await this.Controlador.AcceptSocketAsync();
                this.QtdRequests++;
                Task task = Task.Run(() => ProcessarRequests(conexao, this.QtdRequests));
            }
        }

        private void ProcessarRequests(Socket conexao, int numeroRequest)
        {
            Console.WriteLine($"Processando request #{numeroRequest}...\n");
            if (conexao.Connected)
            {
                byte[] bytesRequisicao = new byte[1024];
                conexao.Receive(bytesRequisicao, bytesRequisicao.Length, 0);
                string textoRequisicao = Encoding.UTF8.GetString(bytesRequisicao)
                    .Replace((char)0, ' ').Trim();

                if(textoRequisicao.Length > 0)
                {
                    Console.WriteLine($"\n{textoRequisicao}\n");
                    conexao.Close();
                }
            }

            Console.WriteLine($"\nConexão {numeroRequest} finalizado.");
        }
    }
}
