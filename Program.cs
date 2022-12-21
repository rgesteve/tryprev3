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
using Microsoft.ML.Trainers.FastTree;

#if NETFRAMEWORK
// AppContext not available in the framework, user needs to set PATH manually
Console.WriteLine($"Not the right TFM, bailing!");
Environment.Exit(1);

#else

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
  var currentDir = AppContext.BaseDirectory;
  var nativeLibs = Path.Join(currentDir, "runtimes", "win-x64", "native");

  var originalPath = Environment.GetEnvironmentVariable("PATH");
  Environment.SetEnvironmentVariable("PATH", nativeLibs + ";" + originalPath);
}
#endif

//var dataRoot = @"/data/projects/machinelearning/test/data";
var dataRoot = @"c:\users\rgesteve\projects\machinelearning\test\data";

Environment.SetEnvironmentVariable("MLNET_BACKEND", "ONEDAL");

if (Directory.Exists(dataRoot)) {
   Console.WriteLine("**** the data directory exists!");
} else {
   Console.WriteLine("Problem finding the data directory");
   Environment.Exit(1);
}

var trainDataPath = Path.Combine(dataRoot, "binary_synth_data_train.csv");
var testDataPath = Path.Combine(dataRoot, "binary_synth_data_test.csv");

if (File.Exists(trainDataPath)) {
   Console.WriteLine("**** Found path to data file!");
} else {
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

