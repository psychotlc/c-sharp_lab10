
namespace Utils.Requests;

public class HttpGet{

    
    public static async Task<string> Get(string url)
    {
        // http GET request to "url" 

        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException e)
            {
                throw new HttpRequestException($"Request error: {e.Message}");
            }
        }
    }
}