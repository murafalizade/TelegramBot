﻿using Bot.Data;
using Bot.DTOs;
using Bot.Model;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Bot.Services;

namespace Bot.Commands
{
    class BotOperation
    {

        public static ReplyKeyboardMarkup OrderCrypto()
        {
            CryptoData crypto = BotCommand.AllCrypto();
            var rkm = new ReplyKeyboardMarkup();
            var rows = new List<KeyboardButton[]>();
            var cols = new List<KeyboardButton>();
            foreach (Crypto p in crypto.Data)
            {
                string name = p.slug;
                cols.Add(new KeyboardButton("" + name));
                rows.Add(cols.ToArray());
                cols = new List<KeyboardButton>();
            }
            rkm.Keyboard = rows.ToArray();
            return rkm;
        }

        public static string SendPrice(string slug)
        {
            string price = BotCommand.GetPrice(slug);
            if (price == null || price == "USD")
            {
                return "CryptoValue doesn't found.Please try again!";
            }
            else
            {
                return price;
            }
        }

        public static async Task<string> ReminderPrice(long chatId,string text)
        {
            string[] words = text.Split(' ');
            if(words.Length != 3)
            {
                return "Please write correct";
            }
            string slug = words[1];
            decimal price;
            try
            {
               price = Convert.ToDecimal(words[2]);
            }
            catch (Exception)
            {
                return "Please write correct price";
            }
            using (SqliteDbContext database = new SqliteDbContext())
            {
                await database.AddAsync(new Reminder() { UserId = chatId, CryptoName = slug, ExceptionPrice = price, CompliteStatus = false });
                await database.SaveChangesAsync();
                return "Your reminder setted.";
            }
        }

        public static async Task<string> TrackAddress(long chatId,string text)
        {
            string[] words = text.Split(' ');
            if (words.Length != 2)
            {
                return "Please write correct";
            }
            string address = words[1];
            using(SqliteDbContext database = new SqliteDbContext())
            {
                await database.AddAsync(new Tracker() { UserId = chatId,Address = address,UpdateTime= DateTime.Now });
                await database.SaveChangesAsync();
            }
            return "Your address watching...";
           
        }

        public static string DrawGraph(string slug)
        {

            double[] xs = { 1, 2, 3, 4, 5 ,6};
            string[] words = slug.Split(' ');
            double[] ys = Api.GetChangesBySlug(words[1]).ToArray();
            var plt = new ScottPlot.Plot(400, 300);
            plt.AddScatter(xs, ys,System.Drawing.Color.Blue);
            Guid path_url = Guid.NewGuid();
            Console.WriteLine(path_url);
            plt.SaveFig("Trade_images/"+ path_url.ToString() + ".png");
            string prePath = @"C:\\Users\\HP\\source\\repos\\Bot\\Bot\\Trade_images\\";
            return prePath+path_url.ToString()+".png";
        }

        public static  string BalanceAddress(long id)
        {
            string price = "";
            using(SqliteDbContext db = new SqliteDbContext())
            {
                var addresses = from cust in db.Trackers
                              where cust.UserId == id
                              select cust;
                foreach(var address in addresses)
                {
                    price += BotCommand.GetBalance(address.Address) + "\r\n";
                }
            }
            return price;
        }
    }
}
