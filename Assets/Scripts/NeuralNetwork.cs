using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;

struct ImgInput
{
    public byte[] img;
    public byte value;
};

struct IOActivations
{
    public byte[] iA;
    public float[] oA;
    public int idx;
}

struct PartialDerivatives
{
    public float[][][] nabla_w;
    public float[][] nabla_b;
};

public class NeuralNetwork : MonoBehaviour
{
    System.Random rand = new System.Random();

    const int NUM_LAYERS = 3;
    enum Layers {
        FIRST,
        SECOND,
        THIRD
    };

    int trainingVariety;

    List<IOActivations> trainingData;
    ImgInput currentTestImage;
    int[] sizes = new int[NUM_LAYERS];
    float[][] biases;
    float[][][] weights;

    float[] CostDerivative(float[] oA, float[] rA)
    {
        float[] costDerivative = new float[sizes[sizes.Length - 1]];

        for (int j = 0; j < sizes[sizes.Length - 1]; j++)
        {
            costDerivative[j] = oA[j] - rA[j];
        }

        return costDerivative;
    }

    float sigmoid(float z)
    {
        return 1.0f / (1.0f + (float) Math.Pow(Math.E, -z));
    }

    float sigmoid_prime(float z)
    {
        return sigmoid(z) * (1 - sigmoid(z));
    }

    float[] sigmoid_prime(float[] z)
    {
        float[] derivative = new float[z.Length];
        for (int i = 0; i < z.Length; i++)
        {
            derivative[i] = sigmoid_prime(z[i]);
        }
        return derivative;
    }

    float Dot(float[] v1, float[] v2, bool show = false)
    {
        float s = 0;
        for (int i = 0; i < v1.Length; i++)
        {
            s += v1[i] * v2[i];
        }
        return s;
    }

    float[] ByteArrayToFloatArray(byte[] bA)
    {
        float[] result = new float[bA.Length];

        for (int i = 0; i < bA.Length; i++)
        {
            result[i] = (float)bA[i];
        }

        return result;
    }

    List<T> RandomShuffle<T>(List<T> v)
    {
        int vCount = v.Count;
        T aux;
        var rand = new System.Random();
        int i1, i2;

        for (int i = 0; i < vCount; i++)
        {
            i1 = rand.Next(0, vCount);
            i2 = rand.Next(0, vCount);
            aux = v[i1];
            v[i1] = v[i2];
            v[i2] = aux;
        }

        return v;
    }

    T[][] ArraySplit<T>(List<T> v, int chunkSize)
    {
        int numberOfChunks = (int)Mathf.Ceil((float)v.Count / chunkSize);
        T[][] split = new T[numberOfChunks][];

        for (int i = 0; i < numberOfChunks - 1; i++)
        {
            split[i] = new T[chunkSize];
            for (int j = 0; j < chunkSize; j++)
            {
                split[i][j] = v[i * chunkSize + j];
            }
        }

        int sizeOfLastChunk = v.Count - ((numberOfChunks - 1) * chunkSize); // dimensiunea ramasa poate fi una diferita de celelalte, 
                                                                            // deoarece ultimul miniBatch acopera restul array-ului de date, intrucat chunkSize nu este neaparat divizor al lungimii array-ului
        split[numberOfChunks - 1] = new T[sizeOfLastChunk];
        for (int j = 0; j < sizeOfLastChunk; j++) {
            split[numberOfChunks - 1][j] = v[(numberOfChunks - 1) * chunkSize + j];
        }

        return split;
    }


    float[] FeedForward(byte[] iA)
    {
        float[] a = ByteArrayToFloatArray(iA);
        float[] aux;

        for (int i = 0; i < sizes.Length - 1; i++)
        {
            aux = new float[sizes[i + 1]];
            for (int j = 0; j < sizes[i + 1]; j++)
            {
                aux[j] = sigmoid(Dot(weights[i][j], a) + biases[i][j]);
            }
            a = (float[]) aux.Clone();
        }
        return a;
    }

    float[][][] ShapeOfWeights()
    {
        float[][][] newInst = new float[weights.Length][][];
        for (int i = 0; i < weights.Length; i++)
        {
            newInst[i] = new float[weights[i].Length][];
            for (int j = 0; j < weights[i].Length; j++)
            {
                newInst[i][j] = new float[weights[i][j].Length];
            }
        }
        return newInst;
    }

    float[][] ShapeOfBiases()
    {
        float[][] newInst = new float[biases.Length][];
        for (int i = 0; i < biases.Length; i++)
        {
            newInst[i] = new float[biases[i].Length];
        }
        return newInst;
    }

    PartialDerivatives Backprop(float[] iA, float[] oA)
    {
        float[][][] nabla_w;
        float[][] nabla_b;

        nabla_w = ShapeOfWeights();
        nabla_b = ShapeOfBiases();

        float[] a = (float[])iA.Clone();
        float[][] activations = new float[sizes.Length][];
        float[][] zs = new float[sizes.Length][];
        float[] z;
        float[] aux;

        // (1) feed forward:
        activations[0] = (float[])a.Clone();

        for (int i = 1; i < sizes.Length; i++)
        {
            aux = new float[sizes[i]];
            z = new float[sizes[i]];
            for (int k = 0; k < sizes[i]; k++)
            {
                z[k] = Dot(weights[i - 1][k], a) + biases[i - 1][k];
                aux[k] = sigmoid(z[k]);
            }
            a = (float[])aux.Clone();
            zs[i] = (float[]) z.Clone();
            activations[i] = (float[])a.Clone();
        }

        // (2) starting from the outputs (right) side of the network: finding all biases and weights:
        float[] delta;  // only 1 dimension, and then each delta.Clone() will be stored in nabla_b and nabla_w. At each layer, delta will refresh.
        delta = CostDerivative(activations[activations.Length - 1], oA);
        for (int j = 0; j < sizes[sizes.Length - 1]; j++)
        {
            delta[j] = delta[j] * sigmoid_prime(zs[sizes.Length - 1][j]);
        }

        nabla_b[sizes.Length - 2] = (float[])delta.Clone();

        for (int k = 0; k < sizes[sizes.Length - 1]; k++)
        {
            for (int j = 0; j < sizes[sizes.Length - 2]; j++)
            {
                nabla_w[sizes.Length - 2][k][j] = delta[k] * activations[sizes.Length - 2][j];
            }
        }

        // (3) iterating back through layers (each current layer is represented by the index = sizes.Length - l):
        for (int l = 2; l < sizes.Length; l++)
        {
            z = zs[sizes.Length - l];

            // se completeaza delta-urile incepand cu layerul sizes.Length - 2 si terminand cu layerul cu indexul 1 (cel imediat de dupa activarile initiale)
            float[] delta_aux = new float[sizes[sizes.Length - l]];
            for (int j = 0; j < sizes[sizes.Length - l]; j++)
            {
                delta_aux[j] = 0;
                for (int k = 0; k < sizes[sizes.Length - l + 1]; k++)
                {
                    delta_aux[j] = delta_aux[j] + weights[sizes.Length - l + 1 - 1][k][j] * delta[k];   // am scris + 1 - 1 explicit, pentru ca ne referim la layerul cu indexul sizes.Length - l + 1, dar pt ca din fiecare layer scadem 1, ajungem la [...] - 1.
                }
                delta_aux[j] = delta_aux[j] * sigmoid_prime(z[j]);
            }

            delta = delta_aux;

            nabla_b[sizes.Length - l - 1] = delta;

            for (int k = 0; k < sizes[sizes.Length - l]; k++)
            {
                for (int j = 0; j < sizes[sizes.Length - l - 1]; j++)
                {
                    nabla_w[sizes.Length - l - 1][k][j] = delta[k] * activations[sizes.Length - l - 1][j];
                }
            }
        }

        PartialDerivatives pd = new PartialDerivatives();
        pd.nabla_b = nabla_b;
        pd.nabla_w = nabla_w;

        return pd;
    }

    void SGD(List<IOActivations> trainingData, int epochs, int miniBatchSize, float eta)
    {
        IOActivations[][] splitData;
        
        for (int j = 0; j < epochs; j++)
        {
            trainingData = RandomShuffle<IOActivations>(trainingData);

            //if (j == 19)
            //    foreach (IOActivations ioact in trainingData) {
            //        Debug.Log("10act[" + ioact.idx + "] = " + ioact.oA[0] + "; " + ioact.oA[1]);
            //    }

            splitData = ArraySplit<IOActivations>(trainingData, miniBatchSize);

            for (int i = 0; i < splitData.Length; i++)
            {
                UpdateMiniBatch(splitData[i], eta);
                //UpdateMiniBatch(trainingData.ToArray(), eta);
            }
            Debug.Log("epoch " + j + " complete.");
        }
    }

    void UpdateMiniBatch(IOActivations[] miniBatch, float eta)
    {
        float[][][] nabla_w, delta_nabla_w;
        float[][] nabla_b, delta_nabla_b;

        nabla_b = new float[sizes.Length - 1][];
        nabla_w = new float[sizes.Length - 1][][];

        for (int i = 1; i < sizes.Length; i++)
        {
            nabla_b[i - 1] = new float[sizes[i]];
            nabla_w[i - 1] = new float[sizes[i]][];
            for (int k = 0; k < sizes[i]; k++)
            {
                nabla_b[i - 1][k] = 0;
                nabla_w[i - 1][k] = new float[sizes[i - 1]];
                for (int j = 0; j < sizes[i - 1]; j++)
                {
                    nabla_w[i - 1][k][j] = 0;
                }
            }
        }

        foreach (IOActivations IOAct in miniBatch)
        {
            PartialDerivatives pd = Backprop(ByteArrayToFloatArray(IOAct.iA), IOAct.oA);
            delta_nabla_b = pd.nabla_b;
            delta_nabla_w = pd.nabla_w;

            for (int i = 1; i < sizes.Length; i++)
            {
                for (int k = 0; k < sizes[i]; k++)
                {
                    nabla_b[i - 1][k] = nabla_b[i - 1][k] + delta_nabla_b[i - 1][k];

                    for (int j = 0; j < sizes[i - 1]; j++)
                    {
                        nabla_w[i - 1][k][j] = nabla_w[i - 1][k][j] + delta_nabla_w[i - 1][k][j];
                    }
                }
            }
        }

        for (int i = 1; i < sizes.Length; i++)
        {
            for (int k = 0; k < sizes[i]; k++)
            {
                biases[i - 1][k] = biases[i - 1][k] - (eta / miniBatch.Length) * nabla_b[i - 1][k];

                for (int j = 0; j < sizes[i - 1]; j++)
                {
                    weights[i - 1][k][j] = weights[i - 1][k][j] - (eta / miniBatch.Length) * nabla_w[i - 1][k][j];
                }
            }
        }
    }

    public byte Identify() {   // identifica obiectul ce se potriveste cel mai mult input-ului
        float[] outputActivations = FeedForward(currentTestImage.img);
        byte maxActivation = 0;

        for (byte k = 1; k < outputActivations.Length; k++)  // nu are rost sa incepem de la 0, pentru ca s-ar compara acelasi lucru cu acelasi lucru pt indicele 0
        {
            if (outputActivations[k] > outputActivations[maxActivation])
            {
                maxActivation = k;
            }
        }

        return maxActivation;
    }

    public void ReverseTrainingData() { // capteaza 4 poze pt Sofa si 4 pt TV, doar atat!
        for (int i = 0; i < 8; i += 2) {
            IOActivations ioa = trainingData[i + 1];
            trainingData[i + 1] = trainingData[8 + i + 1];
            trainingData[8 + i + 1] = ioa;
        }
        Debug.Log("reversed");
    }

    public void DispatchSample(bool isTraining, byte[] resizedImage, byte objectId = 255, int idx = 0) {
        if (isTraining) {
            IOActivations ioa = new IOActivations();
            ioa.iA = resizedImage;
            ioa.oA = new float[trainingVariety];
            ioa.idx = idx;

            for (int i = 0; i < trainingVariety; i++)
            {
                ioa.oA[i] = (i == objectId)
                    ? 1
                    : 0;
            }

            trainingData.Add(ioa);
        } else {
            currentTestImage.img = resizedImage;
            //currentTestImage.value = objectId;    // din moment ce noi nu evaluam corectitudinea in teste, 
                                                    // ci vedem din interfata grafica a Unity daca personajul clasifica bine, 
                                                    // atunci nu mai avem nevoie de grad de comparatie, deci nu mai avem nevoiede value
        }
    }

    public void TrainNetworkAndWipeInputData(int epochs, int bucketSize, float learningCurve) { // learningCurve - ceva intre 1.0f si 3.0f, cam pe acolo e recomandat
        if (trainingData.Count == 0) return;    // nu s-a facut deloc training in ultima sesiune!

        SGD(trainingData, epochs, bucketSize, learningCurve);
        trainingData = new List<IOActivations>();   // set a new address; old ones are wiped by the garbage collector since there would be 0 references to them
    }


    double GaussDistr()
    {
        float mean = 100;
        float stdDev = 10;

        double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal =
                     mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

        return randStdNormal;
    }

    void Start()
    {
        trainingData = new List<IOActivations>();
        currentTestImage = new ImgInput();

        sizes[(int)Layers.FIRST] = 0;       // valoarea size-ului primului layer (layerul de input) se stabileste in metoda SetIOLayersSizes()
        sizes[(int)Layers.SECOND] = 30;
        sizes[(int)Layers.THIRD] = 0;       // valoarea size-ului ultimului layer (layerul de output) se stabileste in metoda SetIOLayersSizes()
    }


    public void SetTrainingVariety(int value) {
        trainingVariety = value;
    }

    public void SetIOLayersSizes(int inputLayerSize, int outputLayerSize) {
        sizes[(int)Layers.FIRST] = inputLayerSize;
        sizes[(int)Layers.THIRD] = outputLayerSize;  // == trainingVariety ; Layers.THIRD e ultimul layer, ca reteaua are 3 layere in total
    }

    public void InitializeCoeficients() {
        biases = new float[sizes.Length - 1][];
        weights = new float[sizes.Length - 1][][];

        //string[] weightsLines = System.IO.File.ReadAllLines(@"C:\Users\bob\Documents\DeepLearningPython-master\weights_16_05.txt");
        //string[] biasesLines = System.IO.File.ReadAllLines(@"C:\Users\bob\Documents\DeepLearningPython-master\biases_16_05.txt");

        for (int i = 1; i < sizes.Length; i++)
        {
            biases[i - 1] = new float[sizes[i]];
            weights[i - 1] = new float[sizes[i]][];
            for (int k = 0; k < sizes[i]; k++)
            {
                biases[i - 1][k] = (float)GaussDistr();

                //if (i == 1)
                //    biases[i - 1][k] = float.Parse(biasesLines[k]);
                //else
                //    biases[i - 1][k] = float.Parse(biasesLines[sizes[1] + k]);

                weights[i - 1][k] = new float[sizes[i - 1]];
                for (int j = 0; j < sizes[i - 1]; j++)
                {
                    weights[i - 1][k][j] = (float)GaussDistr();

                    //if (i == 1)
                    //    weights[i - 1][k][j] = float.Parse(weightsLines[k * sizes[0] + j]);
                    //else
                    //    weights[i - 1][k][j] = float.Parse(weightsLines[sizes[0] * sizes[1] + k * sizes[1] + j]);
                }
            }
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            Debug.Log("variety = " + trainingVariety);
            Debug.Log("size[0] = " + sizes[(int)Layers.FIRST]);
            Debug.Log("size[last] = " + sizes[(int)Layers.THIRD]);

            foreach (IOActivations ioact in trainingData) {
                Debug.Log("act[" + ioact.idx + "] = " + ioact.oA[0] + "; " + ioact.oA[1]);
            }
        }
    }
}
