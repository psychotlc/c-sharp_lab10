using Utils.Requests;
using Utils.Parsers;
namespace Utils.TickerUtils;

public class Ticker{
    public static async Task<double> GetTodayPrice(string ticker)
    {
        DateTime today = DateTime.Today;
        DateTime yesterday = today.AddDays(-1);

        long todayUnixTimestamp = ((DateTimeOffset)today).ToUnixTimeSeconds();
        long yesterdayUnixTimestamp = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

        string url =    $"https://query1.finance.yahoo.com/v7/finance/download/"    +
                        $"{ticker}?period1={yesterdayUnixTimestamp}&period2={todayUnixTimestamp}" +
                        "&interval=1d&events=history&includeAdjustedClose=true";
        
        string response;
        
        response = await HttpGet.Get(url);


        var parsedResponse = await CSVParse.Parse(response);
        
        double price;
        price = Convert.ToDouble(parsedResponse[1]);

        return price;
    }

    public static async Task<double> GetYesterdayPrice(string ticker)
    {

        DateTime yesterday = DateTime.Today.AddDays(-1);
        DateTime twoDayAgo = yesterday.AddDays(-2);

        long yesterdayUnixTimestamp = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();
        long twoDayAgoUnixTimestamp = ((DateTimeOffset)twoDayAgo).ToUnixTimeSeconds();

        string url =    $"https://query1.finance.yahoo.com/v7/finance/download/"    +
                        $"{ticker}?period1={twoDayAgoUnixTimestamp}&period2={yesterdayUnixTimestamp}" +
                        "&interval=1d&events=history&includeAdjustedClose=true";
        
        string response;
        
        response = await HttpGet.Get(url);


        var parsedResponse = await CSVParse.Parse(response);
        
        double price;
        price = Convert.ToDouble(parsedResponse[1]);

        return price;
    }
}