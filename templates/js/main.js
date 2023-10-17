let tickers;

getAllTickers();


async function getAllTickers() {
    var response = await fetch("http://localhost:5000/tickers/get-all-tickers");
    response = await response.json();

    tickers = response;

    var selectTickersObj = document.getElementById("select-tickers")

    for (var item of tickers){
        var optionObj = document.createElement("option")
        optionObj.innerHTML = item;
        selectTickersObj.appendChild(optionObj)
    }


}

async function getTickerState() {
    var selectTickersObj = document.getElementById("select-tickers")
    var ticker = tickers[selectTickersObj.selectedIndex]
    var response = await fetch(
        `http://localhost:5000/tickers/get-todays-condition?ticker=${ticker}`
    ).then((res) => {
        return res.json()
    }).catch((err) => {
        return err
    })

    var tickerStateInformationObj = document.getElementById("ticker-state__information")
    
    tickerStateInformationObj.innerHTML = response;
    
}