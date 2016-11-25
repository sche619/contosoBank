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

        //Create
        public async Task AddAccount(Account account)
        {
            await this.accountTable.InsertAsync(account);
        }

        //Read
        public async Task<List<Account>> GetAccounts()
        {
            return await this.accountTable.ToListAsync();
        }

        //Update
        public async Task UpdateAccount(Account account)
        {
            await this.accountTable.UpdateAsync(account);
        }

        //Delete
        public async Task DeleteAccount(Account account)
        {
            await this.accountTable.DeleteAsync(account);
        }

    }
}