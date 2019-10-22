using System;
using System.Collections.Generic;
using System.Linq;

//The focus should be on clean and simple code 
namespace Visit.Test
{
    /// <summary>
    /// This is the public inteface used by our client and may not be changed
    /// </summary>
    public interface ITaxCalculator
    {
        double GetStandardTaxRate(Commodity commodity);
        void SetCustomTaxRate(Commodity commodity, double rate);
        double GetTaxRateForDateTime(Commodity commodity, DateTime date);
        double GetCurrentTaxRate(Commodity commodity);
    }

    /// <summary>
    /// Implements a tax calculator for our client.
    /// The calculator has a set of standard tax rates that are hard-coded in the class.
    /// It also allows our client to remotely set new, custom tax rates.
    /// Finally, it allows the fetching of tax rate information for a specific commodity and point in time.
    /// TODO: We know there are a few bugs in the code below, since the calculations look messed up every now and then.
    ///       There are also a number of things that have to be implemented.
    /// </summary>
    public class TaxCalculator : ITaxCalculator
    {
        /// <summary>
        /// Get the standard tax rate for a specific commodity.
        /// </summary>
        /// 
                    //TODO: please refactor these ugly if statements somehow...
        public double GetStandardTaxRate(Commodity commodity)
        {

            if (commodity == Commodity.Default || commodity == Commodity.Alcohol)
                return 0.25;
            else if (commodity == Commodity.Food || commodity == Commodity.FoodServices)
                return 0.12;
            else
                return 0.6;
        }


        /// <summary>
        /// This method allows the client to remotely set new custom tax rates.
        /// When they do, we save the commodity/rate information as well as the UTC timestamp of when it was done.
        /// NOTE: Each instance of this object supports a different set of custom rates, since we run one thread per customer.
        /// </summary>
        //TODO: support saving multiple custom rates for different combinations of Commodity/DateTime
        //TODO: make sure we never save duplicates, in case of e.g. clock resets, DST etc - overwrite old values if this happens

        public void SetCustomTaxRate(Commodity commodity, double rate)
        {
            DateTime cur_date = DateTime.Now;
            cur_date = DateTime.ParseExact(cur_date.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null);
            Console.WriteLine(cur_date);
            var ComTimeTuple = Tuple.Create(cur_date, commodity);

            if (!_customRates.ContainsKey(ComTimeTuple))
                _customRates.Add(ComTimeTuple, rate);
            else _customRates[ComTimeTuple] = rate;
        }
        static SortedDictionary<Tuple<DateTime, Commodity>, double> _customRates = new SortedDictionary<Tuple<DateTime, Commodity>, double>();


        /// <summary>
        /// Gets the tax rate that is active for a specific point in time (in UTC).
        /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
        /// If there is no custom tax rate for the specified date, use the standard tax rate.
        /// </summary>
        public double GetTaxRateForDateTime(Commodity commodity, DateTime date)
        {
            var ComTimeTuple = Tuple.Create(date, commodity);
            var commodityKeys = _customRates.Select(s => s.Key).Where(d => d.Item2 == commodity);
            if (commodityKeys.Count() == 0)
                return GetStandardTaxRate(commodity);
            else if (_customRates.ContainsKey(ComTimeTuple))
                return _customRates[ComTimeTuple];
            else if (date < commodityKeys.First().Item1)
                return GetStandardTaxRate(commodity);
            else if (date > commodityKeys.Last().Item1)
                return _customRates[_customRates.Keys.Last()];
            else
            {
                var previousEntry = commodityKeys.First();
                foreach (var entry in commodityKeys)
                {
                    if (entry.Item1 > date)
                        return _customRates[previousEntry];
                    previousEntry = entry;
                }

            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the tax rate that is active for the current point in time.
        /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
        /// If there is no custom tax currently active, use the standard tax rate.
        /// </summary>
        public double GetCurrentTaxRate(Commodity commodity)
        {
            var commodityKeys = _customRates.Select(s => s.Key).Where(d => d.Item2 == commodity);
            if (commodityKeys.Count() == 0)
                return GetStandardTaxRate(commodity);
            else
                return _customRates[commodityKeys.Last()];

        }

    }

    public enum Commodity
    {
        //PLEASE NOTE: THESE ARE THE ACTUAL TAX RATES THAT SHOULD APPLY, WE JUST GOT THEM FROM THE CLIENT!
        Default,            //25%
        Alcohol,            //25%
        Food,               //12%
        FoodServices,       //12%
        Literature,         //6%
        Transport,          //6%
        CulturalServices    //6%
    }

    public class InvalidCommodityException : Exception
    {
        public InvalidCommodityException(String message)

            : base(message)
        {
        }
    }
    public class Program

    {
        static void validateCommodity(int commodity)
        {
            if (commodity < 0 || commodity > 7)
                throw new InvalidCommodityException("Invalid Commodity Entered, try again ..!!");
        }
        public static void Main(string[] args)
        {

            TaxCalculator taxc = new TaxCalculator();
            bool flag = true;
            try
            {
                while (flag)
                {
                    Console.WriteLine("1.GetStandardTaxRate 2.SetCustomTaxRate 3.GetTaxRateForDateTime 4.GetCurrentTaxRate 5.Exit");
                    Console.Write("Enter Your Choice: ");
                    int input = int.Parse(Console.ReadLine());

                    if (input == 1)
                    {
                        Console.WriteLine("0.Default 1.Alcohol 2.Food 3.FoodServices 4.Literature 5.Transport 6.CulturalServices");
                        Console.Write("Enter the Commodity: ");
                        string commodity = Console.ReadLine();
                        Commodity comm = (Commodity)Convert.ToInt32(commodity);
                        validateCommodity(Convert.ToInt32(commodity));
                        Console.WriteLine("Taxrate of {0} is {1}", comm, taxc.GetStandardTaxRate(comm));
                    }
                    else if (input == 2)
                    {
                        Console.WriteLine("0.Default 1.Alcohol 2.Food 3.FoodServices 4.Literature 5.Transport 6.CulturalServices");
                        Console.Write("Enter the Commodity: ");
                        string commodity = Console.ReadLine();
                        validateCommodity(Convert.ToInt32(commodity));
                        Console.Write("Enter the Custom TaxRate: ");
                        string custTax = Console.ReadLine();
                        double customTax = Convert.ToDouble(custTax);
                        Commodity comm = (Commodity)Convert.ToInt32(commodity);
                        taxc.SetCustomTaxRate(comm, customTax);
                        Console.WriteLine("Taxrate of {0} is updated to {1}", comm, customTax);
                    }
                    else if (input == 3)
                    {
                        Console.WriteLine("0.Default 1.Alcohol 2.Food 3.FoodServices 4.Literature 5.Transport 6.CulturalServices");
                        Console.Write("Enter the Commodity: ");
                        string commodity = Console.ReadLine();
                        validateCommodity(Convert.ToInt32(commodity));
                        Console.Write("Enter the DateTime (yyyy-MM-dd HH:mm:ss) : ");
                        string date_pattern = "yyyy-MM-dd HH:mm:ss";
                        DateTime dateTime = DateTime.ParseExact(Console.ReadLine(), date_pattern, null);
                        Commodity comm = (Commodity)Convert.ToInt32(commodity);
                        Console.WriteLine("Taxrate of {0} is  {1}", comm, taxc.GetTaxRateForDateTime(comm, dateTime));
                    }
                    else if (input == 4)
                    {
                        Console.WriteLine("0.Default 1.Alcohol 2.Food 3.FoodServices 4.Literature 5.Transport 6.CulturalServices");
                        Console.Write("Enter the Commodity: ");
                        string commodity = Console.ReadLine();
                        validateCommodity(Convert.ToInt32(commodity));
                        Commodity comm = (Commodity)Convert.ToInt32(commodity);
                        Console.WriteLine("Current Taxrate of {0} is {1}", comm, taxc.GetCurrentTaxRate(comm));
                    }
                    else if (input == 5)
                        Environment.Exit(0);
                    else
                        Console.WriteLine("Enter the correct input again..");
                }
            }
            catch (InvalidCommodityException e)
            {
                Console.WriteLine("User defined exception: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception :  {0}", e.Message);
            }
        }
    }
}
