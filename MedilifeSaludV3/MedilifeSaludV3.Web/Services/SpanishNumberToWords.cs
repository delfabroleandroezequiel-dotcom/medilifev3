using System.Globalization;

namespace MedilifeSaludV3.Web.Services;

public static class SpanishNumberToWords
{
    public static string PesosToWords(decimal amount)
    {
        // Normalizar con 2 decimales
        amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        var pesos = (long)Math.Floor(amount);
        var centavos = (int)Math.Round((amount - pesos) * 100m);

        var words = ToWords(pesos).Trim();
        if (string.IsNullOrWhiteSpace(words)) words = "CERO";

        return $"{words} PESOS CON {centavos:00}/100".ToUpperInvariant();
    }

    public static string ToWords(long n)
    {
        if (n == 0) return "cero";
        if (n < 0) return "menos " + ToWords(Math.Abs(n));

        var parts = new List<string>();

        void AddChunk(long value, string singular, string plural)
        {
            if (value == 0) return;
            if (value == 1) parts.Add(singular);
            else parts.Add(ToWords(value) + " " + plural);
        }

        // miles de millones (billones en escala larga no lo usamos)
        if (n >= 1_000_000_000)
        {
            var billions = n / 1_000_000_000;
            n %= 1_000_000_000;
            AddChunk(billions, "mil millones", "mil millones");
        }

        if (n >= 1_000_000)
        {
            var millions = n / 1_000_000;
            n %= 1_000_000;
            AddChunk(millions, "un millón", "millones");
        }

        if (n >= 1000)
        {
            var thousands = n / 1000;
            n %= 1000;
            if (thousands == 1) parts.Add("mil");
            else parts.Add(ToWords(thousands) + " mil");
        }

        if (n > 0)
        {
            parts.Add(ToWordsLessThan1000((int)n));
        }

        return string.Join(" ", parts).Replace("  ", " ").Trim();
    }

    private static string ToWordsLessThan1000(int n)
    {
        if (n == 0) return "";

        if (n < 100) return ToWordsLessThan100(n);

        var hundreds = n / 100;
        var remainder = n % 100;

        string hundredWord = hundreds switch
        {
            1 => remainder == 0 ? "cien" : "ciento",
            2 => "doscientos",
            3 => "trescientos",
            4 => "cuatrocientos",
            5 => "quinientos",
            6 => "seiscientos",
            7 => "setecientos",
            8 => "ochocientos",
            9 => "novecientos",
            _ => ""
        };

        if (remainder == 0) return hundredWord;
        return hundredWord + " " + ToWordsLessThan100(remainder);
    }

    private static string ToWordsLessThan100(int n)
    {
        if (n < 10)
        {
            return n switch
            {
                0 => "",
                1 => "uno",
                2 => "dos",
                3 => "tres",
                4 => "cuatro",
                5 => "cinco",
                6 => "seis",
                7 => "siete",
                8 => "ocho",
                9 => "nueve",
                _ => ""
            };
        }

        if (n < 16)
        {
            return n switch
            {
                10 => "diez",
                11 => "once",
                12 => "doce",
                13 => "trece",
                14 => "catorce",
                15 => "quince",
                _ => ""
            };
        }

        if (n < 20) return "dieci" + ToWordsLessThan100(n - 10);
        if (n == 20) return "veinte";
        if (n < 30) return "veinti" + ToWordsLessThan100(n - 20);

        var tens = n / 10;
        var unit = n % 10;

        string tenWord = tens switch
        {
            3 => "treinta",
            4 => "cuarenta",
            5 => "cincuenta",
            6 => "sesenta",
            7 => "setenta",
            8 => "ochenta",
            9 => "noventa",
            _ => ""
        };

        if (unit == 0) return tenWord;
        return tenWord + " y " + ToWordsLessThan100(unit);
    }
}
