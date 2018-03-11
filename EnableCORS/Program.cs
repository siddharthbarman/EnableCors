using Amazon;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Sid.Utils;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Net.SByteStream.Utils.EnableCORS
{
    class Program
    {
        static string StripForwardWhitespaces(string s)
        {
            StringBuilder sb = new StringBuilder();
            string[] lines = s.Split(new char[] { '\n' });
            foreach(string line in lines)
            {
                sb.AppendLine(line.TrimStart());
            }
            return sb.ToString();
        }

        static void Help()
        {
            PrintProgramSummary();
            Console.WriteLine("\nSyntax:");
            
            Console.WriteLine("DotNet EnableCORS -accesskey <AWS Access Key> -secretkey <AWS Secret Key> -region <AWS Region> -apiid <API-Gateway-API-ID> inputfile");
            Console.WriteLine("\nExmaple:");
            Console.WriteLine("DotNet EnableCORS -accesskey BAABAABLACKSHEEPFZ5A -secretkey MYSUPERSUPERSECRETKEY -region us-east-1 -apiid myapi5idv5 c:\\deployment\\cors.txt");

            Console.WriteLine("\nSample CORS file:");
            Console.WriteLine(StripForwardWhitespaces(@"
            #The name of the API. Must occur before any path definitions.
            api-name=My APIs

            #Enable CORS for all blog posts
            path=/posts
            allowed-origins=*
            allowed-headers=Content-Type,x-requested-with,Access-Control-Allow-Headers,Access-Control-Allow-Origin,Access-Control-Allow-Methods
            allowed-methods=get,options

            #Enable CORS for all blog categories
            path=/categories
            allowed-origins=mydomain.com,myhost.com
            allowed-headers=Content-Type,x-requested-with,Access-Control-Allow-Headers,Access-Control-Allow-Origin,Access-Control-Allow-Methods
            allowed-methods=*"));

            PrintDeveloperInfo();
        }

        static void PrintProgramSummary()
        {
            Console.WriteLine("Enable CORS on API-Gateway resources.");
            Console.WriteLine("Requires resources and CORS values to be specified in a text file.");
        }

        static void PrintDeveloperInfo()
        {
            Console.WriteLine("(c) 2018 Siddharth Barman");
            Console.WriteLine("Mail: siddharth_b@yahoo.com");
            Console.WriteLine("Web : sbytestream.pythonanywhere.com");
        }

        static bool CheckHelpParam(string cmdParam)
        {
            return cmdParam == "-help" || cmdParam == "?" || cmdParam == "/?" || cmdParam == "help" ||
                cmdParam == "/help";
        }

        static bool CheckCmdlineValidity(CmdLine cmd)
        {
            bool result = true;

            if (!cmd.IsFlagPresent(PARAM_ACCESS_KEY))
            {
                Console.WriteLine("{0} paramter not specified.", PARAM_ACCESS_KEY);
                result = false;
            }

            if (!cmd.IsFlagPresent(PARAM_SECRET_KEY))
            {
                Console.WriteLine("{0} paramter not specified.", PARAM_SECRET_KEY);
                result = false;
            }

            if (!cmd.IsFlagPresent(PARAM_REGION))
            {
                Console.WriteLine("{0} paramter not specified.", PARAM_REGION);
                result = false;
            }

            if (!cmd.IsFlagPresent(PARAM_API_ID))
            {
                Console.WriteLine("{0} paramter not specified.", PARAM_API_ID);
                result = false;
            }
            
            if (cmd.PositionalArgumentCount != 1)
            {
                Console.WriteLine("Inputfile not specified.");
                result = false;
            }
            
            if (result && !File.Exists(cmd[0]))
            {
                Console.WriteLine("Input file {0} does not exist.", cmd[0]);
                result = false;
            }

            return result;
        }

        static void Main(string[] args)
        {   
            if (args.Length == 0)
            {
                PrintProgramSummary();
                Console.WriteLine("For additional help type: dotnet EnableCors.dll -help");
                PrintDeveloperInfo();
                return;                
            }
            else if ((args.Length == 1 && CheckHelpParam(args[0])))
            {
                Help();
                return;
            }

            CmdLine cmd = new CmdLine(args);

            if (!CheckCmdlineValidity(cmd))
            {
                return;
            }
            
            string accessKey = cmd[PARAM_ACCESS_KEY];
            string secretKey = cmd[PARAM_SECRET_KEY];
            string region = cmd[PARAM_REGION];
            string apiID = cmd[PARAM_API_ID];
            string inputFile = cmd[0];

            CreateCORsEntries(inputFile, accessKey, secretKey, region, apiID);            
        }
        
        static MemoryStream GetMemoryStream(string s)
        {
            MemoryStream memStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(memStream);
            writer.Write(s);
            writer.Flush();
            memStream.Position = 0;
            return memStream;
        }
        
        static void CreateCORsEntries(string inputFile, string accessKey, string secretKey, string region, string apiId)
        {
            RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(region);

            ConfigFileReader reader = new ConfigFileReader(inputFile);
            string server = string.Format("https://apigateway.{0}.amazonaws.com", region);
            string path = string.Format("/restapis/{1}", region, apiId);
            bool atleastOnePathAdded = false;

            StringBuilder requestStr = null;
            
            reader.EntryFoundEvent += new EventHandler<ConfigEntry>((sender, entry) => 
            {
                if (requestStr == null)
                {
                    requestStr = new StringBuilder();
                    requestStr.AppendFormat(REQUEST_HEADER_FMT, entry.APIName, apiId, region);
                }
                
                if (atleastOnePathAdded) requestStr.Append(",\n");
                requestStr.AppendFormat(SINGLE_RESOURCE_REQUEST_FMT, 
                    entry.Path, 
                    entry.AllowedHeaders,
                    entry.AllowedMethods,
                    entry.AllowedOrigins);
                atleastOnePathAdded = true;
            });

            reader.Read();

            if (requestStr == null)
            {
                Console.WriteLine("No resources found.");
                return;
            }

            requestStr.Append(REQUEST_FOOTER_FMT);
            string finalRequestJson = requestStr.ToString();

            try
            {
                HttpStatusCode code = SendRequest(accessKey, secretKey, regionEndpoint, apiId, finalRequestJson);
                Console.WriteLine("Enable CORs on api-ID:{0} => status: {1}", apiId, code);
            }
            catch (Exception e)
            {
                Console.WriteLine("Enable CORs on {0} => Exception: {1}", e.Message);
            }
        }

        static HttpStatusCode SendRequest(string accessKey, string secretKey, RegionEndpoint region, string apiId, string resourceRequestJson)
        {   
            AmazonAPIGatewayClient client = new AmazonAPIGatewayClient(accessKey, secretKey, region);                
            PutRestApiRequest request = new PutRestApiRequest
            {
                Mode = PutMode.Merge,
                RestApiId = apiId,
                Body = GetMemoryStream(resourceRequestJson)
            };

            PutRestApiResponse response = client.PutRestApiAsync(request).Result;
            return response.HttpStatusCode;
        }

        const string PARAM_ACCESS_KEY = "accesskey";
        const string PARAM_SECRET_KEY = "secretkey";
        const string PARAM_REGION = "region";
        const string PARAM_API_ID = "apiid";

        const string REQUEST_HEADER_FMT = @"{{
  ""swagger"": ""2.0"",
  ""info"": {{
     ""version"": ""1.0"",
     ""title"": ""{0}""
  }},
  ""host"": ""{1}.execute-api.{2}.amazonaws.com"",
  ""basePath"": ""/Stage"",
  ""schemes"": [
     ""https""
  ],
  ""paths"": {{
";  
        const string SINGLE_RESOURCE_REQUEST_FMT = @"
     ""{0}"": {{
        ""options"": {{
           ""summary"": ""CORS support"",
           ""description"": ""Enable CORS by returning correct headers\n"",
           ""consumes"": [
              ""application/json""
           ],
           ""produces"": [
              ""application/json""
           ],
           ""tags"": [
              ""CORS""
           ],
           ""x-amazon-apigateway-integration"": {{
              ""type"": ""mock"",
              ""requestTemplates"": {{
                 ""application/json"": ""{{\n  \""statusCode\"" : 200\n}}\n""
              }},
              ""responses"": {{
                 ""default"": {{
                    ""statusCode"": ""200"",
                    ""responseParameters"": {{
                       ""method.response.header.Access-Control-Allow-Headers"": ""'{1}'"",
                       ""method.response.header.Access-Control-Allow-Methods"": ""'{2}'"",
                       ""method.response.header.Access-Control-Allow-Origin"": ""'{3}'""
                    }},
                    ""responseTemplates"": {{
                       ""application/json"": ""{{}}\n""
                    }}
                 }}
              }}
           }},
           ""responses"": {{
              ""200"": {{
                 ""description"": ""Response for CORS method"",
                 ""headers"": {{
                    ""Access-Control-Allow-Headers"": {{
                       ""type"": ""string""
                    }},
                    ""Access-Control-Allow-Methods"": {{
                       ""type"": ""string""
                    }},
                    ""Access-Control-Allow-Origin"": {{
                       ""type"": ""string""
                    }}
                 }}
              }}
           }}
        }}
     }}";
        const string REQUEST_FOOTER_FMT = "\n  }\n}";

    }
}
