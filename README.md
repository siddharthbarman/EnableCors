# EnableCors
Enable CORS setting on AWS API Gateway

## Purpose
Let's say you have created an API on AWS API Gateway. We've also added a 
bunch of paths e.g. /posts, /categories and linked various HTTP verbs to
lambda methods. You now want to enable CORS on all the resources.
This program allows you to enable CORS on multiple paths in a single API 
on the AWS API gateway. 

## Technology
This program is built using DotNet Core 1.0 so ... you need DotNet Core 
1.0 on your system. 

## Usage
Build the solution using Visual Studio or other means. The resulting 
binary named EnableCORS.dll can be run using the command line:
DotNet EnableCORS -accesskey <AWS Access Key> -secretkey <AWS Secret Key> -region <AWS Region> -apiid <API-Gateway-API-ID> inputfile

## Input file
The inputfile passed as parameter is a plain text file containing list 
of resources on which the OPTIONS mock method is created to enable CORS.

Please ensure that the api-name which needs to be mentioned in the file
matches the API whose id is specified at the command line (apiid param). 
In case you specify the correct api-id but a different api-name, you 
will rename the API on API Gateway.

Note: Actual method implementation in backend code (e.g. Lambda) needs 
to be made in a way to return the required CORS headers, this is not 
somethinf which this program will take care of.

## Get more help
Run the program with the -help option e.g.:
DotNet EnableCORS -help
