/*using contosoBank.DataModels;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace contosoBank
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<Timeline> timelineTable;//2.4

        private AzureManager()
        {
            this.client = new MobileServiceClient("MOBILE_APP_URL");
            this.timelineTable = this.client.GetTable<Timeline>();//2.4
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        //3.1
        public async Task<List<Timeline>> GetTimelines()
        {
            return await this.timelineTable.ToListAsync();
        }


    }
}*/

using Microsoft.WindowsAzure.MobileServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using contosoBank.DataModels;

namespace contosoBank
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<Account> accountTable;//

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://contosobankmsa.azurewebsites.net");
            this.accountTable = this.client.GetTable<Account>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task AddAccount(Account account)
        {
            await this.accountTable.InsertAsync(account);
        }

        public async Task<List<Account>> GetAccounts()
        {
            return await this.accountTable.ToListAsync();
        }
    }
}