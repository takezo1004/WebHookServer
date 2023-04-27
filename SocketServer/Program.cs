// See https://aka.ms/new-console-template for more information
using SocketServer;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

int port = 8000;
TCPServer server = new TCPServer();

await server.StartListening(port);

