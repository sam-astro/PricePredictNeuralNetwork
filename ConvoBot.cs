﻿using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public class ConvoBot
{
	public NeuralNetwork net;

	public bool failed;

	string prompt;
	float answer;

	public void StartNetwork()
	{
		net.SetFitness(0);
		Prompt promptObject = new Prompt();
		failed = false;
		for (int l = 0; l < promptObject.AmountOfPrompts(); l++)
		{
			answer = 0;
			float nextGuess = 0;

			prompt = promptObject.GetPrompt(l);

			float[] inputs = new float[55];
			for (int i = 0; i < prompt.Split(" ").Length; i++)
			{
				inputs[i] = (int)Math.Round(float.Parse(prompt.Split(" ")[i]));
			}
			float[] outputs = net.FeedForward(inputs);
			for (int i = 0; i < 10; i++)
			{
				answer += Math.Abs(Math.Clamp(outputs[i] * 300, 0, 10000));
			}

			inputs = new float[55];
			for (int i = 0; i < prompt.Split(" ").Length-1; i++)
			{
				inputs[i] = (int)Math.Round(float.Parse(prompt.Split(" ")[i+1]));
			}
			inputs[prompt.Split(" ").Length] = (int)Math.Round(answer);
			outputs = net.FeedForward(inputs);
			for (int i = 0; i < 10; i++)
			{
				nextGuess += Math.Abs(Math.Clamp(outputs[i]*300, 0, 10000));
			}


			int score = 10000 - Math.Clamp((int)Math.Round(promptObject.StringSimilarity(answer)), 0, 10000);
			float guessScore = (float)(100 - Math.Round(promptObject.StringSimilarity(answer) * 1000) / 100);
			if (guessScore > 70)
			{
				Console.Write("Guessed::  " + answer + " : " + promptObject.correctAnswer + " : " + nextGuess + " : ");
				if (guessScore < 94)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write(guessScore + "%" + '\n');
					Console.ResetColor();
				}
				else if (guessScore < 97)
				{
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.Write(guessScore + "%" + '\n');
					Console.ResetColor();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write(guessScore + "%" + '\n');
					Console.ResetColor();
				}
			}
			//Console.WriteLine(score);

			net.AddFitness(score / promptObject.amountOfPrompts);
		}
	}

	public void Init(NeuralNetwork net)
	{
		this.net = net;
		StartNetwork();
	}

}
