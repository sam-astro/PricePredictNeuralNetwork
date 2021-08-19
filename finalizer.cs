using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

public class finalizer
{
    private List<ConvoBot> entList = null;

    public List<NeuralNetwork> FinalizeGeneration(List<NeuralNetwork> nets, int populationSize)
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

        for (int i = 0; i < populationSize; i++)
        {
            nets[i].SetFitness(0f);
        }

		//CreateEntityBodies(nets, populationSize);
        return nets;
    }

    private void CreateEntityBodies(List<NeuralNetwork> nets, int populationSize)
    {
        entList = new List<ConvoBot>();

        for (int i = 0; i < populationSize; i++)
        {
            ConvoBot convoBot = new ConvoBot();
            convoBot.Init(nets[i]);
            entList.Add(convoBot);
        }
    }
}
