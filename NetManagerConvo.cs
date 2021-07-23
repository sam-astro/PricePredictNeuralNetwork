using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Net.Http;

public class NetManagerConvo
{
	private bool networksRunning = false;
	public int populationSize = 100;
	private int generationNumber = 1;
	public int[] layers = new int[] { 55, 20, 60, 20, 10 }; // Default No. of inputs and No. of outputs
	private List<NeuralNetwork> nets;
	public List<ConvoBot> entityList = null;
	bool startup = true;

	public int amntLeft;

	private float[][][] collectedWeights;
	private float[][][] collectedWeightsCopy;

	NeuralNetwork persistenceNetwork;

	public void NeuralManager()
	{
		StreamReader sr = File.OpenText(".\\dat\\weightpersistence.dat");
		string currentGen = sr.ReadLine().Trim();
		generationNumber = int.Parse(currentGen) + 1;
		sr.Close();

		InitEntityNeuralNetworks();

		while (true)
		{
			nets.Sort();
			amntLeft = populationSize;
			CreateEntityBodies();
			if (generationNumber % 50 == 0)
			{
				StreamWriter persistence = new StreamWriter(".\\dat\\weightpersistence.dat");
				persistence.WriteLine(generationNumber);
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


				//if (collectedWeightsCopy == null)
				//	GatherPersistence();
				//StreamWriter persistence = new StreamWriter(".\\dat\\weightpersistence.dat");
				//persistence.WriteLine(generationNumber);
				//for (int layerCount = 1; layerCount < nets[nets.Count - 1].weights.Length;)
				//{
				//	for (int neuronCount = 0; neuronCount < nets[nets.Count - 1].weights[layerCount].Length;)
				//	{
				//		for (int synapseCount = 0; synapseCount < nets[nets.Count - 1].weights[layerCount][neuronCount].Length;)
				//		{
				//			Console.WriteLine(layerCount.ToString() + "=" + neuronCount.ToString() + "=" + synapseCount.ToString());
				//			persistence.WriteLine(layerCount.ToString() + "=" + neuronCount.ToString() + "=" + synapseCount.ToString() + "=" + nets[nets.Count - 1].weights[layerCount][neuronCount][synapseCount]);

				//			synapseCount++;
				//		}
				//		neuronCount++;
				//	}
				//	layerCount++;
				//}
				//persistence.Close();
				//File.WriteAllLines(".\\dat\\weightpersistence.dat", );
			}
			//ThreadStart finalizeRef = new ThreadStart(Finalizer);
			//Thread finalizeThread = new Thread(finalizeRef);
			//finalizeThread.Start();
			float highestFitness = 0;
			float lowestFitness = 100000;
			foreach (var n in nets)
			{
				float fitness = n.GetFitness();

				if (fitness >= highestFitness)
					highestFitness = fitness;
				if (fitness <= lowestFitness)
					lowestFitness = fitness;
			}

			Console.Write("Generation: " + generationNumber + "  |  entities: " + (generationNumber * populationSize));
			if ((highestFitness / 100) < 90)
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("Best Fitness:: " + (highestFitness / 100) + "%");
				Console.ResetColor();
			}
			else if ((highestFitness / 100) < 95)
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.Write("Best Fitness:: " + (highestFitness / 100) + "%");
				Console.ResetColor();
			}
			else
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("Best Fitness:: " + (highestFitness / 100) + "%");
				Console.ResetColor();
			}

			if ((lowestFitness / 100) < 85)
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write("Worst Fitness:: " + (lowestFitness / 100) + "%\n");
				Console.ResetColor();
			}
			else if ((lowestFitness / 100) < 90)
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.Write("Worst Fitness:: " + (lowestFitness / 100) + "%\n");
				Console.ResetColor();
			}
			else
			{
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("Worst Fitness:: " + (lowestFitness / 100) + "%\n");
				Console.ResetColor();
			}


			new finalizer().FinalizeGeneration(nets, populationSize);
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
		}
	}

	private void CreateEntityBodies()
	{
		entityList = new List<ConvoBot>();

		for (int i = 0; i < populationSize; i++)
		{
			ConvoBot convoBot = new ConvoBot();
			convoBot.Init(nets[i]);
			entityList.Add(convoBot);
		}
	}

	void Finalizer()
	{
		for (int i = 2; i < (populationSize - 2) / 2; i++) //Gathers all but best 2 nets
		{
			nets[i] = new NeuralNetwork(nets[populationSize - 2]);
			nets[i].Mutate();                                                    //Mutates new entities

			nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			nets[populationSize - 2].Mutate();
		}

		for (int i = 0; i < populationSize; i++)
		{
			nets[i].SetFitness(0f);
		}
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

		for (int i = 0; i < populationSize; i++)
		{
			NeuralNetwork net = new NeuralNetwork(layers, collectedWeights);
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("Creating net:: " + i + " of " + (populationSize - 1));
			Console.ResetColor();
			net.Mutate();
			if (persistenceNetwork != null)
				net.weights = persistenceNetwork.weights;
			nets.Add(net);
		}

		startup = false;
		Console.Clear();
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓ EVERYTHING READY ✓");
		Console.Write("Just let this program process and learn, and only exit if ");
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.Write("BLUE ");
		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write("text isn't getting printed to screen. (that is when it is saving or loading data). Also, I'm too lazy so when you are done training this, you need to either send me /dat/weightpersistence.dat OR send me the text within that file. No networking, yet :P\n");
		Console.ResetColor();
	}

	void GatherPersistence()
	{
		persistenceNetwork = new NeuralNetwork(layers, null);

		for (int i = 0; i < persistenceNetwork.weights.Length; i++)
		{
			for (int j = 0; j < persistenceNetwork.weights[i].Length; j++)
			{
				for (int k = 0; k < persistenceNetwork.weights[i][j].Length; k++)
				{
					StreamReader sr = File.OpenText(".\\dat\\weightpersistence.dat");
					string[] alllines = sr.ReadToEnd().Split('\n');

					foreach (string line in alllines)
					{
						if (line.Split("=")[0] != i.ToString())
							continue;
						if (line.Split("=")[1] != j.ToString())
							continue;
						if (line.Split("=")[2] != k.ToString())
							continue;
						Console.ForegroundColor = ConsoleColor.Blue;
						Console.WriteLine("Reading:: " + line);
						Console.ResetColor();
						persistenceNetwork.weights[i][j][k] = float.Parse(line.Split("=")[3]);
					}

					sr.Close();
				}
			}
		}

		//collectedWeights = persistenceNetwork.weights; //convert to 3D array
		collectedWeightsCopy = persistenceNetwork.weights; //convert to 3D array
	}
}
