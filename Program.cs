using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.FastTree;

namespace Microsoft.ML.Samples.OneDal
{
    class Program
    {

        public static IDataView[] LoadData(
            MLContext mlContext, string trainingFile, string testingFile,
            string task, string label = "target", char separator = ',')
        {
            List<IDataView> dataList = new List<IDataView>();
            System.IO.StreamReader file = new System.IO.StreamReader(trainingFile);
            string header = file.ReadLine();
            file.Close();
            string[] headerArray = header.Split(separator);
            List<TextLoader.Column> columns = new List<TextLoader.Column>();
            foreach (string column in headerArray)
            {
                if (column == label)
                {
                    if (task == "binary")
                        columns.Add(new TextLoader.Column(column, DataKind.Boolean, Array.IndexOf(headerArray, column)));
                    else
                        columns.Add(new TextLoader.Column(column, DataKind.Single, Array.IndexOf(headerArray, column)));
                }
                else
                {
                    columns.Add(new TextLoader.Column(column, DataKind.Single, Array.IndexOf(headerArray, column)));
                }
            }

            var loader = mlContext.Data.CreateTextLoader(
                separatorChar: separator,
                hasHeader: true,
                columns: columns.ToArray()
            );
            dataList.Add(loader.Load(trainingFile));
            dataList.Add(loader.Load(testingFile));
            return dataList.ToArray();
        }

        public static string[] GetFeaturesArray(IDataView data, string labelName = "target")
        {
            List<string> featuresList = new List<string>();
            var nColumns = data.Schema.Count;
            var columnsEnumerator = data.Schema.GetEnumerator();
            for (int i = 0; i < nColumns; i++)
            {
                columnsEnumerator.MoveNext();
                if (columnsEnumerator.Current.Name != labelName)
                    featuresList.Add(columnsEnumerator.Current.Name);
            }

            return featuresList.ToArray();
        }

        public static double[] RunRandomForestClassification(MLContext mlContext, IDataView trainingData, IDataView testingData, string labelName, int numberOfTrees, int numberOfLeaves)
        {
            var featuresArray = GetFeaturesArray(trainingData, labelName);
            var preprocessingPipeline = mlContext.Transforms.Concatenate("Features", featuresArray);
            var preprocessedTrainingData = preprocessingPipeline.Fit(trainingData).Transform(trainingData);
            var preprocessedTestingData = preprocessingPipeline.Fit(trainingData).Transform(testingData);

            FastForestBinaryTrainer.Options options = new FastForestBinaryTrainer.Options();
            options.LabelColumnName = labelName;
            options.FeatureColumnName = "Features";
            options.NumberOfTrees = numberOfTrees;
            options.NumberOfLeaves = numberOfLeaves;
            options.MinimumExampleCountPerLeaf = 5;
            options.FeatureFraction = 1.0;

            var trainer = mlContext.BinaryClassification.Trainers.FastForest(options);

            ITransformer model = trainer.Fit(preprocessedTrainingData);

            IDataView trainingPredictions = model.Transform(preprocessedTrainingData);
            var trainingMetrics = mlContext.BinaryClassification.EvaluateNonCalibrated(trainingPredictions, labelColumnName: labelName);
            IDataView testingPredictions = model.Transform(preprocessedTestingData);
            var testingMetrics = mlContext.BinaryClassification.EvaluateNonCalibrated(testingPredictions, labelColumnName: labelName);

            double[] metrics = new double[4];
            metrics[0] = trainingMetrics.Accuracy;
            metrics[1] = testingMetrics.Accuracy;
            metrics[2] = trainingMetrics.F1Score;
            metrics[3] = testingMetrics.F1Score;
            return metrics;
        }

        public static double[] RunRandomForestRegression(MLContext mlContext, IDataView trainingData, IDataView testingData, string labelName, int numberOfTrees, int numberOfLeaves)
        {
            var featuresArray = GetFeaturesArray(trainingData, labelName);
            var preprocessingPipeline = mlContext.Transforms.Concatenate("Features", featuresArray);
            var preprocessedTrainingData = preprocessingPipeline.Fit(trainingData).Transform(trainingData);
            var preprocessedTestingData = preprocessingPipeline.Fit(trainingData).Transform(testingData);

            FastForestRegressionTrainer.Options options = new FastForestRegressionTrainer.Options();
            options.LabelColumnName = labelName;
            options.FeatureColumnName = "Features";
            options.NumberOfTrees = numberOfTrees;
            options.NumberOfLeaves = numberOfLeaves;
            options.MinimumExampleCountPerLeaf = 5;
            options.FeatureFraction = 1.0;

            var trainer = mlContext.Regression.Trainers.FastForest(options);

            ITransformer model = trainer.Fit(preprocessedTrainingData);

            IDataView trainingPredictions = model.Transform(preprocessedTrainingData);
            var trainingMetrics = mlContext.Regression.Evaluate(trainingPredictions, labelColumnName: labelName);
            IDataView testingPredictions = model.Transform(preprocessedTestingData);
            var testingMetrics = mlContext.Regression.Evaluate(testingPredictions, labelColumnName: labelName);

            double[] metrics = new double[4];
            metrics[0] = trainingMetrics.RootMeanSquaredError;
            metrics[1] = testingMetrics.RootMeanSquaredError;
            metrics[2] = trainingMetrics.RSquared;
            metrics[3] = testingMetrics.RSquared;
            return metrics;
        }

        public static double[] RunOLSRegression(MLContext mlContext, IDataView trainingData, IDataView testingData, string labelName)
        {
            var featuresArray = GetFeaturesArray(trainingData, labelName);
            var preprocessingPipeline = mlContext.Transforms.Concatenate("Features", featuresArray);
            var preprocessedTrainingData = preprocessingPipeline.Fit(trainingData).Transform(trainingData);
            var preprocessedTestingData = preprocessingPipeline.Fit(trainingData).Transform(testingData);

            OlsTrainer.Options options = new OlsTrainer.Options();
            options.LabelColumnName = labelName;
            options.FeatureColumnName = "Features";

            var trainer = mlContext.Regression.Trainers.Ols(options);

            ITransformer model = trainer.Fit(preprocessedTrainingData);

            IDataView trainingPredictions = model.Transform(preprocessedTrainingData);
            var trainingMetrics = mlContext.Regression.Evaluate(trainingPredictions, labelColumnName: labelName);
            IDataView testingPredictions = model.Transform(preprocessedTestingData);
            var testingMetrics = mlContext.Regression.Evaluate(testingPredictions, labelColumnName: labelName);

            double[] metrics = new double[4];
            metrics[0] = trainingMetrics.RootMeanSquaredError;
            metrics[1] = testingMetrics.RootMeanSquaredError;
            metrics[2] = trainingMetrics.RSquared;
            metrics[3] = testingMetrics.RSquared;
            return metrics;
        }

        static void Main(string[] args)
        {
#if false
            var dataRoot = @"/home/sdp/machinelearning/test/data";

            Environment.SetEnvironmentVariable("MLNET_BACKEND", "ONEDAL");

            if (Directory.Exists(dataRoot))
            {
                Console.WriteLine("**** the data directory exists!");
            }
            else
            {
                Console.WriteLine("Problem finding the data directory");
                Environment.Exit(1);
            }

            var trainDataPath = Path.Combine(dataRoot, "binary_synth_data_train.csv");
            var testDataPath = Path.Combine(dataRoot, "binary_synth_data_test.csv");

            if (File.Exists(trainDataPath))
            {
                Console.WriteLine("**** Found path to data file!");
            }
            else
            {
                Console.WriteLine("Problem finding the data file");
                Environment.Exit(1);
            }

            var ML = new MLContext();

            var loader = ML.Data.CreateTextLoader(columns: new[] {
	      new TextLoader.Column("f0", DataKind.Single, 0),
	      new TextLoader.Column("f1", DataKind.Single, 1),
	      new TextLoader.Column("f2", DataKind.Single, 2),
	      new TextLoader.Column("f3", DataKind.Single, 3),
	      new TextLoader.Column("f4", DataKind.Single, 4),
	      new TextLoader.Column("f5", DataKind.Single, 5),
	      new TextLoader.Column("f6", DataKind.Single, 6),
	      new TextLoader.Column("f7", DataKind.Single, 7),
	      new TextLoader.Column("target", DataKind.Boolean,8)},
              separatorChar: ',',
              hasHeader: true
             );

            var trainingData = loader.Load(trainDataPath);
            var testingData = loader.Load(testDataPath);

            Console.WriteLine($"**** The dataview has {trainingData.Schema.Count} columns");

            var preprocessingPipeline = ML.Transforms.Concatenate("Features", new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" });

            var preprocessedTrainingData = preprocessingPipeline.Fit(trainingData).Transform(trainingData);
            var preprocessedTestingData = preprocessingPipeline.Fit(trainingData).Transform(testingData);

            Console.WriteLine($"**** After preprocessing the data got {preprocessedTrainingData.Schema.Count} columns.");

            FastForestBinaryTrainer.Options options = new FastForestBinaryTrainer.Options();
            options.LabelColumnName = "target";
            options.FeatureColumnName = "Features";
            options.NumberOfTrees = 100;
            options.NumberOfLeaves = 128;
            options.MinimumExampleCountPerLeaf = 5;
            options.FeatureFraction = 1.0;

            var trainer = ML.BinaryClassification.Trainers.FastForest(options);
            var model = trainer.Fit(preprocessedTrainingData);
            var trainingPredictions = model.Transform(preprocessedTrainingData);
            var trainingMetrics = ML.BinaryClassification.EvaluateNonCalibrated(trainingPredictions, labelColumnName: "target");
            var testingPredictions = model.Transform(preprocessedTestingData);
            var testingMetrics = ML.BinaryClassification.EvaluateNonCalibrated(testingPredictions, labelColumnName: "target");

            Console.WriteLine($"The Accuracy is {testingMetrics.Accuracy}.");
#else
            // args[0] - training data filename
            // args[1] - testing data filename
            // args[2] - machine learning task (regression, binary)
            // args[3] - machine learning algorithm (RandomForest, OLS)
            // Random Forest parameters:
            //     args[4] - NumberOfTrees
            //     args[5] - NumberOfLeaves
            var mlContext = new MLContext(seed: 42);
            // data[0] - training subset
            // data[1] - testing subset
            IDataView[] data = LoadData(mlContext, args[0], args[1], args[2]);
            string labelName = "target";

            var mainWatch = System.Diagnostics.Stopwatch.StartNew();
            double[] metrics;
            if (args[3] == "RandomForest")
            {
                int numberOfTrees = Int32.Parse(args[4]);
                int numberOfLeaves = Int32.Parse(args[5]);
                if (args[2] == "binary")
                {

                    metrics = RunRandomForestClassification(mlContext, data[0], data[1], labelName, numberOfTrees, numberOfLeaves);
                    mainWatch.Stop();
                    Console.WriteLine("algorithm,all workflow time[ms],training accuracy,testing accuracy,training F1 score,testing F1 score");
                    Console.WriteLine($"Random Forest Binary,{mainWatch.Elapsed.TotalMilliseconds},{metrics[0]},{metrics[1]},{metrics[2]},{metrics[3]}");
                }
                else
                {
                    metrics = RunRandomForestRegression(mlContext, data[0], data[1], labelName, numberOfTrees, numberOfLeaves);
                    mainWatch.Stop();
                    Console.WriteLine("algorithm,all workflow time[ms],training RMSE,testing RMSE,training R2 score,testing R2 score");
                    Console.WriteLine($"Random Forest Regression,{mainWatch.Elapsed.TotalMilliseconds},{metrics[0]},{metrics[1]},{metrics[2]},{metrics[3]}");
                }
            }
            else if (args[3] == "OLS")
            {
                metrics = RunOLSRegression(mlContext, data[0], data[1], labelName);
                mainWatch.Stop();
                Console.WriteLine("algorithm,all workflow time[ms],training RMSE,testing RMSE,training R2 score,testing R2 score");
                Console.WriteLine($"OLS Regression,{mainWatch.Elapsed.TotalMilliseconds},{metrics[0]},{metrics[1]},{metrics[2]},{metrics[3]}");
            }
#endif
        }
    }
}
