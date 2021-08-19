using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Collections.Specialized;

public class NetManagerConvo
{
	private bool networksRunning = false;
	public int populationSize = 100;
	private int generationNumber = 1;
	public int[] layers = new int[] { 55, 20, 60, 20, 10 }; // No. of inputs and No. of outputs
	private List<NeuralNetwork> nets;
	public List<ConvoBot> entityList = null;
	bool startup = true;

	public int amntLeft;

	private float[][][] collectedWeights;
	private float[][][] collectedWeightsCopy;

	NeuralNetwork persistenceNetwork;

	float lastBest = 100000000;
	float lastWorst = 100000000;
	float highestFitness = 0;
	float lowestFitness = 100000;

	bool queuedForUpload;

	public void NeuralManager()
	{
		Console.WriteLine("Before this starts, how powerful is your PC?");
		Console.WriteLine("1. Minimum, please go easy :(");
		Console.WriteLine("2. Fair, it can't do much");
		Console.WriteLine("3. Average, it can browse the web and run some games");
		Console.WriteLine("4. Modest, it's good for some things but still a little slow");
		Console.WriteLine("5. Good, everything can run on it but only at medium settings");
		Console.WriteLine("6. Great, everything runs on it at high settings");
		Console.WriteLine("7. Excellent, everything runs on it at extreme settings");
		Console.WriteLine("8. Monstrous, it is basically a supercomputer.\n");

		while (true)
		{
			Console.Write("Answer (1-8) > ");
			string answer = Console.ReadLine();
			if (answer.Contains("1"))
			{
				populationSize = 20;
				break;
			}
			else if (answer.Contains("2"))
			{
				populationSize = 40;
				break;
			}
			else if (answer.Contains("3"))
			{
				populationSize = 80;
				break;
			}
			else if (answer.Contains("4"))
			{
				populationSize = 200;
				break;
			}
			else if (answer.Contains("5"))
			{
				populationSize = 300;
				break;
			}
			else if (answer.Contains("6"))
			{
				populationSize = 400;
				break;
			}
			else if (answer.Contains("7"))
			{
				populationSize = 1000;
				break;
			}
			else if (answer.Contains("8"))
			{
				populationSize = 10000;
				break;
			}
			else
			{
				Console.WriteLine();
				continue;
			}
		}

		StreamReader sr = File.OpenText(".\\dat\\weightpersistence.dat");
		string firstLine = sr.ReadLine().Trim();
		string currentGen = firstLine.Split("#")[0];
		generationNumber = int.Parse(currentGen) + 1;
		float bestLocalFitness = float.Parse(firstLine.Split("#")[1]);
		sr.Close();

		try
		{
			System.Net.WebClient Client = new System.Net.WebClient();
			string s = Client.DownloadString("http://achillium.us.to/pricepredictneuralnetdata/bestuploadedfitness.php");
			if (s != null)
				if (float.Parse(s) > bestLocalFitness)
				{
					File.Delete(".\\dat\\weightpersistence.dat");
					Client.DownloadFile(new Uri("http://achillium.us.to/pricepredictneuralnetdata/" + s + "%0d%0a_weightpersistence.dat"), @".\dat\weightpersistence.dat");

					sr = File.OpenText(".\\dat\\weightpersistence.dat");
					firstLine = sr.ReadLine().Trim();
					sr.Close();
					currentGen = firstLine.Split("#")[0];
					generationNumber = int.Parse(currentGen) + 1;
					bestLocalFitness = float.Parse(firstLine.Split("#")[1]);

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("📶 Synced with server 📶");
					Console.ResetColor();
				}
		}
		catch (Exception)
		{
			using (StreamWriter outputFile = new StreamWriter(".\\dat\\weightpersistence.dat", true))
			{
				outputFile.Write("0#0");
			}
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("📶 Could not sync with server. Please try again later. 📶");
			Console.ResetColor();
			throw;
		}

		InitEntityNeuralNetworks();
		while (true)
		{
			nets.Sort();
			amntLeft = populationSize;
			CreateEntityBodies();
			//ThreadStart finalizeRef = new ThreadStart(Finalizer);
			//Thread finalizeThread = new Thread(finalizeRef);
			//finalizeThread.Start();
			highestFitness = 0;
			lowestFitness = 100000;

			Parallel.ForEach(nets, n =>
			{
				float fitness = n.GetFitness();

				if (fitness >= highestFitness)
					highestFitness = fitness;
				if (fitness <= lowestFitness)
					lowestFitness = fitness;
			});

			Console.Write("Generation: " + generationNumber + "  |  Population: " + populationSize);
			if ((highestFitness / 100) < lastBest)
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("Best Fitness:: " + (highestFitness / 100) + "%");
				Console.ResetColor();
			}
			else if ((highestFitness / 100) > lastBest || (queuedForUpload == true && generationNumber % 50 == 0))
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("Best Fitness:: " + (highestFitness / 100) + "%");
				Console.ResetColor();

				StreamWriter persistence = new StreamWriter(".\\dat\\weightpersistence.dat");
				persistence.WriteLine((generationNumber).ToString() + "#" + (highestFitness / 100).ToString());
				for (int i = 0; i < nets[nets.Count - 1].weights.Length; i++)
				{
					for (int j = 0; j < nets[nets.Count - 1].weights[i].Length; j++)
					{
						for (int k = 0; k < nets[nets.Count - 1].weights[i][j].Length; k++)
						{
							Console.ForegroundColor = ConsoleColor.Blue;
							Console.WriteLine("Saving:: " + i.ToString() + "=" + j.ToString() + "=" + k.ToString());
							persistence.WriteLine(i.ToString() + "=" + j.ToString() + "=" + k.ToString() + "=" + nets[nets.Count - 1].weights[i][j][k]);
							Console.ResetColor();
						}
					}
				}
				persistence.Close();

				try
				{
					Upload();
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Successfully uploaded save to server. Continuing.");
					Console.ResetColor();
					queuedForUpload = false;
				}
				catch (Exception)
				{
					queuedForUpload = true;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Failed to connect to server, continuing as normal.");
					Console.ResetColor();
				}
			}
			else
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("Best Fitness:: " + (highestFitness / 100) + "%");
				Console.ResetColor();
			}

			if ((lowestFitness / 100) < lastWorst)
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("Worst Fitness:: " + (lowestFitness / 100) + "%\n");
				Console.ResetColor();
			}
			else if ((lowestFitness / 100) > lastWorst)
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("Worst Fitness:: " + (lowestFitness / 100) + "%\n");
				Console.ResetColor();
			}
			else
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("Worst Fitness:: " + (lowestFitness / 100) + "%\n");
				Console.ResetColor();
			}

			Finalizer();
			//new finalizer().FinalizeGeneration(nets, populationSize);
			//nets.Sort();
			//for (int i = 2; i < (populationSize - 2) / 2; i++) //Gathers all but best 2 nets
			//{
			//    nets[i] = new NeuralNetwork(nets[populationSize - 2]);
			//    nets[i].Mutate();                                                    //Mutates new entities

			//    nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			//    nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			//    nets[populationSize - 2].Mutate();
			//}

			//for (int i = 0; i < populationSize; i++)
			//{
			//    nets[i].SetFitness(0f);
			//}

			amntLeft = populationSize;
			networksRunning = true;
			CreateEntityBodies();
			lastBest = (highestFitness / 100);
			lastWorst = (lowestFitness / 100);

			#region Check if nets are training
			amntLeft = populationSize;
			foreach (ConvoBot emt in entityList)
			{
				if (emt.failed)
				{
					amntLeft--;
				}
			}
			if (amntLeft <= 0)
			{
				networksRunning = false;
				amntLeft = populationSize;
			}
			#endregion
			generationNumber++;

			if (generationNumber % 50 == 0)
			{
				try
				{
					System.Net.WebClient Client = new System.Net.WebClient();
					string s = Client.DownloadString("http://achillium.us.to/pricepredictneuralnetdata/bestuploadedfitness.php");
					if (s != null)
						if (float.Parse(s) > bestLocalFitness)
						{
							File.Delete(".\\dat\\weightpersistence.dat");
							Client.DownloadFile(new Uri("http://achillium.us.to/pricepredictneuralnetdata/" + s + "%0d%0a_weightpersistence.dat"), @".\dat\weightpersistence.dat");

							sr = File.OpenText(".\\dat\\weightpersistence.dat");
							firstLine = sr.ReadLine().Trim();
							currentGen = firstLine.Split("#")[0];
							generationNumber = int.Parse(currentGen) + 1;
							bestLocalFitness = float.Parse(firstLine.Split("#")[1]);
							sr.Close();

							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine("📶 Synced with server 📶");
							Console.ResetColor();
							var info = new System.Diagnostics.ProcessStartInfo(Environment.GetCommandLineArgs()[0]);
							System.Diagnostics.Process.Start(info);
						}
				}
				catch (Exception)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("📶 Could not sync with server. Please try again later. 📶");
					Console.ResetColor();
				}
			}
		}
	}

	private void CreateEntityBodies()
	{
		entityList = new List<ConvoBot>();

		//Parallel.For(0, populationSize, i =>
		//{
		//	ConvoBot convoBot = new ConvoBot();
		//	convoBot.Init(nets[i]);
		//	entityList.Add(convoBot);
		//});

		for (int i = 0; i < populationSize; i++)
		{
			ConvoBot convoBot = new ConvoBot();
			convoBot.Init(nets[i]);
			entityList.Add(convoBot);
		}
	}

	void Finalizer()
	{
		nets.Sort();
		//for (int i = 0; i < (populationSize - 2) / 2; i++) //Gathers all but best 2 nets
		//{
		//    nets[i] = new NeuralNetwork(nets[i]);     //Copies weight values from top half networks to worst half
		//    nets[i].Mutate();
		//    nets[i].Mutate();
		//    nets[i].Mutate();

		//    nets[i + (populationSize - 2) / 2] = new NeuralNetwork(nets[populationSize - 1]);
		//    nets[i + (populationSize - 2) / 2].Mutate();

		//    nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//    nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//}

		if ((highestFitness / 100) > lastBest)
		{
			Parallel.For(0, (populationSize - 2) / 2, i =>
			{
				nets[i] = new NeuralNetwork(nets[i]);     //Copies weight values from top half networks to worst half
				nets[i].Mutate();
				nets[i].Mutate();
				nets[i].Mutate();

				nets[i + (populationSize - 2) / 2] = new NeuralNetwork(nets[populationSize - 1]);
				nets[i + (populationSize - 2) / 2].Mutate();

				nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
				nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			});
		}
		else
		{
			Parallel.For(0, (populationSize - 2) / 2, i =>
			{
				nets[i] = new NeuralNetwork(nets[i]);     //Copies weight values from top half networks to worst half

				nets[i].RandomizeWeights();
				nets[i].Mutate();

				nets[i + (populationSize - 2) / 2] = new NeuralNetwork(nets[populationSize - 1]);
				nets[i + (populationSize - 2) / 2].Mutate();

				nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
				nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			});
		}
			

		for (int i = 0; i < populationSize; i++)
		{
			nets[i].SetFitness(0f);
		}

		//CreateEntityBodies(nets, populationSize);
		//return nets;
	}

	void InitEntityNeuralNetworks()
	{
		if (generationNumber > 4 && startup == true)
			GatherPersistence();
		else
			collectedWeights = null;

		if (populationSize % 2 != 0)
		{
			populationSize++;
		}

		nets = new List<NeuralNetwork>();

		Console.ForegroundColor = ConsoleColor.Blue;
		Parallel.For(0, populationSize, i =>
		{
			NeuralNetwork net = new NeuralNetwork(layers, collectedWeights);
			Console.WriteLine("Creating net:: " + i + " of " + populationSize);
			net.Mutate();
			if (persistenceNetwork != null)
				net.weights = persistenceNetwork.weights;
			nets.Add(net);
		});
		Console.ResetColor();

		//for (int i = 0; i < populationSize; i++)
		//{
		//	NeuralNetwork net = new NeuralNetwork(layers, collectedWeights);
		//	Console.ForegroundColor = ConsoleColor.Blue;
		//	Console.WriteLine("Creating net:: " + i + " of " + populationSize);
		//	Console.ResetColor();
		//	net.Mutate();
		//	if (persistenceNetwork != null)
		//		net.weights = persistenceNetwork.weights;
		//	nets.Add(net);
		//}

		startup = false;
		Console.Clear();
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓ EVERYTHING READY ✓");
		Console.Write("Just let this program process and learn, and only exit if ");
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.Write("BLUE ");
		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write("text isn't getting printed to screen. (that is when it is saving or loading data). I have finally implemented networking! Now, as long as you have an internet connection, the weights data will automatically be sent to my server! Hooray!\n");
		Console.ResetColor();
	}

	void GatherPersistence()
	{
		persistenceNetwork = new NeuralNetwork(layers, null);

		StreamReader sr = File.OpenText(".\\dat\\weightpersistence.dat");
		string[] alllines = sr.ReadToEnd().Split('\n');

		Console.ForegroundColor = ConsoleColor.Blue;
		Parallel.For(0, persistenceNetwork.weights.Length, i =>
		{
			Parallel.For(0, persistenceNetwork.weights[i].Length, j =>
			{
				for (int k = 0; k < persistenceNetwork.weights[i][j].Length; k++)
				{
					foreach (string line in alllines)
					{
						if (line.Split("=")[0] != i.ToString())
							continue;
						if (line.Split("=")[1] != j.ToString())
							continue;
						if (line.Split("=")[2] != k.ToString())
							continue;
						Console.WriteLine("Reading:: " + line);
						persistenceNetwork.weights[i][j][k] = float.Parse(line.Split("=")[3]);
					}

				}
			});
		});
		Console.ResetColor();
		//for (int i = 0; i < persistenceNetwork.weights.Length; i++)
		//{
		//	for (int j = 0; j < persistenceNetwork.weights[i].Length; j++)
		//	{
		//		for (int k = 0; k < persistenceNetwork.weights[i][j].Length; k++)
		//		{

		//			foreach (string line in alllines)
		//			{
		//				if (line.Split("=")[0] != i.ToString())
		//					continue;
		//				if (line.Split("=")[1] != j.ToString())
		//					continue;
		//				if (line.Split("=")[2] != k.ToString())
		//					continue;
		//				Console.ForegroundColor = ConsoleColor.Blue;
		//				Console.WriteLine("Reading:: " + line);
		//				Console.ResetColor();
		//				persistenceNetwork.weights[i][j][k] = float.Parse(line.Split("=")[3]);
		//			}

		//		}
		//	}
		//}
		sr.Close();

		//collectedWeights = persistenceNetwork.weights; //convert to 3D array
		collectedWeightsCopy = persistenceNetwork.weights; //convert to 3D array
	}

	static void Upload()
	{
		System.Net.WebClient Client = new System.Net.WebClient();

		Client.Headers.Add("enctype", "multipart/form-data");

		byte[] result = Client.UploadFile("http://achillium.us.to/pricepredictneuralnetdata/uploadweights.php", "POST",
										  @".\dat\weightpersistence.dat");

		string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
	}
}
