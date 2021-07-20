using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

class GetPersistence
{
 //   public float[][][] weights;

 //   void GatherPersistence()
	//{
 //       List<float[][]> weightsList = new List<float[][]>(); //weights list which will later will converted into a weights 3D array

 //       //itterate over all neurons that have a weight connection
 //       for (int i = 1; i < layers.Length; i++)
 //       {
 //           List<float[]> layerWeightsList = new List<float[]>(); //layer weight list for this current layer (will be converted to 2D array)

 //           int neuronsInPreviousLayer = layers[i - 1];

 //           //itterate over all neurons in this current layer
 //           for (int j = 0; j < neurons[i].Length; j++)
 //           {
 //               float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

 //               for (int k = 0; k < neuronsInPreviousLayer; k++)
 //               {
 //                   //I want to add multithreading here because this slows it down
 //                   neuronWeights[k] = GetWeightFromPersistence(i, j, k);
 //                   //Console.WriteLine("Loading:: " + i + " " + j + " " + k);
 //               }

 //               layerWeightsList.Add(neuronWeights); //add neuron weights of this current layer to layer weights
 //           }

 //           weightsList.Add(layerWeightsList.ToArray()); //add this layers weights converted into 2D array into weights list
 //       }

 //       weights = weightsList.ToArray(); //convert to 3D array
 //   }
}