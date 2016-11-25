using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

using System.Collections.Generic;

using contosoBank.DataModels;
using contosoBank;
using contosoBank.Models;
using System.Reflection;

namespace contosoBank
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                StateClient stateClient = activity.GetStateClient();//Day 2.3 - 1.Setup State Client
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);//2.Grab users data

                //3.Get/Set users property data
                var userMessage = activity.Text;
                string endOutput = "";

                if (userMessage.ToLower().Equals("hello") || userMessage.ToLower().Equals("hi"))
                {
                    Activity replyToConversation = activity.CreateReply("Welcome to Contoso Bank");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://cdn2.iconfinder.com/data/icons/ios-7-style-metro-ui-icons/512/MetroUI_iCloud.png"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Title = "Account info",
                        Type = "postBack",
                        Value = "my Account"//user display
                    };
                    cardButtons.Add(plButton);

                    CardAction Button = new CardAction()
                    {
                        Title = "create new Account",
                        Type = "postBack",
                        Value = "create new Account"//user display
                    };
                    cardButtons.Add(Button);

                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Visit Contoso Bank",
                        Subtitle = "Your Account info is here",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);
                }

                bool isAccountRequest = true;

                if (userMessage.ToLower().Contains("clear"))
                {
                    endOutput = "User data cleared";
                    //userData.SetProperty<string>("defaultBase", null);
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                }
                                
                if (userMessage.ToLower().Equals("my account"))
                {
                    string defaultAccount = userData.GetProperty<string>("DefaultAccount");
                    if (defaultAccount == null)
                    {
                        endOutput = "Default Account not assigned";
                        isAccountRequest = false;
                    }
                    else
                    {
                        activity.Text = defaultAccount;
                    }
                }

                if (userMessage.ToLower().Equals("get account"))
                {
                    List<Account> accounts = await AzureManager.AzureManagerInstance.GetAccounts();
                    endOutput = "";
                    foreach (Account a in accounts)
                    {
                        //endOutput += "[" + a.Date + "] Happiness " + a.Happiness + ", Sadness " + a.Sadness + "\n\n";
                        endOutput += "[" + a.accountID + "] " + a.accountName + " balance: $" + a.accountMoney + "\n\n";
                    }
                    isAccountRequest = false;
                }

                if (userMessage.ToLower().Equals("new account"))
                {
                    Account account = new Account()
                    {
                        accountID = 00000001,
                        //accountName = nickname,
                        accountMoney = 0.00,
                        /*
                        Happiness = 0.3,
                        Neutral = 0.2,
                        Sadness = 0.4,
                        Surprise = 0.4,*/
                        Date = DateTime.Now
                    };

                    await AzureManager.AzureManagerInstance.AddAccount(account);

                    isAccountRequest = false;

                    endOutput = "New account added [" + account.accountID + "]";
                }

                if (userData.GetProperty<bool>("setCurrency"))
                {
                    //endOutput = "CURRENCY";
                    userData.SetProperty<bool>("setCurrency", false);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    var userInput = userMessage.Split();
                    string baseCurrency = userInput[0];
                    string exchangeCurrency = userInput[1];

                    CurrencyRate.RootObject rootObject;

                    HttpClient client = new HttpClient();
                    string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + baseCurrency));//baseCurrency

                    rootObject = JsonConvert.DeserializeObject<CurrencyRate.RootObject>(x);

                    PropertyInfo prop = typeof(CurrencyRate.Rates).GetProperty(exchangeCurrency);
                    var value = prop.GetValue(rootObject.rates, null);
                    //endOutput = rootObject.rates.USD.ToString();
                    endOutput = value.ToString();

                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }

                if (userData.GetProperty<bool>("UsingBase"))
                {
                    userData.SetProperty<bool>("UsingBase", false);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                    CurrencyRate.RootObject rootObject;

                    HttpClient client = new HttpClient();
                    string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + userData.GetProperty<string>("defaultBase")));//baseCurrency

                    rootObject = JsonConvert.DeserializeObject<CurrencyRate.RootObject>(x);

                    PropertyInfo prop = typeof(CurrencyRate.Rates).GetProperty(userMessage);
                    var value = prop.GetValue(rootObject.rates, null);
                    endOutput = value.ToString();

                    //await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                }

                if (userMessage.ToLower().Equals("use base currency"))
                {
                    if (userData.GetProperty<string>("defaultBase")!=null)
                    {
                        endOutput = "Please enter the currency you want to exchange into";
                        userData.SetProperty<bool>("UsingBase", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                    else
                    {
                        endOutput = "Base Currency does not exist. Please set the base currency. ";
                    }
                }

                    if (userData.GetProperty<bool>("setBase"))
                {
                    userData.SetProperty<bool>("setBase", false);
                    userData.SetProperty<string>("defaultBase", userMessage);
                    userData.SetProperty<bool>("BaseExists", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    endOutput="Your base currency is now set as "+ userData.GetProperty<string>("defaultBase")+".";

                }

                if (userMessage.ToLower().Equals("set base currency"))
                {
                    endOutput = "Enter the currency please";
                    userData.SetProperty<bool>("setBase", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }

                if (userMessage.ToLower().Equals("exchange rate"))
                {
                    endOutput = "Please enter a base currency and an exchange currency";
                    userData.SetProperty<bool>("setCurrency", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }

                Activity infoReply = activity.CreateReply(endOutput);
                await connector.Conversations.ReplyToActivityAsync(infoReply);

            }

            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}