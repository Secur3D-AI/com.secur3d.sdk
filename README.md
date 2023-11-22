
# Secur3D Unity Package

## Secur3D SDK

The Secur3D SDK is a unity package to interact with Secur3D services through our API using either a Secur3D account or an organization.

To make an account or organization please create one in the [Secur3D Portal](https://portal.secur3d.ai), then use that to access the services in the SDK.

To test the SDK or see how to use it please refer to the sample scripts and scenes.  
The Editor example can be used via ```Window/Secur3D/SDK-Frontend``` with the script being in ```Samples/ExampleEditor/SDKEditorWindow```  
The Runtime sample can be tested by opening and running the scene at ```Samples/ExampleRuntime/Scene/GUI```, the script for this example can be found at ```Samples/ExampleRuntime/SDKRuntimeWindow```

### UploadFile

This function will take a file path of a 3D model and upload it to the Secur3D servers.  
This function will return a UID that you will need to use to interact with the rest of the Secur3D API  
The model's data will be avalible to use up to 2 minuites after its upload.
You can see how to do a model upload [here](https://www.youtube.com/watch?v=NhJBFAkWpwQ)   

In the Secur3D example GUI this function is called ```Upload```.  
This is located in the ```Scripts/Account/AccountAPI``` script for account access.  
For organizations this is located in ```Scripts/Organization/OrganizationAPI``` script.

This is the only function currently accessable through the organization API. When used through the organization API the uploaded model will be uploaded under the organization instead of a user.

### StartComparisonRequest

This function will take 2 file UID's and send them to our comparison queue. When they are done you will be able to retrieve the results using the ```DownloadResults``` function.  
The comparison results will be avalible up to 2 minutes later.  
You can see how to do a model comparison [here](https://www.youtube.com/watch?v=7MLpAiCO3xg)   

In the Secur3D example GUI this function is called ```StartComparison```  
This is located in the ```Scripts/Account/AccountAPI``` script.

### DownloadResults

This function will take 2 file UID's and will look for the results of them being compared.
This will return a JSON containing the two files UID's and the percentage of how much they match in decimal form.

In the Secur3D example GUI this function is called ```Download```  
This is located in the ```Scripts/Account/AccountAPI``` script.

### PollForResults

This function will take in 4 arguments, the first 2 are the UID's of the files you want to retrieve the results of, the second 2 are the amount of milliseconds you want to poll for and how often you want to check in milliseconds.  
This will return a JSON containing the two files UID's and the percentage of how much they match in decimal form.

In the Secur3D example GUI this function is called ```Poll```  
This is located in the ```Scripts/Account/AccountAPI``` script.

### RequestFiles

This function will get all the data of each file you have uploaded.
This will return a list of JSON's containing the file's name, UID, and upload timestamp.

In the Secur3D example GUI this function is called ```RequestFileNames```  
This is located in the ```Scripts/Account/AccountAPI``` script.

### GetFileDataFromUID

This function will take a file UID and will get the data for that file.
This will return a JSON containing the file's name, UID, and upload timestamp.

In the Secur3D example GUI this function is called ```RequestFileDataFromUID```  
This is located in the ```Scripts/Account/AccountAPI``` script.

### GetFileDataFromName

This function will take a file name and will get the data for the most recent file with a matching name.
This will return a JSON containing the file's name, UID, and upload timestamp.

In the Secur3D example GUI this function is called ```RequestFileDataFromName```  
This is located in the ```Scripts/Account/AccountAPI``` script.