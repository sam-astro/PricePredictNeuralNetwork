
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;

/// <summary>
/// Neural Network C# (Unsupervised)
/// </summary>
public class NeuralNetwork : IComparable<NeuralNetwork>
{
    public int[] layers; //layers
    public float[][] neurons; //neuron matix
    public float[][][] weights; //weight matrix
    public float fitness; //fitness of the network

    public float GetWeightFromPersistence(int x, int y, int z)
    {
		StreamReader sr = File.OpenText(".\\dat\\weightpersistence.dat");
		string[] alllines = sr.ReadToEnd().Split('\n');

		foreach (string line in alllines)
		{
            if (line.Split("=")[0] != x.ToString())
                continue;
            if (line.Split("=")[1] != y.ToString())
                continue;
            if (line.Split("=")[2] != z.ToString())
                continue;

            sr.Close();
            return float.Parse(line.Split("=")[3]);
        }

		sr.Close();
		return 0;
	}

    /// <summary>
    /// Initilizes and neural network with random weights
    /// </summary>
    /// <param name="layers">layers to the neural network</param>
    public NeuralNetwork(int[] layers, float[][][] persistenceWeights)
    {
        //deep copy of layers of this network 
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }


        //generate matrix
        InitNeurons();
		InitWeights(persistenceWeights);
	}

    /// <summary>
    /// Deep copy constructor 
    /// </summary>
    /// <param name="copyNetwork">Network to deep copy</param>
    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }

        InitNeurons();
        InitWeights(null);
        CopyWeights(copyNetwork.weights);
    }

    private void CopyWeights(float[][][] copyWeights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    /// <summary>
    /// Create neuron matrix
    /// </summary>
    private void InitNeurons()
    {
        //Neuron Initilization
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++) //run through all layers
        {
            neuronsList.Add(new float[layers[i]]); //add layer to neuron list
        }

        neurons = neuronsList.ToArray(); //convert list to array
    }

    public void RandomizeWeights()
	{
        List<float[][]> weightsList = new List<float[][]>(); //weights list which will later be converted into a weights 3D array

        //itterate over all neurons that have a weight connection
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>(); //layer weight list for this current layer (will be converted to 2D array)

            int neuronsInPreviousLayer = layers[i - 1];

            //itterate over all neurons in this current layer
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                //itterate over all neurons in the previous layer and set the weights randomly between 0.5f and -0.5
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //give random weights to neuron weights
                    //neuronWeights[k] = UnityEngine.Random.Range(-0.5f,0.5f);
                    neuronWeights[k] = new Random().Next(-50, 50) / 100.0f;
                }

                layerWeightsList.Add(neuronWeights); //add neuron weights of this current layer to layer weights
            }

            weightsList.Add(layerWeightsList.ToArray()); //add this layers weights converted into 2D array into weights list
        }

        weights = weightsList.ToArray(); //convert to 3D array
    }

    /// <summary>
    /// Create weights matrix.
    /// </summary>
    private void InitWeights(float[][][] persistenceWeights)
    {
        StreamReader streamReader = File.OpenText(".\\dat\\weightpersistence.dat");
        string[] lines = streamReader.ReadToEnd().Split('\n');
        streamReader.Close();

		if (persistenceWeights != null)
        {
            //Console.WriteLine("Copied From Best Load");
            weights = persistenceWeights;
			//CopyWeights(persistenceWeights);
			////itterate over all neurons that have a weight connection
			//int layerCount = 1;
			//foreach (float[][] _layer in weightsList)
			//{
			//	if (layerCount >= weightsList.Count)
			//		continue;

			//	int neuronCount = 1;
			//	foreach (float[] _neuron in _layer)
			//	{
			//		if (neuronCount >= weightsList[layerCount].Length)
			//			continue;

			//		int synapseCount = 1;
			//		foreach (float _synapse in _neuron)
			//		{
			//			if (synapseCount >= weightsList[layerCount][neuronCount].Length)
			//				continue;

			//			//I want to add multithreading here because this slows it down
			//			//Console.WriteLine("Loading:: " + layerCount + " " + neuronCount + " " + synapseCount);
			//			//Console.WriteLine("Maxes::   " + layers.Length + " " + persistenceWeights[layerCount].Length + " " + neuronsInPreviousLayer);
			//			weights[layerCount][neuronCount][synapseCount] = GetWeightFromPersistence(layerCount, neuronCount, synapseCount);
			//			synapseCount++;
			//		}
			//		neuronCount++;
			//	}
			//	layerCount++;
			//}

		}
		else
        {
            List<float[][]> weightsList = new List<float[][]>(); //weights list which will later be converted into a weights 3D array

            //itterate over all neurons that have a weight connection
            for (int i = 1; i < layers.Length; i++)
            {
                List<float[]> layerWeightsList = new List<float[]>(); //layer weight list for this current layer (will be converted to 2D array)

                int neuronsInPreviousLayer = layers[i - 1];

                //itterate over all neurons in this current layer
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                    //itterate over all neurons in the previous layer and set the weights randomly between 0.5f and -0.5
                    for (int k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        //give random weights to neuron weights
                        //neuronWeights[k] = UnityEngine.Random.Range(-0.5f,0.5f);
                        neuronWeights[k] = new Random().Next(-50, 50) / 100.0f;
                    }

                    layerWeightsList.Add(neuronWeights); //add neuron weights of this current layer to layer weights
                }

                weightsList.Add(layerWeightsList.ToArray()); //add this layers weights converted into 2D array into weights list
            }

            weights = weightsList.ToArray(); //convert to 3D array
        }
    }

    /// <summary>
    /// Feed forward this neural network with a given input array
    /// </summary>
    /// <param name="inputs">Inputs to network</param>
    /// <returns></returns>
    public float[] FeedForward(float[] inputs)
    {
        //Add inputs to the neuron matrix
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        //itterate over all neurons and compute feedforward values 
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;

                for (int k = 0; k < neurons[i-1].Length; k++)
                {
                    //Console.WriteLine((i-1) + " " + j + " " + k);
                    value += weights[i - 1][j][k] * neurons[i - 1][k]; //sum off all weights connections of this neuron weight their values in previous layer
                }

                neurons[i][j] = (float)Math.Tanh(value); //Hyperbolic tangent activation
            }
        }

        return neurons[neurons.Length-1]; //return output layer
    }

    /// <summary>
    /// Mutate neural network weights
    /// </summary>
    public void Mutate()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    //mutate weight value 
                    float randomNumber = new Random().Next(0, 100);

                    if (randomNumber <= 2f)
                    { //if 1
                      //flip sign of weight
                        weight += (0.25f * weight);
                    }
                    else if (randomNumber <= 4f)
                    { //if 2
                      //pick random weight between -1 and 1
                        weight = new Random().Next(0, 100) / 100.0f;
                    }
                    else if (randomNumber <= 6f)
                    { //if 3
                      //randomly increase by 0% to 100%
                        float factor = new Random().Next(0, 100) / 100.0f + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= 8f)
                    { //if 4
                      //randomly decrease by 0% to 100%
                        float factor = new Random().Next(-100, 100) / 100.0f;
                        weight *= factor;
                    }
                    else if (randomNumber <= 75f)
                    { //if 5
                      //randomly increase or decrease weight by tiny amount
                        float factor = new Random().Next(-1000, 1000) / 100.0f / 1000;
                        weight += factor;
                    }
                    //else
                    //{
                    //    //pick random weight between -1 and 1
                    //    weight = new Random().Next(-100, 100) / 100.0f;
                    //}

                    weights[i][j][k] = weight;
                }
            }
        }
    }

    public void AddFitness(float fit)
    {
        fitness += fit;
    }

    public void SetFitness(float fit)
    {
        fitness = fit;
    }

    public float GetFitness()
    {
        return fitness;
    }

    /// <summary>
    /// Compare two neural networks and sort based on fitness
    /// </summary>
    /// <param name="other">Network to be compared to</param>
    /// <returns></returns>
    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }
}
