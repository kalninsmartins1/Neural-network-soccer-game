﻿using UnityEngine;
using System.Collections.Generic;

public class GeneticAlgorithm
{
    private List<Genome> population = new List<Genome>();
    private int populationSize;
    private int chromosomeLenght;
    private double totalFitness;
    private double bestFitness;
    private double worstFitness;
    private int fittestGenome;
    private double mutationRate;
    private double crossOverRate;
    private int generationCounter;
    private bool isGoalyAlg;


    public GeneticAlgorithm(int populationSize, double mutationRate, double crossOverRate, int weightCount)
    {
        this.populationSize = populationSize;
        this.mutationRate = mutationRate;
        this.crossOverRate = crossOverRate;
        this.chromosomeLenght = weightCount;
        this.totalFitness = 0;
        this.generationCounter = 0;
        this.fittestGenome = 0;
        this.bestFitness = 0;
        this.worstFitness = int.MaxValue;
       


        /* INITIALIZE POPULATION WITH RANDOM WEIGHTS */
        for (int i = 0; i < populationSize; i++)
        {
            population.Add(new Genome());

            for (int j = 0; j < chromosomeLenght; j++)
            {
                population[i].Weights.Add(Random.value);
            }
        }
    }

    private void CrossOver(List<double> mum, List<double> dad, List<double> baby1, List<double> baby2)
    {
        //just return parents as offspring dependent on the rate
        //or if parents are the same
        if ((Random.value > crossOverRate) || (mum == dad))
        {
            baby1.AddRange(mum);
            baby2.AddRange(dad);

            return;
        }

        //determine a crossover point
        int crossOverPoint = Random.Range(0, chromosomeLenght - 1);

        //create the offspring
        for (int i = 0; i < crossOverPoint; i++)
        {
            baby1.Add(mum[i]);
            baby2.Add(dad[i]);
        }

        for (int i = crossOverPoint; i < mum.Count; i++)
        {
            baby1.Add(dad[i]);
            baby2.Add(mum[i]);
        }
    }

    private void Mutate(List<double> chromo)
    {
        //traverse the chromosome and mutate each weight dependent
        //on the mutation rate
        for (int i = 0; i < chromo.Count; i++)
        {
            //do we perturb this weight?
            if (Random.value < mutationRate)
            {
                //add or subtract a small value to the weight
                chromo[i] += (Random.value * NeuralNetworkConst.MAX_PERTURBATION);
            }
        }
    }

    private Genome GetChromoRoulette()
    {
        //generate a random number between 0 & total fitness
        double magicNumb = (double)Random.Range(0, (float)totalFitness);

        //this will be set to the chosen chromosome
        Genome theChosenOne = null;

        //go through the chromosones adding up the fitness so far
        double fitnessSofar = 0;

        for (int i = 0; i < populationSize; i++)
        {
            fitnessSofar += population[i].Fitness;

            //if the fitness so far > random number return the chromo at 
            //this point
            if (fitnessSofar >= magicNumb)
            {
                theChosenOne = population[i];
                break;
            }

        }

        return theChosenOne;
    }

    private void GrabNBest(int bestCount, int copyCount, List<Genome> pop)
    {
        //add the required amount of copies of the n most fittest 
        //to the supplied vector
        if (bestCount > population.Count)
        {
            pop.AddRange(population);
        }
        else
        {
            while (bestCount > 0)
            {
                for (int i = 0; i < copyCount; i++)
                {
                    pop.Add(population[populationSize - bestCount]);
                }

                bestCount--;
            }
        }
       
    }

    private void CalculateBestWorstAverageTotal()
    {
        totalFitness = 0;

        double highestSoFar = 0;
        double lowestSoFar = double.MaxValue;

        for (int i = 0; i < population.Count; i++)
        {
            //find best fitness
            if (population[i].Fitness > highestSoFar)
            {
                highestSoFar = population[i].Fitness;
                fittestGenome = i;
                bestFitness = highestSoFar;
            }

            //find worst fitness
            if (population[i].Fitness < lowestSoFar)
            {
                lowestSoFar = population[i].Fitness;

                worstFitness = lowestSoFar;
            }

            totalFitness += population[i].Fitness;
        }
    }

    private void Reset()
    {
        totalFitness = 0;
        bestFitness = 0;
        worstFitness = double.MaxValue;
    }

    public List<Genome> Epoch(List<Genome> old_population)
    {
        //assign the given population to the classes population
        population = old_population;

        //reset the appropriate variables
        Reset();

        //sort the population (for scaling and elitism)
        population.Sort(Genome.Comparison);

        //calculate best, worst, average and total fitness
        CalculateBestWorstAverageTotal();

        //create a temporary vector to store new chromosones
        List<Genome> newPopulation = new List<Genome>(); ;

        //Now to add a little elitism we shall add in some copies of the
        //fittest genomes. Make sure we add an EVEN number or the roulette
        //wheel sampling will crash
        if ((NeuralNetworkConst.NUMBER_OF_ELITE_COPYS * NeuralNetworkConst.NUMBER_OF_ELITE % 2) == 0)
        {
            GrabNBest(NeuralNetworkConst.NUMBER_OF_ELITE, NeuralNetworkConst.NUMBER_OF_ELITE_COPYS, newPopulation);
        }


        //now we enter the GA loop

        //repeat until a new population is generated
        while (newPopulation.Count < populationSize)
        {
            //grab two chromosones
            Genome mum = GetChromoRoulette();
            Genome dad = GetChromoRoulette();

            //create some offspring via crossover
            List<double> baby1 = new List<double>();
            List<double> baby2 = new List<double>();

            CrossOver(mum.Weights, dad.Weights, baby1, baby2);

            //now we mutate
            Mutate(baby1);
            Mutate(baby2);

            //now copy into vecNewPop population
            newPopulation.Add(new Genome(baby1, 0));
            newPopulation.Add(new Genome(baby2, 0));
        }

        //remove the second best
        if (newPopulation.Count > 1 || isGoalyAlg)
        {
            newPopulation.RemoveAt(0);
        }

        //finished so assign new pop back into m_vecPop
        population = newPopulation;

        return population;
    }

    public List<Genome> Population
    {
        get { return population; }
        set { population = value; }
    }

    public double BestFitness
    {
        get { return bestFitness; }
    }
}
