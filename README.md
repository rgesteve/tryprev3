# Trying out ML.NET 3 preview

This is a very simple standalone console app exercising the [oneDAL extensions to ML.NET](https://devblogs.microsoft.com/dotnet/accelerate-ml-net-training-with-intel-onedal).

## Hardware/software requirements

The [oneDAL library](https://www.intel.com/content/www/us/en/develop/documentation/oneapi-programming-guide/top/api-based-programming/intel-oneapi-data-analytics-library-onedal.html) takes advantage of features on modern CPU processors, specifically Intel-x64 variants, so you'll need one of those.

To run this sample, you'll also need [.NET 7.0.](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).

With this, trying this out should be as simple as
```bash
dotnet run
```

In some instances, you may see an error loading a library.  In that
case, please take a look at the script `run.sh`, that sets the
`LD_LIBRARY_PATH` environment variable on Linux (on Windows you may
want to do the same with `PATH`)

## Issues and feedback

Please let us know of any problems you experience when trying to run
this, or any suggestions for improvements at [the ML.NET
repo](https://github.com/dotnet/machinelearning/issues).  We look
forward to your feedback!

