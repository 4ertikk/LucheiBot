using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;


namespace Bott
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static TelegramBotClient botClient;
        private const string token = "7471501923:AAG07WnrM0nTBKa-xNNQbn1pRf3Uht0yZVQ";
        public MainWindow()
        {
            InitializeComponent();
            botClient = new TelegramBotClient(token);

            var cts = new CancellationTokenSource();
            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = { } },
                cancellationToken: cts.Token
            );
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var chatId = update.Message.Chat.Id;

                try
                {
                    switch (update.Message.Text)
                    {
                        case "/start":
                            await botClient.SendTextMessageAsync(chatId, "Добро пожаловать! Используйте команды:\n/start - начать\n/date - показать дату\n/joke - получить шутку\n/clear - очистить чат", cancellationToken: cancellationToken);
                            break;
                        case "/date":
                            await botClient.SendTextMessageAsync(chatId, DateTime.Now.ToString("F"), cancellationToken: cancellationToken);
                            break;
                        case "/joke":
                            var joke = await GetRandomJokeAsync();
                            await botClient.SendTextMessageAsync(chatId, joke, cancellationToken: cancellationToken);
                            break;
                        case "/clear":
                            await botClient.SendTextMessageAsync(chatId, "Чат был очищен. 😊", cancellationToken: cancellationToken);
                            break;
                        default:
                            await botClient.SendTextMessageAsync(chatId, "Неизвестная команда. Используйте /start для списка команд.", cancellationToken: cancellationToken);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(chatId, $"Ошибка: {ex.Message}", cancellationToken: cancellationToken);
                }
            }
        }

        private async Task<string> GetRandomJokeAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync("https://official-joke-api.appspot.com/random_joke");
                    dynamic joke = JsonConvert.DeserializeObject(response);
                    return $"{joke.setup}\n{joke.punchline}";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка при получении шутки: {ex.Message}";
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }


    }
}
