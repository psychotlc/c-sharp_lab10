using Utils.Requests;
using Utils.Parsers;
using Utils.TickerUtils;

using Utils.SqlConn;
using MySqlConnector;
using System.Data.Common;
using ZstdSharp;


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


app.MapGet("/tickers/save-current-ticker-prices", async () => {
    DateTime todayDatetime = DateTime.Today;
    DateTime yesterdayDatetime = todayDatetime.AddDays(-1);

    // connection for SELECT sql request

    MySqlConnection selectConnection = DBUtils.GetDBConnection();

    selectConnection.Open();

    // command for SELECT sql request

    MySqlCommand selectCommand = selectConnection.CreateCommand();


    using(StreamReader reader = new StreamReader("../ticker.txt")){

        string ticker;

        while ((ticker = reader.ReadLine()) != null){

            double price = await Ticker.GetTodayPrice(ticker);

            // SELECT request

            selectCommand.CommandText = $"SELECT * FROM tickers WHERE ticker = '{ticker}'";

            // create connection for UPDATE/INSERT requests
            MySqlConnection updateConnection = DBUtils.GetDBConnection();

            updateConnection.Open();

            // command for UPDATE/INSERT requests
            MySqlCommand updateCommand = updateConnection.CreateCommand();

            using (DbDataReader SQLReader = selectCommand.ExecuteReader())
            {
                if (SQLReader.HasRows)
                {
                    // if we have in "tickers" tabel entry about ticker with name {ticker}
                    // we update columns for ticker_id={ticker_id} in "prices" tabel

                    SQLReader.Read();
                    int ticker_id = Convert.ToInt32(SQLReader.GetValue(0));
                    updateCommand.CommandText = $"UPDATE prices SET price = {price}, date = '{todayDatetime.ToString("yyyy-MM-dd")}' WHERE ticker_id = {ticker_id}";
                    updateCommand.ExecuteNonQuery();
                }
                else
                {
                    // if not we insert new information about {ticker} into tabels "prices" and "tickers"

                    updateCommand.CommandText = $"INSERT INTO tickers (ticker) VALUES ('{ticker}')";
                    updateCommand.ExecuteNonQuery();

                    long ticker_id = updateCommand.LastInsertedId;
                    updateCommand.CommandText = $"INSERT INTO prices (ticker_id, price, date) VALUES ({ticker_id}, {price}, '{todayDatetime.ToString("yyyy-MM-dd")}')";
                    updateCommand.ExecuteNonQuery();
                }
            }
            // close connection
            updateConnection.Close();
        }
    }

    // close connection

    selectConnection.Close();

});

app.MapGet("/tickers/get-todays-condition", async (HttpContext context) => {
    string ticker = context.Request.Query["ticker"];

    DateTime todayDatetime = DateTime.Today;
    DateTime yesterdayDatetime = todayDatetime.AddDays(-1);


    MySqlConnection selectConnection = DBUtils.GetDBConnection();
    MySqlConnection updateConnection = DBUtils.GetDBConnection();

    selectConnection.Open();
    updateConnection.Open();

    MySqlCommand selectCommand = selectConnection.CreateCommand();
    MySqlCommand updateCommand = updateConnection.CreateCommand();

    selectCommand.CommandText = $"SELECT id FROM tickers WHERE ticker = '{ticker}'";
    object ticker_id;
    using (DbDataReader SQLReader = selectCommand.ExecuteReader())
    {
        SQLReader.Read();
        ticker_id = SQLReader.GetValue(0);
    }

    selectCommand.CommandText = " SELECT todays_condition.state, prices.date FROM prices " + 
                                " INNER JOIN todays_condition ON todays_condition.ticker_id = prices.ticker_id " +
                                $" WHERE prices.ticker_id = {ticker_id} " ;

    using (DbDataReader SQLReader = selectCommand.ExecuteReader())
    {
        if (SQLReader.HasRows)
        {
            SQLReader.Read();
            if (SQLReader.GetValue(1) == todayDatetime.ToString("yyyy-MM-dd")) 
            {
                return SQLReader.GetValue(0);
            }
            else
            {
                
                try{
                    

                    double currentPrice = await Ticker.GetTodayPrice(ticker);
                    double oneDayAgoPrice = await Ticker.GetYesterdayPrice(ticker);
                    double state = currentPrice - oneDayAgoPrice;
                    updateCommand.CommandText = $" UPDATE prices SET price={currentPrice}, "  + 
                                                $" date='{todayDatetime.ToString("yyyy-MM-dd")}' WHERE ticker_id = {ticker_id}; " +
                                                
                                                $" UPDATE todays_condition SET state = {state} WHERE ticker_id = {ticker_id}";
                    
                    updateCommand.ExecuteNonQuery();
                    return state;
                }
                catch (Exception err){
                    return "finance.yahoo doesn't work. Try attempt later";
                }
            }

        }
        else 
        {
            try{

                DateTime oneDayAgo = DateTime.Now.AddDays(-1);

                double currentPrice = await Ticker.GetTodayPrice(ticker);
                double oneDayAgoPrice = await Ticker.GetYesterdayPrice(ticker);

                double state = currentPrice - oneDayAgoPrice;

                updateCommand.CommandText = $" UPDATE prices SET price={currentPrice}, "  + 
                                            $" date='{todayDatetime.ToString("yyyy-MM-dd")}' WHERE ticker_id= {ticker_id}; " +
                                            
                                            $"INSERT INTO todays_condition (ticker_id, state) VALUES ({ticker_id}, {state})";
                
                updateCommand.ExecuteNonQuery();
                return state;
            }

            catch(Exception err)
            {
                return "finance.yahoo doesn't work. Try attempt later";
            }
            
            
            

            
        }
    }

});

app.MapGet("/tickers/get-all-tickers", () => {

    MySqlConnection selectConnection = DBUtils.GetDBConnection();

    selectConnection.Open();

    MySqlCommand selectCommand = selectConnection.CreateCommand();

    selectCommand.CommandText = "SELECT ticker FROM tickers";

    List <object> result = new List<object>();

    using (DbDataReader SQLReader = selectCommand.ExecuteReader())
    {
        while(SQLReader.Read())
        {
            object ticker = SQLReader.GetValue(0);
            result.Add(ticker);
        }
    }

    selectConnection.Close();

    return result;

});

app.Run();
