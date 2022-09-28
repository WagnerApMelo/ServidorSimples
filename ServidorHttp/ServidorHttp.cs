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
                    var bytesCabecalho = GerarCabecalho("HTTP/1.1", "text/html;charset=utf-8", "200", 0);
                    int bytesEnviados = conexao.Send(bytesCabecalho, bytesCabecalho.Length, 0);
                    conexao.Close();
                    Console.WriteLine($"\n{bytesEnviados} bytes enviados em resposta a requisição # {numeroRequest}.");
                }
            }

            Console.WriteLine($"\nConexão {numeroRequest} finalizado.");
        }

        public byte[] GerarCabecalho(string versaoHttp, string tipoMime,
            string codigoHttp, int qtdeBytes = 0)
        {
            StringBuilder texto = new StringBuilder();
            texto.Append($"{versaoHttp} {codigoHttp}{Environment.NewLine}");
            texto.Append($"Server: Servidor Http Simples 1.0 {Environment.NewLine}");
            texto.Append($"Content-Type: {tipoMime}{Environment.NewLine}");
            texto.Append($"Content-Length: {QtdRequests}{Environment.NewLine}{Environment.NewLine}");
            return Encoding.UTF8.GetBytes(texto.ToString());
        }

    }
}
