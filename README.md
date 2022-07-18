# SampleOfTelegramBot

The program was written just to gain an experience with `Telegram Bot` using.
Later, the following goal was added: deploying the Bot to `Amazon Web Services`.

Bot can be hosted either on PC or on Amazon Web Services. You can find this two approaches on two branches of the repository (although perhaps I should have done two projects in one solution).

## Bot logic

There is a list of keywords by which bot can be called.

Bot logic is listed below:
- The bot checks each message, whether a message contains one of the keywords.
- If a message contains any of the keywords, the bot offers buttons with actions to choose from. 
- In response to user clicking the button, the bot processes selected action and sends an answer.

At current moment there is only one bot action: get the current weather from weather station and send it to the chart.

## Tools and accounts

1. Visual Studio with AWS Toolkit.
2. AWS account.
3. Telegram account.

## Hosting on РС

The bot can be run on any system with `.Net Core`.

There is no block of the caller thread. Receiving is done on the `ThreadPool` via new "pooling" way (instead of the "obsolete" way based on build-in events system).

In the code you should only set the proper path to a file with bot token.

## Hosting on Amazon Web Services

Second option: bot is deployed on `Amazon Web Services`.

Telegram sends each new message to Amazon `API Gateway` which run `Lambda function` with bot logic (see section [“Bot logic”](#bot-logic)).

## Steps for deploying
1. Set proper `bot token` in the code of the `lambda function`.
2. Deploy the lambda function to AWS.
3. Create `Amazon API Gateway` (`REST API` type).
4. For this API, create a method "ANY" that calls the lambda function.
5. Deploy API.
6. Set a webhook in the next form

```html
https://api.telegram.org/bot<YOURTOKEN>/setWebHook?url=<yourAPIurl>
```
Telegram should sent you a HTML-response with message that `Webhook was set`.

## Creation of a telegram bot  
See https://core.telegram.org/bots#3-how-do-i-create-a-bot