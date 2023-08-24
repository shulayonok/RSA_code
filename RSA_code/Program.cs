using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace RSA_code
{
    class Program
    {
        static BigInteger Pow(BigInteger a, BigInteger e, BigInteger n) // Быстрое возведение в степень
        {
            string bin = to_Bin(e);
            BigInteger[] resArr = new BigInteger[bin.Length];
            for (int i = 0; i < bin.Length; i++)
            {
                if (i == 0)
                {
                    resArr[i] = a;
                    continue;
                }
                if (bin[i] == '0')
                    resArr[i] = (resArr[i - 1] * resArr[i - 1]) % n;
                if (bin[i] == '1')
                    resArr[i] = (resArr[i - 1] * resArr[i - 1] * a) % n;
            }
            return resArr[resArr.Length - 1];
        }
        static int NumOfRows(BigInteger a, BigInteger b) // Подсчёт количества итераций для алгоритма Евклида
        {
            int rows = 1; BigInteger mod = a % b;
            while (mod != 0)
            {
                a = b; b = mod; mod = a % b;
                rows++;
            }
            return rows;
        }
        static BigInteger Nod(BigInteger a, BigInteger b) // Алгоритм Евклида
        {
            int rows = NumOfRows(a, b);
            BigInteger[,] mt = new BigInteger[rows, 6];
            BigInteger mod = a % b;
            BigInteger div = a / b;
            mt[0, 0] = a; mt[0, 1] = b; mt[0, 2] = mod; mt[0, 3] = div;
            for (int i = 1; i < rows; i++)
            {
                mt[i, 0] = b; a = mt[i, 0];
                mt[i, 1] = mod; b = mt[i, 1];
                mt[i, 2] = mt[i, 0] % mt[i, 1]; mod = mt[i, 2];
                mt[i, 3] = mt[i, 0] / mt[i, 1]; div = mt[i, 3];
            }
            return b;
        }
        static void Nod(BigInteger a, BigInteger b, ref BigInteger d, BigInteger euler) // Расширенный алгоритм Евклида
        {
            int rows = NumOfRows(a, b);
            BigInteger[,] mt = new BigInteger[rows, 6];
            BigInteger mod = a % b;
            BigInteger div = a / b;
            mt[0, 0] = a; mt[0, 1] = b; mt[0, 2] = mod; mt[0, 3] = div;
            for (int i = 1; i < rows; i++)
            {
                mt[i, 0] = b; a = mt[i, 0];
                mt[i, 1] = mod; b = mt[i, 1];
                mt[i, 2] = mt[i, 0] % mt[i, 1]; mod = mt[i, 2];
                mt[i, 3] = mt[i, 0] / mt[i, 1]; div = mt[i, 3];
            }
            for (int i = rows - 1; i >= 0; i--)
            {
                if (i == rows - 1)
                {
                    mt[i, 4] = 0; mt[i, 5] = 1;
                    continue;
                }
                mt[i, 4] = mt[i + 1, 5];
                mt[i, 5] = mt[i + 1, 4] - mt[i + 1, 5] * mt[i, 3];
            }
            if (mt[0, 5] < 0)
                d = mt[0, 5] + euler;
            else
                d = mt[0, 5];
        }
        static string to_Bin(BigInteger number) //Перевод в двоичный код
        {
            BigInteger delimoe = number; string result = "";
            while (true)
            {
                BigInteger chastnoe = delimoe / 2;
                BigInteger ostatok = delimoe % 2;
                delimoe = chastnoe;
                result += ostatok.ToString();
                if (chastnoe == 0)
                    break;
            }
            char[] charArray = result.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        static ulong[] FermatFactor(ulong n) // Алгоритм факторизации Ферма (у бигинтеджера нет метода, находящего квадратный корень числа)
        {
            ulong a, b;

            //если число четное, возвращаем результат
            if ((n % 2UL) == 0)
            {
                return new [] { 2UL, n / 2UL };
            }

            a = Convert.ToUInt64(Math.Ceiling(Math.Sqrt(n)));
            if (a * a == n)
            {
                return new[] { a, a };
            }

            while (true)
            {
                ulong tmp = a * a - n;
                b = Convert.ToUInt64(Math.Sqrt(tmp));

                if (b * b == tmp)
                {
                    break;
                }

                a++;
            }

            return new[] { a - b, a + b };
        }
        static void Main(string[] args)
        {
            // Основная программа
            BigInteger p, q, m = 0, d = 0;
            Console.WriteLine("Введите p (простое): ");
            p = BigInteger.Parse(Console.ReadLine());
            Console.WriteLine("Введите q (простое): ");
            q = BigInteger.Parse(Console.ReadLine());
            BigInteger n = p * q;
            Console.WriteLine("Введите m: ");
            int count = 0;
            do
            {
                if (count >= 1)
                    Console.WriteLine("Исходное сообщение должно быть в виде числа из интервала (1; n)");
                m = BigInteger.Parse(Console.ReadLine());
                count++;
            }
            while (m >= n || m <= 1);

            // Находим фи
            BigInteger phi = (p - 1) * (q - 1);

            // Находим e
            BigInteger e = 2;
            while (Nod(e, phi) != 1)
            {
                e++;
            }

            // Находим d
            Nod(phi, e, ref d, phi);

            // Шифрограмма
            BigInteger encryption = Pow(m, e, n);

            // Расшифровка
            BigInteger decryption = Pow(encryption, d, n);

            // Запись в файл
            string text = "Открытый ключ: n = " + n.ToString() + " e = " + e.ToString() + "\n" + "Закрытый ключ: n = " +
                n.ToString() + " d = " + d.ToString() + "\n" + "Исходное сообщение m = " + m.ToString() +
                "\n" + "Зашифрованное сообщение e(m) = " + encryption.ToString() + "\n" +
                "Расшифровка m = " + decryption.ToString();
            string path = Environment.CurrentDirectory;
            using (FileStream fstream = new FileStream($"{path}\\log.txt", FileMode.Create))
            {
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes(text);
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);
                Console.WriteLine("Текст записан в файл");
            }

            // Открытие файла после компиляции
            Process.Start(@"notepad.exe", Environment.CurrentDirectory + "\\log.txt");

            // Вывод чисел после алгоритма факторизации Ферма
            ulong[] arr = FermatFactor((ulong)n);
            foreach (ulong elem in arr)
            {
                Console.WriteLine(elem);
            }
        }
    }
}
