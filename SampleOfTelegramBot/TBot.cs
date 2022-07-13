using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using Pavlo.SampleOfTelegramBot.Actions;

namespace Pavlo.SampleOfTelegramBot
{
    public class TBot
    {
        /// <summary>
        /// Bot ID (or bot token)
        /// </summary>
        private readonly string botID = "INSERT_KEY!";

        /// <summary>
        /// words that wake up bot
        /// </summary>
        public List<string> BotOnKeywords
        { get; set; }


        public Telegram.Bot.TelegramBotClient Bot
        {
            get; private set;
        }

        private Weather weatherAction;

        public TBot()
        {
            BotOnKeywords = new List<string>() { "bot", "бот", };

            weatherAction = new Weather();
            BotOnKeywords.AddRange(weatherAction.BotOnKeywords);
        }

        /// <summary>
        /// initialization of the bot instance
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            Bot = new Telegram.Bot.TelegramBotClient(botID);
        }

        public async Task HandleUpdateAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => MessageReceivedTask(update.Message),
                UpdateType.CallbackQuery => CallbackQueryTask(update.CallbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            };
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
        }

        async Task<Message> SendInlineKeyboard(Message message)
        {
            InlineKeyboardButton btnWeather = InlineKeyboardButton.WithCallbackData(weatherAction.BtnName, weatherAction.BtnCallbackData);

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

        private async Task CallbackQueryTask(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == weatherAction.BtnCallbackData)
            {
                await Bot.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: weatherAction.AnswerCallbackQuery);

                await Bot.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: await weatherAction.GetCurrentWeatherAsync());
            }
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($" Unhandled update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
