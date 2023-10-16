using Microsoft.VisualBasic.FileIO;

namespace Utils.Parsers;

public class CSVParse{

    // Parse возвращает только вторую строку т.к. во второй строке будут данные за день.
    // Желательно чтобы он парсил все, можно будет везде его использовать. Но мне лень это делать
    // Все что не продакшн там все насрано
    public static async Task<string[]> Parse(string CSVString)
    {

        using (StringReader stringReader = new StringReader(CSVString)){

            using (TextFieldParser textFieldParser = new TextFieldParser(stringReader))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters(",");

                textFieldParser.ReadFields();

                string[] rows = textFieldParser.ReadFields();
                
                return rows;
            }

        }

    }
}