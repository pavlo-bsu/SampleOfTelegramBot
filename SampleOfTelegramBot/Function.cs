using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Telegram.Bot.Types;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Pavlo.SampleOfTelegramBot
{

    public class Function
    {
        /// <summary>
        /// instance of the bot
        /// </summary>
        private TBot bot;

        /// <summary>
        /// The function processes request (each message in a chart)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(JObject request, ILambdaContext context)
        {
            LambdaLogger.Log("REQUEST: " + JsonConvert.SerializeObject(request));

            bot = new TBot();
            bot.Initialize();
            try
            {
                var updateEvent = request.ToObject<Update>();

                //process the incoming update
                await bot.HandleUpdateAsync(updateEvent);

            }
            catch (Exception e)
            {
                LambdaLogger.Log("exception: " + e.Message);
            }

            return "lambda return";
        }
    }
}