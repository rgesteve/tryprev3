# Trying out ML.NET 3 preview

This is a very simple standalone console app exercising the [oneDAL extensions to ML.NET](https://devblogs.microsoft.com/dotnet/accelerate-ml-net-training-with-intel-onedal).

## Hardware/software requirements

The [oneDAL library](https://www.intel.com/content/www/us/en/develop/documentation/oneapi-programming-guide/top/api-based-programming/intel-oneapi-data-analytics-library-onedal.html) takes advantage of features on modern CPU processors, specifically Intel-x64 variants, so you'll need one of those.

To run this sample, you'll also need [.NET 8.0.](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

With this, trying this out should be as simple as
```bash
dotnet run
```

In some instances, you may see an error loading a library.  In that
case, please take a look at the script `run.sh`, that sets the
`LD_LIBRARY_PATH` environment variable on Linux (on Windows you may
want to do the same with `PATH`)

# Running on Azure

The "bicep" file included provisions an IceLake machine on Azure running Linux, which you can access using the credentials you specify:

```
RGNAME=<some resource group name>
az group create --name $RGNAME --location 'westus2'
az deployment group create --resource-group $RGNAME --template-file infra.bicep --parameters vmname=<vmname> vmuser=<username> vmpass=<password>
az deployment group show -g $RGNAME -n infra --query properties.outputs.sshCommand
```

Warning: for the moment using literal password, ssh key authentication disabled.  Password should comply with regular Azure VM requirements (min 6 characters, mixture of upper- and lower-case, numbers and symbols)

Note that you may want to use a more useful name than the name of the template for the deployment, for easier management.

Log on to the VM using the credentials you provided at instantiation, clone this repo and then issue
```
git clone --depth=1 https://github.com/rgesteve/tryprev3.git
run.sh
```
after cloning this repo on the VM.

## Issues and feedback

Please let us know of any problems you experience when trying to run
this, or any suggestions for improvements at [the ML.NET
repo](https://github.com/dotnet/machinelearning/issues).  We look
forward to your feedback!

