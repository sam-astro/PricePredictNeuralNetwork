using System;

namespace NeuralNetConsoleCSCopy
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "Neural Network";

			NetManagerConvo netmanager = new NetManagerConvo();

			netmanager.NeuralManager();
		}
	}
}
