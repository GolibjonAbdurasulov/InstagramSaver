using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;

class Program
{
    private static string botToken = "7951367218:AAEFOVv2ynUsXzQLxsPmxWz2zHk9SBSVNNA"; // Bot tokenini kiriting
    private static TelegramBotClient botClient = new TelegramBotClient(botToken);

    static async Task Main()
    {
        using var cts = new CancellationTokenSource();
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new ReceiverOptions(), cts.Token);

        Console.WriteLine("📢 Bot ishga tushdi...");
        await Task.Delay(-1);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null || update.Message.Text == null) return;

        string messageText = update.Message.Text;
        long chatId = update.Message.Chat.Id;

        if (messageText.StartsWith("https://www.instagram.com/"))
        {
            await bot.SendTextMessageAsync(chatId, "🔄 Video yuklanmoqda, biroz kuting...");

            string videoUrl = messageText;
            string outputPath = $"instagram_{Guid.NewGuid()}.mp4";

            try
            {
                string downloadedFile = await DownloadInstagramVideo(videoUrl, outputPath);

                if (downloadedFile != null)
                {
                    using var videoStream = File.OpenRead(downloadedFile);
                    await bot.SendVideoAsync(chatId, new InputFileStream(videoStream, downloadedFile));
                    File.Delete(downloadedFile); // Yuklangan faylni o‘chirish
                }
                else
                {
                    await bot.SendTextMessageAsync(chatId, "❌ Video yuklab bo‘lmadi. Iltimos, havolani tekshiring.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Xatolik: {ex.Message}");
                await bot.SendTextMessageAsync(chatId, "❌ Video yuklab bo‘lmadi. Iltimos, qayta urinib ko‘ring.");
            }
        }
        else
        {
            await bot.SendTextMessageAsync(chatId, "📌 Iltimos, Instagram video havolasini yuboring.");
        }
    }

    private static async Task<string> DownloadInstagramVideo(string url, string outputPath)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = $"-f best -o \"{outputPath}\" \"{url}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processInfo })
        {
            process.Start();
            await process.WaitForExitAsync();

            if (File.Exists(outputPath))
                return outputPath;
        }

        return null;
    }

    private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"❌ Xatolik: {exception.Message}");
        return Task.CompletedTask;
    }
}
