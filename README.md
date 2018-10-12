# Solution Commons

Container solution created to collect all the common code used during various works.
The solution is structured as following:

 - **Solution.Core** : Functions that only use Net Framework dependencies
 - **Solution.Tools**: Functions that rely on various external nuget packages
 - **Solution.Deploy**: Project used to build DLL of the first 2 subprojects (in release mode)
 - **Solution.Cli**: Console application used to test code

Code is compiled with VisualStudio 2017 express targeting Net Framework 4.5.2

# Usage Example 

All utils are static classes if some status is required it's better to wrap them inside an object.

## Solution.Core

CsvUtil.cs
    
    //Load a CSV file separated by coma data inside DataTable
    //Columns informations are generated from the first row of CSV file
    FileInfo csvFile = new FileInfo("File.csv");
    DataTable loadedDataTable = CsvUtil.LoadDataTable(csvFile, ',', "", true);
            
    //Serialize an existing DataTable to a tilde separated csv without columns header
    string = serializedTable = CsvUtil.SerializeDataTable(existingDataTable, "~", "\"", false);
    File.AppendAllText("Result.csv", serializedTable));

XsltUtil.cs

    //Launch a XSLT transformation on XML saving result on disk
    XlstUtil.Transform("Transform.xslt", "Data.xml", "Result.html"));

## Solution.Tools

SftpUtil.cs
            
    //List and download all file from remote path
    using (var client = SftpUtil.CreateClient("HOSTNAME", 22, "USER", "PWD"))
    {
    	client.Connect();
    	List<string> remoteFiles = SftpUtil.GetRemoteFileList(client, "REMOTEPATH");
    	foreach (string fileName in remoteFiles)
    	{
    		SftpUtil.DownloadRemoteFile(client, "REMOTEPATH" , fileName, "LOCALPATH", fileName , false);
    	}
    }

