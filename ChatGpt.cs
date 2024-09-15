public record ChatGptResponse(bool IsValid, List<string> Words, List<List<char>> Table);

public static class ChatGpt
{
    public static string Token;

    public static ChatGptResponse AnalyzeImage(string imagePath)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", Token);
        var body = new
        {
            model = "gpt-4o",
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = "Analyze this image and return a JSON object with three fields: \"isValid\" with the value true if this image contains a word search puzzle; \"words\" with an array of strings representing the words which need to be found in the puzzle; \"table\" with a two dimensional array of letters in the table.",
                        },
                        new
                        {
                            type = "image_url",
                            image_url = new { url = $"data:image/jpeg;base64,{Convert.ToBase64String(File.ReadAllBytes(imagePath))}" },
                        },
                    },
                },
            },
            temperature = 0,
        };
        var responseJson = httpClient.PostAsync("https://api.openai.com/v1/chat/completions", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync().Result;
        var resultJson = JToken.Parse(responseJson)["choices"][0]["message"]["content"].ToString()[("```json\n".Length)..^("```".Length)];
        Console.WriteLine(resultJson);
        return JsonConvert.DeserializeObject<ChatGptResponse>(resultJson);
    }
}
