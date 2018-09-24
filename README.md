# Solution Commons

Container solution created to collect all the common code used during various works.
The solution is structured as following:

 - **Solution.Core** : Functions that only use Net Framework dependencies
 - **Solution.Tools**: Functions that rely on various external nuget packages
 - **Solution.Deploy**: Project used to build DLL of the first 2 subprojects
 - **Solution.Cli**: Console application used to test code

Code is compiled with VisualStudio 2017 express targheting Net Framework 4.5.2
