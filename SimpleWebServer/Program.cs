// See https://aka.ms/new-console-template for more information

namespace SimpleWebServer
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.SetWindowSize(80, 20);

			SimpleWebServer.Run(
				@"C:\www\DocRoot",   // ドキュメントルート (index.html 等を配置したフォルダ) を指定する。
							80);		// ポート番号 (特に理由が無ければ 80 を指定する)
		}
	}
}