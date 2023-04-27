// See https://aka.ms/new-console-template for more information
using TCPServer;

Server server;
string ipAddress = "127.0.0.1";
int port = 8000;
int listen = 10; 
int buffersize = 4096;

server = new Server();

server.Open(ipAddress, port, listen, buffersize);