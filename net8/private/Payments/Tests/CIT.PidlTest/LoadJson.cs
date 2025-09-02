using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CIT.PidlTest
{
    public static class LoadJson
    {
        public static JToken ReadJsonFile(string jsonLoc)
        {
            using (StreamReader file = File.OpenText(jsonLoc))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JToken token = JToken.ReadFrom(reader);

                    return token;
                }
            }
        }
    }
}
