using Utils.Requests;
using Utils.Parsers;

using Utils.SqlConn;
using MySqlConnector;
using System.Data.Common;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                // You can configure CORS policies here.
                // By default, this allows all origins, methods, and headers.
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

var app = builder.Build();

app.UseCors();


app.MapGet("/save-current-ticker-prices", async () => {
    DateTime currentTimestamp = DateTime.Now;
    DateTime oneDayAgo = currentTimestamp.AddDays(-1);

    long currentUnixTimestamp = ((DateTimeOffset)currentTimestamp).ToUnixTimeSeconds();
    long oneDayAgoUnixTimestamp = ((DateTimeOffset)oneDayAgo).ToUnixTimeSeconds();

    MySqlConnection selectConnection = DBUtils.GetDBConnection();

    selectConnection.Open();

    MySqlCommand selectCommand = selectConnection.CreateCommand();


    using(StreamReader reader = new StreamReader("../ticker.txt")){

        string ticker;

        while ((ticker = reader.ReadLine()) != null){

            string url =    $"https://query1.finance.yahoo.com/v7/finance/download/"    +
                                    $"{ticker}?period1={oneDayAgoUnixTimestamp}&period2={currentUnixTimestamp}" +
                                    "&interval=1d&events=history&includeAdjustedClose=true";
            
            string response;
            try{
                response = await HttpGet.Get(url);
            }
            catch(Exception e) {
                Console.WriteLine($"url with ticker = {ticker} not Found");
                continue;
            }

            var parsedResponse = await CSVParse.Parse(response);
            double price;
            try{
                price = Convert.ToDouble(parsedResponse[1]);
            }
            catch{
                Console.WriteLine($"Wrong price in response for ticker = {ticker}");
                continue;
            }

            selectCommand.CommandText = $"SELECT * FROM tickers WHERE ticker = '{ticker}'";

            MySqlConnection updateConnection = DBUtils.GetDBConnection();

            updateConnection.Open();

            MySqlCommand updateCommand = updateConnection.CreateCommand();

            using DbDataReader SQLReader = selectCommand.ExecuteReader();
            {
                if (SQLReader.HasRows)
                {
                    SQLReader.Read();
                    int ticker_id = Convert.ToInt32(SQLReader.GetValue(0));
                    updateCommand.CommandText = $"UPDATE prices SET price = {price}, date = '{currentTimestamp.ToString("yyyy-MM-dd")}' WHERE ticker_id = {ticker_id}";
                    updateCommand.ExecuteNonQuery();
                }
                else
                {
                    updateCommand.CommandText = $"INSERT INTO tickers (ticker) VALUES ('{ticker}')";
                    updateCommand.ExecuteNonQuery();

                    long ticker_id = updateCommand.LastInsertedId;
                    updateCommand.CommandText = $"INSERT INTO prices (ticker_id, price, date) VALUES ({ticker_id}, {price}, '{currentTimestamp.ToString("yyyy-MM-dd")}')";
                    updateCommand.ExecuteNonQuery();
                }
            }

            updateConnection.Close();
        }
    }

    selectConnection.Close();

});

app.Run();
