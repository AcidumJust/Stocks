using System;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Stocks
{
    class Program
    {
        static void Main(string[] args)
        {
            // устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(UpdateStoksInfo);
            // создаем таймер с обновлением каждый час
            //Console.WriteLine("Создание таймера");
            List<double> dataArr = new List<double>(new double[] { 123.8500000, 128.8000000, 123.2000000, 124.8000000, 123.1000000, 123.3400000, 119.1500000, 120.2000000, 123.4100000, 127.7500000, 125.1200000, 125.6000000, 122.2000000, 119.4600000, 123.6000000 });
            //RSI(dataArr, 14);
            //MA(dataArr, 26);
            //STOCH(dataArr, 9, 6);
            double a = 2.0 / (3 + 1);
            Console.WriteLine(a*66744.0+(1.0-a)*66724.5);
            Timer timer = new Timer(tm, null, 0, 60000 * 60);

            //Console.WriteLine(decimal.Parse("122,2000000"));
            //UpdateStoksInfo();
            while (true)
            {
                //Thread.Sleep(300000);
                //Console.WriteLine("AAAAA");
            }

        }
        static void UpdateStoksInfo(object obj)
        {
            using (StreamReader re = new StreamReader("stocks_id.json"))
            {
                JsonTextReader reader = new JsonTextReader(re);
                JsonSerializer se = new JsonSerializer();
                object parsedData = se.Deserialize(reader);
                Dictionary<string, int> JsonObject = JsonConvert.DeserializeObject<Dictionary<string, int>>(parsedData.ToString());
                foreach (var v in JsonObject.Keys)
                {
                    int em = JsonObject[v];
                    string code = v;
                    LoadStoksInfo(em, code);
                    Thread.Sleep(1000);
                }
            }
        }
        static async void UpdateStoksInfoAsync(object obj)
        {

            await Task.Run(() =>
            {
                using (StreamReader re = new StreamReader("stocks_id.json"))
                {
                    JsonTextReader reader = new JsonTextReader(re);
                    JsonSerializer se = new JsonSerializer();
                    object parsedData = se.Deserialize(reader);
                    Dictionary<string, int> JsonObject = JsonConvert.DeserializeObject<Dictionary<string, int>>(parsedData.ToString());
                    foreach (var v in JsonObject.Keys)
                    {
                        int em = JsonObject[v];
                        string code = v;
                        LoadStoksInfo(em, code);
                    }
                }
            });

        }
        static void LoadStoksInfo(int em, string code)
        {
            Console.WriteLine("Try " + code);
            DateTime dateTo = DateTime.Today;
            //регулировка глубины данных
            DateTime dateFrom = dateTo.AddMonths(-24);
            //periods ={ 'tick': 1, 'min': 2, '5min': 3, '10min': 4, '15min': 5, '30min': 6, 'hour': 7, 'daily': 8, 'week': 9, 'month': 10}
            int per = 8;

            // Адрес ресурса, к которому выполняется запрос
            string url = $"http://export.finam.ru/info?market=1&em={em}&code={code}&apply=0&df={dateFrom.Day}&mf={dateFrom.Month - 1}&yf={dateFrom.Year}&dt={dateTo.Day}&mt={dateTo.Month - 1}&yt={dateTo.Year}&p={per}&f=info_{code}&e=.csv&cn={code}&dtf=1&tmf=1&MSOR=1&mstime=on&mstimever=1&sep=3&sep2=1&datf=1&at=1";

            string responseWeb = "";
            // Создаём объект WebClient
            using (var webClient = new WebClient())
            {
                try
                {
                    // Выполняем запрос по адресу и получаем ответ в виде строки
                    responseWeb = webClient.DownloadString(url).Replace("\r", "").Replace(".", ",");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exeption {ex.Message} | {code}");
                    if (ex.Message.Contains("403"))
                    {
                        Thread.Sleep(1000);
                        LoadStoksInfo(em, code);
                    }
                }
            }
            if (responseWeb.Length == 0)
            {
                Console.WriteLine("Len=0 " + code);
            }
            else if (responseWeb.Length < 100)
            {
                Console.WriteLine("Len<100 " + code);
                Thread.Sleep(1000);
                LoadStoksInfo(em, code);
            }
            else
            {
                using (StreamWriter sw = new StreamWriter($"{code}.txt", false, Encoding.Default))
                {
                    var tmpArr1 = responseWeb.Split("\n");
                    var tmpArr2 = tmpArr1.SkipLast(1).Skip(1);//.Where(res => Array.IndexOf(tmpArr1, res) < tmpArr1.Length - 1 && Array.IndexOf(tmpArr1, res) > 0);
                    int ind = Array.IndexOf(tmpArr1[0].Split(";"), "<CLOSE>");
                    int indH = Array.IndexOf(tmpArr1[0].Split(";"), "<HIGH>");
                    int indL = Array.IndexOf(tmpArr1[0].Split(";"), "<LOW>");
                    var responseClose = tmpArr2.Select(str => double.Parse(str.Split(";").GetValue(ind).ToString())).ToList();
                    var responseHigh = tmpArr2.Select(str => double.Parse(str.Split(";").GetValue(indH).ToString())).ToList();
                    var responseLow = tmpArr2.Select(str => double.Parse(str.Split(";").GetValue(indL).ToString())).ToList();
                    sw.WriteLine(responseWeb.Replace(",", "."));
                    SMA(responseClose, 26);
                    RSI(responseClose, 14);
                    WilliamsR(responseClose, responseHigh, responseLow, 14, 3);
                    STOCH(responseClose, responseHigh, responseLow, 14, 3);
                    MACD(responseClose, 12, 26);
                    Console.WriteLine("Good " + code);
                }
            }
        }
        static double SMA(List<double> response, int period)
        {
            ////забираем данные за определенный период
            //List<double> dataArr = response.Skip(response.Count - period - 1).ToList();
            //double alpha = 2.0 / (period + 1);
            //double ema = dataArr[0];
            //for (int i = 1; i < dataArr.Count; ++i)
            ////foreach (var v in dataArr)
            //{
            //    //alpha = 2.0 / (period-i + 1);
            //    //Console.WriteLine(v);
            //    //ema = alpha * dataArr[i] + (1 - alpha) * ema;
            //    ema = alpha * dataArr[i] + (1 - alpha) * ema;
            //    //ema = (dataArr[i] - ema) * alpha - ema;
            //}
            Console.WriteLine($"SMA={response.Skip(response.Count - period).Average()}");
            //Console.WriteLine($"EMA_{period}={ema} a={alpha}");
            return response.Skip(response.Count - period).Average();
        }
        static double EMA(List<double> response, int period)
        {
            //забираем данные за определенный период
            //List<double> dataArr = response.Skip(response.Count - period-1).ToList();
            //List<double> dataArr = response.ToList();
            double alpha = 2.0 / (period + 1);
            double ema = response[0];
            for (int i = 1; i < response.Count; ++i)
            {
                //Console.WriteLine(v);
                ema = alpha * response[i] + (1 - alpha) * ema;
            }
            //Console.WriteLine($"SMA={dataArr.Average()}");
            //Console.WriteLine($"EMA_{dataArr.Count}={ema} a={alpha}");
            return ema;
        }
        static void RSI(List<double> response, int period)
        {
            //Для расчета RSI используются положительные(U) и отрицательные(D) ценовые изменения.
            Queue<double> dataQueue = new Queue<double>(response.ToList());
            List<double> U = new List<double>();
            List<double> D = new List<double>();
            //Console.WriteLine(dataQueue.Peek());
            U.Add(0);
            D.Add(0);
            //Console.WriteLine(v);
            while (dataQueue.Count > 1)
            {
                //вчера - сегодня
                var v = dataQueue.Dequeue() - dataQueue.Peek();
                //Console.WriteLine(v);
                if (v == 0)
                {

                    U.Add((U.Last() * 13.0 + 0.0) / 14.0);
                    D.Add((D.Last() * 13.0 + 0.0) / 14.0);
                }
                else if (v < 0)
                {
                    U.Add((U.Last() * 13.0 - v) / 14.0);
                    D.Add(((D.Last() * 13.0 + 0.0) / 14.0));
                }
                else
                {
                    D.Add(((D.Last() * 13.0 + v) / 14.0));
                    U.Add(((U.Last() * 13.0 + 0.0) / 14.0));
                }
            }
            Console.WriteLine($"RSI({period})={100 * (U.Last() / (U.Last() + D.Last()))}");
        }
        static double STOCH(List<double> response, List<double> high, List<double> low, int period1, int period2)
        {
            List<double> ma = new List<double>();
            List<double> dataArr = response.Skip(response.Count - period1).ToList();
            List<double> dataArrH = high.Skip(high.Count - period1).ToList();
            List<double> dataArrL = low.Skip(low.Count - period1).ToList();
            Console.WriteLine($"STOCH={((dataArr.Last() - dataArrL.Min()) / (dataArrH.Max() - dataArrL.Min())) * 100}");
            for (int i = 0; i < period2; i++)
            {
                var d = response.SkipLast(i).Skip(response.Count - period1 - i).ToList();
                var h = high.SkipLast(i).Skip(high.Count - period1 - i).ToList();
                var l = low.SkipLast(i).Skip(low.Count - period1 - i).ToList();
                ma.Add(((d.Last() - l.Min()) / (h.Max() - l.Min())) * 100);
            }
            Console.WriteLine($"STOCHma={ma.Average()}");

            return ((dataArr.Last() - dataArrL.Min()) / (dataArrH.Max() - dataArrL.Min())) * 100;
        }
        static double WilliamsR(List<double> response, List<double> high, List<double> low, int period1, int period2)
        {
            List<double> ma = new List<double>();
            List<double> dataArr = response.Skip(response.Count - period1).ToList();
            List<double> dataArrH = high.Skip(high.Count - period1).ToList();
            List<double> dataArrL = low.Skip(low.Count - period1).ToList();
            Console.WriteLine($"WilliamsR={((dataArr.Last() - dataArrH.Max()) / (dataArrH.Max() - dataArrL.Min())) * 100}");

            return ((dataArr.Last() - dataArrH.Max()) / (dataArrH.Max() - dataArrL.Min())) * 100;
        }
        static void MACD(List<double> response, int period1, int period2)
        {
            Console.WriteLine($"EMA({period1})={EMA(response, period1)}");
            Console.WriteLine($"EMA({period2})={EMA(response, period2)}");
            Console.WriteLine($"MACD({period1},{period2})={EMA(response, period1) - EMA(response, period2)}");
            //Console.WriteLine($"MACD_Signal({period1},{period2})={EMA(response, period1) - EMA(response, period2)}");

        }
    }
}