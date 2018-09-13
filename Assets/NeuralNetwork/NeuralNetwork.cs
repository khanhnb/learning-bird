using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork
{
    public class NeuralNet
    {
        public double LearnRate { get; set; }
        public double Momentum { get; set; }
        public List<Neuron> InputLayer { get; set; }
        public List<List<Neuron>> HiddenLayers { get; set; }
        public List<Neuron> OutputLayer { get; set; }
        private static readonly System.Random Random = new System.Random();
        double[] weights;
        int numWeights = 0;
        public NeuralNet(int inputSize, int hiddenSize, int outputSize, int numHiddenLayers = 1, double? learnRate = null, double? momentum = null)
        {
            LearnRate = learnRate ?? .4;
            Momentum = momentum ?? .9;
            InputLayer = new List<Neuron>();
            HiddenLayers = new List<List<Neuron>>();
            OutputLayer = new List<Neuron>();
            for (var i = 0; i < inputSize; i++)
                InputLayer.Add(new Neuron());

            for (int i = 0; i < numHiddenLayers; i++)
            {
                HiddenLayers.Add(new List<Neuron>());
                for (var j = 0; j < hiddenSize; j++)
                    HiddenLayers[i].Add(new Neuron(i == 0 ? InputLayer : HiddenLayers[i - 1]));
            }

            for (var i = 0; i < outputSize; i++)
                OutputLayer.Add(new Neuron(HiddenLayers[numHiddenLayers - 1]));

            numWeights = inputSize * hiddenSize + (numHiddenLayers - 1) * hiddenSize * hiddenSize + hiddenSize * outputSize;
            weights = new double[numWeights];
            ExportWeights();

        }

        public void Train(List<DataSet> dataSets, int numEpochs)
        {
            for (var i = 0; i < numEpochs; i++)
            {
                foreach (var dataSet in dataSets)
                {
                    ForwardPropagate(dataSet.Values);
                    BackPropagate(dataSet.Targets);
                }
            }
        }

        public void Train(List<DataSet> dataSets, double minimumError)
        {
            var error = 1.0;
            var numEpochs = 0;

            while (error > minimumError && numEpochs < int.MaxValue)
            {
                var errors = new List<double>();
                foreach (var dataSet in dataSets)
                {
                    ForwardPropagate(dataSet.Values);
                    BackPropagate(dataSet.Targets);
                    errors.Add(CalculateError(dataSet.Targets));
                }
                error = errors.Average();
                numEpochs++;
            }
        }

        private void ForwardPropagate(params double[] inputs)
        {
            var i = 0;
            InputLayer.ForEach(a => a.Value = inputs[i++]);
            foreach (var layer in HiddenLayers)
                layer.ForEach(a => a.CalculateValue());
            OutputLayer.ForEach(a => a.CalculateValue());
        }

        private void BackPropagate(params double[] targets)
        {
            var i = 0;
            OutputLayer.ForEach(a => a.CalculateGradient(targets[i++]));
            foreach (var layer in HiddenLayers.AsEnumerable<List<Neuron>>().Reverse())
            {
                layer.ForEach(a => a.CalculateGradient());
                layer.ForEach(a => a.UpdateWeights(LearnRate, Momentum));
            }
            OutputLayer.ForEach(a => a.UpdateWeights(LearnRate, Momentum));
        }

        public double[] Compute(params double[] inputs)
        {
            ForwardPropagate(inputs);
            return OutputLayer.Select(a => a.Value).ToArray();
        }

        private double CalculateError(params double[] targets)
        {
            var i = 0;
            return OutputLayer.Sum(a => Mathf.Abs((float)a.CalculateError(targets[i++])));
        }

        public static double GetRandom()
        {
            return 2 * Random.NextDouble() - 1;
        }
        // Save weights to a flat list
        public void ExportWeights()
        {
            var i = 0;
            var layersHaveWeights = new List<List<Neuron>>(HiddenLayers);
            layersHaveWeights.Add(OutputLayer);
            foreach (var layer in layersHaveWeights)
            {
                foreach (var neuron in layer)
                {
                    foreach (var synapse in neuron.InputSynapses)
                    {
                        weights[i] = synapse.Weight;
                        i++;
                    }
                }
            }
        }

        public void SetWeights(double[] weights)
        {
            var i = 0;
            var w = 0;
            this.weights = weights;
            for (i = 0; i < HiddenLayers.Count; i++)
            {
                for (int j = 0; j < HiddenLayers[i].Count; j++)
                {
                    for (int k = 0; k < HiddenLayers[i][j].InputSynapses.Count; k++)
                    {
                        HiddenLayers[i][j].InputSynapses[k].Weight = weights[w];
                        w++;
                    }
                }
            }
            for (int j = 0; j < OutputLayer.Count; j++)
            {
                for (int k = 0; k < OutputLayer[j].InputSynapses.Count; k++)
                {
                    OutputLayer[j].InputSynapses[k].Weight = weights[w];
                    w++;
                }
            }
            if (w != weights.Length) Debug.LogError("SetWeights error: w != weights.Count");
        }
        public double[] GetWeights()
        {
            return weights;
        }
    }

    public enum TrainingType
    {
        Epoch,
    }
}