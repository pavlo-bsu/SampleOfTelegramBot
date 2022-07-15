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
        // Bot and it initialization (in stat. constructor) are done static just to use Bot instance several times during "living" of the instance of the class (depends on AWS settings)

        /// <summary>
        /// instance of the bot
        /// </summary>
        private static TBot bot;

        static Function()
        {
            bot = new TBot();
            bot.Initialize();
        }

        /// <summary>
        /// The function processes request (each message in a chart)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(JObject request, ILambdaContext context)
        {
            //LambdaLogger.Log("REQUEST: " + JsonConvert.SerializeObject(request));

            try
            {
                var updateEvent = request.ToObject<Update>();

                //process the incoming update
                await bot.HandleUpdateAsync(updateEvent);
            }
            catch (Exception e)
            {
                LambdaLogger.Log("exception: " + e.Message + "\n REQUEST: " + JsonConvert.SerializeObject(request));
            }

            return "lambda return";
        }
    }
}