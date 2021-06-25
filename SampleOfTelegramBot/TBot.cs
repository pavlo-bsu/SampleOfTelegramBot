using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using Pavlo.SampleOfTelegramBot.Actions;

namespace Pavlo.SampleOfTelegramBot
{
    public class TBot
    {
        private readonly string pathToBotID;

        public List<string> BotOnKeywords
        { get; set; }


        public Telegram.Bot.TelegramBotClient Bot
        {
            get; private set;
        }

        public Telegram.Bot.Types.User BotUser
        {
            get;
            private set;
        }

        private Weather weatherAction;

        public TBot(string pathToBotID)
        {
            this.pathToBotID = pathToBotID;

            BotOnKeywords = new List<string>() { "bot", "бот",};

            weatherAction = new Weather();
            BotOnKeywords.AddRange(weatherAction.BotOnKeywords);
        }

        public async Task Initialize()
        {
            //reading of the ID
            string botID = await System.IO.File.ReadAllTextAsync(this.pathToBotID);

            //creating of BotUser
            Bot = new Telegram.Bot.TelegramBotClient(botID);
            BotUser = await Bot.GetMeAsync();

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!revise all below 
            var cts = new CancellationTokenSource();
            //Below the new "pooling" way instead of the "obsolete" way based on build-in events system 
            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            Bot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                               cts.Token);

        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => MessageReceivedTask(update.Message),
                UpdateType.CallbackQuery => CallbackQueryTask(update.CallbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private async Task MessageReceivedTask(Message message)
        {
            //check for the message type
            if (message == null || message.Type != MessageType.Text)
                return;

            //check for the first word in the message
            bool isRequestMessage = BotOnKeywords.Contains(message.Text.Split(' ')[0].ToLowerInvariant());

            if (isRequestMessage)
                await SendInlineKeyboard(message);
            else
                return;

            async Task<Message> SendInlineKeyboard(Message message)
            {
                InlineKeyboardButton btnWeather = InlineKeyboardButton.WithCallbackData("Weather \ud83c\udf24\ufe0f", "weather");

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // first row
                    new []
                    {
                        btnWeather
                    }
                });
                return await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Click on your choice",
                    replyMarkup: inlineKeyboard
                );
            }
        }

        private async Task CallbackQueryTask(CallbackQuery callbackQuery)
        {
            await Bot.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"RECIEVED {callbackQuery.Data}");

            await Bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Received {callbackQuery.Data}");
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unhandled update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
