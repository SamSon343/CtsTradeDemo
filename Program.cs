using CTSTestApplication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CtsTradeDemo
{
    class Program
    {
        const string folderAddress = @"..\..\..\Xml\";
        const string fileAddress = folderAddress + "TradesList.xml";
        const string logAddress = @"..\..\..\Log\";
        const int count = 1000000;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("...Starting CTS Trade Demo...");

                if (!CreateTestFile())
                    Environment.Exit(0);

                DataAdapter dt = new DataAdapter(logAddress);

                FillDb(dt);

                //Bohuzel furt to pada na exception. Nevim presne jak funguje ten mock v takovem pripade, ale doufam ze staci aspon SELECT =)
                //var resultSell = dt.Process(Operation.Get, @"SELECT ISIN, SUM(Quantity*Price) AS Total_Price FROM Trades WHERE Direction='S' GROUP BY ISIN ORDER BY Total_Price DESC LIMIT 10");
                //var resultBuy = dt.Process(Operation.Get, @"SELECT ISIN, SUM(Quantity*Price) AS Total_Price FROM Trades WHERE Direction='B' GROUP BY ISIN ORDER BY Total_Price ASC LIMIT 10");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void FillDb(DataAdapter dt)
        {
            try
            {
                dt.BeginTransaction("InsertTestData");
                foreach (var ch in ReadTestFile())
                {
                    try
                    {
                        ch.Create(dt);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception occured. Skipping the row for test purposes");
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine();
                    }
                }

                dt.CommitTransaction("InsertTestData");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                dt.RollbackTransaction("InsertTestData");
            }
        }
        static IEnumerable<Trade> ReadTestFile()
        {
            using (XmlReader reader = XmlReader.Create(fileAddress))
            {
                reader.MoveToContent();

                int id = 0;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "Trade")
                        {
                            XNode node = XNode.ReadFrom(reader);
                            XElement el = XElement.Parse(node.ToString());

                            if(node != null)
                            {
                                Trade trade = new Trade() { Id = id++ };
                                foreach (var subNode in el.Nodes())
                                {
                                    var tradeNode = (XElement)subNode;
                                    switch (tradeNode.Name.LocalName.ToString())
                                    {
                                        case "Direction":
                                            trade.Direction = ((TradesListTradeDirection) Enum.Parse(typeof(TradesListTradeDirection), tradeNode.Value, true)) == TradesListTradeDirection.B ? Direction.Buy : Direction.Sell;
                                            break;
                                        case "ISIN":
                                            trade.Isin = tradeNode.Value;
                                            break;
                                        case "Quantity":
                                            trade.Quantity = decimal.Parse(tradeNode.Value);
                                            break;
                                        case "Price":
                                            trade.Price = decimal.Parse(tradeNode.Value);
                                            break;
                                    }
                                }

                                if (trade != null)
                                {
                                    yield return trade;
                                }
                            }
                        }
                    }
                }
            }
        }

        static bool CreateTestFile()
        {
            try
            {
                Console.WriteLine("Creating a test xml file");
                Tester tester = new Tester();

                if (!File.Exists(@"..\..\..\Xml\TradesList.xml"))
                    tester.CreateTestFile(@"..\..\..\Xml\", count);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return false;
        }
    }
}
