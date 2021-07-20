using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class Prompt
{
    public string prompt;
	public float correctAnswer;
	public int amountOfPrompts;

	public bool CheckPrompt(float answer)
	{
		ReadList();
		if (answer == correctAnswer)
		{
			return true;
		}
		return false;
	}

	public string GetPrompt(int l)
	{
		StreamReader sr = File.OpenText(".\\dat\\promptlist.dat");
		string[] fullFile = sr.ReadToEnd().Split('\n');
		amountOfPrompts = fullFile.Length;

		//Console.WriteLine(fullFile[l].Split(" # ")[0] + "%" + fullFile[l].Split(" # ")[1]);
		prompt = fullFile[l].Split(" # ")[0];
		correctAnswer = float.Parse(fullFile[l].Split(" # ")[1]);

		return prompt;
	}

	void ReadList()
	{
		StreamReader sr = File.OpenText(".\\dat\\promptlist.dat");
		string[] fullFile = sr.ReadToEnd().Split('\n');

		int randomPromptNumber = new Random().Next(0, fullFile.Length);

		prompt = fullFile[randomPromptNumber].Split(" # ")[0];
		correctAnswer = float.Parse(fullFile[randomPromptNumber].Split(" # ")[1]);
	}

	public int AmountOfPrompts()
	{
		StreamReader sr = File.OpenText(".\\dat\\promptlist.dat");
		string[] fullFile = sr.ReadToEnd().Split('\n');
		return fullFile.Length;
	}

	public float StringSimilarity(float s)
	{
		return Math.Abs(correctAnswer - s);
	}
}
