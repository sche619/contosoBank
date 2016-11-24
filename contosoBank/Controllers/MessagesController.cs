/*using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace contosoBank
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
                await connector.Conversations.ReplyToActivityAsync(reply);*

                VisionServiceClient VisionServiceClient = new VisionServiceClient("81ca643d8b1d46d8a2c953c9afc3c147");

                AnalysisResult analysisResult = await VisionServiceClient.DescribeAsync(activity.Attachments[0].ContentUrl, 3);

                Activity reply = activity.CreateReply($"{analysisResult.Description.Captions[0].Text}");
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
}*/

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

namespace contosoBank
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                StateClient stateClient = activity.GetStateClient();//Day 2.3 - 1.Setup State Client
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);//2.Grab users data

                //3.Get/Set users property data
                var userMessage = activity.Text;

                string endOutput = "Hello";

                // calculate something for us to return
                if (userData.GetProperty<bool>("SentGreeting"))
                {
                    endOutput = "Hello again";
                }
                else
                {
                    userData.SetProperty<bool>("SentGreeting", true);
                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                }
                //

                bool isWeatherRequest = true;

                if (userMessage.ToLower().Contains("clear"))
                {
                    endOutput = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isWeatherRequest = false;
                }

                if (userMessage.Length > 9)
                {
                    if (userMessage.ToLower().Substring(0, 8).Equals("set home"))
                    {
                        string homeCity = userMessage.Substring(9);
                        userData.SetProperty<string>("HomeCity", homeCity);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        endOutput = homeCity;
                        isWeatherRequest = false;
                    }
                }

                if (userMessage.ToLower().Equals("home"))
                {
                    string homecity = userData.GetProperty<string>("HomeCity");
                    if (homecity == null)
                    {
                        endOutput = "Home City not assigned";
                        isWeatherRequest = false;
                    }
                    else
                    {
                        activity.Text = homecity;
                    }
                }

                if (userMessage.ToLower().Equals("msa"))
                {
                    Activity replyToConversation = activity.CreateReply("MSA information");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://cdn2.iconfinder.com/data/icons/ios-7-style-metro-ui-icons/512/MetroUI_iCloud.png"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://msa.ms",
                        Type = "openUrl",
                        Title = "MSA Website"
                    };
                    cardButtons.Add(plButton);
                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Visit MSA",
                        Subtitle = "The MSA Website is here",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }

                if (userMessage.ToLower().Equals("get account"))
                {
                    List<Account> accounts = await AzureManager.AzureManagerInstance.GetAccounts();
                    endOutput = "";
                    foreach (Account a in accounts)
                    {
                        endOutput += "[" + a.Date + "] Happiness " + a.Happiness + ", Sadness " + a.Sadness + "\n\n";
                    }
                    isWeatherRequest = false;

                }

                if (userMessage.ToLower().Equals("new account"))
                {
                    Account account = new Account()
                    {
                        Anger = 0.1,
                        Contempt = 0.2,
                        Disgust = 0.3,
                        Fear = 0.3,
                        Happiness = 0.3,
                        Neutral = 0.2,
                        Sadness = 0.4,
                        Surprise = 0.4,
                        Date = DateTime.Now
                    };

                    await AzureManager.AzureManagerInstance.AddAccount(account);

                    isWeatherRequest = false;

                    endOutput = "New account added [" + account.Date + "]";
                }

                //3.
                if (!isWeatherRequest)
                {
                    // return our reply to the user
                    Activity infoReply = activity.CreateReply(endOutput);

                    await connector.Conversations.ReplyToActivityAsync(infoReply);

                }
                else
                {

                    CurrencyRate.RootObject rootObject;

                    HttpClient client = new HttpClient();
                    string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=NZD"));//baseCurrency

                    rootObject = JsonConvert.DeserializeObject<CurrencyRate.RootObject>(x);

                    //string baseCurrency = rootObject.@base;
                    string date = rootObject.date;
                    /*
                    string temp = rootObject.main.temp + "°C";
                    string pressure = rootObject.main.pressure + "hPa";
                    string humidity = rootObject.main.humidity + "%";
                    string wind = rootObject.wind.deg + "°";
                    */

                    // added fields
                    string icon = rootObject.weather[0].icon;
                    int cityId = rootObject.id;

                    // return our reply to the user
                    Activity weatherReply = activity.CreateReply($"Current weather for {cityName}");
                    weatherReply.Recipient = activity.From;
                    weatherReply.Type = "message";
                    weatherReply.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "http://openweathermap.org/img/w/" + icon + ".png"));

                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "https://openweathermap.org/city/" + cityId,
                        Type = "openUrl",
                        Title = "More Info"
                    };
                    cardButtons.Add(plButton);

                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = cityName + " Weather",
                        Subtitle = "Temperature " + temp + ", pressure " + pressure + ", humidity  " + humidity + ", wind speeds of " + wind,
                        Images = cardImages,
                        Buttons = cardButtons
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    weatherReply.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(weatherReply);

                }
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