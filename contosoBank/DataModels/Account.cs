using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace contosoBank.DataModels
{
    public class Account
    {
        [JsonProperty(PropertyName = "accountID")]
        public int accountID { get; set; }

        [JsonProperty(PropertyName = "accountName")]
        public string accountName { get; set; }

        [JsonProperty(PropertyName = "accountBalance")]
        public double accountBalance { get; set; }
    }
}